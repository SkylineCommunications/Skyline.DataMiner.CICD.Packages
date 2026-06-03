namespace Skyline.DataMiner.CICD.Packages.TestHelpers.Projects
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Build.Utilities.ProjectCreation;

    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Fluent builder for creating DataMiner Automation Script projects.
    /// </summary>
    public class AutomationScriptProjectBuilder
    {
        private readonly string testDirectory;
        private readonly string projectName;
        private readonly string targetFramework;
        private readonly string devAutomationVersion;
        private readonly Dictionary<string, string> properties = new();
        private readonly Dictionary<string, string> packageReferences = new();
        private readonly List<string> projectReferences = new();
        private readonly Dictionary<string, string> csharpFiles = new();
        private readonly Dictionary<string, byte[]> binaryFiles = new();
        private string? scriptXml;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationScriptProjectBuilder"/> class.
        /// </summary>
        /// <param name="testDirectory">The root test directory (from InitializeDirectoryForTest).</param>
        /// <param name="projectName">The name of the automation script project.</param>
        /// <param name="targetFramework">Target framework (default: net48).</param>
        /// <param name="devAutomationVersion">Skyline.DataMiner.Dev.Automation package version (default: 10.4.0.24).</param>
        public AutomationScriptProjectBuilder(
            string testDirectory,
            string projectName = "",
            string targetFramework = "net48",
            string devAutomationVersion = "10.4.0.24")
        {
            this.testDirectory = testDirectory;
            this.projectName = String.IsNullOrWhiteSpace(projectName) ? Guid.NewGuid().ToString() : projectName;
            this.targetFramework = targetFramework;
            this.devAutomationVersion = devAutomationVersion;

            scriptXml = GetDefaultScriptXml();
            csharpFiles[$"{projectName}.cs"] = GetDefaultCSharpContent();
        }

        /// <summary>
        /// Gets the resulting project creator after Build() is called.
        /// </summary>
        public ProjectCreator? ProjectCreator { get; private set; }

        /// <summary>
        /// Gets the full path to the .csproj file after Build() is called.
        /// </summary>
        public string FullPath => ProjectCreator?.FullPath ?? string.Empty;

        /// <summary>
        /// Gets the directory containing the .csproj after Build() is called.
        /// </summary>
        public string DirectoryPath => ProjectCreator?.Project.DirectoryPath ?? string.Empty;

        /// <summary>
        /// Gets the full path to the script XML file after Build() is called.
        /// </summary>
        public string ScriptXmlPath => FileSystem.Instance.Path.Combine(DirectoryPath, $"{projectName}.xml");

        /// <summary>
        /// Gets the project name.
        /// </summary>
        public string ProjectName => projectName;

        /// <summary>
        /// Sets a custom script XML content (overrides default).
        /// </summary>
        public AutomationScriptProjectBuilder WithScriptXml(string xmlContent)
        {
            scriptXml = xmlContent;
            return this;
        }

        /// <summary>
        /// Adds a C# file to the project.
        /// </summary>
        /// <param name="relativePath">Relative path within the project directory (e.g., "Script.cs" or "Subfolder/Class1.cs").</param>
        /// <param name="content">The C# source code.</param>
        public AutomationScriptProjectBuilder WithCSharpFile(string relativePath, string content)
        {
            csharpFiles[relativePath] = content;
            return this;
        }

        /// <summary>
        /// Adds a binary file (e.g. a .dll or other asset).
        /// </summary>
        public AutomationScriptProjectBuilder WithBinaryFile(string relativePath, byte[] content)
        {
            binaryFiles[relativePath] = content;
            return this;
        }

        /// <summary>
        /// Adds a NuGet package reference.
        /// </summary>
        public AutomationScriptProjectBuilder WithPackageReference(string packageName, string version)
        {
            packageReferences[packageName] = version;
            return this;
        }

        /// <summary>
        /// Adds a project reference.
        /// </summary>
        public AutomationScriptProjectBuilder WithProjectReference(string csprojPath)
        {
            projectReferences.Add(csprojPath);
            return this;
        }

        /// <summary>
        /// Sets a custom MSBuild property.
        /// </summary>
        public AutomationScriptProjectBuilder WithProperty(string name, string value)
        {
            properties[name] = value;
            return this;
        }

        /// <summary>
        /// Sets the DataMinerType property (default is "AutomationScript").
        /// </summary>
        public AutomationScriptProjectBuilder WithDataMinerType(string type)
        {
            properties["DataMinerType"] = type;
            return this;
        }

        /// <summary>
        /// Builds the project, writing all files to disk and saving the .csproj.
        /// </summary>
        /// <returns>This builder instance (for chaining or accessing FullPath/etc.).</returns>
        public AutomationScriptProjectBuilder Build()
        {
            string projectDir = FileSystem.Instance.Path.Combine(testDirectory, projectName);
            string csprojPath = FileSystem.Instance.Path.Combine(projectDir, $"{projectName}.csproj");
            
            // Create the .csproj via ProjectCreator
            var creator = ProjectCreator.Templates.SdkCsproj(
                csprojPath,
                sdk: "Skyline.DataMiner.Sdk",
                targetFramework: targetFramework);

            // DataMinerType property
            if (!properties.ContainsKey("DataMinerType"))
            {
                creator.Property("DataMinerType", "AutomationScript");
            }

            // Custom properties
            foreach (var kvp in properties)
            {
                creator.Property(kvp.Key, kvp.Value);
            }

            // Dev.Automation package reference
            creator.ItemPackageReference("Skyline.DataMiner.Dev.Automation", devAutomationVersion);

            // Additional package references
            foreach (var kvp in packageReferences)
            {
                creator.ItemPackageReference(kvp.Key, kvp.Value);
            }

            // Project references
            foreach (var projRef in projectReferences)
            {
                creator.ItemProjectReference(projRef);
            }

            creator.Save();

            // Write script XML
            string xmlContent = scriptXml ?? GetDefaultScriptXml();
            TestFixture.WriteFile(FileSystem.Instance.Path.Combine(projectDir, $"{projectName}.xml"), xmlContent);

            // Write C# files
            if (csharpFiles.Count == 0)
            {
                // Write default if none specified
                TestFixture.WriteFile(FileSystem.Instance.Path.Combine(projectDir, $"{projectName}.cs"), GetDefaultCSharpContent());
            }
            else
            {
                foreach (var kvp in csharpFiles)
                {
                    TestFixture.WriteFile(FileSystem.Instance.Path.Combine(projectDir, kvp.Key), kvp.Value);
                }
            }

            // Write binary files
            foreach (var kvp in binaryFiles)
            {
                TestFixture.WriteBinaryFile(FileSystem.Instance.Path.Combine(projectDir, kvp.Key), kvp.Value);
            }

            ProjectCreator = creator;
            return this;
        }

        private string GetDefaultScriptXml()
        {
            return $"""
                    <?xml version="1.0" encoding="utf-8" ?>
                    <DMSScript options="272" xmlns="http://www.skyline.be/automation">
                        <Name>{projectName}</Name>
                        <Description></Description>
                        <Type>Automation</Type>
                        <Author>MOD</Author>
                        <CheckSets>FALSE</CheckSets>
                        <Folder></Folder>
                        <Interactivity>Auto</Interactivity>

                        <Protocols>
                        </Protocols>

                        <Memory>
                        </Memory>

                        <Parameters>
                        </Parameters>

                        <Script>
                            <Exe id="1" type="csharp">
                                <Value><![CDATA[[Project:{projectName}]]]></Value>
                                <!--<Param type="debug">true</Param>-->
                                <Message></Message>
                            </Exe>
                        </Script>
                    </DMSScript>
                    """;
        }

        private string GetDefaultCSharpContent()
        {
            return """
                   namespace TestNamespace
                   {
                       public class TestClass
                       {
                           public void TestMethod()
                           {
                           }
                       }
                   }
                   """;
        }
    }
}
