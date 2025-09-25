namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class ParameterDiscreet
    {
        public string Display { get; set; }
        public string Value { get; set; }

        public ParameterDiscreet(XmlElement xml)
        {
            Display = xml.GetElement("display").InnerText;
            Value = xml.GetElement("value").InnerText;
        }
    }
}
