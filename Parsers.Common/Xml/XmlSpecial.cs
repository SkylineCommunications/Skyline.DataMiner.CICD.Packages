namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    using System.Net;

    /// <summary>
    /// Represents a special, self-closing XML tag
    /// </summary>
    public class XmlSpecial : XmlNode
    {
        /// <summary>
        /// Returns the inner text.
        /// </summary>
        public string InnerText
        {
            get
            {
                string text = Token.Text.Substring(1, Token.Text.Length - 2);
                return WebUtility.HtmlDecode(text);
            }
        }

        public override string GetXml()
        {
            return $"<{WebUtility.HtmlEncode(InnerText)}>";
        }

        public override string ToString()
		{
			return "< ??? >";
		}
	}
}