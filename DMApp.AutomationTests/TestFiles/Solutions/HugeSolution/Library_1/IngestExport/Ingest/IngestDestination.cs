using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest
{
	public class IngestDestination
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public IngestDestination()
		{
			Destination = default(string);
			InterplayDestinationFolder = default(string);
		}

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Destination { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string InterplayDestinationFolder { get; set; }
	}
}