namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IXmlNode
    {
        int Depth { get; }
    }

    public interface IXmlNode<out TData> : IXmlNode
        where TData : Xml.XmlNode
    {
        TData Data { get; }
    }

    public abstract class XmlNode : XmlNode<Xml.XmlNode>
    {
        protected XmlNode(Xml.XmlNode data) : base(data)
        {

        }
    }

    public abstract class XmlNode<TData> : IXmlNode<TData> where TData : Xml.XmlNode
    {
        /// <summary>
        /// Contains the parent node.
        /// </summary>
        public XmlNode ParentNode { get; internal set; }

        public TData Data { get; }

        public AttributeList Attributes { get; protected set; }

        protected XmlNode(TData data)
        {
            Data = data;
        }

        /// <summary>
        /// Returns the depth in the XML tree of this node. The XmlDocument root element has depth 0. (calculated)
        /// </summary>
        public int Depth => ParentNode == null ? 0 : ParentNode.Depth + 1;

        private bool _hasChanges;

        /// <summary>
        /// When true, this node or it's subtree has changes.
        /// </summary>
        public bool HasChanges
        {
            get => _hasChanges;
            internal set
            {
                _hasChanges = value;
                if (_hasChanges && ParentNode != null)
                {
                    ParentNode.HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Formats this <see cref="XmlNode"/> and its children.
        /// </summary>
        public abstract void Format();

        internal abstract void FormatInternal();

        /// <summary>
        /// Creates an exact copy of this <see cref="XmlNode"/>.
        /// </summary>
        public abstract XmlNode Clone<T>() where T : XmlNode;

        public abstract string GetXml();
    }

    public abstract class XmlNode<TData, TNode> : XmlNode
        where TData : Xml.XmlNode
        where TNode : XmlNode
    {
        public new TData Data => (TData)base.Data;

        protected XmlNode(TData data) : base(data)
        {
        }

        /// <summary>
        /// Creates an exact copy of this <see cref="XmlNode"/>.
        /// </summary>
        public override XmlNode Clone<T>()
        {
            return Clone();
        }

        /// <summary>
        /// Creates an exact copy of this <see cref="XmlNode"/>.
        /// </summary>
        public abstract TNode Clone();

        /// <summary>
        /// Tries to find the given node in the subtree of this XML node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public XmlNode TryFindNode(Xml.XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            Stack<Xml.XmlNode> path = new Stack<Xml.XmlNode>();

            var parent = node;
            while (parent != null)
            {
                if (parent == this.Data)
                {
                    break; // we've found the common base node (= this object)!
                }

                path.Push(parent);
                parent = parent.ParentNode;
            }

            XmlNode n = this;

            while (path.Count > 0 && n is IXmlContainer c)
            {
                var p = path.Pop();
                n = c.Children.FirstOrDefault(x => x.Data == p);
            }

            if (n?.Data == node)
            {
                return n; // we've found what we are looking for!
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Tries to find the given node in the subtree of this XML node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public XmlElement TryFindNode(Xml.XmlElement node)
        {
            return TryFindNode((Xml.XmlNode)node) as XmlElement;
        }
    }
}