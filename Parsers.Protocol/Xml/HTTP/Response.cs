namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml.HTTP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class Response
    {
        public Response(XmlElement xml, IList<Xml.Parameter> parameters)
        {
            Node = xml;

            if (xml.TryGetAttribute("statusCode", out XmlAttribute xStatusCode) &&
                Int32.TryParse(xStatusCode.Value, out int statusCode))
            {
                StatusCode = parameters.FirstOrDefault(p => p.ID == statusCode);
            }

            var xHeaders = xml.Element["Headers"];
            if (xHeaders != null)
            {
                foreach (var xHeader in xHeaders.Children.OfType<XmlElement>())
                {
                    headers.Add(new Header(xHeader, parameters));
                }
            }

            var xContent = xml.GetElement("content");
            if (xContent != null)
            {
                Content = new Content(xContent, parameters);
            }
        }

        public Response() { }

        public XmlElement Node { get; private set; }

        public Xml.Parameter StatusCode { get; set; }

        private readonly List<Header> headers = new List<Header>();
        public IList<Header> Headers => headers;

        public Content Content { get; set; }
    }
}