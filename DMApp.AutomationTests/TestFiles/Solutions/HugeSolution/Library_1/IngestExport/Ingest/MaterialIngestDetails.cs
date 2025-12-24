using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest
{
	public abstract class MaterialIngestDetails
	{
		public abstract string Type { get; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string AdditionalInformation { get; set; }
	}
}