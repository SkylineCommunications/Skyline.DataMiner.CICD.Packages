namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Net;

    /// <summary>
    /// Represents text between tags (whitespace, inner text, surrounding text) or an invalid token.
    /// </summary>
    public class XmlText : XmlNode<Xml.XmlText, XmlText>
    {
        private string _text;

        public XmlText(Xml.XmlText data) : base(data)
        {
            _text = data.Text;
        }

        public XmlText(string text) : base(null)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    HasChanges = true;
                }
            }
        }

        public override void Format()
        {
            FormatInternal();
        }

        internal override void FormatInternal()
        {
            // don't format text
        }

        public override XmlText Clone()
        {
            return new XmlText(_text);
        }

        public override string GetXml()
        {
            return WebUtility.HtmlEncode(_text ?? "");
        }

        public override string ToString()
        {
            return "TEXT: " + Text;
        }
    }
}