using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export
{
	public class InterplayPamExport
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public InterplayPamExport()
		{
			ElementName = default(string);
			FolderName = default(string);
			FolderUrls = default(string[]);
			FileUrls = default(string[]);
			Information = default(string);
			ExportFileType = default(InterplayPamExportFileTypes?);
			TargetVideoFormat = default(HiresTargetVideoFormats?);
			InterplayItemType = default(InterplayItemTypes?);
			HiresOtherVideoFormatAdditionalText = default(string);
            HiresExportSpecificationsFile = default(string);
            ViewTargetFormat = default(ViewTargetFormats?);
			ViewingExportSpecifications = default(string);
			VideoViewingQuality = default(VideoViewingQualities?);
			ViewingExportSpecificationsFile = default(string);
			AafExportContainsMedia = default(bool?);
			AafMediaFormats = default(AafMediaFormats?);
            VaasaItemType = default(string);
            VaasaClipName = default(string);
            VaasaProgramId = default(string);
            VaasaProductionNumber = default(string);
            OtherHiresFileAttachmentInfo = new FileAttachmentInfo();
            OtherViewingVideoFileAttachmentInfo = new FileAttachmentInfo();
		}

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ElementName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string FolderName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] FolderUrls { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] FileUrls { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Information { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public InterplayPamExportFileTypes? ExportFileType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public HiresTargetVideoFormats? TargetVideoFormat { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public InterplayItemTypes? InterplayItemType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string HiresOtherVideoFormatAdditionalText { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string HiresExportSpecificationsFile { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public ViewTargetFormats? ViewTargetFormat { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ViewingExportSpecifications { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public VideoViewingQualities? VideoViewingQuality { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ViewingExportSpecificationsFile { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool? AafExportContainsMedia { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AafMediaFormats? AafMediaFormats { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string VaasaItemType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string VaasaProgramId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string VaasaClipName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string VaasaProductionNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FileAttachmentInfo OtherHiresFileAttachmentInfo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FileAttachmentInfo OtherViewingVideoFileAttachmentInfo { get; set; }
    }
}