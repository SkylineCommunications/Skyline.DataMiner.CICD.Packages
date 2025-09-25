namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public abstract class XmlContainer : XmlNode
    {
        protected readonly List<XmlNode> _children = new List<XmlNode>();

        private IIndexer<string, XmlElement> _elementIndexer;
        private IIndexer<string, IEnumerable<XmlElement>> _elementsIndexer;

        #region Public Properties

        /// <summary>
        /// Contains the child nodes. Not used for XmlText and XmlSpecial.
        /// </summary>
        public IReadOnlyList<XmlNode> Children => _children.AsReadOnly();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IIndexer<string, IEnumerable<XmlElement>> Elements
        {
            get
            {
                if (_elementsIndexer == null)
                {
                    _elementsIndexer = new Indexer<XmlContainer, string, IEnumerable<XmlElement>>(
                        this, (s, k) => s.GetElements().Where(e => String.Equals(e.Name, k, StringComparison.OrdinalIgnoreCase)));
                }

                return _elementsIndexer;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IIndexer<string, XmlElement> Element
        {
            get
            {
                if (_elementIndexer == null)
                {
                    _elementIndexer = new Indexer<XmlContainer, string, XmlElement>(
                        this, (s, k) =>
                        {
                            var element = s.Elements[k].FirstOrDefault();
                            return element;
                        });
                }

                return _elementIndexer;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Recursively finds the node that corresponds with the given text offset.
        /// </summary>
        public XmlNode FindNode(int offset)
        {
            int childIndex = Tools.BinarySearch(Children, node =>
            {
                if (node.LastCharOffset < offset)
                {
                    return 1;
                }

                if (node.FirstCharOffset > offset)
                {
                    return -1;
                }

                return 0;
            });

            if (childIndex >= 0 && Children[childIndex] is XmlContainer container)
            {
                return container.FindNode(offset);
            }
            else
            {
                return this;
            }
        }

        public IEnumerable<XmlElement> GetElements()
        {
            return _children.OfType<XmlElement>();
        }

        #endregion

        #region Internal Methods

        internal void InsertRange(int index, IEnumerable<XmlNode> items)
        {
            _children.InsertRange(index, items);
        }

        /// <summary>
        /// Recursive.
        /// </summary>
        internal virtual void GroupUnaffectedChildren(int offset, int length)
        {
            if (Children.Count == 0)
            {
                return; // only nodes with children can do something here
            }

            int x1 = offset;
            int x2 = offset + length - 1;
            if (!OverlapsWithRange(x1, x2) && IsSubtreeValid)
            {
                return; // no overlap with this valid node, our parent will wrap us in a XmlPlaceholder node if possible
            }
            // this should never occur: a parent should never descend into non-overlapping valid subtrees


            // there is overlap, or this node has an invalid subtree

            int firstIndex = -1;
            XmlPlaceholder ph = new XmlPlaceholder() { ParentNode = this };

            for (int i = 0; i < Children.Count; i++)
            {
                XmlNode child = Children[i];
                Debug.Assert(!(child is XmlPlaceholder));

                if (child.OverlapsWithRange(x1, x2) || !child.IsSubtreeValid)
                {
                    // finish current placeholder and prepare a new one
                    if (ph.WrappedChildren.Count > 0)
                    {
                        _children.RemoveRange(firstIndex, ph.WrappedChildren.Count);
                        _children.Insert(firstIndex, ph);
                        i = firstIndex + 1; // skip the XmlPlaceholder AND the current 'child'
                        ph = new XmlPlaceholder() { ParentNode = this };
                        firstIndex = -1;
                    }

                    // We need to descend in both cases:
                    // - if the subtree overlaps, we need to go all the way down to find the exact range of overlapping tokens,
                    // - if the subtree is invalid, we still want to group all valid parts of the subtree, to improve performance:
                    //   An invalid token inside one <Param> tag would cause XmlDocument.GetTokenList() to re-generate all tokens for all parameters,
                    //   but none of the valid neighbouring <Param> subtrees could ever affect the global structure, they can only be moved to a different parent.
                    if (child is XmlContainer container)
                    {
                        container.GroupUnaffectedChildren(offset, length);
                    }
                }
                else
                {
                    // wrap into current placeholder
                    ph.WrappedChildren.Add(child);
                    child.ParentNode = ph;
                    if (firstIndex == -1)
                    {
                        firstIndex = i;
                    }
                }
            }

            // don't forget the last wrapper
            if (ph.WrappedChildren.Count > 0)
            {
                _children.RemoveRange(firstIndex, ph.WrappedChildren.Count);
                _children.Add(ph);
            }
        }

        #endregion
    }
}