namespace Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the <c>manifest.json</c> file located at the root of a connector script project (a subfolder of a
    /// protocol solution's <c>Scripts</c> folder).
    /// </summary>
    /// <remarks>
    /// See the Edge Node "Scripted Connector Packaging" documentation for the authoritative schema description.
    /// </remarks>
    public class Manifest
    {
        /// <summary>
        /// Gets or sets the version of the manifest schema (e.g. "1.0").
        /// </summary>
        [JsonPropertyName("schema_version")]
        public string SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the project metadata.
        /// </summary>
        [JsonPropertyName("project")]
        public ManifestProject Project { get; set; }

        /// <summary>
        /// Gets or sets the runtime configuration.
        /// </summary>
        [JsonPropertyName("runtime")]
        public ManifestRuntime Runtime { get; set; }

        /// <summary>
        /// Deserializes a <see cref="Manifest"/> from the contents of a <c>manifest.json</c> file.
        /// </summary>
        /// <param name="json">The raw JSON content.</param>
        /// <returns>The deserialized manifest.</returns>
        public static Manifest Parse(string json)
        {
            return JsonSerializer.Deserialize<Manifest>(json, ManifestJsonOptions.Default);
        }

        /// <summary>
        /// Serializes this <see cref="Manifest"/> to its <c>manifest.json</c> JSON representation.
        /// </summary>
        /// <returns>The JSON representation.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, ManifestJsonOptions.Default);
        }
    }
}
