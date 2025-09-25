namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System.Collections.Generic;

    public class InterfaceType
    {
        public static readonly InterfaceType In = new InterfaceType("In");
        public static readonly InterfaceType Out = new InterfaceType("Out");
        public static readonly InterfaceType InOut = new InterfaceType("InOut");

        public InterfaceType() { }
        public InterfaceType(string value)
        {
            Value = (value ?? "").ToLower();
            DisplayValue = value;
        }
        public InterfaceType(string value, string displayValue)
        {
            Value = (value ?? "");
            DisplayValue = displayValue;
        }

        public string Value { get; set; }

        public string DisplayValue { get; set; }

        public static List<InterfaceType> AllInterfaceTypes { get; } = new List<InterfaceType> { In, Out, InOut };

        public static InterfaceType Parse(string value)
        {
            switch (value?.ToLowerInvariant())
            {
                case "in": return In;
                case "out": return Out;
                case "inout": return InOut;
            }

            return null;
        }

        public static IEnumerable<InterfaceType> GetTypes()
        {
            foreach (var type in AllInterfaceTypes)
            {
                yield return type;
            }
        }
    }
}
