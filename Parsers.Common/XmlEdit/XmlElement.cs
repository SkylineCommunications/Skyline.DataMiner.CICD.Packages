namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;

    /// <summary>
    /// Represents a normal XML element tag.
    /// </summary>
    public class XmlElement : XmlContainer<Xml.XmlElement, XmlElement>
    {
        private string _name;
        private IIndexer<string, XmlAttribute> _attributeIndexer;

        public XmlElement(Xml.XmlElement data) : base(data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Attributes = new AttributeList(this, data.GetAttributes());

            _name = data.Name;
        }

        public XmlElement(string name) : this(name, null) { }

        public XmlElement(string name, string innerText) : base()
        {
            Attributes = new AttributeList(this);

            Name = name;
            InnerText = innerText;
        }

        /// <summary>
        /// The name of this XML element.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                string newName = (value ?? "").Trim();

                if (String.IsNullOrWhiteSpace(newName))
                {
                    throw new ArgumentException(nameof(value));
                }

                if (_name != newName)
                {
                    _name = newName;
                    this.HasChanges = true;
                }
            }
        }

        public IIndexer<string, XmlAttribute> Attribute
        {
            get
            {
                if (_attributeIndexer == null)
                {
                    _attributeIndexer = new Indexer<AttributeList, string, XmlAttribute>(Attributes, (s, k) => s.FirstOrDefault(x => String.Equals(x.Name, k, StringComparison.OrdinalIgnoreCase)));
                }

                return _attributeIndexer;
            }
        }

        public string GetAttributeValue(string name, string defaultValue = null)
        {
            var attribute = Attribute[name];
            return attribute != null ? attribute.Value : defaultValue;
        }

        /// <summary>
        /// Only returns the inner text if this is a simple element like &lt;Name&gt;abc&lt;/Name&gt; (or with CDATA).
        /// Returns null if the element has child elements or mixed content.
        /// </summary>
        public string InnerText
        {
            get
            {
                if (Children.Count == 0)
                {
                    return String.Empty;
                }

                if (Children.Count == 1 && Children[0] is XmlText t)
                {
                    return t.Text;
                }

                if (Children.Count == 1 && Children[0] is XmlCDATA c)
                {
                    return c.InnerText;
                }

                if (Children.Count >= 1)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var child in Children)
                    {
                        if (child is XmlText t2)
                        {
                            if (!String.IsNullOrWhiteSpace(t2.Text))
                            {
                                sb.Append(t2.Text);
                            }
                        }
                        else if (child is XmlCDATA c2)
                        {
                            sb.Append(c2.InnerText);
                        }
                        else
                        {
                            return null;
                        }
                    }

                    return sb.ToString();
                }

                return null;
            }
            set
            {
                if (InnerText == value)
                {
                    return;
                }

                if (value == null)
                {
                    Children.Clear();
                }
                else if (Children.Count == 1 && Children[0] is XmlText t)
                {
                    t.Text = value;
                }
                else
                {
                    List<XmlCDATA> cdataChildren = new List<XmlCDATA>();
                    List<XmlNode> otherChildren = new List<XmlNode>();

                    foreach (var child in Children)
                    {
                        if (child is XmlCDATA c)
                        {
                            cdataChildren.Add(c);
                        }
                        else
                        {
                            otherChildren.Add(child);
                        }
                    }

                    if (cdataChildren.Count == 1 && otherChildren.All(c => c is XmlText t2 && String.IsNullOrWhiteSpace(t2.Text)))
                    {
                        cdataChildren[0].InnerText = value;
                    }
                    else
                    {
                        Children.Clear();
                        Children.Add(new XmlText(value));
                    }
                }
            }
        }

        public override void Format()
        {
            Helper.FormatTextBeforeNode(this);
            Helper.FormatTextAfterNode(this);

            // format children
            FormatInternal();
        }

        public override XmlElement Clone()
        {
            var e = new XmlElement(_name);

            e.Attributes.AddRange(Attributes.Select(x => x.Clone()));
            e.Children.AddRange(Children.Select(x => x.Clone<XmlNode>()));

            return e;
        }

        public override string GetXml()
        {
            StringBuilder sb = new StringBuilder(64);

            sb.Append("<");
            sb.Append(Name);

            foreach (var a in Attributes)
            {
                sb.Append(" " + a.GetXml());
            }

            if (Children.Count == 0)
            {
                sb.Append(" />");
            }
            else
            {
                sb.Append(">");

                foreach (var c in Children)
                {
                    sb.Append(c.GetXml());
                }

                sb.Append("</" + Name + ">");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the path from the root node to this <see cref="XmlElement"/>;
        /// </summary>
        public IEnumerable<XmlElement> GetPath()
        {
            var stack = new Stack<XmlElement>();
            stack.Push(this);
            while (stack.Peek().ParentNode is XmlElement parent)
            {
                stack.Push(parent);
            }

            return stack;
        }

        public override string ToString()
        {
            bool hasAttributes = Attributes.Count > 0;
            bool hasChildren = base.HasChildren;

            if (!hasChildren)
            {
                return String.Format("<{0}{1}/>", Name, hasAttributes ? " ... " : "");
            }

            return String.Format("<{0}{1}>{2}</{0}>", Name, hasAttributes ? " ... " : "", " ... ");
        }
    }
}