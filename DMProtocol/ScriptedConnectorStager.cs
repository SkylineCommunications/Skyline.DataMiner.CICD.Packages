namespace Skyline.DataMiner.CICD.DMProtocol
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;
    using Skyline.DataMiner.CICD.DMProtocol.DependencyResolution;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    /// <summary>
    /// Stages the content of a scripted connector (Python Edge Node) project directory into a temporary folder shaped
    /// as a scripted connector package (<c>manifest.json</c>, an optional <c>README.md</c>, the <c>run/</c> folder,
    /// plus a resolved <c>dependencies/</c> folder), ready to be embedded into a <c>.dmprotocol</c> package under
    /// <c>Scripts/{guid}/</c>. Only these opted-in items are copied; development-only content such as
    /// <c>Tests/</c>, <c>requirements.txt</c> and the <c>.pyproj</c> file is intentionally left out of the package.
    /// </summary>
    internal class ScriptedConnectorStager
    {
        /// <summary>
        /// Factory class for staging scripted connector content.
        /// </summary>
        internal static class Factory
        {
            /// <summary>
            /// Stages the scripted connector content found in the specified project directory into a new temporary
            /// directory, resolving its Python dependencies in the process.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="sourceDirectory">
            /// Path to the scripted connector project directory (typically a <c>ScriptedConnector_{n}</c> folder at
            /// the solution root). It must contain a <c>manifest.json</c> file and a <c>run/</c> folder with the
            /// entry point script referenced by the manifest.
            /// </param>
            /// <param name="requirementsFilePath">
            /// Path to the <c>requirements.txt</c> file listing the connector's direct Python dependencies.
            /// </param>
            /// <param name="pythonVersion">
            /// Target Python version to resolve dependencies for (pip format, e.g. "3.14"). If not specified, it is
            /// derived from the manifest's <c>runtime.python.version</c> constraint when possible.
            /// </param>
            /// <param name="cancellationToken">A token to cancel the operation.</param>
            /// <returns>
            /// The full path of the temporary staging directory. The caller is responsible for deleting it once it is no
            /// longer needed.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="logCollector"/> or <paramref name="sourceDirectory"/> is <see langword="null"/>.</exception>
            /// <exception cref="System.IO.DirectoryNotFoundException">The directory specified in <paramref name="sourceDirectory"/> does not exist.</exception>
            /// <exception cref="InvalidManifestException">The <c>manifest.json</c> file is missing, malformed, or fails validation, or no <c>run/</c> folder is found.</exception>
            /// <exception cref="DependencyResolution.Exceptions.RequirementsNotFoundException">The <c>requirements.txt</c> file does not exist.</exception>
            /// <exception cref="DependencyResolution.Exceptions.ConflictingDependenciesException">Pip detected conflicting dependencies.</exception>
            /// <exception cref="DependencyResolution.Exceptions.PipNotFoundException">No valid pip executable could be found on the current system.</exception>
            public static Task<string> StageAsync(ILogCollector logCollector, string sourceDirectory, string requirementsFilePath, string pythonVersion = null, CancellationToken cancellationToken = default)
            {
                return StageAsync(logCollector, FileSystem.Instance, sourceDirectory, requirementsFilePath, pythonVersion, cancellationToken);
            }

            /// <summary>
            /// Stages the scripted connector content found in the specified project directory into a new temporary
            /// directory, resolving its Python dependencies in the process.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="fileSystem">The file system abstraction to use.</param>
            /// <param name="sourceDirectory">
            /// Path to the scripted connector project directory (typically a <c>ScriptedConnector_{n}</c> folder at
            /// the solution root). It must contain a <c>manifest.json</c> file and a <c>run/</c> folder with the
            /// entry point script referenced by the manifest.
            /// </param>
            /// <param name="requirementsFilePath">
            /// Path to the <c>requirements.txt</c> file listing the connector's direct Python dependencies.
            /// </param>
            /// <param name="pythonVersion">
            /// Target Python version to resolve dependencies for (pip format, e.g. "3.14"). If not specified, it is
            /// derived from the manifest's <c>runtime.python.version</c> constraint when possible.
            /// </param>
            /// <param name="cancellationToken">A token to cancel the operation.</param>
            /// <returns>
            /// The full path of the temporary staging directory. The caller is responsible for deleting it once it is no
            /// longer needed.
            /// </returns>
            internal static Task<string> StageAsync(ILogCollector logCollector, IFileSystem fileSystem, string sourceDirectory, string requirementsFilePath, string pythonVersion, CancellationToken cancellationToken)
            {
                if (logCollector == null) throw new ArgumentNullException(nameof(logCollector));
                if (fileSystem == null) throw new ArgumentNullException(nameof(fileSystem));
                if (String.IsNullOrWhiteSpace(sourceDirectory)) throw new ArgumentException("Source directory cannot be null, empty or whitespace.", nameof(sourceDirectory));
                if (String.IsNullOrWhiteSpace(requirementsFilePath)) throw new ArgumentException("Requirements file path cannot be null, empty or whitespace.", nameof(requirementsFilePath));

                sourceDirectory = fileSystem.Path.GetFullPath(sourceDirectory);
                requirementsFilePath = fileSystem.Path.GetFullPath(requirementsFilePath);

                if (!fileSystem.Directory.Exists(sourceDirectory))
                {
                    throw new System.IO.DirectoryNotFoundException($"Directory '{sourceDirectory}' not found.");
                }

                Manifest manifest = ManifestLoader.LoadAndValidate(fileSystem, sourceDirectory);

                string normalizedEntryPoint = manifest.Runtime.Python.EntryPoint.Replace('\\', '/');
                if (!normalizedEntryPoint.StartsWith("run/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidManifestException($"The entry point '{manifest.Runtime.Python.EntryPoint}' declared in manifest.json must be located under the 'run/' folder, since only 'run/' is included when the package is built.");
                }

                string entryPointPath = fileSystem.Path.Combine(sourceDirectory, normalizedEntryPoint.Replace('/', fileSystem.Path.DirectorySeparatorChar));
                if (!fileSystem.File.Exists(entryPointPath))
                {
                    throw new InvalidManifestException($"The entry point '{manifest.Runtime.Python.EntryPoint}' declared in manifest.json could not be found in '{sourceDirectory}'.");
                }

                string runSourceDirectory = fileSystem.Path.Combine(sourceDirectory, "run");
                if (!fileSystem.Directory.Exists(runSourceDirectory))
                {
                    throw new InvalidManifestException($"No 'run' folder found in '{sourceDirectory}'.");
                }

                if (String.IsNullOrWhiteSpace(pythonVersion))
                {
                    pythonVersion = PythonVersionConstraint.ExtractMinimumVersion(manifest.Runtime.Python.Version);
                }

                return StageValidatedAsync(logCollector, fileSystem, sourceDirectory, requirementsFilePath, runSourceDirectory, manifest, pythonVersion, cancellationToken);
            }

            /// <summary>
            /// Performs the actual (asynchronous) staging work, once all parameters and the manifest have already
            /// been validated by <see cref="StageAsync(ILogCollector, IFileSystem, string, string, string, CancellationToken)"/>.
            /// </summary>
            private static async Task<string> StageValidatedAsync(ILogCollector logCollector, IFileSystem fileSystem, string sourceDirectory, string requirementsFilePath, string runSourceDirectory, Manifest manifest, string pythonVersion, CancellationToken cancellationToken)
            {
                string stagingDirectory = fileSystem.Directory.CreateTemporaryDirectory();

                try
                {
                    // Only the opted-in package content is copied: manifest.json, an optional README.md, and the
                    // run/ folder (the entry point script and any other supporting files it needs). Development-only
                    // content (Tests/, requirements.txt, .pyproj, ...) is intentionally left out.
                    fileSystem.File.Copy(
                        fileSystem.Path.Combine(sourceDirectory, "manifest.json"),
                        fileSystem.Path.Combine(stagingDirectory, "manifest.json"));

                    string readmeSourcePath = fileSystem.Path.Combine(sourceDirectory, "README.md");
                    if (fileSystem.File.Exists(readmeSourcePath))
                    {
                        fileSystem.File.Copy(readmeSourcePath, fileSystem.Path.Combine(stagingDirectory, "README.md"));
                    }

                    string runTargetDirectory = fileSystem.Path.Combine(stagingDirectory, "run");
                    fileSystem.Directory.CreateDirectory(runTargetDirectory);
                    fileSystem.Directory.CopyRecursive(runSourceDirectory, runTargetDirectory, Array.Empty<string>());

                    // Resolve and download the Python dependencies into the dependencies/ folder.
                    var wheelsResolver = new WheelsResolver(logCollector, fileSystem, stagingDirectory);
                    var resolvedPlatforms = await wheelsResolver.DownloadWheelsAsync(requirementsFilePath, pythonVersion, cancellationToken).ConfigureAwait(false);

                    WarnAboutUnresolvedPlatforms(logCollector, manifest, resolvedPlatforms);

                    return stagingDirectory;
                }
                catch
                {
                    fileSystem.Directory.DeleteDirectory(stagingDirectory);
                    throw;
                }
            }

            private static void WarnAboutUnresolvedPlatforms(ILogCollector logCollector, Manifest manifest, System.Collections.Generic.IReadOnlyList<SupportedPlatform> resolvedPlatforms)
            {
                var declaredPlatforms = manifest.Runtime.SupportedPlatforms ?? Array.Empty<SupportedPlatform>();

                foreach (var declaredPlatform in declaredPlatforms)
                {
                    if (!resolvedPlatforms.Contains(declaredPlatform))
                    {
                        logCollector.ReportWarning($"No wheels could be resolved for platform '{declaredPlatform.ToTargetTriple()}' declared as supported in manifest.json.");
                    }
                }
            }
        }
    }
}
