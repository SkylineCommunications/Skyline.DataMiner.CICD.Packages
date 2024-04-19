namespace Skyline.DataMiner.CICD.DMApp.Keystone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        ToolMetaData toolMetaData;

        public AppPackageCreatorForKeystone(ToolMetaData toolMetaData, IFileSystem fileSystem, ILogCollector logCollector, string directoryPath, string packageName, DMAppVersion packageVersion) : base(fileSystem, logCollector, directoryPath, packageName, packageVersion)
        {
            this.toolMetaData = toolMetaData;
        }

        public override async Task AddItemsAsync(AppPackageBuilder appPackageBuilder)
        {
            if (appPackageBuilder == null) throw new ArgumentNullException(nameof(appPackageBuilder));

            string pathToCreatedTool;
            string tempDir = null;
            try
            {
                if (RepositoryPath.EndsWith(".nupkg"))
                {
                    // Already provided a nuget dotnet tool?
                    pathToCreatedTool = RepositoryPath;
                }
                else
                {
                    // dotnet tools cannot directly run .exe files. They make their own .exe that runs the Main method from a .dll. So we make our "in-between" tool that then executes the user application.
                    tempDir = FileSystem.Directory.CreateTemporaryDirectory();
                    IUserExecutable userExecutable = new UserExecutable();
                    var dotnet = DotnetFactory.Create();
                    pathToCreatedTool = userExecutable.WrapIntoDotnetTool(FileSystem, tempDir, dotnet, RepositoryPath, toolMetaData);
                }

                // Update appPackageBuilder with the created .nupkg as a keystone.
                //appPackageBuilder.WithKeyStone(pathToCreatedTool);
            }
            finally
            {
                if (tempDir != null)
                {
                    FileSystem.Directory.DeleteDirectory(tempDir);
                }
            }
        }
    }
}
