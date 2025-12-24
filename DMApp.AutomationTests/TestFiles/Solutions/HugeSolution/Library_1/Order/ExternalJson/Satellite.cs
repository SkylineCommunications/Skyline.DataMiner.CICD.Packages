using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson
{
	public class Satellite
	{
		public Satellite()
		{
			SatelliteResource = default(string);
		}

		[JsonProperty("Satellite")]
		[IsResourceName]
		public string SatelliteResource { get; set; }
	}
}