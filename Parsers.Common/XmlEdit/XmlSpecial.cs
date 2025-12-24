namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Net;

    /// <summary>
    /// Represents a special, self-closing XML tag like Comments and CDATA.
    /// </summary>
    public class XmlSpecial : XmlNode<Xml.XmlSpecial, XmlSpecial>
    {
        private string _innerText;

        public XmlSpecial(Xml.XmlSpecial data)
            : base(data)
        {

        }

        public XmlSpecial(string innerText) : base(null)
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
                    this.HasChanges = true;
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

        public override XmlSpecial Clone()
        {
            return new XmlSpecial(_innerText);
        }

        public override string GetXml()
        {
            return "<" + WebUtility.HtmlEncode(InnerText ?? "") + ">";
        }

        public override string ToString()
        {
            return "< ??? >";
        }
    }
}