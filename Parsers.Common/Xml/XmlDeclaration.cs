namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents an XML declaration tag.
    /// </summary>
    public class XmlDeclaration : XmlNode
    {
        public string Version => GetAttributeValue("version");

        public string Encoding => GetAttributeValue("encoding");

        public string Standalone => GetAttributeValue("standalone");
        
        public override string GetXml()
        {
            var sb = new StringBuilder();

            sb.Append("<?xml");
            if (!String.IsNullOrEmpty(Version)) sb.Append($" version=\"{Version}\"");
            if (!String.IsNullOrEmpty(Encoding)) sb.Append($" encoding=\"{Encoding}\"");
            if (!String.IsNullOrEmpty(Standalone)) sb.Append($" standalone=\"{Standalone}\"");
            sb.Append(" ?>");

            return sb.ToString();
        }

        public override string ToString()
        {
            return "<?xml version=\"" + Version + "\" ... ?>";
        }

        private string GetAttributeValue(string name)
        {
            var attribute = Token.ElementAttributes.FirstOrDefault(a => String.Equals(a.Name, name));
            return attribute?.Value;
        }
    }
}