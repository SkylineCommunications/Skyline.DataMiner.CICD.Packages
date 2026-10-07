namespace Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest
{
    using System;
    using System.IO;
    using System.Linq;

    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Loads and validates the <c>manifest.json</c> file of a connector script project.
    /// </summary>
    public static class ManifestLoader
    {
        /// <summary>
        /// Loads and validates the <c>manifest.json</c> file located in <paramref name="sourceDirectory"/>.
        /// </summary>
        /// <param name="sourceDirectory">Path to the connector script source directory.</param>
        /// <returns>The parsed and validated <see cref="Manifest"/>.</returns>
        /// <exception cref="InvalidManifestException">The <c>manifest.json</c> file is missing, malformed, or fails validation.</exception>
        public static Manifest LoadAndValidate(string sourceDirectory)
        {
            return LoadAndValidate(FileSystem.Instance, sourceDirectory);
        }

        /// <summary>
        /// Loads and validates the <c>manifest.json</c> file located in <paramref name="sourceDirectory"/>.
        /// </summary>
        /// <param name="fileSystem">The file system abstraction to use.</param>
        /// <param name="sourceDirectory">Path to the connector script source directory.</param>
        /// <returns>The parsed and validated <see cref="Manifest"/>.</returns>
        /// <exception cref="InvalidManifestException">The <c>manifest.json</c> file is missing, malformed, or fails validation.</exception>
        public static Manifest LoadAndValidate(IFileSystem fileSystem, string sourceDirectory)
        {
            string manifestPath = fileSystem.Path.Combine(sourceDirectory, "manifest.json");

            if (!fileSystem.File.Exists(manifestPath))
            {
                throw new InvalidManifestException($"No manifest.json file found in '{sourceDirectory}'.");
            }

            string manifestContent = fileSystem.File.ReadAllText(manifestPath);

            Manifest manifest;
            try
            {
                manifest = Manifest.Parse(manifestContent);
            }
            catch (Exception ex)
            {
                throw new InvalidManifestException($"Failed to parse manifest.json: {ex.Message}", ex);
            }

            Validate(manifest);

            return manifest;
        }

        /// <summary>
        /// Validates that the required fields of a <see cref="Manifest"/> are present.
        /// </summary>
        /// <param name="manifest">The manifest to validate.</param>
        /// <exception cref="InvalidManifestException">The manifest fails validation.</exception>
        internal static void Validate(Manifest manifest)
        {
            if (manifest == null)
            {
                throw new InvalidManifestException("The manifest.json file is empty.");
            }

            if (String.IsNullOrWhiteSpace(manifest.SchemaVersion))
            {
                throw new InvalidManifestException("The manifest.json file is missing the required 'schema_version' field.");
            }

            if (manifest.Project == null || String.IsNullOrWhiteSpace(manifest.Project.Name))
            {
                throw new InvalidManifestException("The manifest.json file is missing the required 'project.name' field.");
            }

            if (manifest.Project.Id == Guid.Empty)
            {
                throw new InvalidManifestException("The manifest.json file is missing the required 'project.id' field.");
            }

            if (String.IsNullOrWhiteSpace(manifest.Project.Version))
            {
                throw new InvalidManifestException("The manifest.json file is missing the required 'project.version' field.");
            }

            if (manifest.Runtime == null || manifest.Runtime.Language != RuntimeLanguage.Python)
            {
                throw new InvalidManifestException("The manifest.json file must declare 'runtime.language' as 'python'.");
            }

            if (manifest.Runtime.SupportedPlatforms == null || manifest.Runtime.SupportedPlatforms.Length == 0)
            {
                throw new InvalidManifestException("The manifest.json file must declare at least one entry in 'runtime.supported_platforms'.");
            }

            if (manifest.Runtime.Python == null || String.IsNullOrWhiteSpace(manifest.Runtime.Python.EntryPoint))
            {
                throw new InvalidManifestException("The manifest.json file is missing the required 'runtime.python.entry_point' field.");
            }

            ValidateEntryPoint(manifest.Runtime.Python.EntryPoint);

            if (String.IsNullOrWhiteSpace(manifest.Runtime.Python.Version))
            {
                throw new InvalidManifestException("The manifest.json file is missing the required 'runtime.python.version' field.");
            }
        }

        /// <summary>
        /// Validates that the declared entry point is a relative path that stays within the connector script's
        /// source directory (no path traversal, no rooted/absolute paths).
        /// </summary>
        /// <param name="entryPoint">The <c>runtime.python.entry_point</c> value to validate.</param>
        /// <exception cref="InvalidManifestException">The entry point is rooted, or contains '.' or '..' path segments.</exception>
        private static void ValidateEntryPoint(string entryPoint)
        {
            if (Path.IsPathRooted(entryPoint))
            {
                throw new InvalidManifestException($"The 'runtime.python.entry_point' value '{entryPoint}' must be a relative path.");
            }

            string[] segments = entryPoint.Split(new[] { '/', '\\' });

            if (segments.Any(segment => segment is "." or ".."))
            {
                throw new InvalidManifestException($"The 'runtime.python.entry_point' value '{entryPoint}' must not contain '.' or '..' path segments.");
            }
        }
    }
}
