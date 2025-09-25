namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;

    public class Parser
    {
        #region Private Fields

        private readonly DummyErrorLogger _errorLogger = DummyErrorLogger.Instance;

        #endregion

        #region Public Properties

        public XmlDocument Document { get; private set; }

        public TokenList Tokens { get; private set; }

        public StringBuilder RawTextBuffer { get; private set; }

        public event EventHandler DocumentUpdated;
        protected void RaiseDocumentUpdated()
        {
            var handler = DocumentUpdated;
            handler?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Constructor

        public Parser(StringBuilder text)
        {
            System.Diagnostics.Debug.Assert(text != null, "Textbuffer cannot be null");

            RawTextBuffer = text;
            Parse();
        }

        public Parser(string text) : this(new StringBuilder(text))
        {
        }

        #endregion

        #region Public Methods

        public void CharsInserted(int offset, string chars)
        {
            Tools.Assert(offset >= 0, "Cannot insert text at position {0}, out of bounds", offset);
            Tools.Assert(offset <= RawTextBuffer.Length, "Cannot insert text at position {0}, out of bounds (textbuffer only contains {1} character(s))", offset, RawTextBuffer.Length);

            Parser p;
            string s;

            int index = Tokens.FindIndex(offset);
            Token token = index == -1 ? null : Tokens[index];
            int firstIndex = index;
            int lastIndex = index;

            if (token == null)
            #region { there are no tokens yet: copy & exit }
            {
                //System.Diagnostics.Debug.WriteLine("{ first token }");
                p = new Parser(new StringBuilder(chars));
                Tokens = p.Tokens;

                RawTextBuffer = p.RawTextBuffer; // all the tokens have a reference to 'p._text' and not our local '_text'
                Document = p.Document;
                RaiseDocumentUpdated();
                return;
            }
            #endregion

            if (offset == token.Offset && index == 0)
            #region { before the first token }
            {
                //System.Diagnostics.Debug.WriteLine("{ before the first token }");
                s = chars + token.Text;
            }
            #endregion
            else if (offset == token.Offset)
            #region { between this and the previous token }
            {
                //System.Diagnostics.Debug.WriteLine("{ between this and the previous token }");
                Token prevToken = Tokens[index - 1];
                if (prevToken.Type != TokenType.Element)
                {
                    s = prevToken.Text + chars + token.Text;
                    firstIndex--;
                }
                else
                {
                    s = chars + token.Text;
                }
            }
            #endregion
            else if (offset == token.Offset + token.Text.Length && index == Tokens.Count - 1)
            #region { after the last token }
            {
                //System.Diagnostics.Debug.WriteLine("{ after the last token }");
                s = token.Text + chars;
            }
            #endregion
            else if (offset == token.Offset + token.Text.Length)
            #region { between this and the next token }
            {
                //System.Diagnostics.Debug.WriteLine("{ between this and the next token }");
                Token nextToken = Tokens[index + 1];
                if (nextToken.Type != TokenType.Element)
                {
                    s = token.Text + chars + nextToken.Text;
                    lastIndex++;
                }
                else
                {
                    s = token.Text + chars;
                }
            }
            #endregion
            else
            #region { inside a single token }
            {
                //System.Diagnostics.Debug.WriteLine("{ inside a single token }");
                s = token.Text.Insert(offset - token.Offset, chars);
            }
            #endregion

            RawTextBuffer.Insert(offset, chars);
            TokenList tokens = CharsToTokens(new StringBuilder(s), DummyErrorLogger.Instance);
            ReplaceTokens(firstIndex, lastIndex, tokens);
        }

        public void CharsRemoved(int offset, int length)
        {
            Tools.Assert(offset >= 0, "Cannot remove text at position {0}, out of bounds", offset);
            Tools.Assert(offset < RawTextBuffer.Length, "Cannot remove text at position {0}, out of bounds (textbuffer only contains {1} character(s))", offset, RawTextBuffer.Length);
            Tools.Assert(offset + length <= RawTextBuffer.Length, "Cannot remove {0} chars at position {1}, out of bounds (textbuffer only contains {2} character(s))", length, offset, RawTextBuffer.Length);

            // document was cleared?
            if (offset == 0 && length == RawTextBuffer.Length)
            {
                RawTextBuffer.Clear();
                Tokens.Clear();
                Document = new XmlDocument() { TotalLength = 0 };
                RaiseDocumentUpdated();
                return;
            }

            int firstIndex = Tokens.FindIndex(offset);
            int lastIndex = Tokens.FindIndex(offset + length - 1);

            Token firstToken = Tokens[firstIndex];
            Token lastToken = Tokens[lastIndex];

            string s1 = firstToken.Text.Substring(0, offset - firstToken.Offset);
            int startIndex = offset + length - lastToken.Offset;

            // Use Math.Min to ensure startIndex does not exceed string length.
            string s2 = lastToken.Text.Substring(Math.Min(startIndex, lastToken.Text.Length));

            // when deleting a '<'character, also include the previous token
            if (firstIndex > 0 && firstToken.GetSubTextByGlobalPos(offset, length).Contains('<'))
            {
                firstIndex--;
                s1 = Tokens[firstIndex].Text + s1;
            }
            // when deleting a '>' character, also include the next token
            if (lastIndex < Tokens.Count - 1 && lastToken.GetSubTextByGlobalPos(offset, length).Contains('>'))
            {
                lastIndex++;
                s2 += Tokens[lastIndex].Text;
            }

            RawTextBuffer.Remove(offset, length);
            TokenList tokens = CharsToTokens(new StringBuilder(s1 + s2), DummyErrorLogger.Instance);
            ReplaceTokens(firstIndex, lastIndex, tokens);
        }

        #endregion

        #region Private Methods

        private void Parse()
        {
            Queue<Token> tokens = new Queue<Token>();

            Step1_CharsToTokens(tokens);
            Tokens = new TokenList(tokens);

            Step2_TokensToTree(tokens, out XmlDocument root);
            Document = root;
        }

        private void Step1_CharsToTokens(Queue<Token> tokens)
        {
            foreach (var token in CharsToTokens(RawTextBuffer, _errorLogger))
            {
                tokens.Enqueue(token);
            }
        }

        private void Step2_TokensToTree(Queue<Token> tokens, out XmlDocument root)
        {
            Stack<XmlNode> nodes = new Stack<XmlNode>();
            Stack<XmlElement> openTags = new Stack<XmlElement>();
            List<XmlNode> children = new List<XmlNode>();

            while (tokens.Count > 0)
            {
                Token t = tokens.Dequeue();

                #region Placeholder
                if (t is TokenPlaceholder tp)
                {
                    // Stack<T> is slow at inserting multiple items, expand this later when adding to parentnode.Children
                    nodes.Push(tp.Node);

                    //XmlPlaceholder ph = (t as TokenPlaceholder).Node;
                    //nodes.PushRange(ph.WrappedChildren);

                    continue;
                }
                #endregion

                #region Text or Invalid node
                if (t.Type == TokenType.Text || t.Type == TokenType.Invalid)
                {
                    nodes.Push(new XmlText() { Token = t });
                    continue;
                }
                #endregion

                #region Special node
                if (t.Type == TokenType.Special)
                {
                    if (t.IsComment)
                    {
                        nodes.Push(new XmlComment() { Token = t });
                    }
                    else if (t.IsCDATA)
                    {
                        nodes.Push(new XmlCDATA() { Token = t });
                    }
                    else if (t.IsXmlDeclaration)
                    {
                        nodes.Push(new XmlDeclaration() { Token = t });
                    }
                    else
                    {
                        nodes.Push(new XmlSpecial() { Token = t });
                    }

                    continue;
                }
                #endregion                

                if (t.Type == TokenType.Element)
                {
                    #region Self-contained element node
                    if (t.TagType == ElementTagType.SelfContained)
                    {
                        nodes.Push(new XmlElement() { Name = t.ElementName, Token = t, TokenClose = t, IsSubtreeValid = true });
                        continue;
                    }
                    #endregion

                    #region Opening/Closing element node

                    if (t.TagType == ElementTagType.Opening)
                    {
                        // keep the node marked as invalid until its closing tag is found
                        nodes.Push(new XmlElement() { Name = t.ElementName, Token = t });
                        openTags.Push(nodes.Peek() as XmlElement);
                    }
                    else if (t.TagType == ElementTagType.Closing)
                    {
                        // try closing an open element

                        #region Error: there are no opening tags (continue)
                        if (openTags.Count == 0)
                        {
                            _errorLogger.Log(t.Offset, "Closing tag for element '{0}' has no matching opening tag.", t.ElementName);
                            // push into the tree as an invalid XmlElement
                            nodes.Push(new XmlElement() { Name = t.ElementName, Token = t });
                            continue;
                        }
                        #endregion

                        #region Error: closing tag does not match the last opening tag (pop until match found)
                        if (!String.Equals(openTags.Peek().Name, t.ElementName, StringComparison.OrdinalIgnoreCase))
                        {
                            _errorLogger.Log(t.Offset, "Closing tag for element '{0}' does not match the last opening tag '{1}'.", t.ElementName, openTags.Peek().Name);

                            int popCount = 0;
                            foreach (XmlElement openTag in openTags)
                            {
                                if (String.Equals(openTag.Name, t.ElementName, StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }

                                popCount++;
                            }

                            if (popCount < openTags.Count)
                            {
                                for (int i = 0; i < popCount; i++)
                                {
                                    _errorLogger.Log(openTags.Peek().Token.Offset, " -> Opening tag '{0}' was never closed.", openTags.Peek().Name);
                                    openTags.Pop();
                                }
                            }
                        }
                        #endregion

                        XmlElement openTagsPeek = openTags.Peek();
                        if (String.Equals(openTagsPeek.Name, t.ElementName, StringComparison.OrdinalIgnoreCase))
                        {
                            // warning
                            if (!String.Equals(openTagsPeek.Name, t.ElementName, StringComparison.Ordinal))
                            {
                                _errorLogger.Log(openTagsPeek.Token.Offset, "Opening and closing tags '{0}' have different casing.", openTagsPeek.Name);
                            }

                            // pop the opening node stack and 'close' the node
                            XmlElement openNode = openTags.Pop();
                            openNode.TokenClose = t;

                            // pop the main node stack until we reach the opening node
                            bool allValid = true;

                            //while (nodes.Count_TRANS > 0)
                            while (nodes.Count > 0)
                            {
                                //XmlNode child = nodes.Peek_TRANS();
                                XmlNode child = nodes.Peek();
                                if (child == openNode)
                                {
                                    openNode.IsSubtreeValid = allValid;
                                    break; // leave the opening node on the main stack
                                }

                                if (child is XmlPlaceholder ph)
                                {
                                    foreach (var c in ph.WrappedChildren)
                                    {
                                        c.ParentNode = openNode;
                                    }

                                    children.AddRange(ph.WrappedChildren.Backwards());
                                    // subtree is valid by definition
                                }
                                else
                                {
                                    child.ParentNode = openNode;
                                    if (!child.IsSubtreeValid)
                                    {
                                        allValid = false;
                                    }

                                    children.Add(child);
                                }
                                //nodes.Pop_TRANS();
                                nodes.Pop();
                            }

                            if (children.Count > 0)
                            {
                                children.Reverse();
                                openNode.InsertRange(0, children);
                                children.Clear();
                            }

                            //XmlNode[] array = new XmlNode[nodes.CountPopped_TRANS];
                            //nodes.CopyTo_TRANS(array);
                            //openNode.Children.InsertRange(0, array);
                            //nodes.Commit_TRANS();


                            //if (openNode.Children.Count > 150)
                            //    Trace.WriteLine(String.Format("Stacksize is {0} after popping {1} children for {2}", nodes.Count, openNode.Children.Count, openNode.Name));
                        }
                        else
                        {
                            // push into the tree as an invalid XmlElement
                            nodes.Push(new XmlElement() { Name = t.ElementName, Token = t });
                        }
                    }

                    #endregion
                }
            }

            // cleanup all unclosed tags
            foreach (XmlElement openNode in openTags)
            {
                _errorLogger.Log(openNode.Token.Offset, "Opening tag '{0}' has no closing tag.", openNode.Name);
            }

            XmlDocument doc = new XmlDocument() { TotalLength = RawTextBuffer.Length };

            foreach (XmlNode n in nodes)
            {
                if (n is XmlPlaceholder ph)
                {
                    foreach (var c in ph.WrappedChildren)
                    {
                        c.ParentNode = doc;
                    }

                    children.AddRange(ph.WrappedChildren.Backwards());
                    // subtree is valid by definition
                }
                else
                {
                    n.ParentNode = doc;
                    children.Add(n);
                }
            };

            if (children.Count > 0)
            {
                children.Reverse();
                doc.InsertRange(0, children);
            }

            root = doc;
        }

        /// <summary>
        /// Replace all tokens between [first;last] with the resulting tokens of the parser.
        /// </summary>
        private void ReplaceTokens(int firstIndex, int lastIndex, TokenList newTokens)
        {
            Token firstToken = Tokens[firstIndex];
            Token lastToken = Tokens[lastIndex];

            int firstOffset = firstToken.Offset;
            int lastOffset = lastToken.OffsetEnd;
            int oldLength = lastOffset - firstOffset + 1;

            int newLength = 0;
            if (newTokens.Count > 0)
            {
                newLength = newTokens.Last().OffsetEnd - newTokens.First().Offset + 1;
            }

            int delta = newLength - oldLength;
            
            Document.GroupUnaffectedChildren(firstOffset, oldLength);
            var wrappedTokens = Document.GetTokenList();
            int idxA = wrappedTokens.FindIndex(firstOffset);
            int idxB = wrappedTokens.FindIndex(lastOffset);

            // update token offsets for the new string
            for (int i = 0; i < newTokens.Count; i++)
            {
                newTokens[i].UpdateTextReference(firstToken);
                newTokens[i].UpdateOffset(firstOffset);
            }

            // update token offsets for the old tokens
            for (int i = lastIndex + 1; i < Tokens.Count; i++)
            {
                Tokens[i].UpdateOffset(delta);
            }

            for (int i = idxA; i < wrappedTokens.Count; i++)
            {
                if (wrappedTokens[i] is TokenPlaceholder)
                {
                    wrappedTokens[i].UpdateOffset(delta);
                }
            }

            Tokens.ReplaceRange(firstIndex, lastIndex - firstIndex + 1, newTokens);
            Tokens.Validate();
            wrappedTokens.ReplaceRange(idxA, idxB - idxA + 1, newTokens);
            wrappedTokens.Validate();

            #region Check if a range of (invalid) tokens can be merged into a single valid tag
            {
                int ii = -1;
                for (int i = 0; i < wrappedTokens.Count; i++)
                {
                    if (wrappedTokens[i].Type == TokenType.Invalid)
                    {
                        if (ii == -1)
                        {
                            ii = i;
                        }
                        else
                        {
                            int startOffset = wrappedTokens[ii].Offset;
                            int endOffset = wrappedTokens[i].OffsetEnd;
                            int length = endOffset - startOffset + 1;

                            Token replacingToken = Token.Create(_errorLogger, RawTextBuffer, startOffset, length);
                            if (replacingToken.Type == TokenType.Invalid)
                            {
                                continue; // still invalid
                            }

                            idxA = Tokens.FindIndex(startOffset);
                            idxB = Tokens.FindIndex(endOffset);

                            // TODO: handle nested comments?
                            //for (int j = idxA + 1; j < idxB; j++)
                            //if (Tokens[j].Type == TokenType.Special && Tokens[j].IsComment)
                            //{
                            //
                            //}

                            var replacingTokens = new Token[] { replacingToken };
                            Tokens.ReplaceRange(idxA, idxB - idxA + 1, replacingTokens);
                            Tokens.Validate();
                            wrappedTokens.ReplaceRange(ii, i - ii + 1, replacingTokens);
                            wrappedTokens.Validate();

                            //reset
                            ii = -1;
                        }
                    }
                }
            }
            #endregion

            Queue<Token> queue = new Queue<Token>(wrappedTokens);

            Step2_TokensToTree(queue, out XmlDocument root); // note: this will expand XmlPlaceholder nodes
            Document = root;

            RaiseDocumentUpdated();
        }

        #endregion

        #region Static

        internal static TokenList CharsToTokens(StringBuilder text, IErrorLogger errorLogger)
        {
            TokenList tokens = new TokenList();

            int startOffset = 0;
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '<' && !builder.StartsWith("<!--") && !builder.StartsWith("<!["))
                {
                    // terminate current token
                    if (builder.Length > 0)
                    {
                        tokens.Add(Token.Create(errorLogger, text, startOffset, builder.Length));
                    }

                    // start new token
                    startOffset = i;
                    builder.Clear();
                    builder.Append(c);
                }
                else
                {
                    builder.Append(c);

                    if (c == '>' && (!builder.StartsWith("<!--") || builder.EndsWith("-->"))
                                 && (!builder.StartsWith("<![") || builder.EndsWith("]]>")))
                    {
                        // terminate current token
                        tokens.Add(Token.Create(errorLogger, text, startOffset, builder.Length));

                        // start new token
                        startOffset = i + 1;
                        builder.Clear();
                    }
                }
            }

            // terminate current token
            if (builder.Length > 0)
            {
                tokens.Add(Token.Create(errorLogger, text, startOffset, builder.Length));
            }

            return tokens;
        }

        #endregion

    }
}
