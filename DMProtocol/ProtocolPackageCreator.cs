namespace Skyline.DataMiner.CICD.DMProtocol
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.Assemblers.Protocol;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Models.Protocol.Read;
    using Skyline.DataMiner.CICD.FileSystem;
    using FileInfo = Skyline.DataMiner.CICD.FileSystem.FileInfoWrapper.FileInfo;

    /// <summary>
    /// Package creator for Protocol solutions.
    /// </summary>
    public class ProtocolPackageCreator
    {
        /// <summary>
        /// Factory class for Protocol solution package creators.
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// Creates an <see cref="IAppPackageProtocol"/> instance from the specified repository with the specified name and version.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="repositoryPath">The path of the repository that contains the Protocol solution.</param>
            /// <returns>The <see cref="IAppPackageProtocol"/> instance.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="logCollector"/>, <paramref name="repositoryPath"/> is <see langword="null"/>.</exception>
            /// <exception cref="System.IO.DirectoryNotFoundException">The directory specified in <paramref name="repositoryPath"/> does not exist.</exception>
            /// <exception cref="AssemblerException">Project with name could not be found.</exception>
            /// <exception cref="InvalidOperationException">The protocol does not have a name specified in the Name tag.
            /// -or-
            /// The protocol does not have a version specified in the Version tag.</exception>
            public static async Task<IAppPackageProtocol> FromRepositoryAsync(ILogCollector logCollector, string repositoryPath)
            {
                return await FromRepositoryAsync(logCollector, repositoryPath, String.Empty);
            }

            /// <summary>
            /// Creates an <see cref="IAppPackageProtocol"/> instance from the specified repository with the specified name and version.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="repositoryPath">The path of the repository that contains the Protocol solution.</param>
            /// <param name="versionOverride">Override the version in the protocol.</param>
            /// <returns>The <see cref="IAppPackageProtocol"/> instance.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="logCollector"/>, <paramref name="repositoryPath"/> is <see langword="null"/>.</exception>
            /// <exception cref="DirectoryNotFoundException">The directory specified in <paramref name="repositoryPath"/> does not exist.</exception>
            /// <exception cref="AssemblerException">Project with name could not be found.</exception>
            /// <exception cref="InvalidOperationException">The protocol does not have a name specified in the Name tag.
            /// -or-
            /// The protocol does not have a version specified in the Version tag.</exception>
            public static async Task<IAppPackageProtocol> FromRepositoryAsync(ILogCollector logCollector, string repositoryPath, string versionOverride)
            {
                return await FromRepositoryAsync(logCollector, repositoryPath, versionOverride, null);
            }

            /// <summary>
            /// Creates an <see cref="IAppPackageProtocol"/> instance from the specified repository with the specified name and version.
            /// If the protocol solution declares any scripted connectors (see <see cref="ProtocolSolution.Scripts"/>), their Python
            /// dependencies are resolved and embedded into the resulting package under <c>Scripts/{guid}/</c>.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="repositoryPath">The path of the repository that contains the Protocol solution.</param>
            /// <param name="versionOverride">Override the version in the protocol.</param>
            /// <param name="pythonVersion">
            /// Target Python version to resolve scripted connector dependencies for (pip format, e.g. "3.14"). If not
            /// specified, it is derived per-script from its manifest's <c>runtime.python.version</c> constraint. Ignored
            /// if the protocol does not declare any scripted connectors.
            /// </param>
            /// <returns>The <see cref="IAppPackageProtocol"/> instance.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="logCollector"/>, <paramref name="repositoryPath"/> is <see langword="null"/>.</exception>
            /// <exception cref="DirectoryNotFoundException">The directory specified in <paramref name="repositoryPath"/> does not exist.</exception>
            /// <exception cref="AssemblerException">Project with name could not be found.</exception>
            /// <exception cref="InvalidOperationException">The protocol does not have a name specified in the Name tag.
            /// -or-
            /// The protocol does not have a version specified in the Version tag.</exception>
            /// <exception cref="Assemblers.Protocol.ScriptedConnectorManifest.InvalidManifestException">A declared script's <c>manifest.json</c> file is missing, malformed, or fails validation.</exception>
            /// <exception cref="DependencyResolution.Exceptions.RequirementsNotFoundException">A declared script's <c>requirements.txt</c> file does not exist.</exception>
            /// <exception cref="DependencyResolution.Exceptions.ConflictingDependenciesException">Pip detected conflicting dependencies for a declared script.</exception>
            /// <exception cref="DependencyResolution.Exceptions.PipNotFoundException">No valid pip executable could be found on the current system.</exception>
            public static Task<IAppPackageProtocol> FromRepositoryAsync(ILogCollector logCollector, string repositoryPath, string versionOverride, string pythonVersion)
            {
                if (repositoryPath == null) throw new ArgumentNullException(nameof(repositoryPath));

                repositoryPath = FileSystem.Instance.Path.GetFullPath(repositoryPath);

                if (String.IsNullOrWhiteSpace(repositoryPath)) throw new ArgumentException("Invalid repository path", nameof(repositoryPath));
                if (!FileSystem.Instance.Directory.Exists(repositoryPath)) throw new System.IO.DirectoryNotFoundException($"Directory '{repositoryPath}' not found.");

                return FromValidatedRepositoryAsync(logCollector, repositoryPath, versionOverride, pythonVersion);
            }

            /// <summary>
            /// Builds the package from a <paramref name="repositoryPath"/> that has already been validated (non-null,
            /// full path, existing directory) by <see cref="FromRepositoryAsync(ILogCollector, string, string, string)"/>.
            /// </summary>
            private static async Task<IAppPackageProtocol> FromValidatedRepositoryAsync(ILogCollector logCollector, string repositoryPath, string versionOverride, string pythonVersion)
            {
                string solutionFilePath = FileSystem.Instance.Directory.GetFiles(repositoryPath, "*.sln", System.IO.SearchOption.TopDirectoryOnly)
                                                    .Concat(FileSystem.Instance.Directory.GetFiles(repositoryPath, "*.slnx",
                                                        SearchOption.TopDirectoryOnly))
                                                    .FirstOrDefault();

                if (solutionFilePath == null) throw new InvalidOperationException("The specified repository path does not contain a solution (.sln or .slnx) file in the root folder.");

                string destinationDllFolder = "C:\\Skyline DataMiner\\ProtocolScripts\\DllImport";

                ProtocolSolution solution = ProtocolSolution.Load(solutionFilePath, logCollector);
                ProtocolModel protocolModel = new ProtocolModel(solution.ProtocolDocument);

                string protocolName = protocolModel.Protocol?.Name?.Value;
                string protocolVersion = protocolModel.Protocol?.Version?.Value;

                if (protocolName == null) throw new InvalidOperationException("The protocol does not have a name specified in the Name tag.");
                if (protocolVersion == null) throw new InvalidOperationException("The protocol does not have a version specified in the Version tag.");

                ProtocolBuilder protocolBuilder;
                if (!String.IsNullOrWhiteSpace(versionOverride))
                {
                    protocolVersion = versionOverride;
                    protocolBuilder = new ProtocolBuilder(solution, logCollector, versionOverride);
                }
                else
                {
                    protocolBuilder = new ProtocolBuilder(solution, logCollector);
                }
                
                var buildResultItems = await protocolBuilder.BuildAsync();
                string document = buildResultItems.Document;
                byte[] bytes = Encoding.UTF8.GetBytes(document);

                IAppPackageProtocolBuilder packageBuilder = new AppPackageProtocol.AppPackageProtocolBuilder(protocolName, protocolVersion, bytes);

                AddNuGetAssemblies(buildResultItems, destinationDllFolder, packageBuilder);

                string dllsFolder = FileSystem.Instance.Path.Combine(repositoryPath, "Dlls");
                AddAssemblies(dllsFolder, packageBuilder, destinationDllFolder, repositoryPath);

                AddTemplates(solution, packageBuilder);

                IAppPackageProtocol protocolPackage = packageBuilder.Build();

                if (solution.Scripts.Count > 0)
                {
                    protocolPackage = await EmbedScriptsAsync(logCollector, solution, protocolPackage, pythonVersion).ConfigureAwait(false);
                }

                return protocolPackage;
            }

            /// <summary>
            /// Resolves the Python dependencies of each script declared in <paramref name="solution"/> and embeds the
            /// resulting scripted connector content into <paramref name="basePackage"/>'s package bytes, under
            /// <c>Scripts/{guid}/</c>.
            /// </summary>
            private static async Task<IAppPackageProtocol> EmbedScriptsAsync(ILogCollector logCollector, ProtocolSolution solution, IAppPackageProtocol basePackage, string pythonVersion)
            {
                byte[] baseBytes = basePackage.CreatePackage();

                var stagingDirectories = new List<string>();

                try
                {
                    byte[] mergedBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        memoryStream.Write(baseBytes, 0, baseBytes.Length);

                        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Update, leaveOpen: true))
                        {
                            foreach (var script in solution.Scripts)
                            {
                                string stagingDirectory = await ScriptedConnectorStager.Factory.StageAsync(logCollector, script.ProjectDirectory, script.RequirementsFilePath, pythonVersion).ConfigureAwait(false);
                                stagingDirectories.Add(stagingDirectory);

                                AddDirectoryToArchive(archive, stagingDirectory, $"Scripts/{script.Guid}");
                            }
                        }

                        mergedBytes = memoryStream.ToArray();
                    }

                    return new ScriptEmbeddingAppPackageProtocol(basePackage, mergedBytes);
                }
                finally
                {
                    foreach (var stagingDirectory in stagingDirectories)
                    {
                        FileSystem.Instance.Directory.DeleteDirectory(stagingDirectory);
                    }
                }
            }

            /// <summary>
            /// Recursively adds the content of <paramref name="sourceDirectory"/> to <paramref name="archive"/>, with each
            /// entry prefixed by <paramref name="entryPrefix"/>.
            /// </summary>
            private static void AddDirectoryToArchive(ZipArchive archive, string sourceDirectory, string entryPrefix)
            {
                string normalizedRoot = FileSystem.Instance.Path.GetFullPath(sourceDirectory).TrimEnd('\\', '/');

                foreach (var filePath in FileSystem.Instance.Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
                {
                    string relativePath = filePath.Substring(normalizedRoot.Length).TrimStart('\\', '/').Replace('\\', '/');
                    string entryName = $"{entryPrefix}/{relativePath}";

                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    byte[] fileBytes = FileSystem.Instance.File.ReadAllBytes(filePath);

                    using (var entryStream = entry.Open())
                    {
                        entryStream.Write(fileBytes, 0, fileBytes.Length);
                    }
                }
            }

            private static void AddTemplates(ProtocolSolution solution, IAppPackageProtocolBuilder packageBuilder)
            {
                string templatesFolder = FileSystem.Instance.Path.Combine(solution.SolutionDirectory, "DefaultTemplates");
                if (!Directory.Exists(templatesFolder))
                {
                    // No templates folder found.
                    return;
                }

                FileInfo alarmTemplateFile = new FileInfo(Path.Combine(templatesFolder, "Template_Alarm_Default.xml"));
                FileInfo trendTemplateFile = new FileInfo(Path.Combine(templatesFolder, "Trending_Template_Default.xml"));
                FileInfo informationTemplateFile = new FileInfo(Path.Combine(templatesFolder, "Information_Template_Default.xml"));

                if (alarmTemplateFile.Exists)
                {
                    packageBuilder.WithAlarmTemplate(alarmTemplateFile.FullName, true);
                }

                if (trendTemplateFile.Exists)
                {
                    packageBuilder.WithTrendTemplate(trendTemplateFile.FullName, true);
                }

                if (informationTemplateFile.Exists)
                {
                    // Will not be part of the protocol package, but is already future-proof.
                    packageBuilder.WithInformationTemplate(informationTemplateFile.FullName, true);
                }
            }

            private static void AddNuGetAssemblies(BuildResultItems buildResultItems, string destinationDllFolder, IAppPackageProtocolBuilder packageBuilder)
            {
                foreach (var assembly in buildResultItems.Assemblies)
                {
                    // Can be null in cases where a DataMiner DLL must be included in the dllImport attribute but must not be included in the Dlls folder.
                    if (assembly.AssemblyPath != null)
                    {
                        string destinationFilePath = FileSystem.Instance.Path.Combine(destinationDllFolder, assembly.DllImport);
                        string destinationFolderPath = FileSystem.Instance.Path.GetDirectoryName(destinationFilePath);
                        packageBuilder.WithAssembly(assembly.AssemblyPath, destinationFolderPath);
                    }
                }
            }

            /// <summary>
            /// Copies all the DLLs from the DLLs folder in case of dependencies (e.g.: NPOI).
            /// </summary>
            /// <param name="dllsFolder">DLLs folder in the solution.</param>
            /// <param name="packageBuilder">Protocol Package Builder.</param>
            /// <param name="destinationDllFolder">Destination folder on DataMiner.</param>
            /// <param name="repositoryPath">Solution location.</param>
            private static void AddAssemblies(string dllsFolder, IAppPackageProtocolBuilder packageBuilder, string destinationDllFolder, string repositoryPath)
            {
                string dllsFolderPath = FileSystem.Instance.Path.Combine(FileSystem.Instance.Path.GetFullPath(repositoryPath), "Dlls");

                if (FileSystem.Instance.Directory.Exists(dllsFolderPath))
                {
                    foreach (var dll in FileSystem.Instance.Directory.EnumerateFiles(dllsFolder, "*.dll"))
                    {
                        packageBuilder.WithAssembly(dll, destinationDllFolder);
                    }
                }
            }
        }
    }
}
