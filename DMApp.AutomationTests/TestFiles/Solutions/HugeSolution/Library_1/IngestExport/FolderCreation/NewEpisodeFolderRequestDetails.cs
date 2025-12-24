using System;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;
using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation
{
	public class NewEpisodeFolderRequestDetails
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public NewEpisodeFolderRequestDetails()
		{
			DeleteDate = default(DateTime);
			ProducerName = default(string);
			ProducerEmail = default(string);
			MediaManagerName = default(string);
			MediaManagerEmail = default(string);
			ProductOrProductionName = default(string);
			EpisodeNumberOrName = default(string);
		}

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
        public string ProductOrProductionName { get; set; }

        [JsonProperty]
        public string EpisodeNumberOrName { get; set; }
	}
}