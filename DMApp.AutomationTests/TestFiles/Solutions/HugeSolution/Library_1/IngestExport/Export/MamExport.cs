using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export
{
	public class MamExport
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public MamExport()
		{
			ProgramName = default(string);
			ProgramId = default(string);
			ExportFileTypes = default(MamExportFileTypes?);
			ViewingVideoTargetFormat = default(ViewTargetFormats?);
            ExportFileTypeFileSpecifications = default(string);
            OtherTargetVideoFormatFileAttachmentInfo = new FileAttachmentInfo();
        }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ProgramName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ProgramId { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public MamExportFileTypes? ExportFileTypes { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public ViewTargetFormats? ViewingVideoTargetFormat { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ExportFileTypeFileSpecifications { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FileAttachmentInfo OtherTargetVideoFormatFileAttachmentInfo { get; set; }
    }
}