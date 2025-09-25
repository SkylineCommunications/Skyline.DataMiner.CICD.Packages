namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    using System.Net;

    public class XmlAttribute
    {
        protected readonly Token _token;
        protected readonly int _nameOffset;
        protected readonly int _nameLength;
        protected readonly int _valueOffset;
        protected readonly int _valueLength;

        public XmlAttribute()
        { }

        public XmlAttribute(Token token, int nameOffset, int nameLength, int valueOffset, int valueLength)
        {
            _token = token;
            _nameOffset = nameOffset;
            _nameLength = nameLength;
            _valueOffset = valueOffset;
            _valueLength = valueLength;
        }

        public virtual string Name => _token?.GetTextByRelativePos(_nameOffset, _nameLength);

        public virtual string Value
        {
            get
            {
                if (_token == null) return null;

                string value = _token.GetTextByRelativePos(_valueOffset, _valueLength);
                return WebUtility.HtmlDecode(value);
            }
        }

        public int NameOffset => _token.Offset + _nameOffset;

        public int NameOffsetEnd => _token.Offset + _nameOffset + _nameLength;

        public int NameLength => _nameLength;

        public int ValueOffset => _token.Offset + _valueOffset;

        public int ValueOffsetEnd => _token.Offset + _valueOffset + _valueLength;

        public int ValueLength => _valueLength;

        public int TotalLength => ValueOffset + ValueLength - NameOffset + 1;

        public static XmlAttribute Empty { get; } = new XmlAttribute();

        public virtual string GetXml()
        {
            return $"{Name}=\"{WebUtility.HtmlEncode(Value)}\"";
        }

        public override string ToString()
        {
            return "ATTRIBUTE: " + Name + "=\"" + Value + "\"";
        }
    }
}