namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Runtime.InteropServices.WindowsRuntime;
	using System.Text;
	using System.Threading;
	using Library_1.Utilities;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reservations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceUpdates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Statuses;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.ReservationAction;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Time;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using static Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface.DataMinerInterface;
	using BackupType = Order.BackupType;
	using BookingManager = Library.Solutions.SRM.BookingManager;
	using Constants = YLE.Configuration.Constants;
	using Engine = DataMiner.Automation.Engine;
	using Function = Function.Function;
	using InterfaceConfiguration = Net.ServiceManager.Objects.InterfaceConfiguration;
	using ReservationInstance = Net.ResourceManager.Objects.ReservationInstance;
	using ServiceChange = History.ServiceChange;
	using ServiceDefinition = ServiceDefinition.ServiceDefinition;
	using ServiceReservationInstance = Skyline.DataMiner.Net.ResourceManager.Objects.ServiceReservationInstance;
	using UpdateCustomPropertiesTask = Tasks.ServiceTasks.UpdateCustomPropertiesTask;
	using VirtualPlatform = ServiceDefinition.VirtualPlatform;

	public class Service : DisplayedObject, IYleChangeTracking, ICloneable
	{
		private readonly Dictionary<string, object> initialPropertyValues = new Dictionary<string, object>();

		private string name;
		private bool isCancelled;
		private string comments;
		private DateTime start;
		private DateTime end;
		private TimeSpan preRoll;
		private TimeSpan postRoll;
		private bool hasCustomPreRoll = false;
		private bool hasCustomPostRoll = false;
		private bool isSharedSource;
		private bool isBooked = false;

		private IReadOnlyList<Service> availableServicesToRecordOrTransmit = new List<Service>();

		private Service(Service other)
		{
			Functions = other.Functions.Select(f => (Function)f.Clone()).ToList();
			Children = new ObservableCollection<Service>(other.Children.Select(s => (Service)s.Clone()));
			Interfaces = other.Interfaces.Select(i => (InterfaceConfiguration)i.Clone()).ToList();
			UserTasks = other.UserTasks.Select(ut => (LiveUserTask)ut.Clone()).ToList();

			this.hasCustomPreRoll = other.hasCustomPreRoll;
			this.hasCustomPostRoll = other.hasCustomPostRoll;
			this.isCancelled = other.isCancelled;

			CloneHelper.CloneProperties(other, this);
		}

		protected Service()
		{
			Id = Guid.NewGuid();
		}

		protected Service(string name)
			: this()
		{
			this.name = name;
		}

		/// <summary>
		/// Creates a new Service instance based on a ServiceConfiguration.
		/// </summary>
		/// <param name="nodeLabel"></param>
		/// <param name="configuration">ServiceConfiguration object defining the Service.</param>
		/// <param name="helpers"></param>
		/// <param name="nodeId"></param>
		protected Service(Helpers helpers, int nodeId, string nodeLabel, ServiceConfiguration configuration)
		{
			helpers.LogMethodStart(nameof(Service), nameof(Service), out var stopwatch, configuration.Name);

			Name = configuration.Name;
			Id = configuration.Id;
			PreRoll = configuration.PreRoll;
			PostRoll = configuration.PostRoll;

			Start = configuration.Start.FromServiceConfiguration().Truncate(TimeSpan.FromSeconds(1)).Truncate(TimeSpan.FromMilliseconds(1));
			End = configuration.End.FromServiceConfiguration().Truncate(TimeSpan.FromSeconds(1)).Truncate(TimeSpan.FromMilliseconds(1));

			BackupType = configuration.ServiceLevel.HasValue ? configuration.ServiceLevel.Value : BackupType.None;
			LinkedServiceId = configuration.LinkedServiceId;

			IsEurovisionService = configuration.IsEurovisionService;
			IsEurovisionMultiFeedService = configuration.IsEurovisionMultiFeedService;
			EurovisionWorkOrderId = configuration.EurovisionWorkOrderId;
			EurovisionTransmissionNumber = configuration.EurovisionTransmissionNumber;
			EurovisionBookingDetails = configuration.EurovisionBookingDetails ?? new EurovisionBookingDetails();
			EurovisionServiceConfigurations = configuration.EurovisionServiceConfigurations;
			EurovisionDestinationId = configuration.EurovisionDestinationId;
			EurovisionTechnicalSystemId = configuration.EurovisionTechnicalSystemId;
			EvsId = configuration.EvsId;
			IntegrationType = configuration.IntegrationType;
			IntegrationIsMaster = configuration.IntegrationIsMaster;

			Comments = configuration.Comments;
			ContactInformationName = configuration.ContactInformationName;
			ContactInformationTelephoneNumber = configuration.ContactInformationTelephoneNumber;
			LiveUDeviceName = configuration.LiveUDeviceName;
			AudioReturnInfo = configuration.AudioReturnInfo;
			VidigoStreamSourceLink = configuration.VidigoStreamSourceLink;
			AdditionalDescriptionUnknownSource = configuration.AdditionalDescriptionUnknownSource;
			IsUnknownSourceService = configuration.IsUnknownSourceService;
			IsAudioConfigurationCopiedFromSource = configuration.IsAudioConfigurationCopiedFromSource;

			RequiresRouting = configuration.RequiresRouting;
			MajorTimeslotChange = configuration.MajorTimeslotChange;
			RecordingConfiguration = configuration.RecordingConfiguration ?? new RecordingConfiguration();
			IsBooked = false;
			IsPreliminary = true;
			NodeId = nodeId;
			NodeLabel = nodeLabel;
			Functions = new List<Function>();
			OrderReferences = configuration.OrderReferences ?? new HashSet<Guid>();
			UserTasks = new List<LiveUserTask>();
			IsSharedSource = configuration.IsGlobalEventLevelReception;
			HasResourcesAssigned = false;
			HasAnIssueBeenreportedManually = configuration.HasAnIssueBeenreportedManually;
			SecurityViewIds = configuration.SecurityViewIds;
			NameOfServiceToTransmit = configuration.NameOfServiceToTransmit;
			ChangedByUpdateServiceScript = configuration.ChangedByUpdateServiceScript;
			ShouldStartDirectly = configuration.ShouldStartDirectly;
			SynopsisFiles = configuration.SynopsisFiles == null ? new ObservableCollection<string>() : new ObservableCollection<string>(configuration.SynopsisFiles);
			SavedStatus = EnumExtensions.GetEnumValueFromDescription<Status>(configuration.SavedStatus);

			InitializeServiceDefinitionAndFunctions(helpers, this, configuration.ServiceDefinitionId, string.Empty, configuration.Functions, nodeId, nodeLabel);

			helpers.Log(nameof(Service), nameof(Service), $"Summary of properties on service object taken from service configuration: {PropertiesToString()}", Name);

			helpers.LogMethodCompleted(nameof(Service), nameof(Service), null, stopwatch);
		}

		public static Service FromTemplate(Helpers helpers, ServiceTemplate template, DateTime startTime)
		{
			helpers.LogMethodStart(nameof(Service), nameof(FromTemplate), out var stopwatch);

			var service = new DisplayedService
			{
				Start = startTime,
				End = startTime.Add(template.Duration),
				BackupType = template.BackupType,
				IsEurovisionService = template.IsEurovisionService,
				IsSharedSource = template.IsGlobalEventLevelReceptionService,
				EurovisionBookingDetails = template.EurovisionBookingDetails ?? new EurovisionBookingDetails(),
				IntegrationType = IntegrationType.None,
				IntegrationIsMaster = false,
				Comments = template.Comments,
				ContactInformationName = template.ContactInformationName,
				ContactInformationTelephoneNumber = template.ContactInformationTelephoneNumber,
				LiveUDeviceName = template.LiveUDeviceName,
				AudioReturnInfo = template.AudioReturnInfo,
				VidigoStreamSourceLink = template.VidigoStreamSourceLink,
				AdditionalDescriptionUnknownSource = template.AdditionalDescriptionUnknownSource,
				IsUnknownSourceService = template.IsUnknownSourceService,
				RequiresRouting = template.RequiresRouting,
				RecordingConfiguration = template.RecordingConfiguration ?? new RecordingConfiguration(),
				IsBooked = false,
				IsPreliminary = true,
				NodeId = 0,
				NodeLabel = null,
				OrderReferences = new HashSet<Guid>(),
				UserTasks = new List<LiveUserTask>(),
				HasResourcesAssigned = false,
				HasAnIssueBeenreportedManually = false,
				SecurityViewIds = template.SecurityViewIds,
				LinkedServiceId = template.LinkedServiceTemplateId // Needs to be replaced with the Id of the actual linked Service
			};

			InitializeServiceDefinitionAndFunctions(helpers, service, template.ServiceDefinitionId, template.ServiceDefinitionName, template.Functions, 0, null);

			helpers.LogMethodCompleted(nameof(Service), nameof(FromTemplate), null, stopwatch);

			return service;
		}

		public void TryUpdateMcrStatus(Helpers helpers, Order orderContainingService)
		{
			MCRStatus status = DetermineMcrStatus(helpers, orderContainingService);

			TryUpdateCustomProperties(helpers, new Dictionary<string, object> { { ServicePropertyNames.MCRStatus, status.GetDescription() } });
		}

		public MCRStatus DetermineMcrStatus(Helpers helpers, Order orderContainingService)
		{
			var mcrStatus = MCRStatus.OK;
			bool mcrOperatorUserTasksAreIncomplete = UserTasks.Where(x => x.UserGroup == UserGroup.McrOperator).Any(x => x.Status == UserTaskStatus.Incomplete);

			helpers?.Log(nameof(Service), nameof(DetermineMcrStatus), $"Service has{(mcrOperatorUserTasksAreIncomplete ? string.Empty : " no")} incomplete MCR operator user tasks", Name);

			if (mcrOperatorUserTasksAreIncomplete)
			{
				mcrStatus = MCRStatus.NOK;
			}

			var routingParent = orderContainingService.AllServices.SingleOrDefault(x => x.Children.Contains(this) && x.Definition.VirtualPlatform == VirtualPlatform.Routing);

			if (routingParent != null)
			{
				bool mcrOperatorUserTasksOfParentServiceAreIncomplete = routingParent.UserTasks.Where(x => x.UserGroup == UserGroup.McrOperator).Any(x => x.Status == UserTaskStatus.Incomplete);

				helpers?.Log(nameof(Service), nameof(DetermineMcrStatus), $"Routing parent {routingParent.Name} has{(mcrOperatorUserTasksOfParentServiceAreIncomplete ? string.Empty : " no")} incomplete MCR operator user tasks", Name);

				if (mcrOperatorUserTasksOfParentServiceAreIncomplete)
				{
					mcrStatus = MCRStatus.NOK;
				}
			}

			helpers?.Log(nameof(Service), nameof(DetermineMcrStatus), $"Determined MCR status: {mcrStatus.GetDescription()}", Name);

			return mcrStatus;
		}

		public static Service FromExternalJson(Helpers helpers, Source source)
		{
			return ExternalJsonServiceCreator.CreateSourceService(helpers, source);
		}

		private static void InitializeServiceDefinitionAndFunctions(Helpers helpers, Service service, Guid serviceDefinitionId, string serviceDefinitionName, Dictionary<string, FunctionConfiguration> functionConfigurations, int nodeId, string nodeLabel)
		{
			helpers.LogMethodStart(nameof(Service), nameof(InitializeServiceDefinitionAndFunctions), out var stopwatch);

			if (serviceDefinitionId != Guid.Empty)
			{
				service.Definition = helpers.ServiceDefinitionManager.GetServiceDefinition(serviceDefinitionId) ?? throw new ServiceDefinitionNotFoundException($"Unable to find SD with ID {serviceDefinitionId} for service {service.Name}");
			}
			else if (service.IsEurovisionService)
			{
				bool hasServiceName = !string.IsNullOrEmpty(service.Name);
				bool hasServiceDefinitionName = !string.IsNullOrEmpty(serviceDefinitionName);

				bool serviceNameContainsReception = hasServiceName && service.Name.Contains("Reception");
				bool serviceNameContainsTransmission = hasServiceName && service.Name.Contains("Transmission");
				bool serviceDefinitionNameContainsReception = hasServiceDefinitionName && serviceDefinitionName.Contains("Reception");
				bool serviceDefinitionNameContainsTransmission = hasServiceDefinitionName && serviceDefinitionName.Contains("Transmission");

				// Eurovision service that was booked from the CustomerUI
				if (serviceNameContainsReception || serviceDefinitionNameContainsReception)
				{
					service.Definition = ServiceDefinition.GenerateEurovisionReceptionServiceDefinition();
				}
				else if (serviceNameContainsTransmission || serviceDefinitionNameContainsTransmission)
				{
					service.Definition = ServiceDefinition.GenerateEurovisionTransmissionServiceDefinition();
				}
				else
				{
					throw new InvalidOperationException("Unable to determine the service definition for the following Eurovision Service: " + service.Name);
				}
			}
			else if (service.IsUnknownSourceService)
			{
				service.Definition = ServiceDefinition.GenerateDummyUnknownReceptionServiceDefinition();
				service.NodeId = nodeId;
				service.NodeLabel = nodeLabel;
			}
			else
			{
				// dummy source
				service.Definition = ServiceDefinition.GenerateDummyReceptionServiceDefinition();
				service.NodeId = nodeId; // node ID of a main source service should always be 1
				service.NodeLabel = nodeLabel;
			}

			try
			{
				InitializeFunctions(helpers, service, functionConfigurations, nodeLabel);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Service), nameof(InitializeServiceDefinitionAndFunctions), $"Unable to initialize functions: {e}");
			}

			helpers.LogMethodCompleted(nameof(Service), nameof(InitializeServiceDefinitionAndFunctions), null, stopwatch);
		}

		/// <summary>
		/// Initializing service functions.
		/// </summary>
		/// <param name="helpers">helpers class.</param>
		/// <param name="service">service object.</param>
		/// <param name="functionConfigurations">Saved function configurations of this service.</param>
		/// <param name="nodeLabel">Node label of the contributing from this service.</param>
		/// <remarks>Node Label can be null if the service is created from template.</remarks>
		private static void InitializeFunctions(Helpers helpers, Service service, Dictionary<string, FunctionConfiguration> functionConfigurations, string nodeLabel)
		{
			if (helpers is null) throw new ArgumentNullException(nameof(helpers));
			if (service is null) throw new ArgumentNullException(nameof(service));
			if (functionConfigurations is null) throw new ArgumentNullException(nameof(functionConfigurations));
			if (service.Definition is null) throw new ArgumentException($"Service definition cannot be null.", nameof(service));
			if (service.Definition.Diagram is null) throw new ArgumentException($"Service definition diagram cannot be null.", nameof(service));

			foreach (var node in service.Definition.Diagram.Nodes)
			{
				var functionDefinition = service.Definition.FunctionDefinitions.SingleOrDefault(fd => fd.Label == node.Label) ?? throw new FunctionDefinitionNotFoundException(nodeLabel);
				var function = new DisplayedFunction(helpers, node, functionDefinition);

				if (functionConfigurations.TryGetValue(function.Definition.Label, out var functionConfiguration))
				{
					function.UpdateValuesBasedOnFunctionConfiguration(helpers, functionConfiguration);
				}
				else if (functionConfigurations.TryGetValue(function.Definition.Id.ToString(), out functionConfiguration))
				{
					// backwards compatibility
					function.UpdateValuesBasedOnFunctionConfiguration(helpers, functionConfiguration);
				}
				else
				{
					// Use default function
				}

				service.Functions.Add(function);
				helpers.Log(nameof(Service), nameof(Service), $"Function summary: {function.Configuration}", service.Name);
			}

			service.AudioChannelConfiguration = InitializeAudioChannelConfiguration(service.Functions, service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception);
		}

		/// <summary>
		/// Reservation instance of the service.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Name of the order that uses this service.
		/// Currently used for recording services
		/// </summary>
		public string OrderName { get; set; }

		/// <summary>
		/// The name of the service.
		/// </summary>
		public string Name
		{
			get
			{
				if (name == null && Definition != null)
				{
					// the id used here is not the id of the reservation itself
					// this id is generated when the service object itself is created initially
					// the name of a contributed service cannot be changed afterwards and is only known after booking the reservation
					// this id is included in the name because the name needs to be unique
					name = $"{Definition.VirtualPlatformServiceType.GetDescription()} [{Id}]";
					LofDisplayName = LofDisplayName ?? name;
				}

				return name;
			}

			set
			{
				name = value;
			}
		}

		/// <summary>
		/// Start date and time of the service.
		/// </summary>
		[ChangeTracked]
		public DateTime Start
		{
			get
			{
				return start;
			}

			set
			{
				if (start != value)
				{
					start = value;
					StartChanged?.Invoke(this, start);
				}
			}
		}

		internal event EventHandler<DateTime> StartChanged;

		public DateTime StartWithPreRoll => Start.Subtract(PreRoll);

		/// <summary>
		/// End date and time of the service.
		/// </summary>
		[ChangeTracked]
		public DateTime End
		{
			get => end;

			set
			{
				if (end != value)
				{
					end = value;
					EndChanged?.Invoke(this, end);
				}
			}
		}

		internal event EventHandler<DateTime> EndChanged;

		public DateTime EndWithPostRoll => End.Add(PostRoll);

		[ChangeTracked]
		public TimeSpan PreRoll
		{
			get => hasCustomPreRoll ? preRoll : ServiceManager.GetPreRollDuration(Definition, IntegrationType);

			set
			{
				hasCustomPreRoll = true;
				preRoll = value;
				PreRollChanged?.Invoke(this, PreRoll);
			}
		}

		public event EventHandler<TimeSpan> PreRollChanged;

		[ChangeTracked]
		public TimeSpan PostRoll
		{
			get => hasCustomPostRoll ? postRoll : ServiceManager.GetPostRollDuration(Definition, IntegrationType);

			set
			{
				hasCustomPostRoll = true;
				postRoll = value;
				PostRollChanged?.Invoke(this, PostRoll);
			}
		}

		public event EventHandler<TimeSpan> PostRollChanged;

		/// <summary>
		/// The status of this service.
		/// </summary>
		[ChangeTracked]
		public Status Status => GenerateStatus();

		public Status SavedStatus { get; private set; }

		public bool AllUserTasksCompleted => UserTasks.All(x => x.Status == UserTaskStatus.Complete);

		/// <summary>
		/// Indicates if this Service is saved or not. Used to determine the correct Service Status.
		/// </summary>
		public bool IsPreliminary { get; set; }

		/// <summary>
		/// The list of orders ids this service is used in.
		/// For now this doesn't contain the order itself to avoid having to retrieve all orders each time we retrieve a service.
		/// Could be improved in the future.
		/// </summary>
		[ChangeTracked]
		public HashSet<Guid> OrderReferences { get; set; } = new HashSet<Guid>();

		/// <summary>
		/// The service definition for the service.
		/// </summary>
		public ServiceDefinition Definition { get; set; }

		/// <summary>
		/// The reservation which is retrieved during FromReservationInstance() is stored in this property.
		/// </summary>
		public ServiceReservationInstance ReservationInstance { get; set; }

		private BackupType backupType = BackupType.None;

		/// <summary>
		/// The service level (backup) for the service.
		/// </summary>
		public BackupType BackupType
		{
			get => backupType;
			set
			{
				backupType = value;
			}
		}

		/// <summary>
		/// The list of functions in this service.
		/// </summary>
		[ChangeTracked]
		public List<Function> Functions { get; set; } = new List<Function>();

		public Function FirstResourceRequiringFunction => Functions.SingleOrDefault(f => FunctionIsFirstResourceRequiringFunctionInDefinition(null, f)) ?? throw new FunctionNotFoundException();

		public Function LastResourceRequiringFunction => Functions.SingleOrDefault(f => FunctionIsLastResourceRequiringFunctionInDefinition(null, f)) ?? throw new FunctionNotFoundException();

		/// <summary>
		/// Gets the configuration of the Audio Channel Profile Parameters.
		/// </summary>
		[ChangeTracked]
		public AudioChannelConfiguration AudioChannelConfiguration { get; protected set; } = new AudioChannelConfiguration(false);

		/// <summary>
		/// Is set to true when a destination or recording contains an audio configuration that is copied from source.
		/// </summary>
		public bool IsAudioConfigurationCopiedFromSource { get; set; }

		/// <summary>
		/// The comments for the service.
		/// </summary>
		[ChangeTracked]
		public string Comments
		{
			get => comments;

			set
			{
				comments = value;
				CommentsChanged?.Invoke(this, comments);
			}
		}

		internal event EventHandler<string> CommentsChanged;

		/// <summary>
		/// The list of child services for the service.
		/// </summary>
		[JsonIgnore]
		public ObservableCollection<Service> Children { get; private set; } = new ObservableCollection<Service>();

		public List<Service> Descendants => OrderManager.FlattenServices(Children);

		/// <summary>
		/// Gets all the children that are not auto generated.
		/// Checks all child branches and returns the first direct or indirect child service for each branch that is not auto generated.
		/// </summary>
		public List<Service> GetNonAutoGeneratedDescendants()
		{
			return Descendants.Where(s => !s.IsAutogenerated).ToList();
		}

		/// <summary>
		/// Indicates which integration created this Service.
		/// If not specified this will be set to None.
		/// </summary>
		public IntegrationType IntegrationType { get; set; }

		/// <summary>
		/// Gets a boolean indicating if the related integration is master or if DataMiner is the master.
		/// Used to determine which updates from which sources are allowed.
		/// Always false for manually created services.
		/// </summary>
		[ChangeTracked]
		public bool IntegrationIsMaster { get; set; } = false;

		/// <summary>
		/// The resource linked to this service.
		/// This is an internal property.
		/// </summary>
		public Resource ContributingResource { get; set; }

		/// <summary>
		/// The resource pool for the resource linked to the service (can be found in Contributing Config property of Service Definition).
		/// This is an internal property.
		/// </summary>
		public ResourcePool ResourcePool { get; set; }

		/// <summary>
		/// The node id for this service in the order service definition.
		/// This is an internal property.
		/// </summary>
		public int NodeId { get; set; }

		/// <summary>
		/// The node label for this service in the order service definition.
		/// </summary>
		public string NodeLabel { get; set; }

		/// <summary>
		/// The available interfaces for this service in the order service definitions.
		/// This is an internal property.
		/// </summary>
		public IEnumerable<InterfaceConfiguration> Interfaces { get; set; } = new List<InterfaceConfiguration>();

		/// <summary>
		/// Indicates if this service can be used as an global Event Level Reception.
		/// This is an internal property.
		/// </summary>
		public bool IsSharedSource
		{
			get => isSharedSource;
			set
			{
				if (value == IsSharedSource) return;

				isSharedSource = value;
				IsSharedSourceChanged?.Invoke(this, IsSharedSource);
			}
		}

		public event EventHandler<bool> IsSharedSourceChanged;

		/// <summary>
		/// Gets a boolean indicating if this service is already booked as a global ELR.
		/// </summary>
		public bool IsAlreadyBookedAsSharedSource => IsBooked && IsSharedSource;

		/// <summary>
		/// Indicates if this service has any Resources assigned to it.
		/// </summary>
		public bool HasResourcesAssigned { get; set; }

		/// <summary>
		/// It only applies to Child Services (Destination/Recording) of a backup source when the Service Level of the Backup is Active.
		/// This is the Id of the Service to which this service is linked.
		/// </summary>
		public Guid LinkedServiceId { get; set; }

		/// <summary>
		/// Should be passed from the UI to the Library when a new Order is created where the Backup source is configured as Active backup.
		/// This property links the Source child service to it backup counterpart and vice versa.
		/// </summary>
		[JsonIgnore]
		public Service LinkedService { get; set; }

		/// <summary>
		/// Should be passed from the UI to the Library when an order is created.
		/// This property contains the type of the order this service is part of.
		/// </summary>
		public OrderType OrderType { get; set; }

		/// <summary>
		/// A list of User Tasks linked to this service.
		/// </summary>
		[JsonIgnore]
		public List<LiveUserTask> UserTasks { get; set; } = new List<LiveUserTask>();

		/// <summary>
		/// Indicates if this is an Eurovision service.
		/// </summary>
		public bool IsEurovisionService { get; set; }

		/// <summary>
		/// This is set to true from the HandleIntegrationUpdate script if the synopsis contains a value for the ioMuxName field.
		/// The ioMuxName field is used to define multiple multiplexed feeds in the signal.
		/// If this boolean is true, you will be able to edit the Service Selection profile parameter of the Satellite Reception in the LiveOrderForm.
		/// </summary>
		public bool IsEurovisionMultiFeedService { get; set; }

		private string eurovisionWorkOrderId;

		public event EventHandler<string> EurovisionWorkOrderIdChanged;

		/// <summary>
		/// The id of the Eurovision work order that was requested through the Live Order Form.
		/// </summary>
		public string EurovisionWorkOrderId
		{
			get => eurovisionWorkOrderId;
			set
			{
				eurovisionWorkOrderId = value;
				EurovisionWorkOrderIdChanged?.Invoke(this, eurovisionWorkOrderId);
			}
		}

		/// <summary>
		/// The transmission number of the Eurovision booking in case of a Eurovision service.
		/// This can be used to map the incoming Eurovision synopsis to this service.
		/// This is only applicable if this is a Eurovision service and if it was automatically created by the Eurovision integration.
		/// </summary>
		public string EurovisionTransmissionNumber { get; set; }

		/// <summary>
		/// Used in the UI to indicate if a Eurovision Number should be linked to this service.
		/// </summary>
		public bool LinkEurovisionId => EurovisionBookingDetails?.Type == Integrations.Eurovision.Type.None;

		/// <summary>
		/// Only used in the CustomerUI script.
		/// This value is not stored or retrieved from the SRM configuration.
		/// Used to determine if the Resource assigned to this Service should be included when updating the available Resources in the script.
		/// </summary>
		public bool IsDuplicate { get; set; } = false;

		/// <summary>
		/// ID of the recording session in EVS.
		/// </summary>
		public string EvsId { get; set; }

		/// <summary>
		/// Gets a boolean indicating if this Recording service changed within 12 hour (or less) of the start time.
		/// </summary>
		public bool LateChange { get; set; }

		/// <summary>
		/// Serialized data of the Eurovision booking details as configured in the Customer UI.
		/// This is only applicable in case of an Eurovision service.
		/// </summary>
		public EurovisionBookingDetails EurovisionBookingDetails { get; set; } = new EurovisionBookingDetails();

		/// <summary>
		/// This property is only used when the Service is a Dummy Service that was generated from the EBU integration.
		/// In that case this property will contain the possible Receptions or Transmissions.
		/// </summary>
		public List<ServiceConfiguration> EurovisionServiceConfigurations { get; set; }

		/// <summary>
		/// This property is only used when the Service is a Reception Service that was generated by the EBU integration.
		/// In that case this property will contain the id of the destination in the synopsis on which this service is based. 
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string EurovisionDestinationId { get; set; }

		/// <summary>
		/// This property is only used when the Service is a Reception or Transmission Service that was generated by the EBU integration.
		/// In that case this property will contain the id of the technical system in the synopsis on which this service is based. 
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string EurovisionTechnicalSystemId { get; set; }

		/// <summary>
		/// Whenever an order contains an unknown source the user is able to specify additional information 
		/// So that a user in a later stage can select the desired source whenever they want to book the order definitively.
		/// </summary>
		public string AdditionalDescriptionUnknownSource { get; set; }

		/// <summary>
		/// Indicates if this service is unknown.
		/// </summary>
		public bool IsUnknownSourceService { get; set; }

		/// <summary>
		/// Contact Information Name.
		/// Only applicable for LiveU receptions.
		/// Will be saved in a custom property for LiveU receptions;
		/// </summary>
		[ChangeTracked]
		public string ContactInformationName { get; set; }

		/// <summary>
		/// Contact Information Telephone Number.
		/// Only applicable for LiveU receptions.
		/// Will be saved in a custom property for LiveU receptions;
		/// </summary>
		[ChangeTracked]
		public string ContactInformationTelephoneNumber { get; set; }

		/// <summary>
		/// Additional information reception/transmission LiveU device name.
		/// Only applicable for LiveU receptions or transmissions.
		/// Will be saved in a custom property for LiveU receptions or transmissions;
		/// </summary>
		[ChangeTracked]
		public string LiveUDeviceName { get; set; }

		/// <summary>
		/// Normal users will fill in this field when LiveU is selected
		/// News users will be able to fill in this field for every reception service.
		/// </summary>
		[ChangeTracked]
		public string AudioReturnInfo { get; set; }

		/// <summary>
		/// Additional info for IP RX Vidigo services.
		/// Will be saved in custom property on IP receptions.
		/// </summary>
		[ChangeTracked]
		public string VidigoStreamSourceLink { get; set; }

		/// <summary>
		/// Paths of the synopsis files that were uploaded through the LiveOrderForm.
		/// These only apply to Satellite Rx services.
		/// </summary>
		[ChangeTracked]
		public ObservableCollection<string> SynopsisFiles { get; } = new ObservableCollection<string>();

		/// <summary>
		/// Contains additional recording configurations that are not stored as profile parameters.
		/// </summary>
		[ChangeTracked]
		public RecordingConfiguration RecordingConfiguration { get; set; } = new RecordingConfiguration();

		public string NameOfServiceToTransmit { get; set; }

		public event EventHandler<string> NameOfServiceToTransmitOrRecordChanged;

		public string NameOfServiceToTransmitOrRecord
		{
			get
			{
				if (Definition?.VirtualPlatformServiceType == VirtualPlatformType.Recording) return RecordingConfiguration?.NameOfServiceToRecord ?? string.Empty;
				else if (Definition?.VirtualPlatformServiceType == VirtualPlatformType.Transmission) return NameOfServiceToTransmit ?? string.Empty;
				else return string.Empty;
			}
			set
			{
				if (Definition?.VirtualPlatformServiceType == VirtualPlatformType.Recording && RecordingConfiguration != null)
				{
					RecordingConfiguration.NameOfServiceToRecord = value;
					NameOfServiceToTransmitOrRecordChanged?.Invoke(this, value);
				}
				else if (Definition?.VirtualPlatformServiceType == VirtualPlatformType.Transmission)
				{
					NameOfServiceToTransmit = value;
					NameOfServiceToTransmitOrRecordChanged?.Invoke(this, value);
				}
				else
				{
					// Property doesn't apply to other service types
				}
			}
		}



		public event EventHandler<IReadOnlyList<Service>> AvailableServicesToRecordOrTransmitChanged;

		// Used in UI to fill out dropdown with services to record or transmit
		public IReadOnlyList<Service> AvailableServicesToRecordOrTransmit
		{
			get => availableServicesToRecordOrTransmit;
			set
			{
				availableServicesToRecordOrTransmit = value ?? throw new ArgumentNullException("value");
				AvailableServicesToRecordOrTransmitChanged?.Invoke(this, value);
			}
		}

		public bool IsHmxRouting
		{
			get
			{
				if (Definition.VirtualPlatform != VirtualPlatform.Routing) return false;

				var firstResource = Functions[0].Resource;
				string firstResourceName = firstResource?.Name;

				var lastResource = Functions.Last().Resource;
				string lastResourceName = lastResource?.Name;

				bool isHmxRouting = firstResourceName?.IndexOf("HMX", StringComparison.InvariantCultureIgnoreCase) != -1 || lastResourceName?.IndexOf("HMX", StringComparison.InvariantCultureIgnoreCase) != -1;

				return isHmxRouting;
			}
		}

		public bool IsNewsRouting
		{
			get
			{
				if (Definition.VirtualPlatform != VirtualPlatform.Routing) return false;

				var firstResource = Functions[0].Resource;
				string firstResourceName = firstResource?.Name;

				var lastResource = Functions.Last().Resource;
				string lastResourceName = lastResource?.Name;

				bool isNewsRouting = firstResourceName?.IndexOf("NMX", StringComparison.InvariantCultureIgnoreCase) != -1 || lastResourceName?.IndexOf("NMX", StringComparison.InvariantCultureIgnoreCase) != -1;

				return isNewsRouting;
			}
		}

		/// <summary>
		/// Indicates if routing is required for this service.
		/// </summary>
		public bool RequiresRouting { get; set; } = true;

		/// <summary>
		/// Flag indicating that the EVS automation should be triggered from the background if applicable.
		/// Usually used in certain cases where the foreground script would update an EVS Recording service in SRM.
		/// </summary>
		public bool MajorTimeslotChange { get; set; }

		/// <summary>
		/// Indicates if this Service is a Dummy Service.
		/// </summary>
		public bool IsDummy => Definition == null || Definition.IsDummy;

		/// <summary>
		/// Indicates if the Service is a Dummy reception service that was generated by EBU Integration.
		/// </summary>
		public bool IsEbuDummyReception
		{
			get
			{
				if (Definition == null) return false;
				return IntegrationType == IntegrationType.Eurovision && IsDummy && Definition.ContributingConfig != null && Definition.ContributingConfig.ParentSystemFunction == ServiceManager.SourceServiceSystemFunctionId;
			}
		}

		/// <summary>
		/// Indicates if the Service is a Dummy transmission service that was generated by EBU Integration.
		/// </summary>
		public bool IsEbuDummyTransmission
		{
			get
			{
				if (Definition == null) return false;
				return IntegrationType == IntegrationType.Eurovision && IsDummy && Definition.ContributingConfig != null && Definition.ContributingConfig.ParentSystemFunction == ServiceManager.TransmissionServiceSystemFunctionId;
			}
		}

		public bool IsBooked
		{
			get
			{
				return isBooked || ReservationInstance != null;
			}
			set
			{
				isBooked = value;
			}
		}

		public string ProfileConfigurationFailReason { get; set; }

		/// <summary>
		/// Indication if this service is newly added to a running order and if it should start directly (start now) or at a later stage.
		/// </summary>
		public bool ShouldStartDirectly { get; set; }

		/// <summary>
		/// Gets a collection of DataMiner Cube view IDs of views where this service is visible. If empty, this service should be visible for everyone.
		/// </summary>
		[ChangeTracked]
		public HashSet<int> SecurityViewIds { get; set; } = new HashSet<int>();

		public string LofDisplayName { get; set; }

		/// <summary>
		/// A boolean used to skip certain logic because the user input from the UpdateService script should overwrite it
		/// </summary>
		public bool ChangedByUpdateServiceScript { get; set; } = false;

		/// <summary>
		/// Gets a boolean indicating if Change Tracking is enabled.
		/// </summary>
		/// <see cref="IYleChangeTracking"/>
		[JsonIgnore]
		public bool ChangeTrackingStarted { get; private set; }

		/// <summary>
		/// Gets a boolean indicating if the service is a Plasma recording with Messi Live Backup service definition.
		/// Used by UI.
		/// </summary>
		public bool UI_IsBackupService
		{
			get => Definition?.Id == ServiceDefinitionGuids.RecordingMessiLiveBackup && IntegrationType == IntegrationType.Plasma;
		}

		/// <summary>
		/// Gets a boolean indicating if service has commentary audio.
		/// Used by UI.
		/// </summary>
		public bool UI_HasCommentaryAudio
		{
			get => false;
		} // TODO implement when audio commentary is supported.

		/// <summary>
		/// If an issue has been reported manually via the UpdateService script and when the order is finished.
		/// The service will have the status Completed With Errors at the end.
		/// </summary>
		public bool HasAnIssueBeenreportedManually { get; set; }

		/// <summary>
		/// Indicates whether the Service should stop immediately.
		/// </summary>
		public bool StopNow { get; set; } = false;

		/// <summary>
		/// Checks whether the service should be running (mainly timings) or is running (service status)
		/// </summary>
		public bool IsOrShouldBeRunning
		{
			get
			{
				DateTime now = DateTime.Now;
				var serviceStatus = Status;

				bool isServiceStatusAndTimingValid = now >= StartWithPreRoll && now <= EndWithPostRoll && serviceStatus != Status.Preliminary && Id != Guid.Empty;
				return serviceStatus == Status.ServiceRunning || isServiceStatusAndTimingValid;
			}
		}

		/// <summary>
		/// Checks whether the service should be running without checking whether its state is running.
		/// </summary>
		public bool ShouldBeRunning
		{
			get
			{
				DateTime now = DateTime.Now;
				var serviceStatus = Status;
				return now >= StartWithPreRoll && now <= EndWithPostRoll && serviceStatus != Status.Preliminary && Id != Guid.Empty;
			}
		}

		private bool IsInPrerollTimeSpan
		{
			get
			{
				DateTime now = DateTime.Now;
				return StartWithPreRoll <= now && now < Start;
			}
		}

		public bool InPreRollState
		{
			get
			{

				var serviceStatus = Status;
				return serviceStatus == Status.ServiceQueuingConfigOk || serviceStatus == Status.ServiceQueingAndConfigFailed || IsInPrerollTimeSpan;
			}
		}

		/// <summary>
		/// Indicates if the service was automatically generated.
		/// This is determined by checking the Virtual Platform of the Service Definition.
		/// </summary>
		public bool IsAutogenerated
		{
			get
			{
				if (Definition == null) return false;
				if (Definition.VirtualPlatform == VirtualPlatform.Routing) return true;
				if (Definition.VirtualPlatform == VirtualPlatform.AudioProcessing) return true;
				if (Definition.VirtualPlatform == VirtualPlatform.GraphicsProcessing) return true;
				if (Definition.VirtualPlatform == VirtualPlatform.FileProcessing) return true;
				if (Definition.VirtualPlatform == VirtualPlatform.VideoProcessing) return true;
				return false;
			}
		}

		/// <summary>
		/// Generates a dummy source service to be used in case no source is defined for a saved order.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="start">The start time.</param>
		/// <param name="end">The end time.</param>
		/// <returns>New dummy service with the specified start and end times.</returns>
		public static Service GenerateDummyReception(Helpers helpers, DateTime start, DateTime end)
		{
			return new DisplayedService(helpers, ServiceDefinition.GenerateDummyReceptionServiceDefinition())
			{
				Start = start,
				End = end,
				BackupType = BackupType.None,
				UserTasks = new List<LiveUserTask>()
			};
		}

		public void SetChildren(IEnumerable<Service> children)
		{
			Children.Clear();
			foreach (var child in children.ToList())
			{
				Children.Add(child);
			}
		}

		/// <summary>
		/// Generates a dummy unknown source service to be used in case no source is known for a saved order.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="start">The start time.</param>
		/// <param name="end">The end time.</param>
		/// <returns>New dummy service with the specified start and end times.</returns>
		public static Service GenerateDummyUnknownReception(Helpers helpers, DateTime start, DateTime end)
		{
			return new DisplayedService(helpers, ServiceDefinition.GenerateDummyUnknownReceptionServiceDefinition())
			{
				Start = start,
				End = end,
				BackupType = BackupType.None,
			};
		}

		public static Service GenerateDummyTransmission(Helpers helpers, DateTime start, DateTime end)
		{
			return new DisplayedService(helpers, ServiceDefinition.GenerateDummyTransmissionServiceDefinition())
			{
				Start = start,
				End = end,
				BackupType = BackupType.None,
			};
		}

		public ServiceConfiguration GetConfiguration()
		{
			var serviceConfiguration = new ServiceConfiguration
			{
				Id = Id,
				Name = Name,
				PreRoll = PreRoll,
				PostRoll = PostRoll,
				Start = Start.ToUniversalTime(),
				End = End.ToUniversalTime(),
				Functions = new Dictionary<string, FunctionConfiguration>(),
				ServiceDefinitionId = Definition.Id,
				ServiceLevel = BackupType,
				SavedStatus = SavedStatus.GetDescription(),
				LinkedServiceId = LinkedServiceId,
				IsEurovisionService = IsEurovisionService,
				IsEurovisionMultiFeedService = IsEurovisionMultiFeedService,
				EurovisionWorkOrderId = EurovisionWorkOrderId ?? String.Empty,
				EurovisionTransmissionNumber = EurovisionTransmissionNumber ?? String.Empty,
				EurovisionBookingDetails = EurovisionBookingDetails,
				EurovisionDestinationId = EurovisionDestinationId ?? String.Empty,
				EurovisionTechnicalSystemId = EurovisionTechnicalSystemId ?? String.Empty,
				EvsId = EvsId ?? String.Empty,
				IntegrationType = IntegrationType,
				IntegrationIsMaster = IntegrationIsMaster,
				Comments = !string.IsNullOrWhiteSpace(Comments) ? Comments.Clean(allowSiteContent: true) : string.Empty,
				ContactInformationName = !string.IsNullOrWhiteSpace(ContactInformationName) ? ContactInformationName : string.Empty,
				ContactInformationTelephoneNumber = !string.IsNullOrWhiteSpace(ContactInformationTelephoneNumber) ? ContactInformationTelephoneNumber : string.Empty,
				LiveUDeviceName = !string.IsNullOrWhiteSpace(LiveUDeviceName) ? LiveUDeviceName : string.Empty,
				AudioReturnInfo = !string.IsNullOrWhiteSpace(AudioReturnInfo) ? AudioReturnInfo : string.Empty,
				RequiresRouting = RequiresRouting,
				MajorTimeslotChange = MajorTimeslotChange,
				RecordingConfiguration = RecordingConfiguration,
				EurovisionServiceConfigurations = EurovisionServiceConfigurations,
				IsGlobalEventLevelReception = IsSharedSource,
				HasAnIssueBeenreportedManually = HasAnIssueBeenreportedManually,
				SecurityViewIds = SecurityViewIds,
				NameOfServiceToTransmit = NameOfServiceToTransmit ?? String.Empty,
				OrderReferences = OrderReferences,
				VidigoStreamSourceLink = VidigoStreamSourceLink ?? String.Empty,
				AdditionalDescriptionUnknownSource = !string.IsNullOrWhiteSpace(AdditionalDescriptionUnknownSource) ? AdditionalDescriptionUnknownSource : string.Empty,
				IsUnknownSourceService = IsUnknownSourceService,
				IsAudioConfigurationCopiedFromSource = IsAudioConfigurationCopiedFromSource,
				ChangedByUpdateServiceScript = ChangedByUpdateServiceScript,
				ShouldStartDirectly = ShouldStartDirectly,
				SynopsisFiles = new HashSet<string>(SynopsisFiles)
			};

			if (Functions == null) return serviceConfiguration;

			foreach (var function in Functions)
			{
				serviceConfiguration.Functions[function.Definition.Label] = function.Configuration;
			}
			return serviceConfiguration;
		}

		public void StartNow(Helpers helpers)
		{
			var bookingManager = new Library.Solutions.SRM.BookingManager((DataMiner.Automation.Engine)helpers.Engine, helpers.Engine.FindElement(Definition.BookingManagerElementName)) { CustomProperties = true };

			var reservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id);

			if (reservation.Status != ReservationStatus.Confirmed)
			{
				try
				{
					reservation = DataMinerInterface.BookingManager.ChangeStateToConfirmed(helpers, bookingManager, reservation);
					helpers.Log(nameof(Service), nameof(StartNow), Name + " was confirmed");
				}
				catch (Exception ex)
				{
					helpers.Log(nameof(Service), nameof(StartNow), $"Unable to confirm {Name}: {ex}");
				}
			}

			helpers.Log(nameof(Service), nameof(StartNow), "Starting " + Name + " now...");

			bookingManager.EventReschedulingDelay = TimeSpan.Zero;
			if (DataMinerInterface.BookingManager.TryStart(helpers, bookingManager, ref reservation))
			{
				//System.InvalidOperationException: Only Confirmed reservations can be immediately started up.

				helpers.Log(nameof(Service), nameof(StartNow), Name + " was started");

				Start = DateTimeExtensions.RoundToMinutes(reservation.Start.FromReservation().Add(reservation.GetPreRoll()));

				helpers.Log(nameof(Service), nameof(StartNow), "new start time: " + Start);
			}
			else
			{
				helpers.Log(nameof(Service), nameof(StartNow), "Unable to start service " + Name + " now");
			}

			ReservationInstance = reservation as ServiceReservationInstance;
		}

		public IEnumerable<Task> StopService(Helpers helpers, Order orderContainingService)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (orderContainingService == null) throw new ArgumentNullException(nameof(orderContainingService));

			var serviceStopHandler = new ServiceStopHandler(helpers, orderContainingService, this);

			serviceStopHandler.Execute(out var serviceTasks, out bool createUserTaskForThisService);

			return serviceTasks;
		}

		public void TryStopServiceNow(Helpers helpers)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			if (IsBooked)
			{
				helpers.Log(nameof(Service), nameof(TryStopServiceNow), $"Stopping booked service {Name} now...");
				StopBookedService(helpers);
			}
			else
			{
				helpers.Log(nameof(Service), nameof(TryStopServiceNow), "Stopping saved service" + Name + " now...");
				End = DateTimeExtensions.RoundToMinutes(DateTime.Now);
			}
		}

		private void StopBookedService(Helpers helpers)
		{
			// No booking manager element can be found for dummy services -> null ref exception
			var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(Definition.BookingManagerElementName)) { AllowPostroll = true, AllowPreroll = true, CustomProperties = true };
			var reservation = helpers.ServiceManager.GetReservation(Id);

			bool serviceWilEndAutomatically = reservation.Status == ReservationStatus.Ongoing && DateTime.Now.RoundToMinutes() >= reservation.End.Subtract(reservation.GetPostRoll()).ToLocalTime();
			if (serviceWilEndAutomatically)
			{
				helpers.Log(nameof(Service), nameof(StopBookedService), $"No extra stop action needed as the booking will stop automatically in the background");
				return;
			}

			bookingManager.EventReschedulingDelay = TimeSpan.FromSeconds(30);

			try
			{
				reservation = DataMinerInterface.BookingManager.Finish(helpers, bookingManager, reservation);

				var reservationEnd = reservation.End.FromReservation().Subtract(reservation.GetPostRoll());

				End = DateTimeExtensions.RoundToMinutes(reservationEnd); // Update End time of service
				PostRoll = reservation.GetPostRoll();

				var updateEvsTask = new AddOrUpdateInEvsTask(helpers, this);
				if (!updateEvsTask.Execute(false))
				{
					helpers.Log(nameof(Service), nameof(StopBookedService), $"Unable to update EVS block due to {updateEvsTask.Exception}");
				}

				helpers.Log(nameof(Service), nameof(StopBookedService), $"Service {Name} is stopped. Reservation end is {reservationEnd} {reservationEnd.Kind}. Service object end is set to {End} {End.Kind}");
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(Service), nameof(StopBookedService), $"Unable to stop service {Name} now: {ex}");
			}
		}

		public bool TryUpdateStatus(Helpers helpers, Order orderContainingService = null, LsoEnhancedAction handleServiceAction = null, bool updateOrderStatus = true)
		{
			using (MetricLogger.StartNew(helpers, nameof(Service)))
			{
				if (!IsBooked) return true; // No status updates possible for unbooked services

				// We need to wait until the service reservation has the correct booking life cycle to proceed.
				if (!TryWaitingUntilServiceHasValidBookingLifeCycle(helpers, handleServiceAction, out var serviceReservationInstance))
				{
					helpers.Log(nameof(Service), nameof(TryUpdateStatus), $"Waiting on the correct service booking life cycle didn't succeed", Name);
					return false;
				}

				var serviceReservation = serviceReservationInstance ?? DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id);
				if (serviceReservation == null)
				{
					helpers.Log(nameof(Service), nameof(TryUpdateStatus), "ReservationInstance could not be retrieved", Name);
					return false;
				}

				var newServiceStatus = GenerateStatus(helpers, orderContainingService);
				helpers.Log(nameof(Service), nameof(TryUpdateStatus), $"New (generated) service status: {newServiceStatus.ToString()}", Name);
				if (newServiceStatus == SavedStatus)
				{
					helpers.Log(nameof(Service), nameof(TryUpdateStatus), "Previous status and new status are exactly the same so no need to update", Name);
					return false;
				}

				if (!TrySetStatusOnReservation(helpers, serviceReservation, newServiceStatus))
				{
					helpers.Log(nameof(Service), nameof(TryUpdateStatus), $"Failed to update service {Name} reservation status to {newServiceStatus}");
					return false;
				}

				bool orderStatusUpdateAllowed = !IsSharedSource && updateOrderStatus;
				bool orderStatusUpdateRequired = orderStatusUpdateAllowed && (newServiceStatus == Status.ServiceRunning || newServiceStatus == Status.PostRoll || newServiceStatus == Status.ServiceCompleted || newServiceStatus == Status.ServiceCompletedWithErrors || newServiceStatus == Status.FileProcessing);

				helpers.Log(nameof(Service), nameof(TryUpdateStatus), $"Order(s) status update {(orderStatusUpdateRequired ? string.Empty : "not ")}required after service status update");

				if (orderStatusUpdateRequired)
				{
					UpdateOrderStatusesAfterServiceStatusUpdate(helpers, orderContainingService, handleServiceAction, orderStatusUpdateAllowed);
				}

				return true;
			}
		}

		private void UpdateOrderStatusesAfterServiceStatusUpdate(Helpers helpers, Order orderContainingService, LsoEnhancedAction handleServiceAction, bool orderStatusUpdateAllowed)
		{
			foreach (var orderId in OrderReferences)
			{
				helpers.Log(nameof(Service), nameof(TryUpdateStatus), $"Handling order id: {orderId}");
				try
				{
					Order order = null;
					if (orderId == orderContainingService?.Id)
					{
						helpers.Log(nameof(Service), nameof(TryUpdateStatus), "Using orderContainingService");
						order = orderContainingService;
					}
					else
					{
						helpers.Log(nameof(Service), nameof(TryUpdateStatus), $"orderContainingService is null or doesn't match order reference => retrieving order with id: {orderId}");
						order = helpers.OrderManager.GetOrder(orderId, false, true);
					}

					if (order == null) throw new OrderNotFoundException(orderId);

					bool triggeredByManualInteraction = handleServiceAction == null && orderStatusUpdateAllowed;
					order.RefreshStatusAfterServiceStatusUpdate(helpers, triggeredByManualInteraction);

					helpers.Log(nameof(Service), nameof(TryUpdateStatus), "Order status refreshed successfully");
				}
				catch (Exception e)
				{
					helpers.Log(nameof(Service), nameof(TryUpdateStatus), $"Exception refreshing order {orderId} status after service update: {e}");
				}
			}
		}

		public bool WaitForSrmStatus(Helpers helpers, ReservationStatus desiredStatus, out ReservationInstance reservationWithCorrectStatus)
		{
			helpers.LogMethodStart(nameof(Service), nameof(WaitForSrmStatus), out var stopwatch);

			int retries = 0;
			do
			{
				reservationWithCorrectStatus = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id);
				if (reservationWithCorrectStatus == null)
				{
					retries++;
					Thread.Sleep(500);
					continue;
				}

				if (reservationWithCorrectStatus.Status == desiredStatus)
				{
					helpers.Log(nameof(Service), nameof(WaitForSrmStatus), $"Service {Name} desired SRM status {desiredStatus}", Name);
					helpers.LogMethodCompleted(nameof(Service), nameof(WaitForSrmStatus), Name, stopwatch);
					return true;
				}

				helpers.Log(nameof(Service), nameof(WaitForSrmStatus), $"Retry {retries}: Service {Name} has status {reservationWithCorrectStatus.Status}, which is not desired SRM status {desiredStatus}", Name);

				retries++;
				Thread.Sleep(500);

			}
			while (retries < 10);

			helpers.LogMethodCompleted(nameof(Service), nameof(WaitForSrmStatus), Name, stopwatch);

			return false;
		}

		/// <summary>
		/// Waits until the main booking has the actual desired booking life cycle. Matching with the action (pre roll/start/stop/post roll) of the order reservation.
		/// </summary>
		/// <param name="helpers">helpers class.</param>
		/// <param name="action">Will be filled in when the status update is called via Handle Service Action, otherwise should be empty.</param>
		/// <param name="serviceReservation">The actual reservation instance of the service it self, when reaching the correct booking life cycle the reservation instance will be up to date.</param>
		private bool TryWaitingUntilServiceHasValidBookingLifeCycle(Helpers helpers, LsoEnhancedAction action, out ReservationInstance serviceReservation)
		{
			helpers.LogMethodStart(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), out var stopWatch, Name);

			serviceReservation = null;

			try
			{
				if (action is null)
				{
					helpers.LogMethodCompleted(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), Name, stopWatch);
					return true;
				}

				bool triggeredByPreRoll = action.Event == Library.Solutions.SRM.Model.Events.SrmEvent.START_BOOKING_WITH_PREROLL;
				bool triggeredByStart = action.Event == Library.Solutions.SRM.Model.Events.SrmEvent.START;
				bool triggeredByPostRoll = action.Event == Library.Solutions.SRM.Model.Events.SrmEvent.STOP;
				bool triggeredByEnd = action.Event == Library.Solutions.SRM.Model.Events.SrmEvent.STOP_BOOKING_WITH_POSTROLL;

				int retries = 0;

				do
				{
					serviceReservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id);
					if (serviceReservation == null)
					{
						retries++;
						Thread.Sleep(500);
						continue;
					}

					var bookingLifeCycle = serviceReservation.GetBookingLifeCycle();

					if (triggeredByPreRoll && bookingLifeCycle == GeneralStatus.Starting)
					{
						helpers.Log(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), $"Service {Name} is containing a starting booking life cycle", Name);
						helpers.LogMethodCompleted(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), Name, stopWatch);
						return true;
					}
					else if (triggeredByStart && bookingLifeCycle == GeneralStatus.Running)
					{
						helpers.Log(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), $"Service {Name} is containing a running booking life cycle", Name);
						helpers.LogMethodCompleted(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), Name, stopWatch);
						return true;
					}
					else if (triggeredByPostRoll && bookingLifeCycle == GeneralStatus.Stopping)
					{
						helpers.Log(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), $"Service {Name} is containing a stopping booking life cycle", Name);
						helpers.LogMethodCompleted(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), Name, stopWatch);
						return true;
					}
					else if (triggeredByEnd && bookingLifeCycle == GeneralStatus.Completed)
					{
						helpers.Log(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), $"Service {Name} is containing a completed booking life cycle", Name);
						helpers.LogMethodCompleted(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), Name, stopWatch);
						return true;
					}
					else
					{
						//Nothing
					}

					retries++;
					Thread.Sleep(500);

				} while (retries < 10);

				helpers.LogMethodCompleted(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), Name, stopWatch);

				return false;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Service), nameof(TryWaitingUntilServiceHasValidBookingLifeCycle), $"Service {Name} encountered an issue while waiting on the correct booking life cycle: {e}", Name);
				return false;
			}
		}

		private bool TrySetStatusOnReservation(Helpers helpers, ReservationInstance serviceReservation, Status newServiceStatus)
		{
			helpers.LogMethodStart(nameof(Service), nameof(TrySetStatusOnReservation), out var stopWatch, Name);

			string desiredServiceStatus = newServiceStatus.GetDescription();

			bool updateSuccessful = TryUpdateCustomProperties(helpers, new Dictionary<string, object> { { ServicePropertyNames.Status, desiredServiceStatus } }, serviceReservation);

			helpers.Log(nameof(Service), nameof(TrySetStatusOnReservation), $"Updating custom property '{ServicePropertyNames.Status}' to value '{desiredServiceStatus}' {(updateSuccessful ? "succeeded" : "failed")}", Name);

			helpers.LogMethodCompleted(nameof(Service), nameof(TrySetStatusOnReservation), Name, stopWatch);

			return updateSuccessful;
		}

		public UpdateResult TryUpdateIfThisServiceIsSharedSource(Helpers helpers, Order orderContainingService)
		{
			if (!IsSharedSource) throw new NotSupportedException("This method should only be called for Shared Sources");
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (orderContainingService == null) throw new ArgumentNullException(nameof(orderContainingService));

			if (!this.IsBooked)
			{
				helpers.Log(nameof(Service), nameof(TryUpdateIfThisServiceIsSharedSource), $"Shared Source {Name} is not booked yet, update not possible");
				return new UpdateResult { UpdateWasSuccessful = true };
			}

			var existingService = helpers.ServiceManager.GetService(Id);

			var serviceAddOrUpdateHandler = new EventLevelReceptionUpdateHandler(helpers, this, orderContainingService, existingService);

			return serviceAddOrUpdateHandler.Execute(out var serviceTasks, out bool createUserTaskForThisService) ? new UpdateResult { Tasks = serviceTasks, UpdateWasSuccessful = true } : new UpdateResult { UpdateWasSuccessful = false };
		}

		public bool TryUpdateCustomProperties(Helpers helpers, Dictionary<string, object> customProperties)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (customProperties == null) throw new ArgumentNullException(nameof(customProperties));

			var reservationInstance = ReservationInstance ?? DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id);

			if (reservationInstance is null)
			{
				helpers.Log(nameof(Service), nameof(TryUpdateCustomProperties), "ReservationInstance could not be retrieved");
				return false;
			}

			return TryUpdateCustomProperties(helpers, customProperties, reservationInstance);
		}

		public static bool TryUpdateCustomProperties(Helpers helpers, Dictionary<string, object> customProperties, ReservationInstance reservationInstance)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (customProperties == null) throw new ArgumentNullException(nameof(customProperties));
			if (reservationInstance == null) throw new ArgumentNullException(nameof(reservationInstance));

			// To avoid that null values inside the custom properties dictionary throw a 'value is not serializable to JSON' exception.
			var keysOfCustomProperties = new List<string>(customProperties.Keys);
			foreach (var key in keysOfCustomProperties)
			{
				if (customProperties[key] == null) customProperties[key] = String.Empty;
			}

			try
			{
				if (customProperties.Any())
				{
					helpers.Log(nameof(Service), nameof(TryUpdateCustomProperties), $"Update service reservation {reservationInstance.Name} ({reservationInstance.ID}) custom properties: '{string.Join(",", customProperties.Select(cp => $"{cp.Key}='{cp.Value.ToString()}'"))}'");
					DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(helpers, reservationInstance, customProperties);
				}
				else
				{
					helpers.Log(nameof(Service), nameof(TryUpdateCustomProperties), $"No custom properties to update on service reservation", reservationInstance.Name);
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Service), nameof(TryUpdateCustomProperties), $"Exception updating service reservation properties: {e}");
				return false;
			}

			return true;
		}

		public List<Service> GetChildServicesForVirtualPlatformType(VirtualPlatformType virtualPlatformType)
		{
			switch (virtualPlatformType)
			{
				case VirtualPlatformType.Reception:
					{
						VirtualPlatform[] virtualPlatforms = new VirtualPlatform[] { VirtualPlatform.ReceptionEurovision, VirtualPlatform.ReceptionFiber, VirtualPlatform.ReceptionFixedLine, VirtualPlatform.ReceptionIp, VirtualPlatform.ReceptionLiveU, VirtualPlatform.ReceptionMicrowave, VirtualPlatform.ReceptionSatellite, VirtualPlatform.ReceptionNone };
						return GetChildServicesForVirtualPlatform(virtualPlatforms);
					}

				case VirtualPlatformType.Recording:
					return GetChildServicesForVirtualPlatform(VirtualPlatform.Recording);
				case VirtualPlatformType.Destination:
					return GetChildServicesForVirtualPlatform(VirtualPlatform.Destination);
				case VirtualPlatformType.Routing:
					return GetChildServicesForVirtualPlatform(VirtualPlatform.Routing);
				case VirtualPlatformType.Transmission:
					{
						// Added None to the list of Transmission VPs to support Dummy Tx Services. These are the only type of Child Dummy Services.
						VirtualPlatform[] virtualPlatforms = new VirtualPlatform[] { VirtualPlatform.TransmissionEurovision, VirtualPlatform.TransmissionFiber, VirtualPlatform.TransmissionIp, VirtualPlatform.TransmissionLiveU, VirtualPlatform.TransmissionMicrowave, VirtualPlatform.TransmissionSatellite, VirtualPlatform.TransmissionNone };
						return GetChildServicesForVirtualPlatform(virtualPlatforms);
					}

				case VirtualPlatformType.AudioProcessing:
					return GetChildServicesForVirtualPlatform(VirtualPlatform.AudioProcessing);
				case VirtualPlatformType.VideoProcessing:
					return GetChildServicesForVirtualPlatform(VirtualPlatform.VideoProcessing);
				case VirtualPlatformType.GraphicsProcessing:
					return GetChildServicesForVirtualPlatform(VirtualPlatform.GraphicsProcessing);
				default:
					throw new ArgumentException("Unknown Virtual Platform Type");
			}
		}

		/// <summary>
		/// Returns the child services for the given virtual platforms.
		/// </summary>
		/// <param name="virtualPlatforms">The virtual platforms.</param>
		/// <returns>The list of child services</returns>
		public List<Service> GetChildServicesForVirtualPlatform(params VirtualPlatform[] virtualPlatforms)
		{
			var services = new List<Service>();

			Action<ICollection<Service>> getMatchingChildren = null;
			getMatchingChildren = (ICollection<Service> children) =>
			{
				if (children == null || !children.Any())
				{
					return;
				}

				foreach (var child in children)
				{
					if (child.Definition != null && virtualPlatforms.Contains(child.Definition.VirtualPlatform))
					{
						services.Add(child);

						continue;
					}

					getMatchingChildren(child.Children);
				}
			};

			getMatchingChildren(Children);

			return services;
		}

		public bool TryGetParentFunctionWithinService(Function function, out Function parentFunction)
		{
			try
			{
				parentFunction = GetParentFunctionWithinService(function);
				return true;
			}
			catch
			{
				parentFunction = null;
				return false;
			}
		}

		public Function GetParentFunctionWithinService(Function function)
		{
			int functionPosition = Definition.GetFunctionPosition(function);

			if (functionPosition == 0) throw new ArgumentException("Function " + function.Name + " has no parent within service " + Name, nameof(function));

			int parentFunctionPosition = functionPosition - 1;

			return Functions.Single(f => Definition.GetFunctionPosition(f) == parentFunctionPosition);
		}

		public bool TryChangeServiceEndTime(Helpers helpers)
		{
			var reservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id) ?? throw new ReservationNotFoundException(Name);

			var bookingManager = new BookingManager((DataMiner.Automation.Engine)helpers.Engine, helpers.Engine.FindElement(Definition.BookingManagerElementName));

			var changeTimeInputData = new ChangeTimeInputData
			{
				StartDate = reservation.Start.Add(reservation.GetPreRoll()).FromReservation(),
				PreRoll = reservation.GetPreRoll(),
				EndDate = End,
				PostRoll = PostRoll,
				IsSilent = true
			};

			try
			{
				helpers.Log(nameof(Service), nameof(TryChangeServiceEndTime), $"Changing service end timing from {reservation.End.ToFullDetailString()} to {changeTimeInputData.EndDate.ToFullDetailString()}", Name);

				reservation = DataMinerInterface.BookingManager.ChangeTime(helpers, bookingManager, reservation, changeTimeInputData);
				return true;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Order), nameof(TryChangeServiceEndTime), $"Something went wrong while setting service end time from {reservation.End.ToFullDetailString()} to {changeTimeInputData.EndDate.ToFullDetailString()}: {e}", Name);
				return false;
			}
		}

		public bool TryGetChildFunctionWithinService(Function function, out Function childFunction)
		{
			try
			{
				childFunction = GetChildFunctionWithinService(function);
				return true;
			}
			catch
			{
				childFunction = null;
				return false;
			}
		}

		public Function GetChildFunctionWithinService(Function function)
		{
			int functionPosition = Definition.GetFunctionPosition(function);

			if (functionPosition == Functions.Count - 1) throw new ArgumentException("Function " + function.Name + " has no child service " + Name, nameof(function));

			int childFunctionPosition = functionPosition + 1;

			return Functions.Single(f => Definition.GetFunctionPosition(f) == childFunctionPosition);
		}

		/// <summary>
		/// Adds or updates the OrderIds property with the references of each order that uses this service.
		/// </summary>
		public void AddOrUpdateOrderIdsProperty(Helpers helpers)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			var serviceReservation = helpers.ServiceManager.GetReservation(Id);
			if (serviceReservation == null)
			{
				return;
			}

			if (OrderReferences == null || !OrderReferences.Any())
			{
				return;
			}

			TryUpdateCustomProperties(helpers, new Dictionary<string, object> { { ServicePropertyNames.OrderIdsPropertyName, string.Join(";", OrderReferences) } }, serviceReservation);
		}

		/// <summary>
		/// Resets Change Tracking.
		/// </summary>
		/// <see cref="IYleChangeTracking"/>
		public void AcceptChanges(Helpers helpers = null)
		{
			ChangeTrackingStarted = true;
			ChangeTrackingHelper.AcceptChanges(this, initialPropertyValues, helpers);
		}

		[JsonIgnore]
		public Change Change
		{
			get
			{
				if (!ChangeTrackingStarted) throw new InvalidOperationException($"Change Tracking has not been started for object {UniqueIdentifier}");

				var change = ChangeTrackingHelper.GetUpdatedChange(this, initialPropertyValues, new ServiceChange(Name, DisplayName)) as ServiceChange;

				change.UpdateSummaryWithServiceInfo(this);

				return change;
			}
		}

		[JsonIgnore]
		public string UniqueIdentifier => Name;

		[JsonIgnore]
		public string DisplayName { get; private set; }

		[JsonIgnore]
		public string SharedSourceDescription
		{
			get
			{
				if (!IsSharedSource) return String.Empty;
				return $"{Definition.VirtualPlatformServiceName.GetDescription()} {Definition.Description}";
			}
		}

		public Change GetChangeComparedTo<T>(Helpers helpers, T oldObjectInstance)
		{
			ServiceChange change = new ServiceChange(Name, DisplayName);

			if (object.Equals(oldObjectInstance, default(T)))
			{
				((ServiceChangeSummary)change.Summary).IsNew = true;

				return change;
			}
			else if (oldObjectInstance is Service oldService)
			{
				change = ChangeTrackingHelper.GetChangeComparedTo(this, oldService, change, helpers) as ServiceChange;

				change.UpdateSummaryWithServiceInfo(this);

				return change;
			}
			else throw new ArgumentException($"Argument is not of type {nameof(Service)}", nameof(oldObjectInstance));
		}

		/// <summary>
		/// Adjust the pre roll and start time of the service.
		/// </summary>
		/// <param name="helpers">helpers class which provides us with several logging, manager features to interact with Dataminer.</param>
		/// <param name="now">Current DateTime, to make sure the same compare value is used like on the order.</param>
		/// <param name="order">Linked order of this service.</param>
		public void CheckToAdjustStartTimes(Helpers helpers, DateTime now, Order order)
		{
			if (order.StartNow)
			{
				// If the Order should start as fast as possible, the service should start as soon as the user tasks are completed
				// These services should not have PreRoll
				PreRoll = TimeSpan.Zero;

				TimeSpan delay;
				bool shouldUseAddServicesToRunningOrderStartNowDelay = order.ConvertedFromRunningToStartNow;
				bool shouldUseFeenixStartNowDelay = order.IntegrationType == IntegrationType.Feenix && !shouldUseAddServicesToRunningOrderStartNowDelay;
				if (shouldUseAddServicesToRunningOrderStartNowDelay)
				{
					delay = TimeSpan.FromMinutes(Order.StartNowDelayInMinutesWhenAddingServicesToRunningOrder);
				}
				else if (shouldUseFeenixStartNowDelay)
				{
					delay = TimeSpan.FromMinutes(Order.StartNowDelayInMinutesForFeenix);
				}
				else
				{
					delay = TimeSpan.FromMinutes(Order.StartNowDelayInMinutes);
				}

				var nowWithDelay = now.Add(delay).RoundToMinutes();

				if (End <= nowWithDelay)
				{
					var oldNowWithDelay = nowWithDelay;
					nowWithDelay = End.Subtract(TimeSpan.FromMinutes(1));

					helpers.Log(nameof(Service), nameof(CheckToAdjustStartTimes), $"Trying to set start time from {Start.ToString("O")} to {oldNowWithDelay.ToString("O")}, but new value is later than end time {End.ToString("O")}, setting start to {nowWithDelay.ToString("O")}", Name);
				}
				else
				{
					helpers.Log(nameof(Service), nameof(CheckToAdjustStartTimes), $"Adjusting start time from {Start.ToString("O")} to {nowWithDelay.ToString("O")}", Name);
				}

				Start = nowWithDelay; // now with delay is meant to allow book services enough time to complete
			}
			else
			{
				var delay = TimeSpan.FromMinutes(Order.StartInTheFutureDelayInMinutes);
				var nowWithDelay = now.Add(delay).RoundToMinutes();

				bool startWithPreRollBeginsWithinTheDelayTimeSpan = StartWithPreRoll <= nowWithDelay && nowWithDelay < Start && Definition.VirtualPlatform != VirtualPlatform.ReceptionSatellite; // Preroll should be adjusted in case if it's not a satellite Rx due to automation rules
				bool adjustStartTime = Start <= nowWithDelay; // Preroll should be 0 and startTime should match the nowWithDelay. The StartNow will be called after the service is booked.

				if (startWithPreRollBeginsWithinTheDelayTimeSpan)
				{
					helpers?.Log(nameof(Service), nameof(CheckToAdjustStartTimes), $"Now with delay (={nowWithDelay}) falls between Start with preroll (={StartWithPreRoll}) and start (={Start}). Preroll is set to {Start.Subtract(nowWithDelay)}", Name);

					PreRoll = Start.Subtract(nowWithDelay);
				}
				else if (adjustStartTime)
				{
					helpers?.Log(nameof(Service), nameof(CheckToAdjustStartTimes), $"Now with delay (={nowWithDelay}) falls before Start (={Start}). Preroll is set to zero", Name);

					PreRoll = TimeSpan.Zero;
				}
				else
				{
					//Nothing to do
				}
			}
		}

		public void ApplyProfileConfiguration(Helpers helpers)
		{
			try
			{
				AutomationHandler.ApplyProfiles(helpers, this);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Service), nameof(ApplyProfileConfiguration), $"Something went wrong while applying the profile configuration for this service: {Name}: {e}");
			}
			finally
			{
				UpdateProfileConfigurationFailReasonProperty(helpers);
			}
		}

		public void UpdateFunctionsMainElementOrderNameProperty(Helpers helpers, string newPropertyValue)
		{
			foreach (var function in Functions)
			{
				if (!function.TryUpdateFunctionMainElementProperty(helpers, FunctionElementPropertyNames.EventName, newPropertyValue, OrderName))
				{
					helpers?.Log(nameof(Service), nameof(UpdateFunctionsMainElementOrderNameProperty), $"Update property: {FunctionElementPropertyNames.EventName} failed for function: {function.Name}");
				}
			}
		}

		public Dictionary<string, bool> AssignResourcesToFunctions(Helpers helpers, Order orderContainingService, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			var resourceAssignmentHandler = helpers.GetResourceAssignmentHandler(this, orderContainingService, overwrittenFunctionTimeRanges);

			resourceAssignmentHandler?.AssignResources();

			return resourceAssignmentHandler?.FunctionAssignments ?? Functions.ToDictionary(f => f.Definition.Label, f => true);
		}

		public bool AllCurrentlyAssignedResourcesAreAvailable(Helpers helpers, out List<FunctionResource> unavailableResources)
		{
			var availableResourcesPerFunction = GetAvailableResourcesPerFunctionBasedOnTiming(helpers);

			unavailableResources = new List<FunctionResource>();

			bool allSelectedResourcesAreAvailable = true;

			foreach (var function in Functions)
			{
				if (function.Resource is null) continue;

				bool resourceIsAvailable = availableResourcesPerFunction.TryGetValue(function.Definition.Label, out var availableResources) && availableResources.Contains(function.Resource);

				if (!resourceIsAvailable)
				{
					unavailableResources.Add(function.Resource);
				}

				allSelectedResourcesAreAvailable &= resourceIsAvailable;
			}

			return allSelectedResourcesAreAvailable;
		}

		public bool UsesOccupiedResources(Helpers helpers)
		{
			bool oneOrMoreResourcesAreOccupied = false;

			AllCurrentlyAssignedResourcesAreAvailable(helpers, out var unavailableResources);

			foreach (var function in Functions)
			{
				bool resourceForThisFunctionIsOccupied = unavailableResources.Contains(function.Resource);
				if (!resourceForThisFunctionIsOccupied) continue;

				var occupyingServices = helpers.ResourceManager.GetOccupyingServices(function.Resource, StartWithPreRoll, EndWithPostRoll, OrderReferences.First(), Name);

				function.Resource = new OccupiedResource(function.Resource) { OccupyingServices = occupyingServices };

				oneOrMoreResourcesAreOccupied = true;

				helpers.Log(nameof(Service), nameof(UsesOccupiedResources), $"Service {Name} function {function.Definition.Label} resource {function.ResourceName} is occupied over {TimingInfoToString(this)} by services {string.Join(", ", occupyingServices.Select(os => $"{os.Service.Name} part of order {os.Orders[0].Name}"))}");
			}

			return oneOrMoreResourcesAreOccupied;
		}

		public bool IsMissingResources(Helpers helpers)
		{
			foreach (var function in Functions)
			{
				if (function.RequiresResource && !function.IsDummy && function.Resource == null)
				{
					helpers?.Log(nameof(Service), nameof(IsMissingResources), $"Function {function.Name} is missing a resource, update required");
					return true;
				}
			}

			return false;
		}

		public bool IsRecordingAfterDestination(Order order)
		{
			if (Definition.VirtualPlatform != VirtualPlatform.Recording) return false;

			var parent = order.AllServices.Single(s => s.Children.Contains(this));

			return parent.Definition.VirtualPlatform == VirtualPlatform.Destination;
		}

		/// <summary>
		/// Gets a collection of tasks to update the reservationInstance based on ChangeTracked changes.
		/// </summary>
		/// <param name="helpers">Helpers class</param>
		/// <param name="oldService">Old service to compare with.</param>
		/// <param name="orderContainingService">Actual order which is containing this service.</param>
		/// <returns>A collection of tasks to update the reservationInstance.</returns>
		/// <exception cref="ArgumentNullException"/>
		public IEnumerable<Task> GetUpdateTasks(Helpers helpers, Order orderContainingService, Service oldService)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (orderContainingService == null) throw new ArgumentNullException(nameof(orderContainingService));

			var tasks = new List<Task>();

			helpers.Log(nameof(Service), nameof(GetUpdateTasks), $"Change tracking is {(ChangeTrackingStarted ? "enabled" : "disabled")}");

			var serviceChange = ChangeTrackingStarted ? Change as ServiceChange : GetChangeComparedTo(null, oldService) as ServiceChange;

			return GetUpdateTasks(helpers, orderContainingService, oldService, serviceChange);
		}

		private List<Task> GetUpdateTasks(Helpers helpers, Order orderContainingService, Service oldService, ServiceChange serviceChangeInfo)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (orderContainingService == null) throw new ArgumentNullException(nameof(orderContainingService));
			if (serviceChangeInfo == null) throw new ArgumentNullException(nameof(serviceChangeInfo));

			helpers.LogMethodStart(nameof(Service), nameof(GetUpdateTasks), out var stopwatch, Name);

			helpers.Log(nameof(OrderUpdateHandler), nameof(GetUpdateTasks), $"Service: {Name}, EVS ID: {EvsId}");

			var tasks = new List<Task>();

			var now = DateTime.UtcNow;
			bool serviceIsNew = !IsBooked || oldService == null;
			bool oldServiceShouldBeRunning = oldService != null && (oldService.StartWithPreRoll.ToUniversalTime() <= now && now <= oldService.EndWithPostRoll.ToUniversalTime());
			bool oldServiceHasEnded = oldService != null && (oldService.EndWithPostRoll.ToUniversalTime() < now);

			var serviceChangeSummary = (ServiceChangeSummary)serviceChangeInfo.Summary;

			bool newOrChangedBeforeLive = serviceIsNew || (serviceChangeSummary.IsChanged && !oldServiceShouldBeRunning && !oldServiceHasEnded);

			tasks.AddRange(GetEvsUpdateTasks(helpers, orderContainingService, oldService, serviceChangeInfo));

			if (Definition.VirtualPlatform == VirtualPlatform.Routing && serviceChangeSummary.OnlyResourcesHaveChanged)
			{
				// Routing resources are being cleared before routing configuration is (re)evaluated. This will always cause a resource update to be necessary. For performance reasons we avoid an EditBooking call in this specific case.

				tasks.Add(new UpdateResourcesTask(helpers, this, Functions));

				helpers.Log(nameof(Service), nameof(GetUpdateTasks), "Added task to re-update the routing resources after clearing them", Name);
			}
			else if (newOrChangedBeforeLive)
			{
				tasks.AddRange(GetUpdateTasksBeforeLive(helpers, orderContainingService, oldService, serviceChangeInfo));
			}
			else if (oldServiceShouldBeRunning)
			{
				tasks.AddRange(GetUpdateTasksDuringLive(helpers, oldService, serviceChangeInfo, orderContainingService));
			}
			else if (oldServiceHasEnded)
			{
				tasks.AddRange(GetUpdateTasksAfterLive(helpers, oldService, serviceChangeInfo, orderContainingService));
			}
			else
			{
				helpers.Log(nameof(Service), nameof(GetUpdateTasks), "Unknown use case", Name);
			}

			if (tasks.Any())
			{
				tasks.Add(new VerifyServiceTask(helpers, this, orderContainingService));
				tasks.Add(new ClearMajorTimeslotFlagTask(helpers, this));
				helpers.Log(nameof(Service), nameof(GetUpdateTasks), "Added task to verify service", Name);
			}

			helpers.LogMethodCompleted(nameof(Service), nameof(GetUpdateTasks), Name, stopwatch);

			return tasks;
		}

		private List<Task> GetUpdateTasksAfterLive(Helpers helpers, Service oldService, ServiceChange serviceChangeInfo, Order orderContainingService)
		{
			var tasks = new List<Task>();

			helpers.Log(nameof(Service), nameof(GetUpdateTasks), "Service has ended, only custom property updates allowed", Name);

			var serviceChangeSummary = serviceChangeInfo.Summary as ServiceChangeSummary;

			if (serviceChangeSummary.PropertyChangeSummary.IsChanged)
			{
				tasks.Add(new UpdateCustomPropertiesTask(helpers, this, oldService, orderContainingService));
				helpers.Log(nameof(Service), nameof(GetUpdateTasks), "Added task to update custom properties", Name);
			}

			return tasks;
		}

		private List<Task> GetEvsUpdateTasks(Helpers helpers, Order orderContainingService, Service oldService, ServiceChange serviceChangeInfo)
		{
			var tasks = new List<Task>();

			if (Definition.VirtualPlatformServiceType != VirtualPlatformType.Recording)
			{
				helpers.Log(nameof(Service), nameof(GetEvsUpdateTasks), "Service is not a recording", Name);
				return tasks;
			}

			if (!helpers.OrderManagerElement.IsDeviceAutomationEnabled(this))
			{
				helpers.Log(nameof(Service), nameof(GetEvsUpdateTasks), "EVS automation is disabled", Name);
				return tasks;
			}

			if (MajorTimeslotChange || IsEvsUpdateRequired(oldService, serviceChangeInfo))
			{
				helpers.Log(nameof(Service), nameof(GetEvsUpdateTasks), "Service meets requirements for registration in EVS", Name);

				OrderName = orderContainingService.Name;

				tasks.Add(new AddOrUpdateInEvsTask(helpers, this));
			}
			else
			{
				helpers.Log(nameof(Service), nameof(GetEvsUpdateTasks), "Service does not meet conditions for registration in EVS", Name);
			}

			return tasks;
		}

		private bool IsEvsUpdateRequired(Service oldService, ServiceChange serviceChangeInfo)
		{
			if (!IsBooked || oldService == null) return true; // New Service
			if (!String.Equals(oldService.OrderName, OrderName)) return true; // Order name changed
			if (HasRecordingConfigurationChanges(serviceChangeInfo)) return true;

			var serviceChangeSummary = serviceChangeInfo.Summary as ServiceChangeSummary;
			if (serviceChangeSummary.TimingChangeSummary.IsChanged) return true; // Timing changes
			if (serviceChangeSummary.FunctionChangeSummary.IsChanged) return true; // Function changes

			return false;
		}

		private bool HasRecordingConfigurationChanges(ServiceChange serviceChangeInfo)
		{
			ClassChange recordingConfigurationChange = serviceChangeInfo.ClassChanges.FirstOrDefault(x => x.ClassName.Equals(nameof(RecordingConfiguration)));

			if (recordingConfigurationChange == null) return false;

			// Fast Areena Copy, Plasma ID, RecordingName, FastRerunCopy, SubtitleProxy
			bool fastAreenaCopyChanged = recordingConfigurationChange.PropertyChanges.FirstOrDefault(x => x.PropertyName.Equals(nameof(RecordingConfiguration.FastAreenaCopy)))?.Summary.IsChanged ?? false;
			bool plasmaIdChanged = recordingConfigurationChange.PropertyChanges.FirstOrDefault(x => x.PropertyName.Equals(nameof(RecordingConfiguration.PlasmaIdForArchive)))?.Summary.IsChanged ?? false;
			bool recordingNameChanged = recordingConfigurationChange.PropertyChanges.FirstOrDefault(x => x.PropertyName.Equals(nameof(RecordingConfiguration.RecordingName)))?.Summary.IsChanged ?? false;
			bool fastRerunCopyChanged = recordingConfigurationChange.PropertyChanges.FirstOrDefault(x => x.PropertyName.Equals(nameof(RecordingConfiguration.FastRerunCopy)))?.Summary.IsChanged ?? false;
			bool subtitleProxyChanged = recordingConfigurationChange.PropertyChanges.FirstOrDefault(x => x.PropertyName.Equals(nameof(RecordingConfiguration.SubtitleProxy)))?.Summary.IsChanged ?? false;
			bool messiNewsTargetChanged = recordingConfigurationChange.PropertyChanges.FirstOrDefault(x => x.PropertyName.Equals(nameof(RecordingConfiguration.EvsMessiNewsTarget)))?.Summary.IsChanged ?? false;

			return fastAreenaCopyChanged || plasmaIdChanged || recordingNameChanged || fastRerunCopyChanged || subtitleProxyChanged || messiNewsTargetChanged;
		}

		private List<Task> GetUpdateTasksBeforeLive(Helpers helpers, Order orderContainingService, Service oldService, ServiceChange serviceChangeInfo)
		{
			var tasks = new List<Task>();

			bool serviceIsNew = !IsBooked || oldService == null;

			var serviceChangeSummary = serviceChangeInfo.Summary as ServiceChangeSummary;

			if (serviceIsNew || serviceChangeSummary.IsMissingResources || serviceChangeSummary.FunctionChangeSummary.ProfileParameterChangeSummary.IsChanged || serviceChangeSummary.TimingChangeSummary.IsChanged)
			{
				Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null;

				var hasFunctionsWithoutEnforcedResource = Functions.Any(f => !f.EnforceSelectedResource);
				if (hasFunctionsWithoutEnforcedResource)
				{
					overwrittenFunctionTimeRanges = new Dictionary<string, TimeRangeUtc>();

					foreach (var function in Functions)
					{
						if (function.EnforceSelectedResource || (orderContainingService.SourceService.LastResourceRequiringFunction.Resource?.HasPrioritizedResourcesDefined() ?? false))
						{
							overwrittenFunctionTimeRanges.Add(function.Definition.Label, new TimeRangeUtc(StartWithPreRoll.ToUniversalTime(), EndWithPostRoll.ToUniversalTime(), TimeZoneInfo.Utc));
						}
						else
						{
							var overwrittenTimeRange = helpers.ServiceManager.GetOverwrittenTimeRange(this);

							overwrittenFunctionTimeRanges.Add(function.Definition.Label, new TimeRangeUtc(overwrittenTimeRange, TimeZoneInfo.Utc));
						}
					}
				}

				tasks.Add(new AssignResourcesToFunctionsTask(helpers, this, orderContainingService, overwrittenFunctionTimeRanges));
				helpers.Log(nameof(Service), nameof(GetUpdateTasksBeforeLive), "Added task to assign resources to functions of service", Name);
			}

			tasks.Add(new AddOrUpdateServiceTask(helpers, this, oldService, orderContainingService));
			helpers.Log(nameof(Service), nameof(GetUpdateTasksBeforeLive), "Added task for full add or update of service", Name);

			return tasks;
		}

		private List<Task> GetUpdateTasksDuringLive(Helpers helpers, Service oldService, History.ServiceChange serviceChangeInfo, Order orderContainingService)
		{
			// Because of timing restrictions in UI scripts, the flow will only enter this else-clause after using UpdateService or UpdateELRs scripts when the service is live.
			// This means DTR and Resource assignment is already done in the UI itself, no need to re-check or re-assign here.

			helpers.Log(nameof(Service), nameof(GetUpdateTasksDuringLive), "Service should be running. (Considering resource assignment to be already done)", Name);

			var tasks = new List<Task>();

			var functionsForWhichToUpdateResourcesAndProfileParameters = new List<Function>();
			var functionsForWhichToUpdateResources = new List<Function>();
			var functionsForWhichToUpdateOnlyProfileParameters = new List<Function>();

			helpers.Log(nameof(Service), nameof(GetUpdateTasksDuringLive), $"Service functions: {string.Join(";", Functions.Select(x => $"Id: {x.Id} | Label: {x.Definition.Label}"))}", Name);

			if (Definition.VirtualPlatform == VirtualPlatform.Routing)
			{
				// routing resources are cleared at the start of book services flow!!
				// They should always be reassigned here

				functionsForWhichToUpdateResourcesAndProfileParameters = Functions;

				foreach (var childService in OrderManager.FlattenServices(Children))
				{
					tasks.Add(new UpdateMcrDescriptionTask(helpers, childService, orderContainingService));
					helpers.Log(nameof(Service), nameof(GetUpdateTasks), Name + $"|Added tasks to update MCR description of child service {childService.Name}");
				}
			}
			else
			{
				AddFunctionsToRelevantCollections(serviceChangeInfo, functionsForWhichToUpdateResourcesAndProfileParameters, functionsForWhichToUpdateResources, functionsForWhichToUpdateOnlyProfileParameters);
			}

			if (functionsForWhichToUpdateResourcesAndProfileParameters.Any())
			{
				tasks.Add(new UpdateResourcesAndProfileParametersTask(helpers, this, functionsForWhichToUpdateResourcesAndProfileParameters));

				helpers.Log(nameof(Service), nameof(GetUpdateTasksDuringLive), $"Added task to update resources and profile parameters for {string.Join(",", functionsForWhichToUpdateResourcesAndProfileParameters.Select(f => f.Name))}", Name);
			}

			if (functionsForWhichToUpdateResources.Any())
			{
				tasks.Add(new UpdateResourcesTask(helpers, this, functionsForWhichToUpdateResources));

				helpers.Log(nameof(Service), nameof(GetUpdateTasksDuringLive), $"Added task to update resources for {string.Join(",", functionsForWhichToUpdateResources.Select(f => f.Name))}", Name);
			}

			if (functionsForWhichToUpdateOnlyProfileParameters.Any())
			{
				tasks.Add(new UpdateProfileParametersTask(helpers, this, functionsForWhichToUpdateOnlyProfileParameters));

				helpers.Log(nameof(Service), nameof(GetUpdateTasksDuringLive), $"Added task to update profile parameters for {string.Join(",", functionsForWhichToUpdateOnlyProfileParameters.Select(f => f.Name))}", Name);
			}

			var serviceChangeSummary = serviceChangeInfo.Summary as ServiceChangeSummary;
			if (serviceChangeSummary.TimingChangeSummary.IsChanged)
			{
				tasks.Add(new ChangeServiceTimeTask(helpers, this));

				helpers.Log(nameof(Service), nameof(GetUpdateTasksDuringLive), "Added task to change timing", Name);
			}

			if (serviceChangeSummary.PropertyChangeSummary.IsChanged)
			{
				tasks.Add(new UpdateCustomPropertiesTask(helpers, this, oldService, orderContainingService));

				helpers.Log(nameof(Service), nameof(GetUpdateTasksDuringLive), "Added task to update custom properties", Name);
			}

			if (serviceChangeSummary.SecurityViewIdsHaveChanged)
			{
				tasks.Add(new UpdateSecurityViewIdsTask(helpers, this));

				helpers.Log(nameof(Service), nameof(GetUpdateTasksDuringLive), "Added task to update security view ids", Name);
			}

			return tasks;
		}

		private void AddFunctionsToRelevantCollections(ServiceChange serviceChangeInfo, List<Function> functionsForWhichToUpdateResourcesAndProfileParameters, List<Function> functionsForWhichToUpdateResources, List<Function> functionsForWhichToUpdateOnlyProfileParameters)
		{
			foreach (var functionChangeInfo in serviceChangeInfo.FunctionChanges)
			{
				var functionChangeSummary = functionChangeInfo.Summary as FunctionChangeSummary;

				var function = Functions.SingleOrDefault(f => f.Definition.Label == functionChangeInfo.FunctionLabel) ?? throw new FunctionNotFoundException(functionChangeInfo.FunctionLabel);

				if (functionChangeSummary.ProfileParameterChangeSummary.IsChanged && functionChangeSummary.ResourceChangeSummary.IsChanged)
				{
					functionsForWhichToUpdateResourcesAndProfileParameters.Add(function);
				}
				else if (functionChangeSummary.ResourceChangeSummary.IsChanged)
				{
					functionsForWhichToUpdateResources.Add(function);
				}
				else if (functionChangeSummary.ProfileParameterChangeSummary.IsChanged)
				{
					functionsForWhichToUpdateOnlyProfileParameters.Add(function);
				}
				else
				{
					// No action needed
				}
			}
		}

		/// <summary>
		/// Verify if the service is correctly created and all details are correct.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="orderContainingService">The order where the service is included in.</param>
		public void Verify(Helpers helpers, Order orderContainingService)
		{
			helpers.LogMethodStart(nameof(Service), nameof(Verify), out var stopwatch);

			ReservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id) as ServiceReservationInstance ?? throw new ReservationNotFoundException(Id);

			VerifyContributingResourceCreation(helpers);

			helpers.LogMethodCompleted(nameof(Service), nameof(Verify), null, stopwatch);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Service other)) return false;

			return Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public void UpdateSecurityViewIds(Helpers helpers, IEnumerable<int> securityViewIds)
		{
			if (!IsBooked || IsSharedSource) return;

			var reservation = ReservationInstance ?? DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id);

			if (reservation.SecurityViewIDs.OrderBy(x => x).SequenceEqual(securityViewIds.OrderBy(x => x)))
			{
				helpers.Log(nameof(Service), nameof(UpdateSecurityViewIds), $"Security View IDs on reservation are already {string.Join(",", securityViewIds)}", reservation.Name);
				return;
			}

			SecurityViewIds = new HashSet<int>(securityViewIds);

			ReservationInstance = helpers.ReservationManager.UpdateSecurityViewIds(reservation, securityViewIds) as ServiceReservationInstance;
		}

		/// <summary>
		/// Get the service for a give reservation instance id.
		/// </summary>
		/// <param name="helpers">helpers class.</param>
		/// <param name="reservationInstanceId">The reservation instance id.</param>
		/// <returns>The service object.</returns>
		public static Service FromReservationInstance(Helpers helpers, Guid reservationInstanceId)
		{
			var reservationInstance = helpers.ServiceManager.GetReservation(reservationInstanceId);
			return FromReservationInstance(helpers, reservationInstance);
		}

		/// <summary>
		/// Get the service for a give reservation instance.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="reservationInstance">The reservation instance.</param>
		/// <returns>The service object.</returns>
		public static DisplayedService FromReservationInstance(Helpers helpers, ReservationInstance reservationInstance)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (reservationInstance == null) throw new ArgumentNullException(nameof(reservationInstance));

			helpers.LogMethodStart(nameof(Service), nameof(FromReservationInstance), out var stopwatch, reservationInstance.Name);

			var booking = reservationInstance.GetBookingData();

			var service = new DisplayedService(booking.Description)
			{
				Id = reservationInstance.ID,
			};

			var convertedStartTime = reservationInstance.Start.FromReservation().Truncate(TimeSpan.FromMinutes(1));
			var convertedEndTime = reservationInstance.End.FromReservation().Truncate(TimeSpan.FromMinutes(1));

			service.PreRoll = reservationInstance.GetPreRoll();
			service.PostRoll = reservationInstance.GetPostRoll();
			service.Start = convertedStartTime.Add(service.PreRoll);
			service.End = convertedEndTime.Subtract(service.PostRoll);

			service.IsBooked = true;
			service.BackupType = reservationInstance.GetServiceLevel();
			service.IntegrationType = reservationInstance.GetIntegrationType();
			service.IntegrationIsMaster = reservationInstance.GetBooleanProperty(ServicePropertyNames.IntegrationIsMasterPropertyName);
			service.IsSharedSource = reservationInstance.GetBooleanProperty(ServicePropertyNames.IsGlobalEventLevelReceptionPropertyName);
			if (service.IsSharedSource) service.VerifyContributingResourceCreation(helpers);
			service.HasResourcesAssigned = reservationInstance.ResourcesInReservationInstance.Any();
			service.OrderReferences = reservationInstance.GetOrderReferences();
			service.SecurityViewIds = new HashSet<int>(reservationInstance.SecurityViewIDs);

			var serviceResourceReservationInstance = reservationInstance as ServiceReservationInstance;
			if (serviceResourceReservationInstance == null) throw new ArgumentException($"Unable to cast to {nameof(ServiceReservationInstance)}", nameof(reservationInstance));

			var serviceDefinition = helpers.ServiceDefinitionManager.GetServiceDefinition(serviceResourceReservationInstance.ServiceDefinitionID);
			service.Definition = serviceDefinition;

			service.Functions = helpers.ServiceManager.GetFunctions(serviceResourceReservationInstance, serviceDefinition);

			service.LinkedServiceId = reservationInstance.GetLinkedServiceId();
			service.AudioChannelConfiguration = InitializeAudioChannelConfiguration(service.Functions, serviceDefinition.VirtualPlatformServiceType == VirtualPlatformType.Reception);

			service.EurovisionWorkOrderId = reservationInstance.GetStringProperty(ServicePropertyNames.EurovisionIdPropertyName);
			service.EurovisionTransmissionNumber = reservationInstance.GetStringProperty(ServicePropertyNames.EurovisionTransmissionNumberPropertyName);
			service.EurovisionBookingDetails = reservationInstance.GetEurovisionBookingDetails();
			if (!string.IsNullOrEmpty(service.EurovisionWorkOrderId) || service.EurovisionBookingDetails != null)
				service.IsEurovisionService = true;

			service.ContactInformationName = reservationInstance.GetStringProperty(ServicePropertyNames.ContactInformationNamePropertyName);
			service.ContactInformationTelephoneNumber = reservationInstance.GetStringProperty(ServicePropertyNames.ContactInformationTelephoneNumberPropertyName);
			service.VidigoStreamSourceLink = reservationInstance.GetStringProperty(ServicePropertyNames.VidigoStreamSourceLinkPropertyName);
			service.HasAnIssueBeenreportedManually = reservationInstance.GetBooleanProperty(ServicePropertyNames.ReportedIssuePropertyName);

			if (serviceDefinition.VirtualPlatformServiceName == VirtualPlatformName.LiveU)
			{
				service.LiveUDeviceName = reservationInstance.GetStringProperty(ServicePropertyNames.LiveUDeviceNamePropertyName);
			}

			service.AudioReturnInfo = reservationInstance.GetStringProperty(ServicePropertyNames.AudioReturnInfoPropertyName);

			service.RecordingConfiguration = reservationInstance.GetRecordingConfiguration();
			service.NameOfServiceToTransmit = reservationInstance.GetStringProperty(ServicePropertyNames.NameOfServiceToTransmitPropertyName);

			service.UserTasks = helpers.UserTaskManager.GetUserTasks(service).ToList();

			service.SavedStatus = reservationInstance.GetServiceStatus();
			service.isCancelled = service.SavedStatus == Status.Cancelled;

			service.Comments = reservationInstance.GetStringProperty(ServicePropertyNames.CommentsPropertyName);
			service.OrderName = reservationInstance.GetStringProperty(ServicePropertyNames.OrderNamePropertyName);
			service.ProfileConfigurationFailReason = reservationInstance.GetStringProperty(ServicePropertyNames.ProfileConfigurationFailReason);

			service.EvsId = reservationInstance.GetStringProperty(ServicePropertyNames.EvsIdPropertyName);

			service.ReservationInstance = serviceResourceReservationInstance;

			helpers.Log(nameof(Service), nameof(FromReservationInstance), $"Summary of properties on service object taken from reservation instance: ID={service.Id} IsSharedSource={service.IsSharedSource}, SecurityViewIds={string.Join(",", service.SecurityViewIds)}, RequiresRouting={service.RequiresRouting}, HasIssueBeenReportedManually={service.HasAnIssueBeenreportedManually}, NameOfServiceToTransmitOrRecord={service.NameOfServiceToTransmitOrRecord}, IntegrationIsMaster={service.IntegrationIsMaster}, Timing={TimingInfoToString(service)}", service.Name);

			helpers.LogMethodCompleted(nameof(Service), nameof(FromReservationInstance), reservationInstance.Name, stopwatch);

			return service;
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

		public List<Service> GetOtherServicesWithSameVirtualPlatform(Helpers helpers, List<ServiceDefinition> otherServiceDefinitions)
		{
			helpers.LogMethodStart(nameof(Service), nameof(GetOtherServicesWithSameVirtualPlatform), out var stopwatch, Name);

			var otherServiceDefinitionsSameVirtualPlatform = otherServiceDefinitions.Where(x => x.VirtualPlatform == Definition.VirtualPlatform && x.Description != Definition.Description && !string.IsNullOrEmpty(x.Description));

			var newServicesWithSameVirtualPlatform = new List<Service>();
			foreach (var otherNewServiceDefinition in otherServiceDefinitionsSameVirtualPlatform)
			{
				var newService = CreateNewServiceBasedOnServiceDefinition(helpers, this, otherNewServiceDefinition);
				newService.OrderReferences = new HashSet<Guid>(OrderReferences);

				newServicesWithSameVirtualPlatform.Add(newService);
			}

			helpers.LogMethodCompleted(nameof(Service), nameof(GetOtherServicesWithSameVirtualPlatform), Name, stopwatch);

			return newServicesWithSameVirtualPlatform;
		}

		public static Service CreateNewServiceBasedOnServiceDefinition(Helpers helpers, Service existingService, ServiceDefinition newServiceDefinition)
		{
			if (existingService == null) throw new ArgumentNullException(nameof(existingService));
			if (newServiceDefinition == null) throw new ArgumentNullException(nameof(newServiceDefinition));

			var configurationForNewService = existingService.GetConfiguration().Copy();
			configurationForNewService.Id = Guid.NewGuid();
			configurationForNewService.Name = $"{EnumExtensions.GetDescriptionFromEnumValue(newServiceDefinition.VirtualPlatformServiceType)} [{configurationForNewService.Id}]";
			configurationForNewService.ServiceDefinitionId = newServiceDefinition.Id;
			configurationForNewService.Functions = new Dictionary<string, FunctionConfiguration>();

			foreach (var newFunctionDefinition in newServiceDefinition.FunctionDefinitions.Where(f => f != null))
			{
				if (newFunctionDefinition.ProfileDefinition != null)
				{
					var newFunctionConfiguration = new FunctionConfiguration { Id = newFunctionDefinition.Id, Name = newFunctionDefinition.Name, ResourceName = string.Empty, ResourceId = Guid.Empty, ProfileParameters = new Dictionary<Guid, object>(), RequiresResource = true, ConfiguredByMcr = false, McrHasOverruledFixedTieLineLogic = false };

					configurationForNewService.Functions[newFunctionDefinition.Label] = newFunctionConfiguration;
				}
			}

			return new DisplayedService(helpers, existingService.NodeId, existingService.NodeLabel, configurationForNewService);
		}

		public static void CopyProfileParameterValuesFromPreviousToNewFunction(Service existingSourceService, Service serviceThatNeedsNewSection)
		{
			var previousFunctionProfileParameters = existingSourceService.Functions.SelectMany(f => f.Parameters).ToList();

			foreach (var newFunction in serviceThatNeedsNewSection.Functions)
			{
				if (newFunction != null)
				{
					var allNewFunctionProfileParameters = newFunction.InterfaceParameters.Concat(newFunction.Parameters);

					for (int i = 0; i < previousFunctionProfileParameters.Count; i++)
					{
						var matchingProfileParameter = allNewFunctionProfileParameters.SingleOrDefault(p => p != null && previousFunctionProfileParameters[i]?.Id == p.Id);
						if (matchingProfileParameter == null) continue;

						switch (previousFunctionProfileParameters[i].Type)
						{
							case Library.Solutions.SRM.Model.ParameterType.Number:
								matchingProfileParameter.Value = previousFunctionProfileParameters[i].Value;
								break;
							case Library.Solutions.SRM.Model.ParameterType.Discrete:
								matchingProfileParameter.Discreets = previousFunctionProfileParameters[i].Discreets.ToList();
								matchingProfileParameter.DefaultValue = previousFunctionProfileParameters[i].DefaultValue;
								matchingProfileParameter.Value = previousFunctionProfileParameters[i].Value;
								break;
							case Library.Solutions.SRM.Model.ParameterType.Text:
								matchingProfileParameter.Value = previousFunctionProfileParameters[i].Value;
								break;
							default:
								break;
						}
					}
				}
			}
		}

		protected static AudioChannelConfiguration InitializeAudioChannelConfiguration(IEnumerable<Function> functions, bool isReception)
		{
			HashSet<ProfileParameter> audioChannelProfileParameters = new HashSet<ProfileParameter>();
			foreach (Function function in functions)
			{
				foreach (ProfileParameter parameter in function.Parameters)
				{
					if (parameter == null) continue;

					if (ProfileParameterGuids.AllAudioChannelConfigurationGuids.Contains(parameter.Id))
					{
						audioChannelProfileParameters.Add(parameter);
					}
				}
			}

			return new AudioChannelConfiguration(isReception, audioChannelProfileParameters);
		}

		/// <summary>
		/// Retrieves all orders that are linked with the service through the OrderReferences property.
		/// If an order would not exist or does not use this service, the Id of the order will get removed from the reservationInstance property.
		/// </summary>
		/// <returns></returns>
		public bool TryGetLinkedOrders(Helpers helpers, out List<Order> orders)
		{
			orders = new List<Order>();
			List<Guid> invalidLinkedOrderIds = new List<Guid>();

			bool success = true;
			foreach (Guid orderReference in OrderReferences)
			{
				var getOrderTask = new GetOrderTask(helpers, orderReference);

				bool wasTaskSuccessful = getOrderTask.Execute();
				if (IsSharedSource)
				{
					// Check if the list of linked order ids got corrupt somehow
					if (!success && getOrderTask.Exception is ReservationNotFoundException)
					{
						// This could happen because the order was removed, but the link was not removed from the Shared Source
						helpers.Log(nameof(Service), nameof(TryGetLinkedOrders), $"Order reservation not found, removing Order ID {orderReference} from the entries.");
						invalidLinkedOrderIds.Add(orderReference);
						continue;
					}

					if (success && getOrderTask.Order != null && !getOrderTask.Order.AllServices.Exists(x => x.Id.Equals(Id)))
					{
						// This could happen because the Shared Source got replaced but the order id was not removed from the list
						helpers.Log(nameof(Service), nameof(TryGetLinkedOrders), $"Linked Order does not use the Shared Source, removing Order ID {orderReference} from the entries.");
						invalidLinkedOrderIds.Add(orderReference);
						continue;
					}
				}

				success &= wasTaskSuccessful;
				orders.Add(getOrderTask.Order);
			}

			// If any invalid order references -> remove them from the list and update the reservationInstance
			if (invalidLinkedOrderIds.Any())
			{
				helpers.Log(nameof(Service), nameof(TryGetLinkedOrders), $"Removing the following invalid linked order IDs from Shared Source: {String.Join(", ", invalidLinkedOrderIds)}");
				OrderReferences.RemoveWhere(x => invalidLinkedOrderIds.Contains(x));
				UpdateOrderReferencesProperty(helpers);
			}

			return success;
		}

		private void UpdateFunctionsBasedOnReservation(Helpers helpers, ServiceReservationInstance reservationInstance)
		{
			helpers.LogMethodStart(nameof(Service), nameof(UpdateFunctionsBasedOnReservation), out var stopwatch, Name);

			var functionsInReservation = reservationInstance.GetFunctionData();

			foreach (var function in Functions)
			{
				if (function == null) continue;

				var functionInReservation = functionsInReservation.FirstOrDefault(f => f.Id == function.NodeId);
				var resourceUsageDefinition = reservationInstance.ResourcesInReservationInstance.FirstOrDefault(r => ((ServiceResourceUsageDefinition)r)?.ServiceDefinitionNodeID == function.NodeId);

				function.UpdateValuesBasedOnReservation(helpers, function.Definition, functionInReservation, resourceUsageDefinition);

				helpers.Log(nameof(Service), nameof(UpdateFunctionsBasedOnReservation), $"Function Summary: {function.Configuration}");
			}

			helpers.LogMethodCompleted(nameof(Service), nameof(UpdateFunctionsBasedOnReservation), Name, stopwatch);
		}

		public void UpdateAudioChannelConfiguration()
		{
			AudioChannelConfiguration = InitializeAudioChannelConfiguration(Functions, Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception);
		}

		public void UpdateOrderReferencesProperty(Helpers helpers)
		{
			var serviceReservation = helpers.ServiceManager.GetReservation(Id);
			UpdateOrderReferencesProperty(helpers, serviceReservation, OrderReferences);
		}

		public void UpdateOrderReferencesProperty(Helpers helpers, Net.ResourceManager.Objects.ReservationInstance serviceReservation)
		{
			serviceReservation = serviceReservation ?? helpers.ServiceManager.GetReservation(Id) ?? throw new ReservationNotFoundException(Id);
			UpdateOrderReferencesProperty(helpers, serviceReservation, OrderReferences);
		}

		public static void UpdateOrderReferencesProperty(Helpers helpers, Net.ResourceManager.Objects.ReservationInstance serviceReservation, IEnumerable<Guid> orderReferences)
		{
			helpers.Log(nameof(Service), nameof(UpdateOrderReferencesProperty), $"Updating Order references property to '{String.Join(";", orderReferences)}'", serviceReservation?.Name);
			TryUpdateCustomProperties(helpers, new Dictionary<string, object> { { ServicePropertyNames.OrderIdsPropertyName, String.Join(";", orderReferences.Distinct()) } }, serviceReservation);
		}

		public Booking GetBookingDataForBooking(Helpers helpers)
		{
			var bookingData = new Library.Solutions.SRM.Model.Booking
			{
				ConfigureResources = true,
				Description = Name,
				DesiredReservationStatus = DesiredReservationStatus.Confirmed, // Status will always go to Pending when ConvertToContributing == true
				Recurrence = new Library.Solutions.SRM.Model.Recurrence
				{
					StartDate = Start,
					EndDate = End,
					SingleEvent = true,
					PreRoll = PreRoll,
					PostRoll = PostRoll,
				},
				ServiceDefinition = Definition.Id.ToString(),
				Type = BookingType.SingleEvent,
				ConvertToContributing = true,
				ExternalServiceManagement = true
			};

			helpers.Log(nameof(Service), nameof(GetBookingDataForBooking), $"Booking data: PreRoll={bookingData.Recurrence.PreRoll}, Start={bookingData.Recurrence.StartDate}, End={bookingData.Recurrence.EndDate}, PostRoll={bookingData.Recurrence.PostRoll}", Name);

			return bookingData;
		}

		public IEnumerable<Library.Solutions.SRM.Model.Function> GetFunctionsForBooking(Helpers helpers, bool checkResourceAvailability = false)
		{
			var functions = new List<Library.Solutions.SRM.Model.Function>();
			if (Functions == null) return functions;

			foreach (var function in Functions)
			{
				var srmFunction = new Library.Solutions.SRM.Model.Function
				{
					Name = function.Definition.Label,
					ByReference = false,
					Id = function.NodeId,
					ShouldAutoSelectResource = false,
					SkipResourceValidation = true,
					Parameters = function.Parameters.Select(p => p.GetParameterForBooking()).ToList(),
					InputInterfaces = function.InputInterfaces.Select(ii => ii.GetInterfaceForBooking()).ToList(),
					OutputInterfaces = function.OutputInterfaces.Select(ii => ii.GetInterfaceForBooking()).ToList(),
				};

				functions.Add(srmFunction);

				if (checkResourceAvailability && function.Resource != null)
				{
					CheckResourceAvailabilityBeforeBooking(helpers, function, srmFunction);
				}
				else
				{
					srmFunction.SelectedResource = function.Resource != null ? function.Resource.ID.ToString() : Guid.Empty.ToString();
				}
			}

			return functions;
		}

		private void CheckResourceAvailabilityBeforeBooking(Helpers helpers, Function function, Library.Solutions.SRM.Model.Function srmFunction)
		{
			var availableResourcesPerFunction = GetAvailableResourcesPerFunctionBasedOnTiming(helpers);

			helpers.Log(nameof(Service), nameof(GetFunctionsForBooking), $"Checking availability for function {function.Definition.Label} resource {function.Resource?.Name}", Name);

			bool selectedResourceIsAvailable;
			if (availableResourcesPerFunction.TryGetValue(function.Definition.Label, out var availableResources))
			{
				selectedResourceIsAvailable = availableResources.Contains(function.Resource);
			}
			else
			{
				selectedResourceIsAvailable = false;

				helpers.Log(nameof(Service), nameof(GetFunctionsForBooking), $"Eligible resources call did not return a collection for function {function.Name} {function.Id}", Name);
			}

			helpers.Log(nameof(Service), nameof(GetFunctionsForBooking), $"Function {function.Definition.Label} resource {function.Resource.Name} is {(selectedResourceIsAvailable ? "available" : "not available, setting to null")}", Name);

			if (selectedResourceIsAvailable)
			{
				srmFunction.SelectedResource = function.Resource.ID.ToString();
			}
			else
			{
				var occupyingReservations = helpers.ServiceManager.GetReservationsWithSpecificResourceAndWithinTimeSpan(function.Resource, StartWithPreRoll.ToUniversalTime(), EndWithPostRoll.ToUniversalTime());

				helpers.Log(nameof(Service), nameof(GetFunctionsForBooking), $"Resource {function.Resource.Name} is being occupied by reservations {string.Join(", ", occupyingReservations.Select(r => $"{r.Name} [{r.Start}({r.Start.Kind}) - {r.End}({r.End.Kind})])"))}");

				srmFunction.SelectedResource = Guid.Empty.ToString();
				function.Resource = null;
			}
		}

		public IEnumerable<Library.Solutions.SRM.Model.Events.Event> GetCustomEventsForBooking(Helpers helpers, IEnumerable<Library.Solutions.SRM.Model.Events.Event> eventsFromBookingManager)
		{
			var customEvents = new List<Library.Solutions.SRM.Model.Events.Event>();

			foreach (var @event in eventsFromBookingManager)
			{
				switch (@event.Name)
				{
					case "UpdateUiProperties":
						bool isSatelliteReception = Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite;
						bool startIsMoreThan72hAway = Start - DateTime.Now > TimeSpan.FromHours(72);
						if (isSatelliteReception && startIsMoreThan72hAway)
						{
							@event.TimeValue = 259200; /* 72 hours in seconds */
							@event.IsChecked = true;

							helpers.Log(nameof(Service), nameof(GetCustomEventsForBooking), "Service is sat RX and starts in more than 72 hours, added custom event to update UI properties", Name);
						}
						break;

					default:
						// nothing
						break;
				}

				if (@event.IsChecked) customEvents.Add(@event);
			}

			return customEvents;
		}

		public IEnumerable<Library.Solutions.SRM.Model.Properties.Property> GetPropertiesForBooking(IEnumerable<Library.Solutions.SRM.Model.Properties.Property> propertiesFromBookingManager, Order order, Helpers helpers = null)
		{
			var properties = new List<Library.Solutions.SRM.Model.Properties.Property>();

			foreach (var property in propertiesFromBookingManager)
			{
				switch (property.Name)
				{
					case ServicePropertyNames.Status:
						property.Value = Status.GetDescription();
						break;
					case ServicePropertyNames.CommentsPropertyName:
						property.Value = Comments?.Clean(allowSiteContent: true) ?? string.Empty;
						break;
					case ServicePropertyNames.ServiceLevelPropertyName:
						property.Value = Convert.ToString((int)BackupType);
						break;
					case ServicePropertyNames.IntegrationTypePropertyName:
						property.Value = IntegrationType.GetDescription();
						break;
					case ServicePropertyNames.IntegrationIsMasterPropertyName:
						property.Value = IntegrationIsMaster.ToString();
						break;
					case ServicePropertyNames.IsGlobalEventLevelReceptionPropertyName:
						property.Value = IsSharedSource.ToString();
						break;
					case ServicePropertyNames.OrderIdsPropertyName:
						property.Value = string.Join(";", OrderReferences);
						break;
					case ServicePropertyNames.LinkedServiceIdPropertyName:
						property.Value = LinkedServiceId.ToString();
						break;
					case ServicePropertyNames.EurovisionIdPropertyName when !string.IsNullOrEmpty(EurovisionWorkOrderId):
						property.Value = EurovisionWorkOrderId;
						break;
					case ServicePropertyNames.EurovisionTransmissionNumberPropertyName when !string.IsNullOrEmpty(EurovisionTransmissionNumber):
						property.Value = EurovisionTransmissionNumber;
						break;
					case ServicePropertyNames.EurovisionBookingDetailsPropertyName:
						property.Value = JsonConvert.SerializeObject(EurovisionBookingDetails, Formatting.None);
						break;
					case ServicePropertyNames.ContactInformationNamePropertyName when !string.IsNullOrEmpty(ContactInformationName):
						property.Value = ContactInformationName.Clean() ?? string.Empty;
						break;
					case ServicePropertyNames.ContactInformationTelephoneNumberPropertyName when !string.IsNullOrEmpty(ContactInformationTelephoneNumber):
						property.Value = ContactInformationTelephoneNumber.Clean() ?? string.Empty;
						break;
					case ServicePropertyNames.LiveUDeviceNamePropertyName when !string.IsNullOrEmpty(LiveUDeviceName):
						property.Value = LiveUDeviceName.Clean() ?? string.Empty;
						break;
					case ServicePropertyNames.AudioReturnInfoPropertyName when !string.IsNullOrEmpty(AudioReturnInfo):
						property.Value = AudioReturnInfo.Clean() ?? string.Empty;
						break;
					case ServicePropertyNames.RecordingConfigurationPropertyName when RecordingConfiguration.IsConfigured:
						property.Value = RecordingConfiguration.Serialize();
						break;
					case ServicePropertyNames.BackupServicePropertyName:
						property.Value = UI_IsBackupService.ToString();
						break;
					case ServicePropertyNames.CommentaryAudioPropertyName:
						property.Value = UI_HasCommentaryAudio.ToString();
						break;
					case ServicePropertyNames.ReportedIssuePropertyName:
						property.Value = HasAnIssueBeenreportedManually.ToString();
						break;
					case ServicePropertyNames.NameOfServiceToTransmitPropertyName:
						property.Value = NameOfServiceToTransmit;
						break;
					case ServicePropertyNames.OrderNamePropertyName:
						property.Value = OrderName;
						break;
					case ServicePropertyNames.VidigoStreamSourceLinkPropertyName when !string.IsNullOrEmpty(VidigoStreamSourceLink):
						property.Value = VidigoStreamSourceLink;
						break;
					case ServicePropertyNames.PlasmaIdsForArchivingPropertyName when !String.IsNullOrEmpty(RecordingConfiguration?.PlasmaIdForArchive):
						property.Value = RecordingConfiguration.PlasmaIdForArchive;
						break;
					case ServicePropertyNames.McrDestination when Definition.VirtualPlatform == VirtualPlatform.Recording || Definition.VirtualPlatform == VirtualPlatform.Routing:
						property.Value = ServiceCategorizer.IsConsideredAsMcrDestinationInMcrView(helpers, this).ToString();
						break;
					case ServicePropertyNames.McrDescription when Definition.VirtualPlatform == VirtualPlatform.Recording || Definition.VirtualPlatform == VirtualPlatform.Routing:
						property.Value = GetMcrDescription(helpers, order);
						break;
					case ServicePropertyNames.MessiLiveDescription when Definition.VirtualPlatform == VirtualPlatform.Recording:
						property.Value = GetMessiLiveDescription();
						break;
					case ServicePropertyNames.ShortDescription:
						property.Value = GetShortDescription(order);
						break;
					case ServicePropertyNames.FileName:
						property.Value = RecordingConfiguration?.RecordingName ?? string.Empty;
						break;
					case ServicePropertyNames.ProfileConfigurationFailReason:
						property.Value = ProfileConfigurationFailReason ?? string.Empty;
						break;
					case ServicePropertyNames.LateChange when LateChange:
						property.Value = true.ToString();
						break;
					case ServicePropertyNames.Channel when IntegrationType == IntegrationType.Plasma:
						property.Value = RecordingConfiguration?.PlasmaTvChannelName ?? String.Empty;
						break;
					case ServicePropertyNames.MCRStatus:
						property.Value = DetermineMcrStatus(helpers, order).GetDescription();
						break;
					case ServicePropertyNames.Automated:
						property.Value = (helpers.OrderManagerElement.IsDeviceAutomationEnabled(this) && Functions.Any(f => f.Resource?.GetResourcePropertyBooleanValue(ResourcePropertyNames.IsAutomated) ?? false)).ToString();
						break;

					case ServicePropertyNames.AllUserTasksCompleted:
						property.Value = AllUserTasksCompleted.ToString().ToLower(); // VSC: ToLower() required
						break;

					case ServicePropertyNames.EvsIdPropertyName:
						property.Value = EvsId;
						break;

					default:
						continue;
				}

				property.Value = property.Value?.Clean(true);
				property.IsChecked = true;

				properties.Add(property);
			}

			helpers?.Log(nameof(Service), nameof(GetPropertiesForBooking), $"Custom properties for booking: {string.Join(",", properties.Select(p => $"{p.Name}={p.Value}"))}");

			return properties;
		}

		public bool UpdateUiProperties(Helpers helpers, Order order)
		{
			if (!IsBooked) return false;

			var reservation = helpers.ReservationManager.GetReservation(Id);
			if (reservation == null)
			{
				helpers.Log(nameof(Service), nameof(UpdateUiProperties), $"Unable to retrieve reservation {Id}");
				return false;
			}

			try
			{
				var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(Definition.BookingManagerElementName));
				var uiProperties = GetPropertiesForBooking(bookingManager.Properties, order, helpers).Where(x => ServicePropertyNames.UiProperties.Contains(x.Name)).ToDictionary(x => x.Name, x => (object)x.Value);
				if (!uiProperties.Any()) return true;

				return TryUpdateCustomProperties(helpers, uiProperties, reservation);
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool UpdateRecordingConfigurationProperty(Helpers helpers, Order order)
		{
			if (!IsBooked || Definition.VirtualPlatformServiceType != VirtualPlatformType.Recording) return false;

			var reservation = helpers.ReservationManager.GetReservation(Id);
			if (reservation == null)
			{
				helpers.Log(nameof(Service), nameof(UpdateUiProperties), $"Unable to retrieve reservation {Id}");
				return false;
			}

			try
			{
				var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(Definition.BookingManagerElementName));
				var uiProperties = GetPropertiesForBooking(bookingManager.Properties, order, helpers).Where(x => ServicePropertyNames.RecordingConfigurationPropertyName.Equals(x.Name)).ToDictionary(x => x.Name, x => (object)x.Value);

				helpers.Log(nameof(Service), nameof(UpdateRecordingConfigurationProperty), $"Updating properties: {String.Join(", ", uiProperties.Select(x => x.Key + ": " + x.Value))}");

				if (!uiProperties.Any()) return false;

				return TryUpdateCustomProperties(helpers, uiProperties, reservation);
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Checks if the resources assigned to the functions connected to the given matrix input and output are the same device.
		/// </summary>
		/// <param name="matrixFunctionA">Matrix Input or Matrix Output function.</param>
		/// <param name="matrixFunctionB">Matrix Input or Matrix Output function.</param>
		/// <returns>A boolean indicating if the resources used by the functions connected to the matrix input and output are the same device.</returns>
		/// <exception cref="EdgeNotFoundException"/>
		/// <exception cref="FunctionNotFoundException"/>
		/// <exception cref="ArgumentException">Thrown when the two given functions are not connected.</exception>
		public bool MatrixInputAndOutputAreSameDevice(Function matrixFunctionA, Function matrixFunctionB)
		{
			bool matrixFunctionAisInput = Definition.Diagram.Edges.Any(e => e.FromNodeID == matrixFunctionA.NodeId && e.ToNodeID == matrixFunctionB.NodeId);
			bool matrixFunctionBisInput = Definition.Diagram.Edges.Any(e => e.FromNodeID == matrixFunctionB.NodeId && e.ToNodeID == matrixFunctionA.NodeId);
			if (!matrixFunctionAisInput && !matrixFunctionBisInput) throw new ArgumentException($"Function {matrixFunctionA.Definition.Label} is not connected to {matrixFunctionB.Definition.Label}");

			var matrixInputFunction = matrixFunctionAisInput ? matrixFunctionA : matrixFunctionB;
			var matrixOutputFunction = matrixFunctionAisInput ? matrixFunctionB : matrixFunctionA;

			var edgeToMatrixInput = Definition.Diagram.Edges.FirstOrDefault(e => e.ToNodeID == matrixInputFunction.NodeId);
			if (edgeToMatrixInput == null) throw new EdgeNotFoundException(matrixInputFunction.NodeId, Definition.Id);

			var functionToMatrixInput = Functions.FirstOrDefault(f => f.NodeId == edgeToMatrixInput.FromNodeID);
			if (functionToMatrixInput == null) throw new FunctionNotFoundException(edgeToMatrixInput.FromNodeID);

			var edgeFromMatrixOutput = Definition.Diagram.Edges.FirstOrDefault(e => e.FromNodeID == matrixOutputFunction.NodeId);
			if (edgeFromMatrixOutput == null)
				throw new EdgeNotFoundException(matrixOutputFunction.NodeId, Definition.Id);

			var functionFromMatrixOutput = Functions.FirstOrDefault(f => f.NodeId == edgeFromMatrixOutput.ToNodeID);
			if (functionFromMatrixOutput == null) throw new FunctionNotFoundException(edgeFromMatrixOutput.ToNodeID);

			bool isSameDevice = functionToMatrixInput.Resource != null && functionFromMatrixOutput.Resource != null && functionToMatrixInput.Resource.MainDVEElementID == functionFromMatrixOutput.Resource.MainDVEElementID;

			return isSameDevice;
		}

		/// <summary>
		/// Checks if resources are assigned to the Functions that should have resources assigned. 
		/// </summary>
		/// <remarks>
		/// Due to the way the standard SRM solution works, all Functions in the Service Definitions have been marked optional.
		/// Therefore this method is used to see if all Functions that require a resource have a resource assigned.
		/// </remarks>
		/// <returns>A boolean indicating if all resources are correctly assigned.</returns>
		public bool VerifyFunctionResources(Helpers helpers = null)
		{
			if (Definition == null) return false;

			if (Definition.VirtualPlatform == VirtualPlatform.AudioProcessing)
			{
				return VerifyAudioProcessingFunctionResources();
			}

			foreach (var function in Functions)
			{
				// matrix functions in a non-routing service always need to have a resource assigned
				if (function.Name.Contains("Matrix") && Definition.VirtualPlatform != VirtualPlatform.Routing)
				{
					if (function.Name.Contains("Input") && !VerifyMatrixFunctionResources(helpers, function))
					{
						return false;
					}
					else
					{
						// output will automatically be checked when verifying the input
					}
				}
				else if (function.RequiresResource && !function.IsDummy && function.Resource is null)
				{
					helpers?.Log(nameof(Service), nameof(VerifyFunctionResources), $"Function {function.Definition.Label} requires a resource but has no resource assigned", Name);
					return false;
				}
				else
				{
					//Nothing
				}
			}

			helpers?.Log(nameof(Service), nameof(VerifyFunctionResources), $"All necessary functions have resources assigned", Name);

			return true;
		}

		/// <summary>
		/// Verify that the matrix function resources are correctly assigned.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="matrixInputFunction">The matrix input function.</param>
		/// <returns>Returns true if the matrix input and connected output are correctly assigned.</returns>
		private bool VerifyMatrixFunctionResources(Helpers helpers, Function matrixInputFunction)
		{
			// if the function is a matrix and has no resource assigned, check if the resources before and after the matrix are the same device
			// if they aren't the same device, a matrix is required as routing between those different devices 
			// the lack of a resource for the Matrix function then leads to a ResourceOverbooked state on the Service
			// if they are the same device, no routing is required between those functions
			var edgeToMatrixInput = Definition.Diagram.Edges.FirstOrDefault(e => e.ToNodeID == matrixInputFunction.NodeId);
			if (edgeToMatrixInput == null)
			{
				throw new EdgeNotFoundException($"Edge to node id {matrixInputFunction.NodeId} not found in service definition {Definition.Id}");
			}

			var functionToMatrixInput = Functions.FirstOrDefault(f => f.NodeId == edgeToMatrixInput.FromNodeID);
			if (functionToMatrixInput == null)
			{
				throw new FunctionNotFoundException($"Function not found with node id {edgeToMatrixInput.FromNodeID}");
			}

			var edgeToMatrixOutput = Definition.Diagram.Edges.FirstOrDefault(e => e.FromNodeID == matrixInputFunction.NodeId);
			if (edgeToMatrixOutput == null)
			{
				throw new EdgeNotFoundException($"Edge from node id {matrixInputFunction.NodeId} not found in service definition {Definition.Id}");
			}

			var matrixOutputFunction = Functions.FirstOrDefault(f => f.NodeId == edgeToMatrixOutput.ToNodeID);
			if (matrixOutputFunction == null)
			{
				throw new FunctionNotFoundException($"Function not found with node id {edgeToMatrixInput.ToNodeID}");
			}

			var edgeFromMatrixOutput = Definition.Diagram.Edges.FirstOrDefault(e => e.FromNodeID == edgeToMatrixOutput.ToNodeID);
			if (edgeFromMatrixOutput == null)
			{
				throw new EdgeNotFoundException($"Edge from node id {edgeToMatrixOutput.ToNodeID} not found in service definition {Definition.Id}");
			}

			var functionFromMatrixOutput = Functions.FirstOrDefault(f => f.NodeId == edgeFromMatrixOutput.ToNodeID);
			if (functionFromMatrixOutput == null)
			{
				throw new FunctionNotFoundException($"Function not found with node id {edgeFromMatrixOutput.ToNodeID}");
			}

			if (functionToMatrixInput.Resource != null && functionFromMatrixOutput.Resource != null && functionToMatrixInput.Resource.MainDVEDmaID == functionFromMatrixOutput.Resource.MainDVEDmaID && functionToMatrixInput.Resource.MainDVEElementID == functionFromMatrixOutput.Resource.MainDVEElementID)
			{
				// in case the resources connected to the input and the output matrix are from the same device
				// then no matrix resources are needed
				helpers?.Log(nameof(Service), nameof(VerifyMatrixFunctionResources), $"Resource {functionToMatrixInput.Resource.Name} from function {functionToMatrixInput.Definition.Label} and resource {functionFromMatrixOutput.Resource.Name} from function {functionFromMatrixOutput} are the same device, matrix functions are OK", Name);
				return true;
			}

			helpers?.Log(nameof(Service), nameof(VerifyMatrixFunctionResources), $"Function {matrixInputFunction.Definition.Label} has resource {matrixInputFunction.Resource?.Name}, function {matrixOutputFunction.Definition.Label} has resource {matrixOutputFunction.Resource?.Name}", Name);

			// if we reach this code then both the input and output matrix function should have a resource assigned
			return matrixInputFunction.Resource != null && matrixOutputFunction.Resource != null;
		}

		/// <summary>
		/// Checks if all audio processing functions have a correct resource assigned.
		/// </summary>
		/// <returns>Returns true in case all resources are ok.</returns>
		private bool VerifyAudioProcessingFunctionResources()
		{
			var audioDeembeddingFunction = Functions.FirstOrDefault(f => f.Name == AudioProcessingService.AudioDeembeddingFunctionDefinitionName) ?? throw new FunctionNotFoundException(AudioProcessingService.AudioDeembeddingFunctionDefinitionName);
			var audioDeembeddingRequiredParameter = audioDeembeddingFunction.Parameters.FirstOrDefault(p => p.Name.Contains("Required"));
			var audioDeembeddingRequired = Convert.ToString(audioDeembeddingRequiredParameter?.Value) != "Yes";

			var audioDolbyDecodingFunction = Functions.FirstOrDefault(f => f.Name == AudioProcessingService.AudioDolbyDecodingFunctionDefinitionName) ?? throw new FunctionNotFoundException(AudioProcessingService.AudioDolbyDecodingFunctionDefinitionName);
			var audioDolbyDecodingRequiredParameter = audioDolbyDecodingFunction.Parameters.FirstOrDefault(p => p.Name.Contains("Required"));
			var audioDolbyDecodingRequired = Convert.ToString(audioDolbyDecodingRequiredParameter?.Value) != "Yes";

			var audioShufflingFunction = Functions.FirstOrDefault(f => f.Name == AudioProcessingService.AudioShufflingFunctionDefinitionName) ?? throw new FunctionNotFoundException(AudioProcessingService.AudioShufflingFunctionDefinitionName);
			var audioShufflingRequiredParameter = audioShufflingFunction.Parameters.FirstOrDefault(p => p.Name.Contains("Required"));
			var audioShufflingRequired = Convert.ToString(audioShufflingRequiredParameter?.Value) != "Yes";

			var audioEmbeddingFunction = Functions.FirstOrDefault(f => f.Name == AudioProcessingService.AudioEmbeddingFunctionDefinitionName) ?? throw new FunctionNotFoundException(AudioProcessingService.AudioEmbeddingFunctionDefinitionName);
			var audioEmbeddingRequiredParameter = audioEmbeddingFunction.Parameters.FirstOrDefault(p => p.Name.Contains("Required"));
			var audioEmbeddingRequired = Convert.ToString(audioEmbeddingRequiredParameter?.Value) != "Yes";

			var matrixSdiInputAudioDeembeddingFunction = Functions.FirstOrDefault(f => f.NodeId == AudioProcessingService.MatrixSdiInputAudioDeembeddingFunctionNodeId) ?? throw new FunctionNotFoundException(AudioProcessingService.MatrixSdiInputAudioDeembeddingFunctionNodeId);
			var audioDeembeddingCheckMatrixInputResource = audioDeembeddingRequired && (audioDolbyDecodingRequired || audioShufflingRequired || audioEmbeddingRequired);
			var audioDeembeddingCheckMatrixOutputResource = false;
			if (!VerifyAudioProcessingFunctionResource(audioDeembeddingFunction, matrixSdiInputAudioDeembeddingFunction, null, audioDeembeddingCheckMatrixInputResource, audioDeembeddingCheckMatrixOutputResource)) return false;

			var matrixSdiInputAudioDolbyDecodingFunction = Functions.FirstOrDefault(f => f.NodeId == AudioProcessingService.MatrixSdiInputAudioDolbyDecodingFunctionNodeId) ?? throw new FunctionNotFoundException(AudioProcessingService.MatrixSdiInputAudioDolbyDecodingFunctionNodeId);
			var matrixSdiOutputAudioDolbyDecodingFunction = Functions.FirstOrDefault(f => f.NodeId == AudioProcessingService.MatrixSdiOutputAudioDolbyDecodingFunctionNodeId) ?? throw new FunctionNotFoundException(AudioProcessingService.MatrixSdiOutputAudioDolbyDecodingFunctionNodeId);
			var audioDolbyDecodingCheckMatrixInputResource = audioDolbyDecodingRequired && (audioShufflingRequired || audioEmbeddingRequired);
			var audioDolbyDecodingCheckMatrixOutputResource = audioDolbyDecodingRequired && audioDeembeddingRequired;
			if (!VerifyAudioProcessingFunctionResource(audioDolbyDecodingFunction, matrixSdiInputAudioDolbyDecodingFunction, matrixSdiOutputAudioDolbyDecodingFunction, audioDolbyDecodingCheckMatrixInputResource, audioDolbyDecodingCheckMatrixOutputResource)) return false;

			var matrixSdiInputAudioShufflingFunction = Functions.FirstOrDefault(f => f.NodeId == AudioProcessingService.MatrixSdiInputAudioShufflingFunctionNodeId) ?? throw new FunctionNotFoundException(AudioProcessingService.MatrixSdiInputAudioShufflingFunctionNodeId);
			var matrixSdiOutputAudioShufflingFunction = Functions.FirstOrDefault(f => f.NodeId == AudioProcessingService.MatrixSdiOutputAudioShufflingFunctionNodeId) ?? throw new FunctionNotFoundException(AudioProcessingService.MatrixSdiOutputAudioShufflingFunctionNodeId);
			var audioShufflingCheckMatrixInputResource = audioShufflingRequired && audioEmbeddingRequired;
			var audioShufflingCheckMatrixOutputResource = audioShufflingRequired && (audioDeembeddingRequired || audioDolbyDecodingRequired);
			if (!VerifyAudioProcessingFunctionResource(audioShufflingFunction, matrixSdiInputAudioShufflingFunction, matrixSdiOutputAudioShufflingFunction, audioShufflingCheckMatrixInputResource, audioShufflingCheckMatrixOutputResource)) return false;

			var matrixSdiOutputAudioEmbeddingFunction = Functions.FirstOrDefault(f => f.NodeId == AudioProcessingService.MatrixSdiOutputAudioEmbeddingFunctionNodeId) ?? throw new FunctionNotFoundException(AudioProcessingService.MatrixSdiOutputAudioEmbeddingFunctionNodeId);
			var audioEmbeddingCheckMatrixInputResource = false;
			var audioEmbeddingCheckMatrixOutputResource = audioEmbeddingRequired && (audioDeembeddingRequired || audioDolbyDecodingRequired || audioShufflingRequired);
			if (!VerifyAudioProcessingFunctionResource(audioEmbeddingFunction, null, matrixSdiOutputAudioEmbeddingFunction, audioEmbeddingCheckMatrixInputResource, audioEmbeddingCheckMatrixOutputResource)) return false;

			return true;
		}

		private bool VerifyAudioProcessingFunctionResource(Function audioProcessingFunction, Function matrixInputFunction, Function matrixOutputFunction, bool checkInput = true, bool checkOutput = true)
		{
			if (audioProcessingFunction == null)
			{
				throw new FunctionNotFoundException("Audio processing function could not be found");
			}

			var requiredParameter = audioProcessingFunction.Parameters.FirstOrDefault(p => p.Name.Contains("Required"));
			if (requiredParameter == null)
			{
				throw new FunctionParameterNotFoundException("Audio processing Required");
			}

			// if this function is not required then we don't need to check the actual resource(s)
			if (Convert.ToString(requiredParameter.Value) != "Yes") return true;

			if (audioProcessingFunction.Resource == null) return false;

			if (checkInput)
			{
				if (matrixInputFunction == null)
				{
					throw new FunctionNotFoundException("Matrix SDI Input audio processing function could not be found");
				}

				if (matrixInputFunction.Resource == null) return false;
			}

			if (checkOutput)
			{
				if (matrixOutputFunction == null)
				{
					throw new FunctionNotFoundException("Matrix SDI Output audio processing function could not be found");
				}

				if (matrixOutputFunction.Resource == null) return false;
			}

			return true;
		}

		private void VerifyContributingResourceCreation(Helpers helpers)
		{
			// check if the contributing resource was correctly created
			// the resource should have the same id as the service
			var contributingFunctionResource = DataMinerInterface.ResourceManager.GetResource(helpers, Id);
			if (contributingFunctionResource == null)
			{
				// Catching faulty event level reception resources
				ContributingResource = null;
				ResourcePool = null;
				return;
			}

			// the contributing resource will only be added to 1 resource pool
			var resourcePoolId = contributingFunctionResource.PoolGUIDs.FirstOrDefault();
			var resourcePool = DataMinerInterface.ResourceManager.GetResourcePool(helpers, resourcePoolId) ?? throw new ResourcePoolNotFoundException($"Resource pool not found {resourcePoolId}");

			ContributingResource = contributingFunctionResource;
			ResourcePool = resourcePool;
		}

		/// <summary>
		/// Cancel this service.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">The order containing this Service.</param>
		public void Cancel(Helpers helpers, Order order)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (order == null) throw new ArgumentNullException(nameof(order));

			isCancelled = true;

			TryUpdateStatus(helpers, order);

			ReleaseResources(helpers, order);

			DeleteEvsRecordingSession(helpers);

			DeleteUserTasks(helpers);
		}

		/// <summary>
		/// Uncancel this service.
		/// </summary>
		/// <param name="engine">The engine object.</param>
		public void Uncancel(IEngine engine)
		{
			isCancelled = false;
		}

		/// <summary>
		/// This method is used to directly updated the Comments property on the ReservationInstance.
		/// </summary>
		/// <param name="helpers">Used to report progress.</param>
		/// <returns>True if update was successful.</returns>
		public bool UpdateCommentsProperty(Helpers helpers)
		{
			var dictionary = new Dictionary<string, object>
			{
				{ServicePropertyNames.CommentsPropertyName, Comments.Clean(allowSiteContent: true) }
			};

			return TryUpdateCustomProperties(helpers, dictionary);
		}

		/// <summary>
		/// This method is used to directly updated the Comments property on the ReservationInstance.
		/// </summary>
		/// <param name="helpers">Used to report progress.</param>
		/// <returns>True if update was successful.</returns>
		public bool UpdateProfileConfigurationFailReasonProperty(Helpers helpers)
		{
			var dictionary = new Dictionary<string, object>
			{
				{ ServicePropertyNames.ProfileConfigurationFailReason, ProfileConfigurationFailReason }
			};

			return TryUpdateCustomProperties(helpers, dictionary);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			//sb.AppendLine($"Short Description: {GetShortDescription()}"); // throws exception if service is recording
			sb.AppendLine($"\tStatus: {Status}");
			sb.AppendLine($"\tBackup Type: {BackupType}");
			sb.AppendLine($"\tID: {Id}");
			sb.AppendLine($"\tNode ID: {NodeId}");
			sb.AppendLine($"\tStart: {Start}");
			sb.AppendLine($"\tEnd: {End}");
			sb.AppendLine($"\tStart With Pre Roll: {StartWithPreRoll}");
			sb.AppendLine($"\tEnd With Post Roll: {EndWithPostRoll}");
			sb.AppendLine($"\tIntegration Type: {IntegrationType}");
			sb.AppendLine($"\tEurovision ID: {EurovisionWorkOrderId}");
			sb.AppendLine($"\tEurovision Transmission Number: {EurovisionTransmissionNumber}");
			sb.AppendLine($"\tSD Virtual Platform: {Definition.VirtualPlatform}");
			sb.AppendLine($"\tSD ID: {Definition.Id}");
			sb.AppendLine($"\tSD Name: {Definition.Name}");
			sb.AppendLine($"\tIs Eurovision Service: {IsEurovisionService}");

			sb.AppendLine($"\tChildren [{Children.Count}]: {String.Join(Environment.NewLine, Children.Select(x => x.ToString()))}");

			return sb.ToString();
		}

		public void ReleaseResources(Helpers helpers, Order order)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));

			foreach (var function in Functions)
			{
				function.Resource = null;

				// Integration services usually have specific resources assigned to them
				if (IntegrationType == IntegrationType.None) function.EnforceSelectedResource = false;
			}

			helpers.ServiceManager.TryReleaseResources(this, Functions);

			helpers.ServiceManager.UpdateShortDescription(this, order);

			helpers.ServiceManager.TryUpdateAllCustomProperties(this, order);

			order.UpdateUiProperties(helpers);
			order.UpdateServiceConfigurationProperty(helpers);
		}

		public void DeleteEvsRecordingSession(Helpers helpers)
		{
			// Delete EVS recordings if they were created and have not been recording yet
			bool shouldDeleteEvsRecordingSession = !String.IsNullOrEmpty(EvsId);
			helpers.Log(nameof(Service), nameof(DeleteEvsRecordingSession), $"{(shouldDeleteEvsRecordingSession ? String.Empty : "No")} EVS Recording Session to delete");
			if (!shouldDeleteEvsRecordingSession) return;

			helpers.Log(nameof(Service), nameof(DeleteEvsRecordingSession), $"Deleting EVS Recording Session {EvsId}");
			helpers.EvsManager.DeleteRecordingSession(this, EvsId);
		}

		private void DeleteUserTasks(Helpers helpers)
		{
			if (UserTasks == null || !UserTasks.Any())
			{
				UserTasks = helpers.UserTaskManager.GetUserTasks(this).ToList();
			}

			foreach (var userTask in UserTasks)
			{
				userTask.Delete(helpers.UserTaskManager.TicketingManager);
			}
		}

		public Status GenerateStatus(Helpers helpers = null, Order orderContainingService = null)
		{
			var now = DateTime.Now.RoundToMinutes();
			helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Evaluating status at {now.ToFullDetailString()}", Name);
			helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Service timing: {TimingInfoToString(this)}", Name);

			if (isCancelled)
			{
				helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Service is canceled, generated status is {Status.Cancelled.GetDescription()}", Name);
				return Status.Cancelled;
			}

			if (IsPreliminary || !IsBooked)
			{
				helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Service is not booked, generated status is {Status.Preliminary.GetDescription()}", Name);
				return Status.Preliminary;
			}

			var incompleteConfigurationUserTasks = UserTasks.Where(userTask => userTask.Status == UserTaskStatus.Incomplete && !userTask.Description.ContainsIgnoreCase("File Processing"));

			helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Incomplete non-file-processing user tasks: {string.Join("\n", incompleteConfigurationUserTasks.Select(ut => ut.Description))}", Name);

			bool hasIncompleteConfigurationUserTasks = incompleteConfigurationUserTasks.Any();		

			if (!String.IsNullOrWhiteSpace(ProfileConfigurationFailReason) || (IsInPrerollTimeSpan && hasIncompleteConfigurationUserTasks))
			{
				if (IsInPrerollTimeSpan)
				{
					helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Service PLS failed during preroll", Name);
					helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Generated status is {Status.ServiceQueingAndConfigFailed.GetDescription()}", Name);
					return Status.ServiceQueingAndConfigFailed;
				}
				else
				{
					helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Service PLS failed", Name);
					helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Generated status is {Status.AutomatedConfigurationFailed.GetDescription()}", Name);
					return Status.AutomatedConfigurationFailed;
				}
			}

			bool isResourceOverbooked = !VerifyFunctionResources(helpers);
			helpers?.Log(nameof(Service), nameof(GenerateStatus), $"{(isResourceOverbooked ? "Not all" : "All")} expected resources are assigned", Name);

			if (EndWithPostRoll <= now)
			{
				return GenerateStatusAfterEndWithPostRoll(helpers, orderContainingService, incompleteConfigurationUserTasks, hasIncompleteConfigurationUserTasks, isResourceOverbooked);
			}

			if (isResourceOverbooked)
			{
				helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Generated status is {Status.ResourceOverbooked.GetDescription()}", Name);
				return Status.ResourceOverbooked;
			}

			if (hasIncompleteConfigurationUserTasks)
			{
				helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Service has incomplete configuration user tasks: {string.Join(", ", incompleteConfigurationUserTasks.Select(ut => ut.Name))}", Name);
				helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Generated status is {Status.ConfigurationPending.GetDescription()}", Name);
				return Status.ConfigurationPending;
			}

			if (StartWithPreRoll <= now && now < Start)
			{
				helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Generated status is {Status.ServiceQueuingConfigOk.GetDescription()}", Name);

				return Status.ServiceQueuingConfigOk;
			}

			if (Start <= now && now < End)
			{
				helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Generated status is {Status.ServiceRunning.GetDescription()}", Name);

				return Status.ServiceRunning;
			}

			if (End <= now && now < EndWithPostRoll)
			{
				helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Generated status is {Status.PostRoll.GetDescription()}", Name);

				return Status.PostRoll;
			}

			helpers?.Log(nameof(Service), nameof(GenerateStatus), $"Generated status is {Status.AutomatedConfigurationCompleted.GetDescription()}", Name);

			return Status.AutomatedConfigurationCompleted;
		}

		private Status GenerateStatusAfterEndWithPostRoll(Helpers helpers, Order orderContainingService, IEnumerable<LiveUserTask> incompleteConfigurationUserTasks, bool hasIncompleteConfigurationUserTasks, bool isResourceOverbooked)
		{
			if (HasAnIssueBeenreportedManually || isResourceOverbooked || hasIncompleteConfigurationUserTasks)
			{
				helpers?.Log(nameof(Service), nameof(GenerateStatusAfterEndWithPostRoll), $"Service is completed with errors because {nameof(HasAnIssueBeenreportedManually)}={HasAnIssueBeenreportedManually}, {nameof(isResourceOverbooked)}={isResourceOverbooked}, incomplete user tasks: {string.Join(", ", incompleteConfigurationUserTasks.Select(ut => ut.Description))}", Name);

				helpers?.Log(nameof(Service), nameof(GenerateStatusAfterEndWithPostRoll), $"Generated status is {Status.ServiceCompletedWithErrors.GetDescription()}", Name);

				return Status.ServiceCompletedWithErrors;
			}

			bool fileProcessingStatusNeeded = Definition.VirtualPlatform == VirtualPlatform.Recording && StatusManager.ServiceHasIncompleteFileProcessingUserTasks(helpers, this, hasIncompleteConfigurationUserTasks, orderContainingService);

			if (fileProcessingStatusNeeded)
			{
				helpers?.Log(nameof(Service), nameof(GenerateStatusAfterEndWithPostRoll), "Service has incomplete file processing user tasks", Name);

				helpers?.Log(nameof(Service), nameof(GenerateStatusAfterEndWithPostRoll), $"Generated status is {Status.FileProcessing.GetDescription()}", Name);

				return Status.FileProcessing;
			}

			helpers?.Log(nameof(Service), nameof(GenerateStatusAfterEndWithPostRoll), $"Generated status is {Status.ServiceCompleted.GetDescription()}", Name);

			return Status.ServiceCompleted;
		}

		public bool FunctionIsFirstResourceRequiringFunctionInDefinition(Helpers helpers, Function function)
		{
			var positionsOfResourceRequiringFunctions = Functions.Where(f => f.RequiresResource && !f.IsDummy).ToDictionary(f => f.Definition.Label, f => Definition.GetFunctionPosition(f));
			if (!positionsOfResourceRequiringFunctions.Any()) throw new InvalidOperationException($"None of the functions ({String.Join(", ", Functions.Select(f => $"{f.Definition.Label} {nameof(Function.RequiresResource)}={f.RequiresResource} & {nameof(Function.IsDummy)}={f.IsDummy}"))}) in the service require a resource");

			int lowestResourceRequiringPosition = positionsOfResourceRequiringFunctions.Values.Min();

			if (positionsOfResourceRequiringFunctions.TryGetValue(function.Definition.Label, out int functionPosition))
			{
				return functionPosition == lowestResourceRequiringPosition;
			}
			else
			{
				return false;
			}
		}

		public Function GetLastResourceRequiringFunction(Helpers helpers)
		{
			return Functions.Single(f => FunctionIsLastResourceRequiringFunctionInDefinition(helpers, f));
		}

		/// <summary>
		/// Checks if the provided function is the last one (from left to right) that requires a resource.
		/// </summary>
		/// <param name="helpers">Link with DataMiner.</param>
		/// <param name="function">Function to check.</param>
		/// <returns>True if the given function is the last one in the service that requires a resource.</returns>
		public bool FunctionIsLastResourceRequiringFunctionInDefinition(Helpers helpers, Function function)
		{
			var positionsOfResourceRequiringFunctions = Functions.Where(f => f.RequiresResource && !f.IsDummy).ToDictionary(f => f.Definition.Label, f => Definition.GetFunctionPosition(f));
			if (!positionsOfResourceRequiringFunctions.Any()) throw new InvalidOperationException($"None of the functions ({String.Join(", ", Functions.Select(f => $"{f.Definition.Label} {nameof(Function.RequiresResource)}={f.RequiresResource} & {nameof(Function.IsDummy)}={f.IsDummy}"))}) in the service require a resource");

			int highestResourceRequiringPosition = positionsOfResourceRequiringFunctions.Values.Max();

			if (positionsOfResourceRequiringFunctions.TryGetValue(function.Definition.Label, out int functionPosition))
			{
				return functionPosition == highestResourceRequiringPosition;
			}
			else
			{
				return false;
			}
		}

		public string PropertiesToString()
		{
			return $"ID={Id}, IsSharedSource={IsSharedSource}, SecurityViewIds={string.Join(",", SecurityViewIds)}, RequiresRouting={RequiresRouting}, HasIssueBeenReportedManually={HasAnIssueBeenreportedManually}, NameOfServiceToRecordOrTransmit={NameOfServiceToTransmitOrRecord}, IntegrationIsMaster={IntegrationIsMaster}, Timing={TimingInfoToString(this)}, ChangedByUpdateServiceScript={ChangedByUpdateServiceScript}";
		}

		/// <summary>
		/// Gets the short description for this service.
		/// </summary>
		/// <param name="order">The order that contains this service. Can be null in case of non-recording services. Cannot be null in case of recording services.</param>
		/// <param name="engine"></param>
		/// <exception cref="ArgumentNullException">Thrown in case this service is a recording and <paramref name="order"/> is null.</exception>
		public string GetShortDescription(Order order = null)
		{
			switch (Definition.VirtualPlatform)
			{
				case VirtualPlatform.ReceptionSatellite:
					DisplayName = GetSatelliteReceptionDescription();
					break;

				case VirtualPlatform.ReceptionIp:
					DisplayName = GetIpReceptionShortDescription();
					break;

				case VirtualPlatform.TransmissionSatellite:
					DisplayName = GetSatelliteTransmissionDescription();
					break;

				case VirtualPlatform.TransmissionFiber:
					DisplayName = GetFiberTranmissionDescription();
					break;

				case VirtualPlatform.ReceptionLiveU:
					DisplayName = GetLiveUReceptionDescription();
					break;

				case VirtualPlatform.ReceptionFiber:
					DisplayName = GetFiberReceptionDescription();
					break;

				case VirtualPlatform.Recording:
					if (order == null) throw new ArgumentNullException(nameof(order), "Order cannot be null for a recording service.");
					DisplayName = GetRecordingDescription(order);
					break;

				case VirtualPlatform.Routing:
					DisplayName = GetRoutingDescription();
					break;

				case VirtualPlatform.AudioProcessing:
					DisplayName = GetAudioProcessingDescription();
					break;

				case VirtualPlatform.ReceptionFixedService:
					DisplayName = GetFixedServiceDescription();
					break;

				case VirtualPlatform.ReceptionFixedLine when Definition.Description.Contains("Helsinki City Connections"):
					DisplayName = GetFixedLineHelsinkiCityConnectionsReceptionShortDescription();
					break;

				case VirtualPlatform.ReceptionFixedLine when Definition.Description.Contains("LY"):
					DisplayName = GetFixedLineLyReceptionShortDescription();
					break;

				case VirtualPlatform.ReceptionFixedLine when Definition.Id == ServiceDefinitionGuids.FixedLineAtvuReception:
					DisplayName = $"{Definition.VirtualPlatformServiceName.GetDescription()} - ATVU ({Functions.Single().ResourceName})";
					break;

				case VirtualPlatform.Destination when Definition.Description.Contains("Helsinki City Connections"):
					if (order == null) throw new ArgumentNullException(nameof(order), "Order cannot be null for a Helsinki City Connections Destination service.");
					DisplayName = GetDestinationHelsinkiCityConnectionsShortDescription(order);
					break;

				case VirtualPlatform.VizremStudio when Definition.Description.Contains("ST26"):
					DisplayName = GetVizremSt26ShortDescription(order);
					break;

				case VirtualPlatform.VizremStudio:
					// vizrem studio asource needs short description "Studio - [resource] + [vizrem farm resource]"
					DisplayName = GetStandardShortDescription().Remove(0, "VIZREM".Count()).Trim() + $" + {order.AllServices.Single(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremFarm).Functions.Single().ResourceName}";
					break;

				default:
					DisplayName = GetStandardShortDescription();
					break;
			}
			return DisplayName;
		}

		private string GetVizremSt26ShortDescription(Order order)
		{
			var sb = new StringBuilder("STUDIO - ST26");

			if (order.SourceService.Equals(this))
			{
				string vizremFarmResourceName = order.AllServices.Single(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremFarm).Functions.Single().ResourceName;

				sb.Append($" + {vizremFarmResourceName}");
			}

			return sb.ToString();
		}

		private string GetDestinationHelsinkiCityConnectionsShortDescription(Order order)
		{
			var result = new StringBuilder();

			var routingParent = order.AllServices.SingleOrDefault(s => s.Children.Contains(this) && s.Definition.VirtualPlatform == VirtualPlatform.Routing);
			if (routingParent is null) return GetStandardShortDescription();

			var matrixOutputFunction = routingParent.Functions.Single(f => routingParent.Definition.FunctionIsLast(f));
			var matrixOutputResourceName = matrixOutputFunction.ResourceName;
			string matrixOutputResourceDisplayName = matrixOutputResourceName.Split('.').Last();
			result.Append(matrixOutputResourceDisplayName);

			return result.ToString();
		}

		private string GetFixedLineHelsinkiCityConnectionsReceptionShortDescription()
		{
			var result = new StringBuilder();

			var routingChild = Children.FirstOrDefault(child => child.Definition.VirtualPlatform == VirtualPlatform.Routing);
			if (routingChild is null) return GetStandardShortDescription();

			string virtualPlatformName = Definition.VirtualPlatformServiceName.GetDescription();
			result.Append(virtualPlatformName);

			var matrixInputFunction = routingChild.Functions.Single(f => routingChild.Definition.FunctionIsFirst(f));
			var matrixInputResourceName = matrixInputFunction?.Resource?.Name ?? Constants.None;
			string matrixInputResourceDisplayName = matrixInputResourceName.Split('.').Last();
			result.Append(" - " + matrixInputResourceDisplayName);

			var helsinkiCityConnectionsSourceLocationProfileParameter = Functions.SelectMany(f => f.Parameters).FirstOrDefault(pp => pp.Id == ProfileParameterGuids.FixedLineHelsinkiCityConnectionsSourceLocation);

			if (helsinkiCityConnectionsSourceLocationProfileParameter?.StringValue == "Eduskunta")
			{
				var matrixOutputFunction = routingChild.Functions.Single(f => routingChild.Definition.FunctionIsLast(f));
				var matrixOutputResourceName = matrixOutputFunction?.Resource?.Name ?? Constants.None;
				string matrixOutputResourceDisplayName = matrixOutputResourceName.Split('.').Last();
				result.Append($" ({matrixOutputResourceDisplayName})");
			}

			return result.ToString();
		}

		private string GetFixedLineLyReceptionShortDescription()
		{
			var result = new StringBuilder(GetStandardShortDescription());

			var hmxRoutingChild = Children.FirstOrDefault(child => child.IsHmxRouting);

			if (hmxRoutingChild is null) return result.ToString();

			var matrixInputFunction = hmxRoutingChild.Functions.Single(f => hmxRoutingChild.Definition.FunctionIsFirst(f));
			var matrixInputResourceName = matrixInputFunction?.Resource?.Name ?? Constants.None;
			string matrixInputResourceDisplayName = matrixInputResourceName.Split('.').Last();
			result.Append($" ({matrixInputResourceDisplayName})");

			return result.ToString();
		}

		private string GetStandardShortDescription()
		{
			var result = new StringBuilder();
			bool isServiceDestination = Definition.VirtualPlatform == VirtualPlatform.Destination;

			if (!isServiceDestination)
			{
				string virtualPlatformName = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
				result.Append(virtualPlatformName);
			}

			if (Functions.Count == 1 || Functions.Count == 2)
			{
				var function = Functions.FirstOrDefault() ?? throw new FunctionNotFoundException();
				var resource = function.Resource;
				string resourceName = resource != null ? resource.GetDisplayName(function.Id) : Constants.None;

				string resourceDescription = !isServiceDestination ? " - " + resourceName : resourceName;
				result.Append(resourceDescription);

				string feedName = Functions.SelectMany(f => f.Parameters).SingleOrDefault(p => p.Id == ProfileParameterGuids.FeedName)?.StringValue;
				if (!string.IsNullOrEmpty(feedName))
				{
					result.Append($" - {feedName}");
				}
			}

			return result.ToString();
		}

		private string GetIpReceptionShortDescription()
		{
			string result = $"{Definition.VirtualPlatformServiceName.GetDescription()} - {Functions.Single(f => Definition.FunctionIsFirst(f)).ResourceName}";

			var lastFunction = Functions.Single(f => Definition.FunctionIsLast(f));
			bool serviceHasValidIpDecodingOutput = Functions.Count == 2 && !lastFunction.IsDummy;
			if (serviceHasValidIpDecodingOutput)
			{
				result += $" ({lastFunction.ResourceName})";
			}

			return result.ToString();
		}

		private string GetFiberReceptionDescription()
		{
			var description = new StringBuilder();

			bool isFullCapacity = Definition.Description.Contains("Full Capacity");
			description.Append(isFullCapacity ? GetFiberReceptionFullCapacityDescription() : GetFiberReceptionLimitedCapacityDescription());

			return description.ToString();
		}

		private string GetFiberReceptionFullCapacityDescription()
		{
			var description = new StringBuilder();

			string virtualPlatformName = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
			description.Append($"{virtualPlatformName}");

			var function = Functions.SingleOrDefault() ?? throw new FunctionNotFoundException();

			string fiberResourceName = function.Resource != null ? function.Resource.GetDisplayName(function.Id) : Constants.None;
			if (!string.IsNullOrEmpty(fiberResourceName))
			{
				description.Append($" - {fiberResourceName}");
			}

			string feedName = Functions.SelectMany(f => f.Parameters).SingleOrDefault(p => p.Id == ProfileParameterGuids.FeedName)?.StringValue;
			if (!string.IsNullOrEmpty(feedName))
			{
				description.Append($" - {feedName}");
			}

			return description.ToString();
		}

		private string GetFiberReceptionLimitedCapacityDescription()
		{
			var description = new StringBuilder();

			string virtualPlatformName = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
			description.Append($"{virtualPlatformName}");

			var fiberSourceFunction = Functions.SingleOrDefault(f => f != null && f.Id == FunctionGuids.FiberSource) ?? throw new FunctionNotFoundException(FunctionGuids.FiberSource);

			string fiberResourceName = fiberSourceFunction.Resource != null ? fiberSourceFunction.Resource.GetDisplayName(fiberSourceFunction.Id) : Constants.None;
			if (!string.IsNullOrEmpty(fiberResourceName))
			{
				description.Append($" - {fiberResourceName} --> ");
			}

			var function = Functions.Single(f => f != null && f.Id == FunctionGuids.FiberDecoding) ?? throw new FunctionNotFoundException(FunctionGuids.FiberDecoding);

			fiberResourceName = function.Resource != null ? function.Resource.GetDisplayName(function.Id) : Constants.None;
			if (!string.IsNullOrEmpty(fiberResourceName))
			{
				description.Append(fiberResourceName);
			}

			string feedName = Functions.SelectMany(f => f.Parameters).Single(p => p.Id == ProfileParameterGuids.FeedName).StringValue;
			if (!string.IsNullOrEmpty(feedName))
			{
				description.Append($" - {feedName}");
			}

			return description.ToString();
		}

		private string GetFiberTranmissionDescription()
		{
			var description = new StringBuilder();
			bool isLimitedCapacity = Definition.Description.Contains("Limited Capacity");

			if (isLimitedCapacity)
			{
				string virtualPlatformName = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
				string virtualPlatformType = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceType);

				description.Append($"{virtualPlatformName} {virtualPlatformType}");

				var genericEncodingfunction = Functions.SingleOrDefault(f => f.Id == FunctionGuids.GenericEncoding);
				string fiberResourceName = genericEncodingfunction?.Resource != null ? genericEncodingfunction.Resource.GetDisplayName(genericEncodingfunction.Id) : Constants.None;

				if (!string.IsNullOrEmpty(fiberResourceName))
				{
					description.Append($" - {fiberResourceName} --> ");
				}

				var fiberSourceFunction = Functions.SingleOrDefault(f => f != null && f.Id == FunctionGuids.FiberDestination);
				fiberResourceName = fiberSourceFunction?.Resource != null ? fiberSourceFunction.Resource.GetDisplayName(fiberSourceFunction.Id) : Constants.None;

				if (!string.IsNullOrEmpty(fiberResourceName))
				{
					description.Append(fiberResourceName);
				}

				string nameValue = Functions.SelectMany(f => f.Parameters).SingleOrDefault(p => p.Id == ProfileParameterGuids.FeedName)?.StringValue;
				if (!string.IsNullOrEmpty(nameValue))
				{
					description.Append($" - {nameValue}");
				}

				return description.ToString();
			}
			else
			{
				string standardDescription = GetStandardShortDescription();
				description.Append(standardDescription);

				return description.ToString();
			}
		}

		private string GetSatelliteReceptionDescription()
		{
			var result = new StringBuilder();

			string virtualPlatformName = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
			result.Append($"{virtualPlatformName}");

			var allProfileParams = Functions.SelectMany(f => f.Parameters).ToList();

			var modulationStandardProfileParameter = allProfileParams.FirstOrDefault(p => p.Id == ProfileParameterGuids.ModulationStandard) ?? throw new ProfileParameterNotFoundException(ProfileParameterGuids.ModulationStandard.ToString(), null, allProfileParams);
			bool serviceUsesNs3OrNs4Modulation = modulationStandardProfileParameter.StringValue == "NS3" || modulationStandardProfileParameter.StringValue == "NS4";
			if (serviceUsesNs3OrNs4Modulation)
			{
				var demodulatingFunction = Functions.Single(f => f.Id == FunctionGuids.Demodulating);
				var demodulatingResource = demodulatingFunction.Resource;
				string demodulatingResourceName = demodulatingResource != null ? demodulatingResource.GetDisplayName(demodulatingFunction.Id) : string.Empty;
				result.Append(" - " + demodulatingResourceName);
			}

			var decodingFunction = Functions.Single(f => f.Id == FunctionGuids.Decoding);
			var decodingResource = decodingFunction.Resource;
			string decodingResourceName = decodingResource != null ? decodingResource.GetDisplayName(decodingFunction.Id) : string.Empty;
			if (!string.IsNullOrEmpty(decodingResourceName))
			{
				if (serviceUsesNs3OrNs4Modulation) result.Append(" > ");
				else result.Append(" - ");

				result.Append(decodingResourceName);
			}

			string serviceSelection = Functions.SelectMany(f => f.Parameters).FirstOrDefault(p => p.Id == ProfileParameterGuids.ServiceSelection)?.StringValue;
			if (!string.IsNullOrWhiteSpace(serviceSelection)) result.Append(" - " + serviceSelection);

			return result.ToString();
		}

		private string GetSatelliteTransmissionDescription()
		{
			var result = new StringBuilder();

			string virtualPlatformName = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
			string virtualPlatformType = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceType);
			result.Append($"{virtualPlatformName} {virtualPlatformType}");

			result.Append(" - ");

			var encodingFunction = Functions.Single(f => f.Id == FunctionGuids.GenericEncoding);
			var encodingResource = encodingFunction.Resource;
			string encodingResourceName = encodingResource != null ? encodingResource.GetDisplayName(encodingFunction.Id) : "None";
			result.Append(encodingResourceName);

			result.Append(" --> ");

			var modulatingFunction = Functions.Single(f => f.Id == FunctionGuids.GenericModulating);
			var modulatingResource = modulatingFunction.Resource;
			string modulatingResourceName = modulatingResource != null ? modulatingResource.GetDisplayName(modulatingFunction.Id) : "None";
			result.Append(modulatingResourceName);

			return result.ToString();
		}

		private string GetLiveUReceptionDescription()
		{
			var result = new StringBuilder();

			string virtualPlatformName = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
			result.Append($"{virtualPlatformName}");

			var function = Functions.Single();
			var functionResource = function.Resource;
			string resourceName = functionResource != null ? functionResource.GetDisplayName(function.Id) : string.Empty;
			if (!string.IsNullOrEmpty(resourceName)) result.Append(" - " + resourceName);

			bool isPasila = Definition.Description.Contains("Pasila");
			bool isAudioReturn = Functions.SelectMany(f => f.Parameters).Any(p => p.Id == ProfileParameterGuids.AudioReturnChannel && p.StringValue == "Required");
			if (isPasila && isAudioReturn) result.Append(" - Audio Return");

			return result.ToString();
		}

		private string GetRoutingDescription()
		{
			string virtualPlatform = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatform);

			var firstResource = Functions[0].Resource;
			string firstResourceName = firstResource != null ? firstResource.Name : Constants.None;
			string firstResourceDisplayName = firstResourceName.Split('.').Last();

			var lastResource = Functions.Last().Resource;
			string lastResourceName = lastResource != null ? lastResource.Name : Constants.None;
			string lastResourceDisplayName = lastResourceName.Split('.').Last();

			if (firstResource != null || lastResource != null)
			{
				string matrix = string.Empty;

				bool isEduskuntaMatrix = firstResourceName.Contains("EDUSKUNTA") || lastResourceName.Contains("EDUSKUNTA");
				bool isHmxMatrix = firstResourceName.Contains("HMX") || lastResourceName.Contains("HMX");
				bool isNmxMatrix = firstResourceName.Contains("NMX") || lastResourceName.Contains("NMX");

				if (isEduskuntaMatrix) matrix = "EDU";
				else if (isHmxMatrix) matrix = "HMX";
				else if (isNmxMatrix) matrix = "NEWS";
				else
				{
					// nothing
				}

				return $"{matrix} {virtualPlatform} - {firstResourceDisplayName} --> {lastResourceDisplayName}";
			}

			if (!(string.IsNullOrEmpty(firstResourceName) && string.IsNullOrEmpty(lastResourceName)))
				return $"{virtualPlatform} - {firstResourceDisplayName} --> {lastResourceDisplayName}";
			else return virtualPlatform;
		}

		private string GetRecordingDescription(Order order)
		{
			var description = new StringBuilder();

			switch (Definition.Description)
			{
				case "Messi Live":
					description.Append("Live Rec.");
					description.Append(GetLiveRecordingDescriptionResourcePart(order));
					break;
				case "Messi Live Backup":
					description.Append("Live Backup Rec.");
					description.Append(GetLiveRecordingDescriptionResourcePart(order));
					break;
				case "Messi News":
					description.Append(GetNewsRecordingDescription());
					description.Append(GetNewsRecordingDetailsPart(order));
					break;
				default:
					return "Recording";
			}

			if (RecordingConfiguration != null)
			{
				if (IntegrationType == IntegrationType.Plasma)
				{
					if (!string.IsNullOrEmpty(RecordingConfiguration.PlasmaTvChannelName)) description.Append(" - " + RecordingConfiguration.PlasmaTvChannelName);
					if (!string.IsNullOrEmpty(RecordingConfiguration.PlasmaProgramName)) description.Append(" - " + RecordingConfiguration.PlasmaProgramName);
				}
				else if (!string.IsNullOrEmpty(RecordingConfiguration.RecordingName)) description.Append(" - " + RecordingConfiguration.RecordingName);
				else {  /* nothing */ }
			}

			return description.ToString();
		}

		private string GetLiveRecordingDescriptionResourcePart(Order order)
		{
			var result = new StringBuilder();

			var routingService = order.AllServices.Single(s => s.Children.Contains(this));
			var matrixOutputFunction = routingService.Functions.LastOrDefault(); // Not all services have functions, for example the Dummy Services or Eurovision Dummy Services.
			var matrixOutputResource = matrixOutputFunction?.Resource;
			string matrixOutputResourceName = matrixOutputResource?.Name ?? Constants.None;
			string matrixOutputResourceDisplayName = matrixOutputResourceName.Split('.').Last();
			result.Append(" - " + matrixOutputResourceDisplayName);

			string functionResourceName = Functions.Single().Resource?.Name ?? Constants.None;
			if (!string.IsNullOrEmpty(functionResourceName)) result.Append(" --> " + functionResourceName);

			return result.ToString();
		}

		private string GetNewsRecordingDescription()
		{
			var newsRecordingFunction = Functions.SingleOrDefault();
			if (newsRecordingFunction == null) return "News Rec.";

			var feedTypeProfileParameter = newsRecordingFunction.Parameters.FirstOrDefault(p => p.Id == ProfileParameterGuids._FeedType);
			if (feedTypeProfileParameter == null || string.IsNullOrEmpty(feedTypeProfileParameter.StringValue) || feedTypeProfileParameter.StringValue == "None") return "News Rec.";

			return $"News {feedTypeProfileParameter.StringValue} Rec.";
		}

		private string GetNewsRecordingDetailsPart(Order order)
		{
			var result = new StringBuilder();

			var routingService = order.AllServices.Single(s => s != null && s.Children?.Contains(this) == true);

			if (routingService.Functions != null && routingService.Functions.Any())
			{
				string matrixInputResourceName = routingService.Functions.Single(f => f != null && routingService.Definition?.FunctionIsFirst(f) == true).ResourceName;
				string matrixInputResourceDisplayName = matrixInputResourceName.Split('.').Last();

				if (!string.IsNullOrEmpty(matrixInputResourceDisplayName))
				{
					result.Append(" - " + matrixInputResourceDisplayName);
				}
			}

			string recordingFunctionResourceName = Functions.Single().ResourceName;
			if (!string.IsNullOrEmpty(recordingFunctionResourceName)) result.Append(" --> " + recordingFunctionResourceName);

			return result.ToString();
		}

		private string GetAudioProcessingDescription()
		{
			string virtualPlatformName = EnumExtensions.GetDescriptionFromEnumValue(Definition.VirtualPlatformServiceName);
			var nonRoutingResourceNames = Functions.Where(f => !FunctionGuids.AllMatrixGuids.Contains(f.Id) && f.Resource != null).Select(f => f.Resource.Name);

			var stringBuilder = new StringBuilder(virtualPlatformName);
			foreach (var resourceName in nonRoutingResourceNames) stringBuilder.Append(" - " + resourceName);

			return stringBuilder.ToString();
		}

		private string GetFixedServiceDescription()
		{
			var result = new StringBuilder();

			string virtualPlatformName = Definition.VirtualPlatformServiceName.GetDescription();
			result.Append(virtualPlatformName);

			var sourceFunction = Functions.SingleOrDefault(f => Definition.FunctionIsFirst(f)) ?? throw new FunctionNotFoundException();
			result.Append(" - " + sourceFunction.ResourceName);

			var decodingFunction = Functions.SingleOrDefault(f => Definition.FunctionIsLast(f)) ?? throw new FunctionNotFoundException();
			result.Append(" - " + decodingFunction.ResourceName);

			string serviceSelection = sourceFunction.Parameters.FirstOrDefault(p => p.Id == ProfileParameterGuids.ServiceSelection)?.StringValue;
			if (!string.IsNullOrWhiteSpace(serviceSelection)) result.Append(" - " + serviceSelection);

			return result.ToString();
		}

		private List<Service> FindDeepestEndPointServiceChildren()
		{
			var nonRoutingChildren = new List<Service>();

			if (Children == null || !Children.Any()) return nonRoutingChildren;

			foreach (var child in Children)
			{
				if (child.Definition.IsEndPointService)
				{
					nonRoutingChildren.Add(child);
				}
				else
				{
					nonRoutingChildren.AddRange(child.FindDeepestEndPointServiceChildren());
				}
			}

			return nonRoutingChildren;
		}

		public string GetMcrDescription(Helpers helpers, Order order)
		{
			switch (Definition.VirtualPlatform)
			{
				case VirtualPlatform.Recording:
					return GetRecordingMcrDescription(helpers, order);
				case VirtualPlatform.Routing:
					return GetRoutingMcrDescription();
				default:
					return String.Empty;
			}
		}

		private string GetRecordingMcrDescription(Helpers helpers, Order order)
		{
			try
			{
				var liveVideoOrder = new LiveVideoOrder(helpers, order);

				var routingServiceChainForThisService = ((EndPointService)liveVideoOrder.GetLiveVideoService(this))?.RoutingServiceChain ?? throw new ServiceNotFoundException($"Could not find service {Name} among live video services {string.Join(", ", liveVideoOrder.LiveVideoServices.Select(s => s.Service.name))}", true);

				if (Definition.Id == ServiceDefinitionGuids.RecordingMessiNews)
				{
					// For Messi News recordings: should return the NMX (News) matrix input with the EVS resource between parentheses

					var nmxRoutingService = routingServiceChainForThisService.AllRoutingServices.SingleOrDefault(s => s.IsNmxMatrix);

					string nmxRoutingResource = nmxRoutingService?.MatrixInputSdi?.Name?.Split('.').Last() ?? Constants.None;

					string evsResourceName = Functions.Single().ResourceName;

					if (!evsResourceName.Contains("Dummy EVS"))
					{
						return $"{nmxRoutingResource} ({evsResourceName})";
					}
					else
					{
						return nmxRoutingResource;
					}
				}
				else
				{
					// For Messi Live recordings: should return the HMX output

					return routingServiceChainForThisService.AllRoutingServices.SingleOrDefault(s => s.IsHmxMatrix)?.MatrixOutputSdi?.Name.Split('.').Last() ?? Constants.None;
				}
			}
			catch (Exception ex)
			{
				helpers?.Log(nameof(Service), nameof(GetRecordingMcrDescription), $"Unable to find recording MCR description: {ex}");
				return Constants.NotFound;
			}
		}

		private string GetRoutingMcrDescription()
		{
			var matrixOutputFunction = Functions.LastOrDefault();
			var matrixOutputResource = matrixOutputFunction?.Resource;
			string matrixOutputResourceName = matrixOutputResource?.Name ?? Constants.None;
			string matrixOutputResourceDisplayName = matrixOutputResourceName.Split('.').Last();

			return matrixOutputResourceDisplayName;
		}

		private string GetMessiLiveDescription()
		{
			string resultDescription = string.Empty;

			if (!string.IsNullOrWhiteSpace(Definition.Description) && Definition.Description.Contains("Messi Live"))
			{
				var function = Functions.FirstOrDefault();
				var functionResourceName = function?.Resource?.GetDisplayName(function.Id) ?? String.Empty;
				resultDescription = functionResourceName;
			}

			return resultDescription;
		}

		public string GetFormattedTechnicalDetails()
		{
			if (Functions == null) return string.Empty;

			StringBuilder sb = new StringBuilder();
			foreach (var function in Functions)
			{
				if (function == null) continue;

				sb.AppendLine($"{function.Name.ToUpper()} : ");

				foreach (var parameter in function.Parameters)
				{
					if (!ProfileParameterGuids.AllAudioChannelConfigurationGuids.Contains(parameter.Id))
					{
						sb.AppendLine($"{parameter.ToString()} | ");
					}
				}
			}

			sb.AppendLine($"Security view ids: {string.Join(";", SecurityViewIds)} | ");

			return sb.ToString();
		}

		public Dictionary<string, HashSet<FunctionResource>> GetAvailableResourcesPerFunctionBasedOnTiming(Helpers helpers, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null)
		{
			helpers.LogMethodStart(nameof(Service), nameof(GetAvailableResourcesPerFunctionBasedOnTiming), out var stopwatch, Name);

			overwrittenFunctionTimeRanges = overwrittenFunctionTimeRanges ?? Functions.ToDictionary(f => f.Definition.Label, f => new TimeRangeUtc(StartWithPreRoll.ToUniversalTime(), EndWithPostRoll.ToUniversalTime(), TimeZoneInfo.Utc));

			helpers.Log(nameof(Service), nameof(GetAvailableResourcesPerFunctionBasedOnTiming), $"Using function time ranges: {string.Join(", ", overwrittenFunctionTimeRanges.Select(f => $"{f.Key} from {f.Value.Start.ToFullDetailString()} until {f.Value.Stop.ToFullDetailString()}"))}, while Service time range is {StartWithPreRoll.ToFullDetailString()} until {EndWithPostRoll.ToFullDetailString()}");

			var contexts = new List<YleEligibleResourceContext>();

			var nonDummyFunctions = Functions.Where(f => !f.Definition.IsDummy).ToList();
			var dummyFunctions = Functions.Where(f => f.Definition.IsDummy).ToList();

			foreach (var function in nonDummyFunctions)
			{
				if (!overwrittenFunctionTimeRanges.TryGetValue(function.Definition.Label, out var functionTimeRange))
				{
					functionTimeRange = new TimeRangeUtc(StartWithPreRoll.ToUniversalTime(), EndWithPostRoll.ToUniversalTime(), TimeZoneInfo.Utc);

					helpers.Log(nameof(Service), nameof(GetAvailableResourcesPerFunctionBasedOnTiming), $"No function time range found for {function.Definition.Label}, using Service time range from {StartWithPreRoll.ToFullDetailString()} until {EndWithPostRoll.ToFullDetailString()}");
				}

				var context = function.Definition.GetEligibleResourceContext(helpers, functionTimeRange.Start, functionTimeRange.Stop);

				if (IsBooked && function.Resource != null)
				{
					context.ReservationIdToIgnore = new ReservationInstanceID(Id);
					context.NodeIdToIgnore = function.NodeId;

					// WARNING GetEligibleResources fails when adding IdsToIgnore while
					// - reservation doesn't exist (= is not booked)
					// - function currently has no resource assigned (function.Resource == null)
				}

				contexts.Add(context);
			}

			var availableResourcesPerFunction = helpers.ResourceManager.GetAvailableResources(contexts, true);

			foreach (var dummyFunction in dummyFunctions)
			{
				availableResourcesPerFunction.Add(dummyFunction.Definition.Label, new HashSet<FunctionResource>());
			}

			helpers.LogMethodCompleted(nameof(Service), nameof(GetAvailableResourcesPerFunctionBasedOnTiming), null, stopwatch);

			return availableResourcesPerFunction;
		}

		public static string TimingInfoToString(Service service)
		{
			return $"Preroll: {service.PreRoll}, Start:{service.Start.ToFullDetailString()}, End:{service.End.ToFullDetailString()}, Postroll:{service.PostRoll}";
		}

		public static string TimingInfoToString(TimeSpan preroll, DateTime start, DateTime end, TimeSpan postroll)
		{
			return $"Preroll: {preroll}, Start:{start.ToFullDetailString()}, End:{end.ToFullDetailString()}, Postroll: {postroll}";
		}

		/// <summary>
		/// This method is called after the booked service was removed to set it back to its unbooked state
		/// </summary>
		internal void WasRemoved()
		{
			ReservationInstance = null;
			IsBooked = false;
		}

		public object Clone()
		{
			return new Service(this);
		}
	}
}