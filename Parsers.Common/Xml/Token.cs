namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;

    public class Token
    {
        #region Protected Fields

        protected StringBuilder _text;
        protected List<XmlAttribute> _attributes = null;
        protected int _nameOffset = 0;
        protected int _nameLength = 0;

        #endregion

        #region Public Properties

        public int Length { get; protected set; }
        public int Offset { get; protected set; }
        public int OffsetEnd => Offset + Length - 1;
        public string Text => _text.ToString(Offset, Length);

        public string ElementName
        {
            get
            {
                if (_nameLength <= 0)
                {
                    return String.Empty;
                }

                if (_nameOffset < 0)
                {
                    return "INVALID_OFFSET";
                }

                if (_text == null)
                {
                    return "TEXT_NULL";
                }

                if (_text.Length < _nameLength + _nameOffset + Offset)
                {
                    return "INVALID_LENGTH";
                }

                return _text.ToString(Offset + _nameOffset, _nameLength);
            }
        }
        public int NameOffset => Offset + _nameOffset;
        public int NameLength => _nameLength;

        public IReadOnlyList<XmlAttribute> ElementAttributes => _attributes?.AsReadOnly() ?? (IReadOnlyList<XmlAttribute>)Array.Empty<XmlAttribute>();

        public bool IsComment { get; private set; }
        public bool IsCDATA { get; private set; }
        public bool IsXmlDeclaration { get; private set; }
        public TokenType Type { get; private set; }
        public ElementTagType TagType { get; private set; }

        #endregion

        #region Constructor

        protected Token(StringBuilder text, int offset, int length)
        {
            _text = text;
            Offset = offset;
            Length = length;
        }

        #endregion

        #region Private Methods

        private bool ProcessContents(IErrorLogger logger)
        {
            string tag = Text;
            Type = TokenType.Invalid;

            bool startsWithLT = tag.Length > 0 && tag[0] == '<';
            bool endsWithGT = tag.Length > 0 && tag[tag.Length - 1] == '>';

            #region Error: Tag is opened but not closed
            if (startsWithLT && !endsWithGT)
            {
                logger.Log(Offset, "Tag is opened but not closed");
                return false;
            }
            #endregion

            #region Error: Tag is closed but not opened
            if (!startsWithLT && endsWithGT)
            {
                logger.Log(Offset + tag.Length - 1, "Tag is closed but not opened");
                return false;
            }
            #endregion

            #region Error: Tag is empty
            if (tag.Length == 2 && startsWithLT && endsWithGT)
            {
                logger.Log(Offset, "Tag has no content");
                return false;
            }
            #endregion

            #region Not a tag, exit
            if (!startsWithLT || !endsWithGT)
            {
                Type = TokenType.Text;
                return true;
            }
            #endregion
            
            #region Special tags:  <! ... >  <? ... >  <% ... >

            if (tag[1] == '!' || tag[1] == '?' || tag[1] == '%')
            {
                Type = TokenType.Special;
                TagType = ElementTagType.SelfContained;

                if (tag.StartsWith("<!--"))
                {
                    IsComment = true;

                    if (tag.Length < 7 /* to handle the special cases <!--> and <!---> */ || !tag.EndsWith("-->"))
                    {
                        logger.Log(Offset, "Comment tag is not closed correctly: expecting '-->'");
                        return false;
                    }

                    // These are just warnings:
                    //int dashPos = 2;
                    //while ((dashPos = tag.IndexOf("--", dashPos + 2, tag.Length - 5 - dashPos)) > 0)
                    //    logger.Log(Offset + dashPos, "Comment should not contain substring '--'");
                }

                if (tag.StartsWith("<![CDATA[", StringComparison.OrdinalIgnoreCase))
                {
                    IsCDATA = true;
                    if (!tag.EndsWith("]]>"))
                    {
                        logger.Log(Offset, "CDATA tag is not closed correctly: expecting ']]>'");
                        return false;
                    }
                }

                if (tag.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
                {
                    IsXmlDeclaration = true;
                    if (!tag.EndsWith("?>"))
                    {
                        logger.Log(Offset, "XML declaration tag is not closed correctly: expecting '?>'");
                        return false;
                    }
                    ExtractElementName(logger);
                    ExtractAttributes(logger);
                }

                if (tag.StartsWith("<%") && !tag.EndsWith("%>"))
                {
                    return false;
                }

                if (tag.StartsWith("<?") && !tag.EndsWith("?>"))
                {
                    return false;
                }

                // other cases:  <!DOCTYPE ... >  <![INCLUDE[ ... ]]>
                return true;
            }

            #endregion

            #region Closing tags:  </ ... >

            if (tag[1] == '/')
            {
                Type = TokenType.Element;
                TagType = ElementTagType.Closing;
                ExtractElementName(logger);
                if (String.IsNullOrEmpty(ElementName))
                {
                    return false;
                }

                // TODO: generate warning if this tag contains any other character besides the elementname and whitespace

                return true;
            }

            #endregion

            #region Self-contained tags:  < ... />

            if (tag.EndsWith("/>"))
            {
                Type = TokenType.Element;
                TagType = ElementTagType.SelfContained;
                ExtractElementName(logger);
                if (String.IsNullOrEmpty(ElementName))
                {
                    return false;
                }

                ExtractAttributes(logger);

                return true;
            }

            #endregion

            #region DEFAULT: Opening tags: < ... >

            {
                Type = TokenType.Element;
                TagType = ElementTagType.Opening;
                ExtractElementName(logger);
                if (String.IsNullOrEmpty(ElementName))
                {
                    return false;
                }

                ExtractAttributes(logger);

                return true;
            }

            #endregion
        }

        private void ExtractElementName(IErrorLogger logger)
        {
            _nameOffset = 0;
            _nameLength = 0;

            int length = 0;
            int offset = 0;
            int state = 0;

            int i = Offset + 1;       // skip "<"  prefix
            if (_text[i] == '/')
            {
                i++; // skip "</" prefix
            }

            if (_text[i] == '?')
            {
                i++; // skip "<?" prefix
            }

            int lastIndex = OffsetEnd - 1;            // skip  ">" suffix
            if (_text[lastIndex] == '/')
            {
                lastIndex--; // skip "/>" suffix
            }

            if (_text[lastIndex] == '?')
            {
                lastIndex--; // skip "?>" suffix
            }

            for (; i <= lastIndex; i++)
            {
                char c = _text[i];

                if (state == 0)
                {
                    if (IsValidFirstNameChar(c))
                    {
                        offset = i - Offset;
                        length = 1;
                        state = 1;
                    }
                    else if (!Char.IsWhiteSpace(c))
                    {
                        logger.Log(offset, "Element name starts with invalid character '{0}'", c);
                        return;
                    }
                }
                else if (state == 1)
                {
                    if (IsValidNameChar(c))
                    {
                        length++;
                    }
                    else if (Char.IsWhiteSpace(c))
                    {
                        break; // end of name
                    }
                    else
                    {
                        logger.Log(offset, "Element name contains invalid character '{0}'", c);
                        return;
                    }
                }
            }

            _nameOffset = offset;
            _nameLength = length;
        }

        private void ExtractAttributes(IErrorLogger logger)
        {
            int nameOffset = 0;
            int nameLength = 0;
            int valueOffset = 0;
            int valueLength = 0;
            int state = 0;
            char q = 'x';

            // State machine: (much faster than regular expression)
            // --------------
            // -1 = reset (abort on higher state)
            //  0 = find valid start for name
            //  1 = name, continue on '=' or whitespace, abort on other
            //  2 = skip whitespace, continue on '=', abort on other
            //  3 = skip whitespace, continue on quotes, abort on other
            //  4 = value, end on matching quote

            int i;
            if (_nameLength > 0)
            {
                i = Offset + _nameOffset + _nameLength + 1;       // skip tag name
            }
            else
            {
                i = Offset + 1;
            }

            int lastIndex = OffsetEnd - 1;            // skip  ">" suffix
            if (_text[lastIndex] == '?')
            {
                lastIndex--; // skip "?>" suffix
            }

            for (; i <= lastIndex; i++)
            {
                if (state == -1)
                {
                    state = 0;
                    i--;
                }

                char c = _text[i];

                if (state == 0)
                {
                    if (IsValidFirstNameChar(c))
                    {
                        nameLength = 0;
                        valueLength = 0;

                        state = 1;
                        nameOffset = i - Offset;
                        nameLength++;
                    }
                }
                else if (state == 1)
                {
                    if (c == '=')
                    {
                        state = 3;
                    }
                    else if (IsValidNameChar(c))
                    {
                        nameLength++;
                    }
                    else if (Char.IsWhiteSpace(c))
                    {
                        state = 2;
                    }
                    else
                    {
                        state = -1;
                    }
                }
                else if (state == 2)
                {
                    if (c == '=')
                    {
                        state = 3;
                    }
                    else if (!Char.IsWhiteSpace(c))
                    {
                        state = -1;
                    }
                }
                else if (state == 3)
                {
                    if (c == '\'' || c == '\"')
                    {
                        valueOffset = i + 1 - Offset;
                        q = c;
                        state = 4;
                    }
                    else if (!Char.IsWhiteSpace(c))
                    {
                        state = -1;
                    }
                }
                else if (state == 4)
                {
                    if (c == q)
                    {
                        if (_attributes == null)
                        {
                            _attributes = new List<XmlAttribute>();
                        }

                        var att = new XmlAttribute(this, nameOffset, nameLength, valueOffset, valueLength);
                        _attributes.Add(att);
                        state = 0;
                    }
                    else
                    {
                        valueLength++;
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public IEnumerable<XmlAttribute> GetAttributes()
        {
            return ElementAttributes ?? Enumerable.Empty<XmlAttribute>();
        }

        /// <summary>
        /// Returns the text in this token that is included in the given global document range.
        /// </summary>
        public string GetSubTextByGlobalPos(int offset, int length)
        {
            if (offset > OffsetEnd)
            {
                return String.Empty;
            }

            if (offset + length <= Offset)
            {
                return String.Empty;
            }

            return Text.SafeSubstring(offset - Offset, length);
        }

        public string GetTextByRelativePos(int relativeOffset, int length)
        {
            var offset = Offset + relativeOffset;

            return _text.SafeSubstring(offset, length);
        }

        internal virtual void UpdateOffset(int delta)
        {
            Offset += delta;

#if DEBUG
            Tools.Assert(Offset >= 0, "Token start index out of bounds: {0} < 0", Offset);
            Tools.Assert(OffsetEnd < _text.Length, "Token end index out of bounds: {0} >= {1}", OffsetEnd, _text.Length);
#endif

        }

        internal void UpdateTextReference(Token otherToken)
        {
            _text = otherToken._text;
        }

        #endregion

        #region Static

        internal static Token Create(IErrorLogger logger, StringBuilder text, int offset, int length)
        {
            Token t = new Token(text, offset, length);

            if (!t.ProcessContents(logger))
            {
                t.Type = TokenType.Invalid;
            }

            return t;
        }

        // see http://www.w3.org/TR/REC-xml/#NT-NameChar
        private static bool IsValidFirstNameChar(char c)
        {
            return (Char.IsLetter(c) || "_:".Contains(c));
        }
        private static bool IsValidNameChar(char c)
        {
            return (Char.IsLetterOrDigit(c) || "_:.-·".Contains(c));
        }

        #endregion
    }

    public class TokenPlaceholder : Token
    {
        public XmlPlaceholder Node { get; private set; }

        protected TokenPlaceholder(int offset, int length)
            : base(null, offset, length)
        {
        }

        public static TokenPlaceholder Create(XmlPlaceholder node)
        {
            return new TokenPlaceholder(node.FirstCharOffset, node.LastCharOffset - node.FirstCharOffset + 1) { Node = node };
        }

        internal override void UpdateOffset(int delta)
        {
            Offset += delta;
        }

        public override string ToString()
        {
            return String.Format("[placeholder] [{0}...{1}]", Offset, OffsetEnd/*, Node.Children[0].Token.DEBUG_GetTextArray().ClipString(Offset, Length, 32) */);
        }
    }


    /// <summary>
    /// Alias for List&lt;Token&gt; with the extra condition that all tokens are adjacent and ordered.
    /// </summary>
    public class TokenList : List<Token>
    {
        public TokenList() { }
        public TokenList(IEnumerable<Token> collection) : base(collection) { }

        /// <summary>
        /// Returns the total number of characters represented by all tokens.
        /// </summary>
        public int TotalLength
        {
            get
            {
                if (Count == 0)
                {
                    return 0;
                }

                return this[Count - 1].OffsetEnd - this[0].Offset + 1;
            }
        }

        /// <summary>
        /// Returns the index of the Token in this list that contains the character at the given offset.
        /// </summary>
        public int FindIndex(int offset)
        {
            return Tools.BinarySearch(this, a =>
                {
                    if (a.Offset > offset)
                    {
                        return -1; // discard all tokens after this one
                    }

                    if (a.OffsetEnd < offset)
                    {
                        return 1; // discard all tokens before this one
                    }

                    return 0; // found it
                });
        }

        /// <summary>
        /// Verifies if this list is still valid.
        /// </summary>
        [Conditional("DEBUG")]
        public void Validate()
        {
            if (!Debugger.IsAttached)
            {
                return;
            }

            if (Count == 0)
            {
                return;
            }

            Token t0 = this[0];
            Tools.Assert(t0.Offset == 0, "First token should start at offset 0");

            for (int i = 1; i < Count; i++)
            {
                Token t1 = this[i];

                Tools.Assert(t0.OffsetEnd + 1 == t1.Offset, "Tokens are not adjacent: {0} chars missing", t1.Offset - (t0.OffsetEnd + 1));

                t0 = t1;
            }
        }
    }
}
