namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using System.Xml.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class CreateAssociatedItemRequest : IEurovisionObject
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("transmission")]
		public string Transmission { get; set; }

		[JsonProperty("destination")]
		public string Destination { get; set; }

		[JsonProperty("request")]
		public Facility Request { get; set; }

		public class Facility
		{
			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("productCode")]
			public string ProductCode { get; set; }

			[JsonProperty("beginDate"), JsonConverter(typeof(DateTimeExtensions.DateFormatConverter), "yyyy-MM-dd")]
			public DateTime BeginDate { get; set; }

			[JsonProperty("startTime")]
			public string StartTime { get; set; }

			[JsonProperty("endTime")]
			public string EndTime { get; set; }

			public XDocument SerializeToXml()
			{
				var xmlns = XNamespace.Get("http://www.eurovision.net/");
				var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
				var schemaLocation = XNamespace.Get("http://www.eurovision.net/ http://www.eurovision.net/rest/xsd/eos/associated-item.1.0.xsd");

				var associatedItem = new XElement(xmlns + "associatedItem",
					new XAttribute(XNamespace.Xmlns + "xsi", xsi),
					new XAttribute(xsi + "schemaLocation", xsi));

				associatedItem.Add(new XElement(xmlns + "id", Id));
				associatedItem.Add(new XElement(xmlns + "productCode", ProductCode));
				associatedItem.Add(new XElement(xmlns + "beginDate", BeginDate));
				if (!String.IsNullOrEmpty(StartTime)) associatedItem.Add(new XElement(xmlns + "startTime", StartTime));
				if (!String.IsNullOrEmpty(EndTime)) associatedItem.Add(new XElement(xmlns + "endTime", EndTime));

				return new XDocument(associatedItem);
			}
		}
	}
}