namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    /// <summary>
    /// Represents an XML CDATA tag.
    /// </summary>
    public class XmlCDATA : XmlNode
    {

		/// <summary>
		/// Returns the inner text.
		/// </summary>
		public string InnerText =>
            // CDATA content must not be decoded!
            Token.Text.Substring(9, Token.Text.Length - 12);

        public override string GetXml()
        {
            // CDATA content must not be encoded!
            return $"<![CDATA[{InnerText}]]>";
        }

        public override string ToString()
		{
			return "<![CDATA[ ... ]]>";
		}
	}
}