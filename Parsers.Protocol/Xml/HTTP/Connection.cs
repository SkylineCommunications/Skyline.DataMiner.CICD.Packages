namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml.HTTP
{
    using System.Collections.Generic;

    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class Connection
    {
        public Connection(XmlElement xml, Session session, IList<Xml.Parameter> parameters)
        {
            Node = xml;

            Session = session;

            System.Int32.TryParse(xml.GetAttributeValue("id"), out int id);
            ID = id;

            var xRequest = xml.GetElement("request");
            if (xRequest != null)
            {
                Request = new Request(xRequest, parameters);
            }

            var xResponse = xml.GetElement("response");
            if (xResponse != null)
            {
                Response = new Response(xResponse, parameters);
            }
        }

        public Connection(int id, Session session)
        {
            ID = id;

            Session = session;
        }

        public XmlElement Node { get; private set; }

        public Session Session { get; set; }

        public int ID { get; set; }

        public Request Request { get; set; }

        public Response Response { get; set; }
    }
}