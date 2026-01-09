namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class CreateTransmissionRequest : IEurovisionObject
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("customerOrder")]
		public string CustomerOrder { get; set; }
		[JsonProperty("request")]
		public Transmission Request { get; set; }

		public class Transmission
		{
			[JsonProperty("eventNo", NullValueHandling = NullValueHandling.Ignore)]
			public string EventNumber { get; set; }
			[JsonProperty("productCode")]
			public string ProductCode { get; set; }
			[JsonProperty("startDate"), JsonConverter(typeof(DateTimeExtensions.DateFormatConverter), "yyyy-MM-dd")]
			public DateTime StartDate { get; set; }
			[JsonProperty("startTime")]
			public string StartTime { get; set; }
			[JsonProperty("endTime")]
			public string EndTime { get; set; }
			[JsonProperty("lineUp", NullValueHandling = NullValueHandling.Ignore)]
			public string LineUp { get; set; }
			[JsonProperty("nature1", NullValueHandling = NullValueHandling.Ignore)]
			public string Nature1 { get; set; }
			[JsonProperty("nature2", NullValueHandling = NullValueHandling.Ignore)]
			public string Nature2 { get; set; }
			[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
			public string Title { get; set; }
			[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
			public string Description { get; set; }
			[JsonProperty("originOrganizationCode", NullValueHandling = NullValueHandling.Ignore)]
			public string OriginOrganizationCode { get; set; }
			[JsonProperty("originCityCode", NullValueHandling = NullValueHandling.Ignore)]
			public string OriginCityCode { get; set; }
			[JsonProperty("originVenue", NullValueHandling = NullValueHandling.Ignore)]
			public string OriginVenue { get; set; }
			[JsonProperty("feedPointId", NullValueHandling = NullValueHandling.Ignore)]
			public string FeedPointId { get; set; }
			[JsonProperty("video")]
			public Video VideoDetails { get; set; }
			[JsonProperty("audios")]
			public Audio[] AudioDetails { get; set; }
			[JsonProperty("encryptionTypeCode", NullValueHandling = NullValueHandling.Ignore)]
			public string EncryptionTypeCode { get; set; }
			[JsonProperty("encryptionKey", NullValueHandling = NullValueHandling.Ignore)]
			public string EncryptionKey { get; set; }
			[JsonProperty("note")]
			public string Note { get; set; }
			[JsonProperty("reference")]
			public string Reference { get; set; }
			[JsonProperty("contractCode", NullValueHandling = NullValueHandling.Ignore)]
			public string ContractCode { get; set; }

			public class Video
			{
				[JsonProperty("videoDefinitionCode")]
				public string VideoDefinitionCode { get; set; }
				[JsonProperty("videoResolutionCode", NullValueHandling = NullValueHandling.Ignore)]
				public string VideoResolutionCode { get; set; }
				[JsonProperty("videoBitrateCode", NullValueHandling = NullValueHandling.Ignore)]
				public string VideoBitrateCode { get; set; }
				[JsonProperty("videoBandwidthCode", NullValueHandling = NullValueHandling.Ignore)]
				public string VideoBandwidthCode { get; set; }
				[JsonProperty("videoFrameRateCode", NullValueHandling = NullValueHandling.Ignore)]
				public string VideoFrameRateCode { get; set; }
				[JsonProperty("videoAspectRatioCode", NullValueHandling = NullValueHandling.Ignore)]
				public string VideoAspectRatioCode { get; set; }
				[JsonProperty("videoStreamCode", NullValueHandling = NullValueHandling.Ignore)]
				public string VideoStreamCode { get; set; }
			}

			public class Audio
			{
				[JsonProperty("code")]
				public string Code { get; set; }
				[JsonProperty("text")]
				public string Text { get; set; }
			}
		}
	}
}