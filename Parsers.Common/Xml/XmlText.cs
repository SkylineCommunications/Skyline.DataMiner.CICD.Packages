namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    using System;
    using System.Net;

    /// <summary>
    /// Represents text between tags (whitespace, inner text, surrounding text) or an invalid token.
    /// </summary>
    public class XmlText : XmlNode
    {
        public string Text
        {
            get
            {
                string text = Token.Text;
                return WebUtility.HtmlDecode(text);
            }
        }

        public override string GetXml()
        {
            return WebUtility.HtmlEncode(Text ?? String.Empty);
        }

        public override string ToString()
        {
            return "TEXT: " + Token.Text;
        }
    }
}