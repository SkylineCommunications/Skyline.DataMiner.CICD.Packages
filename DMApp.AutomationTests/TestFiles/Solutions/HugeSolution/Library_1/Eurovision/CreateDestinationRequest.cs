namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using System.Xml.Linq;
	using Newtonsoft.Json;

	public class CreateDestinationRequest : IEurovisionObject
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("transmission")]
		public string Transmission { get; set; }
		[JsonProperty("request")]
		public Destination Request { get; set; }

		public class Destination
		{
			[JsonProperty("organizationCode", NullValueHandling = NullValueHandling.Ignore)]
			public string OrganizationCode { get; set; }
			[JsonProperty("organizationCodeBillingTo", NullValueHandling = NullValueHandling.Ignore)]
			public string OrganizationCodeBillingTo { get; set; }
			[JsonProperty("cityCode", NullValueHandling = NullValueHandling.Ignore)]
			public string CityCode { get; set; }
			[JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
			public string StartTime { get; set; }
			[JsonProperty("endTime", NullValueHandling = NullValueHandling.Ignore)]
			public string EndTime { get; set; }
			[JsonProperty("via", NullValueHandling = NullValueHandling.Ignore)]
			public string Via { get; set; }
			[JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
			public string Note { get; set; }

			public XDocument SerializeToXml()
			{
				var xmlns = XNamespace.Get("http://www.eurovision.net/");
				var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
				var schemaLocation = XNamespace.Get("http://www.eurovision.net/ http://www.eurovision.net/rest/xsd/eos/destination.1.0.xsd");

				var destination = new XElement(xmlns + "destination",
					new XAttribute(XNamespace.Xmlns + "xsi", xsi),
					new XAttribute(xsi + "schemaLocation", xsi));

				if (!String.IsNullOrEmpty(OrganizationCode))
					destination.Add(new XElement(xmlns + "organizationCode", OrganizationCode));
				if (!String.IsNullOrEmpty(OrganizationCodeBillingTo))
					destination.Add(new XElement(xmlns + "organizationCodeBillingTo", OrganizationCodeBillingTo));
				if (!String.IsNullOrEmpty(CityCode))
					destination.Add(new XElement(xmlns + "cityCode", CityCode));
				if (!String.IsNullOrEmpty(StartTime))
					destination.Add(new XElement(xmlns + "startTime", StartTime));
				if (!String.IsNullOrEmpty(EndTime))
					destination.Add(new XElement(xmlns + "endTime", EndTime));
				if (!String.IsNullOrEmpty(Via))
					destination.Add(new XElement(xmlns + "via", Via));
				if (!String.IsNullOrEmpty(Note))
					destination.Add(new XElement(xmlns + "note", Note));

				return new XDocument(destination);
			}
		}
	}
}