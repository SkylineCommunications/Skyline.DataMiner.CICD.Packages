namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;

    public class ChildrenList : IList<XmlNode>
    {
        private readonly List<XmlNode> _list = new List<XmlNode>();
        private readonly XmlNode _parent;

        public ChildrenList(XmlNode parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public ChildrenList(XmlNode parent, IEnumerable<Xml.XmlNode> children)
            : this(parent)
        {
            foreach (var child in children)
            {
                var childNode = XmlNodeFactory.CreateXmlNode(child);
                childNode.ParentNode = parent;
                _list.Add(childNode);
            }
        }

        public XmlNode this[int index]
        {
            get => _list[index];
            set
            {
                if (index < 0 || index >= _list.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (_list[index] != value)
                {
                    _list[index] = value;
                    value.ParentNode = _parent;
                    value.HasChanges = true;
                }
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Clear()
        {
            if (_list.Count <= 0)
            {
                return;
            }

            foreach (var item in _list)
            {
                item.ParentNode = null;
            }

            _list.Clear();
            _parent.HasChanges = true;
        }

        public bool Contains(XmlNode item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(XmlNode[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int IndexOf(XmlNode item)
        {
            return _list.IndexOf(item);
        }

        public void Add(XmlNode item)
        {
            Insert(Count, item);
        }

        public void Insert(int index, XmlNode item)
        {
            if (index < 0 || index > _list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.ParentNode != null)
            {
                throw new InvalidOperationException("parent already set");
            }

            item.ParentNode = _parent;
            item.HasChanges = true;

            _list.Insert(index, item);
        }

        /// <summary>
        /// Inserts the specified <see cref="XmlNode"/> immediately before the specified reference node.
        /// </summary>
        public void InsertBefore(XmlNode refItem, XmlNode item)
        {
            if (refItem == null)
            {
                throw new ArgumentNullException(nameof(refItem));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            int index = IndexOf(refItem);
            if (index != -1)
            {
                Insert(index, item);
            }
            else
            {
                throw new InvalidOperationException("Reference node not found.");
            }
        }

        /// <summary>
        /// Inserts the specified <see cref="XmlNode"/> immediately before the specified reference node.
        /// </summary>
        public void InsertAfter(XmlNode refItem, XmlNode item)
        {
            if (refItem == null)
            {
                throw new ArgumentNullException(nameof(refItem));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            int index = IndexOf(refItem);
            if (index != -1)
            {
                Insert(index + 1, item);
            }
            else
            {
                throw new InvalidOperationException("Reference node not found.");
            }
        }

        public bool Remove(XmlNode item)
        {
            return Remove(item, removeEmptyLineBefore: true);
        }

        public bool Remove(XmlNode item, bool removeEmptyLineBefore)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            int index = _list.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);

                if (removeEmptyLineBefore && index > 0 && index <= _list.Count && _list[index - 1] is XmlText t)
                {
                    string text = t.Text;
                    int pos = text.GetLastNonWhiteSpacePosition();

                    if (pos >= 0)
                    {
                        t.Text = text.Substring(0, pos + 1);
                    }
                    else
                    {
                        RemoveAt(index - 1);
                    }
                }

                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var item = _list[index];
            item.ParentNode = null;

            _list.RemoveAt(index);
            _parent.HasChanges = true;
        }

        public void Sort(IComparer<XmlNode> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            _list.Sort(comparer);
            _parent.HasChanges = true;
        }

        public IEnumerator<XmlNode> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}