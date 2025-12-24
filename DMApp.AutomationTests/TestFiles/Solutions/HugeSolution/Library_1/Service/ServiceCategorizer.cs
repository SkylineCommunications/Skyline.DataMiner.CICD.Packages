namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.Utils.YLE.Integrations;

    public static class ServiceCategorizer
	{
		public static bool IsConsideredAsMcrDestinationInMcrView(Helpers helpers, Service service)
		{
			bool isValidLiveRecordingService = !string.IsNullOrWhiteSpace(service.Definition.Description) && service.Definition.Description.Contains("Messi Live") && service.IntegrationType != IntegrationType.Plasma;
			if (isValidLiveRecordingService)
			{
				return true;
			}
			else if (service.Definition.VirtualPlatform == VirtualPlatform.Routing)
			{
				if (service.IsSharedSource) return true;

				var recordingServiceChilds = service.GetChildServicesForVirtualPlatform(VirtualPlatform.Recording);
				bool anyNewsRecordingChild = recordingServiceChilds != null && recordingServiceChilds.Any(recording => recording.Definition?.Description != null && recording.Definition.Description.Contains("News"));

				var destinationServiceChilds = service.GetChildServicesForVirtualPlatform(VirtualPlatform.Destination);
				bool anyNewsDestinationChild = destinationServiceChilds != null && destinationServiceChilds.Any();

				var audioProcessingServiceChilds = service.GetChildServicesForVirtualPlatform(VirtualPlatform.AudioProcessing);
				bool anyAudioProcessingChild = audioProcessingServiceChilds != null && audioProcessingServiceChilds.Any();

				var videoProcessingServiceChilds = service.GetChildServicesForVirtualPlatform(VirtualPlatform.VideoProcessing);
				bool anyVideoProcessingChild = videoProcessingServiceChilds != null && videoProcessingServiceChilds.Any();

				bool isHmxRouting = service.IsHmxRouting;

				bool anyProcessingChildDetected = anyAudioProcessingChild || anyVideoProcessingChild;
				// When any news recording and destination can be found among the hmx routing children, destination can be considered as a news one.
				bool hmxRoutingConsideredAsMcrDestination = isHmxRouting && anyNewsRecordingChild && !anyNewsDestinationChild && !anyProcessingChildDetected;

				helpers?.Log(nameof(Service), nameof(IsConsideredAsMcrDestinationInMcrView), $"This routing service {(hmxRoutingConsideredAsMcrDestination ? "does not" : String.Empty)} need to be considered as a MCR destination: {nameof(hmxRoutingConsideredAsMcrDestination)}={hmxRoutingConsideredAsMcrDestination}, {nameof(anyNewsRecordingChild)}={anyNewsRecordingChild}, {nameof(anyNewsDestinationChild)}={anyNewsDestinationChild}, {nameof(isHmxRouting)}={isHmxRouting}", service.Name);

				return hmxRoutingConsideredAsMcrDestination;
			}
			else
			{
				return false;
			}
		}

		public static bool IsUutisalueDestination(Service service)
		{
			if (service is null) throw new ArgumentNullException(nameof(service));

			if (service.Definition.VirtualPlatform != VirtualPlatform.Destination) return false;

			var yleHelsinkiDestinationLocationProfileParameter = service.Functions.SelectMany(f => f.Parameters).FirstOrDefault(p => p.Id == ProfileParameterGuids.YleHelsinkiDestinationLocation);

			if (yleHelsinkiDestinationLocationProfileParameter is null) return false;

			if (yleHelsinkiDestinationLocationProfileParameter.StringValue != "Uutisalue") return false;

			return true;
		}

		public static bool IsStudioHelsinkiDestination(Service service)
		{
			if (service is null) throw new ArgumentNullException(nameof(service));

			if (service.Definition.VirtualPlatform != VirtualPlatform.Destination) return false;

			var yleHelsinkiDestinationLocationProfileParameter = service.Functions.SelectMany(f => f.Parameters).FirstOrDefault(p => p.Id == ProfileParameterGuids.YleHelsinkiDestinationLocation);

			if (yleHelsinkiDestinationLocationProfileParameter is null) return false;

			if (!yleHelsinkiDestinationLocationProfileParameter.StringValue.Contains("Studio Helsinki")) return false;

			return true;
		}

		public static bool IsAreenaDestination(Service service)
		{
			if (service is null) throw new ArgumentNullException(nameof(service));

			if (service.Definition.VirtualPlatform != VirtualPlatform.Destination) return false;

			var yleHelsinkiDestinationLocationProfileParameter = service.Functions.SelectMany(f => f.Parameters).FirstOrDefault(p => p.Id == ProfileParameterGuids.YleHelsinkiDestinationLocation);

			if (yleHelsinkiDestinationLocationProfileParameter is null) return false;

			if (yleHelsinkiDestinationLocationProfileParameter.StringValue != "Areena") return false;

			return true;
		}

		public static bool IsMessiNewsRecordingUsingUmxTieLine(Helpers helpers, Service service, Order order)
		{
			if (helpers is null) throw new ArgumentNullException(nameof(helpers));
			if (service is null) throw new ArgumentNullException(nameof(service));
			if (order is null) throw new ArgumentNullException(nameof(order));

			if (service.Definition.VirtualPlatform != VirtualPlatform.Recording) return false;

			if (!service.Definition.Description.Contains("News")) return false;

			var liveVideoOrder = new LiveVideoOrder(helpers, order);

			var routingServiceChain = liveVideoOrder.GetRoutingServiceChainsForService(service.Id).SingleOrDefault();

			if (routingServiceChain is null) return false;

			if (!routingServiceChain.GetAllRoutingServiceResources().Exists(r => r.Name.Contains("UMX"))) return false;

			return true;
		}

		public static bool IsEvsRecording(Helpers helpers, Service service)
		{
			if (helpers is null) throw new ArgumentNullException(nameof(helpers));
			if (service is null) throw new ArgumentNullException(nameof(service));

			if (service.Definition.VirtualPlatform != VirtualPlatform.Recording) return false;

			if (!service.Definition.Description.Contains("Live")) return false;

			return true;
		}

		public static bool IsMessiLiveRecording(Helpers helpers, Service service)
		{
			return IsEvsRecording(helpers, service);
		}
	}
}
