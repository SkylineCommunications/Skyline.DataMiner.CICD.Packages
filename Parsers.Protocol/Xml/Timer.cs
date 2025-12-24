namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;
    using ContentTuple = System.Tuple<ContentType, int>;

    public class Timer
    {
        private static readonly XmlElement Dummy = new XmlElement();

        public Timer(XmlElement xml)
        {
            Node = xml;

            Int32.TryParse(xml.GetAttributeValue("id"), out int id);
            ID = id;

            Name = (xml.GetElement("name") ?? Dummy).InnerText;

            Int32.TryParse((xml.GetElement("time") ?? Dummy).InnerText, out int time);
            Time = time;
            Int32.TryParse((xml.GetElement("interval") ?? Dummy).InnerText, out int interval);
            Interval = interval;

            var xmlContent = xml.GetElements("content/group");
            foreach (var e in xmlContent)
            {
                if (!String.IsNullOrWhiteSpace(e.InnerText))
                {
                    Int32.TryParse(e.InnerText.Split(':')[0], out int i);
                    Content.Add(new ContentTuple(ContentType.Group, i));
                }
            }
        }

        public Timer(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public XmlElement Node { get; private set; }

        public int ID { get; set; }

        public string Name { get; set; }

        public int Time { get; set; }

        public int Interval { get; set; }

        private readonly List<ContentTuple> _content = new List<ContentTuple>();
        public IList<ContentTuple> Content => _content;
    }
}
