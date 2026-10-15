namespace Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the <c>runtime</c> section of a connector script's <c>manifest.json</c> file.
    /// </summary>
    public class ManifestRuntime
    {
        /// <summary>
        /// Gets or sets the scripting language used by the connector. Currently, only <see cref="RuntimeLanguage.Python"/> is supported.
        /// </summary>
        [JsonPropertyName("language")]
        public RuntimeLanguage Language { get; set; }

        /// <summary>
        /// Gets or sets the platforms supported by this connector script.
        /// </summary>
        [JsonPropertyName("supported_platforms")]
        public SupportedPlatform[] SupportedPlatforms { get; set; }

        /// <summary>
        /// Gets or sets the Python runtime configuration.
        /// </summary>
        [JsonPropertyName("python")]
        public ManifestPython Python { get; set; }
    }
}
