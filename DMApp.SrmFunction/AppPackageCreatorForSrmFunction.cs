namespace Skyline.DataMiner.CICD.DMApp.SrmFunction
{
    using System;
    using System.Threading.Tasks;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.DMApp.Automation;
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    /// <summary>
    /// Package creator for SRM function solutions.
    /// </summary>
    public class AppPackageCreatorForSrmFunction : AppPackageCreator
    {
        public AppPackageCreatorForSrmFunction(IFileSystem fileSystem, ILogCollector logCollector, string repositoryPath, string packageName, DMAppVersion packageVersion)
            : base(fileSystem, logCollector, repositoryPath, packageName, packageVersion)
        {
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
            var automationScriptPackageCreation = AppPackageCreatorForAutomation.Factory.FromRepository(this.LogCollector, this.RepositoryPath, this.PackageName, this.PackageVersion);
            await automationScriptPackageCreation.AddItemsAsync(appPackageBuilder);

            appPackageBuilder.WithFunction(RepositoryPath);
        }

        /// <summary>
        /// Factory class for SRM Function app package creators.
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// Creates an <see cref="IAppPackageCreator"/> instance from the specified repository with the specified name and version.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="repositoryPath">The path of the repository that contains the SRM function.</param>
            /// <param name="packageName">The package name.</param>
            /// <param name="packageVersion">The package version.</param>
            /// <returns>The <see cref="IAppPackageCreator"/> instance.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="logCollector"/>, <paramref name="repositoryPath"/>, <paramref name="packageName"/>, <paramref name="packageVersion"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="packageName"/>, <paramref name="packageVersion"/> is empty or whitespace.</exception>
            /// <exception cref="DirectoryNotFoundException">The directory specified in <paramref name="repositoryPath"/> does not exist.</exception>
            public static IAppPackageCreator FromRepository(ILogCollector logCollector, string repositoryPath, string packageName, DMAppVersion packageVersion)
            {
                return new AppPackageCreatorForSrmFunction(CICD.FileSystem.FileSystem.Instance, logCollector, repositoryPath, packageName, packageVersion);
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

                return new AppPackageCreatorForSrmFunction(CICD.FileSystem.FileSystem.Instance, logCollector, workspace, packageName, packageVersion);
            }
        }
    }
}