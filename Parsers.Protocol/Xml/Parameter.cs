namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class Parameter
    {
        private static XmlElement dummy = new XmlElement();

        public Parameter()
        {
            Alarm = new ParameterAlarm(dummy);
            SNMP = new ParameterSNMP(dummy);
            Display = new ParameterDisplay(dummy);
            Measurement = new ParameterMeasurement(dummy);
            Information = new ParameterInformation(dummy);
            Interprete = new ParameterInterprete(dummy);
        }
        public Parameter(XmlElement xml)
        {
            Node = xml;

            int id;
            Int32.TryParse(xml.GetAttributeValue("id"), out id);
            ID = id;

            Name = (xml.GetElement("name") ?? dummy).InnerText;
            Description = (xml.GetElement("description") ?? dummy).InnerText;

            Type = (xml.GetElement("type") ?? dummy).InnerText;
            if (Type != null)
            {
                Type = Type.Trim().ToLowerInvariant();
            }

            Alarm = new ParameterAlarm(xml.GetElement("alarm") ?? dummy);
            SNMP = new ParameterSNMP(xml.GetElement("snmp") ?? dummy);
            Display = new ParameterDisplay(xml.GetElement("display") ?? dummy);
            Measurement = new ParameterMeasurement(xml.GetElement("measurement") ?? dummy);
            Information = new ParameterInformation(xml.GetElement("information") ?? dummy);
            Interprete = new ParameterInterprete(xml.GetElement("interprete") ?? dummy);

            //check if this parameter is duplicated
            if (xml.TryGetAttribute("duplicateAs", out var duplicateAs))
            {
                var attval = duplicateAs.Value;

                string[] sParts = attval.Split(',');

                foreach (var item in sParts)
                {
                    int dupId = 0;
                    if (Int32.TryParse(item, out dupId))
                    {
                        _duplicateAsIDs.Add(dupId);
                    }
                }
            }


            //check if the parameter is exported -> means part of a DVE protocol.
            if (xml.TryGetAttribute("export", out var export))
            {
                try
                {
                    var attval = export.Value;
                    if (String.Equals(attval, "true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SetExportStatus(true, true);
                    }
                    else
                    {
                        //the regex is used to validate the input string -> reason: it is possible that the user says export="todo:add numbers", in this case we don't want our parser to generate errors, but just to ignore it for now.
                        //65535 should be the max value -> it is allowed to limit the max number of digits in the regex. (if it would become to big it could also be impossible to parse)
                        var regexparser = new Regex("([0-9]{1,5})", RegexOptions.None);
                        _export.AddRange(
                            (from table in regexparser.Matches(attval).Cast<Match>()
                             let pid = Convert.ToInt32(table.Value)
                             where _export.Contains(pid) == false
                             select pid));
                    }
                }
                catch {/*ignore invalid values.*/}
            }

            if (xml.TryGetAttribute("trending", out var trending))
            {
                var attval = trending.Value;
                IsTrending = String.Equals(attval, "true", StringComparison.InvariantCultureIgnoreCase);
            }
            else if (this.Type == "read" || this.Type == "read bit") // trending is default enabled for read parameters
            {
                IsTrending = true;
            }

            if (IsTable)
            {
                var xmlArrayOptions = xml.GetElement("arrayoptions");
                if (xmlArrayOptions != null)
                {
                    IndexColumnIdx = Convert.ToInt32(xmlArrayOptions.GetNonEmptyAttribute("index", "-1"));
                    DisplayColumnIdx = Convert.ToInt32(xmlArrayOptions.GetNonEmptyAttribute("displaycolumn", "-1"));
                }
                else
                {
                    IndexColumnIdx = -1;
                    DisplayColumnIdx = -1;
                }

                foreach (var item in ColumnOption.ParseColumns(xml, SNMP.Enabled))
                {
                    ColumnOptions.Add(item);
                }
                //var xmlColumnOptions = xml.GetElements("arrayoptions/columnoption");
                //foreach (var e in xmlColumnOptions)
                //{
                //    try { ColumnOptions.Add(new ColumnOption(e)); }
                //    catch { }
                //} 
            }

        }

        public XmlElement Node { get; private set; }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        private readonly List<int> _duplicateAsIDs = new List<int>();
        public IList<int> DuplicateAsIDs { get { return _duplicateAsIDs; } }

        public ParameterInterprete Interprete { get; set; }
        public ParameterMeasurement Measurement { get; set; }
        public ParameterDisplay Display { get; set; }
        public ParameterAlarm Alarm { get; set; }
        public ParameterSNMP SNMP { get; set; }
        public ParameterInformation Information { get; set; }


        public string Type { get; set; } // read, write, array, fixed, dummy, bus, group, ...

        /// <summary>
        /// Returns true if the type is "array" and IsArray returns false
        /// </summary>
        public bool IsTable { get { return Type == "array" && !IsMatrix; } }

        public int IndexColumnIdx { get; set; }
        public int DisplayColumnIdx { get; set; }

        /// <summary>
        /// Returns true if measure ment type is "matrix"
        /// </summary>
        public bool IsMatrix { get { return Measurement.Type == "matrix"; } }

        private readonly List<ColumnOption> _columnOptions = new List<ColumnOption>();
        public IList<ColumnOption> ColumnOptions { get { return _columnOptions; } }

        public bool IsTrending { get; set; }

        private readonly List<int> _export = new List<int>();
        /// <summary>
        /// returns a list with parameter ids containing all tables to which the parameter is exported to.
        /// the list will be empty when the parameter is exported to ALL derived elements or when the parameter is not exported to any parameter.
        /// Use the "IsExport" or "ExportToAll" properties to check which case is correct.
        /// </summary>
        public IList<int> Export { get { return _export; } }
        /// <summary>
        /// returns true if there is an "export" attribute defined for the parameter.
        /// </summary>
        public bool IsExport { get; private set; }
        /// <summary>
        /// returns true if 'export="true"' is defined for the parameter.
        /// </summary>
        public bool ExportToAll { get; private set; }
        public void SetExportStatus(bool export = true, bool toall = true, List<int> targets = null)
        {
            /* this function makes sure that parameters linked to the export tag stay consistent.
             * This means that if IsExport is false, then an empty list should be returned when you request "Export" and "ExportToAll" should be false.
             * Also when "ExportToAll" is true, "Export" should return an empty list and "IsExport" should be true.
             *  -> this to make sure that if you ask somewhere else "if(ExportToAll){ bla bla bla; }" this will always do the same.
             */


            IsExport = export;
            if (!IsExport)
            {
                _export.Clear();
                ExportToAll = false;
            }
            else
            {
                ExportToAll = toall;
                if (ExportToAll)
                { _export.Clear(); }
                else if (targets != null)
                {
                    _export.RemoveAll(id => !targets.Contains(id));
                    _export.AddRange(targets.Where(id => !_export.Contains(id)));
                }
            }
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1}", ID, Name);
        }
    }
}
