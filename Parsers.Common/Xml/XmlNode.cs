namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    public abstract class XmlNode
    {
        protected XmlNode()
        {
        }

        /// <summary>
        /// Returns the (first) Token that corresponds with this node.
        /// </summary>
        public Token Token { get; set; }

        /// <summary>
        /// Contains the parent node. This will only be null for XmlDocument.
        /// </summary>
        public virtual XmlContainer ParentNode { get; set; }

        /// <summary>
        /// Returns the position in the textbuffer where the first character of this node is located, e.g. the opening '&lt;'
        /// </summary>
        public virtual int FirstCharOffset => Token.Offset;

        /// <summary>
        /// Returns the position in the textbuffer where the last character of this node is located, e.g. the closing '&gt;'
        /// </summary>
        public virtual int LastCharOffset => Token.OffsetEnd;

        /// <summary>
        /// Returns the number of characters in the textbuffer for this node, e.g. 10 for "&lt;a&gt;xxx&lt;/a&gt;"
        /// </summary>
        public int TotalLength => LastCharOffset - FirstCharOffset + 1;

        /// <summary>
        /// When true, this node and its entire subtree represents a valid XML block that can be collapsed (ie. it contains no invalid element tags or invalid tokens).
        /// </summary>
        public virtual bool IsSubtreeValid
        {
            get => Token.Type != TokenType.Invalid;
            internal set => throw new System.NotSupportedException();
        }

        /// <summary>
        /// Returns the depth in the XML tree of this node. The XmlDocument root element has depth 0. (calculated)
        /// </summary>
        public int Depth => ParentNode == null ? 0 : ParentNode.Depth + 1;
        
        /// <summary>
        /// Returns true if the range of this node [FirstCharOffset...LastCharOffset] overlaps with the given range [x1...x2].
        /// </summary>
        public bool OverlapsWithRange(int x1, int x2)
        {
            return (FirstCharOffset >= x1 && FirstCharOffset <= x2) ||
                   (LastCharOffset >= x1 && LastCharOffset <= x2) ||
                   (FirstCharOffset <= x1 && LastCharOffset >= x2);
        }

        /// <summary>
        /// Returns the XML of this node (and children) as string.
        /// </summary>
        public abstract string GetXml();
    }
}