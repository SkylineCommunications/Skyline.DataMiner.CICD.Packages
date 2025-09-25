namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class XmlPlaceholder : XmlContainer
    {
        public override bool IsSubtreeValid => true;

        public IList<XmlNode> WrappedChildren => _children;

        public override int FirstCharOffset => WrappedChildren.First().FirstCharOffset;

        public override int LastCharOffset => WrappedChildren.Last().LastCharOffset;

        public override string GetXml() => throw new NotSupportedException();
    }
}