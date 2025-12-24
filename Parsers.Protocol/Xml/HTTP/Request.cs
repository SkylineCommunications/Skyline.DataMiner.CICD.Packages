namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml.HTTP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class Request
    {
        public Request(XmlElement xml, IList<Xml.Parameter> parameters)
        {
            Node = xml;

            if (xml.TryGetAttribute("pid", out XmlAttribute xPid) &&
                Int32.TryParse(xPid.Value, out int pid))
            {
                Pid = parameters.FirstOrDefault(p => p.ID == pid);
            }

            if (xml.TryGetAttribute("url", out XmlAttribute xUrl))
            {
                Url = xUrl.Value;
            }

            Verb = RequestVerb.GET;

            if (xml.TryGetAttribute("verb", out XmlAttribute xVerb))
            {
                switch (xVerb.Value)
                {
                    case "CONNECT":
                        Verb = RequestVerb.CONNECT;
                        break;
                    case "DELETE":
                        Verb = RequestVerb.DELETE;
                        break;
                    case "HEAD":
                        Verb = RequestVerb.HEAD;
                        break;
                    case "OPTIONS":
                        Verb = RequestVerb.OPTIONS;
                        break;
                    case "PATCH":
                        Verb = RequestVerb.PATCH;
                        break;
                    case "POST":
                        Verb = RequestVerb.POST;
                        break;
                    case "PUT":
                        Verb = RequestVerb.PUT;
                        break;
                    case "TRACE":
                        Verb = RequestVerb.TRACE;
                        break;
                    case "GET":
                    default:
                        Verb = RequestVerb.GET;
                        break;
                }
            }

            var xHeaders = xml.GetElement("headers");
            if (xHeaders != null)
            {
                foreach (var xHeader in xHeaders.Children.OfType<XmlElement>())
                {
                    headers.Add(new Header(xHeader, parameters));
                }
            }

            var xParameters = xml.GetElement("parameters");
            if (xParameters != null)
            {
                foreach (var xParameter in xParameters.Children.OfType<XmlElement>())
                {
                    this.parameters.Add(new Parameter(xParameter, parameters));
                }
            }

            var xData = xml.GetElement("data");
            if (xData != null)
            {
                Data = new Data(xData, parameters);
            }
        }

        public Request(RequestVerb verb = RequestVerb.GET)
        {
            Verb = verb;
        }

        public XmlElement Node { get; private set; }

        public Xml.Parameter Pid { get; set; }

        public string Url { get; set; }

        public RequestVerb Verb { get; set; }

        private readonly List<Header> headers = new List<Header>();
        public IList<Header> Headers => headers;

        private readonly List<Parameter> parameters = new List<Parameter>();
        public IList<Parameter> Parameters => parameters;

        public Data Data { get; set; }
    }
}