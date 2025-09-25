namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;

    /// <summary>
    /// Represents an XML comment tag.
    /// </summary>
    public class XmlComment : XmlNode<Xml.XmlComment, XmlComment>
    {
        private string _innerText;

        public XmlComment(Xml.XmlComment data) : base(data)
        {
            _innerText = data.InnerText;
        }

        public XmlComment(string innerText) : base(null)
        {
            InnerText = innerText ?? throw new ArgumentNullException(nameof(innerText));
        }

        public string InnerText
        {
            get => _innerText;
            set
            {
                if (_innerText != value)
                {
                    _innerText = value;
                    HasChanges = true;
                }
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

        public override XmlComment Clone()
        {
            return new XmlComment(_innerText);
        }

        public override string GetXml()
        {
            // Comment content must not be encoded!
            return "<!--" + (InnerText ?? "") + "-->";
        }

        public override string ToString()
        {
            return "<!-- ... -->";
        }
    }
}