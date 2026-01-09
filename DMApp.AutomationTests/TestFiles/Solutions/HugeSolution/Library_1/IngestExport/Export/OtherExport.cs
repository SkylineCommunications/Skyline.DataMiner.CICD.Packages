using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export
{
	public class OtherExport
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public OtherExport()
		{
			SourceFileLocation = default(string);
			SourceFileName = default(string);
			ExportType = default(string);
			AdditionalInformation = default(string);
            OtherExportSourceFileAttachmentInfo = new FileAttachmentInfo();
		}

		public string SourceFileLocation { get; set; }

		public string SourceFileName { get; set; }

		public string ExportType { get; set; }

        [JsonProperty]
        public string AdditionalInformation { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FileAttachmentInfo OtherExportSourceFileAttachmentInfo { get; set; }
    }
}