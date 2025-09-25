namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class ExportRule
    {
        //STATICS
        public static IList<ExportRule> ParseExportRules(XmlElement source)
        {
            List<ExportRule> rules = new List<ExportRule>();

            //no rules can be returned if there is no source -> happens when .GetElement("ExportRules") is called on a protocol with no export rules.
            if (source == null)
            {
                return rules;
            }

            if (source.Token.ElementName == "ExportRules")
            {
                foreach (var item in source.GetElements("ExportRule"))
                {
                    rules.Add(new ExportRule(item));
                }
            }
            else if (source.Token.ElementName == "Protocol")
            {
                rules.AddRange(ParseExportRules(source.GetElement("ExportRules")));
            }
            else
            {
                throw new ArgumentException("Unexpected element name: " + source.Token.ElementName + ".", nameof(source));
            }

            return rules;
        }

        //VARS
        private readonly List<int> _targetIDs = new List<int>();

        //INIT
        /// <summary>
        /// parses the information from an exportRule tag.
        /// </summary>
        /// <param name="xml">A Protocol\ExportRules\ExportRule tag.</param>
        public ExportRule(XmlElement xml)
        {
            //parse attributes:
            Name = xml.GetAttributeValue("name");
            Value = xml.GetAttributeValue("value");
            Attribute = xml.GetAttributeValue("attribute");
            Tag = xml.GetAttributeValue("tag");
            RegEx = xml.GetAttributeValue("regEx");
            WhereTag = xml.GetAttributeValue("whereTag");
            WhereValue = xml.GetAttributeValue("whereValue");
            WhereAttribute = xml.GetAttributeValue("whereAttribute");

            //special attribute -> table="*" or table="number" or table="number;number;...;number"
            string target = xml.GetAttributeValue("table");
            switch (target)
            {
                case null: /*potentional error -> should not occure.*/
                    break;
                case "*":
                    _targetIDs.RemoveAll(remove => true);
                    break;
                default:
                    //input validator -> if export="al452;54;244a12;21" ingegeven wordt zal deze functie parsen: 452, 54, 244, 12 & 21
                    var check = new Regex("([0-9]+)");
                    //add all pids to the list (if they aren't in it already!!)
                    _targetIDs.AddRange(
                        (from table in check.Matches(target).Cast<Match>()
                         let pid = Convert.ToInt32(table.Value)
                         where _targetIDs.Contains(pid) == false
                         select pid));
                    break;
            }
        }


        public string Name { get; set; }

        public bool TargetsAll => TargetTable.Count == 0;

        public IList<int> TargetTable => _targetIDs;

        public string Tag { get; set; }

        public string Attribute { get; set; }

        public string Value { get; set; }

        public string RegEx { get; set; }

        public string WhereTag { get; set; }

        public string WhereValue { get; set; }

        public string WhereAttribute { get; set; }
    }
}
