namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	/// <summary>
	/// Represents a service of type Video Processing.
	/// </summary>
	public class VideoProcessingService : RoutingRequiringService
	{
		public VideoProcessingService(Helpers helpers, Service service, LiveVideoOrder liveVideoOrder)
			: base(helpers, service, liveVideoOrder)
		{
		}

		public VideoProcessingService(Helpers helpers, Service service, string inputVideoFormat, string outputVideoFormat, LiveVideoOrder liveVideoOrder)
			: base(helpers, service, liveVideoOrder)
		{
			InputVideoFormat = inputVideoFormat;
			OutputVideoFormat = outputVideoFormat;
		}

		/// <summary>
		/// The input video format of this Video Processing service.
		/// </summary>
		public string InputVideoFormat 
		{
			set => GetAllServiceProfileParameters().Single(p => p.Id == ProfileParameterGuids.InputVideoFormat).Value = value;
			get => GetAllServiceProfileParameters().Single(p => p.Id == ProfileParameterGuids.InputVideoFormat).StringValue;
		}

		/// <summary>
		/// The output video format of this Video Processing service.
		/// </summary>
		public string OutputVideoFormat 
		{
			get => GetAllServiceProfileParameters().Single(p => p.Id == ProfileParameterGuids.OutputVideoFormat).StringValue;
			set => GetAllServiceProfileParameters().Single(p => p.Id == ProfileParameterGuids.OutputVideoFormat).Value = value;
		}

		/// <summary>
		/// Generate a new Video Processing service.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="parent">The parent of this Video Processing service.</param>
		/// <param name="child">The child of this Video Processing service.</param>
		/// <returns></returns>
		public static VideoProcessingService GenerateNewVideoProcessingService(Helpers helpers, ProcessingRelatedService parent, IVideoProcessingRequiringService child)
		{
			var serviceDefinition = helpers.ServiceDefinitionManager.VideoProcessingServiceDefinition ?? throw new ArgumentException("SD Manager cannot find GFX Proc SD", nameof(helpers));

			var service = new DisplayedService
			{
				Start = child.Service.Start,
				End = child.Service.End,
				PreRoll = GetPreRoll(serviceDefinition, parent.Service),
				PostRoll = ServiceManager.GetPostRollDuration(serviceDefinition),
				Definition = serviceDefinition,
				BackupType = parent.Service.BackupType
			};

			var functionNode = serviceDefinition.Diagram.Nodes.Single();
			var functionDefinition = serviceDefinition.FunctionDefinitions.Single();

			var function = new Function.DisplayedFunction(helpers, functionNode, functionDefinition);

			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputVideoFormat).Value = parent.VideoFormat;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputVideoFormat).Value = child.VideoFormat;

			service.Functions.Add(function);

			helpers.Log(nameof(VideoProcessingService), nameof(GenerateNewVideoProcessingService), $"Generated new video processing service: '{service.GetConfiguration().Serialize()}'");

			var videoProcessingService = new VideoProcessingService(helpers, service, parent.VideoFormat, child.VideoFormat, parent.LiveVideoOrder) { IsNew = true };

			videoProcessingService.Service.AcceptChanges();

			return videoProcessingService;
		}

		public void UpdateValuesBasedOn(ProcessingRelatedService parent, ProcessingRelatedService child)
		{
			UpdateFunctionsBasedOn(parent, child);

			if (!LiveVideoOrder.Order.ConvertedFromRunningToStartNow)
			{
				Service.Start = child.Service.Start;
				Service.End = child.Service.End;
			}
		}

		public void UpdateFunctionsBasedOn(ProcessingRelatedService parent, ProcessingRelatedService child)
		{
			InputVideoFormat = parent.VideoFormat;
			OutputVideoFormat = child.VideoFormat;
		}

		/// <summary>
		/// Indicates if this Video Processing service is valid for the given parent and child.
		/// </summary>
		/// <param name="parent">The parent service.</param>
		/// <param name="child">The child service.</param>
		/// <returns></returns>
		public bool IsValid(ProcessingRelatedService parent, IVideoProcessingRequiringService child)
		{
			var valid = parent.VideoFormat == InputVideoFormat && child.VideoFormat == OutputVideoFormat;
			if (!valid)
			{
				return valid;
			}

			var graphicsProcessingParent = parent as GraphicsProcessingService;
			var destinationChild = child as DestinationService;
			if (graphicsProcessingParent != null && destinationChild != null && destinationChild.GraphicsProcessingService != null)
			{
				valid = graphicsProcessingParent.Equals(destinationChild.GraphicsProcessingService);
			}

			if (Service.Start > child.Service.Start || Service.End < child.Service.End)
			{
				return false;
			}

			return valid;
		}

		public override bool ProcessesSignalSameAs(RoutingRequiringService service)
		{
			if (service is VideoProcessingService other)
			{
				Log(nameof(ProcessesSignalSameAs), $"This service has {nameof(InputVideoFormat)}='{InputVideoFormat}' and {nameof(OutputVideoFormat)}='{OutputVideoFormat}'. Second service {other.Service.Name} has {nameof(InputVideoFormat)}='{other.InputVideoFormat}' and {nameof(OutputVideoFormat)}='{other.OutputVideoFormat}'");

				bool inputVideoFormatMatches = InputVideoFormat == other.InputVideoFormat;
				bool outputVideoFormatMatches = OutputVideoFormat == other.OutputVideoFormat;

				return inputVideoFormatMatches && outputVideoFormatMatches;
			}
			else
			{
				Log(nameof(ProcessesSignalSameAs), $"Service {service.Service.Name} is not a {nameof(VideoProcessingService)}");
				return false;
			}
		}
	}
}