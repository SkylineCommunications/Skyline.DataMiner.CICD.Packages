namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class CreateResponseSuccessXml
	{
		public string RequestId { get; set; }
		public string RequestIdReference { get; set; }

		public static bool TryParse(IEngine engine, string response, out CreateResponseSuccessXml createResponseSuccessXml)
		{
			createResponseSuccessXml = null;

			try
			{
				var xml = XmlLinqExtensions.ParseXmlString(response);
				var ns = xml.Root.GetDefaultNamespace();

				if (xml.Root.Name.LocalName == "error")
					return false;

				createResponseSuccessXml = new CreateResponseSuccessXml();

				var requestIdElement = xml.Root.Element(ns + "requestIdRef");
				if (requestIdElement != null)
					createResponseSuccessXml.RequestId = requestIdElement.Value;

				var requestIdRefElement = xml.Root.Element(ns + "requestIdRef");
				if (requestIdRefElement != null)
					createResponseSuccessXml.RequestIdReference = requestIdRefElement.Value;

				return true;
			}
			catch (Exception ex)
			{
				engine.Log("Something went wrong during XML parsing: " + ex.Message);
			}

			return false;
		}
	}
}