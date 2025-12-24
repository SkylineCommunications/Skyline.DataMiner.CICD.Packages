namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class ColumnOption
    {
        public ColumnOption(XmlElement xml)
        {
            Index = Convert.ToInt32(xml.GetAttributeValue("idx"));
            ParameterID = Convert.ToInt32(xml.GetAttributeValue("pid"));

            Type = xml.GetAttributeValue("type");
            Options = xml.GetAttributeValue("options");

            IsOldTypeColumnOption = false;

            Node = xml;
        }

        public ColumnOption(int idx, int pid, string type, string options, bool isOldTypeColumnOption)
        {
            Index = idx;
            ParameterID = pid;

            Type = type;
            Options = options;

            IsOldTypeColumnOption = isOldTypeColumnOption;
        }

        public XmlElement Node { get; private set; }

        public int Index { get; set; }

        public int ParameterID { get; set; }

        public string Type { get; set; }

        public string Options { get; set; }

        public bool IsOldTypeColumnOption { get; set; }

        /// <summary>
        /// Parses all the columns from a valid xml node.
        /// </summary>
        /// <param name="xml">A Param node, or a Param\Type node, or a Param\ArrayOptions node.</param>
        /// <returns></returns>
        public static IList<ColumnOption> ParseColumns(XmlElement xml, bool isSNMPTable)
        {
            List<ColumnOption> cols = new List<ColumnOption>();

            var xmlColumnOptions = xml.GetElements("ArrayOptions/columnoption");
            foreach (var e in xmlColumnOptions)
            {
                try
                { cols.Add(new ColumnOption(e)); }
                catch { }
            }

            var typenode = xml.GetElement("Type");
            if (typenode != null)
            {
                try
                {
                    var idattribute = typenode.GetAttributeValue("id");
                    if (idattribute != null)
                    {
                        int counter = 0;
                        foreach (string PID in idattribute.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            while (cols.Any(c => c.Index == counter)) // check if there is allready a column with this idx
                            {
                                counter++;
                            }

                            cols.Add(new ColumnOption(counter++, Convert.ToInt32(PID), isSNMPTable ? "snmp" : "custom", "", true));
                        }
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Error in static ColumnOptions.ParseColumns(XML Type Node): " + ex.Message);
#endif
                }
            }

            return cols.ToList();
        }
    }
}
