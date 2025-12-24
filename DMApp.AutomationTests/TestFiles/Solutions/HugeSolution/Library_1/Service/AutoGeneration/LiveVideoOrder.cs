namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class LiveVideoOrder : HelpedObject
	{
		private readonly List<LinkedList<LiveVideoService>> chains = new List<LinkedList<LiveVideoService>>();

		public LiveVideoOrder(Helpers helpers, Order order) : base(helpers)
		{
			Order = order;

			LogMethodStart("Constructor", out var stopwatch);

			InitializeChains();
			InitializeParents();
			InitializeRoutingServiceChains();
			InitializeLinkedServices();

			Log("Constructor", ToString());

			LogMethodCompleted("Constructor", stopwatch);
		}

		public Order Order { get; }

		public List<LiveVideoService> LiveVideoServices { get; private set; } = new List<LiveVideoService>();

		public IReadOnlyCollection<RoutingService> GetRoutingServices()
		{
			return LiveVideoServices.OfType<RoutingService>().ToList();
		}

		public IReadOnlyCollection<ProcessingRelatedService> GetProcessingRelatedServices()
		{
			return LiveVideoServices.OfType<ProcessingRelatedService>().ToList();
		}

		public IReadOnlyCollection<SourceService> GetSourceServices()
		{
			return LiveVideoServices.OfType<SourceService>().ToList();
		}

		public IReadOnlyCollection<RoutingRequiringService> GetRoutingRequiringServices()
		{
			return LiveVideoServices.OfType<RoutingRequiringService>().ToList();
		}

		public IReadOnlyCollection<AudioProcessingService> GetAudioProcessingServices()
		{
			return LiveVideoServices.OfType<AudioProcessingService>().ToList();
		}

		public IReadOnlyCollection<GraphicsProcessingService> GetGraphicsProcessingServices()
		{
			return LiveVideoServices.OfType<GraphicsProcessingService>().ToList();
		}

		public IReadOnlyCollection<VideoProcessingService> GetVideoProcessingServices()
		{
			return LiveVideoServices.OfType<VideoProcessingService>().ToList();
		}

		public IReadOnlyCollection<EndPointService> GetEndPointServices()
		{
			return LiveVideoServices.OfType<EndPointService>().ToList();
		}

		public IReadOnlyCollection<DestinationService> GetDestinations()
		{
			return LiveVideoServices.OfType<DestinationService>().ToList();
		}

		public IReadOnlyCollection<TransmissionService> GetTransmissions()
		{
			return LiveVideoServices.OfType<TransmissionService>().ToList();
		}

		public IReadOnlyCollection<RecordingService> GetRecordings()
		{
			return LiveVideoServices.OfType<RecordingService>().ToList();
		}

		public IReadOnlyCollection<IVideoProcessingRequiringService> GetVideoProcessingRequiringServices()
		{
			return LiveVideoServices.OfType<IVideoProcessingRequiringService>().ToList();
		}

		public IReadOnlyCollection<VizremStudioService> GetVizremStudioServices()
		{
			return LiveVideoServices.OfType<VizremStudioService>().ToList();
		}

		public IReadOnlyCollection<VizremFarmService> GetVizremFarmServices()
		{
			return LiveVideoServices.OfType<VizremFarmService>().ToList();
		}

		public IReadOnlyCollection<VizremConverterService> GetVizremConverterServices()
		{
			return LiveVideoServices.OfType<VizremConverterService>().ToList();
		}

		public void Update()
		{
			LogMethodStart(nameof(Update), out var stopwatch);

			LiveVideoServices.Clear();

			InitializeChains();
			InitializeParents();
			InitializeRoutingServiceChains();
			InitializeLinkedServices();

			Log(nameof(Update), ToString());

			LogMethodCompleted(nameof(Update));
		}

		public LiveVideoService GetLiveVideoService(Service service)
		{
			return LiveVideoServices.SingleOrDefault(s => s.Service.Equals(service));
		}

		/// <summary>
		/// Checks if any generated services need to be added, updated or removed in this source chain. 
		/// </summary>
		public void AddOrUpdateProcessingServices()
		{
			LogMethodStart(nameof(AddOrUpdateProcessingServices), out var stopwatch);

			UnshareServices();

			AddOrUpdateGraphicsProcessingServices();

			AddOrUpdateVideoProcessingServices();

			AddOrUpdateAudioProcessingServices();

			ShareProcessingServices();

			UpdateParentsOfRecordingsAndTransmissionsOfDestinations();

			RemoveUnusedNonEndPointServices();

			Log(nameof(AddOrUpdateProcessingServices), $"Result: {ToString()}");

			LogMethodCompleted(nameof(AddOrUpdateProcessingServices), stopwatch);
		}

		public void AddorUpdateVizremConverterServices()
		{
			LogMethodStart(nameof(AddorUpdateVizremConverterServices), out var stopwatch);

			var liveVideoServices = GetVizremConverterEvaluationRequiringServices();

			foreach (var liveVideoService in liveVideoServices)
			{
				liveVideoService.AddOrUpdateVizremConverterConfiguration();
			}

			ReuseUnusedExistingVizremConverterServices(liveVideoServices);

			RemoveUnusedNonEndPointServices();

			Log(nameof(AddorUpdateVizremConverterServices), $"Result: {ToString()}");

			LogMethodCompleted(nameof(AddorUpdateVizremConverterServices), stopwatch);
		}

		private List<VizremConverterRequiringService> GetVizremConverterEvaluationRequiringServices()
		{
			var liveVideoServices = new List<VizremConverterRequiringService>();

			var studioAsSource = GetVizremStudioServices().SingleOrDefault(studio => studio.Children.Any());
			if (studioAsSource != null) liveVideoServices.Add(studioAsSource);

			liveVideoServices.AddRange(GetVizremFarmServices());

			return liveVideoServices;
		}

		private void RemoveUnusedProcessingServices()
		{
			var processingServices = GetAudioProcessingServices().Concat<ProcessingRelatedService>(GetGraphicsProcessingServices()).Concat<ProcessingRelatedService>(GetVideoProcessingServices()).ToList();

			foreach (var processingService in processingServices)
			{
				Log(nameof(RemoveUnusedProcessingServices), $"{processingService.Service.Name} has children '{string.Join(", ", processingService.Children.Select(s => s.Service.Name))}'");
			}

			var unusedProcessingServices = processingServices.Where(a => !a.Children.Any()).ToList();
			foreach (var processingService in unusedProcessingServices)
			{
				LiveVideoServices.Remove(processingService);

				if (processingService.RoutingParent != null && processingService.RoutingParent.Parent != null)
				{
					processingService.RoutingParent.Parent.RemoveChild(processingService.RoutingParent);
				}
				else if (processingService.Parent != null)
				{
					processingService.Parent.RemoveChild(processingService);
				}
				else
				{
					//Nothing
				}
			}
		}

		private void UnshareServices()
		{
			// Connect all branches (except one) starting from a shared service to the source so every branch can be independently handled
			// New services should be as much as possible in a separate branch
			// e.g.:	RX - R - D			becomes	RX - R - D
			//				   - R - Rec			   - R - Rec

			int infiniteLoopProtectionCounter = 0;

			var sharedServices = DetermineServicesToUnshare();

			bool thereAreSharedServices = sharedServices.Any();

			while (thereAreSharedServices && infiniteLoopProtectionCounter <= 50)
			{
				foreach (var service in sharedServices)
				{
					StopSharingService(service);
				}

				sharedServices = LiveVideoServices.Except(GetSourceServices()).Except(GetEndPointServices()).Where(s => s.Children.Count > 1).ToList();
				Log(nameof(UnshareServices), $"Shared services: '{string.Join(", ", sharedServices.Select(s => s.Service.Name))}'");
				thereAreSharedServices = sharedServices.Any();

				infiniteLoopProtectionCounter++;
			}

			if (infiniteLoopProtectionCounter > 50) throw new InvalidOperationException("While loop has been ended to avoid going infinite");

			Log(nameof(UnshareServices), $"Result: {ToString()}");
		}

		private List<LiveVideoService> DetermineServicesToUnshare()
		{
			var sharedServices = LiveVideoServices.Except(GetSourceServices()).Except(GetEndPointServices()).Except(GetVizremStudioServices()).Where(s => s.Children.Count > 1).ToList();
			Log(nameof(DetermineServicesToUnshare), $"Shared services: '{string.Join(", ", sharedServices.Select(s => s.Service.Name))}'");

			var sharedRoutingServicesBeforeAnyProcessing = sharedServices.Where(s => s.Service.Definition.VirtualPlatform == VirtualPlatform.Routing).ToList();

			var sharedRoutingsBeforeGraphicsProcessing = sharedRoutingServicesBeforeAnyProcessing.Where(sharedRouting => GetGraphicsProcessingServices().Any(graphics => graphics.GetOldestRoutingParentServiceOrThis().Equals(sharedRouting)));
			var sharedRoutingsToExclude = sharedRoutingsBeforeGraphicsProcessing.ToHashSet();

			var sharedRoutingsBeforeVideoProcessing = sharedRoutingServicesBeforeAnyProcessing.Where(sharedRouting => GetVideoProcessingServices().Any(video => video.GetOldestRoutingParentServiceOrThis().Equals(sharedRouting)));
			sharedRoutingsToExclude.UnionWith(sharedRoutingsBeforeVideoProcessing);

			var sharedRoutingsBeforeAudioProcessing = sharedRoutingServicesBeforeAnyProcessing.Where(sharedRouting => GetAudioProcessingServices().Any(audio => audio.GetOldestRoutingParentServiceOrThis().Equals(sharedRouting)));
			sharedRoutingsToExclude.UnionWith(sharedRoutingsBeforeAudioProcessing);

			Log(nameof(DetermineServicesToUnshare), $"Shared routings to exclude: '{string.Join(", ", sharedRoutingsToExclude.Select(s => s.Service.Name))}'");

			return sharedServices.Except(sharedRoutingsToExclude).ToList();
		}

		private void ShareProcessingServices()
		{
			var setParentActions = new List<Tuple<LiveVideoService, LiveVideoService>>();

			var endPointServicesToHandle = GetEndPointServices().Where(eps => !(eps.Parent is EndPointService)).ToList();// avoid handling the plasma recordings coming after a destination

			foreach (var endPointService in endPointServicesToHandle)
			{
				Log(nameof(ShareProcessingServices), $"Handling endpointservice {endPointService.Service.Name}");

				var currentChain = chains.Single(c => c.Contains(endPointService));
				var bestShareableServices = DetermineBestShareableServices(currentChain, endPointService);

				Log(nameof(ShareProcessingServices), $"Best shareable services are '{string.Join(", ", bestShareableServices.Select(s => s.Service.Name))}'");
				if (!bestShareableServices.Any()) continue;

				var bestShareableServiceOfCurrentChain = bestShareableServices.Single(s => currentChain.Contains(s));

				var firstServiceOfSubChain = bestShareableServiceOfCurrentChain.Children.Single(s => currentChain.Contains(s));

				var alreadySharedServices = setParentActions.Select(tuple => tuple.Item2).Distinct().ToList();

				Log(nameof(ShareProcessingServices), $"Already shared services are '{string.Join(", ", alreadySharedServices.Select(s => s.Service.Name))}'");

				var serviceToShare = bestShareableServices.Intersect(alreadySharedServices).FirstOrDefault() ?? bestShareableServices.FirstOrDefault(s => !s.IsNew) ?? bestShareableServices[0];

				Log(nameof(ShareProcessingServices), $"Service to share is {serviceToShare.Service.Name}");

				setParentActions.Add(new Tuple<LiveVideoService, LiveVideoService>(firstServiceOfSubChain, serviceToShare));
			}

			foreach (var setParentAction in setParentActions)
			{
				Log(nameof(ShareProcessingServices), $"Setting parent of {setParentAction.Item1.Service.Name} to {setParentAction.Item2.Service.Name}");

				SetParent(setParentAction.Item1, setParentAction.Item2);
			}
		}

		private List<LiveVideoService> DetermineBestShareableServices(LinkedList<LiveVideoService> currentChain, EndPointService endPointService)
		{
			int lengthOfCurrentChain = currentChain.Count(s => !(s is RoutingService)) - 1; // subtract 1 to not take source into account
			var bestShareableServices = new List<LiveVideoService>(); // candidate services from other chains to share with current chain

			Log(nameof(DetermineBestShareableServices), $"Current chain is {string.Join(" -> ", currentChain.Select(s => s.Service.Name))}");

			foreach (var chain in chains)
			{
				// At this point the chains collection contains chains
				// - for endpoint services (rec/dest/tx)
				// - for existing processing services that got removed from their endpoint service. If these can be reused (by sharing) we should do so to avoid creating new services.

				var secondEndPointService = chain.Last.Value;
				if (endPointService.Equals(secondEndPointService)) continue;

				if (!(endPointService.StartsBeforeAndEndsLaterThan(secondEndPointService) || secondEndPointService.StartsBeforeAndEndsLaterThan(endPointService)))
				{
					Log(nameof(DetermineBestShareableServices), $"{endPointService.Service.Name} and {secondEndPointService.Service.Name} have timings that won't enable sharing services in their chains.");
					continue;
				}

				Log(nameof(DetermineBestShareableServices), $"Searching in chain {string.Join(" -> ", chain.Select(s => s.Service.Name))} for shareable services");

				var shareableServices = GetYoungestShareableServices(endPointService, secondEndPointService);
				if (!shareableServices.Any()) continue;

				var shareableServiceOfCurrentChain = shareableServices.Single(s => currentChain.Contains(s));

				var potentialSubChain = currentChain.SkipWhile(s => !s.Equals(shareableServiceOfCurrentChain)).Skip(1).ToList();
				Log(nameof(DetermineBestShareableServices), $"Potential sub chain is {string.Join(" -> ", potentialSubChain.Select(s => s.Service.Name))}");

				int lengthOfPotentialSubChain = potentialSubChain.Count(s => !(s is RoutingService));
				Log(nameof(DetermineBestShareableServices), $"Length of the potential sub chain (without routing) would be {lengthOfPotentialSubChain}");

				if (lengthOfPotentialSubChain < lengthOfCurrentChain)
				{
					Log(nameof(DetermineBestShareableServices), $"Length of the potential sub chain is less than current chain length {lengthOfCurrentChain}");

					lengthOfCurrentChain = lengthOfPotentialSubChain;
					bestShareableServices = shareableServices;
				}
				else if (lengthOfPotentialSubChain == lengthOfCurrentChain)
				{
					Log(nameof(DetermineBestShareableServices), $"Length of the potential sub chain is equal to current chain length {lengthOfCurrentChain}");

					bestShareableServices.AddRange(shareableServices);
					bestShareableServices = bestShareableServices.Distinct().ToList();
				}
				else
				{
					//Nothing
				}
			}

			return bestShareableServices;
		}

		public void AddOrUpdateRoutingConfiguration(out List<Service> allRemovedServices)
		{
			LogMethodStart(nameof(AddOrUpdateRoutingConfiguration), out var stopwatch);

			var allRemovedLiveVideoServices = new List<LiveVideoService>();

			// recordings and transmissions linked to the source or a destination (non-integration recordings/transmissions) will be handled separately
			var recordingsAndTransmissionsLinkedToSpecificService = new List<RoutingRequiringService>();
			recordingsAndTransmissionsLinkedToSpecificService.AddRange(GetRecordings().Where(r => r.Service.IntegrationType == IntegrationType.None && r.Service.RequiresRouting));
			recordingsAndTransmissionsLinkedToSpecificService.AddRange(GetTransmissions().Where(r => r.Service.IntegrationType == IntegrationType.None && r.Service.RequiresRouting));

			ValidateServicesToRecordOrTransmit(recordingsAndTransmissionsLinkedToSpecificService);

			AddOrUpdateRoutingConfigurationFor(GetRoutingRequiringServices().Except(recordingsAndTransmissionsLinkedToSpecificService).ToList(), out var removedRoutingServices);
			allRemovedLiveVideoServices.AddRange(removedRoutingServices);

			AddOrUpdateRoutingConfigurationFor(recordingsAndTransmissionsLinkedToSpecificService, out removedRoutingServices);
			allRemovedLiveVideoServices.AddRange(removedRoutingServices);

			allRemovedServices = allRemovedLiveVideoServices.Select(s => s.Service).Distinct().ToList();
			Log(nameof(AddOrUpdateRoutingConfiguration), $"All removed services: '{string.Join(", ", allRemovedServices.Select(s => s.Name))}'");

			UpdateRoutingServiceIntegrationTypes();
			UpdateRoutingServicePreAndPostRolls();

			Order.SetTimingBasedOnServices(helpers);

			LogMethodCompleted(nameof(AddOrUpdateRoutingConfiguration), stopwatch);
		}

		private void ValidateServicesToRecordOrTransmit(List<RoutingRequiringService> recordingsAndTransmissionsLinkedToSpecificService)
		{
			foreach (var routingRequiringService in recordingsAndTransmissionsLinkedToSpecificService)
			{
				if (GetServiceToRecordOrTransmit(routingRequiringService) is null)
				{
					throw new ServiceNotFoundException($"Unable to find service to record or transmit {routingRequiringService.Service.NameOfServiceToTransmitOrRecord} for service {routingRequiringService.Service.Name}", true);
				}
			}
		}

		private void AddOrUpdateRoutingConfigurationFor(List<RoutingRequiringService> routingRequiringServices, out List<LiveVideoService> allRemovedLiveVideoServices)
		{
			allRemovedLiveVideoServices = new List<LiveVideoService>();

			foreach (var routingRequiringService in routingRequiringServices)
			{
				routingRequiringService.AddOrUpdateRoutingConfiguration(out var removedRoutingServices);

				allRemovedLiveVideoServices.AddRange(removedRoutingServices);

				allRemovedLiveVideoServices.AddRange(RemoveUnusedRoutingServices());
			}
		}

		private void InitializeRoutingServiceChains()
		{
			foreach (var routingRequiringService in GetRoutingRequiringServices())
			{
				routingRequiringService.InitializeCurrentRoutingServiceChain();
			}
		}

		public List<RoutingService> GetRoutingServicesUsedByMultipleChains()
		{
			var sharedRoutingServices = new List<RoutingService>();

			var allRoutingServiceChains = LiveVideoServices.Select(s => s as RoutingRequiringService).Where(s => s != null).Select(r => r.RoutingServiceChain).ToList();

			foreach (var routingServiceChain in allRoutingServiceChains)
			{
				foreach (var routingService in routingServiceChain.AllRoutingServices)
				{
					bool routingServiceIsSharedBetweenMultipleChains = allRoutingServiceChains.Count(rcs => rcs.UsesService(routingService.Service.Id)) >= 2;
					if (routingServiceIsSharedBetweenMultipleChains)
					{
						sharedRoutingServices.Add(routingService);
					}
				}
			}

			Log(nameof(GetRoutingServicesUsedByMultipleChains), $"Found shared routing service objects: '{string.Join(", ", sharedRoutingServices.Select(r => r.Service.Name))}'");

			return sharedRoutingServices;
		}

		private void UpdateParentsOfRecordingsAndTransmissionsOfDestinations()
		{
			LogMethodStart(nameof(UpdateParentsOfRecordingsAndTransmissionsOfDestinations), out var stopwatch);

			var recordingsAndTransmissionsOfDestinations = new List<EndPointService>();

			recordingsAndTransmissionsOfDestinations.AddRange(GetRecordings().Where(r => r.Service.RequiresRouting && r.Service.NameOfServiceToTransmitOrRecord != null && r.Service.NameOfServiceToTransmitOrRecord.Contains(VirtualPlatform.Destination.GetDescription())).ToList());
			recordingsAndTransmissionsOfDestinations.AddRange(GetTransmissions().Where(t => t?.Service?.NameOfServiceToTransmitOrRecord != null && t.Service.NameOfServiceToTransmitOrRecord.Contains(VirtualPlatform.Destination.GetDescription())).ToList());

			var plasmaRecordingsWithDestinationParents = GetRecordings().Where(r => r.Service.IntegrationType == IntegrationType.Plasma && r.Parent is DestinationService).ToList();
			recordingsAndTransmissionsOfDestinations = recordingsAndTransmissionsOfDestinations.Except(plasmaRecordingsWithDestinationParents).ToList();
			// filter out plasma recordings that come after a destination, they don't need their parent updated.
			// e.g.:	RX -	D -	Rec
			//					  -	Rec	

			foreach (var recordingOrTransmission in recordingsAndTransmissionsOfDestinations)
			{
				Log(nameof(UpdateParentsOfRecordingsAndTransmissionsOfDestinations), $"Updating linking for {recordingOrTransmission.Service.Name}, which is a rec or tx of {recordingOrTransmission.Service.NameOfServiceToTransmitOrRecord}");

				var destinationService = GetDestinations().SingleOrDefault(d => d.Service.Name == recordingOrTransmission.Service.NameOfServiceToTransmitOrRecord) ?? throw new ServiceNotFoundException($"Unable to find '{recordingOrTransmission.Service.NameOfServiceToTransmitOrRecord}' between Destinations '{string.Join(", ", GetDestinations().Select(d => d.Service.Name))}'", true);

				var youngestNonRoutingParentOfDestination = destinationService.GetYoungestNonRoutingParent();

				var oldestRoutingParentOfRecordingOrTransmission = recordingOrTransmission.GetOldestRoutingParentServiceOrThis();

				SetParent(oldestRoutingParentOfRecordingOrTransmission, youngestNonRoutingParentOfDestination);
			}

			LogMethodCompleted(nameof(UpdateParentsOfRecordingsAndTransmissionsOfDestinations));
		}

		/// <summary>
		/// Checks if any graphics processing service needs to be added, updated or removed for the children of this source.
		/// </summary>
		private void AddOrUpdateGraphicsProcessingServices()
		{
			LogMethodStart(nameof(AddOrUpdateGraphicsProcessingServices), out var stopwatch);

			var liveVideoServices = GetGraphicsProcessingEvaluationRequiringServices();

			foreach (var liveVideoService in liveVideoServices)
			{
				liveVideoService.AddOrUpdateGraphicsProcessingConfiguration();
			}

			ReuseUnusedExistingGraphicsProcessing(liveVideoServices);

			LogMethodCompleted(nameof(AddOrUpdateGraphicsProcessingServices), stopwatch);
		}

		private List<EndPointService> GetGraphicsProcessingEvaluationRequiringServices()
		{
			var liveVideoServices = new List<EndPointService>();
			liveVideoServices.AddRange(GetDestinations());
			var transmissionsOfSource = GetTransmissions().Where(t => !string.IsNullOrWhiteSpace(t.Service.NameOfServiceToTransmit) && t.Service.NameOfServiceToTransmit.Contains(VirtualPlatformType.Reception.GetDescription())).ToList();
			liveVideoServices.AddRange(transmissionsOfSource);
			return liveVideoServices;
		}

		private void ReuseUnusedExistingGraphicsProcessing(List<EndPointService> liveVideoServices)
		{
			var unusedExistingGraphicsProcessingServices = GetGraphicsProcessingServices().Where(gfx => !gfx.IsNew && !gfx.Children.Any()).ToList();

			Log(nameof(ReuseUnusedExistingGraphicsProcessing), $"Updating all graphics processing config resulted in unused existing graphics processing services '{string.Join(", ", unusedExistingGraphicsProcessingServices.Select(s => s.Service.Name))}'");

			if (unusedExistingGraphicsProcessingServices.Any())
			{
				Log(nameof(ReuseUnusedExistingGraphicsProcessing), $"Rerunning all graphics processing config to try to reuse unused existing graphics processing services");

				foreach (var liveVideoService in liveVideoServices)
				{
					liveVideoService.AddOrUpdateGraphicsProcessingConfiguration(unusedExistingGraphicsProcessingServices);

					unusedExistingGraphicsProcessingServices = GetGraphicsProcessingServices().Where(gfx => !gfx.IsNew && !gfx.Children.Any()).ToList();
					if (!unusedExistingGraphicsProcessingServices.Any())
					{
						Log(nameof(ReuseUnusedExistingGraphicsProcessing), $"No more unused existing graphics processing services left, stopping rerun");
						break;
					}
				}
			}
		}

		private void ReuseUnusedExistingVizremConverterServices(List<VizremConverterRequiringService> liveVideoServices)
		{
			var unusedExistingVizremConverterServices = GetVizremConverterServices().Where(vizremConverter => !vizremConverter.IsNew && !vizremConverter.Children.Any()).ToList();

			Log(nameof(ReuseUnusedExistingVizremConverterServices), $"Updating all vizrem converter config resulted in unused existing vizrem converter services '{string.Join(", ", unusedExistingVizremConverterServices.Select(s => s.Service.Name))}'");

			if (unusedExistingVizremConverterServices.Any())
			{
				Log(nameof(ReuseUnusedExistingVizremConverterServices), $"Rerunning all vizrem converter config to try to reuse unused existing vizrem converter services");

				foreach (var liveVideoService in liveVideoServices)
				{
					liveVideoService.AddOrUpdateVizremConverterConfiguration(unusedExistingVizremConverterServices);

					unusedExistingVizremConverterServices = GetVizremConverterServices().Where(vizremConverter => !vizremConverter.IsNew && !vizremConverter.Children.Any()).ToList();

					if (!unusedExistingVizremConverterServices.Any())
					{
						Log(nameof(ReuseUnusedExistingVizremConverterServices), $"No more unused existing vizrem converter services left, stopping rerun");
						break;
					}
				}
			}
		}

		private void AddOrUpdateVideoProcessingServices()
		{
			LogMethodStart(nameof(AddOrUpdateVideoProcessingServices), out var stopwatch);

			var liveVideoServices = GetVideoProcessingEvaluationRequiringServices();

			foreach (var liveVideoService in liveVideoServices)
			{
				var parentToCompare = (ProcessingRelatedService)(liveVideoService as EndPointService)?.GraphicsProcessingService ?? GetSource(liveVideoService as LiveVideoService);

				liveVideoService.AddOrUpdateVideoProcessingConfiguration(parentToCompare);
			}

			LogMethodCompleted(nameof(AddOrUpdateVideoProcessingServices), stopwatch);
		}

		private List<IVideoProcessingRequiringService> GetVideoProcessingEvaluationRequiringServices()
		{
			var liveVideoServices = new List<IVideoProcessingRequiringService>();
			liveVideoServices.AddRange(GetDestinations());
			liveVideoServices.AddRange(GetTransmissions().Where(t => t.Service.Definition.VirtualPlatform == VirtualPlatform.TransmissionEurovision || (!string.IsNullOrWhiteSpace(t.Service.NameOfServiceToTransmit) && t.Service.NameOfServiceToTransmit.Contains(VirtualPlatformType.Reception.GetDescription()))));
			liveVideoServices.AddRange(GetRecordings().Where(r => r.Service.RequiresRouting)); // when requires routing is set to false this means it's a recording linked to a destination and should also not be eligible for video processing
			liveVideoServices.AddRange(GetGraphicsProcessingServices());
			return liveVideoServices;
		}

		private void ReuseUnusedExistingVideoProcessing(List<IVideoProcessingRequiringService> liveVideoServices)
		{
			var unusedExistingVideoProcessingServices = GetVideoProcessingServices().Where(vps => !vps.IsNew && !vps.Children.Any()).ToList();

			Log(nameof(ReuseUnusedExistingVideoProcessing), $"Updating all video processing config resulted in unused existing video processing services '{string.Join(", ", unusedExistingVideoProcessingServices.Select(s => s.Service.Name))}'");

			if (unusedExistingVideoProcessingServices.Any())
			{
				Log(nameof(ReuseUnusedExistingVideoProcessing), $"Rerunning all video processing config to try to reuse unused existing video processing services");

				foreach (var liveVideoService in liveVideoServices)
				{
					var parentToCompare = (ProcessingRelatedService)(liveVideoService as EndPointService)?.GraphicsProcessingService ?? GetSource(liveVideoService as LiveVideoService);

					liveVideoService.AddOrUpdateVideoProcessingConfiguration(parentToCompare, unusedExistingVideoProcessingServices);

					unusedExistingVideoProcessingServices = GetVideoProcessingServices().Where(vps => !vps.IsNew && !vps.Children.Any()).ToList();
					if (!unusedExistingVideoProcessingServices.Any())
					{
						Log(nameof(ReuseUnusedExistingVideoProcessing), $"No more unused existing video processing services left, stopping rerun");
						break;
					}
				}
			}
		}

		/// <summary>
		/// Checks if any audio processing service needs to be added, updated or removed for the children of this source.
		/// </summary>
		private void AddOrUpdateAudioProcessingServices()
		{
			LogMethodStart(nameof(AddOrUpdateAudioProcessingServices), out var stopwatch);

			var audioProcessingRequiringServices = GetAudioProcessingEvaluationRequiringServices();

			foreach (var liveVideoService in audioProcessingRequiringServices)
			{
				liveVideoService.AddOrUpdateAudioProcessingConfiguration();
			}

			ReuseUnusedExistingAudioProcessing(audioProcessingRequiringServices);

			LogMethodCompleted(nameof(AddOrUpdateAudioProcessingServices), stopwatch);
		}

		private List<EndPointService> GetAudioProcessingEvaluationRequiringServices()
		{
			var audioProcessingRequiringServices = new List<EndPointService>();
			audioProcessingRequiringServices.AddRange(GetDestinations());
			audioProcessingRequiringServices.AddRange(GetTransmissions().Where(t => t.Service.Definition.VirtualPlatform == VirtualPlatform.TransmissionEurovision || t.Service.NameOfServiceToTransmit.Contains(VirtualPlatformType.Reception.GetDescription())));
			audioProcessingRequiringServices.AddRange(GetRecordings().Where(r => r.Service.RequiresRouting && r.Service.NameOfServiceToTransmitOrRecord.Contains(VirtualPlatformType.Reception.GetDescription()))); // when requires routing is set to false this means it's a recording linked to a destination (from Plasma) and should also not be eligible for audio processing
			return audioProcessingRequiringServices;
		}

		private void ReuseUnusedExistingAudioProcessing(List<EndPointService> audioProcessingRequiringServices)
		{
			var unusedExistingAudioProcessingServices = GetAudioProcessingServices().Where(aps => !aps.IsNew && !aps.Children.Any()).ToList();

			Log(nameof(ReuseUnusedExistingAudioProcessing), $"Updating all audio processing config resulted in unused existing audio processing services '{string.Join(", ", unusedExistingAudioProcessingServices.Select(s => s.Service.Name))}'");

			if (unusedExistingAudioProcessingServices.Any())
			{
				Log(nameof(ReuseUnusedExistingAudioProcessing), $"Rerunning all audio processing config to try to reuse unused existing audio processing services");

				foreach (var liveVideoService in audioProcessingRequiringServices)
				{
					liveVideoService.AddOrUpdateAudioProcessingConfiguration(unusedExistingAudioProcessingServices);

					unusedExistingAudioProcessingServices = GetAudioProcessingServices().Where(aps => !aps.IsNew && !aps.Children.Any()).ToList();
					if (!unusedExistingAudioProcessingServices.Any())
					{
						Log(nameof(ReuseUnusedExistingAudioProcessing), $"No more unused existing audio processing services left, stopping rerun");
						break;
					}
				}
			}
		}

		/// <summary>
		/// When a audio processing service undergoes a change then the audio configuration of the linked live video services need to be updated.
		/// Those linked live video services will determine when certain audio processing function resources need to be booked.
		/// </summary>
		/// <param name="audioProcessingService">Audio processing service which got changed through edit service.</param>
		public void UpdateLinkedLiveVideoServicesWhenAudioProcessingChanges(Service audioProcessingService)
		{
			var liveVideoAudioProcessingService = GetAudioProcessingServices().FirstOrDefault(x => x.Service.Id == audioProcessingService.Id);

			liveVideoAudioProcessingService?.UpdateSourceDolbyDecodingRequirement();
			liveVideoAudioProcessingService?.UpdateEndPointServicesAudioProcessingConfiguration(GetEndPointServices().Where(s => ServicesArePartOfSameChain(liveVideoAudioProcessingService, s)).ToList());
		}

		public void InsertServicesBetween(List<LiveVideoService> servicesToInsert, LiveVideoService parent, LiveVideoService child)
		{
			if (!servicesToInsert.Any()) return;

			var youngToOldServices = new List<LiveVideoService> { child };
			var revertedServicesToInsert = new List<LiveVideoService>(servicesToInsert);
			revertedServicesToInsert.Reverse();
			youngToOldServices.AddRange(revertedServicesToInsert);
			youngToOldServices.Add(parent);

			for (int i = 0; i < youngToOldServices.Count - 2; i++)
			{
				var newChild = youngToOldServices[i];
				var newParent = youngToOldServices[i + 1];

				SetParent(newChild, newParent, false, false);
			}

			SetParent(servicesToInsert[0], parent, true, true);
		}

		public void InsertServiceBetween(LiveVideoService serviceToInsert, LiveVideoService parent, LiveVideoService child)
		{
			if (serviceToInsert is null) throw new ArgumentNullException(nameof(serviceToInsert));
			if (parent is null) throw new ArgumentNullException(nameof(parent));
			if (child is null) throw new ArgumentNullException(nameof(child));

			Log(nameof(InsertServiceBetween), $"Adding {serviceToInsert.Service.Name} between {parent.Service?.Name} and {child.Service?.Name}");

			SetParent(serviceToInsert, parent, false, false);
			SetParent(child, serviceToInsert, true, true);
		}

		private void SetParent(LiveVideoService child, LiveVideoService parent, bool updateChains, bool updateLinkedServices)
		{
			if (child is null) throw new ArgumentNullException(nameof(child));
			if (parent is null) throw new ArgumentNullException(nameof(parent));

			bool serviceAlreadyHasParent = child.Parent != null;

			if (serviceAlreadyHasParent)
			{
				if (child.Parent.Equals(parent))
				{
					Log(nameof(SetParent), $"Parent of {child.Service.Name} is already {parent.Service.Name}, no change required");
					return;
				}

				Log(nameof(SetParent), $"Removing {child.Service.Name} from the children of {child.Parent.Service.Name}");

				child.Parent.RemoveChild(child);
			}

			Log(nameof(SetParent), $"Setting parent of {child.Service.Name} to {parent.Service.Name}");

			child.Parent = parent;

			parent.AddChild(child);

			if (updateChains) InitializeChains();

			if (updateLinkedServices) InitializeLinkedServices();
		}

		/// <summary>
		/// Set the Parent of the child to the parent argument.
		/// </summary>
		/// <param name="parent">The parent service to set as parent of the child.</param>
		/// <param name="child">The service to change its parent of.</param>
		public void SetParent(LiveVideoService child, LiveVideoService parent)
		{
			SetParent(child, parent, true, true);
		}

		public List<RoutingServiceChain> GetRoutingServiceChains()
		{
			return GetRoutingRequiringServices()
				.Select(s => s.RoutingServiceChain)
				.Where(rsc => rsc != null).ToList();
		}

		public List<RoutingServiceChain> GetRoutingServiceChainsForService(Guid serviceId)
		{
			return GetRoutingServiceChains().Where(rcs => rcs.UsesService(serviceId)).ToList();
		}

		public List<RoutingServiceChain> GetRoutingServiceChainsWithSameInputServiceAs(Guid serviceId)
		{
			var inputServiceIds = GetRoutingServiceChainsForService(serviceId).Select(rsc => rsc.InputService.Service.Id);

			return GetRoutingServiceChains().Where(rcs => inputServiceIds.Contains(rcs.InputService.Service.Id)).ToList();
		}

		public List<RoutingServiceChain> GetRoutingServiceChainsConnectedToSameSourceAs(LiveVideoService liveVideoService)
		{
			if (liveVideoService is null) throw new ArgumentNullException(nameof(liveVideoService));

			var sourceService = GetSource(liveVideoService) ?? throw new ServiceNotFoundException($"Unable to find source service for {liveVideoService.Service.Name} in chains {string.Join("\n", chains.Select(c => string.Join(" -> ", c)))}", true);

			var routingServiceChains = GetRoutingServiceChains();

			return routingServiceChains.Where(rsc => (GetSource(rsc.OutputService) ?? throw new ServiceNotFoundException($"Unable to find source service for {rsc.OutputService.Service.Name} in chains {string.Join("\n", chains.Select(c => string.Join(" -> ", c)))}", true)).Equals(sourceService)).ToList();
		}

		public List<RoutingServiceChain> GetRoutingServiceChainsConnectedToOtherSourceThan(LiveVideoService liveVideoService)
		{
			if (liveVideoService is null) throw new ArgumentNullException(nameof(liveVideoService));

			var sourceService = GetSource(liveVideoService) ?? throw new ServiceNotFoundException($"Unable to find source service for {liveVideoService.Service.Name} in chains {string.Join("\n", chains.Select(c => string.Join(" -> ", c)))}", true);

			var routingServiceChains = GetRoutingServiceChains();

			return routingServiceChains.Where(rsc => !(GetSource(rsc.OutputService) ?? throw new ServiceNotFoundException($"Unable to find source service for {rsc.OutputService.Service.Name} in chains {string.Join("\n", chains.Select(c => string.Join(" -> ", c)))}", true)).Equals(sourceService)).ToList();
		}

		public List<RoutingService> GetRoutingParents(RoutingRequiringService routingRequiringService)
		{
			var routingParents = new List<RoutingService>();

			var nextParent = routingRequiringService.Parent;
			while (nextParent is RoutingService routingParent)
			{
				routingParents.Add(routingParent);

				nextParent = nextParent.Parent;
			}

			return routingParents;
		}

		/// <summary>
		/// This method clears the routing output resource when a destination/recording resource is cleared and vice versa. 
		/// </summary>
		/// <param name="service"></param>
		/// <param name="function"></param>
		public void ClearMatchingResourceOnNeighborServiceConnectedFunction(Service service, Function function)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));
			if (function == null) throw new ArgumentNullException(nameof(function));

			LogMethodStart(nameof(ClearMatchingResourceOnNeighborServiceConnectedFunction), out var stopwatch);

			var liveVideoService = LiveVideoServices.SingleOrDefault(s => s.Service.Id == service.Id) ?? throw new ServiceNotFoundException($"Unable to find Live Video Service object for service {service.Id}", true);

			bool onlyOneFunctionInService = liveVideoService.Service.Functions.Count == 1;
			if (onlyOneFunctionInService || liveVideoService.Service.FunctionIsFirstResourceRequiringFunctionInDefinition(helpers, function))
			{
				ClearParentService(liveVideoService);
			}

			if (onlyOneFunctionInService || liveVideoService.Service.FunctionIsLastResourceRequiringFunctionInDefinition(helpers, function))
			{
				ClearChildServices(liveVideoService);
			}

			LogMethodCompleted(nameof(ClearMatchingResourceOnNeighborServiceConnectedFunction), stopwatch);
		}

		private void ClearParentService(LiveVideoService service)
		{
			Log(nameof(ClearParentService), $"Function is first (or only one) in service definition, trying to find parent routing service");

			if (service.Parent is RoutingService)
			{
				Log(nameof(ClearParentService), $"Found parent routing service {service.Parent.Service.Name}");

				var parentServiceConnectedFunction = service.Parent.Service.Functions.SingleOrDefault(f => service.Parent.Service.Definition.FunctionIsLast(f));

				if (parentServiceConnectedFunction != null)
				{
					parentServiceConnectedFunction.Resource = null;

					Log(nameof(ClearParentService), $"Found parent routing service last function {parentServiceConnectedFunction.Name}, resource is set to {parentServiceConnectedFunction.ResourceName}");
				}
			}
		}

		private void ClearChildServices(LiveVideoService service)
		{
			Log(nameof(ClearChildServices), $"Function is last resource-requiring function in service definition, trying to find child routing services");

			foreach (var child in service.Children)
			{
				if (!(child is RoutingService)) continue;

				Log(nameof(ClearChildServices), $"Found child routing service {child.Service.Name}");

				var childServiceConnectedFunction = child.Service.Functions.SingleOrDefault(f => child.Service.Definition.FunctionIsFirst(f));
				if (childServiceConnectedFunction == null) continue;

				childServiceConnectedFunction.Resource = null;

				Log(nameof(ClearChildServices), $"Found child routing service first function {childServiceConnectedFunction.Name}, resource is set to {childServiceConnectedFunction.ResourceName}");
			}
		}

		/// <summary>
		/// Sets the integration type of the routing services based on integration types of the source and child services it connects.
		/// </summary>
		private void UpdateRoutingServiceIntegrationTypes()
		{
			foreach (var routingService in GetRoutingServices())
			{
				var nonRoutingParent = routingService.GetYoungestNonRoutingParent();
				if (nonRoutingParent == null)
				{
					// should never happen
					continue;
				}

				var nonRoutingChildren = routingService.GetDirectNonRoutingChildren();
				if (nonRoutingChildren == null || !nonRoutingChildren.Any())
				{
					// should never happen
					continue;
				}

				if (nonRoutingChildren.Any(c => c.Service.IntegrationType == IntegrationType.Feenix))
				{
					routingService.Service.IntegrationType = IntegrationType.Feenix;
				}
				else if (nonRoutingChildren.Any(c => c.Service.IntegrationType == IntegrationType.Plasma))
				{
					// if any child is from Plasma integration then the routing service should also be tagged as a Plasma service
					routingService.Service.IntegrationType = IntegrationType.Plasma;
				}
				else
				{
					routingService.Service.IntegrationType = IntegrationType.None;
				}
			}
		}

		private void UpdateRoutingServicePreAndPostRolls()
		{
			foreach (var routingService in GetRoutingServices())
			{
				var nonRoutingChildren = routingService.GetDirectNonRoutingChildren();
				if (nonRoutingChildren == null || !nonRoutingChildren.Any())
				{
					// should never happen
					continue;
				}

				// copy the pre and post roll from the non routing child to this routing service
				var nonRoutingChild = nonRoutingChildren.FirstOrDefault() ?? throw new NotFoundException($"Could not find non-routing child");
				routingService.Service.PreRoll = nonRoutingChild.Service.PreRoll;
				routingService.Service.PostRoll = nonRoutingChild.Service.PostRoll;
			}
		}

		private void RemoveUnusedNonEndPointServices()
		{
			var removedNonEndPointServices = new List<LiveVideoService>();

			var nonEndPointServicesWithoutChildren = LiveVideoServices.Except(GetEndPointServices()).Except(GetVizremStudioServices()).Where(s => !s.Children.Any()).ToList();

			while (nonEndPointServicesWithoutChildren.Any())
			{
				foreach (var service in nonEndPointServicesWithoutChildren)
				{
					Log(nameof(RemoveUnusedNonEndPointServices), $"Unused non-endpoint service {service.Service.Name} has parent '{service.Parent?.Service?.Name}'");

					LiveVideoServices.Remove(service);

					service.Parent?.RemoveChild(service);

					removedNonEndPointServices.Add(service);
				}

				nonEndPointServicesWithoutChildren = LiveVideoServices.Except(GetEndPointServices()).Except(GetVizremStudioServices()).Where(s => !s.Children.Any()).ToList();
			}

			Log(nameof(RemoveUnusedNonEndPointServices), $"Removed unused non-endpoint services: '{string.Join(", ", removedNonEndPointServices.Select(s => s.Service.Name))}'");

			if (removedNonEndPointServices.Any())
			{
				InitializeChains();
			}
		}

		/// <summary>
		/// Removes any Routing services that are not used.
		/// </summary>
		private List<RoutingService> RemoveUnusedRoutingServices()
		{
			var removedRoutingServices = new List<RoutingService>();

			var routingServicesWithoutChildren = GetRoutingServices().Where(rs => !rs.Children.Any()).ToList();

			while (routingServicesWithoutChildren.Any())
			{
				foreach (var routingService in routingServicesWithoutChildren)
				{
					Log(nameof(RemoveUnusedRoutingServices), $"Unused routing service {routingService.Service.Name} has parent '{routingService.Parent?.Service?.Name}'");

					LiveVideoServices.Remove(routingService);

					routingService.Parent?.RemoveChild(routingService);

					removedRoutingServices.Add(routingService);
				}

				routingServicesWithoutChildren = GetRoutingServices().Where(rs => !rs.Children.Any()).ToList();
			}

			Log(nameof(RemoveUnusedRoutingServices), $"Removed unused routing services: '{string.Join(", ", removedRoutingServices.Select(s => s.Service.Name))}'");

			return removedRoutingServices;
		}

		private LiveVideoService GetServiceToRecordOrTransmit(LiveVideoService service)
		{
			var serviceToRecordOrTransmitName = service.Service.NameOfServiceToTransmitOrRecord;

			if (string.IsNullOrWhiteSpace(serviceToRecordOrTransmitName)) return null;

			LiveVideoService serviceToRecordOrTransmit;

			var source = GetSource(service);
			var possibleServiceToRecordOrTransmit = new List<LiveVideoService>(GetDestinations()) { source };

			Log(nameof(GetServiceToRecordOrTransmit), $"Trying to find service to record or transmit {serviceToRecordOrTransmitName} between services {string.Join(",", possibleServiceToRecordOrTransmit.Select(s => s.Service.Name))}");

			var shouldServiceBeLinkedToSource = serviceToRecordOrTransmitName.Contains("Reception");
			if (shouldServiceBeLinkedToSource)
			{
				// in this case we need to make sure that the recording/transmission is correctly linked to the source
				serviceToRecordOrTransmit = source;
			}
			else
			{
				// in this case the recording/transmission should be linked to a specific destination
				serviceToRecordOrTransmit = GetDestinations().FirstOrDefault(d => d.Service.Name == serviceToRecordOrTransmitName);
			}

			return serviceToRecordOrTransmit;
		}

		public SourceService GetSource(LiveVideoService liveVideoService)
		{
			if (liveVideoService is null) throw new ArgumentNullException(nameof(liveVideoService));

			var chain = chains.FirstOrDefault(c => c.Contains(liveVideoService));

			var source = chain?.First?.Value as SourceService;

			return source;
		}

		public SourceService GetSource(Service service)
		{
			if (service is null) throw new ArgumentNullException(nameof(service));

			var chain = chains.FirstOrDefault(c => c.Any(node => node.Service.Equals(service)));

			var source = chain?.First?.Value as SourceService;

			return source;
		}

		public bool ServiceIsShared(LiveVideoService service)
		{
			bool serviceIsShared = chains.Count(chain => chain.Contains(service)) > 1;

			Log(nameof(ServiceIsShared), $"Service {service.Service.Name} is{(serviceIsShared ? string.Empty : " not")} shared");

			return serviceIsShared;
		}

		public void StopSharingService(LiveVideoService service)
		{
			// set the parent of all children (except one) to the source to make the service unshared

			var childThatWillKeepHisChain = service.Children.FirstOrDefault(s => s.Service.IsBooked) ?? service.Children[0];

			var allChildrenThatShouldBeConnectedToSource = service.Children.Except(childThatWillKeepHisChain.Yield()).ToList();

			foreach (var child in allChildrenThatShouldBeConnectedToSource)
			{
				SetParent(child, GetSource(child));
			}
		}

		public List<LiveVideoService> GetYoungestShareableServices(LiveVideoService serviceOfFirstChain, LiveVideoService serviceOfSecondChain)
		{
			var sourceOfFirstChain = GetSource(serviceOfFirstChain);
			if (!sourceOfFirstChain.Equals(GetSource(serviceOfSecondChain))) return new List<LiveVideoService>();

			var youngestShareableServices = new List<LiveVideoService> { sourceOfFirstChain };

			var firstChain = chains.Single(c => c.Contains(serviceOfFirstChain));
			var secondChain = chains.Single(c => c.Contains(serviceOfSecondChain));

			var nextRoutingRequiringServiceOfFirstChain = firstChain.OfType<RoutingRequiringService>().First();
			var nextRoutingRequiringServiceOfSecondChain = secondChain.OfType<RoutingRequiringService>().First();

			bool signalIsIdentical = nextRoutingRequiringServiceOfFirstChain.ProcessesSignalSameAs(nextRoutingRequiringServiceOfSecondChain);

			while (signalIsIdentical)
			{
				youngestShareableServices = new List<LiveVideoService> { nextRoutingRequiringServiceOfFirstChain, nextRoutingRequiringServiceOfSecondChain }.Distinct().ToList();

				var remainingFirstChain = firstChain.SkipWhile(s => !s.Equals(nextRoutingRequiringServiceOfFirstChain)).Skip(1);
				var remainingSecondChain = secondChain.SkipWhile(s => !s.Equals(nextRoutingRequiringServiceOfSecondChain)).Skip(1);

				nextRoutingRequiringServiceOfFirstChain = remainingFirstChain.OfType<RoutingRequiringService>().FirstOrDefault();
				nextRoutingRequiringServiceOfSecondChain = remainingSecondChain.OfType<RoutingRequiringService>().FirstOrDefault();

				if (nextRoutingRequiringServiceOfFirstChain is null || nextRoutingRequiringServiceOfSecondChain is null) break;

				signalIsIdentical = nextRoutingRequiringServiceOfFirstChain.ProcessesSignalSameAs(nextRoutingRequiringServiceOfSecondChain);
			}

			Log(nameof(GetYoungestShareableServices), $"Found youngest shareable services {string.Join(",", youngestShareableServices.Select(s => s.Service.Name))}");

			return youngestShareableServices;
		}

		public bool ServicesArePartOfSameChain(params LiveVideoService[] liveVideoServices)
		{
			foreach (var chain in chains)
			{
				bool chainContainsAllServices = true;
				foreach (var liveVideoService in liveVideoServices)
				{
					chainContainsAllServices &= chain.Contains(liveVideoService);
				}

				if (chainContainsAllServices)
				{
					return true;
				}
			}

			return false;
		}

		public bool ServicesArePartOfSameChain(params Service[] services)
		{
			foreach (var chain in chains)
			{
				bool chainContainsAllServices = true;
				foreach (var service in services)
				{
					chainContainsAllServices &= chain.Select(s => s.Service).Contains(service);
				}

				if (chainContainsAllServices)
				{
					return true;
				}
			}

			return false;
		}

		private void InitializeLinkedServices()
		{
			foreach (var chain in chains)
			{
				foreach (var node in chain.ReverseNodes())
				{
					var service = node.Value;
					var parentService = node.Previous?.Value;

					InitializeLinkedRoutingServices(service, parentService);

					if (service is EndPointService)
					{
						InitializeLinkedProcessingServicesForEndPointServices(chain, node);
					}
					else if (service is IVideoProcessingRequiringService)
					{
						InitializeLinkedVideoProcessingServiceForGraphicsProcessingService(chain, node);
					}
					else
					{
						//Nothing
					}
				}

				// Vizrem converter linking
				foreach (var service in chain)
				{
					var linkedNode = chain.Find(service);
					if (service is VizremConverterRequiringService vizremRequiringService && linkedNode != null)
					{
						vizremRequiringService.VizremConverterService = linkedNode.Next?.Value as VizremConverterService;
					}
				}
			}
		}

		public static List<T> GetAllDescendantsOfType<T>(LiveVideoService liveVideoService)
		{
			return GetAllDescendantsOfType<T>(liveVideoService.Children);
		}

		private static List<T> GetAllDescendantsOfType<T>(IEnumerable<LiveVideoService> children)
		{
			var matchingChildren = children.OfType<T>().ToList();

			foreach (var child in children)
			{
				matchingChildren.AddRange(GetAllDescendantsOfType<T>(child.Children));
			}

			return matchingChildren;
		}

		private static void InitializeLinkedRoutingServices(LiveVideoService service, LiveVideoService parentService)
		{
			if (service is null) throw new ArgumentNullException(nameof(service));

			service.RoutingParent = null;
			service.RoutingChild = null;

			if (parentService is RoutingService parentRouting)
			{
				service.RoutingParent = parentRouting;
			}

			if (parentService != null && service is RoutingService routingService)
			{
				parentService.RoutingChild = routingService;
			}
			else
			{
				var routingServiceChild = service.Children.OfType<RoutingService>().FirstOrDefault();
				if (routingServiceChild != null)
				{
					service.RoutingChild = routingServiceChild;
				}
			}
		}

		private static void InitializeLinkedVideoProcessingServiceForGraphicsProcessingService(LinkedList<LiveVideoService> chain, LinkedListNode<LiveVideoService> graphicsProcessingServiceNode)
		{
			var graphicsProcessingService = graphicsProcessingServiceNode.Value as IVideoProcessingRequiringService;
			if (graphicsProcessingService == null) throw new ArgumentException($"Node does not represent an {nameof(IVideoProcessingRequiringService)}");

			bool foundVideoProcessing = false;
			foreach (var parentNode in chain.ReverseNodesStartFrom(graphicsProcessingServiceNode))
			{
				if (parentNode.Value is VideoProcessingService videoProcessingParent)
				{
					foundVideoProcessing = true;
					graphicsProcessingService.VideoProcessingService = videoProcessingParent;
				}
			}

			if (!foundVideoProcessing)
			{
				graphicsProcessingService.VideoProcessingService = null;
			}
		}

		private void InitializeLinkedProcessingServicesForEndPointServices(LinkedList<LiveVideoService> chain, LinkedListNode<LiveVideoService> endPointServiceNode)
		{
			var endPointService = endPointServiceNode.Value as EndPointService;
			if (endPointService == null) throw new ArgumentException($"Node does not represent an {nameof(EndPointService)}");

			// Note: the order of processing services is 1.Graphics 2.Video 3.Audio.

			bool audioProcessingFound = false;
			bool videoProcessingFound = false;
			bool graphicsProcessingFound = false;

			foreach (var parentNode in chain.ReverseNodesStartFrom(endPointServiceNode))
			{
				var parentService = parentNode.Value;

				if (!audioProcessingFound && parentService is AudioProcessingService audioProcessingParent)
				{
					endPointService.AudioProcessingService = audioProcessingParent;

					audioProcessingFound = true;
				}
				else if (!graphicsProcessingFound && !videoProcessingFound && parentService is VideoProcessingService videoProcessingService)
				{
					// Use case 1: RX - R - VP - R - GP - R - TX : the VP here is used for the GP and not for the TX
					// Use case 2: RX - R - GP - R - VP - R - TX : the VP here is used for the TX and not for the GP
					// Use case 3: RX - R - VP - R - TX	: the VP here is used for the TX
					// Conclusion: Every VP that is found before a GP is found is used for the EndPointService connected to the node argument.
					// Subsequently every VP that is found after a GP is found is used for that GP and not for the EndPointService.
					// Note that we loop backwards over the chain.

					endPointService.VideoProcessingService = videoProcessingService;

					videoProcessingFound = true;
				}
				else if (!graphicsProcessingFound && parentService is GraphicsProcessingService graphicsProcessingParent)
				{
					endPointService.GraphicsProcessingService = graphicsProcessingParent;

					graphicsProcessingFound = true;
				}
				else
				{
					//Nothing
				}
			}

			if (!videoProcessingFound)
			{
				endPointService.VideoProcessingService = null;
			}

			if (!graphicsProcessingFound)
			{
				endPointService.GraphicsProcessingService = null;
			}

			if (!audioProcessingFound)
			{
				endPointService.AudioProcessingService = null;
			}

			helpers.Log(nameof(LiveVideoOrder), nameof(InitializeLinkedProcessingServicesForEndPointServices), $"{nameof(videoProcessingFound)}={videoProcessingFound}, {nameof(graphicsProcessingFound)}={graphicsProcessingFound}, {nameof(audioProcessingFound)}={audioProcessingFound}");
		}

		private void InitializeChains()
		{
			chains.Clear();

			var servicesWithoutChildren = new List<Service>();

			foreach (var service in Order.AllServices)
			{
				var parent = Order.AllServices.SingleOrDefault(s => s.Children.Contains(service));
				if (parent != null && parent.Definition.VirtualPlatform == VirtualPlatform.Destination)
				{
					// special treatment for plasma recordings after destinations: these are not converted into LiveVideoService objects and therefore never considered during any generation of processing or routing

					if (servicesWithoutChildren.Contains(parent)) continue;

					servicesWithoutChildren.Add(parent);
				}
				else if (!service.Children.Any())
				{
					servicesWithoutChildren.Add(service);
				}
				else
				{
					//Nothing
				}
			}

			Log(nameof(InitializeChains), $"Services without children: '{string.Join(", ", servicesWithoutChildren.Select(s => s.Name))}'");

			foreach (var serviceWithoutChildren in servicesWithoutChildren)
			{
				var liveVideoService = ConvertToLiveVideoService(serviceWithoutChildren);
				if (liveVideoService == null) continue; // Is the case for Dummy services

				var chain = new LinkedList<LiveVideoService>();
				chain.AddLast(liveVideoService);

				var child = serviceWithoutChildren;
				var parent = Order.AllServices.SingleOrDefault(s => s.Children.Contains(child));
				while (parent != null)
				{
					var parentLiveVideoService = ConvertToLiveVideoService(parent);
					chain.AddFirst(parentLiveVideoService);

					child = parent;
					parent = Order.AllServices.SingleOrDefault(s => s.Children.Contains(child));
				}

				helpers.Log(nameof(LiveVideoOrder), nameof(InitializeChains), $"Adding chain from {chain.First.Value.Service.Name} to {chain.Last.Value.Service.Name}");

				chains.Add(chain);
			}
		}

		public List<LiveVideoService> GetChildren(LiveVideoService liveVideoService)
		{
			var allChainsWithService = chains.Where(chain => chain.Contains(liveVideoService)).ToList();

			var children = new List<LiveVideoService>();

			foreach (var chain in allChainsWithService)
			{
				var childNode = chain.Find(liveVideoService).Next;

				if (childNode != null)
				{
					children.Add(childNode.Value);
				}
			}

			return children;
		}

		public LiveVideoService GetParent(LiveVideoService liveVideoService)
		{
			var chainWithService = chains.FirstOrDefault(chain => chain.Contains(liveVideoService));

			return chainWithService?.Find(liveVideoService)?.Previous?.Value;
		}

		private void InitializeParents()
		{
			foreach (var chain in chains)
			{
				foreach (var node in chain.ReverseNodes())
				{
					var service = node.Value;
					var parentService = node.Previous?.Value;

					if (parentService != null)
					{
						InitialSetParent(service, parentService);
					}
				}
			}
		}

		private void InitialSetParent(LiveVideoService child, LiveVideoService parent)
		{
			Log(nameof(InitialSetParent), $"Setting parent of {child.Service.Name} to {parent.Service.Name}");

			child.Parent = parent;

			parent.AddChild(child, false);
		}

		public override string ToString()
		{
			return "Chains in order:\n" + String.Join("\n", chains.Select(x => String.Join(" -> ", x.Select(y => y.Service.Name))));
		}

		private LiveVideoService ConvertToLiveVideoService(Service serviceToConvert)
		{
			if (serviceToConvert == null) throw new ArgumentNullException(nameof(serviceToConvert));

			var existingLiveVideoService = LiveVideoServices.SingleOrDefault(s => s.Service.Equals(serviceToConvert));

			if (existingLiveVideoService != null)
			{
				return existingLiveVideoService;
			}
			else
			{
				Log(nameof(ConvertToLiveVideoService), $"Live video service object not found for service {serviceToConvert.Name} with VP {serviceToConvert.Definition?.VirtualPlatformServiceType}");
			}

			LiveVideoService liveVideoService = null;
			switch (serviceToConvert.Definition.VirtualPlatformServiceType)
			{
				case VirtualPlatformType.Reception:
					liveVideoService = new SourceService(helpers, serviceToConvert, this);
					break;
				case VirtualPlatformType.Destination:
					liveVideoService = new DestinationService(helpers, serviceToConvert, this);
					break;
				case VirtualPlatformType.Recording:
					liveVideoService = new RecordingService(helpers, serviceToConvert, this);
					break;
				case VirtualPlatformType.GraphicsProcessing:
					liveVideoService = new GraphicsProcessingService(helpers, serviceToConvert, this);
					break;
				case VirtualPlatformType.VideoProcessing:
					liveVideoService = new VideoProcessingService(helpers, serviceToConvert, this);
					break;
				case VirtualPlatformType.AudioProcessing:
					liveVideoService = new AudioProcessingService(helpers, serviceToConvert, this);
					break;
				case VirtualPlatformType.Routing:
					liveVideoService = new RoutingService(helpers, serviceToConvert, this);
					break;
				case VirtualPlatformType.Transmission:
					liveVideoService = new TransmissionService(helpers, serviceToConvert, this);
					break;
				case VirtualPlatformType.VizremStudio:
					liveVideoService = new VizremStudioService(helpers, serviceToConvert, this);
					break;
				case VirtualPlatformType.VizremFarm:
					liveVideoService = new VizremFarmService(helpers, serviceToConvert, this);
					break;
				case VirtualPlatformType.VizremNC2Converter:
					liveVideoService = new VizremConverterService(helpers, serviceToConvert, this);
					break;
				default:
					// Unsupported virtual platform type
					break;
			}

			LiveVideoServices.Add(liveVideoService);

			return liveVideoService;
		}

		protected void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch, string nameOfObject = null)
		{
			helpers.LogMethodStart(nameof(LiveVideoOrder), nameOfMethod, out stopwatch, nameOfObject);
		}

		protected void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch = null)
		{
			helpers.LogMethodCompleted(nameof(LiveVideoOrder), nameOfMethod, null, stopwatch);
		}

		protected void Log(string nameOfMethod, string message)
		{
			helpers.Log(nameof(LiveVideoOrder), nameOfMethod, message);
		}

		/// <summary>
		/// Updates the start- and end times of the routing services that are part of the same chain as the provided routingService based on the source and output services.
		/// </summary>
		/// <param name="changedRoutingService"></param>
		internal void UpdateRoutingServiceTimings(RoutingService changedRoutingService)
		{
			helpers.LogMethodStart(nameof(LiveVideoOrder), nameof(UpdateRoutingServiceTimings), out Stopwatch stopwatch, changedRoutingService.Service.Name);

			var routingChainsToUpdate = chains.Where(x => x.Contains(changedRoutingService));

			foreach (var chainToUpdate in routingChainsToUpdate)
			{
				foreach (var routingService in chainToUpdate.Where(x => x is RoutingService))
				{
					if (routingService.AllChildrenAndGrandChildren.Count(x => x is EndPointService) <= 1) continue;

					List<LiveVideoService> allOutputServices = routingService.AllChildrenAndGrandChildren.Where(x => !(x is RoutingService)).ToList();

					DateTime earliestStartTime = allOutputServices.Min(x => x.Service.Start);
					DateTime latestEndTime = allOutputServices.Max(x => x.Service.End);

					helpers.Log(nameof(RoutingService), nameof(UpdateRoutingServiceTimings), $"Changing start time of Service {routingService.Service.Name} from {routingService.Service.Start} to {earliestStartTime}");
					routingService.Service.Start = earliestStartTime;

					helpers.Log(nameof(RoutingService), nameof(UpdateRoutingServiceTimings), $"Changing end time of Service {routingService.Service.Name} from {routingService.Service.End} to {latestEndTime}");
					routingService.Service.End = latestEndTime;
				}
			}

			helpers.LogMethodCompleted(nameof(LiveVideoOrder), nameof(UpdateRoutingServiceTimings), changedRoutingService.Service.Name, stopwatch);
		}
	}
}
