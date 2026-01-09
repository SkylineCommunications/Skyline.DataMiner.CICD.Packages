namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    /// <summary>
    /// Represents a Protocol.Params.Param.Display.Positions.Position element.
    /// </summary>
    public class Position
    {
        private static readonly XmlElement Dummy = new XmlElement();

        public Position(string page, int row, int column)
        {
            Page = page;
            Row = row;
            Column = column;
        }

        public Position(XmlElement xml)
        {
            var xPage = (xml.GetElement("page") ?? Dummy);

            Page = xPage.InnerText;
            Row = Convert.ToInt32((xml.GetElement("row") ?? Dummy).InnerText);
            Column = Convert.ToInt32((xml.GetElement("column") ?? Dummy).InnerText);

            MeasType = xPage.GetAttributeValue("measType");

            if (String.IsNullOrEmpty(Page))
            {
                throw new Exception("Page name cannot be empty");
            }
        }

        public string Page { get; set; }

        public int Row { get; set; }

        public int Column { get; set; }

        public string MeasType { get; set; }

        public override string ToString()
        {
            return $"{Page}/{Row}/{Column}";
        }
    }
}
