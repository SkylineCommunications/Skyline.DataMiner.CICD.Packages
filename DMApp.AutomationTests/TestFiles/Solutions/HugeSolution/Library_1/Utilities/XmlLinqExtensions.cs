namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;

	public static class XmlLinqExtensions
	{
		public static XDocument ParseXmlString(this string input)
		{
			if (IsValidXmlString(input))
				return XDocument.Parse(input);
			else
				return XDocument.Parse(new string(input.Where(r => XmlConvert.IsXmlChar(r)).ToArray())); // trim invalid chars
		}

		public static string GetAttributeValue(this XElement element, string attributeName)
		{
			string value = null;
			if (element != null)
			{
				var attribute = element.Attribute(attributeName);
				if (attribute != null)
					return attribute.Value;
			}

			return value;
		}

		public static string GetElementValue(this XElement element)
		{
			return element != null ? element.Value : null;
		}

		public static string GetElementValue(this XElement element, string elementName, XNamespace ns = null)
		{
			string value = null;
			if (element != null)
			{
				var child = ns == null ? element.Element(elementName) : element.Element(ns + elementName);
				value = child.GetElementValue();
			}

			return value;
		}

		private static bool IsValidXmlString(this string input)
		{
			try
			{
				XmlConvert.VerifyXmlChars(input);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}