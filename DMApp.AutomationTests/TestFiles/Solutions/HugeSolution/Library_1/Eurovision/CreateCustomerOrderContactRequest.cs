namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using Newtonsoft.Json;

	public class CreateCustomerOrderContactRequest : IEurovisionObject
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("customerOrder")]
		public string CustomerOrder { get; set; }
		[JsonProperty("request")]
		public Contact Request { get; set; }

		public class Contact
		{
			[JsonProperty("email")]
			public string Email { get; set; }
			[JsonProperty("lastname")]
			public string LastName { get; set; }
			[JsonProperty("firstname")]
			public string FirstName { get; set; }
			[JsonProperty("phone")]
			public string Phone { get; set; }
		}
	}
}