namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using Library_1.Utilities;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceUpdates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Statuses;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.AssignProfilesAndResources;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.Properties;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.ReservationAction;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Constants = Configuration.Constants;
	using Function = DataMiner.DeveloperCommunityLibrary.YLE.Function.Function;
	using Service = Service.Service;
	using ServiceChange = History.ServiceChange;
	using VirtualPlatform = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform;

	public class Order : LiteOrder, IYleChangeTracking, ICloneable
	{
		public static readonly int StartNowDelayInMinutes = 30;
		public static readonly int StartNowDelayInMinutesForFeenix = 5;
		public static readonly int StartNowDelayInMinutesWhenAddingServicesToRunningOrder = 5;
		public static readonly int StartInTheFutureDelayInMinutes = 10;

		public static readonly string ServiceBookingEventName = "Service Booking";
		public static readonly string HandleOrderActionScriptName = "HandleOrderAction";
		public static readonly string HandleOrderActionScriptReservationGuidParameterName = "ReservationGuid";
		public static readonly string HandleOrderActionScriptActionParameterName = "Action";
		public static readonly string HandleOrderActionScriptBookingManagerInfoParameterName = "Booking Manager Info";

		private bool hasCustomPreRoll;
		private TimeSpan customPreRoll;
		private bool hasCustomPostRoll;
		private TimeSpan customPostRoll;

		/// <summary>
		/// Initializes a new instance of the <see cref="Order"/> class.
		/// </summary>
		public Order()
		{

		}

		private Order(Order other)
		{
			CloneHelper.CloneProperties(other, this);

			// Set Sources after cloned properties to make sure the SourceService and BackupSourceService properties are correct.
			Sources = other.Sources.Select(s => (Service)s.Clone()).ToList();
		}

		public static Order FromExternalJson(Helpers helpers, ExternalJson.ExternalJson externalJson)
		{
			return ExternalJsonOrderCreator.CreateOrder(helpers, externalJson);
		}

		public static Order FromTemplate(Helpers helpers, OrderTemplate template, string orderName, DateTime startTime)
		{
			var serviceTemplateIdMapping = new Dictionary<Guid, Guid>(); // Template Id - Service Id
			var sources = ServicesFromTemplate(helpers, template.Sources, startTime, template.ServiceOffsets, serviceTemplateIdMapping);

			// Update linked service Ids with actual service Ids
			var allServices = OrderManager.FlattenServices(sources);
			foreach (var service in allServices)
			{
				service.AcceptChanges();

				// When new orders are created automatically, the recording name must to be the same as the order name.
				if (service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Recording && service.RecordingConfiguration != null)
				{
					service.RecordingConfiguration.RecordingName = orderName.Clean();
				}

				if (service.LinkedServiceId == Guid.Empty || !serviceTemplateIdMapping.ContainsKey(service.LinkedServiceId)) continue;
				service.LinkedServiceId = serviceTemplateIdMapping[service.LinkedServiceId];
			}

			InitializeNamesOfServicesToTransmitOrRecord(helpers, template, serviceTemplateIdMapping, allServices);

			allServices.ForEach(service => helpers.Log(nameof(Service), nameof(Service), $"Summary of properties on service object taken from service configuration: ID={service.Id}, IsSharedSource= {service.IsSharedSource}, SecurityViewIds={string.Join(",", service.SecurityViewIds)}, RequiresRouting={service.RequiresRouting}, HasIssueBeenReportedManually={service.HasAnIssueBeenreportedManually}, NameOfServiceToRecordOrTransmit={service.NameOfServiceToTransmitOrRecord}, IntegrationIsMaster={service.IntegrationIsMaster}, Timing={Service.TimingInfoToString(service)}, ChangedByUpdateServiceScript={service.ChangedByUpdateServiceScript}", service.Name));

			DateTime orderEndTime = FlattenServices(sources).Max(x => x.End);

			Order order = new Order
			{
				Id = Guid.Empty,
				Event = null,
				ManualName = orderName,
				Start = startTime,
				End = orderEndTime,
				Status = Status.Preliminary,
				Comments = template.Comments,
				IntegrationType = IntegrationType.None,
				Type = OrderType.Video,
				Subtype = allServices.Exists(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremStudio) ? OrderSubType.Vizrem : OrderSubType.Normal,
				Definition = new ServiceDefinition { BookingManagerElementName = SrmConfiguration.OrderBookingManagerElementName },
				Company = template.Company,
				Contract = template.Contract,
				SportsPlanning = template.SportsPlanning ?? new SportsPlanning(),
				NewsInformation = template.NewsInformation ?? new NewsInformation(),
				McrOperatorNotes = template.McrOperatorNotes,
				MediaOperatorNotes = template.OperatorNotes,
				IsCreatedFromTemplate = true,
				RecurringSequenceInfo = template.RecurringSequenceInfo ?? new Recurrence.RecurringSequenceInfo(),
				BillingInfo = template.BillingInfo,
				CreatedByUserName = template.CreatedByUserName
			};

			order.SetUserGroupIds(template.UserGroupIds);
			order.SetSecurityViewIds(template.SecurityViewIds);

			if (template.RecurringSequenceInfo != null)
			{
				order.RecurringSequenceInfo.Recurrence.IsConfigured = true;
				order.RecurringSequenceInfo.TemplateId = template.Id;
			}

			order.AcceptChanges();

			order.Sources = sources;
			order.SetServiceDisplayNames();

			return order;
		}

		internal void InitializeAudioConfigCopyFromSource(Helpers helpers)
		{
			// compare audio channel configurations to see if they are copies from the source
			foreach (var sourceService in Sources)
			{
				sourceService.AudioChannelConfiguration.IsCopyFromSource = false;

				helpers.Log(nameof(Order), nameof(InitializeAudioConfigCopyFromSource), $"{sourceService.Name} audio channel config: {sourceService.AudioChannelConfiguration}");

				foreach (var childService in FlattenServices(sourceService.Children))
				{
					try
					{
						helpers.Log(nameof(Order), nameof(InitializeAudioConfigCopyFromSource), $"{childService.Name} audio channel config: {childService.AudioChannelConfiguration}");
						childService.AudioChannelConfiguration.SetIsCopyFromSourceProperty(sourceService.AudioChannelConfiguration);
					}
					catch (Exception)
					{
						childService.AudioChannelConfiguration.IsCopyFromSource = false;
					}
				}
			}
		}

		private static void InitializeNamesOfServicesToTransmitOrRecord(Helpers helpers, OrderTemplate template, Dictionary<Guid, Guid> serviceTemplateIdMapping, List<Service> allServices)
		{
			foreach (var serviceTemplate in OrderTemplate.FlattenServiceTemplates(template.Sources))
			{
				bool serviceTransmitsOrRecordsAnotherService = serviceTemplate.ServiceTemplateIdToTransmitOrRecord != Guid.Empty;
				if (!serviceTransmitsOrRecordsAnotherService)
				{
					helpers.Log(nameof(Order), nameof(FromTemplate), $"Service Template ID {serviceTemplate.Id} does not transmit or record a service");
					continue;
				}

				if (!serviceTemplateIdMapping.ContainsKey(serviceTemplate.Id) || !serviceTemplateIdMapping.ContainsKey(serviceTemplate.ServiceTemplateIdToTransmitOrRecord))
				{
					helpers.Log(nameof(Order), nameof(FromTemplate), $"Service Template ID {serviceTemplate.Id} or {serviceTemplate.ServiceTemplateIdToTransmitOrRecord} could not be found between IDs {string.Join(", ", serviceTemplateIdMapping.Keys)}");
					continue;
				}

				var serviceToUpdate = allServices.SingleOrDefault(s => s.Id.Equals(serviceTemplateIdMapping[serviceTemplate.Id])) ?? throw new ServiceNotFoundException(serviceTemplateIdMapping[serviceTemplate.Id]);
				var serviceToTransmitOrRecord = allServices.SingleOrDefault(x => x.Id.Equals(serviceTemplateIdMapping[serviceTemplate.ServiceTemplateIdToTransmitOrRecord])) ?? throw new ServiceNotFoundException(serviceTemplateIdMapping[serviceTemplate.ServiceTemplateIdToTransmitOrRecord]);

				serviceToUpdate.NameOfServiceToTransmitOrRecord = serviceToTransmitOrRecord.Name;

				helpers.Log(nameof(Order), nameof(FromTemplate), $"Set service {serviceToUpdate.Name} name of service to record or transmit to '{serviceToUpdate.NameOfServiceToTransmitOrRecord}'");
			}
		}

		private static List<Service> ServicesFromTemplate(Helpers helpers, List<ServiceTemplate> templates, DateTime orderStartTime, Dictionary<Guid, TimeSpan> offsets, Dictionary<Guid, Guid> serviceTemplateIdMapping)
		{
			var services = new List<Service>();
			foreach (var template in templates)
			{
				var serviceStartTime = orderStartTime.Add(offsets[template.Id]);

				var service = Service.FromTemplate(helpers, template, serviceStartTime);
				serviceTemplateIdMapping.Add(template.Id, service.Id);
				service.SetChildren(ServicesFromTemplate(helpers, template.Children, orderStartTime, offsets, serviceTemplateIdMapping));
				services.Add(service);
			}

			return services;
		}

		/// <summary>
		/// Preroll of the order. Equal to the duration between the start of the first preroll among its services and the first start among its services. 
		/// </summary>
		[ChangeTracked]
		public TimeSpan PreRoll
		{
			get
			{
				if (hasCustomPreRoll)
				{
					return customPreRoll;
				}
				else
				{
					if (Sources == null || !Sources.Any() || AllServices.TrueForAll(s => s.IsSharedSource)) return TimeSpan.Zero;

					var earliestServicePreRollStart = AllServices.Where(s => !s.IsSharedSource).Select(s => s.StartWithPreRoll).Min();
					var earliestServiceStart = AllServices.Where(s => !s.IsSharedSource).Select(s => s.Start).Min();

					bool earliestServicePreRollStartIsBeforeEarliestServiceStart = earliestServicePreRollStart < earliestServiceStart;

					return earliestServicePreRollStartIsBeforeEarliestServiceStart ? earliestServiceStart - earliestServicePreRollStart : TimeSpan.Zero;
				}
			}
			set
			{
				hasCustomPreRoll = true;
				customPreRoll = value;
			}
		}

		public DateTime StartWithPreRoll => Start - PreRoll;

		/// <summary>
		/// Postroll of the order. Equal to the duration between the start of the latest postroll among its services and the latest end among its services. 
		/// </summary>
		[ChangeTracked]
		public TimeSpan PostRoll
		{
			get
			{
				if (hasCustomPostRoll)
				{
					return customPostRoll;
				}
				else
				{
					if (Sources == null || !Sources.Any() || AllServices.TrueForAll(s => s.IsSharedSource)) return TimeSpan.Zero;

					var latestServicePostRollEnd = AllServices.Where(s => !s.IsSharedSource).Select(s => s.EndWithPostRoll).Max();
					var latestServiceEnd = AllServices.Where(s => !s.IsSharedSource).Select(s => s.End).Max();

					bool latestServicePostRollEndIsLaterThanLatestServiceEnd = latestServiceEnd < latestServicePostRollEnd;

					return latestServicePostRollEndIsLaterThanLatestServiceEnd ? latestServicePostRollEnd - latestServiceEnd : TimeSpan.Zero;
				}
			}
			set
			{
				hasCustomPostRoll = true;
				customPostRoll = value;
			}
		}

		public DateTime EndWithPostRoll => End + PostRoll;

		public bool ShouldBeRunning
		{
			get
			{
				DateTime now = DateTime.Now;
				bool consideredAsRunning = now >= StartWithPreRoll && now <= EndWithPostRoll && Status != Status.Preliminary && Id != Guid.Empty;
				return (Status == Status.Running && PreviousRunningOrderId == Guid.Empty) || consideredAsRunning;
			}
		}

		/// <summary>
		/// The list of sources in this order.
		/// </summary>
		public List<Service> Sources { get; set; } = new List<Service>();

		public event EventHandler<Service> SourceServiceChanged;

		[ChangeTracked]
		public Service SourceService
		{
			get
			{
				return Sources.FirstOrDefault(x => x.BackupType == BackupType.None);
			}

			set
			{
				if (value == null) throw new ArgumentNullException("value");
				if (value.BackupType != BackupType.None) throw new ArgumentException("The assigned service cannot be a backup service");

				bool sourceServiceIsBeingReplacedWithItself = SourceService?.Name == value.Name;
				if (!sourceServiceIsBeingReplacedWithItself)
				{
					if (SourceService != null)
					{
						value.SetChildren(SourceService.Children);
						Sources.Remove(SourceService);
					}

					Sources.Add(value);
				}

				SourceServiceChanged?.Invoke(this, value);
			}
		}

		public event EventHandler<Service> BackupSourceServiceChanged;

		public Service BackupSourceService
		{
			get
			{
				return Sources.FirstOrDefault(x => x.BackupType != BackupType.None);
			}

			set
			{
				var currentBackupService = BackupSourceService;

				if (value == null)
				{
					if (currentBackupService != null)
					{
						// Remove existing service
						Sources.Remove(currentBackupService);
					}
				}
				else
				{
					if (value.BackupType == BackupType.None) throw new ArgumentException("The assigned service should be a backup service");

					if (currentBackupService == null)
					{
						Sources.Add(value);
					}
					else if (currentBackupService.Name != value.Name)
					{
						value.SetChildren(currentBackupService.Children);
						Sources.Remove(currentBackupService);
						Sources.Add(value);
					}
					else
					{
						// do nothing
					}
				}

				BackupSourceServiceChanged?.Invoke(this, value);
			}
		}

		/// <summary>
		/// The service definition used by the order.
		/// </summary>
		public ServiceDefinition Definition { get; set; }

		/// <summary>
		/// Work Order Id of the eurovision service that was requested through the LiveOrderForm
		/// </summary>
		public string EurovisionWorkOrderId => AllServices.Select(s => s.EurovisionWorkOrderId).FirstOrDefault(ebuId => !string.IsNullOrWhiteSpace(ebuId)) ?? string.Empty;

		/// <summary>
		/// Transmission Number of the Eurovision Synopsis.
		/// Used for uniquely identifying Eurovision Orders.
		/// Only applicable for Orders that use Eurovision Services or Orders generated by the Eurovision Integration.
		/// </summary>
		public string EurovisionTransmissionNumber => AllServices.Select(s => s.EurovisionTransmissionNumber).FirstOrDefault(ebuId => !string.IsNullOrWhiteSpace(ebuId)) ?? string.Empty;

		/// <summary>
		/// Gets a IEnumerable containing all Services in the Order.
		/// </summary>
		[ChangeTracked]
		public List<Service> AllServices => FlattenServices(Sources);

		public List<Service> GetAllMainServices()
		{
			return SourceService != null ? new[] { SourceService }.Concat(FlattenServices(SourceService.Children)).ToList() : new List<Service>();
		}

		public List<Service> GetAllBackupServices()
		{
			return BackupSourceService != null ? new[] { BackupSourceService }.Concat(FlattenServices(BackupSourceService.Children)).ToList() : new List<Service>();
		}

		public List<Service> GetAddedServices()
		{
			var change = Change as OrderChange;

			var serviceCollectionChanges = change.GetCollectionChanges(nameof(AllServices));
			if (serviceCollectionChanges is null) return new List<Service>();

			var addedServiceIdentifiers = serviceCollectionChanges.Changes.Where(c => c.Type == CollectionChangeType.Add).Select(c => c.ItemIdentifier).ToList();

			return AllServices.Where(s => addedServiceIdentifiers.Contains(s.UniqueIdentifier)).ToList();
		}

		/// <summary>
		/// Gets a boolean indicating if the order contains cueing (preroll) services.
		/// </summary>
		public bool HasCueingServices
		{
			get => AllServices.Exists(x => x.Status == YLE.Service.Status.ServiceQueuingConfigOk);
		}

		public bool HasPostRollServices
		{
			get => AllServices.Exists(x => x.Status == YLE.Service.Status.PostRoll);
		}

		/// <summary>
		/// Gets a boolean indicating if the order contains a fixed plasma source. Used by UI to filter.
		/// </summary>
		public bool HasFixedPlasmaSource
		{
			get
			{
				var sourceService = Sources.FirstOrDefault(s => s.BackupType == BackupType.None);
				if (sourceService == null) return false;

				bool sourceIsFixedLineLy = sourceService.Definition.Name == "_Fixed Line RX LY";
				bool sourceIsMadeFromPlasmaIntegration = sourceService.IntegrationType == IntegrationType.Plasma;

				return sourceIsMadeFromPlasmaIntegration && sourceIsFixedLineLy;
			}
		}

		/// <summary>
		/// Gets a boolean indicating if an order should always be displayed on the operator pages regardless of filtering.
		/// </summary>
		public bool ShouldOrderAlwaysBeDisplayed
		{
			get
			{
				var endPointServices = AllServices.Where(s => !s.Children.Any() && ((s.IntegrationType == IntegrationType.None && !s.IntegrationIsMaster) || s.IntegrationType == IntegrationType.Plasma && s.Definition?.Description != null && s.Definition.Description.Contains("News"))).ToList();

				return IsPlasmaFixedSourceLinkedToAnyHmxRouting(endPointServices);
			}
		}

		/// <summary>
		/// Booking can only be edited when Order is not running and has no cueing or post roll services.
		/// </summary>
		public bool CanEditBooking
		{
			get => !HasCueingServices && !HasPostRollServices && Status != Status.Running;
		}

		/// <summary>
		/// Will check if an HMX routing service can be found between the Plasma Fixed Source and one of the given end point services.
		/// </summary>
		/// <returns>True if an HMX routing service could be found, else false</returns>
		public bool IsPlasmaFixedSourceLinkedToAnyHmxRouting(List<Service> endPointServices)
		{
			if (!HasFixedPlasmaSource)
			{
				return false;
			}

			bool parentIsNotNull;
			var orderServices = AllServices;
			foreach (var endPointService in endPointServices)
			{
				var child = endPointService;

				do
				{
					var parent = orderServices.FirstOrDefault(s => s.Children.Contains(child));
					parentIsNotNull = parent != null;

					if (parentIsNotNull && parent.IsHmxRouting)
					{
						return true;
					}

					child = parent;

				} while (parentIsNotNull);
			}

			return false;
		}

		public bool UI_Team_Messi_Live(Helpers helpers)
		{
			bool orderContainsMessiLiveRecording = AllServices.Exists(s => s != null && s.Definition?.VirtualPlatform == VirtualPlatform.Recording && (s.Definition.Description == "Messi Live" || s.Definition.Description == "Messi Live Backup"));

			helpers.Log(nameof(Order), nameof(UI_Team_Messi_Live), $"{nameof(orderContainsMessiLiveRecording)}={orderContainsMessiLiveRecording}");

			return orderContainsMessiLiveRecording;
		}

		public bool UI_Team_Messi_News(Helpers helpers)
		{
			bool orderContainsMessiNewsRecording = AllServices.Exists(s => s != null && s.Definition?.VirtualPlatform == VirtualPlatform.Recording && s.Definition.Description == "Messi News");

			helpers.Log(nameof(Order), nameof(UI_Team_Messi_News), $"{nameof(orderContainsMessiNewsRecording)}={orderContainsMessiNewsRecording}");

			return orderContainsMessiNewsRecording;
		}

		/// <summary>
		/// Gets a boolean indicating if this order is visible on Areena Monitoring page.
		/// </summary>
		public bool UI_Areena(Helpers helpers)
		{
			bool orderContainsAreenaDestination = AllServices.Exists(s => s.Definition.Description == "Areena");

			helpers.Log(nameof(Order), nameof(UI_Areena), $"{orderContainsAreenaDestination.ToString().ToUpper()}={nameof(orderContainsAreenaDestination)}");

			return orderContainsAreenaDestination;
		}

		/// <summary>
		/// Gets a boolean indicating if this order is visible on TOM News page.
		/// </summary>
		public bool UI_News(Helpers helpers)
		{
			if (AllServices == null) return false;

			var validSourceOrDestinationLocations = new[] { "Uutisalue", "Uutisstudiot", "Uutisstudio ST 27/28" };
			var validPlasmaUserCodes = new[] { "ST-24", "ST-25", "ST-26", "ST-27", "NOPSA", "SU28O" };

			if (Subtype == OrderSubType.Vizrem)
			{
				return AllServices.Exists(s => s.Definition.Id == ServiceDefinitionGuids.St26NdiRouter);
			}

			foreach (var service in AllServices)
			{
				if (service.Definition != null && service.Definition.Description?.Contains("News") == true)
				{
					helpers.Log(nameof(Order), nameof(UI_News), $"TRUE because order contains News service");
					return true;
				}

				if (AnyFunctionMatches_UI_News_Conditions(helpers, service.Functions, validSourceOrDestinationLocations, validPlasmaUserCodes))
				{
					return true;
				}
			}

			helpers.Log(nameof(Order), nameof(UI_News), $"FALSE because no conditions met");

			return false;
		}

		public static bool AnyFunctionMatches_UI_News_Conditions(Helpers helpers, List<Function> functions, string[] validSourceOrDestinationLocations, string[] validPlasmaUserCodes)
		{
			foreach (var function in functions.Where(f => f != null))
			{
				var sourceOrDestinationLocation = function.Parameters.FirstOrDefault(p => p != null && (p.Id == ProfileParameterGuids.FixedLineYleHelsinkiSourceLocation || p.Id == ProfileParameterGuids.YleHelsinkiDestinationLocation));

				if (sourceOrDestinationLocation != null && validSourceOrDestinationLocations.Contains(sourceOrDestinationLocation.StringValue))
				{
					helpers.Log(nameof(Order), nameof(UI_News), $"TRUE because order contains service with source or destination location profile param equal to {string.Join(" or ", validSourceOrDestinationLocations)}");
					return true;
				}

				if (HasFunctionMatchingPlasmaSource(function, validPlasmaUserCodes))
				{
					helpers.Log(nameof(Order), nameof(UI_News), $"TRUE because order contains service with plasma user code profile param equal to {string.Join(" or ", validPlasmaUserCodes)}");
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets a boolean indicating if this order is visible on TOM Studio Helsinki page.
		/// </summary>
		public bool UI_Sho(Helpers helpers)
		{
			if (AllServices == null) return false;

			if (Subtype == OrderSubType.Vizrem)
			{
				return AllServices.Exists(s => s.Definition.Id == ServiceDefinitionGuids.VizremStudioHelsinki);
			}

			var validSourceOrDestinationLocations = new[] { "Studio Helsinki", "Studio Helsinki UT" };
			var validPlasmaUserCodes = new[] { "SHO1", "SHO2", "SHO3", "SHO4", "SHO5" };

			foreach (var service in AllServices)
			{
				foreach (var function in service.Functions.Where(f => f != null))
				{
					var sourceOrDestinationLocation = function.Parameters.FirstOrDefault(p => p != null && (p.Id == ProfileParameterGuids.FixedLineYleHelsinkiSourceLocation || p.Id == ProfileParameterGuids.YleHelsinkiDestinationLocation));

					if (sourceOrDestinationLocation != null && validSourceOrDestinationLocations.Contains(sourceOrDestinationLocation.StringValue))
					{
						helpers.Log(nameof(Order), nameof(UI_Sho), $"TRUE because order contains service with source or destination location profile param equal to {string.Join(" or ", validSourceOrDestinationLocations)}");
						return true;
					}

					if (HasFunctionMatchingPlasmaSource(function, validPlasmaUserCodes))
					{
						helpers.Log(nameof(Order), nameof(UI_Sho), $"TRUE because order contains service with plasma user code profile param equal to {string.Join(" or ", validPlasmaUserCodes)}");
						return true;
					}
				}
			}

			helpers.Log(nameof(Order), nameof(UI_Sho), $"FALSE because no conditions met");

			return false;
		}

		/// <summary>
		/// Gets a boolean indicating if this order is visible on TOM Mediapolis page.
		/// </summary>
		public bool UI_Tre(Helpers helpers)
		{
			var validDefinitionDescriptions = new[] { "YLE Mediapolis", "LiveU Mediapolis", "Studio Mediapolis" };
			var validPlasmaUserCodes = new[] { "TRE 01" };

			if (AllServices.Exists(s => s != null && validDefinitionDescriptions.Contains(s.Definition.Description)))
			{
				helpers.Log(nameof(Order), nameof(UI_Tre), $"TRUE because order contains service with definition description equal to {string.Join(" or ", validDefinitionDescriptions)}");
				return true;
			}

			foreach (var service in AllServices)
			{
				foreach (var function in service.Functions.Where(f => f != null))
				{
					if (HasFunctionMatchingPlasmaSource(function, validPlasmaUserCodes))
					{
						helpers.Log(nameof(Order), nameof(UI_Tre), $"TRUE because order contains service with plasma user code profile param equal to {string.Join(" or ", validPlasmaUserCodes)}");
						return true;
					}
				}
			}

			helpers.Log(nameof(Order), nameof(UI_Tre), $"FALSE because no conditions met");

			return false;
		}

		/// <summary>
		/// Gets a boolean indicating if this order is visible on TOM SVENSKA page.
		/// </summary>
		public bool UI_Svenska(Helpers helpers)
		{
			var orderServices = AllServices;
			if (orderServices == null) return false;

			var validSourceOrDestinationLocations = new[] { "Shohub" };
			var validPlasmaUserCodes = new[] { "SHOHUB" };

			foreach (var service in orderServices)
			{
				foreach (var function in service.Functions.Where(f => f != null))
				{
					var sourceOrDestinationLocation = function.Parameters.FirstOrDefault(p => p != null && (p.Id == ProfileParameterGuids.FixedLineYleHelsinkiSourceLocation || p.Id == ProfileParameterGuids.YleHelsinkiDestinationLocation));

					if (sourceOrDestinationLocation != null && validSourceOrDestinationLocations.Contains(sourceOrDestinationLocation.StringValue))
					{
						helpers.Log(nameof(Order), nameof(UI_Svenska), $"TRUE because order contains service with source or destination location profile param equal to {string.Join(" or ", validSourceOrDestinationLocations)}");
						return true;
					}

					if (HasFunctionMatchingPlasmaSource(function, validPlasmaUserCodes))
					{
						helpers.Log(nameof(Order), nameof(UI_Svenska), $"TRUE because order contains service with plasma user code profile param equal to {string.Join(" or ", validPlasmaUserCodes)}");
						return true;
					}
				}
			}

			helpers.Log(nameof(Order), nameof(UI_Svenska), $"FALSE because no conditions met");

			return false;
		}

		/// <summary>
		/// The order will be shown on the TOM News page if the integration type is Plasma.
		/// </summary>
		public bool UI_Plasma(Helpers helpers)
		{
			bool orderIsPlasma = IntegrationType == IntegrationType.Plasma;

			helpers.Log(nameof(Order), nameof(UI_Plasma), $"{orderIsPlasma.ToString().ToUpper()}={nameof(orderIsPlasma)}");

			return orderIsPlasma;
		}

		/// <summary>
		/// The order will be shown on the TOM News page if it contains one or more recording services.
		/// </summary>
		public bool UI_Messi_News_Rec(Helpers helpers)
		{
			bool orderContainsNewsService = AllServices.Exists(s => s?.Definition != null && !string.IsNullOrEmpty(s.Definition.Description) && s.Definition.Description.Contains("News"));

			helpers.Log(nameof(Order), nameof(UI_Messi_News_Rec), $"{orderContainsNewsService.ToString().ToUpper()}={nameof(orderContainsNewsService)}");

			return orderContainsNewsService;
		}

		/// <summary>
		/// Gets a boolean indicating if this order is visible on MCR Operator Task List page.
		/// </summary>
		public bool UI_McrChange(Helpers helpers)
		{
			if (IsPlannedUnknownSource())
			{
				helpers.Log(nameof(Order), nameof(UI_McrChange), $"TRUE because order status is {Status.PlannedUnknownSource.GetDescription()}");
				return true;
			}

			if (SourceServiceIsSatelliteWithIRDXXX())
			{
				//[DCP201966]
				helpers.Log(nameof(Order), nameof(UI_McrChange), $"TRUE because SAT RX uses IRD XXX");
				return true;
			}

			if (IsSavedOrContainsNonBookedServicesOrOrderPlannedMoreThanOneWeek())
			{
				helpers.Log(nameof(Order), nameof(UI_McrChange), $"FALSE because order is saved or ((order is Plasma or Feenix) and (order contains non-booked services or starts in more than 1 week))");
				return false;
			}

			if (IsCompletedWithErrorsAndHasOperatorNotes() || IsFileProcessingPastEndWithPostRollAndIncompleteUserTask())
			{
				helpers.Log(nameof(Order), nameof(UI_McrChange), $"FALSE because order is completed with errors and has operator notes or has file-processing status and EndWithPostRoll is already in the past");
				return false;
			}

			if (IsPlannedOrChangeRequested() || ContainsNonRecordingServicesWithResourceOverbookedOrServiceCompletedWithErrors())
			{
				helpers.Log(nameof(Order), nameof(UI_McrChange), $"TRUE because order is Planned or Change Requested, or contains non-recording services with status Resource Overbooked or Service Completed with Errors");
				return true;
			}

			if (IsOrderStartedWithIncompleteUserTasks())
			{
				helpers.Log(nameof(Order), nameof(UI_McrChange), $"TRUE because order started with incomplete user tasks");
				return true;
			}

			if (IntegrationType != IntegrationType.Plasma && ContainsSatelliteRXServiceWithSatelliteSpaceNeededWithin3Days())
			{
				helpers.Log(nameof(Order), nameof(UI_McrChange), $"TRUE because order contains Sat RX service with incomplete user task 'Satellite Space Needed' and starts within 3 days");
				return true;
			}

			if (IntegrationType != IntegrationType.Plasma && ContainsDummyEBUServiceWithSelectTechnicalSystemTask())
			{
				helpers.Log(nameof(Order), nameof(UI_McrChange), $"TRUE because order contains dummy EBU service with user task 'Select Technical System'");
				return true;
			}

			if (IntegrationType != IntegrationType.Plasma && ContainsFixedLineNordicRingAndIsPlanned())
			{
				helpers.Log(nameof(Order), nameof(UI_McrChange), $"TRUE because order contains Fixed Line Nordic ring and is planned");
				return true;
			}

			helpers.Log(nameof(Order), nameof(UI_McrChange), $"FALSE because no conditions are met");
			return false;
		}

		private bool IsPlannedUnknownSource()
		{
			return Status == Status.PlannedUnknownSource;
		}

		private bool SourceServiceIsSatelliteWithIRDXXX()
		{
			return SourceService.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite &&
				   SourceService.Functions.Select(f => f.ResourceName).Any(resourceName => resourceName.Contains("IRD XXX"));
		}

		private bool IsSavedOrContainsNonBookedServicesOrOrderPlannedMoreThanOneWeek()
		{
			bool containsOrderNonBookedServices = AllServices.Exists(s => s != null && !s.IsBooked) && IntegrationType == IntegrationType.Plasma;
			bool orderPlannedMoreThanOneWeek = AllServices.Exists(s => s != null && s.StartWithPreRoll.ToLocalTime().Subtract(DateTime.Now) > new TimeSpan(days: 7, hours: 0, minutes: 1, seconds: 0));

			return IsSaved || ((containsOrderNonBookedServices || orderPlannedMoreThanOneWeek) && (IntegrationType == IntegrationType.Plasma || IntegrationType == IntegrationType.Feenix));
		}

		private bool IsCompletedWithErrorsAndHasOperatorNotes()
		{
			return Status == Status.CompletedWithErrors && !String.IsNullOrWhiteSpace(McrOperatorNotes);
		}

		private bool IsFileProcessingPastEndWithPostRollAndIncompleteUserTask()
		{
			return Status == Status.FileProcessing && EndWithPostRoll <= DateTime.Now &&
				   AllServices.Exists(s => s != null && s.UserTasks.Exists(u => u != null && u.Status == UserTaskStatus.Incomplete && u.Description.IndexOf("File Processing", StringComparison.OrdinalIgnoreCase) != -1));
		}

		private bool IsPlannedOrChangeRequested()
		{
			return Status == Status.Planned || Status == Status.ChangeRequested;
		}

		private bool ContainsNonRecordingServicesWithResourceOverbookedOrServiceCompletedWithErrors()
		{
			return AllServices.Exists(s => s.Definition?.VirtualPlatform != VirtualPlatform.Recording &&
											(s.Status == YLE.Service.Status.ResourceOverbooked ||
											 (s.Status == YLE.Service.Status.ServiceCompletedWithErrors && String.IsNullOrWhiteSpace(ErrorDescription))));
		}

		private bool IsOrderStartedWithIncompleteUserTasks()
		{
			bool orderIsStartedWithIncompletedUsertasks = DateTime.Now >= StartWithPreRoll &&
														   AllServices.Exists(s => s.UserTasks != null && s.UserTasks.Exists(u => u.Status == UserTaskStatus.Incomplete));

			return orderIsStartedWithIncompletedUsertasks;
		}

		private bool ContainsSatelliteRXServiceWithSatelliteSpaceNeededWithin3Days()
		{
			return IntegrationType != IntegrationType.Plasma &&
				   AllServices.Exists(s => s.Definition?.VirtualPlatform == VirtualPlatform.ReceptionSatellite &&
										   s.Start - DateTime.Now < new TimeSpan(hours: 72, minutes: 1, seconds: 0) &&
										   s.UserTasks != null &&
										   s.UserTasks.Exists(u => u.Name.Contains(Descriptions.SatelliteReception.SpaceNeeded) && u.Status == UserTaskStatus.Incomplete));
		}

		private bool ContainsDummyEBUServiceWithSelectTechnicalSystemTask()
		{
			return IntegrationType != IntegrationType.Plasma &&
				   AllServices.Exists(s => (s.IsEbuDummyReception || s.IsEbuDummyTransmission) &&
										   s.UserTasks != null &&
										   s.UserTasks.Exists(u => u.Name.Contains(Descriptions.Dummy.SelectTechnicalSystem)));
		}

		private bool ContainsFixedLineNordicRingAndIsPlanned()
		{
			return IntegrationType != IntegrationType.Plasma &&
				   AllServices.Exists(s => s.Functions.Any(f => f.Parameters.Any(p => (p.Id == ProfileParameterGuids.FixedLineEbuSourceLocation || p.Id == ProfileParameterGuids.EbuDestinationLocation) && p.StringValue == "Nordic Ring"))) &&
				   Status == Status.Planned;
		}

		/// <summary>
		/// Gets a boolean indicating if this order is visible on Booking Office Task List page.
		/// </summary>
		public bool UI_Office(Helpers helpers)
		{
			if (AllServices.Exists(s => s.Definition?.VirtualPlatformServiceName == VirtualPlatformName.Satellite && s.UserTasks != null && s.UserTasks.Exists(u => u.Name.Contains(Descriptions.SatelliteReception.SpaceNeeded) && u.Status == UserTaskStatus.Incomplete)))
			{
				helpers.Log(nameof(Order), nameof(UI_Office), $"TRUE because order contains Sat RX service with incomplete user task 'Satellite Space Needed'");

				return true; // All orders which include RX Service (Sat RX or Backup Sat RX) related user task: Satellite space needed
			}

			if (AllServices.Exists(s => s.Definition?.VirtualPlatform == VirtualPlatform.ReceptionFiber && s.UserTasks != null && s.UserTasks.Exists(u => u.Name.Contains(Descriptions.FiberReception.AllocationNeeded) && u.Status == UserTaskStatus.Incomplete)))
			{
				helpers.Log(nameof(Order), nameof(UI_Office), $"TRUE because order contains Fiber RX service with incomplete user task 'Fiber Allocation Needed'");

				return true; // All orders which include RX service (ad hoc fiber RX or backup Ad Hoc Fiber RX) related user task: fiber allocation needed
			}

			if (AllServices.Exists(s => s.Definition?.VirtualPlatform == VirtualPlatform.ReceptionMicrowave && s.UserTasks != null && s.UserTasks.Exists(u => u.Name.Contains(Descriptions.MicrowaveReception.EquipmentAllocation) && u.Status == UserTaskStatus.Incomplete)))
			{
				helpers.Log(nameof(Order), nameof(UI_Office), $"TRUE because order contains Microwave RX service with incomplete user task 'Microwave Equipment Allocation Needed'");

				return true; // All orders which include RX Service (MW link RX or backup MW RX) related user task: MW equipment allocation needed
			}

			helpers.Log(nameof(Order), nameof(UI_Office), $"FALSE because no conditions are met");

			return false;
		}

		/// <summary>
		/// Gets a boolean indicating if this order is visible on MCR Specialist Task List page.
		/// </summary>
		public bool UI_McrSpecialist(Helpers helpers)
		{
			if (Status == Status.Completed || Status == Status.CompletedWithErrors)
			{
				helpers.Log(nameof(Order), nameof(UI_McrSpecialist), $"FALSE because order status is Completed or Completed with Errors");
				return false;
			}

			if (AllServices.Exists(s => s.Definition?.VirtualPlatformServiceName == VirtualPlatformName.Fiber && s.UserTasks != null && s.UserTasks.Exists(u => (u.Name.Contains(Descriptions.FiberReception.AllocationNeeded) || u.Name.Contains(Descriptions.FiberReception.EquipmentAllocation) || u.Name.Contains(Descriptions.FiberTransmission.Configure)) && u.Status == UserTaskStatus.Incomplete)))
			{
				helpers.Log(nameof(Order), nameof(UI_McrSpecialist), $"TRUE because order contains Fiber service with incomplete user tasks");

				return true; // All orders which include (backup) Ad Hoc Fiber RX related user tasks
			}

			if (AllServices.Exists(s => s.Definition?.VirtualPlatformServiceName == VirtualPlatformName.Microwave && s.UserTasks != null && s.UserTasks.Exists(u => (u.Name.Contains(Descriptions.MicrowaveReception.EquipmentAllocation) || u.Name.Contains(Descriptions.MicrowaveReception.EquipmentConfiguration) || u.Name.Contains(Descriptions.MicrowaveTransmission.Configure)) && u.Status == UserTaskStatus.Incomplete)))
			{
				helpers.Log(nameof(Order), nameof(UI_McrSpecialist), $"TRUE because order contains Microwave service with incomplete user tasks");

				return true; // All orders which include (backup) Microwave RX related user tasks
			}

			if (AllServices.Exists(s => s.Definition?.VirtualPlatform == VirtualPlatform.TransmissionIp && s.UserTasks != null && s.UserTasks.Exists(u => u.Status == UserTaskStatus.Incomplete)))
			{
				helpers.Log(nameof(Order), nameof(UI_McrSpecialist), $"TRUE because order contains IP TX service with incomplete user tasks");

				return true; // All orders which include RX SERVICE (IP Transmission) 
			}

			helpers.Log(nameof(Order), nameof(UI_McrSpecialist), $"FALSE because no conditions are met");

			return false;
		}

		/// <summary>
		/// Gets a boolean indicating if this order is visible on Media Operator Task List page.
		/// </summary>
		public bool UI_Recording(Helpers helpers)
		{
			if (IsSaved || (ContainsOrderNonBookedServices() || OrderPlannedMoreThanOneWeek()) && OrderIsPlasmaOrFeenix())
			{
				helpers.Log(nameof(Order), nameof(UI_Recording), $"FALSE because order is saved or ((order is Plasma or Feenix) and (order contains non-booked services or starts in more than 1 week))");
				return false; //When non-plasma orders are saved or where plasma/feenix orders contain one or more services where the start time with preroll is larger than 7 days, with other words services that aren't booked yet.
			}

			if (IsOrderCompletedOrCompletedWithErrors())
			{
				helpers.Log(nameof(Order), nameof(UI_Recording), $"FALSE because order is Completed or Completed with Errors (with operator notes)");
				return false;
			}

			if (ContainsRecordingServiceCompletedWithErrorWithoutNotes()) return true;
			if (ContainsRecordingServiceInResourceOverbookedStatus()) return true;
			if (IsPlasmaOrderContainsPlasmaLiveNewsService()) return true;
			if (ContainsNonRealRecordingFileTimeCodec()) return true;
			if (ContainsRecordingServiceWithSubRecordings()) return true;
			if (IntegrationType == IntegrationType.None && IsOrderCompletedWithTwoRecordingServices()) return true;

			if (IsPlasmaOrderWithDayChangeAndSubtitleProxy())
			{
				helpers.Log(nameof(Order), nameof(UI_Recording), $"TRUE because order is Plasma and contains 1 or more recording services of which one or more is day-change and has Subtitle Proxy set to true");
				return true;
			}

			helpers.Log(nameof(Order), nameof(UI_Recording), $"FALSE because no conditions are met");
			return false;
		}

		private bool ContainsOrderNonBookedServices()
		{
			return AllServices.Exists(s => s != null && !s.IsBooked) && IntegrationType == IntegrationType.Plasma;
		}

		private bool OrderPlannedMoreThanOneWeek()
		{
			return AllServices.Exists(s => s != null && s.StartWithPreRoll.ToLocalTime().Subtract(DateTime.Now) > new TimeSpan(days: 7, hours: 0, minutes: 1, seconds: 0));
		}

		private bool OrderIsPlasmaOrFeenix()
		{
			return IntegrationType == IntegrationType.Plasma || IntegrationType == IntegrationType.Feenix;
		}

		private bool IsOrderCompletedOrCompletedWithErrors()
		{
			return Status == Status.Completed || (Status == Status.CompletedWithErrors && !String.IsNullOrWhiteSpace(MediaOperatorNotes));
		}

		private bool ContainsRecordingServiceCompletedWithErrorWithoutNotes()
		{
			return AllServices.Any(service => service?.Definition?.VirtualPlatform == VirtualPlatform.Recording && service.Status == YLE.Service.Status.ServiceCompletedWithErrors && String.IsNullOrWhiteSpace(MediaOperatorNotes));
		}

		private bool ContainsRecordingServiceInResourceOverbookedStatus()
		{
			return AllServices.Any(service => service?.Definition?.VirtualPlatform == VirtualPlatform.Recording && service.Status == YLE.Service.Status.ResourceOverbooked);
		}

		private bool IsPlasmaOrderContainsPlasmaLiveNewsService()
		{
			return IntegrationType == IntegrationType.Plasma && AllServices.Any(service => service != null && service.RecordingConfiguration?.IsPlasmaLiveNews == true);
		}

		private bool ContainsNonRealRecordingFileTimeCodec()
		{
			return AllServices.Any(service => service.RecordingConfiguration?.RecordingFileTimeCodec == TimeCodec.NonReal && IntegrationType != IntegrationType.Plasma);
		}

		private bool ContainsRecordingServiceWithSubRecordings()
		{
			return AllServices.Any(service => service.RecordingConfiguration?.SubRecordings != null && service.RecordingConfiguration.SubRecordings.Any() && IntegrationType != IntegrationType.Plasma);
		}

		private bool IsOrderCompletedWithTwoRecordingServices()
		{
			var recordingServiceCount = AllServices.Count(service => service.Definition?.VirtualPlatform == VirtualPlatform.Recording);
			return recordingServiceCount == 2 && IntegrationType == IntegrationType.None && Status == Status.Completed;
		}

		private bool IsPlasmaOrderWithDayChangeAndSubtitleProxy()
		{
			var recordingServiceCount = 0;
			bool recordingServiceHasSubtitleProxy = false;

			foreach (var service in AllServices)
			{
				bool isRecording = service.Definition?.VirtualPlatform == VirtualPlatform.Recording;
				if (!isRecording) continue;

				recordingServiceCount++;
				var status = service.Status;

				if (service.RecordingConfiguration?.SubtitleProxy == true && service.Start.Date != service.End.Date)
					recordingServiceHasSubtitleProxy = true;
			}

			return recordingServiceCount >= 1 && recordingServiceHasSubtitleProxy;
		}

		/// <summary>
		/// Gets a boolean indicating if this order is visible on MCR Operator, MCR operator task list and Media Operator Task List page.
		/// </summary>
		public bool UI_NewsArea(Helpers helpers)
		{
			if (Subtype == OrderSubType.Vizrem) return false;

			var orderServices = AllServices;

			bool orderContainsCommentaryAudio = Sources.Exists(s => s != null && s.Definition?.VirtualPlatform == VirtualPlatform.ReceptionCommentaryAudio);
			bool orderContainsMediapolisLiveU = Sources.Exists(s => s != null && s.Definition?.VirtualPlatform == VirtualPlatform.ReceptionLiveU && s.Definition?.Description != null && s.Definition.Description.Contains("Mediapolis"));
			bool orderContainsFixedPlasmaSource = HasFixedPlasmaSource;
			bool isUnknownSourceOrder = Status == Status.PlannedUnknownSource;

			bool orderUsesHmxRouting = orderServices.Any(s => s.IsHmxRouting);
			bool orderContainsServiceWithoutResources = orderServices.Any(s => s.Functions.Any(f => f.ResourceName == Constants.None));

			bool isSourceValid = !orderContainsCommentaryAudio && !orderContainsMediapolisLiveU && !orderContainsFixedPlasmaSource && !isUnknownSourceOrder;
			bool result = !orderUsesHmxRouting && !orderContainsServiceWithoutResources && isSourceValid;

			helpers.Log(nameof(Order), nameof(UI_NewsArea), $"{result.ToString().ToUpper()} because {nameof(orderUsesHmxRouting)}={orderUsesHmxRouting}, {nameof(orderContainsCommentaryAudio)}={orderContainsCommentaryAudio}, {nameof(orderContainsMediapolisLiveU)}={orderContainsMediapolisLiveU}, {nameof(orderContainsFixedPlasmaSource)}={orderContainsFixedPlasmaSource}, {nameof(isUnknownSourceOrder)}={isUnknownSourceOrder}, {nameof(orderContainsServiceWithoutResources)}={orderContainsServiceWithoutResources}");

			return result;
		}

		public bool IsMediaOperatorMessiNewsHiddenFilterNeeded(Helpers helpers)
		{
			return helpers.OrderManagerElement.DoesOrderMatchAutoMessiHideConfiguration(this);
		}

		/// <summary>
		/// Retrieving the audio return information from the source services.
		/// Only news users can fill in this field for every type of service.
		/// Other users will only see this field when LiveU source service is used.
		/// </summary>
		[ChangeTracked]
		public string AudioReturnInfo
		{
			get
			{
				if (AllServices == null) return String.Empty;

				List<string> audioReturnInfos = new List<string>();
				foreach (var sourceService in Sources)
				{
					if (sourceService == null) continue;

					if (!String.IsNullOrWhiteSpace(sourceService.AudioReturnInfo))
					{
						audioReturnInfos.Add(sourceService.AudioReturnInfo);
					}
				}

				return string.Join(";", audioReturnInfos);
			}
		}

		/// <summary>
		/// Additional information for reception/transmission LiveU device name.
		/// Only applicable for LiveU receptions/transmissions.
		/// Will be saved in a custom property
		/// </summary>
		[ChangeTracked]
		public string LiveUDeviceNames
		{
			get
			{
				var orderServices = AllServices;
				if (orderServices == null) return String.Empty;

				List<string> liveUDeviceNamesToAdd = new List<string>();
				foreach (var service in orderServices)
				{
					if (service == null) continue;

					bool isServiceLiveUReceptionOrTransmission = service.Definition?.VirtualPlatform == VirtualPlatform.ReceptionLiveU || service.Definition?.VirtualPlatform == VirtualPlatform.TransmissionLiveU;
					if (isServiceLiveUReceptionOrTransmission && !String.IsNullOrEmpty(service.LiveUDeviceName))
					{
						liveUDeviceNamesToAdd.Add(service.LiveUDeviceName);
					}
				}

				return String.Join(";", liveUDeviceNamesToAdd);
			}
		}

		/// /// <summary>
		/// Returns a dot comma separated list containing the Vidigo stream source links if the order contains Messi News Recordings.
		/// </summary>
		[ChangeTracked]
		public string VidigoStreamSourceLinks
		{
			get
			{
				var vidigoStreamSourceLinks = AllServices.Where(s => !string.IsNullOrWhiteSpace(s.VidigoStreamSourceLink)).Select(s => s.VidigoStreamSourceLink).ToList();

				return string.Join(";", vidigoStreamSourceLinks);
			}
		}

		/// <summary>
		/// Returns a dot comma separated list containing all existing plasma ids for archiving from all live recording services within this order.
		/// </summary>
		[ChangeTracked]
		public string PlasmaIdsForArchiving
		{
			get
			{
				var orderServices = AllServices;
				if (orderServices == null) return String.Empty;

				HashSet<string> plasmaIdsForArchiveToAdd = new HashSet<string>();
				foreach (var service in orderServices)
				{
					if (service == null) continue;

					bool isServiceLiveRecording = service.Definition?.VirtualPlatform == VirtualPlatform.Recording && service.Definition.Description != null && service.Definition.Description.Contains("Live");
					if (isServiceLiveRecording && service.RecordingConfiguration != null && !string.IsNullOrWhiteSpace(service.RecordingConfiguration.PlasmaIdForArchive))
					{
						plasmaIdsForArchiveToAdd.Add(service.RecordingConfiguration.PlasmaIdForArchive);
					}
				}

				return String.Join(";", plasmaIdsForArchiveToAdd);
			}
		}

		/// <summary>
		/// Returns a dot comma separated list containing the Short Descriptions of the Source Services.
		/// </summary>
		public string SourceDescriptions
		{
			get
			{
				if (Sources == null || !Sources.Any()) return String.Empty;

				return String.Join(";", Sources.Where(x => x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception).Select(x => x.GetShortDescription(this).Clean(true)));
			}
		}

		/// <summary>
		/// Returns a dot comma separated list containing the Short Descriptions of the Destination Services.
		/// </summary>
		public string DestinationDescriptions
		{
			get
			{
				List<string> resultDescriptions = new List<string>();

				var otherServicesResourceNames = GetResourceNamesForRecordingsAndTransmissions();
				var destinationShortDescriptions = AllServices.Where(x => x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Destination).Select(x => x.GetShortDescription(this).Clean(true)).ToList();

				foreach (var umxLineResourceName in otherServicesResourceNames)
				{
					bool noDestinationsHaveASimilarShortDescription = !destinationShortDescriptions.Any(x => x.Replace(" ", string.Empty).Equals(umxLineResourceName, StringComparison.InvariantCultureIgnoreCase));
					// e.g.: destination resource is UMX 12 and recording routing output resource is UMX13, then add UMX13

					if (noDestinationsHaveASimilarShortDescription)
					{
						resultDescriptions.Add(umxLineResourceName);
					}
				}

				resultDescriptions.AddRange(destinationShortDescriptions);

				return String.Join(";", resultDescriptions.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());
			}
		}

		/// <summary>
		/// Returns a dot comma separated list containing the Short Descriptions of the Recording Services.
		/// </summary>
		public string RecordingDescriptions
		{
			get
			{
				return String.Join(";", AllServices.Where(x => x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Recording).Select(x => x.GetShortDescription(this).Clean(true)));
			}
		}

		/// <summary>
		/// Returns a dot comma separated list containing the Short Descriptions of the Transmission Services.
		/// </summary>
		public string TransmissionDescriptions
		{
			get
			{
				return String.Join(";", AllServices.Where(x => x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Transmission).Select(x => x.GetShortDescription(this).Clean(true)));
			}
		}

		public string GetProcessingDescription()
		{
			var stringBuilder = new StringBuilder();

			var audioProcessing = AllServices.Where(service => service.Definition.VirtualPlatform.Equals(VirtualPlatform.AudioProcessing));

			foreach (var process in audioProcessing)
			{
				var audioDeembeddingRequired = process.Functions.SelectMany(f => f.Parameters).Single(param => param.Id.Equals(ProfileParameterGuids.AudioDeembeddingRequired));
				if (audioDeembeddingRequired.StringValue.Equals("Yes")) stringBuilder.Append(";Audio Deembedding");

				var audioEmbeddingRequired = process.Functions.SelectMany(f => f.Parameters).Single(param => param.Id.Equals(ProfileParameterGuids.AudioEmbeddingRequired));
				if (audioEmbeddingRequired.StringValue.Equals("Yes")) stringBuilder.Append(";Audio Embedding");

				var audioShufflingRequired = process.Functions.SelectMany(f => f.Parameters).Single(param => param.Id.Equals(ProfileParameterGuids.AudioShufflingRequired));
				if (audioShufflingRequired.StringValue.Equals("Yes")) stringBuilder.Append(";Audio Shuffling");
			}

			var videoProcessingServices = AllServices.Where(service => service.Definition.VirtualPlatform.Equals(VirtualPlatform.VideoProcessing));

			foreach (var videoProcessingService in videoProcessingServices)
			{
				string inputValue = videoProcessingService.Functions.SelectMany(f => f.Parameters).Single(param => param.Id.Equals(ProfileParameterGuids.InputVideoFormat)).StringValue;
				string outputValue = videoProcessingService.Functions.SelectMany(f => f.Parameters).Single(param => param.Id.Equals(ProfileParameterGuids.OutputVideoFormat)).StringValue;

				string inputValueLastThreeCharacters = inputValue.Substring(inputValue.Length - 3);
				string outputValueLastThreeCharacters = outputValue.Substring(outputValue.Length - 3);


				int inputValueFirstPart = Convert.ToInt32(inputValue.Remove(inputValue.Length - 3));
				int outputValueFirstPart = Convert.ToInt32(outputValue.Remove(outputValue.Length - 3));

				if (inputValueLastThreeCharacters != outputValueLastThreeCharacters)
					stringBuilder.Append(";Video FormatConversion");

				else if (inputValueFirstPart != outputValueFirstPart)
					stringBuilder.Append(";Video ResolutionConversion");
				else
				{
					// do nothing
				}
			}

			var isGraphicsProcessing = AllServices.Exists(service => service.Definition.VirtualPlatform.Equals(VirtualPlatform.GraphicsProcessing));
			if (isGraphicsProcessing) stringBuilder.Append(";Graphics Conversion");

			string result = (stringBuilder.Length != 0) ? Convert.ToString(stringBuilder).Remove(0, 1) : Convert.ToString(stringBuilder);

			return result;
		}


		public IEnumerable<FunctionResource> GetAllServiceResources(Guid? serviceToIgnoreId = null, string functionToIgnoreLabel = null)
		{
			return AllServices.Where(s => s.Id != serviceToIgnoreId).SelectMany(s => s.Functions).Where(f => f.Definition.Label != functionToIgnoreLabel && f.Resource != null).Select(f => f.Resource).ToList();
		}

		public IEnumerable<FunctionResource> GetAllServiceResourcesOverlappingWith(Service service)
		{
			return AllServices.Except(new[] { service }).Where(s => s.StartWithPreRoll < service.EndWithPostRoll && service.StartWithPreRoll < s.EndWithPostRoll).SelectMany(s => s.Functions).Where(f => f.Resource != null).Select(f => f.Resource).ToList();
		}

		public static List<Service> FlattenServices(IEnumerable<Service> services)
		{
			var flattenedServices = new List<Service>();
			foreach (var service in services)
			{
				flattenedServices.Add(service);
				flattenedServices.AddRange(FlattenServices(service.Children));
			}

			return flattenedServices;
		}

		/// <summary>
		/// Executed from all scripts that are used to create or update orders.
		/// </summary>
		/// <returns>The list of srm tasks that were executed.</returns>
		public UpdateResult AddOrUpdate(Helpers helpers, bool isMcrUser, OrderUpdateHandler.OptionFlags options = OrderUpdateHandler.OptionFlags.None, bool processChronologically = false, Order existingOrder = null)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			var input = new OrderUpdateHandlerInput
			{
				IsHighPriority = isMcrUser,
				ProcessChronologically = processChronologically,
				Options = options,
				ExistingOrder = existingOrder,
			};

			var orderAddOrUpdater = new OrderAddOrUpdateHandler(helpers, this, input);

			return orderAddOrUpdater.Execute();
		}

		/// <summary>
		/// Is used to update specific custom properties of an existing order and service.
		/// </summary>
		/// <returns>The list of SRM tasks that were executed.</returns>
		public IEnumerable<Task> GetUpdateCustomPropertiesWhenIssueReportedManuallyTasks(Helpers helpers, Service service)
		{
			var tasks = new List<Task>();

			var orderCustomPropertiesUpdater = new OrderReportErrorHandler(helpers, this);
			var serviceCustomPropertyUpdater = new ServiceReportErrorHandler(helpers, this, service);

			serviceCustomPropertyUpdater.Execute(out var serviceTasks, out bool createUserTaskForThisService);
			tasks.AddRange(serviceTasks);
			tasks.AddRange(orderCustomPropertiesUpdater.Execute().Tasks);

			return tasks;
		}

		/// <summary>
		/// Retrieving all linked file attachments from this order.
		/// </summary>
		public List<string> GetAttachments(IEngine engine, string path)
		{
			string[] filePaths = new string[0];

			try
			{
				string fullPath = Path.Combine(path, Convert.ToString(Id));
				if (Directory.Exists(fullPath)) filePaths = Directory.GetFiles(fullPath);
			}
			catch (Exception e)
			{
				engine.Log($"Something went wrong during while collecting the order files: " + e);
			}

			return filePaths.ToList();
		}

		public List<string> GetAttachments(Helpers helpers)
		{
			try
			{
				string fullPath = Path.Combine(OrderManager.OrderAttachmentsDirectory, Convert.ToString(Id));
				if (Directory.Exists(fullPath))
				{
					return new List<string>(Directory.GetFiles(fullPath));
				}
				else
				{
					return new List<string>();
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Order), nameof(GetAttachments), $"Exception occurred: {e}");
				return new List<string>();
			}
		}

		/// <summary>
		/// Create all bookings for the services needed in this order.
		/// </summary>
		public UpdateResult BookServices(Helpers helpers)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			var orderBookServicesHandler = new OrderBookServicesHandler(helpers, this);

			return orderBookServicesHandler.Execute();
		}

		public UpdateResult StopOrderAndLinkedServices(Helpers helpers)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			var orderStopHandler = new OrderStopHandler(helpers, this);

			return orderStopHandler.Execute();
		}

		public UpdateResult BookEventLevelReceptionServices(Helpers helpers)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			var orderBookEventLevelReceptions = new OrderBookEventLevelReceptionsHandler(helpers, this);

			helpers.Log(nameof(Order), nameof(BookEventLevelReceptionServices), "Start booking event level receptions", Name);

			return orderBookEventLevelReceptions.Execute();
		}

		/// <summary>
		/// Remove all booked services from this order.
		/// </summary>
		/// <param name="helpers">The Helpers object.</param>
		public bool RemoveAllBookedServices(Helpers helpers)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			var orderRemoveBookedServicesHandler = new OrderRemoveBookedServicesHandler(helpers, this);

			var result = orderRemoveBookedServicesHandler.Execute();

			var isSuccessful = true;
			foreach (var task in result.Tasks)
			{
				if (task.Status == Tasks.Status.Fail)
				{
					isSuccessful = false;

					helpers.Log(nameof(Order), nameof(RemoveAllBookedServices), $"Task {task.Description} failed: {task.Exception}");
				}
			}

			return isSuccessful;
		}

		public bool RemoveBookedServices(Helpers helpers, List<Service> servicesToRemove)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (servicesToRemove == null) throw new ArgumentNullException(nameof(servicesToRemove));

			var orderRemoveBookedServicesHandler = new OrderRemoveBookedServicesHandler(helpers, this, servicesToRemove);

			var result = orderRemoveBookedServicesHandler.Execute();

			var isSuccessful = true;
			foreach (var task in result.Tasks)
			{
				if (task.Status == Tasks.Status.Fail)
				{
					isSuccessful = false;
					helpers.Log(nameof(Order), nameof(RemoveBookedServices), $"Task {task.Description} failed: {task.Exception}");
				}
			}

			return isSuccessful;
		}

		public bool UpdateUiProperties(Helpers helpers)
		{
			if (Id == Guid.Empty) return false;

			helpers.LogMethodStart(nameof(Order), nameof(UpdateUiProperties), out var stopwatch);

			Reservation = helpers.ReservationManager.GetReservation(Id);
			if (Reservation == null)
			{
				helpers.Log(nameof(Order), nameof(UpdateUiProperties), $"Unable to retrieve reservation {Id}");
				helpers.LogMethodCompleted(nameof(Order), nameof(UpdateUiProperties), null, stopwatch);
				return false;
			}

			try
			{
				bool showNewsAreaOrderOnMediaOperatorTaskList = UI_NewsArea(helpers) && (Status == Status.ChangeRequested || Status == Status.Planned);
				helpers.Log(nameof(Order), nameof(UpdateUiProperties), $"{nameof(showNewsAreaOrderOnMediaOperatorTaskList)}={showNewsAreaOrderOnMediaOperatorTaskList}");

				var customUiProperties = new Dictionary<string, object>
				{
					{ PropertyNameUiAreena, UI_Areena(helpers).ToString() },
					{ PropertyNameUiTomNews, UI_News(helpers).ToString() },
					{ PropertyNameUiTomSho, UI_Sho(helpers).ToString() },
					{ PropertyNameUiTomTre, UI_Tre(helpers).ToString() },
					{ PropertyNameUiTomSvenska, UI_Svenska(helpers).ToString() },
					{ PropertyNameUiPlasma, UI_Plasma(helpers).ToString() },
					{ PropertyNameUiMessiNewsRec, UI_Messi_News_Rec(helpers).ToString() },
					{ PropertyNameUiMcrChange, UI_McrChange(helpers).ToString() },
					{ PropertyNameUiOffice, UI_Office(helpers).ToString() },
					{ PropertyNameUiMcrSpecialist, UI_McrSpecialist(helpers).ToString() },
					{ PropertyNameUiRecording, showNewsAreaOrderOnMediaOperatorTaskList ? true.ToString() : UI_Recording(helpers).ToString() }, // When news area property is true, the rule is to set UI_Recording to true as well.
                    { PropertyNameUiNewsArea, UI_NewsArea(helpers).ToString() },
					{ PropertyNameHiddenMessiNews, IsMediaOperatorMessiNewsHiddenFilterNeeded(helpers).ToString() },
					{ PropertyNameHiddenMessiLive, false.ToString() }, // Currently not in use, will be the case in a later stage
                    { PropertyNameUiTeamMessiLive, UI_Team_Messi_Live(helpers).ToString() },
					{ PropertyNameUiTeamMessiNews, UI_Team_Messi_News(helpers).ToString() },
					{ PropertyNameSources, SourceDescriptions },
					{ PropertyNameDestinations, DestinationDescriptions },
					{ PropertyNameRecordings, RecordingDescriptions },
					{ PropertyNameTransmissions, TransmissionDescriptions },
					{ PropertyNameUiStudioHelsinkiUT, CheckOrderUT(helpers).ToString()},
					{ PropertyNameDestinationOrder, string.Join(",", GetOrderedEndpointServiceGuids(helpers))},
					{ PropertyNameMainSourceType, MapVirtualPlatformToUiValue(SourceService?.Definition?.VirtualPlatformServiceName) },
					{ PropertyNameTOMViewDescription, TOMViewDescription},
					{ PropertyNameVizremType, Subtype.GetDescription()},
					{ PropertyNameEngine, GetEngine() },
				};

				if (IntegrationType == IntegrationType.Plasma || IntegrationType == IntegrationType.Feenix)
				{
					double publicationStartAmountOfMillisecondsSince1970 = PublicationStart.ConvertToCustomDatetimePropertyForReservation();
					if (publicationStartAmountOfMillisecondsSince1970 > TimeSpan.FromDays(1).TotalMilliseconds)
					{
						customUiProperties.Add(PropertyNamePublicationStart, publicationStartAmountOfMillisecondsSince1970);
					}

					double publicationEndAmountOfMillisecondsSince1970 = PublicationEnd.ConvertToCustomDatetimePropertyForReservation();
					if (publicationEndAmountOfMillisecondsSince1970 > TimeSpan.FromDays(1).TotalMilliseconds)
					{
						customUiProperties.Add(PropertyNamePublicationEnd, publicationEndAmountOfMillisecondsSince1970);
					}
				}

				helpers.LogMethodCompleted(nameof(Order), nameof(UpdateUiProperties), null, stopwatch);

				return TryUpdateCustomProperties(helpers, customUiProperties);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Order), nameof(UpdateUiProperties), $"Exception occurred: {e}");
				helpers.LogMethodCompleted(nameof(Order), nameof(UpdateUiProperties), null, stopwatch);
				return false;
			}
		}

		private static string MapVirtualPlatformToUiValue(VirtualPlatformName? virtualPlatformName)
		{
			if (virtualPlatformName is null) return string.Empty;

			switch (virtualPlatformName)
			{
				case VirtualPlatformName.Satellite:
					return "RX_Sat";
				case VirtualPlatformName.LiveU:
					return "Live_U";
				default:
					return virtualPlatformName.GetDescription();
			}
		}

		public bool UpdateAttachmentCount(Helpers helpers)
		{
			using (MetricLogger.StartNew(helpers, nameof(Order)))
			{
				if (Id == Guid.Empty) return false;

				helpers.LogMethodStart(nameof(Order), nameof(UpdateAttachmentCount), out var stopwatch);

				Reservation = Reservation ?? helpers.ReservationManager.GetReservation(Id);
				if (Reservation == null)
				{
					helpers.Log(nameof(Order), nameof(UpdateAttachmentCount), $"Unable to retrieve reservation {Id}");
					return false;
				}

				int attachmentCount = GetAttachments(helpers).Count;
				var customUiProperties = new Dictionary<string, object>
				{
					{ PropertyNameAttachmentCount, attachmentCount },
				};

				return TryUpdateCustomProperties(helpers, customUiProperties);
			}
		}

		/// <summary>
		/// This method is used to directly updated the Service Configuration property on the ReservationInstance.
		/// </summary>
		/// <returns>True if update was successful.</returns>
		public bool UpdateServiceConfigurationProperty(Helpers helpers)
		{
			return helpers.OrderManagerElement.AddOrUpdateServiceConfigurations(Id, EndWithPostRoll, GetSerializedServiceConfigurations());
		}

		public Dictionary<string, object> GetChangedProperties(Helpers helpers, Order oldOrder, List<Library.Solutions.SRM.Model.Properties.Property> propertiesFromBookingManager)
		{
			helpers.LogMethodStart(nameof(Order), nameof(GetChangedProperties), out var stopwatch, Name);

			var changedProperties = new Dictionary<string, object>();

			var currentOrderProperties = GetPropertiesFromBookingManager(helpers, propertiesFromBookingManager).ToList();
			helpers.Log(nameof(Order), nameof(GetChangedProperties), $"Order has {currentOrderProperties.Count} properties", Name);

			if (oldOrder == null)
			{
				foreach (var currentOrderProperty in currentOrderProperties)
				{
					changedProperties.Add(currentOrderProperty.Name, currentOrderProperty.Value);
				}

				helpers.Log(nameof(Order), nameof(GetChangedProperties), "Order is new, all properties will be updated", Name);
			}
			else
			{
				var oldOrderProperties = oldOrder.GetPropertiesFromBookingManager(helpers, propertiesFromBookingManager).ToList();
				helpers.Log(nameof(Order), nameof(GetChangedProperties), $"Existing Order has {oldOrderProperties.Count} properties", Name);

				var propertiesThatAreNull = new List<Property>(); // for debugging/logging purposes
				var unchangedProperties = new List<Property>(); // for debugging/logging purposes

				foreach (var property in propertiesFromBookingManager)
				{
					var currentOrderProperty = currentOrderProperties.SingleOrDefault(p => p.Name == property.Name);
					var oldOrderProperty = oldOrderProperties.SingleOrDefault(p => p.Name == property.Name);

					if (currentOrderProperty == null || oldOrderProperty == null)
					{
						propertiesThatAreNull.Add(property);
						continue;
					}

					if (currentOrderProperty.Value != oldOrderProperty.Value)
					{
						changedProperties.Add(currentOrderProperty.Name, currentOrderProperty.Value);

						helpers.Log(nameof(Order), nameof(GetChangedProperties), $"Property '{currentOrderProperty.Name}' has changed from '{oldOrderProperty.Value}' to '{currentOrderProperty.Value}'", Name);
					}
					else if (property.Name == PropertyNameServiceConfigurations)
					{
						changedProperties.Add(currentOrderProperty.Name, currentOrderProperty.Value);

						helpers.Log(nameof(Order), nameof(GetChangedProperties), $"Force update property '{currentOrderProperty.Name}' with value '{currentOrderProperty.Value}'", Name);
					}
					else
					{
						unchangedProperties.Add(currentOrderProperty);
					}
				}

				helpers.Log(nameof(Order), nameof(GetChangedProperties), $"Properties {string.Join(", ", propertiesThatAreNull.Select(p => p.Name))} are null", Name);

				helpers.Log(nameof(Order), nameof(GetChangedProperties), $"Unchanged properties {string.Join(" ; ", unchangedProperties.Select(p => $"{p.Name}='{p.Value}'"))}", Name);
			}

			helpers.Log(nameof(Order), nameof(GetChangedProperties), $"Changed properties: {string.Join(";", changedProperties.Keys)}", Name);

			helpers.LogMethodCompleted(nameof(Order), nameof(GetChangedProperties), Name);

			return changedProperties;
		}


		public void StopOrderNow(Helpers helpers)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			if (!StopNow || !TryStopOrderNow(helpers))
			{
				helpers.Log(nameof(Order), nameof(StopOrderNow), $"StopNow property is {(StopNow ? "enabled" : "disabled")}", Name);
				throw new StopOrderFailedException(Name);
			}
		}

		public bool TryChangeResources(Helpers helpers, List<Service> services)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (services == null) throw new ArgumentNullException(nameof(services));

			helpers.LogMethodStart(nameof(Order), nameof(TryChangeResources), out var stopwatch, Name);

			try
			{
				var reservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id) as ServiceReservationInstance;

				var requests = services.Select(service => new AssignResourceRequest { TargetNodeLabel = service.NodeLabel, NewResourceId = service.ContributingResource?.ID ?? Guid.Empty }).ToList();

				helpers.Log(nameof(Order), nameof(TryChangeResources), "Setting resources to " + string.Join(";", services.Select(s => $"{s.NodeLabel}:{s.Name}")), Name);

				DataMinerInterface.ReservationInstance.AssignResources(helpers, reservationInstance, requests.ToArray());

				helpers.LogMethodCompleted(nameof(Order), nameof(TryChangeResources), Name, stopwatch);

				return true;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Order), nameof(TryChangeResources), $"Something went wrong: {e}", Name);
				helpers.LogMethodCompleted(nameof(Order), nameof(TryChangeResources), Name, stopwatch);
				return false;
			}
		}

		public Booking GetBookingDataForBooking(Helpers helpers)
		{
			var booking = new Booking
			{
				ConfigureResources = true,
				Description = Name,
				DesiredReservationStatus = DesiredReservationStatus.Confirmed,
				Recurrence = new Library.Solutions.SRM.Model.Recurrence
				{
					StartDate = Start, // Local Time
					PreRoll = PreRoll,
					EndDate = End, // Local Time
					PostRoll = PostRoll,
					SingleEvent = true
				},
				ServiceDefinition = Definition.Id.ToString(),
				Type = BookingType.SingleEvent,
				ExternalServiceManagement = true
			};

			helpers.Log(nameof(Order), nameof(GetBookingDataForBooking), $"Created Booking object with Description={Name}, ServiceDefinition={booking.ServiceDefinition}. PreRoll={booking.Recurrence.PreRoll}. Start={booking.Recurrence.StartDate.ToFullDetailString()}. End={booking.Recurrence.EndDate.ToFullDetailString()}. PostRoll={booking.Recurrence.PostRoll}");

			return booking;
		}

		public bool TryGenerateProcessingServices(Helpers helpers, ref List<Task> tasks)
		{
			helpers.Log(nameof(Order), nameof(TryGenerateProcessingServices), "Generate processing services task");

			var generateProcessingServicesTask = new GenerateProcessingServicesTask(helpers, this);
			tasks.Add(generateProcessingServicesTask);
			if (!generateProcessingServicesTask.Execute())
			{
				helpers.Log(nameof(Order), nameof(TryGenerateProcessingServices), "Generate processing services task failed");
				return false;
			}

			return true;
		}

		public bool TryGetServicesToRemove(Helpers helpers, ref List<Task> tasks, out List<Service> servicesToRemove)
		{
			helpers.Log(nameof(Order), nameof(TryGetServicesToRemove), "Get services to remove");

			var getServicesToRemoveTask = Task.CreateNew(helpers, () => helpers.OrderManager.GetServicesToRemove(this), "Getting services to remove");

			tasks.Add(getServicesToRemoveTask);

			if (!getServicesToRemoveTask.Execute())
			{
				helpers.Log(nameof(Order), nameof(TryGetServicesToRemove), "Get services to remove failed");

				servicesToRemove = new List<Service>();
				return false;
			}

			servicesToRemove = getServicesToRemoveTask.Result;
			return true;
		}

		public void CancelServices(Helpers helpers, List<Service> servicesToCancel, ref List<Task> tasks)
		{
			foreach (var service in servicesToCancel)
			{
				if (!service.IsBooked) return;

				helpers.Log(nameof(Order), nameof(CancelServices), "Service to cancel: " + service.Name);

				helpers.Log(nameof(Order), nameof(CancelServices), "Cancel service task");
				var cancelServiceTask = new CancelServiceTask(helpers, service, this);
				tasks.Add(cancelServiceTask);

				if (!cancelServiceTask.Execute()) helpers.Log(nameof(Order), nameof(CancelServices), "Cancel service task failed");
			}
		}

		internal event EventHandler<Service> ServiceChanged;

		public void ChangeOrderMainSourceService(Service existingSourceService, Service newService)
		{
			if (existingSourceService.Id != newService.Id)
			{
				int indexOfExistingService = Sources.IndexOf(existingSourceService);
				if (indexOfExistingService == -1) return;

				// Updating existing recording or transmission services if they are linked to the previous main source service.
				var orderServices = AllServices;
				var recordingAndTransmissionServices = orderServices.Where(s => s != null && (s.Definition.VirtualPlatformServiceType == VirtualPlatformType.Recording || s.Definition.VirtualPlatformServiceType == VirtualPlatformType.Transmission));

				foreach (var service in recordingAndTransmissionServices)
				{
					if (existingSourceService.Name == service.NameOfServiceToTransmitOrRecord)
					{
						service.NameOfServiceToTransmitOrRecord = newService.Name;
					}
				}

				Sources.RemoveAt(indexOfExistingService);
				Sources.Insert(indexOfExistingService, newService);
				newService.SetChildren(existingSourceService.Children);

				ServiceChanged?.Invoke(this, newService);
			}
		}

		public void ChangeOrderService(Service existingService, Service newService)
		{
			if (existingService.Id != newService.Id)
			{
				int indexOfExistingService = AllServices.IndexOf(existingService);
				if (indexOfExistingService == -1) return;

				// Updating existing recording or transmission services if they are linked to the previous service.
				var recordingAndTransmissionServices = AllServices.Where(s => s != null && (s.Definition.VirtualPlatformServiceType == VirtualPlatformType.Recording || s.Definition.VirtualPlatformServiceType == VirtualPlatformType.Transmission));

				foreach (var service in recordingAndTransmissionServices)
				{
					if (existingService.Name == service.NameOfServiceToTransmitOrRecord)
					{
						service.NameOfServiceToTransmitOrRecord = newService.Name;
					}
				}

				// Attaching parent and child services on new service.
				var parentService = AllServices.SingleOrDefault(s => s.Children.Contains(existingService));
				if (parentService != null)
				{
					int indexOfExistingChild = parentService.Children.IndexOf(existingService);
					if (indexOfExistingChild != -1)
					{
						parentService.Children.RemoveAt(indexOfExistingChild);
						parentService.Children.Insert(indexOfExistingChild, newService);
					}
				}

				AllServices.RemoveAt(indexOfExistingService);
				AllServices.Insert(indexOfExistingService, newService);
				newService.SetChildren(existingService.Children);

				ServiceChanged?.Invoke(this, newService);
			}
		}

		public bool TryChangeOrderEndTime(Helpers helpers)
		{
			var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(Definition.BookingManagerElementName));

			Reservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id) ?? throw new ReservationNotFoundException(Name);

			var changeOrderTimeInputData = new ChangeTimeInputData
			{
				StartDate = Reservation.Start.Add(Reservation.GetPreRoll()).FromReservation(),
				PreRoll = Reservation.GetPreRoll(),
				EndDate = End,
				PostRoll = PostRoll,
				IsSilent = true
			};

			try
			{
				helpers.Log(nameof(Order), nameof(TryChangeOrderEndTime), $"Changing order end timing from {Reservation.End.ToFullDetailString()} to {changeOrderTimeInputData.EndDate.ToFullDetailString()}", Name);

				Reservation = DataMinerInterface.BookingManager.ChangeTime(helpers, bookingManager, Reservation, changeOrderTimeInputData);
				return true;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Order), nameof(TryChangeOrderEndTime), $"Something went wrong while setting order end time from {Reservation.End.ToFullDetailString()} to {changeOrderTimeInputData.EndDate.ToFullDetailString()}: {e}", Name);
				return false;
			}
		}

		public bool TryChangeOrderPostRoll(Helpers helpers, TimeSpan postRollToSet)
		{
			var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(Definition.BookingManagerElementName));

			Reservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id) ?? throw new ReservationNotFoundException(Name);
			if (Reservation.Status == ReservationStatus.Ended)
			{
				helpers.Log(nameof(OrderManager), nameof(TryChangeOrderPostRoll), $"Order has already been ended for a while (Reservation status: {Reservation.Status.GetDescription()}), no timing nor post roll update allowed");
				return false;
			}

			var changeOrderTimeInputData = new ChangeTimeInputData
			{
				StartDate = Reservation.Start.Add(Reservation.GetPreRoll()), // WITHOUT pre roll
				PreRoll = Reservation.GetPreRoll(),
				EndDate = Reservation.End.Subtract(Reservation.GetPostRoll()), // WITHOUT post roll
				PostRoll = postRollToSet,
				IsSilent = true
			};

			try
			{
				Reservation = DataMinerInterface.BookingManager.ChangeTime(helpers, bookingManager, Reservation, changeOrderTimeInputData);
				helpers.Log(nameof(Order), nameof(TryChangeOrderPostRoll), $"Successfully changed order timing from {TimingInfoToString(true)} to {changeOrderTimeInputData.ToString()} (in UTC)", Name);
				return true;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Order), nameof(TryChangeOrderPostRoll), $"Something went wrong while setting order time from {TimingInfoToString(true)} to {changeOrderTimeInputData.ToString()} (in UTC): {e}", Name);
				return false;
			}
		}

		public override object Clone()
		{
			return new Order(this);
		}

		public IEnumerable<Property> GetPropertiesForBooking(Helpers helpers, IEnumerable<Property> propertiesFromBookingManager)
		{
			// Special method to exclude service configurations

			return GetPropertiesFromBookingManager(helpers, propertiesFromBookingManager).Where(p => !String.IsNullOrWhiteSpace(p.Name) && p.Name != PropertyNameServiceConfigurations); // service config is saved separately on order manager
		}

		[SuppressMessage("SonarCloud", "S3776", Justification = "Converting this switch case is to much work")]
		public IEnumerable<Property> GetPropertiesFromBookingManager(Helpers helpers, IEnumerable<Property> propertiesFromBookingManager)
		{
			/* WARNING
			 * Service Configurations property is no longer stored on order reservation but should still be returned in this method,
			 * because it is used to detect changes and to know what to update.
			 * Filter it out on a higher level where applicable. */

			var properties = new List<Property>();

			foreach (var property in propertiesFromBookingManager)
			{
				property.IsChecked = true;

				switch (property.Name)
				{
					case PropertyNameType:
						property.Value = Type.ToString();
						break;
					case PropertyNameIntegration:
						property.Value = IntegrationType.GetDescription();
						break;
					case PropertyNameStatus:
						property.Value = Status.GetDescription();
						break;
					case PropertyNameUsergroups:
						property.Value = $"'{String.Join("','", UserGroupIds)}'";
						break;
					case PropertyNameInternal:
						property.Value = IsInternal.ToString();
						break;
					case PropertyNamePlasmaId:
						property.Value = PlasmaId ?? String.Empty;
						break;
					case PropertyNameEditorialObjectId:
						property.Value = EditorialObjectId ?? String.Empty;
						break;
					case PropertyNameEurovisionId:
						property.Value = EurovisionWorkOrderId ?? String.Empty;
						break;
					case PropertyNameEurovisionTransmissionNumber:
						property.Value = EurovisionTransmissionNumber ?? String.Empty;
						break;
					case PropertyNameYleId:
						property.Value = YleId ?? String.Empty;
						break;
					case PropertyNameComments:
						property.Value = !string.IsNullOrWhiteSpace(Comments) ? Comments : string.Empty;
						break;
					case PropertyNameCustomer:
						property.Value = Company;
						break;
					case PropertyNameSportsplanningSport:
						property.Value = SportsPlanning.Sport ?? String.Empty;
						break;
					case PropertyNameSportsplanningDescr:
						property.Value = SportsPlanning.Description ?? String.Empty;
						break;
					case PropertyNameSportsplanningCommentary:
						property.Value = SportsPlanning.Commentary ?? String.Empty;
						break;
					case PropertyNameSportsplanningCommentary2:
						property.Value = SportsPlanning.Commentary2 ?? String.Empty;
						break;
					case PropertyNameSportsplanningCompetitionTime:
						property.Value = SportsPlanning.CompetitionTime.ToString();
						break;
					case PropertyNameSportsplanningJournalist1:
						property.Value = SportsPlanning.JournalistOne ?? String.Empty;
						break;
					case PropertyNameSportsplanningJournalist2:
						property.Value = SportsPlanning.JournalistTwo ?? String.Empty;
						break;
					case PropertyNameSportsplanningJournalist3:
						property.Value = SportsPlanning.JournalistThree ?? String.Empty;
						break;
					case PropertyNameSportsplanningLocation:
						property.Value = SportsPlanning.Location ?? String.Empty;
						break;
					case PropertyNameSportsplanningTechResources:
						property.Value = SportsPlanning.TechnicalResources ?? String.Empty;
						break;
					case PropertyNameSportsplanningLivehighlights:
						property.Value = SportsPlanning.LiveHighlightsFile ?? String.Empty;
						break;
					case PropertyNameSportsplanningReqBroadcastTime:
						property.Value = SportsPlanning.RequestedBroadcastTime.ToString();
						break;
					case PropertyNameSportsplanningProdNrPlasmaId:
						property.Value = SportsPlanning.ProductionNumberPlasmaId ?? String.Empty;
						break;
					case PropertyNameSportsplanningProdNrCeiton:
						property.Value = SportsPlanning.ProductNumberCeiton ?? String.Empty;
						break;
					case PropertyNameSportsplanningCostDep:
						property.Value = SportsPlanning.CostDepartment ?? String.Empty;
						break;
					case PropertyNameSportsplanningAdditionalInformation:
						property.Value = SportsPlanning.AdditionalInformation ?? String.Empty;
						break;
					case PropertyNameNewsInformationNewsCameraOperator:
						property.Value = NewsInformation.NewsCameraOperator ?? String.Empty;
						break;
					case PropertyNameNewsInformationJournalist:
						property.Value = NewsInformation.Journalist ?? String.Empty;
						break;
					case PropertyNameNewsInformationVirveCommandGroupOne:
						property.Value = NewsInformation.VirveCommandGroupOne ?? String.Empty;
						break;
					case PropertyNameTOMViewDescription:
						property.Value = TOMViewDescription ?? String.Empty;
						break;
					case PropertyNameNewsInformationVirveCommandGroupTwo:
						property.Value = NewsInformation.VirveCommandGroupTwo ?? String.Empty;
						break;
					case PropertyNameNewsInformationAdditionalInformation:
						property.Value = NewsInformation.AdditionalInformation ?? String.Empty;
						break;
					case PropertyNameHiddenMessiNews:
						property.Value = IsMediaOperatorMessiNewsHiddenFilterNeeded(helpers).ToString();
						break;
					case PropertyNameHiddenMessiLive:
						property.Value = false.ToString(); // Currently not in use, will be the case in a later stage
						break;
					case PropertyNameUiTeamMessiLive:
						property.Value = UI_Team_Messi_Live(helpers).ToString();
						break;
					case PropertyNameUiTeamMessiNews:
						property.Value = UI_Team_Messi_News(helpers).ToString();
						break;
					case PropertyNameUiAreena:
						property.Value = UI_Areena(helpers).ToString();
						break;
					case PropertyNameUiMessiNewsRec:
						property.Value = UI_Messi_News_Rec(helpers).ToString();
						break;
					case PropertyNameUiPlasma:
						property.Value = UI_Plasma(helpers).ToString();
						break;
					case PropertyNameUiTomNews:
						property.Value = UI_News(helpers).ToString();
						break;
					case PropertyNameUiTomSho:
						property.Value = UI_Sho(helpers).ToString();
						break;
					case PropertyNameUiTomTre:
						property.Value = UI_Tre(helpers).ToString();
						break;
					case PropertyNameUiTomSvenska:
						property.Value = UI_Svenska(helpers).ToString();
						break;
					case PropertyNameUiMcrChange:
						property.Value = UI_McrChange(helpers).ToString();
						break;
					case PropertyNameUiOffice:
						property.Value = UI_Office(helpers).ToString();
						break;
					case PropertyNameUiMcrSpecialist:
						property.Value = UI_McrSpecialist(helpers).ToString();
						break;
					case PropertyNameUiRecording:
						bool showNewsAreaOrderOnMediaOperatorTaskList = UI_NewsArea(helpers) && (Status == Status.ChangeRequested || Status == Status.Planned);
						property.Value = showNewsAreaOrderOnMediaOperatorTaskList ? true.ToString() : UI_Recording(helpers).ToString(); // When news area property is true, the rule is to set UI_Recording to true as well.
						break;
					case PropertyNameUiNewsArea:
						property.Value = UI_NewsArea(helpers).ToString();
						break;
					case PropertyNameMediaOperatorNotes:
						property.Value = MediaOperatorNotes ?? String.Empty;
						break;
					case PropertyNameMcrOperatorNotes:
						property.Value = McrOperatorNotes ?? String.Empty;
						break;
					case PropertyNameErrorDescription:
						property.Value = ErrorDescription ?? String.Empty;
						break;
					case PropertyNameEventId:
						property.Value = (Event?.Id ?? Guid.Empty).ToString();
						break;
					case PropertyNameVizremType:
						property.Value = Subtype.GetDescription();
						break;
					case PropertyNameEngine:
						property.Value = GetEngine();
						break;
					case PropertyNameShortDescription:
						property.Value = DisplayName ?? string.Empty;
						break;
					case PropertyNameAudioReturnInfo:
						string audioReturnInfoValue = AudioReturnInfo;
						property.Value = audioReturnInfoValue ?? string.Empty;
						break;
					case PropertyNameLiveUDeviceNames:
						property.Value = LiveUDeviceNames ?? String.Empty;
						break;
					case PropertyNameVidigoStreamSourceLinks:
						property.Value = VidigoStreamSourceLinks ?? String.Empty;
						break;
					case PropertyNamePlasmaIdsForArchiving:
						property.Value = PlasmaIdsForArchiving ?? String.Empty;
						break;
					case PropertyNameServiceConfigurations:
						property.Value = GetSerializedServiceConfigurations();
						break;
					case PropertyNameStartnow:
						property.Value = StartNow.ToString();
						break;
					case PropertyNameConvertedFromRunningStartnow:
						property.Value = ConvertedFromRunningToStartNow.ToString();
						break;
					case PropertyNamePreviousRunningOrderId:
						property.Value = PreviousRunningOrderId.ToString();
						break;
					case PropertyNameFixedSourcePlasma:
						bool shouldBehindFixedPlasmaSourceFilter = HasFixedPlasmaSource && !ShouldOrderAlwaysBeDisplayed;
						property.Value = shouldBehindFixedPlasmaSourceFilter.ToString();
						break;
					case PropertyNameReasonForCancellationOrRejection:
						property.Value = ReasonForCancellationOrRejection ?? string.Empty;
						break;
					case PropertyNameSources:
						property.Value = SourceDescriptions ?? String.Empty;
						break;
					case PropertyNameDestinations:
						property.Value = DestinationDescriptions ?? String.Empty;
						break;
					case PropertyNameRecordings:
						property.Value = RecordingDescriptions ?? String.Empty;
						break;
					case PropertyNameTransmissions:
						property.Value = TransmissionDescriptions ?? String.Empty;
						break;
					case PropertyNameCreatedBy:
						property.Value = CreatedByUserName ?? String.Empty;
						break;
					case PropertyNameCreatedByEmail:
						property.Value = CreatedByEmail ?? String.Empty;
						break;
					case PropertyNameCreatedByPhone:
						property.Value = CreatedByPhone ?? String.Empty;
						break;
					case PropertyNameLastUpdatedBy:
						property.Value = LastUpdatedBy ?? String.Empty;
						break;
					case PropertyNameLastUpdatedByEmail:
						property.Value = LastUpdatedByEmail ?? String.Empty;
						break;
					case PropertyNameLastUpdatedByPhone:
						property.Value = LastUpdatedByPhone ?? String.Empty;
						break;
					case PropertyNameMcrLateChange:
						property.Value = LateChange.ToString();
						break;
					case PropertyNameIsLocked when Id == Guid.Empty:
						// order is unlocked when it is new
						property.Value = "False";
						break;
					case PropertyNameIsPending when Id == Guid.Empty:
						// order is not pending when it is new
						property.Value = "False";
						break;
					case PropertyNameRecurrence when RecurringSequenceInfo.Recurrence.IsConfigured:
						property.Value = JsonConvert.SerializeObject(RecurringSequenceInfo, Formatting.None);
						break;
					case PropertyNameFromTemplate when RecurringSequenceInfo.Recurrence.IsConfigured && RecurringSequenceInfo.Id != Guid.Empty:
						property.Value = RecurringSequenceInfo.Id.ToString();
						break;
					case PropertyNameBillingInfo:
						property.Value = JsonConvert.SerializeObject(BillingInfo, Formatting.None);
						break;
					case PropertyNameUiStudioHelsinkiUT:
						property.Value = CheckOrderUT(helpers).ToString();
						break;
					case PropertyNameDestinationOrder:
						property.Value = String.Join(",", GetOrderedEndpointServiceGuids(helpers));
						break;
					case PropertyNameEventName when Event != null:
						property.Value = Event.Name;
						break;
					case PropertyNameMainSourceType:
						property.Value = MapVirtualPlatformToUiValue(SourceService?.Definition?.VirtualPlatformServiceName);
						break;
					case PropertyNamePublicationStart when IntegrationType == IntegrationType.Plasma || IntegrationType == IntegrationType.Feenix:
						{
							double amountOfMillisecondsSince1970 = PublicationStart.ConvertToCustomDatetimePropertyForReservation();
							if (amountOfMillisecondsSince1970 < TimeSpan.FromDays(1).TotalMilliseconds) continue; // check to avoid setting default values
							property.Value = amountOfMillisecondsSince1970.ToString();
							break;
						}
					case PropertyNamePublicationEnd when IntegrationType == IntegrationType.Plasma || IntegrationType == IntegrationType.Feenix:
						{
							double amountOfMillisecondsSince1970 = PublicationEnd.ConvertToCustomDatetimePropertyForReservation();
							if (amountOfMillisecondsSince1970 < TimeSpan.FromDays(1).TotalMilliseconds) continue; // check to avoid setting default values
							property.Value = amountOfMillisecondsSince1970.ToString();
							break;
						}
					case PropertyNameAttachmentCount:
						property.Value = AllServices.Sum(s => s.SynopsisFiles.Count()).ToString();
						break;
					default:
						continue;
				}

				if (string.IsNullOrWhiteSpace(property.Value)) continue;

				properties.Add(new Property
				{
					Action = property.Action,
					AddedByDefault = property.AddedByDefault,
					CopyToCreatedService = property.CopyToCreatedService,
					DiscreetValues = property.DiscreetValues,
					ID = property.ID,
					IsChecked = property.IsChecked,
					Name = property.Name,
					PropType = property.PropType,
					Required = property.Required,
					Type = property.Type,
					Value = property.Value?.Clean(true) ?? String.Empty,
					UiRow = property.UiRow,
					Visible = property.Visible
				});
			}

			helpers.Log(nameof(OrderManager), nameof(GetPropertiesFromBookingManager), $"Order properties {string.Join(", ", properties.Select(x => $"{x.Name}='{x.Value}'"))}", Name);

			return properties;
		}

		private string GetEngine()
		{
			var vizremService = AllServices.SingleOrDefault(x => x.Definition.Id == ServiceDefinitionGuids.VizremFarm);
			if (vizremService == null)
				return String.Empty;

			return vizremService.Functions.Single().ResourceName;
		}

		private bool CheckOrderUT(Helpers helpers)
		{
			if (AllServices == null) return false;

			bool isSH_UT = false;

			var validSourceOrDestinationLocations = new[] { "Studio Helsinki UT" };

			foreach (var service in AllServices)
			{
				foreach (var function in service.Functions.Where(f => f != null))
				{
					var sourceOrDestinationLocation = function.Parameters.FirstOrDefault(p => p != null && (p.Id == ProfileParameterGuids.FixedLineYleHelsinkiSourceLocation || p.Id == ProfileParameterGuids.YleHelsinkiDestinationLocation));

					if (sourceOrDestinationLocation != null && validSourceOrDestinationLocations.Contains(sourceOrDestinationLocation.StringValue))
					{
						helpers.Log(nameof(Order), nameof(CheckOrderUT), $"TRUE because order contains service with source or destination location profile param equal to {string.Join(" or ", validSourceOrDestinationLocations)}");
						isSH_UT = true;
					}
				}
			}

			bool isFiber = AllServices.Exists(s => s.Definition?.VirtualPlatform == VirtualPlatform.ReceptionFiber ||
						  s.Definition?.VirtualPlatform == VirtualPlatform.TransmissionFiber);

			return isFiber || isSH_UT;
		}

		private List<Guid> GetOrderedEndpointServiceGuids(Helpers helpers)
		{
			/* 
			For specs see task DCP196223
			Endpoint services in semi-fixed order. From left (first) to right (last).
			1. UMX (always first if there is any) = Uutisalue YLE Helsinki Destination services or Messi News Recordings using UMX tie line
			2. SH (always second if there is any) = Studio Helsinki YLE Helsinki destinations
			3. Other destinations/transmissions in alphabetical order….
			4. NET (always second to last if there is any) = Areena Destinations
			5. EVS (last one if there is any) = Messi Live (backup) recordings
			*/

			var servicesToConsider = AllServices.Where(s => s.IsBooked && (s.Definition.VirtualPlatformServiceType == VirtualPlatformType.Destination || s.Definition.VirtualPlatformServiceType == VirtualPlatformType.Transmission || ServiceCategorizer.IsConsideredAsMcrDestinationInMcrView(helpers, s))).ToList();

			var firstServices = servicesToConsider.Where(s => ServiceCategorizer.IsUutisalueDestination(s) || s.Definition.VirtualPlatform == VirtualPlatform.Routing).ToList();

			servicesToConsider = servicesToConsider.Except(firstServices).ToList();

			var secondServices = servicesToConsider.Where(s => ServiceCategorizer.IsStudioHelsinkiDestination(s)).ToList();

			servicesToConsider = servicesToConsider.Except(secondServices).ToList();

			var fourthServices = servicesToConsider.Where(s => ServiceCategorizer.IsAreenaDestination(s)).ToList();

			servicesToConsider = servicesToConsider.Except(fourthServices).ToList();

			var fifthServices = servicesToConsider.Where(s => ServiceCategorizer.IsEvsRecording(helpers, s)).ToList();

			servicesToConsider = servicesToConsider.Except(fifthServices).ToList();

			var thirdServices = servicesToConsider.OrderBy(s => s.GetShortDescription(this)).ToList();

			var orderedServiceGuids = new List<Guid>();
			orderedServiceGuids.AddRange(firstServices.Select(s => s.Id));
			orderedServiceGuids.AddRange(secondServices.Select(s => s.Id));
			orderedServiceGuids.AddRange(thirdServices.Select(s => s.Id));
			orderedServiceGuids.AddRange(fourthServices.Select(s => s.Id));
			orderedServiceGuids.AddRange(fifthServices.Select(s => s.Id));

			return orderedServiceGuids;
		}

		public void SetTimingBasedOnServices(Helpers helpers)
		{
			var nonSharedSourceServices = AllServices.Where(s => !s.IsSharedSource).ToList();

			if (!nonSharedSourceServices.Any()) return;

			var earliestService = nonSharedSourceServices.OrderBy(s => s.Start).First();
			var lastService = nonSharedSourceServices.OrderByDescending(s => s.End).First();

			Start = ConvertedFromRunningToStartNow ? Start : earliestService.Start;
			End = lastService.End;

			helpers.Log(nameof(Order), nameof(SetTimingBasedOnServices), $"Set order start to {Start} based on service {earliestService.Name} and order end to {End} based on service {lastService.Name}", Name);
		}

		public bool UpdateStatus(Helpers helpers, Status status)
		{
			helpers.LogMethodStart(nameof(Order), nameof(UpdateStatus), out var stopWatch, Name);

			var reservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id);
			if (reservation == null)
			{
				helpers.Log(nameof(Order), nameof(UpdateStatus), "Order reservation instance not found", Name);
				helpers.LogMethodCompleted(nameof(Order), nameof(UpdateStatus), Name, stopWatch);
				return false;
			}

			var propertiesToUpdate = new Dictionary<string, object> { { PropertyNameStatus, status.GetDescription() } };

			Status = status;

			if (status == Status.Cancelled || status == Status.Rejected)
			{
				propertiesToUpdate.Add(PropertyNameReasonForCancellationOrRejection, ReasonForCancellationOrRejection);
			}

			try
			{
				DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(helpers, reservation, propertiesToUpdate);
			}
			catch (Exception e)
			{
				foreach (var property in propertiesToUpdate)
				{
					helpers.Log(nameof(Order), nameof(UpdateStatus), $"Updating custom property {property.Key} to value {property.Value} failed: {e}");
				}
			}

			switch (status)
			{
				case Status.Running:
					// Do nothing
					break;
				case Status.Cancelled:
					CancelAllServices(helpers);
					NotificationManager.SendLiveOrderCancellationMail(helpers, this, ReasonForCancellationOrRejection);
					break;
				case Status.Rejected:
					helpers.Log(nameof(Order), nameof(UpdateStatus), "Rejecting all services", Name);
					RejectAllServices(helpers);
					NotificationManager.SendLiveOrderRejectionMail(helpers, this, ReasonForCancellationOrRejection);
					break;
				case Status.Confirmed:
					helpers.Log(nameof(Order), nameof(UpdateStatus), "Updating the status on all services", Name);

					foreach (var service in AllServices)
					{
						service.TryUpdateStatus(helpers, this);
					}

					NotificationManager.SendLiveOrderConfirmationMail(helpers, this);
					break;
				case Status.Completed:
					NotificationManager.SendLiveOrderCompletionMail(helpers, this);
					break;
				case Status.CompletedWithErrors:
					NotificationManager.SendLiveOrderCompletionWithErrorsMail(helpers, this);
					break;

				default:
					//nothing
					break;

			}

			helpers.LogMethodCompleted(nameof(Order), nameof(UpdateStatus), Name, stopWatch);

			return true;
		}

		/// <summary>
		/// Checks the Satellite Reception services in the order and verifies if the files that are stored in their SynopsisFiles collection are still available in the order attachments folder.
		/// If they are not available anymore, the files are removed from the SynopsisFiles property.
		/// This check is performed when retrieving an order.
		/// </summary>
		public void VerifySatelliteRxSynposisAttachments()
		{
			var satRxServices = AllServices.Where(x => x?.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite);
			if (!satRxServices.Any()) return;

			string orderAttachmentsDirectory = Path.Combine(OrderManager.OrderAttachmentsDirectory, Id.ToString());
			bool orderAttachmentsDirectoryExists = Directory.Exists(orderAttachmentsDirectory);
			string[] orderAttachments = orderAttachmentsDirectoryExists ? Directory.GetFiles(orderAttachmentsDirectory) : new string[0];
			foreach (var satRxService in satRxServices)
			{
				var invalidAttachments = satRxService.SynopsisFiles.Where(x => !orderAttachments.Contains(x)).ToList();
				foreach (string invalidAttachment in invalidAttachments) satRxService.SynopsisFiles.Remove(invalidAttachment);
			}
		}

		private void RejectAllServices(Helpers helpers)
		{
			foreach (var service in AllServices)
			{
				bool serviceIsUsedInOtherOrders = service.IsSharedSource && service.OrderReferences != null && service.OrderReferences.Count > 1;
				if (serviceIsUsedInOtherOrders) continue;

				if (service.IsPreliminary || service.Status == YLE.Service.Status.Preliminary)
				{
					helpers.Log(nameof(Order), nameof(RejectAllServices), $"Service {service.Name} is already preliminary");
					continue;
				}

				service.IsPreliminary = true;
				service.TryUpdateStatus(helpers, this);
				service.ReleaseResources(helpers, this);
			}
		}

		private void CancelAllServices(Helpers helpers)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			foreach (var service in AllServices)
			{
				// in case it's an event level reception and used by other orders that are not yet canceled then we can't cancel this service
				// this will be checked in the service itself
				bool serviceIsEventLevelReceptionUsedInOtherOrders = service.IsSharedSource && service.OrderReferences != null && service.OrderReferences.Count > 1;

				if (serviceIsEventLevelReceptionUsedInOtherOrders || !service.IsBooked) continue;

				helpers.Log(nameof(Order), nameof(CancelAllServices), $"Canceling service {service.Name}", Name);

				service.Cancel(helpers, this);

				// TODO: update short descriptions of services after clearing resources
			}
		}

		private bool TryStopOrderNow(Helpers helpers)
		{
			var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(Definition.BookingManagerElementName)) { AllowPostroll = true, AllowPreroll = true, CustomProperties = true };
			var orderReservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id) as ServiceReservationInstance ?? throw new ReservationNotFoundException(Id);
			bool orderWilEndAutomatically = DateTime.Now.RoundToMinutes() >= orderReservation.End.Subtract(orderReservation.GetPostRoll()).ToLocalTime();

			helpers.Log(nameof(Order), nameof(TryStopOrderNow), "Stopping Order " + Name + " now...");

			switch (orderReservation.Status)
			{
				case ReservationStatus.Confirmed:
				case ReservationStatus.Pending:
					return TryStopConfirmedOrderNow(helpers, bookingManager, orderReservation);
				case ReservationStatus.Ongoing when !orderWilEndAutomatically:
					return TryStopOngoingOrderNow(helpers, bookingManager, orderReservation);
				case ReservationStatus.Ended:
					helpers.Log(nameof(Order), nameof(TryStopOrderNow), "Unable to stop order now as it was already ended");
					return true;
				default:
					helpers.Log(nameof(Order), nameof(TryStopOrderNow), $"Unable to stop order now with status {orderReservation.Status}");
					return false;
			}
		}

		private bool TryStopConfirmedOrderNow(Helpers helpers, BookingManager bookingManager, ReservationInstance orderReservation)
		{
			if (orderReservation.Status == ReservationStatus.Ended)
			{
				helpers?.Log(nameof(OrderManager), nameof(TryStopConfirmedOrderNow), $"Order has already been ended for a while (Reservation status: {orderReservation.Status.GetDescription()}), doesn't need to be stopped anymore");
				return false;
			}

			bookingManager.EventReschedulingDelay = TimeSpan.FromSeconds(30);

			try
			{
				ChangeTimeInputData inputData = new ChangeTimeInputData
				{
					StartDate = orderReservation.Start.Add(orderReservation.GetPreRoll()).ToLocalTime(),
					PreRoll = orderReservation.GetPreRoll(),
					EndDate = DateTime.Now.Add(bookingManager.EventReschedulingDelay),
					PostRoll = orderReservation.GetPostRoll(),
					IsSilent = true
				};

				Reservation = DataMinerInterface.BookingManager.ChangeTime(helpers, bookingManager, orderReservation, inputData);
				helpers.Log(nameof(Order), nameof(TryStopConfirmedOrderNow), $"Successfully stopped order for order reservation with name: {orderReservation.Name}", Name);
				End = DateTimeExtensions.RoundToMinutes(orderReservation.End.FromReservation().Subtract(orderReservation.GetPostRoll()));
				return true;
			}
			catch (Exception exception)
			{
				helpers.Log(nameof(Order), nameof(TryStopConfirmedOrderNow), $"Unable to finish confirmed order reservation: {exception}");
				return false;
			}
		}

		private bool TryStopOngoingOrderNow(Helpers helpers, BookingManager bookingManager, ServiceReservationInstance orderReservation)
		{
			bookingManager.EventReschedulingDelay = TimeSpan.FromSeconds(30);

			try
			{
				Reservation = DataMinerInterface.BookingManager.Finish(helpers, bookingManager, orderReservation);
				helpers.Log(nameof(Order), nameof(TryStopOngoingOrderNow), $"Successfully stopped order reservation with name: {orderReservation.Name} and service id: {orderReservation.ServiceID}", Name);
				End = DateTimeExtensions.RoundToMinutes(Reservation.End.FromReservation().Subtract(Reservation.GetPostRoll()));
				return true;
			}
			catch (Exception exception)
			{
				helpers.Log(nameof(Order), nameof(TryStopOngoingOrderNow), $"Unable to finish ongoing order reservation: {exception}");
				return false;
			}
		}

		/// <summary>
		/// Refresh the status of the order after updating one if its service states.
		/// </summary>
		public void RefreshStatusAfterServiceStatusUpdate(Helpers helpers, bool updateOrderStatusAfterManualInteraction = false)
		{
			helpers.LogMethodStart(nameof(Order), nameof(RefreshStatusAfterServiceStatusUpdate), out var stopWatch, Name);

			helpers.Log(nameof(Order), nameof(RefreshStatusAfterServiceStatusUpdate), $"Current status: {Status.ToString()}", Name);

			var updatedStatus = DetermineOrderStatus(helpers, updateOrderStatusAfterManualInteraction);

			helpers.Log(nameof(Order), nameof(RefreshStatusAfterServiceStatusUpdate), $"Updated (expected) status: {updatedStatus.ToString()}");

			if (updatedStatus != Status)
			{
				bool updateSuccessful = UpdateStatus(helpers, updatedStatus);
				helpers.Log(nameof(Order), nameof(RefreshStatusAfterServiceStatusUpdate), $"Status update {(updateSuccessful ? "successful" : "failed")}");
			}
			else
			{
				helpers.Log(nameof(Order), nameof(RefreshStatusAfterServiceStatusUpdate), $"Order already has status {Status}. No status update required");
			}

			helpers.LogMethodCompleted(nameof(Order), nameof(RefreshStatusAfterServiceStatusUpdate), Name, stopWatch);
		}

		public string TOMViewDescription
		{
			get
			{
				if (Subtype == OrderSubType.Vizrem)
				{
					return AllServices.Single(x => !x.Children.Any()).GetShortDescription(this);
				}
				else
				{
					return SourceService.GetShortDescription(this);
				}
			}
		}

		private Status DetermineOrderStatus(Helpers helpers, bool updateOrderStatusAfterManualInteraction = false)
		{
			var allNonElrServices = AllServices.Where(s => !s.IsSharedSource).ToList();
			var allNonElrServiceStatuses = allNonElrServices.ToDictionary(s => s.Name, s => s.GenerateStatus(helpers, this));

			helpers.Log(nameof(Order), nameof(DetermineOrderStatus), $"Service statuses:\n{string.Join("\n", allNonElrServiceStatuses.Select(s => $"{s.Key} {s.Value}"))}");

			bool hasRunningServices = allNonElrServices.Any(s => s.ShouldBeRunning);
			if (hasRunningServices)
			{
				// order status should be running in case any of its services is running
				helpers.Log(nameof(Order), nameof(DetermineOrderStatus), $"Order has running services", Name);
				return Status.Running;
			}

			bool hasFileProcessingServices = allNonElrServices.Where(s => s.Definition.VirtualPlatform == VirtualPlatform.Recording).Any(s => StatusManager.DoesOrderContainsFileProcessingServices(helpers, s, this, updateOrderStatusAfterManualInteraction));
			if (hasFileProcessingServices)
			{
				helpers.Log(nameof(Order), nameof(DetermineOrderStatus), $"Order has file processing services", Name);
				// order status should be file processing in case any of its service still have incomplete file processing user tasks
				return Status.FileProcessing;
			}

			var completedServicesWithErrors = allNonElrServiceStatuses.Where(s => s.Value == YLE.Service.Status.ServiceCompletedWithErrors);
			if (completedServicesWithErrors.Any())
			{
				helpers.Log(nameof(Order), nameof(DetermineOrderStatus), $"Order has completed services with errors: {string.Join(", ", completedServicesWithErrors.Select(s => s.Key))}", Name);
				// no services are running
				// assume that order is completed if there are no running services and there are services that are in post roll or completed    
				return Status.CompletedWithErrors;
			}

			var completedOrPostRollServices = allNonElrServiceStatuses.Where(s => s.Value == YLE.Service.Status.ServiceCompleted || s.Value == YLE.Service.Status.PostRoll);
			if (completedOrPostRollServices.Any())
			{
				helpers.Log(nameof(Order), nameof(DetermineOrderStatus), $"Order has completed and/or post roll services: {string.Join(", ", completedOrPostRollServices.Select(s => s.Key))}", Name);
				// no services are running
				// assume that order is completed if there are no running services and there are services that are in post roll or completed    
				return Status.Completed;
			}

			return Status;
		}

		public void AssignContributingResources(Helpers helpers)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));

			helpers.LogMethodStart(nameof(Order), nameof(AssignContributingResources), out var stopwatch, Name);

			var orderReservationInstance = Reservation as ServiceReservationInstance ?? DataMinerInterface.ResourceManager.GetReservationInstance(helpers, Id) as ServiceReservationInstance;

			var requests = new List<AssignResourceRequest>();
			foreach (var node in Definition.Diagram.Nodes)
			{
				var nodeService = AllServices.SingleOrDefault(s => s.NodeId == node.ID);
				var nodeNewResourceId = nodeService?.ContributingResource?.ID ?? Guid.Empty;
				requests.Add(new AssignResourceRequest { TargetNodeLabel = node.Label, NewResourceId = nodeNewResourceId });
			}

			helpers.Log(nameof(Order), nameof(AssignContributingResources), "Setting contributing resources to " + string.Join(";", requests.Select(r => $"{r.TargetNodeLabel}={r.NewResourceId}")), Name);

			DataMinerInterface.ReservationInstance.AssignResources(helpers, orderReservationInstance, requests.ToArray());

			helpers.LogMethodCompleted(nameof(Order), nameof(AssignContributingResources), Name, stopwatch);
		}

		public List<Library.Solutions.SRM.Model.Function> GetServiceFunctions(Helpers helpers)
		{

			var functions = new List<Library.Solutions.SRM.Model.Function>();

			Action<Service> getServiceFunctions = null;
			getServiceFunctions = (service) =>
			{
				// TODO: remove when handled by standard solution or core software

				string resourceId = GetContributingResourceId(service, helpers);

				helpers.Log(nameof(Order), nameof(GetServiceFunctions), $"Service {service.Name}({service.Id}). Node ID: '{service.NodeId}'. Resource ID: '{resourceId}'");

				functions.Add(new Library.Solutions.SRM.Model.Function
				{
					ByReference = false,
					Id = service.NodeId,
					ShouldAutoSelectResource = false,
					Parameters = new List<Library.Solutions.SRM.Model.Parameter>(),
					SelectedResource = resourceId,
					SkipResourceValidation = true // contributing resources are always manually assigned
				});

				if (service.Children == null || !service.Children.Any()) return;

				foreach (var child in service.Children) getServiceFunctions(child);
			};

			foreach (var source in Sources) getServiceFunctions(source);

			return functions;
		}

		private string GetContributingResourceId(Service service, Helpers helpers)
		{
			string resourceId = null;
			if (service.ContributingResource == null) return resourceId;

			var elapsedMilliseconds = 0;
			while (elapsedMilliseconds < 10000)
			{
				try
				{
					var resource = DataMinerInterface.ResourceManager.GetResource(helpers, service.ContributingResource.ID);
					if (resource != null)
					{
						resourceId = resource.ID.ToString();
						break;
					}

					Thread.Sleep(50);
					elapsedMilliseconds += 50;
				}
				catch (Exception)
				{
					/* ignore */
				}
			}

			return resourceId;
		}

		/// <summary>
		/// Removes the services from this Order that were created by the provided Integration Type.
		/// If the source service was created by the Integration, it is replaced with a Dummy Source Service.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="integrationType">Integration type to check.</param>
		public void RemoveIntegrationServices(Helpers helpers, IntegrationType integrationType)
		{
			RemoveRoutingServices();

			YLE.Service.Service sourceService = Sources.FirstOrDefault(x => x.BackupType == BackupType.None && x.IntegrationType == integrationType);
			if (sourceService != null)
			{
				YLE.Service.Service dummyService = Service.GenerateDummyReception(helpers, sourceService.Start, sourceService.End);
				dummyService.SetChildren(sourceService.Children);
				Sources.Remove(sourceService);
				Sources.Add(dummyService);
			}

			foreach (YLE.Service.Service service in Sources)
			{
				RemoveIntegrationServices(service, integrationType);
			}
		}

		/// <summary>
		/// Removes the child services recursively of the provided service when their integration type matches the provided integration type.
		/// </summary>
		/// <param name="service">Service of which child services will be removed.</param>
		/// <param name="integrationType">Integration type used to filter child services.</param>
		private void RemoveIntegrationServices(YLE.Service.Service service, IntegrationType integrationType)
		{
			if (service.Children == null || !service.Children.Any()) return;
			var servicesToRemove = service.Children.Where(x => x.IntegrationType == integrationType).ToList();
			foreach (var serviceToRemove in servicesToRemove)
			{
				service.Children.Remove(serviceToRemove);
			}

			foreach (Service childService in service.Children)
			{
				RemoveIntegrationServices(childService, integrationType);
			}
		}

		/// <summary>
		/// This method will remove all Routing Services from the Order and link the incoming Service directly to the outgoing Service.
		/// </summary>
		public void RemoveRoutingServices()
		{
			List<Service> services = OrderManager.FlattenServices(Sources);
			List<Service> routingServices = services.Where(x => x.Definition.VirtualPlatform == VirtualPlatform.Routing).ToList();

			Service parentService = null;
			foreach (var routingService in routingServices)
			{
				parentService = routingService;
				while (parentService != null && parentService.Definition.VirtualPlatform == VirtualPlatform.Routing)
				{
					parentService = services.FirstOrDefault(x => x.Children.Contains(routingService));
				}

				if (parentService == null) continue;
				parentService.Children.Remove(routingService);
				foreach (var routingChildService in routingService.Children)
				{
					parentService.Children.Add(routingChildService);
				}
			}
		}

		/// <summary>
		/// This method will remove all auto generated service from the Order and link the incoming Service directly to the outgoing Service.
		/// </summary>
		public void RemoveAutogenerateServices()
		{
			List<Service> services = AllServices;
			List<Service> autoGeneratedServices = services.Where(x => x.IsAutogenerated).ToList();

			Service parentService = null;
			foreach (var autoGeneratedService in autoGeneratedServices)
			{
				parentService = autoGeneratedService;
				while (parentService != null && parentService.IsAutogenerated)
				{
					parentService = services.FirstOrDefault(x => x.Children.Contains(autoGeneratedService));
				}

				if (parentService == null) continue;
				parentService.Children.Remove(autoGeneratedService);
				foreach (var autoGeneratedChildService in autoGeneratedService.Children)
				{
					parentService.Children.Add(autoGeneratedChildService);
				}
			}
		}

		/// <summary>
		/// This method can be used to log the current hierarchy of the services in the order.
		/// </summary>
		/// <param name="engine"></param>
		/// <param name="parent">The parent service.</param>
		public void LogHierarchy(IEngine engine, Service parent)
		{
			if (parent.Children == null) return;

			var childServices = new List<Service>(parent.Children);
			foreach (var child in childServices)
			{
				engine.Log("[DEBUG]SERVICE: " + parent.Name + " => Service: " + child.Name);

				LogHierarchy(engine, child);
			}
		}

		/// <summary>
		/// Returns the serialized ServiceConfiguration with the latest information on the services in this order.
		/// </summary>
		/// <returns>Serialized ServiceConfiguration.</returns>
		public string GetSerializedServiceConfigurations()
		{
			var serviceConfigurations = new Dictionary<int, ServiceConfiguration>();
			foreach (var service in AllServices)
			{
				serviceConfigurations[service.NodeId] = service.GetConfiguration();
			}

			return JsonConvert.SerializeObject(serviceConfigurations, Formatting.None);
		}

		private List<string> GetResourceNamesForRecordingsAndTransmissions()
		{
			var result = new List<string>();

			var recordingAndTransmissionServices = AllServices.Where(s => s != null && (s.Definition?.VirtualPlatform == VirtualPlatform.Recording || s.Definition?.VirtualPlatformServiceType == VirtualPlatformType.Transmission));

			foreach (var recordingOrTransmissionService in recordingAndTransmissionServices)
			{
				var parentRoutingService = AllServices.SingleOrDefault(s => s != null && s.Children?.Contains(recordingOrTransmissionService) == true);

				if (parentRoutingService?.Functions is null) continue;

				bool isNewsRecording = recordingOrTransmissionService.Definition.Description?.Contains("News") == true;

				Function matrixFunction = null;
				if (isNewsRecording)
				{
					// that means 2 routing services, take the Matrix Input SDI resource of the output routing service
					matrixFunction = parentRoutingService.Functions.Single(f => f != null && parentRoutingService.Definition?.FunctionIsFirst(f) == true);
				}
				else
				{
					// that means only 1 routing service, take the Matrix Output SDI resource of the output routing service
					matrixFunction = parentRoutingService.Functions.Single(f => f != null && parentRoutingService.Definition?.FunctionIsLast(f) == true);
				}

				string matrixResourceDisplayName = matrixFunction.ResourceName.Split('.').Last();

				if (matrixResourceDisplayName != Constants.None)
				{
					result.Add(matrixResourceDisplayName);
				}
			}

			return result;
		}

		/// <summary>
		/// This method is used to get all the services from an order that match a given virtual platform.
		/// </summary>
		/// <param name="virtualPlatform">The name of the virtual platform.</param>
		/// <returns>The list of matching services.</returns>
		public List<YLE.Service.Service> GetServicesForVirtualPlatform(VirtualPlatform virtualPlatform)
		{
			var services = new List<YLE.Service.Service>();

			foreach (var source in Sources)
			{
				services.AddRange(source.GetChildServicesForVirtualPlatform(virtualPlatform));
			}

			return services;
		}

		/// <summary>
		/// Removes the child service from the parent service in case it's one of its children.
		/// </summary>
		/// <param name="id">The id of the child service.</param>
		/// <param name="parentService">The parent service.</param>
		/// <param name="removeAutoGeneratedParentServicesWithoutChildren">If enabled, the auto generated parent services that don't have any children anymore will also be removed.</param>
		/// <returns>Returns true in case the child service was found and removed.</returns>
		private bool RemoveChildService(Guid id, YLE.Service.Service parentService, bool removeAutoGeneratedParentServicesWithoutChildren)
		{
			foreach (var child in new List<YLE.Service.Service>(parentService.Children))
			{
				if (child.Id == id)
				{
					parentService.Children.Remove(child);

					return true;
				}

				if (RemoveChildService(id, child, removeAutoGeneratedParentServicesWithoutChildren))
				{
					if (removeAutoGeneratedParentServicesWithoutChildren && !child.Children.Any())
					{
						parentService.Children.Remove(child);
					}

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes the child service from the order.
		/// </summary>
		/// <param name="id">The id of the child service.</param>
		/// <param name="removeAutoGeneratedParentServicesWithoutChildren">If enabled, the auto generated parent services that don't have any children anymore will also be removed.</param>
		public void RemoveChildService(Guid id, bool removeAutoGeneratedParentServicesWithoutChildren = true)
		{
			foreach (var source in new List<YLE.Service.Service>(Sources))
			{
				if (RemoveChildService(id, source, removeAutoGeneratedParentServicesWithoutChildren))
				{
					break;
				}
			}
		}

		public void UpdateAutoGeneratedServiceTimings(Helpers helpers, Service childService)
		{
			var parentServices = AllServices.Where(s => s.Children.Contains(childService)).ToList();

			foreach (var parentService in parentServices)
			{
				if (parentService.IsAutogenerated && !parentService.IsSharedSource)
				{
					helpers.Log(nameof(Order), nameof(UpdateAutoGeneratedServiceTimings), $"Updating timing for service {parentService.Name} from {Service.TimingInfoToString(parentService)} to {Service.TimingInfoToString(childService)}");

					if (!childService.IsOrShouldBeRunning && PreviousRunningOrderId == Guid.Empty) parentService.Start = childService.Start;
					parentService.End = childService.End;
				}

				UpdateAutoGeneratedServiceTimings(helpers, parentService);
			}
		}

		public void SetServiceDisplayNames()
		{
			var serviceNameCounters = new Dictionary<string, int>();

			int transmissionsCounter = 0;
			foreach (var service in AllServices)
			{
				switch (service.Definition.VirtualPlatformServiceType)
				{
					case VirtualPlatformType.Recording:
						service.LofDisplayName = $"{VirtualPlatformType.Recording.GetDescription()} - {service.RecordingConfiguration.RecordingFileDestination.GetDescription()}";
						break;
					case VirtualPlatformType.Destination:
						var destinationLocationProfileParam = service.Functions.SelectMany(f => f.Parameters).FirstOrDefault(p => p.Name.Contains("Destination Location"));
						service.LofDisplayName = $"{VirtualPlatformType.Destination.GetDescription()} - {destinationLocationProfileParam?.StringValue ?? "Other"}";
						break;
					case VirtualPlatformType.Transmission:
						service.LofDisplayName = $"{service.Definition.VirtualPlatformServiceType.GetDescription()} {++transmissionsCounter}";
						break;
					default:
						service.LofDisplayName = service.Name;
						break;
				}

				if (serviceNameCounters.ContainsKey(service.LofDisplayName))
				{
					serviceNameCounters[service.LofDisplayName] += 1;
				}
				else
				{
					serviceNameCounters.Add(service.LofDisplayName, 1);
				}
			}

			foreach (var serviceNameCounter in serviceNameCounters)
			{
				if (serviceNameCounter.Value <= 1) continue;

				int counter = 0;
				foreach (var service in AllServices)
				{
					if (service.LofDisplayName != serviceNameCounter.Key) continue;

					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(service.LofDisplayName);
					stringBuilder.Append($" {++counter}");
					service.LofDisplayName = stringBuilder.ToString();
				}
			}

		}

		/// <summary>
		/// Checks if a function uses a matching plasma source 
		/// </summary>
		/// <param name="function">Function that contains the plasma user code profile parameter</param>
		/// <param name="applicablePlasmaUserCodes"></param>
		/// <returns></returns>
		private static bool HasFunctionMatchingPlasmaSource(YLE.Function.Function function, string[] applicablePlasmaUserCodes)
		{
			var sourcePlasmaUserCode = function.Parameters.FirstOrDefault(p => p != null && p.Id == ProfileParameterGuids.FixedLineLySourcePlasmaUserCode);
			if (sourcePlasmaUserCode == null) return false;

			if (applicablePlasmaUserCodes.Contains(sourcePlasmaUserCode.StringValue.ToUpper())) return true;

			return false;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(String.Format("Name: {0}", Name));
			sb.AppendLine(String.Format("\tStatus: {0}", Status));
			sb.AppendLine(String.Format("\tStart: {0}", Start));
			sb.AppendLine(String.Format("\tEnd: {0}", End));
			sb.AppendLine(String.Format("\tStart With Pre Roll: {0}", StartWithPreRoll));
			sb.AppendLine(String.Format("\tEnd With Post Roll: {0}", EndWithPostRoll));
			sb.AppendLine(String.Format("\tService Definition: {0}", Definition.Id));
			sb.AppendLine(String.Format("\tIntegration Type: {0}", IntegrationType));
			sb.AppendLine(String.Format("\tPlasma ID: {0}", PlasmaId));
			sb.AppendLine(String.Format("\tEurovision ID: {0}", EurovisionWorkOrderId));
			sb.AppendLine(String.Format("\tTransmission Number: {0}", EurovisionTransmissionNumber));
			sb.AppendLine(String.Format("\tContract: {0}", Contract));
			sb.AppendLine(String.Format("\tCompany: {0}", Company));
			sb.AppendLine("\tServices:");
			foreach (Service service in Sources)
			{
				sb.AppendLine("\t" + service);
			}

			return sb.ToString();
		}

		public string ServicesAndServiceDefinitionToString()
		{
			var sb = new StringBuilder();

			foreach (var service in AllServices)
			{
				var parentService = AllServices.FirstOrDefault(s => s.Children.Contains(service));
				var parentNodeId = parentService != null ? parentService.NodeId.ToString() : string.Empty;

				var childrenNodeIds = string.Join(",", service.Children.Select(s => s.NodeId));

				var nodeInfo = $"Node {service.NodeId} (parent={parentNodeId}, children={childrenNodeIds})";

				sb.Append($"{nodeInfo} = {service.Name} ; ");
			}

			return sb.ToString();
		}

		public string TimingInfoToString(bool inUtc = false)
		{
			var start = inUtc ? Start.ToUniversalTime() : Start;
			var startWithPreRoll = inUtc ? StartWithPreRoll.ToUniversalTime() : StartWithPreRoll;
			var end = inUtc ? End.ToUniversalTime() : End;
			var endWithPostRoll = inUtc ? EndWithPostRoll.ToUniversalTime() : EndWithPostRoll;

			return $"Start:{start.ToString("dd/MM/yyyy HH:mm:ss")}, Start with preroll:{startWithPreRoll.ToString("dd/MM/yyyy HH:mm:ss")}, End:{end.ToString("dd/MM/yyyy HH:mm:ss")}, End with postroll:{endWithPostRoll.ToString("dd/MM/yyyy HH:mm:ss")} {(inUtc ? "(in UTC)" : "in local time")}";
		}

		public OrderUpdateHandler.OptionFlags GetAddOrUpdateOptions(Helpers helpers, out OrderChangeSummary orderChangeInfo)
		{
			// The goal is to avoid doing too much processing steps in the foreground when editing an order to keep the time the user has to wait to a minimum.
			// Check the changes made in the order to determine what steps are necessary.

			var optionFlags = OrderUpdateHandler.OptionFlags.None;

			orderChangeInfo = Change.Summary as OrderChangeSummary;

			helpers.Log(nameof(Order), nameof(GetAddOrUpdateOptions), $"Order Change: {Change}");
			helpers.Log(nameof(Order), nameof(GetAddOrUpdateOptions), $"Order Change Summary: {orderChangeInfo}");

			bool savedOrderIsBeingBooked = orderChangeInfo.SavedOrderIsBeingBooked; // to make sure routing nodes are added to the definition
			bool profileParametersChanged = orderChangeInfo.ServiceChangeSummary.FunctionChangeSummary.ProfileParameterChangeSummary.IsChanged;
			bool resourcesChanged = orderChangeInfo.ServiceChangeSummary.FunctionChangeSummary.ResourceChangeSummary.IsChanged;
			bool servicesWereAddedOrRemoved = orderChangeInfo.ServicesWereAdded || orderChangeInfo.ServicesWereRemoved;
			bool timingChanged = orderChangeInfo.TimingChangeSummary.IsChanged;

			bool crucialOrderChanges = savedOrderIsBeingBooked || timingChanged;
			bool crucialServiceChanges = profileParametersChanged || resourcesChanged || servicesWereAddedOrRemoved;

			helpers.Log(nameof(Order), nameof(GetAddOrUpdateOptions), $"Crucial changes: {nameof(savedOrderIsBeingBooked)}: {savedOrderIsBeingBooked}, {nameof(profileParametersChanged)}: {profileParametersChanged}, {nameof(resourcesChanged)}: {resourcesChanged}, {nameof(servicesWereAddedOrRemoved)}: {servicesWereAddedOrRemoved}, {nameof(timingChanged)}: {timingChanged}");

			bool determineOrderDefinitionRequired = crucialOrderChanges || crucialServiceChanges;
			if (!determineOrderDefinitionRequired)
			{
				optionFlags |= OrderUpdateHandler.OptionFlags.SkipDetermineOrderDefinition | OrderUpdateHandler.OptionFlags.SkipAddOrUpdateOrderDefinition;
			}

			if (Event != null && !orderChangeInfo.TimingChangeSummary.IsChanged)
			{
				// When editing an order under an event. The Event should only possibly be updated when the order timing changes.
				// (Order ID should already be present in the Event)

				optionFlags |= OrderUpdateHandler.OptionFlags.SkipAddOrUpdateEvent;
			}

			return optionFlags;
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

		public Change GetChangeComparedTo<T>(Helpers helpers, T oldObjectInstance)
		{
			if (!(oldObjectInstance is Order otherOrder)) throw new ArgumentException($"Argument is not of type {nameof(Order)}", nameof(oldObjectInstance));

			var change = ChangeTrackingHelper.GetChangeComparedTo(this, otherOrder, new OrderChange(Name), helpers);

			return change;
		}

		/// <summary>
		/// Determines whether the order should be tagged as a highlight.
		/// </summary>
		/// <param name="helpers">helpers class.</param>
		/// <param name="userIsMcr">Indication if the user is MCR.</param>
		/// <param name="orderChangeSummary">Already gathered order change info.</param>
		public void SetMcrLateChangeRequired(Helpers helpers, bool userIsMcr, OrderChangeSummary orderChangeSummary = null)
		{
			if (IntegrationType == IntegrationType.Eurovision && LateChange)
			{
				helpers.Log(nameof(Order), nameof(SetMcrLateChangeRequired), $"Late change property for Eurovision order is already set to true, not changing it.");
				return;
			}

			// If synopsis files are uploaded by a non-MCR user at any point in time, order should get highlighted [DCP201967]
			bool synopsisFilesChanged = IsBooked && !userIsMcr && IsSourceServiceSynopsisFilesChanged(orderChangeSummary);
			if (synopsisFilesChanged)
			{
				helpers.Log(nameof(Order), nameof(SetMcrLateChangeRequired), $"Set to True because the source service synopsis files have changed");
				LateChange = true;
				return;
			}

			bool lessThanTwoHoursUntilOrderStart = IsLessThanTwoHoursUntilOrderStart();
			if (!lessThanTwoHoursUntilOrderStart)
			{
				helpers.Log(nameof(Order), nameof(SetMcrLateChangeRequired), $"Set to False because the order does not start within 2h");
				LateChange = false;
				return;
			}

			if (StartNow)
			{
				helpers.Log(nameof(Order), nameof(SetMcrLateChangeRequired), $"Set to True because the order should Start Now");
				LateChange = true;
				return;
			}

			if (!IsBooked && !IsSaved)
			{
				helpers.Log(nameof(Order), nameof(SetMcrLateChangeRequired), $"Set to True because the order is newly booked and starts within 2h");
				LateChange = true;
				return;
			}

			if (userIsMcr)
			{
				helpers.Log(nameof(Order), nameof(SetMcrLateChangeRequired), $"Set to False because the user is an MCR user");
				LateChange = false;
				return;
			}

			if (IsBooked)
			{
				orderChangeSummary = orderChangeSummary ?? Change.Summary as OrderChangeSummary;
			}

			bool doServiceChangeAndTimingSpecificationsApply = DoServiceChangeAndTimingSpecificationsApply(orderChangeSummary);
			bool isThereAnyServiceAddedOrRemoved = IsThereAnyServiceAddedOrRemoved(orderChangeSummary);

			LateChange |= doServiceChangeAndTimingSpecificationsApply ||
						  isThereAnyServiceAddedOrRemoved ||
						  (lessThanTwoHoursUntilOrderStart && !IsBooked);

			helpers?.Log(nameof(Order), nameof(SetMcrLateChangeRequired), $"Result: {nameof(LateChange)}={LateChange}, " +
				$"{nameof(isThereAnyServiceAddedOrRemoved)}={isThereAnyServiceAddedOrRemoved}, " +
				$"{nameof(doServiceChangeAndTimingSpecificationsApply)}={doServiceChangeAndTimingSpecificationsApply}, " +
				$"{nameof(lessThanTwoHoursUntilOrderStart)}={lessThanTwoHoursUntilOrderStart}");
		}

		private bool IsSourceServiceSynopsisFilesChanged(OrderChangeSummary orderChangeSummary)
		{
			var sourceServiceChange = SourceService?.Change as ServiceChange;
			return orderChangeSummary != null && sourceServiceChange?.GetCollectionChanges(nameof(Service.SynopsisFiles))?.Summary?.IsChanged == true;
		}

		private bool IsLessThanTwoHoursUntilOrderStart()
		{
			return Start.ToLocalTime() < (DateTime.Now + TimeSpan.FromHours(2)).ToLocalTime();
		}

		private bool DoServiceChangeAndTimingSpecificationsApply(OrderChangeSummary orderChangeSummary)
		{
			var destination = AllServices.FirstOrDefault(s => s.Definition.VirtualPlatform == VirtualPlatform.Destination);
			var destinationChanges = destination?.Change.Summary as ServiceChangeSummary;

			bool newSourceSelected = !SourceService?.IsBooked ?? false;
			bool orderHasDestinationResourceChange = destinationChanges?.FunctionChangeSummary.ResourceChangeSummary.IsChanged ?? false;
			bool gotServiceTimingChanged = orderChangeSummary?.ServiceChangeSummary.TimingChangeSummary.IsChanged ?? false;
			bool gotOrderTimingChanged = orderChangeSummary?.TimingChangeSummary.IsChanged ?? false;

			return newSourceSelected || orderHasDestinationResourceChange || gotServiceTimingChanged || gotOrderTimingChanged;
		}

		private bool IsThereAnyServiceAddedOrRemoved(OrderChangeSummary orderChangeSummary)
		{
			return orderChangeSummary?.ServicesWereAdded == true || orderChangeSummary?.ServicesWereRemoved == true;
		}


		/// <summary>
		/// This method can be used to replace any service in the order with another one.
		/// Child services are copied over from one service to the other.
		/// </summary>
		/// <param name="existingService">Existing service in the order.</param>
		/// <param name="newService">New service to be switched with the existing one.</param>
		public void ReplaceService(Service existingService, Service newService)
		{
			if (!AllServices.Contains(existingService)) throw new InvalidOperationException("Unable to replace service that is not part of the order");
			if (AllServices.Contains(newService)) throw new InvalidOperationException("Unable to replace a service with a service that is already part of the order");

			var parentService = AllServices.FirstOrDefault(x => x.Children.Contains(existingService));

			if (parentService == null)
			{
				SourceService = newService;
				return;
			}

			foreach (var childService in existingService.Children)
			{
				newService.Children.Add(childService);
			}

			parentService.Children[parentService.Children.IndexOf(existingService)] = newService;
			existingService.Children.Clear();
		}

		public bool ChangeTrackingStarted { get; private set; }

		public string UniqueIdentifier => Name;

		public Change Change => ChangeTrackingStarted ? ChangeTrackingHelper.GetUpdatedChange(this, initialPropertyValues, new OrderChange(Name)) : throw new InvalidOperationException($"Change Tracking has not been started for object {UniqueIdentifier}");

		public Service MainSourceService
		{
			get
			{
				return Sources.FirstOrDefault(x => x.BackupType == BackupType.None);
			}
		}

		public Service FeenixDestinationService
		{
			get
			{
				return AllServices.FirstOrDefault(x => x.IntegrationType == IntegrationType.Feenix && x.Definition?.VirtualPlatform == VirtualPlatform.Destination);
			}
		}

		public static string TimingInfoToString(TimeSpan preroll, DateTime start, DateTime end, TimeSpan postroll)
		{
			return $"Preroll: {preroll}, Start:{start.ToUniversalTime().ToString("O")}, End:{end.ToUniversalTime().ToString("O")}, Postroll: {postroll}";
		}

		public static string TimingInfoToString(Order order)
		{
			return TimingInfoToString(order.PreRoll, order.Start, order.End, order.PostRoll);
		}

		public bool CanBeSaved
		{
			get
			{
				return IsSaved;
			}
		}

		/// <summary>
		/// Checks if the Order matches the (basic) requirements to be booked.
		/// </summary>
		public bool CanBeBooked
		{
			get
			{
				if (HasEurovisionServices) return false;
				var allMainServices = GetAllMainServices();

				bool hasReceptionService = Sources.Exists(s => s.BackupType == BackupType.None && s.Definition?.VirtualPlatformServiceType == VirtualPlatformType.Reception && s.Definition?.VirtualPlatformServiceName != VirtualPlatformName.None && s.Definition?.VirtualPlatformServiceName != VirtualPlatformName.Unknown);
				bool hasEndPointService = allMainServices.Exists(s => s.Definition.IsEndPointService);

				bool hasVizremStudioServices = allMainServices.Exists(s => s.Definition?.VirtualPlatformServiceType == VirtualPlatformType.VizremStudio);
				bool hasVizremFarmService = allMainServices.Exists(s => s.Definition?.VirtualPlatformServiceType == VirtualPlatformType.VizremFarm);

				return (hasReceptionService || hasVizremStudioServices) && (hasEndPointService || hasVizremFarmService);
			}
		}

		/// <summary>
		/// Checks if the Order contains Eurovision services that should be manually booked from the LiveOrderForm.
		/// </summary>
		public bool HasEurovisionServices
		{
			get
			{
				foreach (Service service in AllServices)
				{
					if (service?.Definition?.VirtualPlatform == VirtualPlatform.ReceptionEurovision) return true;
					if (service?.Definition?.VirtualPlatform == VirtualPlatform.TransmissionEurovision) return true;
				}

				return false;
			}
		}

		public bool IsBooked
		{
			get
			{
				return Id != Guid.Empty;
			}
		}
	}
}