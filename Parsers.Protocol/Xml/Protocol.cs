namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class Protocol
    {
        /// <summary>
        /// Loads a protocol from the Protocol root node. 
        /// </summary>
        /// <param name="xml">The Protocol tag.</param>
        public Protocol(XmlElement xml)
        {
            Node = xml;

            if (!Node.Name.Equals("protocol", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            XmlElement dummy = new XmlElement();

            Name = (xml.GetElement("name") ?? dummy).InnerText;
            Description = (xml.GetElement("description") ?? dummy).InnerText;
            Version = (xml.GetElement("version") ?? dummy).InnerText;

            XmlElement xmlDisplay = xml.GetElement("display");
            if (xmlDisplay != null)
            {
                DefaultPage = xmlDisplay.GetAttributeValue("defaultPage");
                WidePages = xmlDisplay.GetAttributeValue("wideColumnPages");
                PageOrder = xmlDisplay.GetAttributeValue("pageOrder");
                PageOptions = xmlDisplay.GetAttributeValue("pageOptions");
            }

            XmlElement xmlType = xml.GetElement("type");
            if (xmlType != null)
            {
                TypeOptions = xmlType.GetAttributeValue("options");
            }

            var xmlParams = xml.GetElements("params/param");
            foreach (var x in xmlParams)
            {
                Parameters.Add(new Parameter(x));
            }

            var xmlTriggers = xml.GetElements("triggers/trigger");
            foreach (var x in xmlTriggers)
            {
                Triggers.Add(new Trigger(x));
            }

            var xmlActions = xml.GetElements("actions/action");
            foreach (var x in xmlActions)
            {
                Actions.Add(new Xml.Action(x));
            }

            var xmlPairs = xml.GetElements("pairs/pair");
            foreach (var x in xmlPairs)
            {
                Pairs.Add(new Pair(x));
            }

            var xmlSessions = xml.GetElements("http/session");
            foreach (var x in xmlSessions)
            {
                Sessions.Add(new HTTP.Session(x, Parameters));
            }

            var xmlGroups = xml.GetElements("groups/group");
            foreach (var x in xmlGroups)
            {
                Groups.Add(new Group(x));
            }

            var xmlTimers = xml.GetElements("timers/timer");
            foreach (var x in xmlTimers)
            {
                Timers.Add(new Timer(x));
            }

            // Load all the export rules.
            try
            {
                _exportRules.AddRange(ExportRule.ParseExportRules(xml));
            }
            catch (Exception) { /* ... */ }

            // Parse DVE protocols -> they will remove parameters from this protocol and use the XML to retrieve export rules etcetera ...
            _dveProtocols.AddRange(DVEProtocol.ParseDVEProtocols(this, xml));
        }
        /// <summary>
        /// Can be used by derived elements to create a protocol without parsing the protocol.
        /// </summary>
        protected Protocol()
        { /* Empty initializer: for use in derived classes where special functions are used to load data. */ }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }

        public XmlElement Node { get; private set; }

        public string DefaultPage { get; set; }
        public string WidePages { get; set; }
        public string PageOrder { get; set; }
        public string PageOptions { get; set; }

        public string TypeOptions { get; set; }

        protected readonly List<Parameter> _parameters = new List<Parameter>();
        public IList<Parameter> Parameters => _parameters;

        protected readonly List<Trigger> _triggers = new List<Trigger>();
        public IList<Trigger> Triggers => _triggers;

        protected readonly List<Xml.Action> _actions = new List<Xml.Action>();
        public IList<Xml.Action> Actions => _actions;

        protected readonly List<Pair> _pairs = new List<Pair>();
        public IList<Pair> Pairs => _pairs;

        protected readonly List<HTTP.Session> _sessions = new List<HTTP.Session>();
        public IList<HTTP.Session> Sessions => _sessions;

        protected readonly List<Group> _groups = new List<Group>();
        public IList<Group> Groups => _groups;

        protected readonly List<Timer> _timers = new List<Timer>();
        public IList<Timer> Timers => _timers;

        private readonly List<DVEProtocol> _dveProtocols = new List<DVEProtocol>();
        public IList<DVEProtocol> DVEProtocols => _dveProtocols;

        protected readonly List<ExportRule> _exportRules = new List<ExportRule>();
        public virtual IList<ExportRule> ExportRules => _exportRules;
    }

    /// <summary>
    /// The DVE protocol shows only parameters exported to the DVE and export rules linked to the DVE.
    /// When created it could also remove the exported parameters from the source protocol!
    /// </summary>
    public class DVEProtocol : Protocol
    {
        //STATICS
        /// <summary>
        /// Retrieves a list with all exported (DVE) protocols. 
        /// Note: the params are loaded from the parent protocol. If they were not yet loaded, invoke the LoadParams function to load new parameters.
        /// </summary>
        /// <param name="parent">The parent protocol, can be used to provide a link to the source protocol, but can be null as well.</param>
        /// <param name="source">The XML Protocol node.</param>
        /// <returns></returns>
        public static List<DVEProtocol> ParseDVEProtocols(Protocol parent, XmlElement source)
        {
            List<DVEProtocol> DVEs = new List<DVEProtocol>();

            var element = source.GetElement("type");
            if (element == null)
            {
                return DVEs;
            }

            if (element.GetAttributeValue("options") != null)
            {
                Regex FindExportProtocols = new Regex("exportProtocol:(?<Name>.*?):(?<ID>[0-9]+)", RegexOptions.None);
                MatchCollection Exported = FindExportProtocols.Matches(element.GetAttributeValue("options", ""));
                foreach (Match export in Exported)
                {
                    DVEProtocol protocol = new DVEProtocol(parent, source, Convert.ToInt32(export.Groups["ID"].Captures[0].Value));
                    protocol.Name = export.Groups["Name"].Captures[0].Value;
                    DVEs.Add(protocol);
                }
            }

            return DVEs;
        }

        // INIT
        public DVEProtocol(Protocol sourceProtocol, XmlElement xml, int exportID)
        {
            _sourceProtocol = sourceProtocol;
            _exportTableID = exportID;

            base.Version = sourceProtocol.Version;

            LoadParams();
            LoadExportRules();
        }

        // CODE
        /// <summary>
        /// Selects all parameters linked to the DVE protocol and adds them to the parameter list. It also removes the parameters from the parent.
        /// </summary>
        /// <param name="RemoveFromParent">True if parameters from the parents should be removed</param>
        public void LoadParams(bool RemoveFromParent = false)
        {
            // Copy all exported parameters to the DVE protocol
            base._parameters.AddRange(SourceProtocol.Parameters.Where(x => x.IsExport && (x.ExportToAll || x.Export.Contains(ExportTableID))));
            ////foreach (var param in parent.Parameters.Where(x => x.IsExport && x.Export == ExportTableID))
            ////    try { base.Parameters.Add(param); }
            ////    catch (Exception ex) { log("Failed to copy a parameter to the DVE protocol: " + ex.Message); }

            // Remove all exported parameters from the source protocol.
            if (RemoveFromParent)
            {
                foreach (var param in base.Parameters.Where(p => SourceProtocol.Parameters.Contains(p)))
                {
                    try
                    { SourceProtocol.Parameters.Remove(param); }
                    catch (Exception ex) { log("Failed to remove a exported parameter from the source protocol: " + ex.Message); }
                }
            }
        }

        public void LoadExportRules()
        {
            _exportRules.AddRange(
                SourceProtocol.ExportRules.Where(
                    x => (x.TargetsAll || x.TargetTable.Contains(ExportTableID))
                         && !_exportRules.Contains(x)));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void log(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        // PROPERTIES
        private readonly Protocol _sourceProtocol = null;
        public Protocol SourceProtocol => _sourceProtocol;

        private readonly int _exportTableID = -1;
        public int ExportTableID => _exportTableID;
    }

}
