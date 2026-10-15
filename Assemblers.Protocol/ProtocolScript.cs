namespace Skyline.DataMiner.CICD.Assemblers.Protocol
{
    using System;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;
    using Skyline.DataMiner.CICD.Parsers.Protocol.Xml.EdgeScripts;

    /// <summary>
    /// Represents a connector script project that is part of a protocol solution.
    /// </summary>
    public class ProtocolScript
    {
        /// <summary>
        /// Gets the identifier of the script, as declared in protocol.xml's <c>&lt;Script id="..."&gt;</c> attribute.
        /// </summary>
        public string Id => EdgeScript.Id;

        /// <summary>
        /// Gets the display name of the script, as declared in protocol.xml's <c>&lt;Script displayName="..."&gt;</c> attribute.
        /// </summary>
        public string DisplayName => EdgeScript.DisplayName;

        /// <summary>
        /// Gets the globally unique identifier of the script. Matches both protocol.xml's <c>&lt;Script guid="..."&gt;</c>
        /// attribute and the script's <c>manifest.json</c> <c>project.id</c> field.
        /// </summary>
        public Guid Guid => EdgeScript.Guid;

        /// <summary>
        /// Gets the full path of the script's project directory (a <c>ConnectorScript_{n}</c> folder at the
        /// solution root). This is the Visual Studio Python project folder.
        /// </summary>
        public string ProjectDirectory { get; }

        /// <summary>
        /// Gets the full path of the <c>requirements.txt</c> file, located directly under <see cref="ProjectDirectory"/>.
        /// </summary>
        public string RequirementsFilePath { get; }

        /// <summary>
        /// Gets the parsed and validated <c>manifest.json</c> of the script.
        /// </summary>
        public Manifest Manifest { get; }

        /// <summary>
        /// Gets the underlying protocol.xml declaration.
        /// </summary>
        public EdgeScript EdgeScript { get; }

        internal ProtocolScript(EdgeScript edgeScript, string projectDirectory, string requirementsFilePath, Manifest manifest)
        {
            EdgeScript = edgeScript ?? throw new ArgumentNullException(nameof(edgeScript));
            ProjectDirectory = projectDirectory ?? throw new ArgumentNullException(nameof(projectDirectory));
            RequirementsFilePath = requirementsFilePath ?? throw new ArgumentNullException(nameof(requirementsFilePath));
            Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        }
    }
}
