namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class XmlChangesDetector
    {
        private readonly string _text;
        private readonly List<TextChange> _changes = new List<TextChange>();

        private XmlChangesDetector(string text)
        {
            _text = text;
        }

        public static List<TextChange> GetChanges(string text, XmlNode node)
        {
            var detector = new XmlChangesDetector(text);
            detector.Visit(node);

            return detector._changes;
        }

        private void Visit(XmlNode node, int depth = 0)
        {
            if (!node.HasChanges)
            {
                return; // no changes in this node or subtree
            }

            switch (node)
            {
                case XmlDocument nodeDocument:
                    VisitXmlDocument(nodeDocument);
                    break;

                case XmlElement nodeElement:
                    VisitXmlElement(nodeElement, depth);
                    break;

                case XmlText nodeText:
                    VisitXmlText(nodeText);
                    break;

                case XmlComment nodeComment:
                    VisitXmlComment(nodeComment);
                    break;

                case XmlCDATA nodeCDATA:
                    VisitXmlCDATA(nodeCDATA);
                    break;

                case XmlDeclaration nodeDecl:
                    VisitXmlDeclaration(nodeDecl);
                    break;
            }
        }

        private void VisitXmlDocument(XmlDocument node)
        {
            if (node.Data != null)
            {
                ProcessChildren(node, depth: 0);
            }
            else
            {
                // shouldn't occur
            }
        }

        private void VisitXmlElement(XmlElement node, int depth)
        {
            if (node.Data == null)
            {
                // handled on higher level
                return;
            }

            var token = node.Data.Token;

            // rename
            if (node.Data.Name != node.Name)
            {
                RegisterChange(TextChange.CreateReplace(token.NameOffset, token.NameLength, node.Name));

                var tokenClose = node.Data.TokenClose;
                if (tokenClose != null && tokenClose.TagType != ElementTagType.SelfContained)
                {
                    RegisterChange(TextChange.CreateReplace(tokenClose.NameOffset, tokenClose.NameLength, node.Name));
                }
            }

            if (token.TagType == ElementTagType.Closing)
            {
                return;
            }

            // attributes
            ProcessAttributes(node);

            // children
            bool convertSelfContained = node.Data.Token.TagType == ElementTagType.SelfContained && node.Children.Count > 0;

            if (convertSelfContained)
            {
                // remove '\' and all whitespace before it

                int end = node.Data.Token.OffsetEnd;
                int start = end - 1;

                while (start > 0 && Char.IsWhiteSpace(_text[start - 1]))
                {
                    start--;
                }

                RegisterChange(TextChange.CreateDelete(start, end - start));
            }

            ProcessChildren(node, depth);

            if (convertSelfContained)
            {
                RegisterChange(TextChange.CreateInsert(node.Data.Token.OffsetEnd + 1, "</" + node.Name + ">"));
            }
        }

        private void VisitAttribute(XmlAttribute attribute)
        {
            if (attribute.Data == null)
            {
                // handled on higher level
                return;
            }

            if (attribute.Data.Name != attribute.Name)
            {
                RegisterChange(TextChange.CreateReplace(attribute.Data.NameOffset, attribute.Data.NameLength, attribute.Name));
            }

            if (attribute.Data.Value != attribute.Value)
            {
                var valueEncoded = WebUtility.HtmlEncode(attribute.Value ?? "");
                RegisterChange(TextChange.CreateReplace(attribute.Data.ValueOffset, attribute.Data.ValueLength, valueEncoded));
            }
        }

        private void VisitXmlText(XmlText node)
        {
            if (node.Data == null)
            {
                // handled on higher level
                return;
            }

            if (!String.Equals(node.Data.Text, node.Text))
            {
                string textEncoded = WebUtility.HtmlEncode(node.Text ?? "");
                RegisterChange(TextChange.CreateReplace(node.Data.FirstCharOffset, node.Data.TotalLength, textEncoded));
            }
        }

        private void VisitXmlComment(XmlComment node)
        {
            if (node.Data == null)
            {
                // handled on higher level
                return;
            }

            if (!String.Equals(node.Data.InnerText, node.InnerText))
            {
                // CDATA content must not be encoded!
                string text = node.InnerText ?? "";
                RegisterChange(TextChange.CreateReplace(node.Data.FirstCharOffset + 4, node.Data.TotalLength - 7, text));
            }
        }

        private void VisitXmlCDATA(XmlCDATA node)
        {
            if (node.Data == null)
            {
                // handled on higher level
                return;
            }

            if (!String.Equals(node.Data.InnerText, node.InnerText))
            {
                // CDATA content must not be encoded!
                string text = node.InnerText ?? "";
                RegisterChange(TextChange.CreateReplace(node.Data.FirstCharOffset + 9, node.Data.TotalLength - 12, text));
            }
        }

        private void VisitXmlDeclaration(XmlDeclaration node)
        {
            if (node.Data == null)
            {
                // handled on higher level
                return;
            }

            // tag only has attributes
            ProcessAttributes(node);
        }

        private void ProcessAttributes(XmlNode node)
        {
            // based on http://www.mlsite.net/blog/?p=2250

            if (node.Data == null || node.Attributes == null)
            {
                return;
            }

            var token = node.Data.Token;

            if (token.TagType == ElementTagType.Closing)
            {
                return;
            }

            var currentAttributes = token.GetAttributes().ToList();
            var newAttributes = node.Attributes;

            int insertionPos = token.NameOffset + token.NameLength;

            int i1 = 0, i2 = 0;

            while (i1 < currentAttributes.Count || i2 < newAttributes.Count)
            {
                var oldAttribute = i1 < currentAttributes.Count ? currentAttributes[i1] : null;
                var newAttribute = i2 < newAttributes.Count ? newAttributes[i2] : null;

                // equals?
                if (oldAttribute != null && newAttribute?.Data == oldAttribute)
                {
                    // check internal update
                    VisitAttribute(newAttribute);
                    insertionPos = oldAttribute.NameOffset + oldAttribute.TotalLength;
                    i1++;
                    i2++;
                }
                // deleted?
                else if (oldAttribute != null && !newAttributes.ListSkip(i2).Any(x => x.Data == oldAttribute))
                {
                    // delete
                    int start = oldAttribute.NameOffset;
                    int end = oldAttribute.NameOffset + oldAttribute.TotalLength;

                    // also remove all whitespace in front
                    while (start > 0 && Char.IsWhiteSpace(_text[start - 1]))
                    {
                        start--;
                    }

                    RegisterChange(TextChange.CreateDelete(start, end - start));
                    i1++;
                }
                // new?
                else if (newAttribute != null)
                {
                    // insert
                    bool addSpace = !Char.IsWhiteSpace(_text[insertionPos - 1]);

                    string text = (addSpace ? " " : "") + newAttribute.GetXml();
                    RegisterChange(TextChange.CreateInsert(insertionPos, text));
                    i2++;
                }
                else
                {
                    // something went wrong..
                    throw new InvalidOperationException("Something went wrong...");
                }
            }
        }

        private void ProcessChildren<T>(IXmlContainer<T> node, int depth) where T : XmlContainer
        {
            // based on http://www.mlsite.net/blog/?p=2250

            if (node.Data == null)
            {
                return;
            }

            var token = node.Data.Token;

            var currentChildren = node.Data.Children;
            var newChildren = node.Children;

            int insertionPos = token != null ? token.Offset + token.Length : 0;

            int i1 = 0, i2 = 0;

            while (i1 < currentChildren.Count || i2 < newChildren.Count)
            {
                var oldChild = i1 < currentChildren.Count ? currentChildren[i1] : null;
                var newChild = i2 < newChildren.Count ? newChildren[i2] : null;

                // equals?
                if (oldChild != null && newChild?.Data == oldChild)
                {
                    // check internal update
                    Visit(newChild, depth + 1);
                    insertionPos = oldChild.LastCharOffset + 1;
                    i1++;
                    i2++;
                }
                // deleted?
                else if (oldChild != null && !newChildren.ListSkip(i2).Any(x => x.Data == oldChild))
                {
                    // delete
                    int start = oldChild.FirstCharOffset;
                    int length = oldChild.TotalLength;

                    RegisterChange(TextChange.CreateDelete(start, length));
                    i1++;
                }
                // new?
                else if (newChild != null)
                {
                    // insert
                    string text = newChild.GetXml();
                    RegisterChange(TextChange.CreateInsert(insertionPos, text));
                    i2++;
                }
                else
                {
                    // something went wrong..
                    throw new InvalidOperationException("Something went wrong...");
                }
            }
        }

        private void RegisterChange(TextChange change)
        {
            if (change == null)
            {
                throw new ArgumentNullException(nameof(change));
            }

            _changes.Add(change);
        }
    }
}