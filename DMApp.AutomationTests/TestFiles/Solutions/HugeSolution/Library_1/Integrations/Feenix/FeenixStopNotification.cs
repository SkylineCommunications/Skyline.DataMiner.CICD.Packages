namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Feenix
{
	using Newtonsoft.Json;

	public class FeenixStopNotification
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		public FeenixStopNotification(string id)
		{
			Id = id;
			State = "stopped";
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
