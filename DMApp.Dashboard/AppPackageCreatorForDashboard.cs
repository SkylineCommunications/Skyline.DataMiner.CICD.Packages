namespace Skyline.DataMiner.CICD.DMApp.Dashboard
{
    using System;
    using System.Threading.Tasks;

    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    using static Skyline.AppInstaller.AppPackage;

    /// <summary>
    /// App package creator for Dashboards.
    /// </summary>
    public class AppPackageCreatorForDashboard : AppPackageCreator
    {
        private AppPackageCreatorForDashboard(IFileSystem fileSystem, ILogCollector logCollector, string workspace, string packageName, DMAppVersion packageVersion)
            : base(fileSystem, logCollector, workspace, packageName, packageVersion)
        {
        }

        /// <summary>
        /// Adds the relevant items to the specified package builder.
        /// </summary>
        /// <param name="appPackageBuilder">The package builder to which the items should be added to.</param>
        /// <returns>Task that adds the relevant items to the specified package builder.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="appPackageBuilder"/> is <see langword="null"/>.</exception>
        public override async Task AddItemsAsync(AppPackageBuilder appPackageBuilder)
        {
            if (appPackageBuilder == null) throw new ArgumentNullException(nameof(appPackageBuilder));

            var dashboardFilesPath = FileSystem.Directory.GetFiles(RepositoryPath, "*.json", System.IO.SearchOption.AllDirectories);

            foreach (var dashboardFilePath in dashboardFilesPath)
            {
                if (!dashboardFilePath.EndsWith(".json"))
                {
                    continue;
                }

                string relativePath = dashboardFilePath.Replace(RepositoryPath + "\\", "");
                string targetPath;
                if (relativePath.Contains("\\"))
                {
                    targetPath = FileSystem.Path.Combine(@"C:\Skyline DataMiner\Dashboards", FileSystem.Path.GetDirectoryName(relativePath));
                }
                else
                {
                    targetPath = @"C:\Skyline DataMiner\Dashboards";
                }

                LogCollector.ReportStatus("- dashboardFilePath: '" + dashboardFilePath + "' targetFilePath: '" + targetPath + "'");
                appPackageBuilder.WithDashboard(dashboardFilePath, targetPath);
            }
        }

        /// <summary>
        /// Factory class for dashboard app package creators.
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// Creates an <see cref="IAppPackageCreator"/> instance from the specified repository with the specified name and version.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="repositoryPath">The path of the repository that contains the dashboard.</param>
            /// <param name="packageName">The package name.</param>
            /// <param name="packageVersion">The package version.</param>
            /// <returns>The <see cref="IAppPackageCreator"/> instance.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="logCollector"/>, <paramref name="repositoryPath"/>, <paramref name="packageName"/>, <paramref name="packageVersion"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="packageName"/>, <paramref name="packageVersion"/> is empty or whitespace.</exception>
            /// <exception cref="DirectoryNotFoundException">The directory specified in <paramref name="repositoryPath"/> does not exist.</exception>
            public static IAppPackageCreator FromRepository(ILogCollector logCollector, string repositoryPath, string packageName, DMAppVersion packageVersion)
            {
                return new AppPackageCreatorForDashboard(CICD.FileSystem.FileSystem.Instance, logCollector, repositoryPath, packageName, packageVersion);
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
            /// <exception cref="ArgumentException"><paramref name="workspace"/> or <paramref name="jobBaseName"/> is <see langword="null"/> is empty or whitespace.</exception>
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

                return new AppPackageCreatorForDashboard(CICD.FileSystem.FileSystem.Instance, logCollector, workspace, packageName, packageVersion);
            }
        }
    }
}