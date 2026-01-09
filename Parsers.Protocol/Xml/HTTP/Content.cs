namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml.HTTP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class Content
    {
        public Content(XmlElement xml, IList<Xml.Parameter> parameters)
        {
            Node = xml;

            if (xml.TryGetAttribute("pid", out XmlAttribute xPid) &&
                Int32.TryParse(xPid.Value, out int pid))
            {
                Pid = parameters.FirstOrDefault(p => p.ID == pid);
            }
        }

        public Content() { }

        public XmlElement Node { get; private set; }

        public Xml.Parameter Pid { get; set; }
    }
}