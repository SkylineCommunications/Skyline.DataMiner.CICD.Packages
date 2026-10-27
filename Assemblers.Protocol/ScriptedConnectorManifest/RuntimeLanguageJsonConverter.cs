namespace Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Converts a <see cref="RuntimeLanguage"/> to and from its lowercase JSON string representation (e.g. <c>"python"</c>).
    /// </summary>
    internal sealed class RuntimeLanguageJsonConverter : JsonConverter<RuntimeLanguage>
    {
        /// <inheritdoc/>
        public override RuntimeLanguage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();

            if (String.Equals(value, "python", StringComparison.OrdinalIgnoreCase))
            {
                return RuntimeLanguage.Python;
            }

            throw new JsonException($"Unsupported runtime language '{value}'.");
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, RuntimeLanguage value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case RuntimeLanguage.Python:
                    writer.WriteStringValue("python");
                    break;

                default:
                    throw new JsonException($"Unsupported runtime language '{value}'.");
            }
        }
    }
}
