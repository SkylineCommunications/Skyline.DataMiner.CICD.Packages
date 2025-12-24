namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using FileSystem;

    /// <summary>
    /// Represents the root of the XML document. This is the only type of XmlNode that has no associated Token.
    /// A valid XML document will have exactly one XmlElement in its Children.
    /// </summary>
    public class XmlDocument : XmlContainer
    {
        public override bool IsSubtreeValid { get { return Children.All(x => x.IsSubtreeValid); } }

        public override int FirstCharOffset => 0;

        public override int LastCharOffset => TotalLength - 1;

        public new int TotalLength { get; set; }

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
        }

        /// <summary>
        /// Gets the root element of the XML Tree for this document.
        /// </summary>
        public XmlElement Root => Children.OfType<XmlElement>().FirstOrDefault();

        #region Public Methods

        public static XmlDocument Parse(string xml)
        {
            Parser parser = new Parser(xml);
            return parser.Document;
        }

        public static XmlDocument Load(string path)
        {
            string xml = FileSystem.Instance.File.ReadAllText(path, Encoding.UTF8);
            return Parse(xml);
        }

        /// <inheritdoc />
        public override string GetXml()
        {
            var sb = new StringBuilder();

            foreach (var c in Children)
            {
                sb.Append(c.GetXml());
            }

            return sb.ToString();
        }

        #endregion

        #region Internal Methods

        internal TokenList GetTokenList()
        {
            TokenList tokens = new TokenList();

            Stack<object> stack = new Stack<object>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                object obj = stack.Pop();

                if (obj == null) // ignore
                {
                }
                else if (obj is Token token) // for closing tag of elements
                {
                    tokens.Add(token);
                }
                else if (obj is XmlPlaceholder placeholder) // represent placeholder nodes with a placeholder token
                {
                    tokens.Add(TokenPlaceholder.Create(placeholder));
                }
                else if (obj is XmlNode node)
                {
                    if (!(node is XmlDocument))
                    {
                        tokens.Add(node.Token);
                    }

                    if (node is XmlElement element && element.Token.TagType != ElementTagType.SelfContained) // don't forget the closing token on elements
                    {
                        stack.Push(element.TokenClose);
                    }

                    if (node is XmlContainer container)
                    {
                        foreach (XmlNode child in container.Children.Reverse()) // child nodes
                        {
                            stack.Push(child);
                        }
                    }
                }
            }

            return tokens;
        }

        #endregion
    }
}