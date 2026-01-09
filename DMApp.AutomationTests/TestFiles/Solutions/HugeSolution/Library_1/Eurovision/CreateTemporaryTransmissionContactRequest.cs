namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using System.Xml.Linq;
	using Newtonsoft.Json;

	public class CreateTemporaryTransmissionContactRequest : IEurovisionObject
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("transmission")]
		public string Transmission { get; set; }

		[JsonProperty("request")]
		public Contact Request { get; set; }

		public class Contact
		{
			[JsonProperty("email")]
			public string Email { get; set; }

			[JsonProperty("phone")]
			public string Phone { get; set; }

			[JsonProperty("firstName")]
			public string FirstName { get; set; }

			[JsonProperty("lastName")]
			public string LastName { get; set; }

			public XDocument SerializeToXml()
			{
				string xml = String.Format("<?xml version=\"1.0\" standalone=\"yes\"?>" +
				                           "<temporary-contact xmlns=\"http://www.eurovision.net/\" " +
				                           "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
				                           "xsi:schemaLocation=\"http://www.eurovision.net/ http://www.eurovision.net/rest/xsd/eos/temporary-contact.1.0.xsd\">" +
				                           "<email>{0}</email>" +
				                           "<lastname>{1}</lastname>" +
				                           "<firstname>{2}</firstname>" +
				                           "<phone>{3}</phone>" +
				                           "</temporary-contact>", Email, LastName, FirstName, Phone);

				return XDocument.Parse(xml);
			}
		}
	}
}