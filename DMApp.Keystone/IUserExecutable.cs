namespace Skyline.DataMiner.CICD.DMApp.Keystone
{
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Defines a contract for classes that provide functionality to wrap user executables into .NET tool packages.
    /// </summary>
    public interface IUserExecutable
    {
        /// <summary>
        /// Wraps the specified directory or executable into a .NET tool.
        /// </summary>
        /// <param name="fs">The file system interface to interact with the file system.</param>
        /// <param name="dotnet">The interface to run dotnet commands.</param>
        /// <param name="pathToUserExecutableDir">The path to the directory containing the executable or the executable itself.</param>
        /// <param name="toolMetaData">Metadata for the tool being created.</param>
        /// <returns>The path to the created .nupkg file.</returns>
        /// <remarks>
        /// This method is responsible for creating a .NET tool package by encapsulating a user-provided executable or a directory containing an executable.
        /// It ensures the executable is correctly wrapped along with its metadata, facilitating its deployment and distribution.
        /// Implementors must handle scenarios where multiple executables are found or the provided path does not meet expectations.
        /// </remarks>
        string WrapIntoDotnetTool(IFileSystem fs, IDotnet dotnet, string pathToUserExecutableDir, ToolMetaData toolMetaData);
    }
}