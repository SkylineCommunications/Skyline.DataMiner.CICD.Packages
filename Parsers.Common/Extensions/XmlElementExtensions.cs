namespace Skyline.DataMiner.CICD.Parsers.Common.Extensions
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    internal static class XmlElementExtensions
    {
        internal static IList<XmlElement> GetElements(this XmlElement e, string path)
        {
            List<XmlElement> items = new List<XmlElement>();

            path = path.Replace('\\', '/');
            string[] bits = path.Split('/');

            foreach (var n in e.Children)
            {
                if (!(n is XmlElement child) || !String.Equals(child.Name, bits[0], StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (bits.Length == 1)
                {
                    items.Add(child);
                }
                else
                {
                    items.AddRange(GetElements(child, String.Join("/", bits, 1, bits.Length - 1)));
                }
            }

            return items;
        }

        /// <summary>
        /// Returns the first child element that matches the given path (case-insensitive).
        /// </summary>
        internal static XmlElement GetElement(this XmlElement e, string path)
        {
            path = path.Replace('\\', '/');
            string[] bits = path.Split('/');

            foreach (var n in e.Children)
            {
                if (!(n is XmlElement child) || !String.Equals(child.Name, bits[0], StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (bits.Length == 1)
                {
                    return child;
                }

                var c = GetElement(child, String.Join("/", bits, 1, bits.Length - 1));
                if (c != null)
                {
                    return c;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the value of the attribute if it exists and is not empty or whitespace. Else the default value is returned.
        /// </summary>
        /// <param name="e">The target xml element.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="default">The value to be returned when the attribute does not exist.</param>
        /// <returns></returns>
        internal static string GetNonEmptyAttribute(this XmlElement e, string name, string @default = null)
        {
            string value = e.GetAttributeValue(name, @default);
            return String.IsNullOrWhiteSpace(value) ? @default : value;
        }
    }
}
