namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class CreateParticipationRequest : IEurovisionObject
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("customerOrder")]
		public string CustomerOrder { get; set; }
		[JsonProperty("request")]
		public Participation Request { get; set; }

		public class Participation
		{
			[JsonProperty("startDate"), JsonConverter(typeof(DateTimeExtensions.DateFormatConverter), "yyyy-MM-dd")]
			public DateTime StartDate { get; set; }
			[JsonProperty("startTime")]
			public string StartTime { get; set; }
			[JsonProperty("endTime")]
			public string EndTime { get; set; }
			[JsonProperty("transmissionNo")]
			public string TransmissionNumber { get; set; }
			[JsonProperty("eventNo")]
			public string EventNumber { get; set; }
			[JsonProperty("contract", NullValueHandling = NullValueHandling.Ignore)]
			public string Contract { get; set; }
			[JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
			public string System { get; set; }
			[JsonProperty("reference", NullValueHandling = NullValueHandling.Ignore)]
			public string Reference { get; set; }
			[JsonProperty("organization", NullValueHandling = NullValueHandling.Ignore)]
			public string Organization { get; set; }
			[JsonProperty("billTo", NullValueHandling = NullValueHandling.Ignore)]
			public string BillTo { get; set; }
			[JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
			public string City { get; set; }
			[JsonProperty("bureau", NullValueHandling = NullValueHandling.Ignore)]
			public string Bureau { get; set; }
			[JsonProperty("via", NullValueHandling = NullValueHandling.Ignore)]
			public string Via { get; set; }
			[JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
			public string Note { get; set; }
		}
	}
}