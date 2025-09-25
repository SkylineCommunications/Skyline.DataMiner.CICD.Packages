namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class ParameterAlarm
    {
        public bool IsMonitored { get; set; }

        public ParameterAlarm(XmlElement xml)
        {
            var xml_monitored = xml.GetElement("monitored");

            if (xml_monitored != null)
            {
                IsMonitored = xml_monitored.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
