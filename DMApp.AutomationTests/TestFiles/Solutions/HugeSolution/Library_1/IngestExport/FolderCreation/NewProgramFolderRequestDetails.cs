using System;
using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation
{
	public class NewProgramFolderRequestDetails
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public NewProgramFolderRequestDetails()
		{
			ProgramName = default(string);
			ProgramShortName = default(string);
			DeleteDate = default(DateTime);
			ProducerName = default(string);
			ProducerEmail = default(string);
			MediaManagerName = default(string);
			MediaManagerEmail = default(string);
			ProductNumber = default(string);
			IsDeleteDateUnknown = default(bool);
		}

        [JsonProperty]
        public string ProgramName { get; set; }

        [JsonProperty]
        public string ProgramShortName { get; set; }

        [JsonProperty]
        public DateTime DeleteDate { get; set; }

        [JsonProperty]
        public string ProducerName { get; set; }

        [JsonProperty]
        public string ProducerEmail { get; set; }

        [JsonProperty]
        public string MediaManagerName { get; set; }

        [JsonProperty]
        public string MediaManagerEmail { get; set; }

        [JsonProperty]
        public string ProductNumber { get; set; }

        [JsonProperty]
        public bool IsDeleteDateUnknown { get; set; }
    }
}