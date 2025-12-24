namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System.Collections.Generic;

	public interface IVideoProcessingRequiringService
	{
		Service Service { get; }

		string VideoFormat { get; set; }

		/// <summary>
		/// The Video Processing service used by this Live Video service.
		/// </summary>
		VideoProcessingService VideoProcessingService { get; set; }

		/// <summary>
		/// Indicates if Video Processing is required.
		/// </summary>
		/// <param name="serviceToCompare">The service to verify with.</param>
		/// <returns></returns>
		bool IsVideoProcessingRequired(ProcessingRelatedService serviceToCompare);

		bool VideoProcessingUpdateRequired(ProcessingRelatedService parent, bool videoProcessingRequired);

		/// <summary>
		/// Add or update the video processing configuration.
		/// </summary>
		/// <param name="parentToCompare">The parent service used to check if video processing is required.</param>
		/// <param name="reusableVideoProcessingServices"></param>
		void AddOrUpdateVideoProcessingConfiguration(ProcessingRelatedService parentToCompare, List<VideoProcessingService> reusableVideoProcessingServices = null);
	}
}
