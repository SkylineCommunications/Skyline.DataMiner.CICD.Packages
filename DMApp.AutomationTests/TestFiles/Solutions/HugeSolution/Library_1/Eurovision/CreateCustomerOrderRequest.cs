namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using Newtonsoft.Json;

	public class CreateCustomerOrderRequest : IEurovisionObject
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("request")]
		public CustomerOrder Request { get; set; }

		public class CustomerOrder
		{
			[JsonProperty("billTo")]
			public string BillTo { get; set; }
			[JsonProperty("eventNo")]
			public string EventNumber { get; set; }
			[JsonProperty("reference")]
			public string Reference { get; set; }
			[JsonProperty("bureau")]
			public string Bureau { get; set; }
			[JsonProperty("note")]
			public string Note { get; set; }
			[JsonProperty("draft")]
			public bool Draft { get; set; }
		}
	}
}