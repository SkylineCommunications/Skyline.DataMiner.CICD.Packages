namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml.HTTP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class Header
    {
        public Header(XmlElement xml, IList<Xml.Parameter> parameters)
        {
            Node = xml;

            if (xml.TryGetAttribute("key", out XmlAttribute xKey))
            {
                Key = xKey.Value;
            }

            if (xml.TryGetAttribute("pid", out XmlAttribute xPid) &&
                Int32.TryParse(xPid.Value, out int pid))
            {
                Pid = parameters.FirstOrDefault(p => p.ID == pid);
            }

            Value = xml.InnerText;
        }

        public Header() { }

        public XmlElement Node { get; private set; }

        public string Key { get; set; }

        public Xml.Parameter Pid { get; set; }

        public string Value { get; set; }
    }
}