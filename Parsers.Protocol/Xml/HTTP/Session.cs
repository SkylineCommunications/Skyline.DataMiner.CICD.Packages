namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml.HTTP
{
    using System.Collections.Generic;

    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class Session
    {
        private static readonly XmlElement Dummy = new XmlElement();

        public Session(XmlElement xml, IList<Xml.Parameter> parameters)
        {
            Node = xml;

            System.Int32.TryParse(xml.GetAttributeValue("id"), out int id);
            ID = id;

            if (xml.TryGetAttribute("name", out XmlAttribute xName))
            {
                Name = xName.Value;
            }

            foreach (var c in xml.GetElements("connection"))
            {
                connections.Add(new Connection(c, this, parameters));
            }
        }
        public Session(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public XmlElement Node { get; private set; }

        public int ID { get; set; }

        public string Name { get; set; }

        private readonly List<Connection> connections = new List<Connection>();
        public IList<Connection> Connections => connections;
    }
}
