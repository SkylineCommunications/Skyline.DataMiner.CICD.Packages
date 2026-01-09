namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;

    /// <summary>
    /// Represents an XML CDATA tag.
    /// </summary>
    public class XmlCDATA : XmlNode<Xml.XmlCDATA, XmlCDATA>
    {
        private string _innerText;

        public XmlCDATA(Xml.XmlCDATA data) : base(data)
        {
            _innerText = data.InnerText;
        }

        public XmlCDATA(string innerText) : base(null)
        {
            InnerText = innerText ?? throw new ArgumentNullException(nameof(innerText));
        }

        public XmlCDATA(): this("")
        {

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

        public override XmlCDATA Clone()
        {
            return new XmlCDATA(_innerText);
        }

        public override string GetXml()
        {
            // CDATA content must not be encoded!
            return "<![CDATA[" + (InnerText ?? "") + "]]>";
        }

        public override string ToString()
        {
            return "<![CDATA[ ... ]]>";
        }
    }
}