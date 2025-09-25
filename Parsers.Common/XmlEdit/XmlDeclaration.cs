namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents an XML declaration tag.
    /// </summary>
    public class XmlDeclaration : XmlNode<Xml.XmlDeclaration, XmlDeclaration>
    {
        public XmlDeclaration(Xml.XmlDeclaration data) : base(data)
        {
            Attributes = new AttributeList(this, data.Token.GetAttributes());
        }

        public XmlDeclaration(string version) : this(version, null, null) { }

        public XmlDeclaration(string version, string encoding, string standalone) : base(null)
        {
            Attributes = new AttributeList(this);

            Version = version ?? throw new ArgumentNullException(nameof(version));
            Encoding = encoding;
            Standalone = standalone;
        }

        public string Version
        {
            get
            {
                var attr = Attributes.FirstOrDefault(x => String.Equals(x.Name, "version", StringComparison.OrdinalIgnoreCase));
                return attr?.Value;
            }
            set
            {
                if (Version == value)
                {
                    return;
                }

                var attr = Attributes.FirstOrDefault(x => String.Equals(x.Name, "version", StringComparison.OrdinalIgnoreCase));

                if (value != null)
                {
                    if (attr == null)
                    {
                        attr = new XmlAttribute("version", "");
                        Attributes.Add(attr);
                    }

                    attr.Value = value;
                }
                else if (attr != null)
                {
                    Attributes.Remove(attr);
                }

                this.HasChanges = true;
            }
        }

        public string Encoding
        {
            get
            {
                var attr = Attributes.FirstOrDefault(x => String.Equals(x.Name, "encoding", StringComparison.OrdinalIgnoreCase));
                return attr?.Value;
            }
            set
            {
                if (Encoding == value)
                {
                    return;
                }

                var attr = Attributes.FirstOrDefault(x => String.Equals(x.Name, "encoding", StringComparison.OrdinalIgnoreCase));

                if (value != null)
                {
                    if (attr == null)
                    {
                        attr = new XmlAttribute("encoding", "");
                        Attributes.Add(attr);
                    }
                    attr.Value = value;
                }
                else if (attr != null)
                {
                    Attributes.Remove(attr);
                }

                this.HasChanges = true;
            }
        }

        public string Standalone
        {
            get
            {
                var attr = Attributes.FirstOrDefault(x => String.Equals(x.Name, "standalone", StringComparison.OrdinalIgnoreCase));
                return attr?.Value;
            }
            set
            {
                if (Standalone == value)
                {
                    return;
                }

                var attr = Attributes.FirstOrDefault(x => String.Equals(x.Name, "standalone", StringComparison.OrdinalIgnoreCase));

                if (value != null)
                {
                    if (attr == null)
                    {
                        attr = new XmlAttribute("standalone", "");
                        Attributes.Add(attr);
                    }
                    attr.Value = value;
                }
                else if (attr != null)
                {
                    Attributes.Remove(attr);
                }

                this.HasChanges = true;
            }
        }

        public override void Format()
        {
            Helper.FormatTextBeforeNode(this);
            Helper.FormatTextAfterNode(this);
            FormatInternal();
        }

        internal override void FormatInternal()
        {
            // nothing to format inside this node
        }

        public override XmlDeclaration Clone()
        {
            return new XmlDeclaration(Version, Encoding, Standalone);
        }

        public override string GetXml()
        {
            string version = Version;
            string encoding = Encoding;
            string standalone = Standalone;

            StringBuilder sb = new StringBuilder();

            sb.Append("<?xml");
            sb.Append(" version=\"" + version + "\"");

            if (encoding != null)
            {
                sb.Append(" encoding=\"" + encoding + "\"");
            }

            if (standalone != null)
            {
                sb.Append(" standalone=\"" + standalone + "\"");
            }

            sb.Append(" ?>");

            return sb.ToString();
        }

        public override string ToString()
        {
            return "<?xml version=\"" + Version + "\" ... ?>";
        }
    }
}