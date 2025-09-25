namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class AttributeList : IList<XmlAttribute>
    {
        private readonly List<XmlAttribute> _list = new List<XmlAttribute>();
        private readonly XmlNode _parent;

        public AttributeList(XmlNode parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public AttributeList(XmlNode parent, IEnumerable<Xml.XmlAttribute> attributes)
            : this(parent)
        {
            foreach (var attr in attributes)
            {
                var attribute = new XmlAttribute(attr)
                {
                    ParentNode = parent
                };
                _list.Add(attribute);
            }
        }

        public XmlAttribute this[int index]
        {
            get => _list[index];
            set
            {
                if (index < 0 || index >= _list.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                var item = _list[index];
                item.ParentNode = null;

                _list[index] = value;
                value.ParentNode = _parent;
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Add(XmlAttribute item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.ParentNode != null)
            {
                throw new InvalidOperationException("parent already set");
            }

            _list.Add(item);
            item.ParentNode = _parent;
            item.HasChanges = true;
        }

        public void Add(string name, string value)
        {
            Add(new XmlAttribute(name, value));
        }

        public XmlAttribute GetOrAdd(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var attr = _list.FirstOrDefault(x => String.Equals(x.Name, name));
            if (attr == null)
            {
                attr = new XmlAttribute(name, "");
                Add(attr);
            }

            return attr;
        }

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

        public bool Contains(XmlAttribute item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(XmlAttribute[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int IndexOf(XmlAttribute item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, XmlAttribute item)
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

            _list.Insert(index, item);
            item.ParentNode = _parent;
            item.HasChanges = true;
        }

        public bool Remove(XmlAttribute item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_list.Remove(item))
            {
                item.ParentNode = null;
                _parent.HasChanges = true;
                return true;
            }

            return false;
        }

        public bool Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var item = _list.FirstOrDefault(x => String.Equals(x.Name, name));
            if (item != null)
            {
                return Remove(item);
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var attribute = _list[index];
            attribute.ParentNode = null;
            _list.RemoveAt(index);
            _parent.HasChanges = true;
        }

        public IEnumerator<XmlAttribute> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}