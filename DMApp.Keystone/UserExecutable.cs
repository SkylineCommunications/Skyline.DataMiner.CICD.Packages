namespace Skyline.DataMiner.CICD.DMApp.Keystone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Provides functionality to wrap a user executable into a .NET tool package.
    /// </summary>
    public class UserExecutable : IUserExecutable
    {
        /// <summary>
        /// Wraps the specified directory or executable into a .NET tool.
        /// </summary>
        /// <param name="fs">The file system interface to interact with the file system.</param>
        /// <param name="outputDir">The directory where the packaged tool will be output.</param>
        /// <param name="dotnet">The interface to run dotnet commands.</param>
        /// <param name="pathToUserExecutableDir">The path to the directory containing the executable or the executable itself.</param>
        /// <param name="toolMetaData">Metadata for the tool being created.</param>
        /// <returns>The path to the created .nupkg file.</returns>
        /// <exception cref="InvalidOperationException">Thrown if multiple executables are found or if the path is not valid.</exception>
        public string WrapIntoDotnetTool(IFileSystem fs, string outputDir, IDotnet dotnet, string pathToUserExecutableDir, ToolMetaData toolMetaData)
        {
            string programPath = null;
            if (fs.Directory.IsDirectory(pathToUserExecutableDir))
            {
                var allExecutables = fs.Directory.EnumerateFiles(pathToUserExecutableDir, "*.exe");
                if (allExecutables.Count() > 1)
                {
                    throw new InvalidOperationException($"Multiple executables are not supported. Detected: {String.Join(";", allExecutables)}");
                }

                programPath = allExecutables.FirstOrDefault();
            }

            if (programPath == null)
            {
                throw new InvalidOperationException("Provided Path. Expected either a .nupkg file or a directory containing an executable '.exe'");
            }

            string programName = fs.Path.GetFileNameWithoutExtension(programPath);

            string assemblyFolder = fs.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string userProgramFolderName = "UserProgram";
            string shimmyName = programName + "Shimmy";



            fs.Directory.CopyRecursive(fs.Path.Combine(assemblyFolder, "ExeShimmy"), shimmyName);
            fs.Directory.CopyRecursive(pathToUserExecutableDir, $"{shimmyName}/{userProgramFolderName}");

            // Defaulting data:

            if (String.IsNullOrWhiteSpace(toolMetaData.ToolName)) toolMetaData.ToolName = $"Skyline.DataMiner.Keystone.{programName}";
            if (String.IsNullOrWhiteSpace(toolMetaData.ToolCommand))
            {
                string cleanedProgramName = Regex.Replace(programName, "[^a-zA-Z0-9 ]", "");
                string cleanedCommandName = Regex.Replace(cleanedProgramName, @"\s+", "-").ToLower();
                toolMetaData.ToolCommand = $"dataminer-keystone-{cleanedCommandName}";
            }
            if (String.IsNullOrWhiteSpace(toolMetaData.ToolVersion))
            {
                var exeVersion = fs.File.GetFileProductVersion(programPath);
                toolMetaData.ToolVersion = exeVersion;
            }

            if (String.IsNullOrWhiteSpace(toolMetaData.Company)) toolMetaData.Company = "Undefined";
            if (String.IsNullOrWhiteSpace(toolMetaData.Authors)) toolMetaData.Authors = "Undefined";

            try
            {
                Dictionary<string, string> placeholdersToReplace = new Dictionary<string, string>()
                {
                    {nameof(ToolMetaData.ToolName), toolMetaData.ToolName},
                    {nameof(ToolMetaData.ToolCommand), toolMetaData.ToolCommand},
                    {nameof(ToolMetaData.Authors), toolMetaData.Authors},
                    {nameof(ToolMetaData.Company), toolMetaData.Company},
                    {nameof(ToolMetaData.ToolVersion), toolMetaData.ToolVersion},
                    { "ProgramName",programName },
                    {"ProgramNameShimmy",userProgramFolderName }
                };

                foreach (var topLevelFile in fs.Directory.EnumerateFiles(shimmyName))
                {
                    var extension = fs.Path.GetExtension(topLevelFile);
                    if (extension != ".md" && extension != ".cs" && extension != ".txt" && extension != ".csproj") continue;

                    string fileContent = fs.File.ReadAllText(topLevelFile);
                    fileContent = ReplaceAllVariables(fileContent, placeholdersToReplace);
                    fs.File.WriteAllText(topLevelFile, fileContent);
                }

                string output, errors;
                dotnet.Run($"pack \"{shimmyName}/ExeShim.csproj\" --output {outputDir}", out output, out errors);

                Console.WriteLine(output);
                Console.WriteLine(errors);

                if (!String.IsNullOrWhiteSpace(errors) || (output != null && output.Contains("error")))
                {
                    throw new InvalidOperationException($"Failed to create dotnet tool with output: {output}");
                }
            }
            finally
            {
                fs.Directory.DeleteDirectory(shimmyName);
            }

            return fs.Path.Combine(outputDir, $"{toolMetaData.ToolName}.{toolMetaData.ToolVersion}.nupkg");
        }

        /// <summary>
        /// Replaces placeholders in a file content with the provided values.
        /// </summary>
        /// <param name="fileContent">The content of the file to replace placeholders in.</param>
        /// <param name="placeholders">A dictionary containing placeholders and their replacement values.</param>
        /// <returns>The modified file content.</returns>
        private static string ReplaceAllVariables(string fileContent, Dictionary<string, string> placeholders)
        {
            foreach (var placeholder in placeholders)
            {
                fileContent = fileContent.Replace($"${placeholder.Key}$", placeholder.Value);
            }

            return fileContent;
        }
    }
}