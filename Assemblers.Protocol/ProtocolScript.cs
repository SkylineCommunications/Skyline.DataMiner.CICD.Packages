namespace Skyline.DataMiner.CICD.Assemblers.Protocol
{
    using System;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;
    using Skyline.DataMiner.CICD.Parsers.Protocol.Xml.EdgeScripts;

    /// <summary>
    /// Represents a scripted connector (Python Edge Node script) project that is part of a protocol solution.
    /// </summary>
    public class ProtocolScript
    {
        internal ProtocolScript(EdgeScript edgeScript, string projectDirectory, string sourceDirectory, string requirementsFilePath, Manifest manifest)
        {
            EdgeScript = edgeScript ?? throw new ArgumentNullException(nameof(edgeScript));
            ProjectDirectory = projectDirectory ?? throw new ArgumentNullException(nameof(projectDirectory));
            SourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory));
            RequirementsFilePath = requirementsFilePath ?? throw new ArgumentNullException(nameof(requirementsFilePath));
            Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        }

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
        /// Gets the full path of the script's project directory (a <c>ScriptedConnector_{n}</c> folder at the solution
        /// root, mirroring how QAction projects are named <c>QAction_{n}</c> — the <c>{n}</c> suffix is just a
        /// sequential counter and does not correspond to the script's <c>id</c>/<c>guid</c>; the matching folder is
        /// found by its <c>src/manifest.json</c>'s <c>project.id</c>). This is the Visual Studio Python project
        /// folder: it contains the <c>.pyproj</c> file, <c>requirements.txt</c>, and a <c>src/</c> subfolder with the
        /// actual packaged content.
        /// </summary>
        public string ProjectDirectory { get; }

        /// <summary>
        /// Gets the full path of the script's source directory (the <c>src</c> subfolder of <see cref="ProjectDirectory"/>).
        /// Its content (<c>manifest.json</c>, <c>README.md</c>, <c>run/</c>) is copied as-is into the packaged
        /// scripted connector.
        /// </summary>
        public string SourceDirectory { get; }

        /// <summary>
        /// Gets the full path of the <c>requirements.txt</c> file, located directly under <see cref="ProjectDirectory"/>
        /// (a sibling of <c>src/</c>, not inside it).
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
    }
}
