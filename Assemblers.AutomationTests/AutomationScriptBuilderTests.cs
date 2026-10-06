namespace Assemblers.AutomationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using FluentAssertions;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Utilities.ProjectCreation;


    using Microsoft.Testing.Platform.Extensions.Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Org.XmlUnit.Builder;
    using Org.XmlUnit.Diff;

    using Skyline.DataMiner.CICD.Assemblers.Automation;
    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Packages.TestHelpers;
    using Skyline.DataMiner.CICD.Packages.TestHelpers.Projects;
    using Skyline.DataMiner.CICD.Parsers.Automation.Xml;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    [TestClass]
    public class AutomationScriptBuilderTests
    {
        [ClassInitialize]
        public static async Task BuildHarvestingFixtureOutputs(TestContext context)
        {
            string directory = Path.Combine(AppContext.BaseDirectory, "TestFiles", "ProjectReferenceHarvesting");
            foreach (string project in Directory.EnumerateFiles(directory, "*.csproj", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal))
            {
                await TestFixture.BuildProjectAsync(project).ConfigureAwait(false);
            }
        }

        [TestMethod]
        [DataRow("[Project:SVD-1_2]", "SVD-1_2")]
        [DataRow("[Project:TV2D-SRM-LSO.Satellite Downlink [DVB-S2.S2X]_63000]", "TV2D-SRM-LSO.Satellite Downlink [DVB-S2.S2X]_63000")]
        [DataRow("", null)]
        [DataRow(null, null)]
        public void SLDisCompiler_AutomationScriptBuilder_TryFindProjectPlaceholder(string text, string expectedOutput)
        {
            // Act
            AutomationScriptBuilder.TryFindProjectPlaceholder(text, out string result, out _);

            // Assert
            Assert.AreEqual(expectedOutput, result);
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_BasicAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[using System;]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", new Project("Script_1", new[]{ new ProjectFile("Script.cs", "using System;") }) },
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_BasicNoCDataAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value>[Project:Script_1]</Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[using System;]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", new Project("Script_1", new[]{ new ProjectFile("Script.cs", "using System;") }) },
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_IgnoreTestingFilesAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value>[Project:Script_1]</Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[using System;]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", new Project("Script_1", new[]{ new ProjectFile("Script.cs", "using System;"), new ProjectFile("TestPackageContent\\TestHarvesting\\test.cs", "using System.Linq;") }) },
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_MultipleScriptsAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
        <Exe id=""2"" type=""csharp"">
            <Value><![CDATA[[Project:Script_2]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[using System;]]></Value>
        </Exe>
        <Exe id=""2"" type=""csharp"">
            <Value><![CDATA[using System.Xml;]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", new Project("Script_1", new[]{ new ProjectFile("Script.cs", "using System;") }) },
                { "Script_2", new Project("Script_2", new[]{ new ProjectFile("Script.cs", "using System.Xml;") }) },
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_MultipleFilesAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value>
				<![CDATA[using System;
//---------------------------------
// Script.cs
//---------------------------------

//---------------------------------
// Class1.cs
//---------------------------------
class Class1 {}]]>
			</Value>
        </Exe>
	</Script>
</DMSScript>";

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", new Project("Script_1", new[]{ new ProjectFile("Script.cs", "using System;"), new ProjectFile("Class1.cs", "using System; class Class1 {}") }) },
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public void SLDisCompiler_AutomationScriptBuilder_MissingProject()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            var projects = new Dictionary<string, Project>();

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            var exception = Assert.Throws<AggregateException>(() => builder.BuildAsync().Result);

            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOfType(exception.InnerException, typeof(AssemblerException));
            Assert.AreEqual("Project with name 'Script_1' could not be found!", exception.InnerException.Message);
        }

        [TestMethod]
        public void SLDisCompiler_AutomationScriptBuilder_MissingFiles()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", new Project("Script_1", Array.Empty<ProjectFile>()) },
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            var exception = Assert.Throws<AggregateException>(() => builder.BuildAsync().Result);

            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOfType(exception.InnerException, typeof(AssemblerException));
            Assert.AreEqual("No code files found in project 'Script_1'", exception.InnerException.Message);
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_DllImportsAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[using System;]]></Value>
            <Param type=""ref"">System.Data.dll</Param>
        </Exe>
	</Script>
</DMSScript>";

            var projectFiles = new[] { new ProjectFile("Script.cs", "using System;") };
            var references = new[] { new Reference("System.Data.dll") };
            var project1 = new Project("Script_1", projectFiles: projectFiles, references: references);

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", project1},
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_DllImportsWithFullPathAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[using System;]]></Value>
            <Param type=""ref"">C:\Skyline DataMiner\Files\System.Data.dll</Param>
        </Exe>
	</Script>
</DMSScript>";

            var projectFiles = new[] { new ProjectFile("Script.cs", "using System;") };
            var references = new[] { new Reference(@"C:\Skyline DataMiner\Files\System.Data.dll") };
            var project1 = new Project("Script_1", projectFiles: projectFiles, references: references);

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", project1},
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_ClassLibraryAsync()
        {
            string original = @"<DMSScript>
	<Script>
	    <Exe id=""63000"" type=""csharp"">
            <Value><![CDATA[[Project:Script_63000]]]></Value>
            <Param type=""preCompile"">true</param>
            <Param type=""libraryName"">DIS Class Library</param>
        </Exe>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""63000"" type=""csharp"">
			<Value>
				<![CDATA[namespace Skyline.DataMiner.Library { }]]>
			</Value>
			<Param type=""preCompile"">true</Param>
			<Param type=""libraryName"">DIS Class Library</Param>
		</Exe>
		<Exe id=""1"" type=""csharp"">
			<Value>
				<![CDATA[using System;]]>
			</Value>
			<Param type=""ref"">System.Data.dll</Param>
			<Param type=""scriptRef"">[AutomationScriptName]:DIS Class Library</Param>
		</Exe>
	</Script>
</DMSScript>";

            var projectFiles_project1 = new[] { new ProjectFile("Script.cs", "using System;") };
            var projectFiles_project63000 = new[] { new ProjectFile("Script.cs", "namespace Skyline.DataMiner.Library { }") };
            var references_project1 = new[] { new Reference("System.Data.dll") };
            var projectReferences_project1 = new[] { new ProjectReference("AutomationScript_ClassLibrary") };
            var project1 = new Project("Script_1", projectFiles: projectFiles_project1, references: references_project1, projectReferences: projectReferences_project1);
            var project63000 = new Project("Script_63000", projectFiles: projectFiles_project63000);

            var projects = new Dictionary<string, Project>()
            {
                { "Script_63000", project63000},
                { "Script_1", project1},
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_ScriptRefAsync()
        {
            string original = @"<DMSScript>
	<Script>
	    <Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
            <Param type=""preCompile"">true</param>
            <Param type=""libraryName"">Script1</param>
        </Exe>
		<Exe id=""2"" type=""csharp"">
            <Value><![CDATA[[Project:Script_2]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
			<Value>
				<![CDATA[using System;]]>
			</Value>
			<Param type=""preCompile"">true</Param>
			<Param type=""libraryName"">Script1</Param>
		</Exe>
		<Exe id=""2"" type=""csharp"">
			<Value>
				<![CDATA[using System.Xml;]]>
			</Value>
			<Param type=""scriptRef"">[AutomationScriptName]:Script1</Param>
		</Exe>
	</Script>
</DMSScript>";

            var projectFiles_project1 = new[] { new ProjectFile("Script.cs", "using System;") };
            var projectFiles_project2 = new[] { new ProjectFile("Script.cs", "using System.Xml;") };
            var projectReferences_project2 = new[] { new ProjectReference("Script_1") };
            var project1 = new Project("Script_1", projectFiles: projectFiles_project1);
            var project2 = new Project("Script_2", projectFiles: projectFiles_project2, projectReferences: projectReferences_project2);

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", project1},
                { "Script_2", project2},
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_RefAsync_OtherFiles()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
			<Value>
				<![CDATA[using System;]]>
			</Value>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\newtonsoft.json\13.0.2\lib\net45\Newtonsoft.Json.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.core.dataminersystem.common\1.0.0.1\lib\net462\Skyline.DataMiner.Core.DataMinerSystem.Common.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.core.dataminersystem.automation\1.0.0.1\lib\net462\Skyline.DataMiner.Core.DataMinerSystem.Automation.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\Files\SLManagedScripting.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\Files\SLMediationSnippets.dll</Param>
        </Exe>
	</Script>
</DMSScript>";

            var projectFiles = new[] { new ProjectFile("Script.cs", "using System;") };
            var packageReferences = new[]
            {
                new PackageReference("Skyline.DataMiner.Core.DataMinerSystem.Automation", "1.0.0.1"),
                new PackageReference("Skyline.DataMiner.Dev.Automation", "10.3.5"),
                new PackageReference("Skyline.DataMiner.Files.SLManagedScripting", "10.3.5"),
                new PackageReference("Skyline.DataMiner.Files.SLManagedAutomation", "10.3.5"),
                new PackageReference("Skyline.DataMiner.Files.SLMediationSnippets", "10.3.5")
            };
            var project1 = new Project("Script_1", tfm: ".NETFramework,Version=v4.6.2", projectFiles: projectFiles, packageReferences: packageReferences);

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", project1},
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_RefAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
			<Value>
				<![CDATA[using System;]]>
			</Value>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\Newtonsoft.Json.dll</Param>
        </Exe>
	</Script>
</DMSScript>";

            var projectFiles = new[] { new ProjectFile("Script.cs", "using System;") };
            var references = new[] { new Reference("Newtonsoft.Json.dll") };
            var project1 = new Project("Script_1", projectFiles: projectFiles, references: references);

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", project1},
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_Ref_NoDuplicateAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
			<Param type=""ref"">Newtonsoft.Json.dll</Param>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
			<Value>
				<![CDATA[using System;]]>
			</Value>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\Newtonsoft.Json.dll</Param>
        </Exe>
	</Script>
</DMSScript>";

            var projectFiles = new[] { new ProjectFile("Script.cs", "using System;") };
            var references = new[] { new Reference("Newtonsoft.Json.dll") };
            var project1 = new Project("Script_1", projectFiles: projectFiles, references: references);

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", project1},
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_Ref_RemoveDuplicateAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
			<Param type=""ref"">Newtonsoft.Json.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\Newtonsoft.Json.dll</Param>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
			<Value>
				<![CDATA[using System;]]>
			</Value>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\Newtonsoft.Json.dll</Param>
        </Exe>
	</Script>
</DMSScript>";

            var projectFiles = new[] { new ProjectFile("Script.cs", "using System;") };
            var references = new[] { new Reference("Newtonsoft.Json.dll") };
            var project1 = new Project("Script_1", projectFiles: projectFiles, references: references);

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", project1},
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_Ref_RemoveOtherAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
			<Param type=""ref"">DllToRemove.dll</Param>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
			<Value>
				<![CDATA[using System;]]>
			</Value>
        </Exe>
	</Script>
</DMSScript>";

            var project1 = new Project("Script_1", new[] { new ProjectFile("Script.cs", "using System;") });

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", project1},
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_NotCSharpAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""report"">
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""report"">
        </Exe>
	</Script>
</DMSScript>";

            var projects = new Dictionary<string, Project>();

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_SpecialCharactersAsync()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[using Characterø;]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", new Project("Script_1", new[]{ new ProjectFile("Script.cs", "using Characterø;") }) },
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task SLDisCompiler_AutomationScriptBuilder_Yle_Library()
        {
            string original = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
	</Script>
</DMSScript>";

            string expected = @"<DMSScript>
	<Script>
		<Exe id=""1"" type=""csharp"">
			<Value>
				<![CDATA[using System;]]>
			</Value>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\newtonsoft.json\13.0.3\lib\net45\Newtonsoft.Json.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\sharpziplib\1.0.0\lib\net45\ICSharpCode.SharpZipLib.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\npoi\2.4.1\lib\net45\NPOI.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\npoi\2.4.1\lib\net45\NPOI.OOXML.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\npoi\2.4.1\lib\net45\NPOI.OpenXml4Net.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\npoi\2.4.1\lib\net45\NPOI.OpenXmlFormats.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.core.dataminersystem.common\1.1.0.5\lib\net462\Skyline.DataMiner.Core.DataMinerSystem.Common.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.core.interappcalls.common\1.0.0.2\lib\net462\Skyline.DataMiner.Core.InterAppCalls.Common.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.connectorapi.evs.ipd-via\1.0.0.4-test1\lib\net472\Skyline.DataMiner.ConnectorAPI.EVS.IPD-VIA.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.connectorapi.yle.ordermanager\1.0.0.2-test1\lib\net472\Skyline.DataMiner.ConnectorAPI.YLE.OrderManager.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.utils.interactiveautomationscripttoolkit\6.1.0\lib\net462\Skyline.DataMiner.Utils.InteractiveAutomationScriptToolkit.dll</Param>
			<Param type=""ref"">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.utils.yle.integrations\1.0.1.6-test1\lib\net472\Skyline.DataMiner.Utils.YLE.Integrations.dll</Param>
        </Exe>
	</Script>
</DMSScript>";

            var projectFiles = new[] { new ProjectFile("Script.cs", "using System;") };
            var packageReferences = new[]
            {
                new PackageReference("Skyline.DataMiner.Dev.Automation", "10.3.5"),
                new PackageReference("Newtonsoft.Json", "13.0.3"),
                new PackageReference("NPOI", "2.4.1"),
                new PackageReference("Skyline.DataMiner.ConnectorAPI.EVS.IPD-VIA", "1.0.0.4-Test1"),
                new PackageReference("Skyline.DataMiner.ConnectorAPI.YLE.OrderManager", "1.0.0.2-Test1"),
                new PackageReference("Skyline.DataMiner.Utils.InteractiveAutomationScriptToolkit", "6.1.0"),
                new PackageReference("Skyline.DataMiner.Utils.YLE.Integrations", "1.0.1.6-Test1"),
            };
            var project1 = new Project("Script_1", tfm: ".NETFramework,Version=v4.7.2", projectFiles: projectFiles, packageReferences: packageReferences);

            var projects = new Dictionary<string, Project>()
            {
                { "Script_1", project1},
            };

            Script script = new Script(XmlDocument.Parse(original));
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task AutomationScriptBuilder_SolutionLibraries_SingleStandalone()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            var projectBuilder = new AutomationScriptProjectBuilder(testDirectory, devAutomationVersion: "10.3.0.25")
                                    .WithPackageReference("Skyline.DataMiner.Dev.Utils.DummySolutionLib", "1.0.1")
                                    .Build();

            var projects = new Dictionary<string, Project>
            {
                { projectBuilder.ProjectName, Project.Load(projectBuilder.FullPath) },
            };

            Script script = Script.Load(projectBuilder.ScriptXmlPath);
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            // Act
            var result = await builder.BuildAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Document.Should()
                  .ContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\SolutionLibraries\\DummySolutionLib\\Skyline.DataMiner.Dev.Utils.DummySolutionLib.dll</Param>");

            // Only needs to be referenced, shouldn't be part of the script itself
            result.Assemblies.Should().BeEmpty();
            result.DllAssemblies.Should().BeEmpty();
        }

        [TestMethod]
        public async Task AutomationScriptBuilder_SolutionLibraries_StandAlone_DependingOnAnother()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            var projectBuilder = new AutomationScriptProjectBuilder(testDirectory, devAutomationVersion: "10.3.0.25")
                                    .WithPackageReference("Skyline.DataMiner.Dev.Utils.DummySolutionLib.Automation", "1.0.1")
                                    .Build();

            var projects = new Dictionary<string, Project>
            {
                { projectBuilder.ProjectName, Project.Load(projectBuilder.FullPath) },
            };

            Script script = Script.Load(projectBuilder.ScriptXmlPath);
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            // Act
            var result = await builder.BuildAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Document.Should()
                  .ContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\SolutionLibraries\\DummySolutionLib\\Skyline.DataMiner.Dev.Utils.DummySolutionLib.dll</Param>");
            result.Document.Should()
                  .ContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\SolutionLibraries\\DummySolutionLib.Automation\\Skyline.DataMiner.Dev.Utils.DummySolutionLib.Automation.dll</Param>");

            // Only needs to be referenced, shouldn't be part of the script itself
            result.Assemblies.Should().BeEmpty();
            result.DllAssemblies.Should().BeEmpty();
        }

        [TestMethod]
        public async Task AutomationScriptBuilder_SolutionLibraries_WithDependencies_DependingOnAnother()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            var projectBuilder = new AutomationScriptProjectBuilder(testDirectory, devAutomationVersion: "10.3.0.25")
                                    .WithPackageReference("Skyline.DataMiner.Dev.Utils.DummySolutionLib.Deps.Protocol", "1.0.1")
                                    .Build();

            var projects = new Dictionary<string, Project>
            {
                { projectBuilder.ProjectName, Project.Load(projectBuilder.FullPath) },
            };

            Script script = Script.Load(projectBuilder.ScriptXmlPath);
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            // Act
            var result = await builder.BuildAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Document.Should()
                  .ContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\SolutionLibraries\\DummySolutionLib.Deps\\Skyline.DataMiner.Dev.Utils.DummySolutionLib.Deps.dll</Param>");
            result.Document.Should()
                  .ContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\SolutionLibraries\\DummySolutionLib.Deps.Protocol\\Skyline.DataMiner.Dev.Utils.DummySolutionLib.Deps.Protocol.dll</Param>");

            // Only needs to be referenced, shouldn't be part of the script itself
            result.Assemblies.Should().HaveCount(1);
            result.Assemblies.First().DllImport.Should().Be("newtonsoft.json\\13.0.2\\lib\\net45\\Newtonsoft.Json.dll");
            result.DllAssemblies.Should().BeEmpty();
        }

        [TestMethod]
        public async Task AutomationScriptBuilder_SolutionLibraries_WithDependencies_CustomDependency()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            var projectBuilder = new AutomationScriptProjectBuilder(testDirectory, devAutomationVersion: "10.3.0.25")
                                    .WithPackageReference("Newtonsoft.Json", "13.0.4")
                                    .WithPackageReference("Skyline.DataMiner.Dev.Utils.DummySolutionLib.Deps", "1.0.1")
                                    .Build();

            var projects = new Dictionary<string, Project>
            {
                { projectBuilder.ProjectName, Project.Load(projectBuilder.FullPath) },
            };

            Script script = Script.Load(projectBuilder.ScriptXmlPath);
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            // Act
            var result = await builder.BuildAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Document.Should()
                  .ContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\newtonsoft.json\\13.0.4\\lib\\net45\\Newtonsoft.Json.dll</Param>");
            result.Document.Should()
                  .ContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\SolutionLibraries\\DummySolutionLib.Deps\\Skyline.DataMiner.Dev.Utils.DummySolutionLib.Deps.dll</Param>");

            // Only needs to be referenced, shouldn't be part of the script itself
            result.Assemblies.Should().HaveCount(2);
            result.Assemblies.Should().Contain(a => a.DllImport == "newtonsoft.json\\13.0.4\\lib\\net45\\Newtonsoft.Json.dll");
            result.Assemblies.Should().Contain(a => a.DllImport == "newtonsoft.json\\13.0.2\\lib\\net45\\Newtonsoft.Json.dll");
            result.DllAssemblies.Should().BeEmpty();
        }

        [TestMethod]
        public async Task AutomationScriptBuilder_DataMinerSolutionId_Script1ShouldHaveSameNuGetVersion()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            var script1Builder = new AutomationScriptProjectBuilder(testDirectory)
                                    .WithPackageReference("Newtonsoft.Json", "13.0.3")
                                    .Build();

            var script2Builder = new AutomationScriptProjectBuilder(testDirectory)
                                    .WithPackageReference("Newtonsoft.Json", "13.0.4")
                                    .Build();

            var projectScript1 = Project.Load(script1Builder.FullPath);
            var scriptProjects = new Dictionary<string, Project>
            {
                // Will always be one
                [projectScript1.ProjectName] = projectScript1,
            };

            var solutionProjects = new List<Project>
            {
                projectScript1,
                Project.Load(script2Builder.FullPath),
            };

            Script script1 = Script.Load(script1Builder.ScriptXmlPath);

            var allScripts = new List<Script>
            {
                script1,
                Script.Load(script2Builder.ScriptXmlPath)
            };

            const string dataMinerSolutionId = "RANDOM_DATAMINER_SOLUTION_ID";

            // Act
            AutomationScriptBuilder builder = new AutomationScriptBuilder(dataMinerSolutionId, script1, scriptProjects, solutionProjects, allScripts, directoryForNuGetConfig: null);
            var result = await builder.BuildAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            // Assert SolutionId
            result.Document.Should().ContainEquivalentOf($"<SolutionId>{dataMinerSolutionId}</SolutionId>");

            // Assert the correct reference is used (highest one is in script 2, so that one should be used)
            result.Document.Should()
                  .ContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\newtonsoft.json\\13.0.4\\lib\\net45\\Newtonsoft.Json.dll</Param>");

            result.Document.Should()
                  .NotContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\newtonsoft.json\\13.0.3\\lib\\net45\\Newtonsoft.Json.dll</Param>");

            result.Assemblies.Should().HaveCount(1);
            result.Assemblies.Should().Contain(a => a.DllImport == @"newtonsoft.json\13.0.4\lib\net45\Newtonsoft.Json.dll");

            result.DllAssemblies.Should().BeEmpty();
        }

        [TestMethod]
        public async Task AutomationScriptBuilder_NoDataMinerSolutionId_Script1ShouldHaveDifferentNuGetVersion()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            var script1Builder = new AutomationScriptProjectBuilder(testDirectory)
                                    .WithPackageReference("Newtonsoft.Json", "13.0.3")
                                    .Build();

            var script2Builder = new AutomationScriptProjectBuilder(testDirectory)
                                    .WithPackageReference("Newtonsoft.Json", "13.0.4")
                                    .Build();

            var projectScript1 = Project.Load(script1Builder.FullPath);
            var scriptProjects = new Dictionary<string, Project>
            {
                // Will always be one
                [projectScript1.ProjectName] = projectScript1,
            };

            Script script1 = Script.Load(script1Builder.ScriptXmlPath);

            var allScripts = new List<Script>
            {
                script1,
                Script.Load(script2Builder.ScriptXmlPath)
            };

            // Act
            AutomationScriptBuilder builder = new AutomationScriptBuilder(script1, scriptProjects, allScripts, directoryForNuGetConfig: null);
            var result = await builder.BuildAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            // Assert SolutionId
            result.Document.Should().NotContainEquivalentOf("<SolutionId>");

            // Assert the correct reference is used (highest one is in script 2, but no solution id, so shouldn't change)
            result.Document.Should()
                  .ContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\newtonsoft.json\\13.0.3\\lib\\net45\\Newtonsoft.Json.dll</Param>");

            result.Document.Should()
                  .NotContainEquivalentOf(
                      "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\newtonsoft.json\\13.0.4\\lib\\net45\\Newtonsoft.Json.dll</Param>");

            result.Assemblies.Should().HaveCount(1);
            result.Assemblies.Should().Contain(a => a.DllImport == @"newtonsoft.json\13.0.3\lib\net45\Newtonsoft.Json.dll");

            result.DllAssemblies.Should().BeEmpty();
        }

        [TestMethod]
        public async Task AutomationScriptBuilder_DataMinerSolutionId_AllScriptsShouldHaveSameNuGetVersion()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            var script1Builder = new AutomationScriptProjectBuilder(testDirectory)
                                 .WithPackageReference("Newtonsoft.Json", "13.0.3")
                                 .Build();

            var script2Builder = new AutomationScriptProjectBuilder(testDirectory)
                                 .WithPackageReference("Newtonsoft.Json", "13.0.4")
                                 .Build();

            var script3Builder = new AutomationScriptProjectBuilder(testDirectory)
                                 .WithPackageReference("Newtonsoft.Json", "13.0.2")
                                 .Build();

            List<(AutomationScriptProjectBuilder mainScript, List<AutomationScriptProjectBuilder> otherSolutionScripts)> builders =
            [
                (script1Builder, [script2Builder, script3Builder]),
                (script2Builder, [script1Builder, script3Builder]),
                (script3Builder, [script1Builder, script2Builder])
            ];

            foreach ((AutomationScriptProjectBuilder mainScript, List<AutomationScriptProjectBuilder> otherSolutionScripts) in builders)
            {
                var mainScriptProject = Project.Load(mainScript.FullPath);
                var scriptProjects = new Dictionary<string, Project>
                {
                    // Will always be one
                    [mainScriptProject.ProjectName] = mainScriptProject,
                };

                var solutionProjects = new List<Project>
                {
                    mainScriptProject,
                };

                Script script = Script.Load(mainScript.ScriptXmlPath);

                var allScripts = new List<Script>
                {
                    script,
                };

                foreach (AutomationScriptProjectBuilder otherSolutionScript in otherSolutionScripts)
                {
                    allScripts.Add(Script.Load(otherSolutionScript.ScriptXmlPath));
                    solutionProjects.Add(Project.Load(otherSolutionScript.FullPath));
                }

                const string dataMinerSolutionId = "RANDOM_DATAMINER_SOLUTION_ID";

                // Act
                AutomationScriptBuilder builder = new AutomationScriptBuilder(dataMinerSolutionId, script, scriptProjects, solutionProjects, allScripts, directoryForNuGetConfig: null);
                var result = await builder.BuildAsync().ConfigureAwait(false);

                // Assert
                result.Should().NotBeNull();

                // Assert SolutionId
                result.Document.Should().ContainEquivalentOf($"<SolutionId>{dataMinerSolutionId}</SolutionId>");

                // Assert the correct reference is used
                result.Document.Should()
                      .ContainEquivalentOf(
                          "<Param type=\"ref\">C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\newtonsoft.json\\13.0.4\\lib\\net45\\Newtonsoft.Json.dll</Param>");

                result.Assemblies.Should().HaveCount(1);
                result.Assemblies.Should().Contain(a => a.DllImport == @"newtonsoft.json\13.0.4\lib\net45\Newtonsoft.Json.dll");

                result.DllAssemblies.Should().BeEmpty();
            }

        }


        [TestMethod]
        public async Task AutomationScriptBuilder_ProjectReferenceHarvesting_CSharpLibraryProject_AddsDllImportAsync()
        {

            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            string testFilesDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "TestLibrary");

            string libraryProjectPath = Path.Combine(
                testFilesDirectory,
                "TestLibrary.csproj");

            string assemblyPath = Path.Combine(
                testFilesDirectory,
                "bin",
                "Debug",
                "netstandard2.0",
                "TestLibrary.dll");

            File.Exists(libraryProjectPath).Should().BeTrue(
                $"the test library project should exist at {libraryProjectPath}");

            File.Exists(assemblyPath).Should().BeTrue(
                $"the test library assembly should exist at {assemblyPath}");

            var assemblyVersion =
                System.Reflection.AssemblyName
                    .GetAssemblyName(assemblyPath)
                    .Version;



            var evaluatedLibrary =
                MSBuildHelpers.EvaluateReferenceProject(
                    libraryProjectPath,
                    "netstandard2.0");

            evaluatedLibrary.Should().NotBeNull();
            evaluatedLibrary.OutputType.Should().Be("Library");
            evaluatedLibrary.TargetFramework.Should().Be("netstandard2.0");
            evaluatedLibrary.PackageId.Should().Be("Test.Library");

            evaluatedLibrary.IsDataMinerProject.Should().BeFalse();
            evaluatedLibrary.ShouldHarvestAssembly().Should().BeTrue();

            evaluatedLibrary.TargetPath.Should().NotBeNullOrWhiteSpace();

            File.Exists(evaluatedLibrary.TargetPath)
                .Should()
                .BeTrue(
                    $"the referenced project's DLL should already exist at {evaluatedLibrary.TargetPath}");

            var projectFiles = new[]
        {
        new ProjectFile(
            "Script.cs",
            "using System;"),
    };


            var projectReferences = new[]
            {
        new ProjectReference(
            "TestLibrary",
            libraryProjectPath,
            libraryProjectPath),
    };

            var scriptProject = new Project(
                "Script_1",
                path: Path.Combine(testDirectory, "Script_1.csproj"),
                tfm: "net48",
                projectFiles: projectFiles,
                projectReferences: projectReferences);

            var projects = new Dictionary<string, Project>
            {
                ["Script_1"] = scriptProject,
            };

            string original = @"
<DMSScript>
    <Script>
        <Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
    </Script>
</DMSScript>";

            Script script =
                new Script(XmlDocument.Parse(original));

            AutomationScriptBuilder builder =
                new AutomationScriptBuilder(
                    script,
                    projects,
                    new List<Script> { script },
                    directoryForNuGetConfig: null);

            // Act
            var result =
                await builder.BuildAsync()
                    .ConfigureAwait(false);




            // Assert
            result.Should().NotBeNull();

            result.Assemblies.Should().Contain(
                a =>
                    a.DllImport.Equals(
                        "test.library/7.8.9.0/lib/netstandard2.0/TestLibrary.dll",
                        StringComparison.OrdinalIgnoreCase));

            result.Document.Should().Contain(
                @"C:\Skyline DataMiner\ProtocolScripts\DllImport\test.library\7.8.9.0\lib\netstandard2.0\TestLibrary.dll");

            result.Document.Should().NotContain("scriptRef");
            evaluatedLibrary.PackageVersion.Should().Be("2.3.4");
            evaluatedLibrary.AssemblyVersion.Should().Be("7.8.9.0");
        }
        [TestMethod]
        public void EvaluateReferenceProject_MultiTargetProject_NetStandard_SelectsNetStandardOutput()
        {
            string testFilesDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "MultiTargetLibrary");

            string projectPath = Path.Combine(
                testFilesDirectory,
                "MultiTargetLibrary.csproj");

            var evaluatedProject =
                MSBuildHelpers.EvaluateReferenceProject(
                    projectPath,
                    "netstandard2.0");

            evaluatedProject.Should().NotBeNull();
            evaluatedProject.TargetFramework.Should().Be("netstandard2.0");
            evaluatedProject.TargetPath.Should().NotBeNullOrWhiteSpace();

            evaluatedProject.TargetPath.Should().Contain(
                Path.Combine("bin", "Debug", "netstandard2.0"));

            File.Exists(evaluatedProject.TargetPath)
                .Should()
                .BeTrue();
        }
        [TestMethod]
        public void EvaluateReferenceProject_MultiTargetProject_Net48_SelectsNet48Output()
        {
            string testFilesDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "MultiTargetLibrary");

            string projectPath = Path.Combine(
                testFilesDirectory,
                "MultiTargetLibrary.csproj");

            var evaluatedProject =
                MSBuildHelpers.EvaluateReferenceProject(
                    projectPath,
                    "net48");

            evaluatedProject.Should().NotBeNull();
            evaluatedProject.TargetFramework.Should().Be("net48");
            evaluatedProject.TargetPath.Should().NotBeNullOrWhiteSpace();

            evaluatedProject.TargetPath.Should().Contain(
                Path.Combine("bin", "Debug", "net48"));

            File.Exists(evaluatedProject.TargetPath)
                .Should()
                .BeTrue();
        }
        [TestMethod]
        public async Task AutomationScriptBuilder_ProjectReferenceHarvesting_MultiTargetLibrary_Net48_SelectsNet48OutputAsync()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            string testFilesDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "MultiTargetLibrary");

            string libraryProjectPath = Path.Combine(
                testFilesDirectory,
                "MultiTargetLibrary.csproj");

            string net48AssemblyPath = Path.Combine(
                testFilesDirectory,
                "bin",
                "Debug",
                "net48",
                "MultiTargetLibrary.dll");

            File.Exists(libraryProjectPath).Should().BeTrue(
                $"the multi-target library project should exist at {libraryProjectPath}");

            File.Exists(net48AssemblyPath).Should().BeTrue(
                $"the net48 assembly should exist at {net48AssemblyPath}");

            var net48AssemblyVersion =
                System.Reflection.AssemblyName
                    .GetAssemblyName(net48AssemblyPath)
                    .Version;

            var projectFiles = new[]
            {
        new ProjectFile(
            "Script.cs",
            "using System;"),
    };

            var projectReferences = new[]
            {
        new ProjectReference(
            "MultiTargetLibrary",
            libraryProjectPath,
            libraryProjectPath),
    };

            var scriptProject = new Project(
                "Script_1",
                path: Path.Combine(testDirectory, "Script_1.csproj"),
                tfm: "net48",
                projectFiles: projectFiles,
                projectReferences: projectReferences);

            var projects = new Dictionary<string, Project>
            {
                ["Script_1"] = scriptProject,
            };

            string original = @"
<DMSScript>
    <Script>
        <Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
    </Script>
</DMSScript>";

            Script script =
                new Script(XmlDocument.Parse(original));

            AutomationScriptBuilder builder =
                new AutomationScriptBuilder(
                    script,
                    projects,
                    new List<Script> { script },
                    directoryForNuGetConfig: null);

            // Act
            var result =
                await builder.BuildAsync()
                    .ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            string expectedDllImport =
                $"multitargetlibrary/{net48AssemblyVersion}/lib/net48/MultiTargetLibrary.dll";

            result.Assemblies.Should().Contain(
                a =>
                    a.DllImport.Equals(
                        expectedDllImport,
                        StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(
                a =>
                    Path.GetFullPath(a.AssemblyPath)
                        .Equals(
                            Path.GetFullPath(net48AssemblyPath),
                            StringComparison.OrdinalIgnoreCase));

            result.Document.Should().Contain(
                $@"C:\Skyline DataMiner\ProtocolScripts\DllImport\multitargetlibrary\{net48AssemblyVersion}\lib\net48\MultiTargetLibrary.dll");

            result.Document.Should().NotContain("scriptRef");
        }
        [TestMethod]
        public async Task AutomationScriptBuilder_ProjectReferenceHarvesting_RecursiveReferences_PropagatesSelectedTargetFrameworkAsync()
        {
            // Arrange
            string testDirectory =
                TestFixture.InitializeDirectoryForTest();

            string projectReferenceHarvestingDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting");

            string libraryAProjectPath = Path.Combine(
                projectReferenceHarvestingDirectory,
                "RecursiveLibraryA",
                "RecursiveLibraryA.csproj");

            string libraryAAssemblyPath = Path.Combine(
                projectReferenceHarvestingDirectory,
                "RecursiveLibraryA",
                "bin",
                "Debug",
                "netstandard2.0",
                "RecursiveLibraryA.dll");

            string libraryBProjectPath = Path.Combine(
                projectReferenceHarvestingDirectory,
                "MultiTargetLibrary",
                "MultiTargetLibrary.csproj");

            string libraryBNetStandardAssemblyPath = Path.Combine(
                projectReferenceHarvestingDirectory,
                "MultiTargetLibrary",
                "bin",
                "Debug",
                "netstandard2.0",
                "MultiTargetLibrary.dll");

            string libraryBNet48AssemblyPath = Path.Combine(
                projectReferenceHarvestingDirectory,
                "MultiTargetLibrary",
                "bin",
                "Debug",
                "net48",
                "MultiTargetLibrary.dll");

            File.Exists(libraryAProjectPath)
                .Should()
                .BeTrue(
                    $"LibraryA project should exist at {libraryAProjectPath}");

            File.Exists(libraryAAssemblyPath)
                .Should()
                .BeTrue(
                    $"LibraryA assembly should exist at {libraryAAssemblyPath}");

            File.Exists(libraryBProjectPath)
                .Should()
                .BeTrue(
                    $"LibraryB project should exist at {libraryBProjectPath}");

            File.Exists(libraryBNetStandardAssemblyPath)
                .Should()
                .BeTrue(
                    $"LibraryB netstandard2.0 assembly should exist at {libraryBNetStandardAssemblyPath}");

            File.Exists(libraryBNet48AssemblyPath)
                .Should()
                .BeTrue(
                    $"LibraryB net48 assembly should exist at {libraryBNet48AssemblyPath}");

            var libraryAAssemblyVersion =
                AssemblyName
                    .GetAssemblyName(libraryAAssemblyPath)
                    .Version;

            var libraryBNetStandardAssemblyVersion =
                AssemblyName
                    .GetAssemblyName(libraryBNetStandardAssemblyPath)
                    .Version;

            var projectFiles = new[]
            {
        new ProjectFile(
            "Script.cs",
            "using System;"),
    };

            var projectReferences = new[]
            {
        new ProjectReference(
            "RecursiveLibraryA",
            libraryAProjectPath,
            libraryAProjectPath),
    };

            // Root Automation project targets net48.
            var scriptProject = new Project(
                "Script_1",
                path: Path.Combine(
                    testDirectory,
                    "Script_1.csproj"),
                tfm: "net48",
                projectFiles: projectFiles,
                projectReferences: projectReferences);

            var projects = new Dictionary<string, Project>
            {
                ["Script_1"] = scriptProject,
            };

            string original = @"
                <DMSScript>
                    <Script>
                        <Exe id=""1"" type=""csharp"">
                            <Value><![CDATA[[Project:Script_1]]]></Value>
                        </Exe>
                    </Script>
                </DMSScript>";

            Script script = new Script(XmlDocument.Parse(original));

            AutomationScriptBuilder builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, directoryForNuGetConfig: null);

            // Act
            var result = await builder.BuildAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            string expectedLibraryADllImport =
                $"recursive.librarya/{libraryAAssemblyVersion}/lib/netstandard2.0/RecursiveLibraryA.dll";

            string expectedLibraryBDllImport =
                $"multitargetlibrary/{libraryBNetStandardAssemblyVersion}/lib/netstandard2.0/MultiTargetLibrary.dll";

            // LibraryA must be harvested as netstandard2.0,
            // even though the root Automation project targets net48.
            result.Assemblies.Should().Contain(a => a.DllImport.Equals(expectedLibraryADllImport, StringComparison.OrdinalIgnoreCase));

            // LibraryB must receive LibraryA's selected TFM:
            // netstandard2.0, not the root net48.
            result.Assemblies.Should().Contain(a => a.DllImport.Equals(expectedLibraryBDllImport, StringComparison.OrdinalIgnoreCase));

            // Also verify that the actual physical B assembly came
            // from the netstandard2.0 output.
            result.Assemblies.Should().Contain(a => Path.GetFullPath(a.AssemblyPath).Equals(Path.GetFullPath(libraryBNetStandardAssemblyPath), StringComparison.OrdinalIgnoreCase));

            // This is the important regression assertion:
            // recursive harvesting must NOT jump back to the root net48 TFM.
            result.Assemblies.Should().NotContain(a => a.DllImport.Contains("/lib/net48/MultiTargetLibrary.dll"));

            result.Document.Should().Contain($@"C:\Skyline DataMiner\ProtocolScripts\DllImport\recursive.librarya\{libraryAAssemblyVersion}\lib\netstandard2.0\RecursiveLibraryA.dll");

            result.Document.Should().Contain($@"C:\Skyline DataMiner\ProtocolScripts\DllImport\multitargetlibrary\{libraryBNetStandardAssemblyVersion}\lib\netstandard2.0\MultiTargetLibrary.dll");

            result.Document.Should().NotContain("scriptRef");
        }
        [TestMethod]
        public void ProjectLoad_SharedProject_DoesNotCreateAssemblyProjectReference()
        {
            // Arrange
            string consumerDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "SharedProjectTest",
                "Consumer");

            string projectPath = Path.Combine(
                consumerDirectory,
                "Consumer.csproj");

            File.Exists(projectPath).Should().BeTrue();

            // Act
            Project project = Project.Load(projectPath);

            // Assert
            project.Should().NotBeNull();

            project.ProjectReferences.Should().NotContain(reference =>
                reference.Path.EndsWith(
                    ".shproj",
                    StringComparison.OrdinalIgnoreCase));

            project.ProjectReferences.Should().NotContain(reference =>
                reference.Path.Contains("SharedCode"));

        }
        [TestMethod]
        public async Task SharedProject_ConsumerBuildsWithoutProducingSharedAssembly()
        {
            // Arrange
            string rootDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "SharedProjectTest");

            string consumerDirectory = Path.Combine(
                rootDirectory,
                "Consumer");

            string consumerProjectPath = Path.Combine(
                consumerDirectory,
                "Consumer.csproj");

            // Act
            await TestFixture.BuildProjectAsync(consumerProjectPath).ConfigureAwait(false);

            string consumerDll = Path.Combine(
                consumerDirectory,
                "bin",
                "Debug",
                "netstandard2.0",
                "Consumer.dll");

            File.Exists(consumerDll).Should().BeTrue();

            string sharedDll = Path.Combine(
                rootDirectory,
                "SharedCode",
                "bin",
                "Debug",
                "netstandard2.0",
                "SharedCode.dll");

            File.Exists(sharedDll).Should().BeFalse(
                "shared projects contribute source code and should not produce a separate assembly");
        }
        [TestMethod]
        public async Task AutomationScriptBuilder_ProjectReferenceHarvesting_DeepRecursiveReferences_HarvestsAllLibrariesAsync()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            string fixtureDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "DeepRecursiveReferences");

            string libraryAProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryA",
                "LibraryA.csproj");

            string libraryAAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryA",
                "bin",
                "Debug",
                "netstandard2.0",
                "LibraryA.dll");

            string libraryBAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryB",
                "bin",
                "Debug",
                "netstandard2.0",
                "LibraryB.dll");

            string libraryCAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryC",
                "bin",
                "Debug",
                "netstandard2.0",
                "LibraryC.dll");

            string libraryDAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryD",
                "bin",
                "Debug",
                "netstandard2.0",
                "LibraryD.dll");
            await TestFixture.BuildProjectAsync(libraryAProjectPath).ConfigureAwait(false);
            File.Exists(libraryAAssemblyPath).Should().BeTrue();
            File.Exists(libraryBAssemblyPath).Should().BeTrue();
            File.Exists(libraryCAssemblyPath).Should().BeTrue();
            File.Exists(libraryDAssemblyPath).Should().BeTrue();



            string libraryBProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryB",
                "LibraryB.csproj");

            string libraryCProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryC",
                "LibraryC.csproj");

            string libraryDProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryD",
                "LibraryD.csproj");

            var infoA = MSBuildHelpers.EvaluateReferenceProject(
                libraryAProjectPath,
                "netstandard2.0");

            var infoB = MSBuildHelpers.EvaluateReferenceProject(
                libraryBProjectPath,
                "netstandard2.0");

            var infoC = MSBuildHelpers.EvaluateReferenceProject(
                libraryCProjectPath,
                "netstandard2.0");

            var infoD = MSBuildHelpers.EvaluateReferenceProject(
                libraryDProjectPath,
                "netstandard2.0");

            infoA.DirectPackageReferences.Should().Contain(p =>
                p.Id.Equals("Newtonsoft.Json", StringComparison.OrdinalIgnoreCase));

            infoB.DirectPackageReferences.Should().Contain(p =>
                p.Id.Equals("Polly", StringComparison.OrdinalIgnoreCase));

            infoC.DirectPackageReferences.Should().Contain(p =>
                p.Id.Equals("Humanizer.Core", StringComparison.OrdinalIgnoreCase));

            infoD.DirectPackageReferences.Should().Contain(p =>
                p.Id.Equals("CsvHelper", StringComparison.OrdinalIgnoreCase));

            var projectFiles = new[]
            {
       new ProjectFile(
        "Script.cs",
        @"
using DeepRecursiveReferences;

public class Script
{
    public string Run()
    {
        return LibraryA.GetMessage();
    }
}")
    };

            var projectReferences = new[]
            {
        new ProjectReference(
            "LibraryA",
            libraryAProjectPath,
            libraryAProjectPath),
    };

            var scriptProject = new Project(
                "Script_1",
                path: Path.Combine(testDirectory, "Script_1.csproj"),
                    tfm: ".NETFramework,Version=v4.8",
                projectFiles: projectFiles,
                projectReferences: projectReferences);

            var projects = new Dictionary<string, Project>
            {
                ["Script_1"] = scriptProject,
            };

            string original = @"
<DMSScript>
    <Script>
        <Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
    </Script>
</DMSScript>";
            scriptProject.ProjectReferences.Should().HaveCount(1);
            Script script = new Script(XmlDocument.Parse(original));

            AutomationScriptBuilder builder =
                new AutomationScriptBuilder(
                    script,
                    projects,
                    new List<Script> { script },
                    directoryForNuGetConfig: null);

            // Act
            var result = await builder
                .BuildAsync()
                .ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Equals(
                    "test.librarya/1.1.0.0/lib/netstandard2.0/LibraryA.dll",
                    StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Equals(
                    "test.libraryb/2.2.0.0/lib/netstandard2.0/LibraryB.dll",
                    StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Equals(
                    "test.libraryc/3.3.0.0/lib/netstandard2.0/LibraryC.dll",
                    StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Equals(
                    "test.libraryd/4.4.0.0/lib/netstandard2.0/LibraryD.dll",
                    StringComparison.OrdinalIgnoreCase));
            result.Assemblies.Should().Contain(a =>
    a.DllImport.Replace('\\', '/').StartsWith(
        "newtonsoft.json/13.0.3/",
        StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Replace('\\', '/').StartsWith(
                    "polly/7.2.4/",
                    StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Replace('\\', '/').StartsWith(
                    "humanizer.core/2.14.1/",
                    StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Replace('\\', '/').StartsWith(
                    "csvhelper/30.0.1/",
                    StringComparison.OrdinalIgnoreCase));

        }
        [TestMethod]
        public async Task AutomationScriptBuilder_ProjectReferenceHarvesting_ReferencedLibraryPackageReference_IsIncludedAsync()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            string fixtureDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "LibraryWIthPackageReference");

            string libraryProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryWithPackageReference.csproj");

            string libraryAssemblyPath = Path.Combine(
                fixtureDirectory,
                "bin",
                "Debug",
                "netstandard2.0",
                "LibraryWithPackageReference.dll");

            File.Exists(libraryProjectPath).Should().BeTrue();
            File.Exists(libraryAssemblyPath).Should().BeTrue();

            ReferencedProjectInfo info =
                MSBuildHelpers.EvaluateReferenceProject(
                    libraryProjectPath,
                    "netstandard2.0");

            info.Should().NotBeNull();

            info.DirectPackageReferences.Should().Contain(p =>
                p.Id.Equals(
                    "Newtonsoft.Json",
                    StringComparison.OrdinalIgnoreCase));

            var projectFiles = new[]
            {
        new ProjectFile(
            "Script.cs",
            @"
using LibraryWithPackageReference;

public class Script
{
    public string Run()
    {
        return TestLibrary.GetMessage();
    }
}")
    };

            var projectReferences = new[]
            {
        new ProjectReference(
            "LibraryWithPackageReference",
            libraryProjectPath,
            libraryProjectPath),
    };

            var scriptProject = new Project(
                "Script_1",
                path: Path.Combine(testDirectory, "Script_1.csproj"),
                 tfm: ".NETFramework,Version=v4.8",
                projectFiles: projectFiles,
                projectReferences: projectReferences);

            var projects = new Dictionary<string, Project>
            {
                ["Script_1"] = scriptProject,
            };

            string original = @"
<DMSScript>
    <Script>
        <Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
    </Script>
</DMSScript>";

            Script script = new Script(XmlDocument.Parse(original));

            AutomationScriptBuilder builder =
                new AutomationScriptBuilder(
                    script,
                    projects,
                    new List<Script> { script },
                    directoryForNuGetConfig: null);

            // Act
            var result = await builder
                .BuildAsync()
                .ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Equals(
                    "test.librarywithpackagereference/1.2.3.4/lib/netstandard2.0/LibraryWithPackageReference.dll",
                    StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(a =>
 a.DllImport.Replace('\\', '/').StartsWith(
     "newtonsoft.json/13.0.3/",
     StringComparison.OrdinalIgnoreCase));
        }
        [TestMethod]
        public async Task AutomationScriptBuilder_ProjectReferenceHarvesting_DiamondReferences_HarvestsSharedDependencyOnceAsync()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            string fixtureDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "DiamondReferences");

            string libraryAProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryA",
                "LibraryA.csproj");

            string libraryAAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryA",
                "bin",
                "Debug",
                "netstandard2.0",
                "Diamond.LibraryA.dll");

            string libraryBAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryB",
                "bin",
                "Debug",
                "netstandard2.0",
                "Diamond.LibraryB.dll");

            string libraryCAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryC",
                "bin",
                "Debug",
                "netstandard2.0",
                "Diamond.LibraryC.dll");

            string libraryDAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryD",
                "bin",
                "Debug",
                "netstandard2.0",
                "Diamond.LibraryD.dll");

            File.Exists(libraryAProjectPath).Should().BeTrue();

            File.Exists(libraryAAssemblyPath).Should().BeTrue();
            File.Exists(libraryBAssemblyPath).Should().BeTrue();
            File.Exists(libraryCAssemblyPath).Should().BeTrue();
            File.Exists(libraryDAssemblyPath).Should().BeTrue();

            var projectFiles = new[]
            {
        new ProjectFile(
            "Script.cs",
            @"
using DiamondReferences;

public class Script
{
    public string Run()
    {
        return LibraryA.GetMessage();
    }
}")
    };

            // Root Automation Script directly knows ONLY LibraryA.
            var projectReferences = new[]
            {
        new ProjectReference(
            "LibraryA",
            libraryAProjectPath,
            libraryAProjectPath),
    };

            var scriptProject = new Project(
                "Script_1",
                path: Path.Combine(testDirectory, "Script_1.csproj"),
                tfm: ".NETFramework,Version=v4.8",
                projectFiles: projectFiles,
                projectReferences: projectReferences);

            scriptProject.ProjectReferences.Should().HaveCount(1);

            var projects = new Dictionary<string, Project>
            {
                ["Script_1"] = scriptProject,
            };

            string original = @"
<DMSScript>
    <Script>
        <Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
    </Script>
</DMSScript>";

            Script script = new Script(XmlDocument.Parse(original));

            AutomationScriptBuilder builder =
                new AutomationScriptBuilder(
                    script,
                    projects,
                    new List<Script> { script },
                    directoryForNuGetConfig: null);

            // Act
            var result = await builder
                .BuildAsync()
                .ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            result.Assemblies.Count(a =>
                a.DllImport.Contains(
                    "diamond.librarya/"))
                .Should()
                .Be(1);

            result.Assemblies.Count(a =>
                a.DllImport.Contains(
                    "diamond.libraryb/"))
                .Should()
                .Be(1);

            result.Assemblies.Count(a =>
                a.DllImport.Contains(
                    "diamond.libraryc/"))
                .Should()
                .Be(1);

            result.Assemblies.Count(a =>
                a.DllImport.Contains(
                    "diamond.libraryd/"))
                .Should()
                .Be(1);

            // The shared dependency D must not be harvested twice.
            result.Assemblies
                .Where(a =>
                    a.DllImport.Contains(
                        "diamond.libraryd/"))
                .Should()
                .ContainSingle();

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Equals(
                    "diamond.librarya/1.0.0.0/lib/netstandard2.0/Diamond.LibraryA.dll",
                    StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Equals(
                    "diamond.libraryb/2.0.0.0/lib/netstandard2.0/Diamond.LibraryB.dll",
                    StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Equals(
                    "diamond.libraryc/3.0.0.0/lib/netstandard2.0/Diamond.LibraryC.dll",
                    StringComparison.OrdinalIgnoreCase));

            result.Assemblies.Should().Contain(a =>
                a.DllImport.Equals(
                    "diamond.libraryd/4.0.0.0/lib/netstandard2.0/Diamond.LibraryD.dll",
                    StringComparison.OrdinalIgnoreCase));
        }
        [TestMethod]
        public async Task AutomationScriptBuilder_ProjectReferenceHarvesting_RecursiveMultiTargetReferences_PropagatesSelectedTargetFrameworkAsync()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            string fixtureDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "RecursiveMultiTargetReferences");

            string libraryAProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryA",
                "LibraryA.csproj");

            string libraryBProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryB",
                "LibraryB.csproj");

            string libraryCProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryC",
                "LibraryC.csproj");

            string libraryAAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryA",
                "bin",
                "Debug",
                "netstandard2.0",
                "RecursiveMultiTarget.LibraryA.dll");

            string libraryBNetStandardAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryB",
                "bin",
                "Debug",
                "netstandard2.0",
                "RecursiveMultiTarget.LibraryB.dll");

            string libraryBNet48AssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryB",
                "bin",
                "Debug",
                "net48",
                "RecursiveMultiTarget.LibraryB.dll");

            string libraryCNetStandard20AssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryC",
                "bin",
                "Debug",
                "netstandard2.0",
                "RecursiveMultiTarget.LibraryC.dll");

            string libraryCNetStandard21AssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryC",
                "bin",
                "Debug",
                "netstandard2.1",
                "RecursiveMultiTarget.LibraryC.dll");

            File.Exists(libraryAProjectPath).Should().BeTrue();
            File.Exists(libraryBProjectPath).Should().BeTrue();
            File.Exists(libraryCProjectPath).Should().BeTrue();
            await TestFixture.BuildProjectAsync(libraryCProjectPath).ConfigureAwait(false);
            await TestFixture.BuildProjectAsync(libraryBProjectPath).ConfigureAwait(false);
            await TestFixture.BuildProjectAsync(libraryAProjectPath).ConfigureAwait(false);
            File.Exists(libraryAAssemblyPath).Should().BeTrue();

            // Important: both B outputs exist.
            File.Exists(libraryBNetStandardAssemblyPath).Should().BeTrue();
            File.Exists(libraryBNet48AssemblyPath).Should().BeTrue();

            // Important: both C outputs exist.
            File.Exists(libraryCNetStandard20AssemblyPath).Should().BeTrue();
            File.Exists(libraryCNetStandard21AssemblyPath).Should().BeTrue();

            var projectFiles = new[]
            {
        new ProjectFile(
            "Script.cs",
            @"
using RecursiveMultiTargetReferences;

public class Script
{
    public string Run()
    {
        return LibraryA.GetMessage();
    }
}")
    };

            // Root script directly references ONLY LibraryA.
            var projectReferences = new[]
            {
        new ProjectReference(
            "LibraryA",
            libraryAProjectPath,
            libraryAProjectPath),
    };

            var scriptProject = new Project(
                "Script_1",
                path: Path.Combine(
                    testDirectory,
                    "Script_1.csproj"),
                tfm: ".NETFramework,Version=v4.8",
                projectFiles: projectFiles,
                projectReferences: projectReferences);

            scriptProject.ProjectReferences
                .Should()
                .HaveCount(1);

            var projects = new Dictionary<string, Project>
            {
                ["Script_1"] = scriptProject,
            };

            string original = @"
<DMSScript>
    <Script>
        <Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
    </Script>
</DMSScript>";

            Script script =
                new Script(XmlDocument.Parse(original));

            AutomationScriptBuilder builder =
                new AutomationScriptBuilder(
                    script,
                    projects,
                    new List<Script> { script },
                    directoryForNuGetConfig: null);

            // Act
            var result = await builder
                .BuildAsync()
                .ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            // A only targets netstandard2.0.
            result.Assemblies.Should().Contain(a =>
                a.DllImport.Replace('\\', '/').Equals(
                    "recursivemultitarget.librarya/1.0.0.0/lib/netstandard2.0/RecursiveMultiTarget.LibraryA.dll",
                    StringComparison.OrdinalIgnoreCase));

            // B supports netstandard2.0 and net48.
            // It must inherit A's selected netstandard2.0.
            result.Assemblies.Should().Contain(a =>
                a.DllImport.Replace('\\', '/').Equals(
                    "recursivemultitarget.libraryb/2.0.0.0/lib/netstandard2.0/RecursiveMultiTarget.LibraryB.dll",
                    StringComparison.OrdinalIgnoreCase));

            // C supports netstandard2.0 and netstandard2.1.
            // It must inherit B's selected netstandard2.0.
            result.Assemblies.Should().Contain(a =>
                a.DllImport.Replace('\\', '/').Equals(
                    "recursivemultitarget.libraryc/3.0.0.0/lib/netstandard2.0/RecursiveMultiTarget.LibraryC.dll",
                    StringComparison.OrdinalIgnoreCase));

            // B must NOT fall back to the root script's net48.
            result.Assemblies.Should().NotContain(a =>
                a.DllImport.Replace('\\', '/').Contains(
                    "recursivemultitarget.libraryb/2.0.0.0/lib/net48/"));

            // C must NOT select its other target.
            result.Assemblies.Should().NotContain(a =>
                a.DllImport.Replace('\\', '/').Contains(
                    "recursivemultitarget.libraryc/3.0.0.0/lib/netstandard2.1/"));
        }
        [TestMethod]
        public async Task AutomationScriptBuilder_ProjectReferenceHarvesting_RecursiveLibraryWithSharedProject_HarvestsLibrariesWithoutSharedAssemblyAsync()
        {
            // Arrange
            string testDirectory = TestFixture.InitializeDirectoryForTest();

            string fixtureDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "TestFiles",
                "ProjectReferenceHarvesting",
                "RecursiveSharedProjectReference");

            string libraryAProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryA",
                "LibraryA.csproj");

            string libraryBProjectPath = Path.Combine(
                fixtureDirectory,
                "LibraryB",
                "LibraryB.csproj");

            string sharedProjectPath = Path.Combine(
                fixtureDirectory,
                "SharedProject",
                "SharedProject.shproj");

            string sharedProjItemsPath = Path.Combine(
                fixtureDirectory,
                "SharedProject",
                "SharedProject.projitems");

            File.Exists(libraryAProjectPath).Should().BeTrue();
            File.Exists(libraryBProjectPath).Should().BeTrue();
            File.Exists(sharedProjectPath).Should().BeTrue();
            File.Exists(sharedProjItemsPath).Should().BeTrue();

            // Build A. This builds B as its ProjectReference.
            // SharedProject source is compiled into B.
            await TestFixture.BuildProjectAsync(libraryAProjectPath).ConfigureAwait(false);

            string libraryAAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryA",
                "bin",
                "Debug",
                "netstandard2.0",
                "RecursiveShared.LibraryA.dll");

            string libraryBAssemblyPath = Path.Combine(
                fixtureDirectory,
                "LibraryB",
                "bin",
                "Debug",
                "netstandard2.0",
                "RecursiveShared.LibraryB.dll");

            string sharedAssemblyPath = Path.Combine(
                fixtureDirectory,
                "SharedProject",
                "bin",
                "Debug",
                "netstandard2.0",
                "SharedProject.dll");

            File.Exists(libraryAAssemblyPath)
                .Should()
                .BeTrue();

            File.Exists(libraryBAssemblyPath)
                .Should()
                .BeTrue();

            // Shared Project must not produce its own assembly.
            File.Exists(sharedAssemblyPath)
                .Should()
                .BeFalse();

            var projectFiles = new[]
            {
        new ProjectFile(
            "Script.cs",
            @"
using RecursiveSharedProjectReference;

public class Script
{
    public string Run()
    {
        return LibraryA.GetMessage();
    }
}")
    };

            var projectReferences = new[]
            {
        new ProjectReference(
            "LibraryA",
            libraryAProjectPath,
            libraryAProjectPath),
    };

            var scriptProject = new Project(
                "Script_1",
                path: Path.Combine(
                    testDirectory,
                    "Script_1.csproj"),
                tfm: ".NETFramework,Version=v4.8",
                projectFiles: projectFiles,
                projectReferences: projectReferences);

            // Script directly references ONLY A.
            scriptProject.ProjectReferences
                .Should()
                .HaveCount(1);

            var projects = new Dictionary<string, Project>
            {
                ["Script_1"] = scriptProject,
            };

            string original = @"
<DMSScript>
    <Script>
        <Exe id=""1"" type=""csharp"">
            <Value><![CDATA[[Project:Script_1]]]></Value>
        </Exe>
    </Script>
</DMSScript>";

            Script script =
                new Script(XmlDocument.Parse(original));

            AutomationScriptBuilder builder =
                new AutomationScriptBuilder(
                    script,
                    projects,
                    new List<Script> { script },
                    directoryForNuGetConfig: null);

            // Act
            var result = await builder
                .BuildAsync()
                .ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            // A is harvested.
            result.Assemblies.Should().Contain(a =>
                a.DllImport
                    .Replace('\\', '/')
                    .Equals(
                        "recursiveshared.librarya/1.0.0.0/lib/netstandard2.0/RecursiveShared.LibraryA.dll",
                        StringComparison.OrdinalIgnoreCase));

            // B is recursively harvested.
            result.Assemblies.Should().Contain(a =>
                a.DllImport
                    .Replace('\\', '/')
                    .Equals(
                        "recursiveshared.libraryb/2.0.0.0/lib/netstandard2.0/RecursiveShared.LibraryB.dll",
                        StringComparison.OrdinalIgnoreCase));

            // Shared Project must not become a harvested assembly.
            result.Assemblies.Should().NotContain(a =>
                a.DllImport.Contains(
                    "SharedProject.dll"));

            result.Assemblies.Should().NotContain(a =>
                a.AssemblyPath != null &&
                a.AssemblyPath.EndsWith(
                    "SharedProject.dll",
                    StringComparison.OrdinalIgnoreCase));

            // Only A is directly referenced by the root script.
            scriptProject.ProjectReferences
                .Should()
                .ContainSingle();
        }
    }


}
