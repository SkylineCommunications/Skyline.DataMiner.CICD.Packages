namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System.Collections.Generic;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class ParameterMeasurement
    {
        public string Type { get; set; }
        public Dictionary<string, string> Options { get; set; }

        private readonly List<ParameterDiscreet> _discreets = new List<ParameterDiscreet>();
        public IList<ParameterDiscreet> Discreets => _discreets;

        public ParameterMeasurement(XmlElement xml)
        {
            Options = new Dictionary<string, string>();
            Type = "";

            XmlElement typeelement = xml.GetElement("type");
            if (typeelement != null)
            {
                Type = typeelement.InnerText;

                if (typeelement.TryGetAttribute("options", out var options))
                {
                    foreach (string opt in (options.Value ?? "").Split(';'))
                    {
                        string[] optparts = opt.Split(new char[] { '=' }, 2);

                        if (optparts.Length == 1)
                        {
                            Options.Add(optparts[0], "");
                        }
                        else if (optparts.Length > 1)
                        {
                            Options.Add(optparts[0], optparts[1]);
                        }
                    }
                }
            }

            var xmlDiscreets = xml.GetElements("discreets/discreet");
            foreach (var e in xmlDiscreets)
            {
                try
                { Discreets.Add(new ParameterDiscreet(e)); }
                catch { }
            }
        }
    }
}
