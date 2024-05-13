namespace Skyline.DataMiner.CICD.DMApp.Keystone
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents metadata for a DataMiner Keystone executable command-line tool.
    /// This class provides properties to describe essential details about the tool such as its command, name, authors, and associated company.
    /// </summary>
    public class ToolMetaData
    {
        private string toolCommand;
        private string toolName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolMetaData"/> class with specified metadata.
        /// </summary>
        /// <param name="toolCommand">The command used to execute the tool.</param>
        /// <param name="toolName">The name of the tool.</param>
        /// <param name="toolVersion">The semantic version of the tool.</param>
        /// <param name="authors">The authors of the tool.</param>
        /// <param name="company">The company associated with the tool.</param>
        /// <param name="outputDirectory">The directory that will contain the resulting wrapped .nupkg tool.</param>
        public ToolMetaData(string toolCommand, string toolName, string toolVersion, string authors, string company, string outputDirectory)
        {
            ToolCommand = toolCommand;
            ToolName = toolName;
            ToolVersion = toolVersion;
            Authors = authors;
            Company = company;
            OutputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory), "Output path is a required argument.");
        }

        /// <summary>
        /// Gets or sets the authors of the Keystone tool.
        /// </summary>
        /// <value>
        /// A string listing the developers or authors responsible for the creation of the tool.
        /// </value>
        public string Authors { get; set; }

        /// <summary>
        /// Gets or sets the company associated with the Keystone tool.
        /// </summary>
        /// <value>
        /// The company or organization under which the tool is released or maintained.
        /// </value>
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the output path of the keystone tool. Specifically the location where a .nupkg will be added when wrapping an executable.
        /// </summary>
        /// <value>
        /// A string listing the directory path that will hold the dotnet tool.
        /// </value>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the command used to execute the Keystone tool. The input value is sanitized and formatted 
        /// to ensure it adheres to valid command-line syntax by replacing spaces with hyphens, converting to lowercase,
        /// and removing invalid characters.
        /// </summary>
        /// <value>
        /// The sanitized command string that is executed in the command-line to run the tool.
        /// </value>
        public string ToolCommand
        {
            get => toolCommand;
            set
            {
                if (value != null)
                {
                    toolCommand = Regex.Replace(value.Replace(" ", "-").ToLower(), "[^a-zA-Z0-9-]", "");
                }
                else
                {
                    toolCommand = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the Keystone tool. The input value is sanitized by removing any characters 
        /// not suitable for dotnet tool identifiers, preserving only alphanumeric characters and dots.
        /// </summary>
        /// <value>
        /// The sanitized name representing the tool, typically used for installing the package.
        /// </value>
        public string ToolName
        {
            get => toolName;
            set
            {
                if (value != null)
                {
                    toolName = Regex.Replace(value, "[^a-zA-Z0-9.]", "");
                }
                else
                {
                    toolName = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the version of the Keystone tool.
        /// </summary>
        /// <value>
        /// The version string that identifies the specific build or release of the tool.
        /// </value>
        public string ToolVersion { get; set; }
    }
}