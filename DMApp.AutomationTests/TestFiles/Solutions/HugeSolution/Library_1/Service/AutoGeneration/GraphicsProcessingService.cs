namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Profiles;
    using Skyline.DataMiner.Utils.YLE.Integrations;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	/// <summary>
	/// Represents a service of type Graphics Processing.
	/// </summary>
	public class GraphicsProcessingService : RoutingRequiringService, IVideoProcessingRequiringService
	{
		private VideoProcessingService videoProcessingService;

		public GraphicsProcessingService(Helpers helpers, Service service, LiveVideoOrder liveVideoOrder)
			: base(helpers, service, liveVideoOrder)
		{
		}

		public GraphicsProcessingService(Helpers helpers, Service service, string graphicsProcessingEngine, LiveVideoOrder liveVideoOrder)
			: base(helpers, service, liveVideoOrder)
		{
			RemoteGraphics = graphicsProcessingEngine;
		}

		/// <summary>
		/// The Video Processing service used by this Live Video service.
		/// </summary>
		public VideoProcessingService VideoProcessingService
		{
			get => videoProcessingService;

			set
			{
				if (value == null && videoProcessingService == null) return;
				if (value != null && videoProcessingService != null && videoProcessingService.Equals(value)) return;

				videoProcessingService = value;

				Log($"{nameof(VideoProcessingService)}.Set", $"Set '{videoProcessingService?.Service?.Name}' as Video Processing service of {Service?.Name}");
			}
		}

		/// <summary>
		/// Indicates if Video Processing is required.
		/// </summary>
		/// <param name="serviceToCompare">The service to verify with.</param>
		/// <returns></returns>
		public bool IsVideoProcessingRequired(ProcessingRelatedService serviceToCompare)
		{
			bool isRecordingOfDestination = Service.Definition.VirtualPlatform == VirtualPlatform.Recording &&
											Service.IntegrationType == IntegrationType.None &&
											Service.RecordingConfiguration != null &&
											Service.RecordingConfiguration.NameOfServiceToRecord.Contains(EnumExtensions.GetDescriptionFromEnumValue(VirtualPlatformType.Destination));

			bool isTransmissionOfDestination = Service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Transmission &&
											   Service.IntegrationType == IntegrationType.None &&
											   Service.NameOfServiceToTransmitOrRecord != null &&
											   Service.NameOfServiceToTransmitOrRecord.Contains(VirtualPlatformType.Destination.GetDescription());

			if (isRecordingOfDestination || isTransmissionOfDestination)
			{
				Log(nameof(IsVideoProcessingRequired), $"Service is a recording/transmission of a destination, no video processing required.");

				return false; // Recording or transmission of destination uses same video processing service as destination
			}

			bool requiresVideoProcessing = serviceToCompare.VideoFormat != null && VideoFormat != null && serviceToCompare.VideoFormat != VideoFormat;

			Log(nameof(IsVideoProcessingRequired), $"Service has video format {VideoFormat}. Parent {serviceToCompare.Service.Name} has video format {serviceToCompare.VideoFormat}. Video processing {(requiresVideoProcessing ? string.Empty : "not ")}required");

			return requiresVideoProcessing;
		}

		/// <summary>
		/// Add or update the video processing configuration.
		/// </summary>
		/// <param name="parentToCompare">The parent service used to check if video processing is required.</param>
		/// <param name="reusableVideoProcessingServices">A list of unused existing Video Processing services that are not being used by any other EndPoint or Graphics Processing service</param>
		public void AddOrUpdateVideoProcessingConfiguration(ProcessingRelatedService parentToCompare, List<VideoProcessingService> reusableVideoProcessingServices = null)
		{
			LogMethodStart(nameof(AddOrUpdateVideoProcessingConfiguration), out var stopwatch);

			reusableVideoProcessingServices = reusableVideoProcessingServices ?? new List<VideoProcessingService>();

			var videoProcessingRequired = IsVideoProcessingRequired(parentToCompare);

			bool shouldReuseExistingVideoProcessingServices = (VideoProcessingService is null || VideoProcessingService.IsNew) && reusableVideoProcessingServices.Any();

			var videoProcessingUpdateRequired = VideoProcessingUpdateRequired(parentToCompare, videoProcessingRequired) || shouldReuseExistingVideoProcessingServices;

			if (!videoProcessingUpdateRequired)
			{
				LogMethodCompleted(nameof(AddOrUpdateVideoProcessingConfiguration), stopwatch);
				return;
			}

			if (videoProcessingRequired)
			{
				Log(nameof(AddOrUpdateVideoProcessingConfiguration), $"Video processing required");

				var videoProcessingServiceToUpdate = shouldReuseExistingVideoProcessingServices ? reusableVideoProcessingServices[0] : VideoProcessingService;

				if (videoProcessingServiceToUpdate is null)
				{
					AddVideoProcessing(parentToCompare);
				}
				else
				{
					UpdateVideoProcessing(videoProcessingServiceToUpdate, parentToCompare);
				}
			}
			else if (VideoProcessingService != null)
			{
				Log(nameof(AddOrUpdateVideoProcessingConfiguration), $"No video processing required, but video processing {VideoProcessingService.Service.Name} is present");
				
				RemoveVideoProcessing();
			}
			else
			{
				Log(nameof(AddOrUpdateVideoProcessingConfiguration), $"No video processing required");
			}

			LogMethodCompleted(nameof(AddOrUpdateVideoProcessingConfiguration), stopwatch);
		}

		private void UpdateVideoProcessing(VideoProcessingService videoProcessingServiceToUpdate, ProcessingRelatedService parentToCompare)
		{
			VideoProcessingService = videoProcessingServiceToUpdate ?? throw new ArgumentNullException(nameof(videoProcessingServiceToUpdate));

			VideoProcessingService.UpdateValuesBasedOn(parentToCompare, this);

			var desiredVideoProcessingParent = parentToCompare;
			Log(nameof(UpdateVideoProcessing), $"Desired video processing parent is {desiredVideoProcessingParent.Service?.Name}");

			var desiredVideoProcessingChild = GetOldestRoutingParentServiceOrThis();
			Log(nameof(UpdateVideoProcessing), $"Desired video processing child is {desiredVideoProcessingChild.Service?.Name}");

			if (VideoProcessingService.Parent is RoutingService routingParent)
			{
				LiveVideoOrder.InsertServicesBetween(new List<LiveVideoService> { routingParent, VideoProcessingService }, desiredVideoProcessingParent, desiredVideoProcessingChild);
			}
			else
			{
				LiveVideoOrder.InsertServiceBetween(VideoProcessingService, desiredVideoProcessingParent, desiredVideoProcessingChild);
				// Note: the order of processing services is 1.Graphics 2.Video 3.Audio.
			}
		}

		private void AddVideoProcessing(ProcessingRelatedService parentToCompare)
		{
			VideoProcessingService = VideoProcessingService.GenerateNewVideoProcessingService(Helpers, parentToCompare, this);
			LiveVideoOrder.LiveVideoServices.Add(VideoProcessingService);

			Log(nameof(AddVideoProcessing), $"Created new video processing service {VideoProcessingService.Service?.Name}");

			var desiredVideoProcessingParent = parentToCompare;
			Log(nameof(AddVideoProcessing), $"Desired video processing parent is {desiredVideoProcessingParent.Service?.Name}");

			var desiredVideoProcessingChild = GetOldestRoutingParentServiceOrThis();
			Log(nameof(AddVideoProcessing), $"Desired video processing child is {desiredVideoProcessingChild.Service?.Name}");

			LiveVideoOrder.InsertServiceBetween(VideoProcessingService, desiredVideoProcessingParent, desiredVideoProcessingChild);
			// Note: the order of processing services is 1.Graphics 2.Video 3.Audio.
			// Video Processing should therefore be added between its parent to compare and the (routing for) oldest processing before that parent to compare or this
		}

		private void RemoveVideoProcessing()
		{
			var childOfVideoProcessingPartOfChainOfThisEndPointService = VideoProcessingService.Children.Single(s => LiveVideoOrder.ServicesArePartOfSameChain(s, this));

			var nonRoutingParentOfVideoProcessing = VideoProcessingService.GetYoungestNonRoutingParent();

			LiveVideoOrder.SetParent(childOfVideoProcessingPartOfChainOfThisEndPointService, nonRoutingParentOfVideoProcessing);

			VideoProcessingService = null;
		}

		public bool VideoProcessingUpdateRequired(ProcessingRelatedService parent, bool videoProcessingRequired)
		{
			bool videoProcessingShouldBeRemoved = !videoProcessingRequired && VideoProcessingService != null;
			bool videoProcessingShouldBeAdded = videoProcessingRequired && VideoProcessingService == null;
			bool videoProcessingShouldBeUpdated = videoProcessingRequired && VideoProcessingService != null && !VideoProcessingService.IsValid(parent, this);

			if (videoProcessingShouldBeAdded)
			{
				Log(nameof(VideoProcessingUpdateRequired), $"Video processing is required, but current video processing service is null.");
			}
			else if (videoProcessingShouldBeRemoved)
			{
				Log(nameof(VideoProcessingUpdateRequired), $"Video processing is not required, but current video processing service is not null.");
			}
			else if (videoProcessingShouldBeUpdated)
			{
				Log(nameof(VideoProcessingUpdateRequired), $"Video processing is required, but current video processing service is not valid.");
			}
			else
			{
				Log(nameof(VideoProcessingUpdateRequired), $"No video processing changes required");
			}

			return videoProcessingShouldBeAdded || videoProcessingShouldBeRemoved || videoProcessingShouldBeUpdated;
		}

		public void UpdateValuesBasedOn(EndPointService endPointService)
		{
			UpdateFunctionsBasedOn(endPointService);

			Log(nameof(UpdateValuesBasedOn), $"Updated functions based on {endPointService.Service.Name}");

			if (!LiveVideoOrder.Order.ConvertedFromRunningToStartNow)
			{
				Service.Start = endPointService.Service.Start;
				Service.End = endPointService.Service.End;
				Log(nameof(UpdateValuesBasedOn), $"Updated timing to {Service.TimingInfoToString(Service)} based on {endPointService.Service.Name}");
			}
		}

		private void UpdateFunctionsBasedOn(EndPointService destinationOrTransmission)
		{
			RemoteGraphics = destinationOrTransmission.RemoteGraphics;
		}

		/// <summary>
		/// Generate a new Graphics Processing service.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="parent">The parent of the Graphics Processing service.</param>
		/// <param name="destinationOrTransmission">The destinationOrTransmission of the Graphics Processing service.</param>
		/// <param name="liveVideoOrder"></param>
		/// <returns></returns>
		public static GraphicsProcessingService GenerateNewGraphicsProcessingService(Helpers helpers, LiveVideoService parent, EndPointService destinationOrTransmission, LiveVideoOrder liveVideoOrder)
		{
			var graphicsProcessingServiceDefinition = helpers.ServiceDefinitionManager.GraphicsProcessingServiceDefinition ?? throw new ArgumentException($"SD Manager cannot find SD", nameof(helpers));

			var service = new DisplayedService
			              {
				              Start = destinationOrTransmission.Service.Start,
				              End = destinationOrTransmission.Service.End,
							  PreRoll = GetPreRoll(graphicsProcessingServiceDefinition, destinationOrTransmission.Service),
							  PostRoll = ServiceManager.GetPostRollDuration(graphicsProcessingServiceDefinition),
				              Definition = graphicsProcessingServiceDefinition,
				              BackupType = parent.Service.BackupType
			              };


			var functionNode = graphicsProcessingServiceDefinition.Diagram.Nodes.Single();
			var functionDefinition = graphicsProcessingServiceDefinition.FunctionDefinitions.Single();

			var function = new Function.DisplayedFunction(helpers, functionNode, functionDefinition);

			function.Parameters.Single(p => p.Id == ProfileParameterGuids.VideoFormat).Value = "1080i50";
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.RemoteGraphics).Value = destinationOrTransmission.RemoteGraphics;

			service.Functions.Add(function);

			helpers.Log(nameof(GraphicsProcessingService), nameof(GenerateNewGraphicsProcessingService), $"Generated new graphics processing service: '{service.GetConfiguration().Serialize()}'");

			var graphicsProcessingService = new GraphicsProcessingService(helpers, service, destinationOrTransmission.RemoteGraphics, liveVideoOrder) { IsNew = true };

			graphicsProcessingService.Service.AcceptChanges();

			return graphicsProcessingService;
		}

		public override bool ProcessesSignalSameAs(RoutingRequiringService service)
		{
			if (service is GraphicsProcessingService other)
			{
				Log(nameof(ProcessesSignalSameAs), $"This service has {nameof(RemoteGraphics)}='{RemoteGraphics}'. Second service {other.Service.Name} has {nameof(RemoteGraphics)}='{other.RemoteGraphics}'");

				return RemoteGraphics == other.RemoteGraphics;
			}
			else
			{
				Log(nameof(ProcessesSignalSameAs), $"Service {service.Service.Name} is not a {nameof(GraphicsProcessingService)}");
				return false;
			}
		}
	}
}