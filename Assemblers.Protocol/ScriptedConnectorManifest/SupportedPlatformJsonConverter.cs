namespace Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Converts a <see cref="SupportedPlatform"/> to and from its target triple JSON string representation (e.g. <c>"x86_64-windows-msvc"</c>).
    /// </summary>
    internal sealed class SupportedPlatformJsonConverter : JsonConverter<SupportedPlatform>
    {
        /// <inheritdoc/>
        public override SupportedPlatform Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            return SupportedPlatformExtensions.FromTargetTriple(value);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, SupportedPlatform value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToTargetTriple());
        }
    }
}
