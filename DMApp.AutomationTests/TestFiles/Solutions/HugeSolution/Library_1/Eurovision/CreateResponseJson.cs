namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using Newtonsoft.Json;

	public class CreateResponseJson
	{
		[JsonProperty("requestId")]
		public string RequestId { get; set; }
		[JsonProperty("requestIdRef")]
		public string RequestIdReference { get; set; }
		[JsonProperty("requestNo")]
		public string RequestNumber { get; set; }
	}
}