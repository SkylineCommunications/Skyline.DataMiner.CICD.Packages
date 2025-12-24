namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class ParameterInformation
    {
        public string Text { get; set; }
        public string Subtext { get; set; }

        public HashSet<string> Includes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public ParameterInformation(XmlElement xml)
        {
            var xml_text = xml.GetElement("text");
            if (xml_text != null)
            {
                Text = xml_text.InnerText;
            }

            var xml_subtext = xml.GetElement("subtext");
            if (xml_subtext != null && xml_subtext.InnerText != null)
            {
                Subtext = xml_subtext.InnerText.Trim();
            }

            var xml_includes = xml.GetElement("includes");
            if (xml_includes != null)
            {
                var xml_include = xml_includes.GetElements("include");

                foreach (var xml_inc in xml_include)
                {
                    Includes.Add(xml_inc.InnerText);
                }
            }
        }
    }
}
