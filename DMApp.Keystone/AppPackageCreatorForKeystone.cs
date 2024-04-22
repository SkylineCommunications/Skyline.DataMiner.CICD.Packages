namespace Skyline.DataMiner.CICD.DMApp.Keystone
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
    public class AppPackageCreatorForKeystone : AppPackageCreator
    {
        private ToolMetaData toolMetaData;

        public AppPackageCreatorForKeystone(ToolMetaData toolMetaData, IFileSystem fileSystem, ILogCollector logCollector, string directoryPath, string packageName, DMAppVersion packageVersion) : base(fileSystem, logCollector, directoryPath, packageName, packageVersion)
        {
            this.toolMetaData = toolMetaData;

            if (String.IsNullOrWhiteSpace(toolMetaData.Company)) toolMetaData.Company = "Undefined";
            if (String.IsNullOrWhiteSpace(toolMetaData.Authors)) toolMetaData.Authors = "Undefined";
            if (String.IsNullOrWhiteSpace(toolMetaData.ToolCommand)) toolMetaData.ToolCommand = packageName;
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

                //TODO: Update appPackageBuilder with the created .nupkg as a keystone.
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

        public static class Factory
        {
            public static IAppPackageCreator FromRepository(ToolMetaData metaData, IFileSystem filesystem, ILogCollector log, string directoryPath, string packageName, DMAppVersion packageVersion)
            {
                return new AppPackageCreatorForKeystone(metaData, filesystem, log, directoryPath, packageName, packageVersion);
            }
        }
    }
}