using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export
{
	public class MediaParkkiExport
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public MediaParkkiExport()
		{
			SourceFolder = default(string);
			SourceFolderUrls = default(string[]);
			SourceFileUrls = default(string[]);
			TargetVideoFormat = default(TargetVideoFormats?);
			ExportFileTypes = default(MediaParkkiExportFileTypes);
			ExportTargetVideoFormat = default(string);
			OtherHiresTargetVideoFormatInfo = default(string);
			OtherVideoFormatInfo = default(string);
			VideoViewingQuality = default(VideoViewingQualities?);
            OtherTargetVideoFormatFileAttachmentInfo = new FileAttachmentInfo();
		}

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string SourceFolder { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] SourceFolderUrls { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] SourceFileUrls { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public TargetVideoFormats? TargetVideoFormat { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string OtherVideoFormatInfo { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public MediaParkkiExportFileTypes? ExportFileTypes { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ExportTargetVideoFormat { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string OtherHiresTargetVideoFormatInfo { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public VideoViewingQualities? VideoViewingQuality { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FileAttachmentInfo OtherTargetVideoFormatFileAttachmentInfo { get; set; }
    }
}