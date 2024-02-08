namespace Skyline.DataMiner.CICD.DMApp.Visio
{
    using System;
    using System.Threading.Tasks;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    /// <summary>
    /// App package creator for Visio files other than protocol Visio files.
    /// </summary>
    public class AppPackageCreatorForVisio : AppPackageCreator
    {
        private AppPackageCreatorForVisio(IFileSystem fileSystem, ILogCollector logCollector, string repositoryPath, string packageName, DMAppVersion packageVersion)
            : base(fileSystem, logCollector, repositoryPath, packageName, packageVersion)
        {
        }

        /// <summary>
        /// Adds the relevant items to the specified package builder.
        /// </summary>
        /// <param name="appPackageBuilder">The package builder to which the items should be added to.</param>
        /// <returns>Task that adds the content of the <see cref="AppPackageCreator.RepositoryPath"/> directory to the package builder.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="appPackageBuilder"/> is <see langword="null"/>.</exception>
        public override async Task AddItemsAsync(AppPackage.AppPackageBuilder appPackageBuilder)
        {
            /* Files from the "ExtraFiles" folder are not needed.
             * Those mostly are .png files and so on to show to customers what the VISIO will look like
             * But they are not actually used by VISIO, everything the VISIO file needs is embedded in the .vsdx files
             */
            var visioFilesPath = FileSystem.Directory.GetFiles(RepositoryPath, "*.vsdx", System.IO.SearchOption.AllDirectories);

            foreach (var visioFilePath in visioFilesPath)
            {
                string visioVersion = PackageVersion.ToString();
                LogCollector.ReportStatus("- visioFilePath '" + visioFilePath + "' - visioVersion '" + visioVersion + "'");

                appPackageBuilder.WithVisioForViewOrElement(visioFilePath, visioVersion);
            }
        }

        /// <summary>
        /// Factory class for Visio app package creators.
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
                return new AppPackageCreatorForVisio(CICD.FileSystem.FileSystem.Instance, logCollector, repositoryPath, packageName, packageVersion);
            }

            /// <summary>
            /// Creates an <see cref="IAppPackageCreator"/> instance from the specified input.
            /// </summary>
            /// <param name="logCollector">The log collector.</param>
            /// <param name="workspace">The workspace.</param>
            /// <param name="packageName">The name of the package</param>
            /// <param name="tag">The tag.</param>
            /// <param name="buildNumber">The build number.</param>
            /// <returns>The <see cref="IAppPackageCreator"/> instance.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="workspace"/> or <paramref name="packageName"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException"><paramref name="workspace"/> or <paramref name="packageName"/> is <see langword="null"/> is empty or whitespace.</exception>
            public static IAppPackageCreator FromSkylinePipeline(ILogCollector logCollector, string workspace, string packageName, string tag, int buildNumber)
            {
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

                return new AppPackageCreatorForVisio(CICD.FileSystem.FileSystem.Instance, logCollector, workspace, packageName, packageVersion);
            }
        }
    }
}