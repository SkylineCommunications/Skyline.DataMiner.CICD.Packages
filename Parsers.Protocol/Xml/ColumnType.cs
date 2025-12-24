namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System.Collections.Generic;

    public class ColumnType
    {
        public static readonly ColumnType AutoIncrement = new ColumnType("AutoIncrement");
        public static readonly ColumnType Concatenation = new ColumnType("Concatenation");
        public static readonly ColumnType Custom = new ColumnType("Custom");
        public static readonly ColumnType DisplayKey = new ColumnType("DisplayKey");
        public static readonly ColumnType Index = new ColumnType("Index");
        public static readonly ColumnType Retrieved = new ColumnType("Retrieved");
        public static readonly ColumnType SNMP = new ColumnType("SNMP");
        public static readonly ColumnType State = new ColumnType("State");
        public static readonly ColumnType ViewTableKey = new ColumnType("viewTableKey", "View Table Key");

        public ColumnType(string value)
        {
            Value = (value ?? "").ToLower();
            DisplayValue = value;
        }
        public ColumnType(string value, string displayValue)
        {
            Value = (value ?? "");
            DisplayValue = displayValue;
        }

        public string Value { get; set; }

        public string DisplayValue { get; set; }

        public static List<ColumnType> AllColumnTypes { get; } = new List<ColumnType> { AutoIncrement, Concatenation, Custom, DisplayKey, Index, Retrieved, SNMP, State, ViewTableKey };

        public static IEnumerable<ColumnType> GetTypes()
        {
            foreach (var type in AllColumnTypes)
            {
                yield return type;
            }
        }
    }
}
