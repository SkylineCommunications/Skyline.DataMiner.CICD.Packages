namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition
{
	using System;
	using System.CodeDom;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Profiles;
	using Skyline.DataMiner.Net.ServiceManager.Objects;
	using static Skyline.DataMiner.Library.Solutions.SRM.KnownProperties;
	using FunctionDefinition = Function.FunctionDefinition;
	using FunctionDefinitionNotFoundException = Exceptions.FunctionDefinitionNotFoundException;
	using Node = Net.ServiceManager.Objects.Node;
	using PropertyNotFoundException = Exceptions.PropertyNotFoundException;
	using Service = Service.Service;

	public class ServiceDefinitionManager : IServiceDefinitionManager
	{
		private const string IsDefaultPropertyName = "IsDefault";
		private const string IsSourceOnlyPropertyName = "IsSourceOnly";
		private const string IsMcrOnlyPropertyName = "IsMcrOnly";
		private const string IsIntegrationOnlyPropertyName = "IsIntegrationOnly";
		private const string VirtualPlatformPropertyName = "Virtual Platform";
		private const string ContributiongConfigPropertyName = "Contributing Config";

		private const string BookingManagerProtocolName = "Skyline Booking Manager";

		// cached to improve performance (don't use these fields, use the properties)
		private ServiceDefinition routingServiceDefinition;
		private ServiceDefinition graphicsProcessingServiceDefinition;
		private ServiceDefinition videoProcessingServiceDefinition;
		private ServiceDefinition audioProcessingServiceDefinition;
		private ServiceDefinition vizremConverterHelsinkiServiceDefinition;
		private ServiceDefinition vizremConverterMediapolisServiceDefinition;
		private List<Net.ServiceManager.Objects.ServiceDefinition> allNonOrderSrmServiceDefinitions;
		private readonly Dictionary<Guid, Net.Profiles.ProfileDefinition> cachedInterfaceProfileDefinitions = new Dictionary<Guid, Net.Profiles.ProfileDefinition>();
		private readonly List<Net.Messages.SystemFunctionDefinition> cachedSystemFunctionDefinitions = new List<Net.Messages.SystemFunctionDefinition>();
		private IEnumerable<Net.Messages.FunctionDefinition> allProtocolFunctionDefinitions; // initialized by some methods to improve performance
		private IEnumerable<YleBookingManager> allBookingManagers; // cached for performance
		private ServiceDefinitionsForLiveOrderForm serviceDefinitionsForLiveOrderForm; // cached for performance

		private readonly List<ScriptEntry> orderServiceDefinitionScriptActions = new List<ScriptEntry>
		{
			new ScriptEntry { Name = "START", Script = "HandleOrderAction" },
			new ScriptEntry { Name = "STOP", Script = "HandleOrderAction" }
		};

		public ServiceDefinitionManager(Helpers helpers)
		{
			Helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
		}

		public Helpers Helpers { get; }

		public ServiceDefinition RoutingServiceDefinition => routingServiceDefinition ?? (routingServiceDefinition = GetServiceDefinition("Routing"));

		public ServiceDefinition GraphicsProcessingServiceDefinition => graphicsProcessingServiceDefinition ?? (graphicsProcessingServiceDefinition = GetServiceDefinition("Graphics Processing"));

		public ServiceDefinition VideoProcessingServiceDefinition => videoProcessingServiceDefinition ?? (videoProcessingServiceDefinition = GetServiceDefinition("Video Processing"));

		public ServiceDefinition AudioProcessingServiceDefinition => audioProcessingServiceDefinition ?? (audioProcessingServiceDefinition = GetServiceDefinition("Audio Processing"));

		public ServiceDefinition VizremConverterHelsinkiServiceDefinition => vizremConverterHelsinkiServiceDefinition ?? (vizremConverterHelsinkiServiceDefinition = GetServiceDefinition("NC2 Converter Helsinki"));

		public ServiceDefinition VizremConverterMediapolisServiceDefinition => vizremConverterMediapolisServiceDefinition ?? (vizremConverterMediapolisServiceDefinition = GetServiceDefinition("NC2 Converter Mediapolis"));

		public ServiceDefinitionsForLiveOrderForm ServiceDefinitionsForLiveOrderForm => serviceDefinitionsForLiveOrderForm ?? (serviceDefinitionsForLiveOrderForm = GetServiceDefinitionsForLiveOrderForm());

		private List<Net.ServiceManager.Objects.ServiceDefinition> AllNonOrderSrmServiceDefinitions => allNonOrderSrmServiceDefinitions ?? (allNonOrderSrmServiceDefinitions = GetAllNonOrderSrmServiceDefinitions());

		private IEnumerable<Net.Messages.FunctionDefinition> AllProtocolFunctionDefinitions => allProtocolFunctionDefinitions ?? (allProtocolFunctionDefinitions = GetAllProtocolFunctionDefinitions());

		private IEnumerable<YleBookingManager> AllBookingManagers => allBookingManagers ?? (allBookingManagers = GetAllBookingManagers());


		/// <summary>
		/// Loops in parallel over all srm service definitions and collects the ones relevant for the live order form.
		/// </summary>
		/// <returns>An object containing all service definitions relevant for the Live Order Form Script.</returns>
		/// <remarks>This specific method was made to increase performance when loading the live order form.</remarks>
		private ServiceDefinitionsForLiveOrderForm GetServiceDefinitionsForLiveOrderForm()
		{
			var response = new ServiceDefinitionsForLiveOrderForm();

			FillCache();

			LogMultiThreadedMethodStart(nameof(GetServiceDefinitionsForLiveOrderForm), out var stopwatch);

			Parallel.ForEach(AllNonOrderSrmServiceDefinitions, (srmServiceDefinition) =>
			{
				if (srmServiceDefinition.Properties == null) return;

				var virtualPlatformProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, VirtualPlatformPropertyName, StringComparison.InvariantCultureIgnoreCase));
				if (virtualPlatformProperty == null || String.IsNullOrWhiteSpace(virtualPlatformProperty.Value) || virtualPlatformProperty.Value.StartsWith("order", StringComparison.InvariantCultureIgnoreCase)) return;

				var contributingConfig = GetContributingConfig(srmServiceDefinition);
				if (contributingConfig == null) return;

				if (!EnumExtensions.GetEnumDescriptions<VirtualPlatform>().Contains(virtualPlatformProperty.Value)) return;

				var serviceDefinition = new ServiceDefinition(virtualPlatformProperty.Value)
				{
					Id = srmServiceDefinition.ID,
					Name = srmServiceDefinition.Name,
					Description = srmServiceDefinition.Description ?? String.Empty,
					BookingManagerElementName = GetBookingManager(virtualPlatformProperty.Value.GetEnumValue<VirtualPlatform>()).ElementName,
					ContributingConfig = contributingConfig,
					FunctionDefinitions = GetFunctionDefinitions(srmServiceDefinition),
					Diagram = srmServiceDefinition.Diagram,
					IsDefault = IsDefault(srmServiceDefinition),
					IsSourceOnly = IsSourceOnly(srmServiceDefinition),
					IsMcrOnly = IsMcrOnly(srmServiceDefinition),
					IsIntegrationOnly = IsIntegrationOnly(srmServiceDefinition)
				};

				switch (serviceDefinition.VirtualPlatformServiceType)
				{
					case VirtualPlatformType.Reception:
						response.ReceptionServiceDefinitions.Add(serviceDefinition);
						break;
					case VirtualPlatformType.Destination:
						response.DestinationServiceDefinitions.Add(serviceDefinition);
						break;
					case VirtualPlatformType.Recording:
						response.RecordingServiceDefinitions.Add(serviceDefinition);
						break;
					case VirtualPlatformType.Transmission:
						response.TransmissionServiceDefinitions.Add(serviceDefinition);
						break;
					case VirtualPlatformType.GraphicsProcessing:
						response.GraphicsProcessingServiceDefinition = serviceDefinition;
						break;
					case VirtualPlatformType.AudioProcessing:
						response.AudioProcessingServiceDefinition = serviceDefinition;
						break;
					case VirtualPlatformType.VideoProcessing:
						response.VideoProcessingServiceDefinition = serviceDefinition;
						break;
					case VirtualPlatformType.VizremStudio:
						response.VizremStudios.Add(serviceDefinition);
						break;
					case VirtualPlatformType.VizremFarm:
						response.VizremFarms.Add(serviceDefinition);
						break;
					case VirtualPlatformType.VizremNC2Converter:
						response.VizremNC2Converters.Add(serviceDefinition);
						break;
					case VirtualPlatformType.Routing:
						response.RoutingServiceDefinition = serviceDefinition;
						break;
					default:
						return;
				}
			});

			LogMultiThreadedMethodCompleted(nameof(GetServiceDefinitionsForLiveOrderForm), stopwatch);

			return response;
		}

		private void FillCache()
		{
			LogMethodStart(nameof(FillCache), out var stopwatch);

			allNonOrderSrmServiceDefinitions = allNonOrderSrmServiceDefinitions ?? GetAllNonOrderSrmServiceDefinitions();
			allProtocolFunctionDefinitions = allProtocolFunctionDefinitions ?? GetAllProtocolFunctionDefinitions();
			allBookingManagers = allBookingManagers ?? GetAllBookingManagers();

			LogMethodCompleted(nameof(FillCache), stopwatch);
		}

		/// <remarks>ONLY TO BE USED FOR NON-ORDER SERVICE DEFINITIONS</remarks>
		public ServiceDefinition GetServiceDefinition(Guid serviceDefinitionGuid)
		{
			try
			{
				LogMethodStart(nameof(GetServiceDefinition), out var stopwatch);

				var srmServiceDefinition = AllNonOrderSrmServiceDefinitions.SingleOrDefault(sd => sd.ID == serviceDefinitionGuid);
				if (srmServiceDefinition == null) throw new ServiceDefinitionNotFoundException(serviceDefinitionGuid);

				var virtualPlatformProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, VirtualPlatformPropertyName, StringComparison.InvariantCultureIgnoreCase)) ?? throw new PropertyNotFoundException(VirtualPlatformPropertyName);
				string virtualPlatformName = virtualPlatformProperty.Value;

				var contributingConfig = GetContributingConfig(srmServiceDefinition);
				if (contributingConfig == null)
				{
					// Service Definition should always contain a Contributing Config
					return null;
				}

				var serviceDefinition = new ServiceDefinition(virtualPlatformName)
				{
					Id = srmServiceDefinition.ID,
					Name = srmServiceDefinition.Name,
					Description = srmServiceDefinition.Description ?? string.Empty,
					BookingManagerElementName = GetBookingManagerElementName(srmServiceDefinition),
					ContributingConfig = contributingConfig,
					FunctionDefinitions = GetFunctionDefinitions(srmServiceDefinition),
					Diagram = srmServiceDefinition.Diagram,
					IsDefault = IsDefault(srmServiceDefinition),
					IsSourceOnly = IsSourceOnly(srmServiceDefinition),
					IsMcrOnly = IsMcrOnly(srmServiceDefinition),
					IsIntegrationOnly = IsIntegrationOnly(srmServiceDefinition)
				};

				LogMethodCompleted(nameof(GetServiceDefinition), stopwatch);

				return serviceDefinition;
			}
			catch (Exception e)
			{
				Log(nameof(GetServiceDefinition), $"Something went wrong while getting service definition {serviceDefinitionGuid}: {e}");
				return null;
			}
		}

		public Net.ServiceManager.Objects.ServiceDefinition GetRawServiceDefinition(Guid serviceDefinitionGuid)
		{
			LogMethodStart(nameof(GetRawServiceDefinition), out var stopwatch);

			var serviceDefinition = DataMinerInterface.ServiceManager.GetServiceDefinition(Helpers, serviceDefinitionGuid);

			LogMethodCompleted(nameof(GetRawServiceDefinition), stopwatch);

			return serviceDefinition;
		}

		/// <summary>
		/// Gets the Service Definitions defined on the DMA.
		/// If multiple Service Definitions have the same virtual platform name, then the IsDefault property is used to select the correct one.
		/// </summary>
		/// <returns>Collection of unique Service Definitions.</returns>
		public IEnumerable<ServiceDefinition> GetReceptionServiceDefinitions()
		{
			var receptionServiceDefinitions = new List<ServiceDefinition>();

			foreach (var srmServiceDefinition in AllNonOrderSrmServiceDefinitions)
			{
				if (srmServiceDefinition.Properties == null) continue;

				// Set Virtual Platform Name
				var virtualPlatformProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, VirtualPlatformPropertyName, StringComparison.InvariantCultureIgnoreCase));
				if (virtualPlatformProperty == null || String.IsNullOrWhiteSpace(virtualPlatformProperty.Value) || !virtualPlatformProperty.Value.StartsWith("reception.", StringComparison.InvariantCultureIgnoreCase)) continue;

				// Parse Contributing Config
				var contributingConfig = GetContributingConfig(srmServiceDefinition);
				if (contributingConfig == null)
				{
					// A reception Service Definition should always contain a Contributing Config
					continue;
				}

				ServiceDefinition serviceDefinition = new ServiceDefinition(virtualPlatformProperty.Value)
				{
					Id = srmServiceDefinition.ID,
					Name = srmServiceDefinition.Name,
					Description = srmServiceDefinition.Description,
					BookingManagerElementName = GetBookingManagerElementName(srmServiceDefinition),
					ContributingConfig = contributingConfig,
					FunctionDefinitions = GetFunctionDefinitions(srmServiceDefinition),
					Diagram = srmServiceDefinition.Diagram,
					IsDefault = IsDefault(srmServiceDefinition),
					IsSourceOnly = IsSourceOnly(srmServiceDefinition),
					IsMcrOnly = IsMcrOnly(srmServiceDefinition),
					IsIntegrationOnly = IsIntegrationOnly(srmServiceDefinition)
				};

				receptionServiceDefinitions.Add(serviceDefinition);
			}

			return receptionServiceDefinitions;
		}

		public ServiceDefinition GetDestinationServiceDefinition()
		{
			var serviceDefinition = GetServiceDefinition("Destination");
			return serviceDefinition;
		}

		public IEnumerable<ServiceDefinition> GetDestinationServiceDefinitions()
		{
			List<ServiceDefinition> destinationServiceDefinitions = new List<ServiceDefinition>();

			foreach (var srmServiceDefinition in AllNonOrderSrmServiceDefinitions)
			{
				if (srmServiceDefinition.Properties == null) continue;

				// Set Virtual Platform Name
				var virtualPlatformProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, VirtualPlatformPropertyName, StringComparison.InvariantCultureIgnoreCase));
				if (virtualPlatformProperty == null || virtualPlatformProperty.Value.ToLower() != "destination") continue;

				// Parse Contributing Config
				ContributingConfig contributingConfig = GetContributingConfig(srmServiceDefinition);
				if (contributingConfig == null)
				{
					// A destination Service Definition should always contain a Contributing Config
					continue;
				}

				ServiceDefinition serviceDefinition = new ServiceDefinition(virtualPlatformProperty.Value)
				{
					Id = srmServiceDefinition.ID,
					Name = srmServiceDefinition.Name,
					Description = srmServiceDefinition.Description,
					BookingManagerElementName = GetBookingManagerElementName(srmServiceDefinition),
					ContributingConfig = contributingConfig,
					FunctionDefinitions = GetFunctionDefinitions(srmServiceDefinition),
					Diagram = srmServiceDefinition.Diagram,
					IsDefault = IsDefault(srmServiceDefinition),
					IsSourceOnly = IsSourceOnly(srmServiceDefinition),
					IsMcrOnly = IsMcrOnly(srmServiceDefinition),
					IsIntegrationOnly = IsIntegrationOnly(srmServiceDefinition)
				};

				destinationServiceDefinitions.Add(serviceDefinition);
			}

			return destinationServiceDefinitions;
		}

		public IEnumerable<ServiceDefinition> GetRecordingServiceDefinitions()
		{
			List<ServiceDefinition> recordingServiceDefinitions = new List<ServiceDefinition>();

			foreach (var srmServiceDefinition in AllNonOrderSrmServiceDefinitions)
			{
				if (srmServiceDefinition.Properties == null) continue;

				// Set Virtual Platform Name
				var virtualPlatformProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, VirtualPlatformPropertyName, StringComparison.InvariantCultureIgnoreCase));
				if (virtualPlatformProperty == null || virtualPlatformProperty.Value.ToLower() != "recording") continue;

				// Parse Contributing Config
				ContributingConfig contributingConfig = GetContributingConfig(srmServiceDefinition);
				if (contributingConfig == null)
				{
					// A destination Service Definition should always contain a Contributing Config
					continue;
				}

				ServiceDefinition serviceDefinition = new ServiceDefinition(virtualPlatformProperty.Value)
				{
					Id = srmServiceDefinition.ID,
					Name = srmServiceDefinition.Name,
					Description = srmServiceDefinition.Description,
					BookingManagerElementName = GetBookingManagerElementName(srmServiceDefinition),
					ContributingConfig = contributingConfig,
					FunctionDefinitions = GetFunctionDefinitions(srmServiceDefinition),
					Diagram = srmServiceDefinition.Diagram,
					IsDefault = IsDefault(srmServiceDefinition),
					IsSourceOnly = IsSourceOnly(srmServiceDefinition),
					IsMcrOnly = IsMcrOnly(srmServiceDefinition),
					IsIntegrationOnly = IsIntegrationOnly(srmServiceDefinition)
				};

				recordingServiceDefinitions.Add(serviceDefinition);
			}

			return recordingServiceDefinitions;
		}

		public IEnumerable<ServiceDefinition> GetTransmissionServiceDefinitions()
		{
			List<ServiceDefinition> transmissionServiceDefinitions = new List<ServiceDefinition>();

			foreach (var srmServiceDefinition in AllNonOrderSrmServiceDefinitions)
			{
				if (srmServiceDefinition.Properties == null) continue;

				// Set Virtual Platform Name
				var virtualPlatformProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, VirtualPlatformPropertyName, StringComparison.InvariantCultureIgnoreCase));
				if (virtualPlatformProperty == null || String.IsNullOrWhiteSpace(virtualPlatformProperty.Value) || !virtualPlatformProperty.Value.StartsWith("transmission.", StringComparison.InvariantCultureIgnoreCase)) continue;

				// Parse Contributing Config
				ContributingConfig contributingConfig = GetContributingConfig(srmServiceDefinition);
				if (contributingConfig == null)
				{
					// A Transmission Service Definition should always contain a Contributing Config
					continue;
				}

				ServiceDefinition serviceDefinition = new ServiceDefinition(virtualPlatformProperty.Value)
				{
					Id = srmServiceDefinition.ID,
					Name = srmServiceDefinition.Name,
					Description = srmServiceDefinition.Description,
					BookingManagerElementName = GetBookingManagerElementName(srmServiceDefinition),
					ContributingConfig = contributingConfig,
					FunctionDefinitions = GetFunctionDefinitions(srmServiceDefinition),
					Diagram = srmServiceDefinition.Diagram,
					IsDefault = IsDefault(srmServiceDefinition),
					IsSourceOnly = IsSourceOnly(srmServiceDefinition),
					IsMcrOnly = IsMcrOnly(srmServiceDefinition),
					IsIntegrationOnly = IsIntegrationOnly(srmServiceDefinition)
				};

				transmissionServiceDefinitions.Add(serviceDefinition);
			}

			return transmissionServiceDefinitions;
		}

		public ServiceDefinition GetDummyRxServiceDefinition()
		{
			var serviceDefinition = GetServiceDefinition("Dummy RX");

			return serviceDefinition;
		}

		public Element GetBookingManager(ServiceDefinition serviceDefinition)
		{
			return GetBookingManager(serviceDefinition.VirtualPlatform);
		}

		public Element GetBookingManager(VirtualPlatform virtualPlatform)
		{
			var bookingManager = AllBookingManagers.FirstOrDefault(b => b != null && b.Element.IsActive && b.VirtualPlatform == virtualPlatform);

			return bookingManager?.Element;
		}

		public Net.ServiceManager.Objects.ServiceDefinition GetServiceDefinitionFromOrder(Order order, List<Service> servicesToRemove)
		{
			var orderDefinition = BuildServiceDefinitionFromOrder(order, servicesToRemove);

			orderDefinition = ConditionalPrepareDefinitionToBeCreatedAsNew(order, orderDefinition);

			orderDefinition = TryUseExistingServiceDefinition(orderDefinition);

			return orderDefinition;
		}

		private Net.ServiceManager.Objects.ServiceDefinition ConditionalPrepareDefinitionToBeCreatedAsNew(Order order, Net.ServiceManager.Objects.ServiceDefinition orderDefinition)
		{
			LogMethodStart(nameof(ConditionalPrepareDefinitionToBeCreatedAsNew), out var stopwatch);

			bool orderAlreadyHasExistingServiceDefinition = order.Definition != null && order.Definition.Id != Guid.Empty;
			if (orderAlreadyHasExistingServiceDefinition)
			{
				var existingOrderDefinition = DataMinerInterface.ServiceManager.GetServiceDefinition(Helpers, order.Definition.Id);

				bool orderDefinitionHasChanged = !existingOrderDefinition.Diagram.Matches(orderDefinition.Diagram);
				if (orderDefinitionHasChanged)
				{
					PrepareServiceDefinitionToBeCreatedAsNew(orderDefinition);

					Log(nameof(ConditionalPrepareDefinitionToBeCreatedAsNew), $"Existing Order definition has changed, assigned new Guid to the Definition object so it will be created as new.");
				}
			}

			LogMethodCompleted(nameof(ConditionalPrepareDefinitionToBeCreatedAsNew), stopwatch);

			return orderDefinition;
		}

		public Net.ServiceManager.Objects.ServiceDefinition AddOrUpdateServiceDefinition(Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
		{
			Log(nameof(AddOrUpdateServiceDefinition), $"Adding or updating service definition {serviceDefinition.ID} with Diagram {serviceDefinition.Diagram.DiagramToString()}");

			serviceDefinition.Scripts = orderServiceDefinitionScriptActions;

			var diagramHashCodeProperty = serviceDefinition.Properties.SingleOrDefault(p => p.Name == ServiceDefinitionPropertyNames.DiagramHashCode);

			if (diagramHashCodeProperty is null)
			{
				diagramHashCodeProperty = new Property(ServiceDefinitionPropertyNames.DiagramHashCode, serviceDefinition.Diagram.GetHashCodeForYleProject().ToString());
				serviceDefinition.Properties.Add(diagramHashCodeProperty);
			}
			else
			{
				diagramHashCodeProperty.Value = serviceDefinition.Diagram.GetHashCodeForYleProject().ToString();
			}

			return DataMinerInterface.ServiceManager.AddOrUpdateServiceDefinition(Helpers, serviceDefinition, true);
		}

		public void DeleteServiceDefinition(Guid serviceDefinitionGuid)
		{
			var serviceDefinition = DataMinerInterface.ServiceManager.GetServiceDefinition(Helpers, serviceDefinitionGuid);
			if (serviceDefinition == null) return;

			bool isServiceDefinitionInUse = DataMinerInterface.ResourceManager.GetReservationInstancesByServiceDefinition(Helpers, serviceDefinition).Any();
			if (!isServiceDefinitionInUse)
			{
				string error = null;
				if (!DataMinerInterface.ServiceManager.RemoveServiceDefinitions(Helpers, out error, serviceDefinition))
				{
					// TODO: should this do anything?
				}
			}
		}

		private bool IsDefault(Net.ServiceManager.Objects.ServiceDefinition srmServiceDefinition)
		{
			var isDefaultProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, IsDefaultPropertyName, StringComparison.InvariantCultureIgnoreCase));
			return isDefaultProperty != null && Convert.ToBoolean(isDefaultProperty.Value);
		}

		private bool IsSourceOnly(Net.ServiceManager.Objects.ServiceDefinition srmServiceDefinition)
		{
			var isSourceOnlyProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, IsSourceOnlyPropertyName, StringComparison.InvariantCultureIgnoreCase));
			return isSourceOnlyProperty != null && Convert.ToBoolean(isSourceOnlyProperty.Value);
		}

		private bool IsMcrOnly(Net.ServiceManager.Objects.ServiceDefinition srmServiceDefinition)
		{
			var isMcrOnlyProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, IsMcrOnlyPropertyName, StringComparison.InvariantCultureIgnoreCase));
			return isMcrOnlyProperty != null && Convert.ToBoolean(isMcrOnlyProperty.Value);
		}

		private bool IsIntegrationOnly(Net.ServiceManager.Objects.ServiceDefinition srmServiceDefinition)
		{
			var isIntegrationOnlyProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, IsIntegrationOnlyPropertyName, StringComparison.InvariantCultureIgnoreCase));
			return isIntegrationOnlyProperty != null && Convert.ToBoolean(isIntegrationOnlyProperty.Value);
		}

		private IEnumerable<YleBookingManager> GetAllBookingManagers()
		{
			LogMethodStart(nameof(GetAllBookingManagers), out var stopwatch);

			var result = Helpers.Engine.FindElementsByProtocol(BookingManagerProtocolName).Select(e => new YleBookingManager(Helpers, e)).ToList();

			LogMethodCompleted(nameof(GetAllBookingManagers), stopwatch);

			return result;
		}

		public ServiceDefinition GetServiceDefinition(string nameFilter)
		{
			foreach (var srmServiceDefinition in AllNonOrderSrmServiceDefinitions)
			{
				if (!srmServiceDefinition.Name.Contains(nameFilter) || srmServiceDefinition.Properties == null) continue;

				var virtualPlatformProperty = srmServiceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, VirtualPlatformPropertyName, StringComparison.InvariantCultureIgnoreCase));
				if (virtualPlatformProperty == null || String.IsNullOrWhiteSpace(virtualPlatformProperty.Value)) continue;
				string virtualPlatformName = virtualPlatformProperty.Value;

				var contributingConfig = GetContributingConfig(srmServiceDefinition);
				if (contributingConfig == null) continue;

				ServiceDefinition serviceDefinition = new ServiceDefinition(virtualPlatformName)
				{
					Id = srmServiceDefinition.ID,
					Name = srmServiceDefinition.Name,
					Description = srmServiceDefinition.Description,
					BookingManagerElementName = GetBookingManagerElementName(srmServiceDefinition),
					ContributingConfig = contributingConfig,
					FunctionDefinitions = GetFunctionDefinitions(srmServiceDefinition),
					Diagram = srmServiceDefinition.Diagram,
					IsDefault = IsDefault(srmServiceDefinition),
					IsSourceOnly = IsSourceOnly(srmServiceDefinition),
					IsMcrOnly = IsMcrOnly(srmServiceDefinition),
					IsIntegrationOnly = IsIntegrationOnly(srmServiceDefinition)
				};

				return serviceDefinition;
			}

			return null;
		}

		/// <summary>
		/// Retrieve the function definitions for a given service definition.
		/// </summary>
		/// <param name="serviceDefinition">The service definition.</param>
		/// <returns>Returns the function definitions for each function in the service definition in the order as they should be configured.</returns>
		private IEnumerable<FunctionDefinition> GetFunctionDefinitions(Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
		{
			var functionDefinitions = new List<FunctionDefinition>();
			var nodes = serviceDefinition.Diagram.Nodes.Where(x => !serviceDefinition.Diagram.Edges.Any(y => y.ToNodeID == x.ID)).ToList();

			while (nodes.Any())
			{
				var childNodes = new List<Node>();
				foreach (var node in nodes)
				{
					functionDefinitions.Add(GetFunctionDefinitionOfNode(node));
					childNodes.AddRange(serviceDefinition.Diagram.Edges.Where(x => x.FromNodeID == node.ID).Select(x => x.ToNode));
				}

				// this is used to make sure the function definitions are ordered the same as the nodes in the SD
				nodes.Clear();
				nodes.AddRange(childNodes);
			}

			return functionDefinitions;
		}

		private FunctionDefinition GetFunctionDefinitionOfNode(Node node)
		{
			int order = -1;
			string resourcePool = String.Empty;
			bool isHidden = false;

			foreach (var property in node.Properties)
			{
				switch (property.Name)
				{
					case "ConfigurationOrder":
						if (Int32.TryParse(property.Value, out var configurationOrder)) order = configurationOrder;
						break;
					case "Resource Pool":
						resourcePool = property.Value;
						break;
					case "IsHidden":
						isHidden = !String.IsNullOrEmpty(property.Value) && Convert.ToBoolean(property.Value);
						break;
					default:
						// Unsupported property
						break;
				}
			}

			return GetFunctionDefinition(node.Configuration.FunctionID, node.Label, order, resourcePool, isHidden);
		}

		private FunctionDefinition GetFunctionDefinition(Guid functionId, string label, int order, string resourcePool, bool isHidden)
		{
			var srmFunctionDefinition = AllProtocolFunctionDefinitions.SingleOrDefault(fd => fd.GUID == functionId) ?? throw new FunctionDefinitionNotFoundException(functionId);

			try
			{
				var functionDefinition = new FunctionDefinition
				{
					Id = srmFunctionDefinition.GUID,
					Name = srmFunctionDefinition.Name,
					Label = label,
					ConfigurationOrder = order,
					IsHidden = isHidden,
					ProfileDefinition = Helpers.ProfileManager.GetProfileDefinition(srmFunctionDefinition.ProfileDefinition),
					InputInterfaces = srmFunctionDefinition.InputInterfaces.Select(ii => new FunctionInterfaceDefinition(Helpers, ii)).ToList(),
					OutputInterfaces = srmFunctionDefinition.OutputInterfaces.Select(oi => new FunctionInterfaceDefinition(Helpers, oi)).ToList(),
					ResourcePool = resourcePool,
					Children = AllProtocolFunctionDefinitions.Where(fd => fd.ParentFunctionGUID == srmFunctionDefinition.GUID).Select(fd => fd.GUID).ToList(),
				};

				return functionDefinition;
			}
			catch (Exception ex)
			{
				throw new FunctionNotFoundException($"Unable to construct function definition for {srmFunctionDefinition.Name} [{srmFunctionDefinition.GUID}]", ex);
			}
		}

		private List<Net.Messages.FunctionDefinition> GetAllProtocolFunctionDefinitions()
		{
			LogMethodStart(nameof(GetAllProtocolFunctionDefinitions), out var stopwatch);

			var functionDefinitions = new List<Skyline.DataMiner.Net.Messages.FunctionDefinition>();

			var protocolFunctions = DataMinerInterface.ProtocolFunctionManager.GetAllProtocolFunctions(Helpers);
			foreach (var protocolFunction in protocolFunctions)
			{
				foreach (var protocolFunctionVersion in protocolFunction.ProtocolFunctionVersions.Where(x => x.Active)) // only consider active function definitions
				{
					foreach (var functionDefinition in protocolFunctionVersion.FunctionDefinitions.Where(x => !functionDefinitions.Exists(fd => fd.GUID.Equals(x.GUID))))
					{
						functionDefinitions.Add(functionDefinition);
					}
				}
			}

			Log(nameof(GetAllProtocolFunctionDefinitions), $"Retrieved {functionDefinitions.Count} protocol function definitions.");

			LogMethodCompleted(nameof(GetAllProtocolFunctionDefinitions), stopwatch);

			return functionDefinitions;
		}

		/// <summary>
		/// Creates a new Service Definition of updates the existing one if necessary based on data in the Order.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="servicesToRemove"></param>
		/// <returns></returns>
		private Net.ServiceManager.Objects.ServiceDefinition BuildServiceDefinitionFromOrder(Order order, List<Service> servicesToRemove)
		{
			LogMethodStart(nameof(BuildServiceDefinitionFromOrder), out var stopwatch);

			Net.ServiceManager.Objects.ServiceDefinition updatedServiceDefinition;

			if (order.Definition == null || order.Definition.Id == Guid.Empty)
			{
				// New Order
				updatedServiceDefinition = NewServiceDefinition();
				order.AllServices.ForEach(service => service.NodeId = 0); // reset node IDs for all services to avoid having multiple nodes with same ID
			}
			else
			{
				// Edit Order
				updatedServiceDefinition = DataMinerInterface.ServiceManager.GetServiceDefinition(Helpers, order.Definition.Id);

				// Update existing Service Definition
				// functions/nodes of services that should be removed need to be removed from the service definition
				var servicesToRemoveIds = servicesToRemove.Select(s => s.NodeId);
				updatedServiceDefinition.Diagram.Nodes.RemoveAll(n => servicesToRemoveIds.Contains(n.ID));
				updatedServiceDefinition.Diagram.Edges.RemoveAll(e => servicesToRemoveIds.Contains(e.FromNodeID) || servicesToRemoveIds.Contains(e.ToNodeID));

				Log(nameof(BuildServiceDefinitionFromOrder), $"Removed nodes {string.Join(",", servicesToRemove.Select(s => $"{s.NodeId}({s.Name})"))} from existing order definition", order.Name);
			}

			int row = 0;
			int column = 0;
			foreach (var sourceService in order.Sources)
			{
				++row;
				AddOrUpdateServiceToServiceDefinition(updatedServiceDefinition, sourceService, ref row, ref column, null);
				column = 0;
			}

			var nodeIdsToRemove = new HashSet<int>();
			foreach (var node in updatedServiceDefinition.Diagram.Nodes)
			{
				int amountOfNodesWithSameId = updatedServiceDefinition.Diagram.Nodes.Count(n => n.ID == node.ID);
				if (amountOfNodesWithSameId > 1)
				{
					throw new InvalidOperationException($"There are {amountOfNodesWithSameId} nodes with ID {node.ID}, coming from services {string.Join(",", order.AllServices.Where(s => s.NodeId == node.ID))}");
				}

				var nodesWithSamePosition = updatedServiceDefinition.Diagram.Nodes.Where(n => n.Position.Equals(node.Position)).ToList();

				foreach (var nodeWithSamePosition in nodesWithSamePosition)
				{
					bool noServiceLinkedToThisNode = !order.AllServices.Exists(s => s.NodeId == nodeWithSamePosition.ID);
					if (noServiceLinkedToThisNode)
					{
						nodeIdsToRemove.Add(nodeWithSamePosition.ID);

						Log(nameof(BuildServiceDefinitionFromOrder), $"Node {nodeWithSamePosition.ID} at position {nodeWithSamePosition.Position} should be removed from the order definition as there is no service linked to it");
					}
				}
			}

			foreach (var nodeIdToRemove in nodeIdsToRemove)
			{
				updatedServiceDefinition.Diagram.Nodes.RemoveAll(n => n.ID == nodeIdToRemove);
				updatedServiceDefinition.Diagram.Edges.RemoveAll(e => e.FromNodeID == nodeIdToRemove || e.ToNodeID == nodeIdToRemove);

				Log(nameof(BuildServiceDefinitionFromOrder), $"Removed node {nodeIdToRemove}");
			}

			LogMethodCompleted(nameof(BuildServiceDefinitionFromOrder), stopwatch);

			return updatedServiceDefinition;
		}

		private void AddOrUpdateServiceToServiceDefinition(Net.ServiceManager.Objects.ServiceDefinition serviceDefinition, Service service, ref int row, ref int column, Service parentService = null)
		{
			LogMethodStart(nameof(AddOrUpdateServiceToServiceDefinition), out var stopwatch);

			UpdateServiceNodeId(serviceDefinition, service);

			if (parentService != null)
			{
				// update column based on parent node column
				var parentNode = serviceDefinition.Diagram.Nodes.FirstOrDefault(n => n.ID == parentService.NodeId) ?? throw new NotFoundException($"Could not find node with ID {parentService.NodeId}");
				column = parentNode.Position.Column + 1;
			}

			var contributingConfig = service.Definition.ContributingConfig ?? throw new InvalidOperationException(String.Format("Contributing config for service {0} is null.", service.Name));
			if (!Guid.TryParse(contributingConfig.ParentSystemFunction, out var parentSystemFunctionId))
			{
				throw new InvalidOperationException(String.Format("Invalid parent system function {0} in contributing config for service {1}.", contributingConfig.ParentSystemFunction, service.Name));
			}

			var parentFunctionDefinition = GetSystemFunctionDefinition(parentSystemFunctionId) ?? throw new InvalidOperationException(String.Format("Parent Function Definition with ID {0} was not found.", parentSystemFunctionId));

			var node = DetermineServiceDefinitionNode(serviceDefinition, service, parentFunctionDefinition, contributingConfig, ref row, ref column);

			service.Interfaces = AddInterfacesToServiceDefinitionNode(node, parentFunctionDefinition);

			// Add edge between parent and this node if it doesn't exist yet
			if (parentService != null && !serviceDefinition.Diagram.Edges.Exists(x => x.FromNodeID == parentService.NodeId && x.ToNodeID == service.NodeId))
			{
				AddEdge(service, parentService, serviceDefinition);
			}

			// Remove edges between the parent node and any child nodes that are no longer its direct child
			if (parentService != null)
			{
				RemoveEdges(parentService, serviceDefinition);
			}

			// Add children nodes of this parent to the service definition
			if (service.Children != null && service.Children.Any())
			{
				AddChildServices(service, serviceDefinition, ref column, ref row);
			}

			LogMethodCompleted(nameof(AddOrUpdateServiceToServiceDefinition), stopwatch);
		}

		private void UpdateServiceNodeId(Net.ServiceManager.Objects.ServiceDefinition serviceDefinition, Service service)
		{
			if (service.NodeId == 0)
			{
				service.NodeId = serviceDefinition.Diagram.Nodes.Any() ? serviceDefinition.Diagram.Nodes.Max(n => n.ID) + 1 : 1;

				Log(nameof(UpdateServiceNodeId), $"Selected new node ID {service.NodeId}", service.Name);
			}
			else
			{
				Log(nameof(UpdateServiceNodeId), $"Service already has node ID {service.NodeId}", service.Name);
			}
		}

		private Node DetermineServiceDefinitionNode(Net.ServiceManager.Objects.ServiceDefinition serviceDefinition, Service service, SystemFunctionDefinition parentSystemFunctionDefinition, ContributingConfig contributingConfig, ref int row, ref int column)
		{
			var node = serviceDefinition.Diagram.Nodes.SingleOrDefault(n => n.ID == service.NodeId);
			if (node == null)
			{
				node = new Node
				{
					ID = service.NodeId,
					Position = new Position { Row = row, Column = column },
					Properties = new[]
					{
						new Property { Name = "Options", Value = "Optional" },
						new Property { Name = "Resource Pool", Value = contributingConfig.ResourcePool },
						new Property { Name = "IsContributing", Value = "TRUE" },
						new Property { Name= "IsProfileInstanceOptional", Value = "TRUE" }
					}.ToList()
				};

				serviceDefinition.Diagram.Nodes.Add(node);
			}
			else
			{
				node.Position.Row = row;
				node.Position.Column = column;
			}

			node.Label = $"{parentSystemFunctionDefinition.Name} [{service.NodeId}]";
			service.NodeLabel = node.Label;

			if (node.Configuration == null) node.Configuration = new NodeConfiguration { FunctionID = parentSystemFunctionDefinition.GUID };
			else node.Configuration.FunctionID = parentSystemFunctionDefinition.GUID;

			return node;
		}

		private void AddEdge(Service service, Service parentService, Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
		{
			var interfacesToConnect = GetInterfacesToConnect(service, parentService);
			var selectedParentFunctionInterface = interfacesToConnect[0];
			var selectedFunctionInterface = interfacesToConnect[1];

			if (selectedParentFunctionInterface != null && selectedFunctionInterface != null)
			{
				var edge = new Edge { FromNodeID = parentService.NodeId, ToNodeID = service.NodeId, FromNodeInterfaceID = selectedParentFunctionInterface.ID, ToNodeInterfaceID = selectedFunctionInterface.ID };

				serviceDefinition.Diagram.Edges.Add(edge);

				Log(nameof(AddEdge), $"Added edge from {edge.FromNodeID} to {edge.ToNodeID}");

				// update the column for the destination node
				var edgeSourceNode = serviceDefinition.Diagram.Nodes.FirstOrDefault(n => n.ID == parentService.NodeId) ?? throw new NotFoundException($"Unable to find child of node {parentService.NodeId}");
				var edgeDestinationNode = serviceDefinition.Diagram.Nodes.FirstOrDefault(n => n.ID == service.NodeId) ?? throw new NotFoundException($"Unable to find node {service.NodeId}");
				edgeDestinationNode.Position.Column = edgeSourceNode.Position.Column + 1;
			}
			else
			{
				Log(nameof(AddEdge), $"Unable to create edge from {parentService.Name} to {service.Name}");
			}
		}

		private InterfaceConfiguration[] GetInterfacesToConnect(Service service, Service parentService)
		{
			InterfaceConfiguration selectedParentFunctionInterface = null;
			InterfaceConfiguration selectedFunctionInterface = null;
			foreach (InterfaceConfiguration parentFunctionInterface in parentService.Interfaces)
			{
				// only parent output can be connected to child input
				if (parentFunctionInterface.Type != InterfaceType.Out && parentFunctionInterface.Type != InterfaceType.InOut) continue;

				foreach (InterfaceConfiguration interfaceConfiguration in service.Interfaces)
				{
					// only parent output can be connected to child input
					if (interfaceConfiguration.Type != InterfaceType.In && interfaceConfiguration.Type != InterfaceType.InOut) continue;

					if (TryConnectInterfaces(parentFunctionInterface, interfaceConfiguration, out InterfaceConfiguration selectedParent, out InterfaceConfiguration selected))
					{
						selectedParentFunctionInterface = selectedParent;
						selectedFunctionInterface = selected;
					}
				}
			}

			return new[] { selectedParentFunctionInterface, selectedFunctionInterface };
		}

		private bool TryConnectInterfaces(InterfaceConfiguration parentFunctionInterface, InterfaceConfiguration interfaceConfiguration, out InterfaceConfiguration selectedParentFunctionInterface, out InterfaceConfiguration selectedFunctionInterface)
		{
			selectedParentFunctionInterface = null;
			selectedFunctionInterface = null;

			// interfaces can be connected if they have the same profile definition
			if (parentFunctionInterface.ProfileDefinitionID.Equals(interfaceConfiguration.ProfileDefinitionID))
			{
				selectedParentFunctionInterface = parentFunctionInterface;
				selectedFunctionInterface = interfaceConfiguration;
				return true;
			}
			else
			{
				// check if they have the same parent
				var fromInterfaceProfileDefinition = GetInterfaceProfileDefinition(parentFunctionInterface.ProfileDefinitionID);
				var toInterfaceProfileDefinition = GetInterfaceProfileDefinition(interfaceConfiguration.ProfileDefinitionID);
				if (fromInterfaceProfileDefinition != null && toInterfaceProfileDefinition != null && fromInterfaceProfileDefinition.BasedOnIDs.Intersect(toInterfaceProfileDefinition.BasedOnIDs).Any())
				{
					selectedParentFunctionInterface = parentFunctionInterface;
					selectedFunctionInterface = interfaceConfiguration;
					return true;
				}
			}

			Log(nameof(TryConnectInterfaces), $"Unable to find connected interfaces between");

			return false;
		}

		private void RemoveEdges(Service parentService, Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
		{
			foreach (var edgeComingFromParent in serviceDefinition.Diagram.Edges.Where(e => e.FromNodeID == parentService.NodeId).ToList())
			{
				if (!parentService.Children.Any(c => c.NodeId == edgeComingFromParent.ToNodeID))
				{
					serviceDefinition.Diagram.Edges.Remove(edgeComingFromParent);
					Log(nameof(RemoveEdges), $"Removing edge from {edgeComingFromParent.FromNodeID} to {edgeComingFromParent.ToNodeID}");
				}
			}
		}

		private void AddChildServices(Service service, Net.ServiceManager.Objects.ServiceDefinition serviceDefinition, ref int column, ref int row)
		{
			column++;

			bool firstChild = true;
			foreach (var childService in service.Children)
			{
				if (!firstChild)
				{
					row++;
				}

				firstChild = false;

				AddOrUpdateServiceToServiceDefinition(serviceDefinition, childService, ref row, ref column, service);
			}
		}

		public SystemFunctionDefinition GetSystemFunctionDefinition(Guid id)
		{
			LogMethodStart(nameof(GetSystemFunctionDefinition), out var stopwatch);

			var systemFunctionDefinition = cachedSystemFunctionDefinitions.SingleOrDefault(fd => fd.GUID == id);

			if (systemFunctionDefinition == null)
			{
				systemFunctionDefinition = DataMinerInterface.ProtocolFunctionManager.GetFunctionDefinition(Helpers, new FunctionDefinitionID(id));

				if (systemFunctionDefinition != null)
				{
					cachedSystemFunctionDefinitions.Add(systemFunctionDefinition);
				}
			}

			LogMethodCompleted(nameof(GetSystemFunctionDefinition), stopwatch);

			return systemFunctionDefinition;
		}

		private IEnumerable<InterfaceConfiguration> AddInterfacesToServiceDefinitionNode(Node node, SystemFunctionDefinition functionDefinition)
		{
			var nodeInterfaces = node.InterfaceConfigurations != null ? node.InterfaceConfigurations.ToList() : new List<InterfaceConfiguration>();
			var nodeInterfacesToRemove = nodeInterfaces.Select(i => i.ID).ToList();

			foreach (var inputInterface in functionDefinition.InputInterfaces)
			{
				DefineNodeInterface(inputInterface, nodeInterfaces, nodeInterfacesToRemove, InterfaceType.In);
			}

			foreach (var outputInterface in functionDefinition.OutputInterfaces)
			{
				DefineNodeInterface(outputInterface, nodeInterfaces, nodeInterfacesToRemove, InterfaceType.Out);
			}

			foreach (var inputOutputInterface in functionDefinition.InputOutputInterfaces)
			{
				DefineNodeInterface(inputOutputInterface, nodeInterfaces, nodeInterfacesToRemove, InterfaceType.InOut);
			}

			if (nodeInterfacesToRemove.Any())
			{
				nodeInterfaces.RemoveAll(n => nodeInterfacesToRemove.Contains(n.ID));
			}

			node.InterfaceConfigurations = nodeInterfaces.ToArray();
			return nodeInterfaces;
		}

		private void DefineNodeInterface(Net.Messages.FunctionInterface functionInterface, List<InterfaceConfiguration> nodeInterfaces, List<int> nodeInterfacesToRemove, InterfaceType interfaceType)
		{
			var nodeInterface = nodeInterfaces.FirstOrDefault(i => i.ID == functionInterface.Id);
			if (nodeInterface == null)
			{
				nodeInterfaces.Add(new InterfaceConfiguration
				{
					ID = functionInterface.Id,
					Type = interfaceType,
					ProfileDefinitionID = functionInterface.ProfileDefinition,
					Properties = new List<Property>
						{
							new Property { Name= "IsProfileInstanceOptional", Value = "TRUE" }
}
				});
			}
			else
			{
				nodeInterfacesToRemove.Remove(functionInterface.Id);

				if (nodeInterface.Type != interfaceType)
				{
					nodeInterface.Type = interfaceType;
				}
			}
		}

		private Net.Profiles.ProfileDefinition GetInterfaceProfileDefinition(Guid profileDefinitionId)
		{
			if (!cachedInterfaceProfileDefinitions.TryGetValue(profileDefinitionId, out var profileDefinition))
			{
				profileDefinition = DataMinerInterface.ProfileHelper.ReadProfileDefinitions(Helpers, ProfileDefinitionExposers.ID.Equal(profileDefinitionId)).FirstOrDefault();

				cachedInterfaceProfileDefinitions.Add(profileDefinitionId, profileDefinition);
			}

			return profileDefinition;
		}

		private Net.ServiceManager.Objects.ServiceDefinition TryUseExistingServiceDefinition(Net.ServiceManager.Objects.ServiceDefinition newServiceDefinition)
		{
			LogMethodStart(nameof(TryUseExistingServiceDefinition), out var stopwatch);

			var isTemplateFilter = ServiceDefinitionExposers.IsTemplate.Equal(true);
			var nameDoesntStartWithUnderscoreFilter = ServiceDefinitionExposers.Name.NotMatches("_.*");
			var nameDoesntContainDefaultFilter = ServiceDefinitionExposers.Name.NotContains("Default");

			var distinctNodeFunctionIds = newServiceDefinition.Diagram.Nodes.Select(n => n.Configuration.FunctionID).Distinct().ToList();
			var functionIdsToIncludeFilters = distinctNodeFunctionIds.Select(id => ServiceDefinitionExposers.NodeFunctionIDs.Contains(id)).ToArray();

			var unusedFunctionIds = SystemFunctionGuids.All().Except(distinctNodeFunctionIds);
			var functionIdsToNotIncludeFilters = unusedFunctionIds.Select(id => ServiceDefinitionExposers.NodeFunctionIDs.NotContains(id)).ToArray();

			var diagramHashCodePropertyFilter = ServiceDefinitionExposers.Properties.DictField(ServiceDefinitionPropertyNames.DiagramHashCode).Equal(newServiceDefinition.Diagram.GetHashCodeForYleProject());

			var combinedFilter = isTemplateFilter.AND(nameDoesntStartWithUnderscoreFilter).AND(nameDoesntContainDefaultFilter).AND(functionIdsToIncludeFilters).AND(functionIdsToNotIncludeFilters).AND(diagramHashCodePropertyFilter);

			var existingServiceDefinitions = DataMinerInterface.ServiceManager.GetServiceDefinitions(Helpers,
				combinedFilter);

			Log(nameof(TryUseExistingServiceDefinition), $"Found {existingServiceDefinitions.Count()} existing order definitions matching the Elastic filters.");

			var existingServiceDefinition = existingServiceDefinitions.FirstOrDefault(x => x.Diagram.Matches(newServiceDefinition.Diagram));

			if (existingServiceDefinition != null)
			{
				Log(nameof(TryUseExistingServiceDefinition), $"Found existing order definition {existingServiceDefinition.ID} that matches the definition for the order. This definition will be reused.");
				LogMethodCompleted(nameof(TryUseExistingServiceDefinition), stopwatch);
				return existingServiceDefinition;
			}

			Log(nameof(TryUseExistingServiceDefinition), $"Unable to find an existing order definition that matches the definition for the order.");

			LogMethodCompleted(nameof(TryUseExistingServiceDefinition), stopwatch);

			return newServiceDefinition;
		}

		private Net.ServiceManager.Objects.ServiceDefinition NewServiceDefinition()
		{
			var newServiceDefinitionId = Guid.NewGuid();
			return new Net.ServiceManager.Objects.ServiceDefinition
			{
				ID = newServiceDefinitionId,
				Name = newServiceDefinitionId.ToString(),
				Description = string.Empty,
				Diagram = new Graph { Nodes = new List<Node>(), Edges = new List<Edge>() },
				IsTemplate = true,
				Properties = new[] { new Property { Name = "Virtual Platform", Value = "Order" } }.ToList(),
			};
		}

		private string GetBookingManagerElementName(Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
		{
			LogMethodStart(nameof(GetBookingManagerElementName), out var stopwatch);

			var virtualPlatformProperty = serviceDefinition.Properties.FirstOrDefault(p => p.Name == "Virtual Platform");
			if (virtualPlatformProperty == null)
			{
				throw new InvalidOperationException(String.Format("ServiceDefinition with ID {0} does not contain a Virtual Platform property", serviceDefinition.ID));
			}

			var bookingManager = GetBookingManager(EnumExtensions.GetEnumValueFromDescription<VirtualPlatform>(virtualPlatformProperty.Value));

			LogMethodCompleted(nameof(GetBookingManagerElementName), stopwatch);

			return bookingManager?.ElementName;
		}

		private ContributingConfig GetContributingConfig(Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
		{
			ContributingConfig contributingConfig = null;
			try
			{
				var contributingConfigProperty = serviceDefinition.Properties.FirstOrDefault(p => String.Equals(p.Name, ContributiongConfigPropertyName, StringComparison.InvariantCultureIgnoreCase)) ?? throw new PropertyNotFoundException(ContributiongConfigPropertyName);

				contributingConfig = JsonConvert.DeserializeObject<ContributingConfig>(contributingConfigProperty.Value);
			}
			catch (Exception ex)
			{
				Log(nameof(GetContributingConfig), $"Exception while retrieving contributing config: {ex}");
			}

			return contributingConfig;
		}

		private static void PrepareServiceDefinitionToBeCreatedAsNew(Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
		{
			serviceDefinition.ID = Guid.NewGuid();
			serviceDefinition.Name = serviceDefinition.ID.ToString();
		}

		private void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			Helpers.Log(nameof(ServiceDefinitionManager), nameOfMethod, message, nameOfObject);
		}

		protected void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch)
		{
			Helpers.LogMethodStart(nameof(ServiceDefinitionManager), nameOfMethod, out stopwatch);
		}

		protected void LogMultiThreadedMethodStart(string nameOfMethod, out Stopwatch stopwatch)
		{
			Helpers.LogMethodStart(nameof(ServiceDefinitionManager), nameOfMethod, out stopwatch, null, true);
		}

		protected void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch = null)
		{
			Helpers.LogMethodCompleted(nameof(ServiceDefinitionManager), nameOfMethod, null, stopwatch);
		}

		protected void LogMultiThreadedMethodCompleted(string nameOfMethod, Stopwatch stopwatch = null)
		{
			Helpers.LogMethodCompleted(nameof(ServiceDefinitionManager), nameOfMethod, null, stopwatch, true);
		}

		private List<Net.ServiceManager.Objects.ServiceDefinition> GetAllNonOrderSrmServiceDefinitions()
		{
			LogMethodStart(nameof(GetAllNonOrderSrmServiceDefinitions), out var stopwatch);

			var result = DataMinerInterface.ServiceManager.GetServiceDefinitions(Helpers, ServiceDefinitionExposers.Properties.DictStringField("Virtual Platform").NotEqual("Order").AND(ServiceDefinitionExposers.Name.NotContains("Default"))).Where(sd => sd.Name.StartsWith("_")).ToList();

			Log(nameof(GetAllNonOrderSrmServiceDefinitions), $"Retrieved {result.Count} Service Definitions");

			LogMethodCompleted(nameof(GetAllNonOrderSrmServiceDefinitions), stopwatch);

			return result;
		}
	}
}