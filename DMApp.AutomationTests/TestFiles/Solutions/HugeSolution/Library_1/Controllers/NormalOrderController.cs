namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ExternalSources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Service = Service.Service;

	public class NormalOrderController : OrderController
	{
		private NormalOrderSection orderSection;

		private readonly IReadOnlyDictionary<VirtualPlatformType, List<List<DisplayedService>>> cachedSourceChildServices = new Dictionary<VirtualPlatformType, List<List<DisplayedService>>>
		{
			{ VirtualPlatformType.Destination, new List<List<DisplayedService>>() },
			{ VirtualPlatformType.Recording, new List<List<DisplayedService>>() },
			{ VirtualPlatformType.Transmission, new List<List<DisplayedService>>() }
		};

		private readonly IReadOnlyDictionary<VirtualPlatformType, List<List<DisplayedService>>> cachedBackupSourceChildServices = new Dictionary<VirtualPlatformType, List<List<DisplayedService>>>
		{
			{ VirtualPlatformType.Destination, new List<List<DisplayedService>>() },
			{ VirtualPlatformType.Recording, new List<List<DisplayedService>>() },
			{ VirtualPlatformType.Transmission, new List<List<DisplayedService>>() }
		};

		private readonly HashSet<ExternalSourceInfo> externalSources = new HashSet<ExternalSourceInfo>();

		public NormalOrderController(Helpers helpers, Order order, UserInfo userInfo, NormalOrderSection orderSection = null, IEnumerable<Service> controlledServicesWhereOrderRefersTo = null) : base(helpers, order, userInfo, orderSection, controlledServicesWhereOrderRefersTo)
		{
			Initialize();
		}

		public override OrderSection OrderSection => orderSection;

		protected override IReadOnlyDictionary<VirtualPlatformType, List<List<DisplayedService>>> CachedSourceChildServices => cachedSourceChildServices;

		public override void AddOrReplaceSection(OrderSection orderSection)
		{
			this.orderSection = orderSection as NormalOrderSection;

			SubscribeToUi();
			AddAllServiceSectionsToServiceControllers();
		}

		protected override IEnumerable<DisplayedService> GetCachedServices()
		{
			IEnumerable<DisplayedService> services = new List<DisplayedService>(cachedReceptionServices.Select(x => x.Value));
			foreach (var kvp in cachedSourceChildServices)
			{
				foreach (var cachedService in kvp.Value)
				{
					services = services.Concat(cachedService);
				}
			}

			foreach (var kvp in cachedBackupSourceChildServices)
			{
				foreach (var cachedService in kvp.Value)
				{
					services = services.Concat(cachedService);
				}
			}

			return services.ToList();
		}

		public override void HandleSelectedResourceUpdate(Service service, Function function)
		{
			using (StartPerformanceLogging())
			{
				if (LiveVideoOrder == null)
				{
					Log(nameof(HandleSelectedResourceUpdate), $"{nameof(LiveVideoOrder)} is null");
					return;
				}

				SetResourcesOnOtherServices(service, function);

				ConditionalSetFixedTieLineOverrideProperty(service, function);

				HandleAudioProcessingServiceChanges(service);
			}
		}

		private void SetResourcesOnOtherServices(Service service, Function function)
		{
			if (service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception)
			{
				TryUpdateDestinationResourcesBasedOnFixedTieLines(service);
			}

			if (function.Resource != null && service.Definition.VirtualPlatform != VirtualPlatform.Routing)
			{
				UnenforceSelectedResourceForRoutingServicesLinkedTo(service);
			}

			if (function.Resource == null)
			{
				LiveVideoOrder.ClearMatchingResourceOnNeighborServiceConnectedFunction(service, function);
			}
			else if (service.Definition.VirtualPlatform == VirtualPlatform.Routing)
			{
				var routingServiceChainsWhereServiceIsPartOfTieLine = LiveVideoOrder.GetRoutingServiceChainsForService(service.Id).Where(rsc => rsc.UsesMultipleRoutingServices).ToList();
				foreach (var routingServiceChain in routingServiceChainsWhereServiceIsPartOfTieLine)
				{
					// Set the correct tie line resource on the neighbor routing service 

					routingServiceChain.SetMatchingResourceOnNeighborServiceConnectedFunction(service, function);
				}

				TryUpdateRecordingResourceBasedOnFixedTieLines(service);
			}
			else
			{
				/* Resource changes (excluding setting resource to None/null) on non-routing services
				 * will result in routing reprocessing in the Book Services logic in the background
				 * causing the routing services to have the correct resource.
				 * So no need to set routing resources based on non-routing resource changes here.*/
			}
		}

		private void UnenforceSelectedResourceForRoutingServicesLinkedTo(Service service)
		{
			// If a new resource is selected for a non-routing service, set the EnforceSelectedResource properties on all functions of all routings in the routing chains for that service to false. This will make sure a valid routing chain will be generated for this newly selected resource.

			var routingServiceChainsForThisService = LiveVideoOrder.GetRoutingServiceChainsForService(service.Id);

			var allRoutingServices = routingServiceChainsForThisService.SelectMany(rsc => rsc.AllRoutingServices).ToList();

			var allFunctionsFromRoutingResources = allRoutingServices.SelectMany(rs => rs.Service.Functions).ToList();

			allFunctionsFromRoutingResources.ForEach(f => f.EnforceSelectedResource = false);

			Log(nameof(UnenforceSelectedResourceForRoutingServicesLinkedTo), $"Set {nameof(Function.EnforceSelectedResource)} property to false on services {string.Join(", ", allRoutingServices.Select(s => s.Service.Name))} to allow valid routing chain generation.");
		}

		private void TryUpdateRecordingResourceBasedOnFixedTieLines(Service routingServiceThatChanged)
		{
			/* Changing the routing resource while Messi News Recordings are present in the order
			* might require a change of the recording resource because of the fixed tie line logic. [DCP215073] */

			var messiNewRecordingsConnectedToRouting = LiveVideoOrder.GetRecordings().Where(d => LiveVideoOrder.ServicesArePartOfSameChain(routingServiceThatChanged, d.Service) && d.Service.Definition.Id == ServiceDefinitionGuids.RecordingMessiNews).ToList();

			Log(nameof(TryUpdateRecordingResourceBasedOnFixedTieLines), $"Found Messi News recordings {string.Join(", ", messiNewRecordingsConnectedToRouting.Select(d => d.Service.Name))} connected to routing {routingServiceThatChanged.Name}");

			foreach (var recordingService in messiNewRecordingsConnectedToRouting.Select(x => x.Service))
			{
				var routingServiceChain = LiveVideoOrder.GetRoutingServiceChainsForService(recordingService.Id).SingleOrDefault();
				if (routingServiceChain is null) continue;

				SetResourceOnOutputServiceBasedOnFixedTieLine(routingServiceChain.OutputRoutingService.MatrixInputSdi, recordingService);
			}
		}

		private void SetResourceOnOutputServiceBasedOnFixedTieLine(FunctionResource outputRoutingServiceInputResource, Service recording)
		{
			using (StartPerformanceLogging())
			{
				if (outputRoutingServiceInputResource.GetResourcePropertyBooleanValue(ResourcePropertyNames.IsSpecificTieLine))
				{
					var fixedTieLineSourceCapability = outputRoutingServiceInputResource.Capabilities.SingleOrDefault(c => c.CapabilityProfileID == ProfileParameterGuids.FixedTieLineSource) ?? throw new NotFoundException($"Could not find Fixed Tie Line Source capability on resource {outputRoutingServiceInputResource.Name}");

					string fixedTieLineSourceCapabilityValue = fixedTieLineSourceCapability.Value.Discreets.FirstOrDefault() ?? throw new NotFoundException($"Could not find a value in the Fixed Tie Line Source capability on resource {outputRoutingServiceInputResource.Name}");

					var fixedTieLineSourceResource = helpers.ResourceManager.GetResourcesByName(fixedTieLineSourceCapabilityValue).SingleOrDefault() ?? throw new NotFoundException($"Could not find a resource matching the Fixed Tie Line Source capability value '{fixedTieLineSourceCapabilityValue}' from resource {outputRoutingServiceInputResource.Name}");

					var resourceOutputConnectionsSdiCapability = fixedTieLineSourceResource.Capabilities.SingleOrDefault(c => c.CapabilityProfileID == ProfileParameterGuids.ResourceOutputConnectionsSdi) ?? throw new NotFoundException($"Could not find ResourceOutputConnections_SDI capability on resource {fixedTieLineSourceResource.Name}");

					string resourceOutputConnectionsSdiCapabilityValue = resourceOutputConnectionsSdiCapability.Value.Discreets.FirstOrDefault() ?? throw new NotFoundException($"Could not find a value in the ResourceOutputConnections_SDI capability on resource {fixedTieLineSourceResource.Name}");

					var recordingResourceToSet = helpers.ResourceManager.GetResourcesByName(resourceOutputConnectionsSdiCapabilityValue).OfType<FunctionResource>().SingleOrDefault() ?? throw new NotFoundException($"Could not find a resource matching the ResourceOutputConnections_SDI capability value '{resourceOutputConnectionsSdiCapabilityValue}' from resource {fixedTieLineSourceResource.Name}");

					recordingResourceToSet = OccupiedResource.WrapIfOccupied(helpers, recordingResourceToSet, recording.StartWithPreRoll, recording.EndWithPostRoll, order.Id, recording.Name);

					recording.FirstResourceRequiringFunction.EnforceSelectedResource = true;
					((DisplayedFunction)recording.FirstResourceRequiringFunction).IncludeUnavailableResources = recordingResourceToSet is OccupiedResource occupiedRecordingResourceToSet && occupiedRecordingResourceToSet.IsFullyOccupied;
					recording.FirstResourceRequiringFunction.Resource = recordingResourceToSet;

					Log(nameof(SetResourceOnOutputServiceBasedOnFixedTieLine), $"Set resource {recording.FirstResourceRequiringFunction.ResourceName} on service {recording.Name} based on fixed tie line logic");
				}
				else
				{
					Log(nameof(SetResourceOnOutputServiceBasedOnFixedTieLine), $"Resource {outputRoutingServiceInputResource.Name} is not part of a specific tie line");
				}
			}
		}

		private void TryUpdateDestinationResourcesBasedOnFixedTieLines(Service source)
		{
			/* Changing the source resource while Uutisalue destinations are present in the order
			* might require a change of the destination resource because of the fixed tie line logic. */

			var uutisalueDestinationsForSameSource = LiveVideoOrder.GetDestinations().Where(d => LiveVideoOrder.ServicesArePartOfSameChain(source, d.Service) && d.Service.Definition.Id == ServiceDefinitionGuids.YleHelsinkiDestination && d.Service.Functions.Single().Parameters.Single(p => p.Id == ProfileParameterGuids.YleHelsinkiDestinationLocation).StringValue == "Uutisalue").ToList();

			Log(nameof(TryUpdateDestinationResourcesBasedOnFixedTieLines), $"Found Uutisalue destinations {string.Join(", ", uutisalueDestinationsForSameSource.Select(d => d.Service.Name))} connected to source {source.Name}");

			foreach (var destination in uutisalueDestinationsForSameSource)
			{
				destination.Service.AssignResourcesToFunctions(helpers, order);
			}
		}

		private void ConditionalSetFixedTieLineOverrideProperty(Service service, Function function)
		{
			if (!order.IsBooked)
			{
				Log(nameof(ConditionalSetFixedTieLineOverrideProperty), $"Order is not booked yet, no need to override fixed tie line logic");
				return;
			}

			bool serviceIsDestinationYleHelsinkiUutisalue = service.Definition.Id == ServiceDefinitionGuids.YleHelsinkiDestination && service.Functions.Single().Parameters.Single(p => p.Id == ProfileParameterGuids.YleHelsinkiDestinationLocation).StringValue.Equals("uutisalue");
			bool serviceIsRoutingPartOfTieLine = service.Definition.VirtualPlatform == VirtualPlatform.Routing && LiveVideoOrder.GetLiveVideoService(service).FunctionIsConnectedToNeighborRoutingService(function);

			if (!serviceIsDestinationYleHelsinkiUutisalue && !serviceIsRoutingPartOfTieLine) return;

			var routingServiceChains = LiveVideoOrder.GetRoutingServiceChainsForService(service.Id);
			foreach (var routingServiceChain in routingServiceChains)
			{
				bool inputServiceRequiresSpecificTieLine = routingServiceChain.InputService.OutputResource != null && routingServiceChain.InputService.OutputResource.RequiresSpecificTieLine();
				bool chainUsesFixedTieLineResources = routingServiceChain.GetAllRoutingServiceResources().Exists(r => r.GetResourcePropertyBooleanValue(ResourcePropertyNames.IsSpecificTieLine));

				if (inputServiceRequiresSpecificTieLine || chainUsesFixedTieLineResources)
				{
					function.McrHasOverruledFixedTieLineLogic = true;
					Log(nameof(HandleSelectedResourceUpdate), $"Set service {service.Name} function {function.Name} property {nameof(function.McrHasOverruledFixedTieLineLogic)} to true");
				}
			}
		}

		private void HandleAudioProcessingServiceChanges(Service audioProcessingService)
		{
			if (audioProcessingService?.Definition?.VirtualPlatform == VirtualPlatform.AudioProcessing)
			{
				LiveVideoOrder.UpdateLinkedLiveVideoServicesWhenAudioProcessingChanges(audioProcessingService);
			}
		}

		private void Initialize()
		{
			using (StartPerformanceLogging())
			{
				InitializeServiceDefinitions();
				InitializeSharedSources();
				InitializeCachedServices();
				InitializeDisplayedOrder();
				InitializeServiceControllers();

				if (orderSection is null) return;

				AddOrReplaceSection(orderSection);
			}
		}

		private void InitializeServiceDefinitions()
		{
			using (StartPerformanceLogging())
			{
				var allServiceDefinitions = helpers.ServiceDefinitionManager.ServiceDefinitionsForLiveOrderForm;

				var serviceDefinitions = new Dictionary<VirtualPlatformType, IReadOnlyList<ServiceDefinition>>
				{
					{ VirtualPlatformType.Reception, new List<ServiceDefinition>() },
					{ VirtualPlatformType.Destination, new List<ServiceDefinition>() },
					{ VirtualPlatformType.Recording, new List<ServiceDefinition>() },
					{ VirtualPlatformType.Transmission, new List<ServiceDefinition>() },
					{ VirtualPlatformType.Routing, new List<ServiceDefinition>() },
					{ VirtualPlatformType.AudioProcessing, new List<ServiceDefinition>() },
					{ VirtualPlatformType.VideoProcessing, new List<ServiceDefinition>() },
					{ VirtualPlatformType.GraphicsProcessing, new List<ServiceDefinition>() },
				};

				var allowedServiceDefinitions = new Dictionary<VirtualPlatformType, IReadOnlyList<ServiceDefinition>>
				{
					{ VirtualPlatformType.Reception, new List<ServiceDefinition>() },
					{ VirtualPlatformType.Destination, new List<ServiceDefinition>() },
					{ VirtualPlatformType.Recording, new List<ServiceDefinition>() },
					{ VirtualPlatformType.Transmission, new List<ServiceDefinition>() },
					{ VirtualPlatformType.Routing, new List<ServiceDefinition>() },
					{ VirtualPlatformType.AudioProcessing, new List<ServiceDefinition>() },
					{ VirtualPlatformType.VideoProcessing, new List<ServiceDefinition>() },
					{ VirtualPlatformType.GraphicsProcessing, new List<ServiceDefinition>() },
				};

				var receptions = allServiceDefinitions.ReceptionServiceDefinitions.ToList();
				receptions.Add(ServiceDefinition.GenerateDummyReceptionServiceDefinition());
				receptions.Add(ServiceDefinition.GenerateDummyUnknownReceptionServiceDefinition());
				receptions.Add(ServiceDefinition.GenerateEurovisionReceptionServiceDefinition());
				serviceDefinitions[VirtualPlatformType.Reception] = receptions;
				allowedServiceDefinitions[VirtualPlatformType.Reception] = receptions.Where(x => userInfo.Contract.IsServiceAllowed(x, userInfo)).ToList();

				serviceDefinitions[VirtualPlatformType.Destination] = allServiceDefinitions.DestinationServiceDefinitions.ToList();
				allowedServiceDefinitions[VirtualPlatformType.Destination] = allServiceDefinitions.DestinationServiceDefinitions.Where(x => userInfo.Contract.IsServiceAllowed(x, userInfo)).ToList();

				serviceDefinitions[VirtualPlatformType.Recording] = allServiceDefinitions.RecordingServiceDefinitions.ToList();
				allowedServiceDefinitions[VirtualPlatformType.Recording] = allServiceDefinitions.RecordingServiceDefinitions.Where(x => userInfo.Contract.IsServiceAllowed(x, userInfo)).ToList();

				var transmissions = allServiceDefinitions.TransmissionServiceDefinitions.ToList();
				transmissions.Add(ServiceDefinition.GenerateEurovisionTransmissionServiceDefinition());
				serviceDefinitions[VirtualPlatformType.Transmission] = transmissions;
				allowedServiceDefinitions[VirtualPlatformType.Transmission] = transmissions.Where(x => userInfo.Contract.IsServiceAllowed(x, userInfo)).ToList();

				serviceDefinitions[VirtualPlatformType.Routing] = new List<ServiceDefinition> { allServiceDefinitions.RoutingServiceDefinition };

				this.serviceDefinitions = serviceDefinitions;
				this.allowedServiceDefinitions = allowedServiceDefinitions;
			}
		}

		private void InitializeSharedSources()
		{
			if (Array.Exists(userInfo.AllUserContracts, x => x != null && x.ContractGlobalEventLevelReceptionConfiguration.HasFlag(GlobalEventLevelReceptionConfigurations.GlobalEventLevelReceptionUsageAllowed)))
			{
				// Filter out shared sources that end in the past
				var sharedSources = helpers.ServiceManager.GetSharedSourceReservationsEndingInFuture().Select(r => new SharedSourceInfo(helpers, r));
				foreach (var sharedSource in sharedSources) externalSources.Add(sharedSource);
			}
		}

		private void UpdateNonSharedSources()
		{
			if (Array.Exists(userInfo.AllUserContracts, x => x != null && x.ContractGlobalEventLevelReceptionConfiguration.HasFlag(GlobalEventLevelReceptionConfigurations.GlobalEventLevelReceptionUsageAllowed)) && (orderSection?.IncludeExistingSources ?? false))
			{
				var shareableSources = helpers.ServiceManager.GetShareableSourceReservations(order.Start, order.End).Select(r => new NonSharedSourceInfo(helpers, r));

				foreach (var shareableSource in shareableSources)
				{
					var orderIdsProperty = shareableSource.Reservation.Properties.FirstOrDefault(x => x.Key.Equals(ServicePropertyNames.OrderIdsPropertyName));
					if (orderIdsProperty.Equals(default(KeyValuePair<string, object>))) continue; // In case service does not have the Order IDs property
					if (Convert.ToString(orderIdsProperty.Value).Split(';').Contains(order.Id.ToString())) continue; // Filter out already booked source service for this order

					externalSources.Add(shareableSource);
				}
			}
			else
			{
				externalSources.RemoveWhere(x => x is NonSharedSourceInfo);
			}
		}

		protected override void GenerateNewChildService(Service previousDisplayedService, string virtualPlatformName, string description, out DisplayedService newDisplayedService, out ServiceDefinition newServiceDefinition)
		{
			using (StartPerformanceLogging())
			{
				newServiceDefinition = serviceDefinitions[previousDisplayedService.Definition.VirtualPlatformServiceType].FirstOrDefault(x => x.VirtualPlatformServiceName.GetDescription() == virtualPlatformName && x.Description == description) ?? throw new NotFoundException($"Unable to find Service Definition with virtual platform {previousDisplayedService.Definition.VirtualPlatformServiceType.GetDescription()}.{virtualPlatformName} and description {description}");

				newDisplayedService = new DisplayedService(helpers, newServiceDefinition)
				{
					Start = previousDisplayedService.Start,
					End = previousDisplayedService.End,
					BackupType = previousDisplayedService.BackupType,
					AvailableServicesToRecordOrTransmit = order.AllServices.Where(x => x.BackupType == previousDisplayedService.BackupType && (x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception || x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Destination)).ToList(),
					NameOfServiceToTransmitOrRecord = (previousDisplayedService.BackupType == BackupType.None) ? order.SourceService.Name : order.BackupSourceService?.Name
				};

				if (newDisplayedService.Definition.VirtualPlatform == VirtualPlatform.Recording)
				{
					newDisplayedService.RecordingConfiguration.IsConfigured = true;
					newDisplayedService.RecordingConfiguration.RecordingName = order.Name;
					newDisplayedService.RecordingConfiguration.SelectableRecordingFileDestinations = newDisplayedService.Definition.Description.Contains("Live") ? new List<string> { FileDestination.ArchiveMetro.GetDescription() } : new List<string> { FileDestination.UaIplay.GetDescription() };
					newDisplayedService.RecordingConfiguration.RecordingFileDestination = newDisplayedService.Definition.Description.Contains("Live") ? FileDestination.ArchiveMetro : FileDestination.UaIplay;

					newDisplayedService.RecordingConfiguration.SelectableEvsMessiNewsTargets = helpers.OrderManagerElement.GetEvsMessiNewsTargets(newServiceDefinition);
					newDisplayedService.RecordingConfiguration.EvsMessiNewsTarget = newDisplayedService.RecordingConfiguration.SelectedEvsMessiNewsTarget?.Target;
				}

				newDisplayedService.RecordingConfiguration.RecordingName = order.Name;
				newDisplayedService.LofDisplayName = previousDisplayedService.LofDisplayName;

				if (newDisplayedService.BackupType == BackupType.None)
				{
					newDisplayedService.AudioChannelConfiguration?.CopyFromSource(order.SourceService.AudioChannelConfiguration);
					newDisplayedService.AudioChannelConfiguration?.SetSourceOptions(order.SourceService.AudioChannelConfiguration.GetSourceOptions());

					SetValuesBasedOnSourceService(newDisplayedService, order.SourceService);
				}
				else if (order.BackupSourceService != null)
				{
					newDisplayedService.AudioChannelConfiguration?.CopyFromSource(order.BackupSourceService.AudioChannelConfiguration);
					newDisplayedService.AudioChannelConfiguration?.SetSourceOptions(order.BackupSourceService.AudioChannelConfiguration.GetSourceOptions());

					SetValuesBasedOnSourceService(newDisplayedService, order.BackupSourceService);
				}
				else
				{
					// Nothing to do
				}
			}
		}

		protected void InitializeCachedServices()
		{
			InitializeCachedMainServices(); // cache main services

			if (order.BackupSourceService is null) return;

			InitializeCachedReceptionService(order.BackupSourceService);

			foreach (var service in OrderManager.FlattenServices(order.BackupSourceService.Children))
			{
				switch (service.Definition.VirtualPlatformServiceType)
				{
					case VirtualPlatformType.Recording:
					case VirtualPlatformType.Destination:
					case VirtualPlatformType.Transmission:
						cachedBackupSourceChildServices[service.Definition.VirtualPlatformServiceType].Add(new List<DisplayedService> { service as DisplayedService });
						break;

					default:
						// nothing
						break;
				}
			}
		}

		protected override void SubscribeToUi()
		{
			base.SubscribeToUi();

			if (orderSection == null) return;

			orderSection.UseSharedSourceChanged += (s, e) => SharedSourceChanged();
			orderSection.IncludeExternalSourcesChanged += (s, e) => SharedSourceChanged();
			orderSection.SharedSourceChanged += (s, e) => SharedSourceChanged();

			orderSection.AddDestination += (s, e) => Section_AddChildService(VirtualPlatformType.Destination);
			orderSection.AddRecording += (s, e) => Section_AddChildService(VirtualPlatformType.Recording);
			orderSection.AddTransmission += (s, e) => Section_AddChildService(VirtualPlatformType.Transmission);

			orderSection.BackupSourceChanged += Section_BackupSourceChanged;
			orderSection.BackupSourceDescriptionChanged += Section_BackupSourceDescriptionChanged;
			orderSection.BackupSourceServiceLevelChanged += Section_BackupSourceServiceLevelChanged;
			orderSection.BackupSourceServiceSectionAdded += (s, addedBackupSourceServiceSection) =>
			{
				RegisterServiceController(addedBackupSourceServiceSection.Service);
				AddServiceSectionToServiceController(addedBackupSourceServiceSection);
			};

			orderSection.AdditionalInformationSection.DisplayedPropertyChanged += (s, e) => order.SetPropertyValue(helpers, e.PropertyName, e.PropertyValue);

			orderSection.SportsPlanningSection.DisplayedPropertyChanged += (s, e) => order.SportsPlanning.SetPropertyValue(helpers, e.PropertyName, e.PropertyValue);
			orderSection.SportsPlanningSection.CompetitionTimeChanged += (s, e) => order.SportsPlanning.CompetitionTime = Math.Round((e - new DateTime(1970, 1, 1)).TotalMilliseconds);
			orderSection.SportsPlanningSection.RequestedBroadcastTimeChanged += (s, e) => order.SportsPlanning.RequestedBroadcastTime = Math.Round((e - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)).TotalMilliseconds);

			orderSection.NewsInformationSection.DisplayedPropertyChanged += (s, e) => order.NewsInformation.SetPropertyValue(helpers, e.PropertyName, e.PropertyValue);
		}

		protected override void InitializeServiceControllers()
		{
			using (StartPerformanceLogging())
			{
				base.InitializeServiceControllers();

				if (order.BackupSourceService is null) return;

				RegisterServiceController(order.BackupSourceService);
				foreach (var backupDescendant in order.BackupSourceService.Descendants)
				{
					RegisterServiceController(backupDescendant);
				}
			}
		}

		protected override void InitializeDisplayedOrder()
		{
			base.InitializeDisplayedOrder();

			UpdateSharedSources(true, false);

			foreach (var service in order.AllServices)
			{
				UpdateAvailableServicesToRecordOrTransmit(service, false);
				if (service.Definition.VirtualPlatformServiceType != VirtualPlatformType.Reception)
				{
					service.AudioChannelConfiguration.InitializeUiValues(service.BackupType == BackupType.None ? order.SourceService.AudioChannelConfiguration : order.BackupSourceService?.AudioChannelConfiguration, helpers);
				}
			}
		}

		private void SharedSourceChanged()
		{
			if (!orderSection.UseSharedSource)
			{
				bool orderUsesSharedSource = order.SourceService.IsSharedSource;
				bool sourceContributingResourceIsNotAssignedToOrder = !order.SourceService.OrderReferences.Contains(order.Id);

				if (orderUsesSharedSource || sourceContributingResourceIsNotAssignedToOrder)
				{
					// When the Use Shared Source checkbox gets unselected and a shared source was selected -> set source service back to previously selected service
					UpdateOrderSourceService(orderSection.Source, orderSection.SourceDescription);
				}
				else
				{
					// When the Use Shared Source checkbox gets unselected and NO shared source was selected -> previous source is still in the order
				}

				return;
			}

			UpdateSharedSources(false, false);

			Log(nameof(SharedSourceChanged), $"Shared Source service changed to {orderSection.ExternalSource}");

			var selectedSharedSource = externalSources.FirstOrDefault(ss => ss.DropDownOption == orderSection.ExternalSource);

			if (selectedSharedSource == null)
			{
				// If no shared sources are available
				helpers.Log(nameof(OrderController), nameof(SharedSourceChanged), $"No shared source found with short description: {orderSection.ExternalSource}");
				return;
			}

			if (!cachedSharedSourceServices.TryGetValue(selectedSharedSource.Reservation.ID, out DisplayedService sharedSourceService))
			{
				sharedSourceService = selectedSharedSource.Service;

				sharedSourceService.AcceptChanges();

				// Promote to shared source
				if (selectedSharedSource is NonSharedSourceInfo) sharedSourceService.IsSharedSource = true;

				cachedSharedSourceServices.Add(selectedSharedSource.Reservation.ID, sharedSourceService);
			}

			Log(nameof(SharedSourceChanged), $"Updating Source Service to Shared Source {sharedSourceService.Name}");

			ReplaceService(order.SourceService, sharedSourceService);
			AudioChannelConfigurationChanged(sharedSourceService);

			UpdateLiveVideoOrder();
			InvokeValidationRequired();
		}

		/// <summary>
		/// Called when the audio channel configuration that can apply to other services is updated.
		/// Whenever Copy From Source or a value of an Audio Channel Pair is updated.
		/// </summary>
		/// <param name="service">Service on which the change was performed.</param>
		private void AudioChannelConfigurationChanged(DisplayedService service)
		{
			// Update available Audio options when source service changes

			bool orderIsMainOrBackupSourceService = order.Sources.Contains(service);
			if (orderIsMainOrBackupSourceService)
			{
				var sourceOptions = service.AudioChannelConfiguration.GetSourceOptions();
				foreach (var childService in OrderManager.FlattenServices(service.Children))
				{
					childService.AudioChannelConfiguration.SetSourceOptions(sourceOptions);
				}
			}
			else
			{
				var sourceServiceToCopyFrom = service.BackupType == BackupType.None ? order.SourceService : order.BackupSourceService;

				if (service.AudioChannelConfiguration.IsCopyFromSource)
				{
					service.AudioChannelConfiguration.CopyFromSource(sourceServiceToCopyFrom.AudioChannelConfiguration);
					service.AudioChannelConfiguration.SetSourceOptions(sourceServiceToCopyFrom.AudioChannelConfiguration.GetSourceOptions());
				}
				else if (!service.AudioChannelConfiguration.MatchesSourceConfiguration(sourceServiceToCopyFrom.AudioChannelConfiguration) && service.AudioChannelConfiguration.AudioShufflingRequiredProfileParameter != null)
				{
					service.AudioChannelConfiguration.AudioShufflingRequiredProfileParameter.Value = "Yes";
				}
				else
				{
					//Nothing
				}
			}

			// Get affected services and copy Audio Channel configuration
			var linkedServices = GetLinkedAudioProcessingServices(service);

			foreach (var linkedAudioConfiguration in linkedServices.Select(x => x.AudioChannelConfiguration))
			{
				linkedAudioConfiguration.CopyFromSource(service.AudioChannelConfiguration);
				linkedAudioConfiguration.UpdateSelectableOptions();
			}

			OrderSection.RegenerateUi();
		}

		private IEnumerable<DisplayedService> GetLinkedAudioProcessingServices(DisplayedService sourceService)
		{
			List<DisplayedService> linkedProcessingServices = new List<DisplayedService>();
			foreach (var service in order.AllServices.Cast<DisplayedService>())
			{
				if (sourceService.Equals(service) || service.BackupType != sourceService.BackupType) continue;

				switch (service.Definition.VirtualPlatformServiceType)
				{
					case VirtualPlatformType.Destination:
						if (sourceService.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception && service.AudioChannelConfiguration.IsCopyFromSource)
						{
							linkedProcessingServices.Add(service);
							linkedProcessingServices.AddRange(GetLinkedAudioProcessingServices(service));
						}
						break;
					case VirtualPlatformType.Recording:
					case VirtualPlatformType.Transmission:
						if (String.Equals(sourceService.Name, service.NameOfServiceToTransmitOrRecord) && service.AudioChannelConfiguration.IsCopyFromSource) linkedProcessingServices.Add(service);
						break;
					default:
						// Don't update service
						break;
				}
			}

			return linkedProcessingServices;
		}


		private void Section_BackupSourceChanged(object sender, string e)
		{
			var backupSourceServiceDefinitions = allowedServiceDefinitions[VirtualPlatformType.Reception].Where(x => x.VirtualPlatformServiceName.GetDescription() == e).ToList();
			order.AvailableBackupSourceServiceDescriptions = backupSourceServiceDefinitions.Select(s => s.Description).OrderBy(s => s).ToList();

			var defaultServiceDefinition = backupSourceServiceDefinitions.FirstOrDefault(x => x.IsDefault) ?? backupSourceServiceDefinitions[0];

			UpdateOrderBackupSourceService(orderSection.BackupSource, defaultServiceDefinition.Description);
		}

		private void Section_BackupSourceDescriptionChanged(object sender, string e)
		{
			helpers.Log(nameof(OrderController), nameof(Section_BackupSourceDescriptionChanged), $"Backup Source Description Dropdown changed: {e}");

			UpdateOrderBackupSourceService(orderSection.BackupSource, e);
		}

		private void Section_BackupSourceServiceLevelChanged(object sender, BackupType backupType)
		{
			helpers.Log(nameof(OrderController), nameof(Section_BackupSourceServiceLevelChanged), $"Backup Source Service Level Dropdown changed: {backupType.GetDescription()}");

			if (backupType == order.BackupSourceService.BackupType) return;

			BackupType previousBackupType = order.BackupSourceService.BackupType;
			order.BackupSourceService.BackupType = backupType;

			bool activeToColdOrStandby = previousBackupType == BackupType.Active;
			bool coldOrStandbyToActive = backupType == BackupType.Active;

			if (activeToColdOrStandby)
			{
				// Remove all backup children
				foreach (var backupChildService in order.BackupSourceService.GetNonAutoGeneratedDescendants()) RemoveBackupChildService(backupChildService);
				order.BackupSourceService.Children.Clear();
			}
			else if (coldOrStandbyToActive)
			{
				foreach (var child in order.SourceService.GetNonAutoGeneratedDescendants())
				{
					Section_AddBackupChildService(child);
				}
			}
			else
			{
				// Cold <-> Standby
				// Nothing to do
			}

			foreach (var service in order.AllServices) UpdateAvailableServicesToRecordOrTransmit(service, false);

			UpdateLiveVideoOrder();
		}

		protected override void ReplaceService(Service existingService, DisplayedService newService)
		{
			base.ReplaceService(existingService, newService);

			UpdateServicesToRecordOrTransmit(existingService, newService);
			AudioChannelConfigurationChanged(newService);

			InvokeValidationRequired();
		}

		/// <summary>
		/// Updates the service to Record Or Transmit property on recording and transmission services in the order when a service gets switched.
		/// if newly displayed service is null, the property will be set to the source service.
		/// </summary>
		/// <param name="previousService">Previously displayed service.</param>
		/// <param name="newService">Newly displayed service, can be null.</param>
		private void UpdateServicesToRecordOrTransmit(Service previousService, Service newService)
		{
			if (previousService == null) return; // Can be null for example when switching Backup Source type from None

			Log(nameof(UpdateServicesToRecordOrTransmit), $"Previous Service {previousService.Name}, replaced with {newService?.Name}");

			foreach (var service in order.AllServices)
			{
				if (service.Definition.VirtualPlatformServiceType != VirtualPlatformType.Recording && service.Definition.VirtualPlatformServiceType != VirtualPlatformType.Transmission) continue;

				if (service.NameOfServiceToTransmitOrRecord != previousService.Name) continue;

				if (newService != null)
				{
					service.NameOfServiceToTransmitOrRecord = newService.Name;
				}
				else if (service.BackupType == BackupType.None)
				{
					service.NameOfServiceToTransmitOrRecord = order.SourceService.Name;
				}
				else
				{
					service.NameOfServiceToTransmitOrRecord = order.BackupSourceService?.Name;
				}
			}
		}

		public override void AddChildService(DisplayedService serviceToAdd)
		{
			base.AddChildService(serviceToAdd);

			foreach (var otherService in order.AllServices) UpdateAvailableServicesToRecordOrTransmit(otherService, false);

			if (order.BackupSourceService?.BackupType == BackupType.Active)
			{
				Section_AddBackupChildService(serviceToAdd);
			}

			UpdateLiveVideoOrder();
		}

		protected override void InitializeServiceBeforeBeingAdded(DisplayedService serviceToAdd, Service replacedService = null)
		{
			base.InitializeServiceBeforeBeingAdded(serviceToAdd, replacedService);

			CopyProcessingSettings(serviceToAdd, order.SourceService);

			UpdateAvailableServicesToRecordOrTransmit(serviceToAdd, true);

			if (serviceToAdd.Definition.VirtualPlatform == VirtualPlatform.Recording)
			{
				serviceToAdd.RecordingConfiguration.IsConfigured = true;
				serviceToAdd.RecordingConfiguration.RecordingName = order.Name;
				serviceToAdd.RecordingConfiguration.SelectableRecordingFileDestinations = serviceToAdd.Definition.Description.Contains("Live") ? new List<string> { FileDestination.ArchiveMetro.GetDescription() } : new List<string> { FileDestination.UaIplay.GetDescription() };
				serviceToAdd.RecordingConfiguration.RecordingFileDestination = serviceToAdd.Definition.Description.Contains("Live") ? FileDestination.ArchiveMetro : FileDestination.UaIplay;

				serviceToAdd.RecordingConfiguration.SelectableEvsMessiNewsTargets = helpers.OrderManagerElement.GetEvsMessiNewsTargets(serviceToAdd.Definition);
				serviceToAdd.RecordingConfiguration.EvsMessiNewsTarget = serviceToAdd.RecordingConfiguration.SelectedEvsMessiNewsTarget?.Target;
			}

			serviceToAdd.AcceptChanges();
		}

		protected override void DeleteEndpointService(Service sectionService)
		{
			base.DeleteEndpointService(sectionService);

			if (order.BackupSourceService?.BackupType == BackupType.Active)
			{
				RemoveBackupChildServiceByLinkedService(sectionService);
			}

			UpdateServicesToRecordOrTransmit(sectionService, null);

			foreach (var service in order.AllServices) UpdateAvailableServicesToRecordOrTransmit(service, false);

			UpdateLiveVideoOrder();
		}

		private void UpdateAvailableServicesToRecordOrTransmit(Service service, bool initServiceToRecordOrTransmit)
		{
			if (service.Definition.VirtualPlatformServiceType != VirtualPlatformType.Recording && service.Definition.VirtualPlatformServiceType != VirtualPlatformType.Transmission) return;

			var availableServicesToRecordOrTransmit = order.AllServices.Where(x => x.BackupType == service.BackupType && (x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception || x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Destination)).ToList();

			service.AvailableServicesToRecordOrTransmit = availableServicesToRecordOrTransmit;

			if (!initServiceToRecordOrTransmit) return;

			service.NameOfServiceToTransmitOrRecord = (service.BackupType == BackupType.None) ? order.SourceService.Name : order.BackupSourceService?.Name;
		}

		private void UpdateOrderBackupSourceService(string backupSource, string backupSourceDescription)
		{
			helpers.Log(nameof(OrderController), nameof(UpdateOrderBackupSourceService), $"Updating Source Service to '{backupSource}' - '{backupSourceDescription}'");

			var serviceDefinition = allowedServiceDefinitions[VirtualPlatformType.Reception].First(x => x.VirtualPlatformServiceName.GetDescription() == backupSource && x.Description == backupSourceDescription);

			if (serviceDefinition.VirtualPlatform == VirtualPlatform.ReceptionNone)
			{
				// Remove Backup Service from order
				UpdateServicesToRecordOrTransmit(order.BackupSourceService, null);
				order.BackupSourceService = null;
			}
			else
			{
				if (!cachedBackupReceptionServices.TryGetValue(serviceDefinition.Name, out var service))
				{
					service = new DisplayedService(helpers, serviceDefinition)
					{
						Start = order.Start,
						End = order.End,
					};

					if (order.IntegrationType != IntegrationType.None)
					{
						service.PreRoll = ServiceManager.GetPreRollDuration(serviceDefinition, order.IntegrationType);
						service.PostRoll = ServiceManager.GetPostRollDuration(serviceDefinition, order.IntegrationType);
					}

					service.AcceptChanges();
					cachedBackupReceptionServices.Add(serviceDefinition.Name, service);
				}

				service.BackupType = orderSection.BackupSourceServiceLevel;

				helpers.Log(nameof(OrderController), nameof(UpdateOrderBackupSourceService), $"Updating Backup Source Service to {service.Name}, {service.Definition.VirtualPlatform} ({service.Definition.Description}, backupType {service.BackupType.GetDescription()})");

				UpdateServicesToRecordOrTransmit(order.BackupSourceService, service);
				order.BackupSourceService = service;
			}

			UpdateLiveVideoOrder();

			InvokeValidationRequired();
		}

		private void Section_AddBackupChildService(Service linkedService)
		{
			var backupChildService = new DisplayedService(helpers, linkedService.Definition)
			{
				Start = linkedService.Start,
				End = linkedService.End,
				BackupType = BackupType.Active,
				LinkedService = linkedService,
			};

			if (backupChildService.Definition.VirtualPlatform == VirtualPlatform.Recording)
			{
				backupChildService.RecordingConfiguration.IsConfigured = true;
				backupChildService.RecordingConfiguration.RecordingName = order.Name;
				backupChildService.RecordingConfiguration.SelectableRecordingFileDestinations = backupChildService.Definition.Description.Contains("Live") ? new List<string> { FileDestination.ArchiveMetro.GetDescription() } : new List<string> { FileDestination.UaIplay.GetDescription() };
				backupChildService.RecordingConfiguration.RecordingFileDestination = backupChildService.Definition.Description.Contains("Live") ? FileDestination.ArchiveMetro : FileDestination.UaIplay;

				backupChildService.RecordingConfiguration.SelectableEvsMessiNewsTargets = helpers.OrderManagerElement.GetEvsMessiNewsTargets(backupChildService.Definition);
				backupChildService.RecordingConfiguration.EvsMessiNewsTarget = backupChildService.RecordingConfiguration.SelectedEvsMessiNewsTarget?.Target;
			}

			for (int i = 1; i <= order.GetAllBackupServices().Count(s => s.Definition.VirtualPlatformServiceType == backupChildService.Definition.VirtualPlatformServiceType) + 1; i++)
			{
				string potentialDisplayName = $"Backup {backupChildService.Definition.VirtualPlatformServiceType.GetDescription()} {i}";

				if (!order.GetAllBackupServices().Exists(s => s.LofDisplayName == potentialDisplayName))
				{
					backupChildService.LofDisplayName = potentialDisplayName;
				}
			}

			if (order.IntegrationType != IntegrationType.None)
			{
				backupChildService.PreRoll = ServiceManager.GetPostRollDuration(backupChildService.Definition, order.IntegrationType);
				backupChildService.PostRoll = ServiceManager.GetPostRollDuration(backupChildService.Definition, order.IntegrationType);
			}

			CopyProcessingSettings(backupChildService, order.BackupSourceService);

			backupChildService.SetAvailableVirtualPlatformNames(allowedServiceDefinitions[backupChildService.Definition.VirtualPlatformServiceType].Select(x => x.VirtualPlatformServiceName.GetDescription()));
			backupChildService.SetAvailableServiceDescriptions(allowedServiceDefinitions[backupChildService.Definition.VirtualPlatformServiceType].Where(x => x.VirtualPlatform == backupChildService.Definition.VirtualPlatform).Select(x => x.Description));

			UpdateAvailableServicesToRecordOrTransmit(backupChildService, true);

			Log(nameof(Section_AddBackupChildService), $"Name of Service To Record Or Transmit: {backupChildService.NameOfServiceToTransmitOrRecord}");

			backupChildService.AcceptChanges();

			cachedBackupSourceChildServices[backupChildService.Definition.VirtualPlatformServiceType].Add(new List<DisplayedService> { backupChildService });

			order.BackupSourceService.Children.Add(backupChildService);

			InvokeValidationRequired();
		}

		private void CopyProcessingSettings(Service serviceToUpdate, Service sourceService)
		{
			CopyVideoFormat(serviceToUpdate, sourceService);

			serviceToUpdate.AudioChannelConfiguration.CopyFromSource(sourceService.AudioChannelConfiguration);
			Log(nameof(CopyProcessingSettings), $"Audio Config of {serviceToUpdate.Name} after copy, {serviceToUpdate.AudioChannelConfiguration}");

			serviceToUpdate.AudioChannelConfiguration.SetSourceOptions(sourceService.AudioChannelConfiguration.GetSourceOptions());
			Log(nameof(CopyProcessingSettings), $"Audio Config of {serviceToUpdate.Name} after updating options, {serviceToUpdate.AudioChannelConfiguration}");

			if (serviceToUpdate.Definition.Description.Equals("messi news", StringComparison.OrdinalIgnoreCase) && serviceToUpdate.IntegrationType == IntegrationType.None)
			{
				// Recording Messi News connected to a IP RX Vidigo source does not require routing

				bool sourceIsIpVidigoReception = sourceService.Definition.VirtualPlatform == VirtualPlatform.ReceptionIp && sourceService.Definition.Description == "Vidigo";

				serviceToUpdate.RequiresRouting = !sourceIsIpVidigoReception;

				Log(nameof(CopyProcessingSettings), $"Service {serviceToUpdate.Name} is a manual Messi News recording service, RequiresRouting property set to {serviceToUpdate.RequiresRouting}, based on if the source is IP Vidigo");
			}
		}

		private void CopyVideoFormat(Service serviceToUpdate, Service sourceService)
		{
			var sourceVideoFormatProfileParameter = sourceService.Functions.SelectMany(f => f.Parameters).FirstOrDefault(pp => pp.Id == ProfileParameterGuids.VideoFormat);
			var childVideoFormatProfileParameter = serviceToUpdate.Functions.SelectMany(f => f.Parameters).FirstOrDefault(pp => pp.Id == ProfileParameterGuids.VideoFormat);

			if (sourceVideoFormatProfileParameter is null)
			{
				Log(nameof(CopyVideoFormat), $"WARNING: Could not find Video Format profile parameter in source service {sourceService.Name} to copy its value to service {serviceToUpdate.Name}");
			}
			else if (childVideoFormatProfileParameter is null)
			{
				Log(nameof(CopyVideoFormat), $"WARNING: Could not find Video Format profile parameter in service {serviceToUpdate.Name} to copy its value from source service {sourceService.Name}");
			}
			else
			{
				childVideoFormatProfileParameter.Value = sourceVideoFormatProfileParameter.Value;
				Log(nameof(CopyVideoFormat), $"Copied source service {sourceService.Name} profile parameter {sourceVideoFormatProfileParameter.Name} value {sourceVideoFormatProfileParameter.StringValue} to service {serviceToUpdate.Name}");
			}
		}

		private void RemoveBackupChildServiceByLinkedService(Service linkedService)
		{
			var backupService = order.AllServices.FirstOrDefault(x => x.LinkedService != null && x.LinkedService.Equals(linkedService));
			if (backupService == null) return;

			RemoveBackupChildService(backupService);
			UpdateServicesToRecordOrTransmit(backupService, null);
		}

		private void RemoveBackupChildService(Service backupService)
		{
			var cachedAlternativeServices = cachedBackupSourceChildServices[backupService.Definition.VirtualPlatformServiceType].First(x => x.Contains(backupService));
			Log(nameof(RemoveBackupChildService), $"Removing cached backup source services: {String.Join(", ", cachedAlternativeServices.Select(x => (x.Name + " " + x.Definition.Name)))}");

			cachedBackupSourceChildServices[backupService.Definition.VirtualPlatformServiceType].Remove(cachedAlternativeServices);

			var parentService = order.AllServices.FirstOrDefault(x => x.Children.Contains(backupService)) ?? throw new ServiceNotFoundException($"Unable to find parent of {backupService.Name}", true);
			parentService.Children.Remove(backupService);

			foreach (var serviceToRemove in cachedAlternativeServices)
			{
				serviceControllers.Remove(serviceToRemove);
			}
		}

		protected override void RegisterServiceController(Service service)
		{
			var displayedService = service as DisplayedService;

			base.RegisterServiceController(displayedService);

			serviceControllers[displayedService].BookEurovisionService += (sender, args) => OnBookEurovisionService(displayedService);
			serviceControllers[displayedService].AudioChannelConfigurationChanged += (sender, args) => AudioChannelConfigurationChanged(displayedService);
			serviceControllers[displayedService].ServiceToRecordOrTransmitChanged += (sender, args) => ServiceToRecordOrTransmitChanged(displayedService, args);
			serviceControllers[displayedService].UploadJsonButtonPressed += (sender, args) => OnUploadJsonButtonPressed();
			serviceControllers[displayedService].UploadSynopsisButtonPressed += (sender, args) => OnUploadSynopsisButtonPressed(displayedService);
			serviceControllers[displayedService].OrderValidationRequired += (sender, args) => InvokeValidationRequired();
		}

		protected override void OrderStartTimeChanged(DateTime orderStartTime)
		{
			using (StartPerformanceLogging())
			{
				base.OrderStartTimeChanged(orderStartTime);

				UpdateSharedSources(false, true);
			}
		}

		protected override void OrderEndTimeChanged(DateTime orderEndTime)
		{
			using (StartPerformanceLogging())
			{
				base.OrderEndTimeChanged(orderEndTime);

				UpdateSharedSources(false, true);
			}
		}

		private void ServiceToRecordOrTransmitChanged(DisplayedService service, string nameOfServiceToRecordOrTransmit)
		{
			if (order.SourceService.Name.Equals(nameOfServiceToRecordOrTransmit))
			{
				helpers.Log(nameof(OrderController), nameof(ServiceToRecordOrTransmitChanged), $"{service.Name} records or transmits the source service");
				if (service.AudioChannelConfiguration.IsCopyFromSource) service.AudioChannelConfiguration.CopyFromSource(order.SourceService.AudioChannelConfiguration);
			}
			else if (order.BackupSourceService != null && order.BackupSourceService.Name.Equals(nameOfServiceToRecordOrTransmit))
			{
				helpers.Log(nameof(OrderController), nameof(ServiceToRecordOrTransmitChanged), $"{service.Name} records or transmits the backup source service");
				if (service.AudioChannelConfiguration.IsCopyFromSource) service.AudioChannelConfiguration.CopyFromSource(order.BackupSourceService.AudioChannelConfiguration);
			}
			else
			{
				helpers.Log(nameof(OrderController), nameof(ServiceToRecordOrTransmitChanged), $"{service.Name} records or transmits another service");

				// Copy audio config and video config and disable them
				Service serviceToRecordOrTransmit = order.AllServices.First(x => x.Name.Equals(nameOfServiceToRecordOrTransmit));
				service.AudioChannelConfiguration.CopyFromSource(serviceToRecordOrTransmit.AudioChannelConfiguration);
				service.AudioChannelConfiguration.SetSourceOptions(serviceToRecordOrTransmit.AudioChannelConfiguration.GetSourceOptions());
			}
		}

		public override void HandleServiceTimeUpdate()
		{
			base.HandleServiceTimeUpdate();

			UpdateSharedSources(false, true);
		}

		private void UpdateSharedSources(bool initialize, bool fromTimingChange)
		{
			using (StartPerformanceLogging())
			{
				IReadOnlyList<ExternalSourceInfo> availableExternalSources;
				IReadOnlyList<ExternalSourceInfo> unavailableExternalSources;

				UpdateNonSharedSources();

				availableExternalSources = externalSources.Where(s => s.SharedSourceStartWithPreRoll <= order.Start && order.End <= s.SharedSourceEndWithPostRoll).ToList();
				unavailableExternalSources = externalSources.Except(availableExternalSources).ToList();

				bool availableSharedSourcesChanged = !order.AvailableSharedSources.ToHashSet().SetEquals(availableExternalSources);
				bool unavailableSharedSourcesChanged = !order.UnavailableSharedSources.ToHashSet().SetEquals(unavailableExternalSources);
				if (!initialize && !availableSharedSourcesChanged && !unavailableSharedSourcesChanged)
				{
					Log(nameof(UpdateSharedSources), $"Available and unavailable shared sources did not change.");
					return;
				}

				// Check if current order source is shared source and now unavailable
				if (fromTimingChange && order.SourceService.IsSharedSource && orderSection.UseSharedSource && !availableExternalSources.Any(x => x.Reservation.ID == order.SourceService.Id))
				{
					Log(nameof(UpdateSharedSources), $"Order shared source service: {order.SourceService.Name} became unavailable due to timing change");
					OnSharedSourceUnavailableDueToOrderTimingChange();
				}

				order.SetAvailableSharedSources(availableExternalSources);
				order.SetUnavailableSharedSources(unavailableExternalSources);

				Log(nameof(UpdateSharedSources), $"Available shared sources: '{string.Join(";", order.AvailableSharedSources.Select(x => $"{x.Reservation.Name} (option '{x.DropDownOption}')"))}'");
				Log(nameof(UpdateSharedSources), $"Unavailable shared sources: '{string.Join(";", order.UnavailableSharedSources.Select(x => $"{x.Reservation.Name} (option '{x.DropDownOption}')"))}'");

				var previousSelectedSharedSourceShortDescription = orderSection?.Source;

				if (initialize || orderSection is null) return;

				Log(nameof(UpdateSharedSources), $"Use Shared Source is{(orderSection.UseSharedSource ? string.Empty : " not")} checked. Previous selected shared source is '{previousSelectedSharedSourceShortDescription}'. Current selected shared source is '{orderSection.ExternalSource}'.");

				if (availableExternalSources.Any() && orderSection.UseSharedSource)
				{
					// Other shared sources available, update the order source service to the one that is auto-selected by the dropdown.
					SharedSourceChanged();
				}
			}
		}

		protected override List<DisplayedService> GetCachedAlternativeServices(Service previousDisplayedService)
		{
			List<DisplayedService> cachedAlternativeServices;
			if (previousDisplayedService.BackupType == BackupType.None)
			{
				cachedAlternativeServices = cachedSourceChildServices[previousDisplayedService.Definition.VirtualPlatformServiceType].First(x => x.Contains(previousDisplayedService));
			}
			else
			{
				cachedAlternativeServices = cachedBackupSourceChildServices[previousDisplayedService.Definition.VirtualPlatformServiceType].First(x => x.Contains(previousDisplayedService));
			}

			return cachedAlternativeServices;
		}
	}
}
