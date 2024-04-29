namespace Skyline.DataMiner.CICD.DMApp.Keystone
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    using static Skyline.AppInstaller.AppPackage;

    /// <summary>
    /// Represents a creator for application packages specifically for Keystone dashboards within the DataMiner System.
    /// </summary>
    public class AppPackageCreatorForKeystone : AppPackageCreator
    {
        private readonly ToolMetaData toolMetaData;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppPackageCreatorForKeystone"/> class.
        /// </summary>
        /// <param name="toolMetaData">Metadata associated with the tool.</param>
        /// <param name="fileSystem">File system interface to manage file operations.</param>
        /// <param name="logCollector">Log collector to capture logs during operations.</param>
        /// <param name="directoryPath">The directory path where packages are to be created.</param>
        /// <param name="packageName">The name of the package.</param>
        /// <param name="packageVersion">The version of the package.</param>
        public AppPackageCreatorForKeystone(ToolMetaData toolMetaData, IFileSystem fileSystem, ILogCollector logCollector, string directoryPath, string packageName, DMAppVersion packageVersion) : base(fileSystem, logCollector, directoryPath, packageName, packageVersion)
        {
            this.toolMetaData = toolMetaData;
        }

        /// <summary>
        /// Asynchronously adds items to the application package.
        /// </summary>
        /// <param name="appPackageBuilder">The application package builder to which the items are added.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task AddItemsAsync(AppPackageBuilder appPackageBuilder)
        {
            if (appPackageBuilder == null) throw new ArgumentNullException(nameof(appPackageBuilder));
            string pathToCreatedTool;

            if (RepositoryPath.EndsWith(".nupkg"))
            {
                // Already provided a nuget dotnet tool?
                pathToCreatedTool = RepositoryPath;
            }
            else
            {
                // Dotnet tools cannot directly run .exe files. They make their own .exe that runs the Main method from a .dll. So we make our "in-between" tool that then executes the user application.
                IUserExecutable userExecutable = new UserExecutable();

                static void OnOutput(object sender, DataReceivedEventArgs e)
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                    }
                }

                static void OnError(object sender, DataReceivedEventArgs e)
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        Console.Error.WriteLine(e.Data); // Write the error data to the console
                    }
                }

                var dotnet = DotnetFactory.Create(OnOutput, OnError);

                pathToCreatedTool = userExecutable.WrapIntoDotnetTool(FileSystem, dotnet, RepositoryPath, toolMetaData);
            }

            Console.WriteLine($"Creating dmapp from keystone file {pathToCreatedTool}");
            appPackageBuilder.WithKeystone(pathToCreatedTool);
        }

        /// <summary>
        /// Contains methods to facilitate the creation of <see cref="AppPackageCreatorForKeystone"/> instances.
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// Creates an instance of <see cref="AppPackageCreatorForKeystone"/> from a repository with specified parameters.
            /// </summary>
            /// <param name="metaData">Metadata associated with the tool.</param>
            /// <param name="filesystem">File system interface to manage file operations.</param>
            /// <param name="log">Log collector to capture logs during operations.</param>
            /// <param name="directoryPath">The directory path where packages are to be created.</param>
            /// <param name="packageName">The name of the package.</param>
            /// <param name="packageVersion">The version of the package.</param>
            /// <returns>Returns a new instance of <see cref="AppPackageCreatorForKeystone"/>.</returns>
            public static IAppPackageCreator FromRepository(ToolMetaData metaData, IFileSystem filesystem, ILogCollector log, string directoryPath, string packageName, DMAppVersion packageVersion)
            {
                return new AppPackageCreatorForKeystone(metaData, filesystem, log, directoryPath, packageName, packageVersion);
            }
        }
    }
}