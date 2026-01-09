using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson
{
	public class ExternalJson
	{
		[JsonProperty("Order")]
		public JsonOrder Order { get; set; }
	}
}
