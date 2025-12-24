namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    /// <summary>
    /// Represents an XML comment tag.
    /// </summary>
    public class XmlComment : XmlNode
	{

		/// <summary>
		/// Returns the inner text.
		/// </summary>
		public string InnerText =>
            // Comment content must not be decoded!
            Token.Text.Substring(4, Token.Text.Length - 7);

        public override string GetXml()
        {
            // Comment content must not be encoded!
            return $"<!-- {InnerText} -->";
        }

        public override string ToString()
		{
			return "<!-- ... -->";
		}
	}
}