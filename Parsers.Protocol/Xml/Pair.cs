namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class Pair
    {
        private static readonly XmlElement Dummy = new XmlElement();

        public Pair(XmlElement xml)
        {
            Node = xml;

            Int32.TryParse(xml.GetAttributeValue("id"), out int id);
            ID = id;

            Name = (xml.GetElement("name") ?? Dummy).InnerText;

        }

        public XmlElement Node { get; private set; }

        public int ID { get; set; }

        public string Name { get; set; }
    }
}