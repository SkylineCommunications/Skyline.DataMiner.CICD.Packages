namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class ParameterSNMP
    {
        public bool Enabled { get; set; }

        public string OID { get; set; }

        public ParameterSNMP(XmlElement xml)
        {
            var xml_enabled = xml.GetElement("enabled");

            if (xml_enabled != null)
            {
                Enabled = xml_enabled.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            var xml_oid = xml.GetElement("oid");
            if (xml_oid != null)
            {
                OID = xml_oid.InnerText;
            }
        }
    }
}
