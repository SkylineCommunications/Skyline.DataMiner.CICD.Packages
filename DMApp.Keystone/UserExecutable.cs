namespace Skyline.DataMiner.CICD.DMApp.Keystone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.FileSystem;

    public class UserExecutable : IUserExecutable
    {
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

            // Find program name
            string programName = fs.Path.GetFileNameWithoutExtension(programPath);
            string userProgramFolderName = "UserProgram";
            string shimmyName = programName + "Shimmy";

            // duplicate ExeShimmy to allow parallel runs
            fs.Directory.CopyRecursive("ExeShimmy", shimmyName);
            fs.Directory.CopyRecursive(pathToUserExecutableDir, $"{shimmyName}/{userProgramFolderName}");

            try
            {
                // Update the Shimmy placeholers
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

                // Run dotnet pack on that project      
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
