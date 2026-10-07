namespace Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the <c>project</c> section of a connector script's <c>manifest.json</c> file.
    /// </summary>
    public class ManifestProject
    {
        /// <summary>
        /// Gets or sets the globally unique identifier of the connector script.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of the connector script.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the semantic version of the connector script.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets a short description explaining the connector's purpose.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the author or organization responsible for the connector.
        /// </summary>
        [JsonPropertyName("author")]
        public string Author { get; set; }
    }
}
