namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class ParameterInterprete
    {
        public string RawType { get; set; }
        public string LengthType { get; set; }
        public string Type { get; set; }

        public ParameterInterprete(XmlElement xml)
        {
            var xml_rawtype = xml.GetElement("rawtype");
            if (xml_rawtype != null && xml_rawtype.InnerText != null)
            {
                RawType = xml_rawtype.InnerText.Trim();
            }

            var xml_lengthtype = xml.GetElement("lengthtype");
            if (xml_lengthtype != null && xml_lengthtype.InnerText != null)
            {
                LengthType = xml_lengthtype.InnerText.Trim();
            }

            var xml_type = xml.GetElement("type");
            if (xml_type != null && xml_type.InnerText != null)
            {
                Type = xml_type.InnerText.Trim();
            }
        }
    }
}
