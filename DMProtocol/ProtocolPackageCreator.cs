namespace Skyline.DataMiner.CICD.DMProtocol
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;
    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.Assemblers.Protocol;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Models.Protocol.Read;
    using Skyline.DataMiner.CICD.Parsers.Protocol.VisualStudio;
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
                if (repositoryPath == null) throw new ArgumentNullException(nameof(repositoryPath));

                repositoryPath = FileSystem.Instance.Path.GetFullPath(repositoryPath);

                if (String.IsNullOrWhiteSpace(repositoryPath)) throw new ArgumentException("Invalid repository path", nameof(repositoryPath));
                if (!FileSystem.Instance.Directory.Exists(repositoryPath)) throw new System.IO.DirectoryNotFoundException($"Directory '{repositoryPath}' not found.");

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

                return protocolPackage;
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
