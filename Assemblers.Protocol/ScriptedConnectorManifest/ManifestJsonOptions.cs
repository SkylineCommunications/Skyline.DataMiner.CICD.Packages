namespace Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest
{
    using System.Text.Json;

    /// <summary>
    /// Provides the shared <see cref="JsonSerializerOptions"/> used to (de)serialize <see cref="Manifest"/> instances.
    /// </summary>
    public static class ManifestJsonOptions
    {
        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> configured with the converters required to read and write <c>manifest.json</c> files.
        /// </summary>
        public static JsonSerializerOptions Default { get; } = Create();

        private static JsonSerializerOptions Create()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            options.Converters.Add(new RuntimeLanguageJsonConverter());
            options.Converters.Add(new SupportedPlatformJsonConverter());

            return options;
        }
    }
}
