using Newtonsoft.Json;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export
{
	public class ExportInformation
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public ExportInformation()
		{
			ExportDepartment = default(string);
			YleLogoInUpperLeftCornerOfVideo = default(bool);
			SourceTimecodeBurninToVideo = default(bool);
			AddSubtitlesToVideo = default(bool);
			TargetOfExport = default(string);
			MediaparkkiTargetFolder = default(string);
			AsperaFaspexReceiversEmailAddress = default(string);
			AsperaFaspexMessageHeadline = default(string);
			AsperaFaspexMessage = default(string);
			OtherExportTarget = default(string);
            ExportInformationFileAttachmentInfo = new FileAttachmentInfo();
		}

        [JsonProperty]
        public string ExportDepartment { get; set; }

        [JsonProperty]
        public bool YleLogoInUpperLeftCornerOfVideo { get; set; }

        [JsonProperty]
        public bool SourceTimecodeBurninToVideo { get; set; }

        [JsonProperty]
        public bool AddSubtitlesToVideo { get; set; }

        [JsonProperty]
        public string TargetOfExport { get; set; }

        [JsonProperty]
        public string MediaparkkiTargetFolder { get; set; }

        [JsonProperty]
        public string AsperaFaspexReceiversEmailAddress { get; set; }

        [JsonProperty]
        public string AsperaFaspexMessageHeadline { get; set; }

        [JsonProperty]
        public string AsperaFaspexMessage { get; set; }

        [JsonProperty]
        public string OtherExportTarget { get; set; }

        [JsonProperty]
        public FileAttachmentInfo ExportInformationFileAttachmentInfo { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"YLE logo in upper left corner of the video: {YleLogoInUpperLeftCornerOfVideo} | ");
            sb.AppendLine($"Source time code burn-in to video: { SourceTimecodeBurninToVideo} | ");
            sb.AppendLine($"Add subtitles to video: {AddSubtitlesToVideo} | ");

            sb.AppendLine($"Subtitle file attachments: {string.Join(";", ExportInformationFileAttachmentInfo.SubtitleAttachmentFileNames)} | ");

            return sb.ToString();
        }
    }
}