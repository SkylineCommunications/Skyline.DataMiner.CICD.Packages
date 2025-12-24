namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a normal XML element tag.
    /// </summary>
    public class XmlElement : XmlContainer
    {
        public override bool IsSubtreeValid { get; internal set; }

        public override int LastCharOffset => (TokenClose ?? Token).OffsetEnd;

        public virtual string Name { get; set; }

        /// <summary>
        /// Will always be null, except when Token.TagType == ElementTagType.Opening
        /// </summary>
        public Token TokenClose { get; set; }

        /// <summary>
        /// Only returns the inner text if this is a simple element like &lt;Name&gt;abc&lt;/Name&gt; (or with CDATA). 
        /// Returns null if the element has child elements or mixed content.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public virtual string InnerText
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
                        else if (child is XmlComment)
                        {
                            continue; // ignore comment parts
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
        }

        private IIndexer<string, XmlAttribute> _attributeIndexer;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IIndexer<string, XmlAttribute> Attribute
        {
            get
            {
                return _attributeIndexer ?? (_attributeIndexer = new Indexer<XmlElement, string, XmlAttribute>(
                    this, (s, k) =>
                    {
                        TryGetAttribute(k, out XmlAttribute a);
                        return a;
                    }));
            }
        }

        #region Public Methods

        public virtual IEnumerable<XmlAttribute> GetAttributes()
        {
            return Token.GetAttributes();
        }

        public IEnumerable<XmlAttribute> GetAttributes(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return GetAttributes().Where(e => String.Equals(e.Name, name));
        }

        public bool TryGetAttribute(string name, out XmlAttribute attribute)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            attribute = GetAttributes(name).FirstOrDefault();
            return attribute != null;
        }

        /// <summary>
        /// Checks if the attribute exists and if so the value is returned. Else a default value is returned. 
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="defaultValue">The value to be returned when the attribute does not exist.</param>
        /// <returns></returns>
        public string GetAttributeValue(string name, string defaultValue = null)
        {
            if (TryGetAttribute(name, out var attribute))
            {
                return attribute.Value;
            }

            return defaultValue;
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

        public override string GetXml()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<");
            sb.Append(Name);

            foreach (var a in GetAttributes())
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

        public override string ToString()
        {
            bool hasAttributes = Token.ElementAttributes != null && Token.ElementAttributes.Count > 0;
            bool hasChildren = Children.Count > 0;

            if (Token.TagType == ElementTagType.SelfContained)
            {
                return String.Format("<{0}{1}/>", Name, hasAttributes ? " ... " : "");
            }

            return String.Format("<{0}{1}>{2}</{0}>", Name, hasAttributes ? " ... " : "", hasChildren ? " ... " : "");
        }

        #endregion
    }
}