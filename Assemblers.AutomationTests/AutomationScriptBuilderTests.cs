namespace Assemblers.AutomationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using FluentAssertions;

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
    }
}
