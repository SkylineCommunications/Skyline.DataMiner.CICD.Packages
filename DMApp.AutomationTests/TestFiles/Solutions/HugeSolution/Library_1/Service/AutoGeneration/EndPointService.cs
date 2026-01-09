namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices.WindowsRuntime;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using SLDataGateway.API.Types.MessageHandlers;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public abstract class EndPointService : RoutingRequiringService, IVideoProcessingRequiringService
	{
		private GraphicsProcessingService graphicsProcessingService;
		private VideoProcessingService videoProcessingService;
		private AudioProcessingService audioProcessingService;

		protected EndPointService(Helpers helpers, Service service, LiveVideoOrder liveVideoOrder) : base(helpers, service, liveVideoOrder)
		{
		}

		/// <summary>
		/// Reference to the Graphics Processing service used for this Destination.
		/// </summary>
		public GraphicsProcessingService GraphicsProcessingService
		{
			get => graphicsProcessingService;

			set
			{
				if (value == null && graphicsProcessingService == null) return;
				if (value != null && graphicsProcessingService != null && graphicsProcessingService.Equals(value)) return;

				graphicsProcessingService = value;

				Log($"{nameof(GraphicsProcessingService)}.Set", $"Set '{graphicsProcessingService?.Service?.Name}' as Graphics Processing service of {Service?.Name}");
			}
		}

		/// <summary>
		/// Indicates if the current Graphics Processing configuration is valid.
		/// </summary>
		public bool HasValidGraphicsProcessingConfiguration
		{
			get
			{
				if (IsGraphicsProcessingRequired)
				{
					if (GraphicsProcessingService == null) return false;

					return HasValidGraphicsProcessingService;
				}
				else
				{
					return GraphicsProcessingService == null;
				}
			}
		}

		public override bool ProcessesSignalSameAs(RoutingRequiringService service)
		{
			return Equals(service);
		}

		/// <summary>
		/// Indicates if Graphics Processing is required.
		/// </summary>
		private bool IsGraphicsProcessingRequired => RemoteGraphics != null && RemoteGraphics != "None";

		/// <summary>
		/// Indicates if the current Graphics Processing service is valid.
		/// </summary>
		private bool HasValidGraphicsProcessingService => GraphicsProcessingService.RemoteGraphics == RemoteGraphics && GraphicsProcessingService.Service.Start.ToUniversalTime() <= Service.Start.ToUniversalTime() && GraphicsProcessingService.Service.End.ToUniversalTime() >= Service.End.ToUniversalTime();


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
			if (serviceToCompare is null) throw new ArgumentNullException(nameof(serviceToCompare));

			bool isRecordingOrTransmission = Service.Definition.VirtualPlatform == VirtualPlatform.Recording || Service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Transmission;

			bool isRecordingOrTransmissionOfDestination = isRecordingOrTransmission &&
											Service.IntegrationType == IntegrationType.None &&
											Service.NameOfServiceToTransmitOrRecord != null &&
											Service.NameOfServiceToTransmitOrRecord.Contains(VirtualPlatformType.Destination.GetDescription());

			if (isRecordingOrTransmissionOfDestination)
			{
				Log(nameof(IsVideoProcessingRequired), $"Service is a recording/transmission of a destination, no video processing required.");

				return false; // Recording or transmission of destination uses same video processing service as destination
			}

			bool containsSupportedVideoFormat = VideoFormatProfileParameter?.Discreets.Any(d => d.InternalValue.Equals(serviceToCompare.VideoFormat)) ?? false;
			if (Service.Definition.Id == ServiceDefinitionGuids.AreenaDestinationServiceDefinitionId && containsSupportedVideoFormat)
			{
				Log(nameof(IsVideoProcessingRequired), $"Service is an Areena Destination which supports the following video format: {serviceToCompare.VideoFormat}, no video processing required.");

				return false;
			}

			bool requiresVideoProcessing = serviceToCompare.VideoFormat != null && VideoFormat != null && serviceToCompare.VideoFormat != VideoFormat;

			Log(nameof(IsVideoProcessingRequired), $"Service has video format {VideoFormat}. Parent {serviceToCompare.Service.Name} has video format {serviceToCompare.VideoFormat}. Video processing {(requiresVideoProcessing ? string.Empty : "not ")}required");

			return requiresVideoProcessing;
		}

		/// <summary>
		/// The Audio Processing service used by this Live Video service.
		/// </summary>
		public AudioProcessingService AudioProcessingService
		{
			get => audioProcessingService;

			set
			{
				if (value == null && audioProcessingService == null) return;
				if (value != null && audioProcessingService != null && audioProcessingService.Equals(value)) return;

				audioProcessingService = value;

				Log($"{nameof(AudioProcessingService)}.Set", $"Set '{audioProcessingService?.Service?.Name}' as Audio Processing service of {Service?.Name}");
			}
		}

		/// <summary>
		/// Indicates if Audio Processing is required.
		/// </summary>
		/// <returns>True in case audio processing is required.</returns>
		public bool IsAudioProcessingRequired()
		{
			var source = Source ?? throw new ServiceNotFoundException($"Unable to find source for {Service.Name}", true);

			bool isAudioConfigurationCopiedFromSource = Service.IsAudioConfigurationCopiedFromSource;

			bool isManualRecording = Service.Definition.VirtualPlatform == VirtualPlatform.Recording && Service.IntegrationType == IntegrationType.None;
			bool isManualRecordingOfDestination = isManualRecording &&
											Service.RecordingConfiguration != null &&
											!string.IsNullOrWhiteSpace(Service.RecordingConfiguration.NameOfServiceToRecord) &&
											Service.RecordingConfiguration.NameOfServiceToRecord.Contains(VirtualPlatformType.Destination.GetDescription());

			if (source.Service.IntegrationType == IntegrationType.Eurovision && isAudioConfigurationCopiedFromSource && !source.AudioDolbyDecodingRequired)
			{
				return false;
			}

			if (isManualRecordingOfDestination)
			{
				Log(nameof(IsAudioProcessingRequired), $"Is manual recording of a destination, no audio processing required.");
				return false; // Recording of destination uses same audio processing service as destination
			}

			if (AudioEmbeddingRequired || AudioDeembeddingRequired || AudioShufflingRequired || source.AudioDolbyDecodingRequired)
			{
				return true;
			}

			// Check if shuffling is required even if set to false
			bool isShufflingRequired = IsShufflingRequired(source, isAudioConfigurationCopiedFromSource);
			if (isShufflingRequired)
			{
				AudioShufflingRequired = true;
			}

			return isShufflingRequired;
		}

		private bool IsShufflingRequired(SourceService source, bool copiedFromSource)
		{
			if (copiedFromSource) return false;
			if (AreFirstAudioChannelsDifferent(source)) return true;
			if (AreLastAudioChannelsDifferent(source)) return true;
			return false;
		}

		private bool AreFirstAudioChannelsDifferent(SourceService source)
		{
			if (AudioChannelIsDifferent(source.AudioChannel1, AudioChannel1)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel2, AudioChannel2)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel3, AudioChannel3)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel4, AudioChannel4)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel5, AudioChannel5)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel6, AudioChannel6)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel7, AudioChannel7)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel8, AudioChannel8)) return true;
			return false;
		}

		private bool AreLastAudioChannelsDifferent(SourceService source)
		{
			if (AudioChannelIsDifferent(source.AudioChannel9, AudioChannel9)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel10, AudioChannel10)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel11, AudioChannel11)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel12, AudioChannel12)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel13, AudioChannel13)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel14, AudioChannel14)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel15, AudioChannel15)) return true;
			if (AudioChannelIsDifferent(source.AudioChannel16, AudioChannel16)) return true;
			return false;
		}

		private static bool AudioChannelIsDifferent(string sourceAudioChannel, string currentAudioChannel)
		{
			return sourceAudioChannel != currentAudioChannel && currentAudioChannel != null && currentAudioChannel != "None";
		}

		/// <summary>
		/// Add or update the video processing configuration.
		/// </summary>
		/// <param name="parentToCompare">The parent service used to check if video processing is required.</param>
		/// <param name="reusableVideoProcessingServices">A list of unused existing Video Processing services that are not being used by any other EndPoint or Graphics Processing services.</param>
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

		/// <summary>
		/// Add or update the graphics processing configuration.
		/// </summary>
		public void AddOrUpdateGraphicsProcessingConfiguration(List<GraphicsProcessingService> unusedExistingGraphicsProcessingServices = null)
		{
			LogMethodStart(nameof(AddOrUpdateGraphicsProcessingConfiguration), out var stopwatch);

			Log(nameof(AddOrUpdateGraphicsProcessingConfiguration), $"Current graphics processing service '{GraphicsProcessingService?.Service?.Name}' is {(HasValidGraphicsProcessingConfiguration ? "valid, no need to update" : "invalid, update required")} ");

			if (HasValidGraphicsProcessingConfiguration)
			{
				LogMethodCompleted(nameof(AddOrUpdateGraphicsProcessingConfiguration), stopwatch);
				return; // if the current graphics processing configuration is valid then nothing needs to be updated			
			}

			if (IsGraphicsProcessingRequired)
			{
				Log(nameof(AddOrUpdateGraphicsProcessingConfiguration), $"Graphics processing required");

				unusedExistingGraphicsProcessingServices = unusedExistingGraphicsProcessingServices ?? new List<GraphicsProcessingService>();

				bool shouldReuseExistingGraphicsProcessingServices = (GraphicsProcessingService is null || GraphicsProcessingService.IsNew) && unusedExistingGraphicsProcessingServices.Any();

				var graphicsProcessingServiceToUpdate = shouldReuseExistingGraphicsProcessingServices ? unusedExistingGraphicsProcessingServices[0] : GraphicsProcessingService;

				if (graphicsProcessingServiceToUpdate is null)
				{
					AddGraphicsProcessing();
				}
				else
				{
					UpdateGraphicsProcessing(graphicsProcessingServiceToUpdate);
				}
			}
			else if (GraphicsProcessingService != null)
			{
				Log(nameof(AddOrUpdateGraphicsProcessingConfiguration), $"No graphics processing required, but graphics processing {GraphicsProcessingService.Service.Name} is present");

				RemoveGraphicsProcessing();
			}
			else
			{
				Log(nameof(AddOrUpdateGraphicsProcessingConfiguration), $"No graphics processing required");
			}

			LogMethodCompleted(nameof(AddOrUpdateGraphicsProcessingConfiguration), stopwatch);
		}

		private void AddGraphicsProcessing()
		{
			GraphicsProcessingService = GraphicsProcessingService.GenerateNewGraphicsProcessingService(Helpers, LiveVideoOrder.GetSource(this), this, LiveVideoOrder);

			LiveVideoOrder.LiveVideoServices.Add(GraphicsProcessingService);

			Log(nameof(AddGraphicsProcessing), $"Created new graphics processing service {GraphicsProcessingService.Service?.Name}");

			var desiredGraphicsProcessingParent = Source;
			Log(nameof(AddGraphicsProcessing), $"Desired graphics processing parent is {desiredGraphicsProcessingParent.Service?.Name}");

			var desiredGraphicsProcessingChild = VideoProcessingService?.GetOldestRoutingParentServiceOrThis() ?? AudioProcessingService?.GetOldestRoutingParentServiceOrThis() ?? GetOldestRoutingParentServiceOrThis();
			Log(nameof(AddGraphicsProcessing), $"Desired graphics processing child is {desiredGraphicsProcessingChild.Service?.Name}");

			LiveVideoOrder.InsertServiceBetween(GraphicsProcessingService, desiredGraphicsProcessingParent, desiredGraphicsProcessingChild);
			// Note: the order of processing services is 1.Graphics 2.Video 3.Audio.
			// Graphics Processing should therefore be added between the source and the (routing for) oldest processing or this

		}

		private void UpdateGraphicsProcessing(GraphicsProcessingService graphicsProcessingServiceToUpdate)
		{
			GraphicsProcessingService = graphicsProcessingServiceToUpdate ?? throw new ArgumentNullException(nameof(graphicsProcessingServiceToUpdate));

			Log(nameof(UpdateGraphicsProcessing), $"Updating graphics processing service {GraphicsProcessingService.Service.Name}");

			GraphicsProcessingService.UpdateValuesBasedOn(this);

			var desiredGraphicsProcessingParent = GraphicsProcessingService.GetYoungestNonRoutingParent();
			Log(nameof(UpdateGraphicsProcessing), $"Desired graphics processing parent is {desiredGraphicsProcessingParent.Service?.Name}");

			var desiredGraphicsProcessingChild = VideoProcessingService?.GetOldestRoutingParentServiceOrThis() ?? AudioProcessingService?.GetOldestRoutingParentServiceOrThis() ?? GetOldestRoutingParentServiceOrThis();
			Log(nameof(UpdateGraphicsProcessing), $"Desired graphics processing child is {desiredGraphicsProcessingChild.Service?.Name}");

			var routingParents = LiveVideoOrder.GetRoutingParents(GraphicsProcessingService);
			if (routingParents.Count > 1)
			{
				routingParents.Reverse(); // Reverse order again to ensure the correct node id order is used.
				var servicesToInsert = new List<LiveVideoService>(routingParents) { GraphicsProcessingService };
				LiveVideoOrder.InsertServicesBetween(servicesToInsert, desiredGraphicsProcessingParent, desiredGraphicsProcessingChild);
			}
			if (GraphicsProcessingService.Parent is RoutingService routingParent)
			{
				LiveVideoOrder.InsertServicesBetween(new List<LiveVideoService> { routingParent, GraphicsProcessingService }, desiredGraphicsProcessingParent, desiredGraphicsProcessingChild);
			}
			else
			{
				LiveVideoOrder.InsertServiceBetween(AudioProcessingService, desiredGraphicsProcessingParent, desiredGraphicsProcessingChild);
				// Note: the order of processing services is 1.Graphics 2.Video 3.Audio.
			}
		}

		private void RemoveGraphicsProcessing()
		{
			var childOfGraphicsProcessingPartOfChainOfThisEndPointService = GraphicsProcessingService.Children.Single(s => LiveVideoOrder.ServicesArePartOfSameChain(s, this));

			var nonRoutingParentOfGraphicsProcessing = GraphicsProcessingService.GetYoungestNonRoutingParent();

			LiveVideoOrder.SetParent(childOfGraphicsProcessingPartOfChainOfThisEndPointService, nonRoutingParentOfGraphicsProcessing);

			GraphicsProcessingService = null;
		}

		/// <summary>
		/// Indicates if the Graphics Processing service is valid for this Destination.
		/// </summary>
		/// <param name="service">The Graphics Proccessing service to verify.</param>
		/// <returns>True if the Graphics Processing service is valid.</returns>
		public bool IsValidGraphicsProcessingService(GraphicsProcessingService service)
		{
			return service.RemoteGraphics == RemoteGraphics;
		}

		/// <summary>
		/// Add or update the audio processing configuration.
		/// </summary>
		public void AddOrUpdateAudioProcessingConfiguration(List<AudioProcessingService> unusedExistingAudioProcessingServices = null)
		{
			LogMethodStart(nameof(AddOrUpdateAudioProcessingConfiguration), out var stopwatch, Service.Name);

			if (IsAudioProcessingRequired())
			{
				Log(nameof(AddOrUpdateAudioProcessingConfiguration), $"Audio processing required");

				unusedExistingAudioProcessingServices = unusedExistingAudioProcessingServices ?? new List<AudioProcessingService>();

				bool shouldReuseExistingAudioProcessingServices = (AudioProcessingService is null || AudioProcessingService.IsNew) && unusedExistingAudioProcessingServices.Any();

				var audioProcessingServiceToUpdate = shouldReuseExistingAudioProcessingServices ? unusedExistingAudioProcessingServices[0] : AudioProcessingService;

				if (audioProcessingServiceToUpdate is null)
				{
					AddAudioProcessing();
				}
				else
				{
					UpdateAudioProcessing(audioProcessingServiceToUpdate);
				}
			}
			else if (AudioProcessingService != null)
			{
				Log(nameof(AddOrUpdateAudioProcessingConfiguration), $"No audio processing required, but audio processing {AudioProcessingService.Service.Name} is present");

				RemoveAudioProcessing();
			}
			else
			{
				Log(nameof(AddOrUpdateAudioProcessingConfiguration), $"No audio processing required");
			}

			LogMethodCompleted(nameof(AddOrUpdateAudioProcessingConfiguration), stopwatch);
		}

		private void RemoveAudioProcessing()
		{
			var childOfAudioProcessingPartOfChainOfThisEndPointService = AudioProcessingService.Children.Single(s => LiveVideoOrder.ServicesArePartOfSameChain(s, this));

			var nonRoutingParentOfAudioProcessing = AudioProcessingService.GetYoungestNonRoutingParent();

			LiveVideoOrder.SetParent(childOfAudioProcessingPartOfChainOfThisEndPointService, nonRoutingParentOfAudioProcessing);

			AudioProcessingService = null;
		}

		internal void UpdateEmbeddingBasedOnLinkedAudioProcessingService()
		{
			if (AudioProcessingService == null) return;

			var audioProcessingAudioEmbeddingFunction = AudioProcessingService.AudioEmbeddingFunction;

			bool audioEmbeddingRequired = audioProcessingAudioEmbeddingFunction?.Resource != null && audioProcessingAudioEmbeddingFunction.Name != Constants.None;
			Helpers?.Log(nameof(EndPointService), nameof(UpdateEmbeddingBasedOnLinkedAudioProcessingService), $"Audio embedding required value will be set to: {audioEmbeddingRequired}");

			AudioEmbeddingRequired = audioEmbeddingRequired;
		}

		internal void UpdateDeEmbeddingBasedOnLinkedAudioProcessingService()
		{
			if (AudioProcessingService == null) return;

			var audioProcessingAudioDeEmbeddingFunction = AudioProcessingService.AudioDeEmbeddingFunction;

			bool audioDeEmbeddingRequired = audioProcessingAudioDeEmbeddingFunction?.Resource != null && audioProcessingAudioDeEmbeddingFunction.Name != Constants.None;
			Helpers?.Log(nameof(EndPointService), nameof(UpdateDeEmbeddingBasedOnLinkedAudioProcessingService), $"Audio deembedding required value will be set to: {audioDeEmbeddingRequired}");

			AudioDeembeddingRequired = audioDeEmbeddingRequired;

		}

		internal void UpdateAudioShufflingBasedOnLinkedAudioProcessingService()
		{
			if (AudioProcessingService == null) return;

			var audioProcessingAudioShufflingFunction = AudioProcessingService.AudioShufflingFunction;

			bool audioShufflingRequired = audioProcessingAudioShufflingFunction?.Resource != null && audioProcessingAudioShufflingFunction.Name != Constants.None;
			Helpers?.Log(nameof(EndPointService), nameof(UpdateAudioShufflingBasedOnLinkedAudioProcessingService), $"Audio shuffling required value will be set to: {audioShufflingRequired}");

			AudioShufflingRequired = audioShufflingRequired;
		}

		private void UpdateAudioProcessing(AudioProcessingService audioProcessingServiceToUpdate)
		{
			AudioProcessingService = audioProcessingServiceToUpdate ?? throw new ArgumentNullException(nameof(audioProcessingServiceToUpdate));

			Log(nameof(UpdateAudioProcessing), $"Updating audio processing service {AudioProcessingService.Service.Name}");

			AudioProcessingService.UpdateValuesBasedOn(this);

			var desiredAudioProcessingParent = VideoProcessingService ?? (LiveVideoService)GraphicsProcessingService ?? Source;
			Log(nameof(UpdateAudioProcessing), $"Desired audio processing parent is {desiredAudioProcessingParent.Service?.Name}");

			var desiredAudioProcessingChild = GetOldestRoutingParentServiceOrThis(considerSharedRoutingServices: false);
			Log(nameof(UpdateAudioProcessing), $"Desired audio processing child is {desiredAudioProcessingChild.Service?.Name}");

			var routingParents = LiveVideoOrder.GetRoutingParents(AudioProcessingService);
			if (routingParents.Count > 1)
			{
				routingParents.Reverse(); // Reverse order again to ensure the correct node id order is used.
				var servicesToInsert = new List<LiveVideoService>(routingParents) { AudioProcessingService };
				LiveVideoOrder.InsertServicesBetween(servicesToInsert, desiredAudioProcessingParent, desiredAudioProcessingChild);
			}
			else if (AudioProcessingService.Parent is RoutingService routingParent)
			{
				LiveVideoOrder.InsertServicesBetween(new List<LiveVideoService> { routingParent, AudioProcessingService }, desiredAudioProcessingParent, desiredAudioProcessingChild);
			}
			else
			{
				LiveVideoOrder.InsertServiceBetween(AudioProcessingService, desiredAudioProcessingParent, desiredAudioProcessingChild);
				// Note: the order of processing services is 1.Graphics 2.Video 3.Audio.
				// Audio Processing should therefore be added between the youngest processing or source (= youngest non-routing parent) and (the oldest routing for) this
			}
		}

		private void AddAudioProcessing()
		{
			AudioProcessingService = AudioProcessingService.GenerateNewAudioProcessingService(Helpers, Source, this);

			LiveVideoOrder.LiveVideoServices.Add(AudioProcessingService);

			Log(nameof(AddAudioProcessing), $"Created new audio processing service {AudioProcessingService.Service?.Name}");

			var desiredAudioProcessingParent = GetYoungestNonRoutingParent();
			Log(nameof(AddAudioProcessing), $"Desired audio processing parent is {desiredAudioProcessingParent.Service?.Name}");

			var desiredAudioProcessingChild = GetOldestRoutingParentServiceOrThis(considerSharedRoutingServices: false);
			Log(nameof(AddAudioProcessing), $"Desired audio processing child is {desiredAudioProcessingChild.Service?.Name}");

			LiveVideoOrder.InsertServiceBetween(AudioProcessingService, desiredAudioProcessingParent, desiredAudioProcessingChild);
			// Note: the order of processing services is 1.Graphics 2.Video 3.Audio.
			// Audio Processing should therefore be added between the youngest processing or source (= youngest non-routing parent) and (the oldest routing for) this

		}

		private void UpdateVideoProcessing(VideoProcessingService videoProcessingServiceToUpdate, ProcessingRelatedService parentToCompare)
		{
			VideoProcessingService = videoProcessingServiceToUpdate ?? throw new ArgumentNullException(nameof(videoProcessingServiceToUpdate));

			Log(nameof(UpdateVideoProcessing), $"Updating video processing service {VideoProcessingService.Service.Name}");

			VideoProcessingService.UpdateValuesBasedOn(parentToCompare, this);

			var desiredVideoProcessingParent = parentToCompare;
			Log(nameof(UpdateVideoProcessing), $"Desired video processing parent is {desiredVideoProcessingParent.Service?.Name}");

			var desiredVideoProcessingChild = AudioProcessingService?.GetOldestRoutingParentServiceOrThis() ?? GetOldestRoutingParentServiceOrThis();
			Log(nameof(UpdateVideoProcessing), $"Desired video processing child is {desiredVideoProcessingChild.Service?.Name}");

			var routingParents = LiveVideoOrder.GetRoutingParents(VideoProcessingService);
			if (routingParents.Count > 1)
			{
				routingParents.Reverse(); // Reverse order again to ensure the correct node id order is used.
				var servicesToInsert = new List<LiveVideoService>(routingParents) { VideoProcessingService };
				LiveVideoOrder.InsertServicesBetween(servicesToInsert, desiredVideoProcessingParent, desiredVideoProcessingChild);
			}
			else if (VideoProcessingService.Parent is RoutingService routingParent)
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

			var desiredVideoProcessingChild = AudioProcessingService?.GetOldestRoutingParentServiceOrThis() ?? GetOldestRoutingParentServiceOrThis();
			Log(nameof(AddVideoProcessing), $"Desired video processing child is {desiredVideoProcessingChild.Service?.Name}");

			LiveVideoOrder.InsertServiceBetween(VideoProcessingService, desiredVideoProcessingParent, desiredVideoProcessingChild);
			// Note: the order of processing services is 1.Graphics 2.Video 3.Audio.
			// Video Processing should therefore be added between its parent to compare and the (routing for) oldest processing before that parent to compare or this
		}

		public override void LogHierarchy()
		{
			base.LogHierarchy();
			Log(nameof(LogHierarchy), $"Service Audio Processing Service: {((AudioProcessingService?.Service != null) ? AudioProcessingService.Service.Name : "None")}");
			Log(nameof(LogHierarchy), $"Service Video Processing Service: {((VideoProcessingService?.Service != null) ? VideoProcessingService.Service.Name : "None")}");
			Log(nameof(LogHierarchy), $"Service Graphics Processing Service: {((GraphicsProcessingService?.Service != null) ? GraphicsProcessingService.Service.Name : "None")}");
		}
	}
}
