namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml.EdgeScripts
{
    using System;

    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    /// <summary>
    /// Represents a <c>&lt;Script&gt;</c> entry declared under <c>&lt;Protocol&gt;&lt;Edge&gt;&lt;Scripts&gt;</c> in protocol.xml.
    /// </summary>
    public class EdgeScript
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeScript"/> class based on the specified XML node.
        /// </summary>
        /// <param name="node">The <c>&lt;Script&gt;</c> XML node.</param>
        /// <exception cref="Skyline.DataMiner.CICD.Parsers.Common.Exceptions.ParserException">The node does not have a valid <c>guid</c> attribute.</exception>
        public EdgeScript(XmlElement node)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));

            Id = node.GetAttributeValue("id");
            DisplayName = node.GetAttributeValue("displayName");

            string guidValue = node.GetAttributeValue("guid");
            if (!Guid.TryParse(guidValue, out Guid guid))
            {
                throw new Skyline.DataMiner.CICD.Parsers.Common.Exceptions.ParserException($"Script '{Id}' does not have a valid 'guid' attribute.");
            }

            Guid = guid;
        }

        /// <summary>
        /// Gets the identifier of the script, as declared in the <c>id</c> attribute.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the display name of the script, as declared in the <c>displayName</c> attribute.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the globally unique identifier of the script, as declared in the <c>guid</c> attribute. This must match
        /// the <c>project.id</c> field of the corresponding scripted connector's <c>manifest.json</c> file.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Gets the underlying XML node.
        /// </summary>
        public XmlElement Node { get; }
    }
}
