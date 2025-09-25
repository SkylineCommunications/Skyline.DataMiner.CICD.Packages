namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;
    using ContentTuple = System.Tuple<ContentType, int>;

    public class Trigger
    {
        private static readonly XmlElement Dummy = new XmlElement();

        public Trigger(XmlElement xml)
        {
            Node = xml;

            Int32.TryParse(xml.GetAttributeValue("id"), out int id);
            ID = id;

            Name = (xml.GetElement("name") ?? Dummy).InnerText;

            string strType = (xml.GetElement("type") ?? Dummy).InnerText;
            switch (strType.ToLowerInvariant())
            {
                case "trigger":
                    Type = TriggerType.Trigger;
                    break;
                case "action":
                    Type = TriggerType.Action;
                    break;
                default:
                    Type = TriggerType.Action;
                    break;
            }

            var xmlContent = xml.GetElements("content/id");
            foreach (var e in xmlContent)
            {
                if (!String.IsNullOrWhiteSpace(e.InnerText))
                {
                    Int32.TryParse(e.InnerText, out int i);
                    Content.Add(new ContentTuple(ContentType.Group, i));
                }
            }
        }

        public XmlElement Node { get; private set; }

        public int ID { get; set; }

        public string Name { get; set; }

        public TriggerType Type { get; set; }

        private readonly List<ContentTuple> _content = new List<ContentTuple>();
        public IList<ContentTuple> Content => _content;
    }
}
