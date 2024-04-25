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
    /// App package creator for Dashboards.
    /// </summary>
    public class AppPackageCreatorForKeystone : AppPackageCreator
    {
        private ToolMetaData toolMetaData;

        public AppPackageCreatorForKeystone(ToolMetaData toolMetaData, IFileSystem fileSystem, ILogCollector logCollector, string directoryPath, string packageName, DMAppVersion packageVersion) : base(fileSystem, logCollector, directoryPath, packageName, packageVersion)
        {
            this.toolMetaData = toolMetaData;
        }

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
                // dotnet tools cannot directly run .exe files. They make their own .exe that runs the Main method from a .dll. So we make our "in-between" tool that then executes the user application.
                IUserExecutable userExecutable = new UserExecutable();

                DataReceivedEventHandler onOutput = (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                    }
                };

                DataReceivedEventHandler onError = (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.Error.WriteLine(e.Data); // Write the error data to the console
                    }
                };

                var dotnet = DotnetFactory.Create(onOutput, onError);

                pathToCreatedTool = userExecutable.WrapIntoDotnetTool(FileSystem, dotnet, RepositoryPath, toolMetaData);
            }

            Console.WriteLine($"Creating dmapp from keystone file {pathToCreatedTool}");

            if (FileSystem.File.Exists(pathToCreatedTool))
            {
                Console.WriteLine($"File Exists: {pathToCreatedTool}");
            }
            else
            {
                Console.WriteLine($"File does not exist: {pathToCreatedTool}");
            }

            appPackageBuilder.WithKeystone(pathToCreatedTool);
        }

        public static class Factory
        {
            public static IAppPackageCreator FromRepository(ToolMetaData metaData, IFileSystem filesystem, ILogCollector log, string directoryPath, string packageName, DMAppVersion packageVersion)
            {
                return new AppPackageCreatorForKeystone(metaData, filesystem, log, directoryPath, packageName, packageVersion);
            }
        }
    }
}