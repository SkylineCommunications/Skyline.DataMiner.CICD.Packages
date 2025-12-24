namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Net;

    public class XmlAttribute
    {
        private bool _hasChanges;

        private string _name;
        private string _value;

        /// <summary>
        /// Contains the parent node.
        /// </summary>
        public XmlNode ParentNode { get; internal set; }

        public Xml.XmlAttribute Data { get; }

        /// <summary>
        /// When true, this attribute has changes.
        /// </summary>
        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                _hasChanges = value;
                if (_hasChanges && ParentNode != null)
                {
                    ParentNode.HasChanges = true;
                }
            }
        }

        public XmlAttribute(Xml.XmlAttribute data)
        {
            Data = data;

            _name = data.Name;
            _value = data.Value;
        }

        public XmlAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public XmlAttribute(string name, int value)
        {
            Name = name;
            Value = Convert.ToString(value);
        }

        /// <summary>
        /// The name of this attribute.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException(nameof(value));
                }

                value = value.Trim();
                if (_name != value)
                {
                    _name = value;
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// The value of this attribute.
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    HasChanges = true;
                }
            }
        }

        public XmlAttribute Clone()
        {
            return new XmlAttribute(_name, _value);
        }

        internal string GetXml()
        {
            return Name + "=\"" + WebUtility.HtmlEncode(Value ?? "") + "\"";
        }

        public override string ToString()
        {
            return "ATTRIBUTE: " + Name + "=\"" + Value + "\"";
        }
    }
}