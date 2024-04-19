namespace Skyline.DataMiner.CICD.DMApp.Keystone
{
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.FileSystem;

    public interface IUserExecutable
    {
        string WrapIntoDotnetTool(IFileSystem fs, string outputDir, IDotnet dotnet, string pathToUserExecutableDir, ToolMetaData toolMetaData);
    }
}