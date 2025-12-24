namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using System.Xml.Linq;
	using Newtonsoft.Json;

	public class CreateTechnicalSystemRequest : IEurovisionObject
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("transmission")]
		public string Transmission { get; set; }
		[JsonProperty("request")]
		public TechnicalSystem Request { get; set; }

		public class TechnicalSystem
		{
			[JsonProperty("systemId")]
			public string SystemId { get; set; }
			[JsonProperty("video")]
			public Video VideoDetails { get; set; }
			[JsonProperty("audios")]
			public Audio[] AudioDetails { get; set; }
			[JsonProperty("encryptionTypeCode", NullValueHandling = NullValueHandling.Ignore)]
			public string EncryptionTypeCode { get; set; }
			[JsonProperty("encryptionKey", NullValueHandling = NullValueHandling.Ignore)]
			public string EncryptionKey { get; set; }

			public XDocument SerializeToXml()
			{
				var xmlns = XNamespace.Get("http://www.eurovision.net/");
				var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
				var schemaLocation = XNamespace.Get("http://www.eurovision.net/ http://www.eurovision.net/rest/xsd/eos/technical-system.1.0.xsd");

				var technicalSystem = new XElement(xmlns + "technicalSystem",
					new XAttribute(XNamespace.Xmlns + "xsi", xsi),
					new XAttribute(xsi + "schemaLocation", xsi));

				if (!String.IsNullOrEmpty(SystemId))
					technicalSystem.Add(new XElement(xmlns + "systemId", SystemId));

				var video = new XElement(xmlns + "video");
				if (!String.IsNullOrEmpty(VideoDetails.VideoDefinitionCode))
					video.Add(new XElement(xmlns + "videoDefinitionCode", VideoDetails.VideoDefinitionCode));
				if (!String.IsNullOrEmpty(VideoDetails.VideoResolutionCode))
					video.Add(new XElement(xmlns + "videoResolutionCode", VideoDetails.VideoResolutionCode));
				if (!String.IsNullOrEmpty(VideoDetails.VideoBandwidthCode))
					video.Add(new XElement(xmlns + "videoBandwidthCode", VideoDetails.VideoBandwidthCode));
				if (!String.IsNullOrEmpty(VideoDetails.VideoFrameRateCode))
					video.Add(new XElement(xmlns + "videoFrameRateCode", VideoDetails.VideoFrameRateCode));
				if (!String.IsNullOrEmpty(VideoDetails.VideoStreamCode))
					video.Add(new XElement(xmlns + "videoStreamCode", VideoDetails.VideoStreamCode));
				technicalSystem.Add(video);

				var audios = new XElement(xmlns + "audios");
				foreach (var audioDetails in AudioDetails)
				{
					audios.Add(new XElement(xmlns + "audio",
						new XElement(xmlns + "code", audioDetails.Code),
						new XElement(xmlns + "text", audioDetails.Text)));
				}

				technicalSystem.Add(audios);

				if (!String.IsNullOrEmpty(EncryptionTypeCode))
					technicalSystem.Add(new XElement(xmlns + "encryptionTypeCode", EncryptionTypeCode));
				if (!String.IsNullOrEmpty(EncryptionKey))
					technicalSystem.Add(new XElement(xmlns + "encryptionKey", EncryptionKey));

				return new XDocument(technicalSystem);
			}

			public class Video
			{
				[JsonProperty("videoDefinitionCode")]
				public string VideoDefinitionCode { get; set; }
				[JsonProperty("videoResolutionCode")]
				public string VideoResolutionCode { get; set; }
				[JsonProperty("videoBitrateCode", NullValueHandling = NullValueHandling.Ignore)]
				public string VideoBitrateCode { get; set; }
				[JsonProperty("videoBandwidthCode", NullValueHandling = NullValueHandling.Ignore)]
				public string VideoBandwidthCode { get; set; }
				[JsonProperty("videoFrameRateCode")]
				public string VideoFrameRateCode { get; set; }
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