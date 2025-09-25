namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class ParameterDisplay
    {
        public bool RTDisplay { get; set; }
        public string Units { get; set; }
        public int Decimals { get; set; }
        public string RangeLow { get; set; }
        public string RangeHigh { get; set; }

        private readonly List<Position> _positions = new List<Position>();
        public IList<Position> Positions { get { return _positions; } }

        public ParameterDisplay(XmlElement xml)
        {
            var xml_rtdisplay = xml.GetElement("rtdisplay");
            if (xml_rtdisplay != null)
            {
                RTDisplay = xml_rtdisplay.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            var xml_units = xml.GetElement("units");
            if (xml_units != null)
            {
                Units = xml_units.InnerText;
            }

            var xml_decimals = xml.GetElement("decimals");
            if (xml_decimals != null)
            {
                try
                {
                    Decimals = Convert.ToInt32(xml_decimals.InnerText);
                }
                catch (Exception) { }
            }

            var xml_rangelow = xml.GetElement("range/low");
            if (xml_rangelow != null)
            {
                try
                {
                    RangeLow = xml_rangelow.InnerText;
                }
                catch (Exception) { }
            }

            var xml_rangehigh = xml.GetElement("range/high");
            if (xml_rangehigh != null)
            {
                try
                {
                    RangeHigh = xml_rangehigh.InnerText;
                }
                catch (Exception) { }
            }

            var xmlPositions = xml.GetElements("positions/position");
            foreach (var e in xmlPositions)
            {
                try
                { Positions.Add(new Position(e)); }
                catch { }
            }
        }
    }
}
