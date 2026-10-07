namespace Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the <c>runtime.python</c> section of a connector script's <c>manifest.json</c> file.
    /// </summary>
    public class ManifestPython
    {
        /// <summary>
        /// Gets or sets the Python version constraint required by the connector (e.g. <c>"&gt;=3.10;&lt;3.11"</c>).
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the path to the entry point script, relative to the package root (e.g. <c>"run/main.py"</c>).
        /// </summary>
        [JsonPropertyName("entry_point")]
        public string EntryPoint { get; set; }
    }
}
