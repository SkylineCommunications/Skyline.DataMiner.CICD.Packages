namespace Skyline.DataMiner.CICD.DMApp.Automation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.Assemblers.Automation;
    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Parsers.Automation.VisualStudio;
    using Skyline.DataMiner.CICD.Parsers.Automation.Xml;

    /// <summary>
    /// Package creator for Automation script solutions.
    /// </summary>
    public class AppPackageCreatorForAutomation : AppPackageCreator
    {
        private readonly IEnumerable<string> _scriptNames;

        private AppPackageCreatorForAutomation(IFileSystem fileSystem, ILogCollector logCollector, string repositoryPath, string packageName, DMAppVersion packageVersion, IEnumerable<string> scriptNames)
            : base(fileSystem, logCollector, repositoryPath, packageName, packageVersion)
        {
            _scriptNames = scriptNames;
        }

        /// <summary>
        /// Adds the relevant items to the specified package builder.
        /// </summary>
        /// <param name="appPackageBuilder">The package builder to which the items should be added to.</param>
        /// <returns>The package creator.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="appPackageBuilder"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">No solution (.sln) file was detected or multiple solutions were detected.</exception>
        public override async Task AddItemsAsync(AppPackage.AppPackageBuilder appPackageBuilder)
        {
            if (appPackageBuilder == null)
            {
                throw new ArgumentNullException(nameof(appPackageBuilder));
            }

            LogCollector.ReportStatus("Using Workspace: " + RepositoryPath);
            string[] files = FileSystem.Directory.GetFiles(RepositoryPath, "*.sln", SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
                throw new InvalidOperationException("No solution file (.sln) detected in " + RepositoryPath + ".");
            if (files.Length > 1)
                throw new InvalidOperationException("More than one solution file (.sln) detected in " + RepositoryPath + ".");

            HashSet<string> namesOfScriptsToInclude = null;

            if (_scriptNames != null)
            {
                namesOfScriptsToInclude = new HashSet<string>(_scriptNames);
            }

            string solutionPath = files[0];
            LogCollector.ReportStatus("Using Solution: " + solutionPath);
            var solution = AutomationScriptSolution.Load(solutionPath, LogCollector);
            var builder = new AutomationScriptSolutionBuilder(solution, LogCollector);

            var result = await builder.BuildAsync();

            if (result.Count == 0)
            {
                LogCollector.ReportError("Building AutomationScript Solution had zero scripts as a result");
            }

            for (int i = 0; i < result.Count; i++)
            {
                var script = result[i];

                if (namesOfScriptsToInclude != null && !namesOfScriptsToInclude.Contains(script.Key.Name))
                {
                    continue;
                }

                // Convert to byte[].
                var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(true)))
                {
                    streamWriter.Write(script.Value.Document);
                }

                byte[] content = memoryStream.ToArray();

                LogCollector.ReportStatus("With Script: " + script.Key.Name);
                IAppPackageAutomationScriptBuilder appPackageAutomationScriptBuilder = new AppPackageAutomationScript.AppPackageAutomationScriptBuilder(script.Key.Name, PackageVersion.ToString(), content);

                var automationScriptBuilderHelper = new AppPackageAutomationScriptBuilderHelper();
                string dllsFolderPath = FileSystem.Path.Combine(RepositoryPath, "Dlls");
                var dllFilesPath = FileSystem.Directory.EnumerateFiles(dllsFolderPath, "*.dll", SearchOption.AllDirectories).ToArray();
                
                AddScriptAssemblies(automationScriptBuilderHelper, content, dllFilesPath, appPackageAutomationScriptBuilder);
                AddNuGetAssemblies(script, appPackageAutomationScriptBuilder);

                // Build
                var appPackageAutomationScript = appPackageAutomationScriptBuilder.Build();

                // Expecting a folder called CompanionFiles under the repository. Behaves the same as the Content folder from the CompanionFiles Repositories.
                string companionFilesPath = FileSystem.Path.Combine(RepositoryPath, "CompanionFiles");
                if (FileSystem.Directory.Exists(companionFilesPath))
                {
                    appPackageBuilder.WithCompanionFiles(companionFilesPath);
                }

                appPackageBuilder.WithAutomationScript(appPackageAutomationScript);
            }
        }

        private void AddNuGetAssemblies(KeyValuePair<Script, BuildResultItems> script, IAppPackageAutomationScriptBuilder appPackageAutomationScriptBuilder)
        {
            var nugetAssemblies = script.Value.Assemblies;

            foreach (var nugetAssembly in nugetAssemblies)
            {
                if (nugetAssembly.AssemblyPath == null)
                {
                    continue;
                }

                var destinationFolderPath = FileSystem.Path.Combine(@"C:\Skyline DataMiner\ProtocolScripts\DllImport", nugetAssembly.DllImport);
                LogCollector.ReportStatus("     With destinationFolderPath: " + destinationFolderPath);
                var destinationDirectory = FileSystem.Path.GetDirectoryName(destinationFolderPath);

                LogCollector.ReportStatus("     With Assembly: " + destinationDirectory);
                appPackageAutomationScriptBuilder.WithAssembly(nugetAssembly.AssemblyPath, destinationDirectory);
            }
        }

        private static void AddScriptAssemblies(AppPackageAutomationScriptBuilderHelper automationScriptBuilderHelper, byte[] content, string[] dllFilesPath,
            IAppPackageAutomationScriptBuilder appPackageAutomationScriptBuilder)
        {
            var assemblies = automationScriptBuilderHelper.ExtractAssemblies(content, dllFilesPath);
            try
            {
                foreach (var (assemblyFilePath, destinationFolderPath) in assemblies)
                {
                    // Look lower, there's a second one for NuGets!
                    appPackageAutomationScriptBuilder.WithAssembly(assemblyFilePath, destinationFolderPath);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Problem Caught with Script Assemblies: " + e);
            }
        }

        /// <summary>
        /// Factory class for Automation script solution app package creators.
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// Creates an <see cref="IAppPackageCreator"/> instance from the specified repository with the specified name and version.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="repositoryPath">The path of the repository that contains the Automation script solution.</param>
            /// <param name="packageName">The package name.</param>
            /// <param name="packageVersion">The package version.</param>
            /// <returns>The <see cref="IAppPackageCreator"/> instance.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="logCollector"/>, <paramref name="repositoryPath"/>, <paramref name="packageName"/>, <paramref name="packageVersion"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="packageName"/>, <paramref name="packageVersion"/> is empty or whitespace.</exception>
            /// <exception cref="DirectoryNotFoundException">The directory specified in <paramref name="repositoryPath"/> does not exist.</exception>
            public static IAppPackageCreator FromRepository(ILogCollector logCollector, string repositoryPath, string packageName, DMAppVersion packageVersion)
            {
                return new AppPackageCreatorForAutomation(CICD.FileSystem.FileSystem.Instance, logCollector, repositoryPath, packageName, packageVersion, null);
            }

            /// <summary>
            /// Creates an <see cref="IAppPackageCreator"/> instance from the specified repository with the specified name and version.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="repositoryPath">The path of the repository that contains the Automation script solution.</param>
            /// <param name="packageName">The package name.</param>
            /// <param name="packageVersion">The package version.</param>
            /// <param name="scriptNames">The names of the scripts to include.</param>
            /// <returns>The <see cref="IAppPackageCreator"/> instance.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="logCollector"/>, <paramref name="repositoryPath"/>, <paramref name="packageName"/>, <paramref name="packageVersion"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="packageName"/>, <paramref name="packageVersion"/> is empty or whitespace.</exception>
            /// <exception cref="DirectoryNotFoundException">The directory specified in <paramref name="repositoryPath"/> does not exist.</exception>
            public static IAppPackageCreator FromRepository(ILogCollector logCollector, string repositoryPath, string packageName, DMAppVersion packageVersion, IEnumerable<string> scriptNames)
            {
                return new AppPackageCreatorForAutomation(CICD.FileSystem.FileSystem.Instance, logCollector, repositoryPath, packageName, packageVersion, scriptNames);
            }

            /// <summary>
            /// Creates an <see cref="IAppPackageCreator"/> instance from the specified input.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="workspace">The workspace.</param>
            /// <param name="jobBaseName">The job base name.</param>
            /// <param name="tag">The tag.</param>
            /// <param name="buildNumber">The build number.</param>
            /// <returns>The <see cref="IAppPackageCreator"/> instance.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="workspace"/> or <paramref name="jobBaseName"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="jobBaseName"/> is <see langword="null"/> is empty or whitespace.</exception>
            /// <exception cref="DirectoryNotFoundException">The directory specified in <paramref name="workspace"/> does not exist.</exception>
            public static IAppPackageCreator FromSkylinePipeline(ILogCollector logCollector, string workspace, string jobBaseName, string tag, int buildNumber)
            {
                string packageName = jobBaseName;

                bool isRelease = !String.IsNullOrWhiteSpace(tag) && !String.Equals(tag, "null", StringComparison.OrdinalIgnoreCase);
                DMAppVersion packageVersion;

                if (isRelease)
                {
                    packageVersion = DMAppVersion.FromProtocolVersion(tag);
                }
                else
                {
                    packageVersion = DMAppVersion.FromBuildNumber(buildNumber);
                }

                return new AppPackageCreatorForAutomation(CICD.FileSystem.FileSystem.Instance, logCollector, workspace, packageName, packageVersion, null);
            }
        }
    }
}