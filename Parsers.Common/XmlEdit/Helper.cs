namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;

    internal static class Helper
    {
        public static string FormatTextBetweenNodes(string text, int depth, bool parentHasMixedContent, bool isLastChild = false)
        {
            var lines = text.ReadLines().ToList();

            if (lines.Count == 1 && !parentHasMixedContent)
            {
                // don't change single line text value
                return text;
            }

            {
                // trim all lines
                for (int i = 0; i < lines.Count; i++)
                {
                    lines[i] = lines[i].TrimStart();
                }

                // first and last line should be empty
                if (depth > 0)
                {
                    if (lines.Count == 0 || lines[0] != "")
                    {
                        lines.Insert(0, "");
                    }

                    if (lines.Count < 2 || lines[lines.Count - 1] != "")
                    {
                        lines.Add("");
                    }
                }

                // add tabs before each line, except the first
                for (int i = 1; i < lines.Count; i++)
                {
                    string prefix;

                    if (i < lines.Count - 1)
                    {
                        prefix = new string('\t', depth);
                    }
                    else // last line
                    {
                        int length = depth + (isLastChild ? -1 : 0);
                        prefix = new string('\t', length < 0 ? 0 : length);
                    }

                    lines[i] = prefix + lines[i];
                }

                return String.Join(Environment.NewLine, lines);
            }
        }

        public static void FormatTextBeforeNode(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (!(node.ParentNode is IXmlContainer container))
            {
                return;
            }

            bool hasMixedContent = !container.Children.All(c => c is XmlText);

            List<XmlText> textNodes = new List<XmlText>();

            int index = container.Children.IndexOf(node);
            while (index > 0)
            {
                index--;
                if (container.Children[index] is XmlText t)
                {
                    textNodes.Add(t);
                }
                else
                {
                    break;
                }
            }

            string text = String.Join("", textNodes.Select(t => t.Text));
            string newText = Helper.FormatTextBetweenNodes(text, container.Depth, hasMixedContent);

            if (!String.Equals(text, newText))
            {
                // replace all text nodes
                textNodes.ForEach(x => container.Children.Remove(x));
                container.Children.InsertBefore(node, new XmlText(newText));
            }
        }

        public static void FormatTextAfterNode(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (!(node.ParentNode is IXmlContainer container))
            {
                return;
            }

            bool hasMixedContent = !container.Children.All(c => c is XmlText);

            List<XmlText> textNodes = new List<XmlText>();
            bool isLastChild = false;

            int index = container.Children.IndexOf(node);

            while (index < container.Children.Count)
            {
                index++;

                if (index >= container.Children.Count)
                {
                    isLastChild = true;
                    break;
                }

                if (container.Children[index] is XmlText t)
                {
                    textNodes.Add(t);
                }
                else
                {
                    break;
                }
            }

            string text = String.Join("", textNodes.Select(t => t.Text));
            string newText = Helper.FormatTextBetweenNodes(text, container.Depth, hasMixedContent, isLastChild);

            if (!String.Equals(text, newText))
            {
                // replace all text nodes
                textNodes.ForEach(x => container.Children.Remove(x));
                container.Children.InsertAfter(node, new XmlText(newText));
            }
        }
    }
}