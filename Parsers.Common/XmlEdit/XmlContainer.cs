namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IXmlContainer : IXmlNode
    {
        ChildrenList Children { get; }

        IIndexer<string, IEnumerable<XmlElement>> Elements { get; }

        IIndexer<string, XmlElement> Element { get; }
    }

    public interface IXmlContainer<TData> : IXmlContainer, IXmlNode<TData>
        where TData : Xml.XmlContainer
    {
    }

    /// <summary>
    /// Represents an XML node that can contain other nodes.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TNode"></typeparam>
    public abstract class XmlContainer<TData, TNode> : XmlNode<TData, TNode>, IXmlContainer<TData>
        where TData : Xml.XmlContainer
        where TNode : XmlNode
    {
        private IIndexer<string, XmlElement> _elementIndexer;
        private IIndexer<string, IEnumerable<XmlElement>> _elementsIndexer;

        private readonly Lazy<ChildrenList> _lazyChildren;
        public ChildrenList Children => _lazyChildren.Value;

        protected XmlContainer(TData data) : base(data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            _lazyChildren = new Lazy<ChildrenList>(() => new ChildrenList(this, data.Children), false);
        }

        protected XmlContainer() : base(null)
        {
            _lazyChildren = new Lazy<ChildrenList>(() => new ChildrenList(this), false);
        }

        public IIndexer<string, IEnumerable<XmlElement>> Elements
        {
            get
            {
                if (_elementsIndexer == null)
                {
                    _elementsIndexer = new Indexer<IEnumerable<XmlElement>, string, IEnumerable<XmlElement>>(Children.OfType<XmlElement>(), (s, k) => s.Where(x => String.Equals(x.Name, k, StringComparison.OrdinalIgnoreCase)));
                }

                return _elementsIndexer;
            }
        }

        public IIndexer<string, XmlElement> Element
        {
            get
            {
                if (_elementIndexer == null)
                {
                    _elementIndexer = new Indexer<IEnumerable<XmlElement>, string, XmlElement>(Children.OfType<XmlElement>(), (s, k) => s.FirstOrDefault(x => String.Equals(x.Name, k, StringComparison.OrdinalIgnoreCase)));
                }

                return _elementIndexer;
            }
        }

        public bool HasChildren
        {
            get
            {
                if (_lazyChildren.IsValueCreated)
                {
                    return _lazyChildren.Value.Count > 0;
                }

                if (Data != null)
                {
                    return Data.Children.Count > 0;
                }

                return false;
            }
        }

        internal override void FormatInternal()
        {
            bool hasMixedContent = !Children.All(n => n is XmlText);

            List<XmlText> textNodes = new List<XmlText>();

            foreach (var child in Children.ToList())
            {
                child.FormatInternal();

                if (child is XmlText t)
                {
                    textNodes.Add(t);
                }
                else
                {
                    string allText = String.Join("", textNodes.Select(x => x.Text));
                    string expectedText = Helper.FormatTextBetweenNodes(allText, Depth, hasMixedContent);

                    if (!String.Equals(allText, expectedText))
                    {
                        // replace all text nodes
                        textNodes.ForEach(x => Children.Remove(x));
                        Children.InsertBefore(child, new XmlText(expectedText));
                    }

                    textNodes.Clear();
                }
            }

            // whitespace at the end
            string allEndText = String.Join("", textNodes.Select(x => x.Text));
            string expectedEndText;

            if (Children.Count > 0)
            {
                expectedEndText = Helper.FormatTextBetweenNodes(allEndText, Depth, hasMixedContent, isLastChild: true);
            }
            else
            {
                expectedEndText = ""; // this node has no content
            }

            if (!String.Equals(allEndText, expectedEndText))
            {
                // replace all text nodes
                textNodes.ForEach(x => Children.Remove(x));
                Children.Add(new XmlText(expectedEndText));
            }
        }
    }
}