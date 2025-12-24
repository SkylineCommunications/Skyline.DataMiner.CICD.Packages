namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Linq;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;

    /// <summary>
    /// Represents the root of the XML document.
    /// A valid XML document will have exactly one XmlElement in its Children.
    /// </summary>
    public class XmlDocument : XmlContainer<Xml.XmlDocument, XmlDocument>
    {
        public XmlDocument(Xml.XmlDocument data) : base(data)
        {
        }

        public XmlDocument() : base()
        {
        }

        public XmlDeclaration Declaration
        {
            get
            {
                if (Children.Count > 0 && Children[0] is XmlDeclaration decl)
                {
                    return decl;
                }

                return null;
            }
            set
            {
                if (Children.Count > 0 && Children[0] is XmlDeclaration)
                {
                    if (value != null)
                    {
                        Children[0] = value;
                    }
                    else
                    {
                        Children.RemoveAt(0);
                    }
                }
                else if (value != null)
                {
                    Children.Insert(0, value);
                }
            }
        }

        /// <summary>
        /// Gets the root element of the XML Tree for this document.
        /// </summary>
        public XmlElement Root => Children.OfType<XmlElement>().FirstOrDefault();

        public override XmlDocument Clone()
        {
            var e = new XmlDocument();

            e.Children.AddRange(Children.Select(node => node.Clone<XmlNode>()));

            return e;
        }

        public override void Format()
        {
            FormatInternal();
        }

        public override string GetXml()
        {
            return String.Join(String.Empty, Children.Select(node => node.GetXml()));
        }
    }
}