namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    using ContentTuple = System.Tuple<string, string>;

    public class Group
    {
        private static readonly XmlElement Dummy = new XmlElement();

        public Group(XmlElement xml)
        {
            Node = xml;

            Int32.TryParse(xml.GetAttributeValue("id"), out int id);
            ID = id;

            Name = (xml.GetElement("name") ?? Dummy).InnerText;
            Description = (xml.GetElement("description") ?? Dummy).InnerText;

            string strType = (xml.GetElement("type") ?? Dummy).InnerText ?? "";
            switch (strType.ToLowerInvariant())
            {
                case "poll":
                    Type = GroupType.Poll;
                    break;
                case "trigger":
                    Type = GroupType.Trigger;
                    break;
                case "action":
                    Type = GroupType.Action;
                    break;
                case "poll trigger":
                    Type = GroupType.PollTrigger;
                    break;
                case "poll action":
                    Type = GroupType.PollAction;
                    break;
                default:
                    Type = GroupType.Poll;
                    break;
            }

            var xmlContent = xml.GetElement("content") ?? Dummy;

            var attval = xmlContent.GetAttributeValue("multipleget");
            if (attval != null)
            {
                MultipleGet = String.Equals(attval, "true", StringComparison.InvariantCultureIgnoreCase);
            }

            foreach (var e in xmlContent.Children.Where(e => e is XmlElement).Cast<XmlElement>())
            {
                Content.Add(new ContentTuple(e.Token.ElementName, e.InnerText));
            }
        }

        public Group(int id, string name, string description)
        {
            ID = id;
            Name = name;
            Description = description;
            Type = GroupType.Poll;
            MultipleGet = false;
        }

        public XmlElement Node { get; private set; }

        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public GroupType Type { get; set; }

        public bool MultipleGet { get; set; }

        private readonly List<ContentTuple> _content = new List<ContentTuple>();
        public IList<ContentTuple> Content => _content;
    }
}
