namespace Assemblers.ProtocolTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Org.XmlUnit.Builder;
    using Org.XmlUnit.Diff;

    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.Assemblers.Protocol;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;
    using Skyline.DataMiner.CICD.Parsers.Protocol.VisualStudio;

    [TestClass]
    public class ProtocolBuilderTests
    {
        [TestMethod]
        public async Task ProtocolBuilder_BuildAsync_IgnoreSatelliteAssemblies()
        {
            var logCollector = new Logging(true);

            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solutions\Solution2"));
            var solutionFilePath = FileSystem.Instance.Path.Combine(dir, "protocol.sln");

            ProtocolSolution solution = ProtocolSolution.Load(solutionFilePath, logCollector);
            ProtocolBuilder protocolBuilder = new ProtocolBuilder(solution, logCollector);

            var buildResultItems = await protocolBuilder.BuildAsync();

            Assert.IsNotNull(buildResultItems.Assemblies);
            Assert.AreEqual(1, buildResultItems.Assemblies.Count);
            Assert.AreEqual(@"microsoft.visualstudio.validation\17.8.8\lib\netstandard2.0\Microsoft.VisualStudio.Validation.dll", buildResultItems.Assemblies.First().DllImport);
        }

        [TestMethod]
        public async Task ProtocolBuilder_BuildAsync_KeepReferenceAssemblies()
        {
            var logCollector = new Logging(true);

            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solutions\Solution3"));
            var solutionFilePath = FileSystem.Instance.Path.Combine(dir, "protocol.sln");

            ProtocolSolution solution = ProtocolSolution.Load(solutionFilePath, logCollector);
            ProtocolBuilder protocolBuilder = new ProtocolBuilder(solution, logCollector);

            var buildResultItems = await protocolBuilder.BuildAsync();

            Assert.IsNotNull(buildResultItems.Assemblies);
            Assert.AreEqual(18, buildResultItems.Assemblies.Count);
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_BasicAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"">
			<![CDATA[using System;]]>
		</QAction>
	</QActions>
</Protocol>";

            var projects = new Dictionary<string, Project>()
            {
                { "QAction_1", new Project("QAction_1", new[]{ new ProjectFile("QAction_1.cs", "using System;") }) },
            };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_MultipleQActionsAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" />
		<QAction id=""2"" encoding=""csharp"" name=""QAction 2"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"">
			<![CDATA[using System;]]>
		</QAction>
		<QAction id=""2"" encoding=""csharp"" name=""QAction 2"">
			<![CDATA[using System.Xml;]]>
		</QAction>
	</QActions>
</Protocol>";

            var projects = new Dictionary<string, Project>()
            {
                { "QAction_1", new Project("QAction_1", new[]{ new ProjectFile("QAction_1.cs", "using System;") }) },
                { "QAction_2", new Project("QAction_2", new[]{ new ProjectFile("QAction_2.cs", "using System.Xml;") }) },
            };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_VersionHistoryAsync()
        {
            string originalProtocol = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no"" ?>
<Protocol>
	<VersionHistory>
		<Branches>
			<Branch id=""1"">
				<SystemVersions>
					<SystemVersion id=""0"">
						<MajorVersions>
							<MajorVersion id=""0"">
								<MinorVersions>
									<MinorVersion id=""1"">
										<Changes>
											<Change>Change1</Change>
											<Fix>Fix1</Fix>
											<NewFeature>NewFeature1</NewFeature>
										</Changes>
										<Date>2020-01-02</Date>
										<Provider>
											<Author>TWA</Author>
											<Company>Skyline Communications</Company>
										</Provider>
									</MinorVersion>
									<MinorVersion id=""10"">
										<Changes>
											<Change>Change2</Change>
										</Changes>
										<Date>2020-01-03</Date>
										<Provider>
											<Author>TWA</Author>
											<Company>Skyline Communications</Company>
										</Provider>
									</MinorVersion>
									<MinorVersion id=""11"">
										<Changes></Changes><!-- empty to detect IndexOutOfRange -->
										<Date>2020-01-04</Date>
										<Provider>
											<Author>TWA</Author>
											<Company>Skyline Communications</Company>
										</Provider>
									</MinorVersion>
								</MinorVersions>
							</MajorVersion>
						</MajorVersions>
					</SystemVersion>
				</SystemVersions>
			</Branch>
		</Branches>
	</VersionHistory>
</Protocol>";

            string expected = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no"" ?>
<!--

Revision History (auto generated):

DATE          VERSION     AUTHOR                         COMMENTS

02/01/2020    1.0.0.1     TWA, Skyline Communications    Fix: Fix1
                                                         Change: Change1
                                                         NF: NewFeature1
03/01/2020    1.0.0.10    TWA, Skyline Communications    Change: Change2
04/01/2020    1.0.0.11    TWA, Skyline Communications    
-->
<Protocol>
	<VersionHistory>
		<Branches>
			<Branch id=""1"">
				<SystemVersions>
					<SystemVersion id=""0"">
						<MajorVersions>
							<MajorVersion id=""0"">
								<MinorVersions>
									<MinorVersion id=""1"">
										<Changes>
											<Change>Change1</Change>
											<Fix>Fix1</Fix>
											<NewFeature>NewFeature1</NewFeature>
										</Changes>
										<Date>2020-01-02</Date>
										<Provider>
											<Author>TWA</Author>
											<Company>Skyline Communications</Company>
										</Provider>
									</MinorVersion>
									<MinorVersion id=""10"">
										<Changes>
											<Change>Change2</Change>
										</Changes>
										<Date>2020-01-03</Date>
										<Provider>
											<Author>TWA</Author>
											<Company>Skyline Communications</Company>
										</Provider>
									</MinorVersion>
									<MinorVersion id=""11"">
										<Changes></Changes><!-- empty to detect IndexOutOfRange -->
										<Date>2020-01-04</Date>
										<Provider>
											<Author>TWA</Author>
											<Company>Skyline Communications</Company>
										</Provider>
									</MinorVersion>
								</MinorVersions>
							</MajorVersion>
						</MajorVersions>
					</SystemVersion>
				</SystemVersions>
			</Branch>
		</Branches>
	</VersionHistory>
</Protocol>";

            var projects = new Dictionary<string, Project>();

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_MultipleFilesAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""3"" encoding=""csharp"" name=""QAction 3"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""3"" encoding=""csharp"" name=""QAction 3"">
			<![CDATA[using System;
//---------------------------------
// QAction_3.cs
//---------------------------------

//---------------------------------
// Class1.cs
//---------------------------------
class Class1 {}]]>
		</QAction>
	</QActions>
</Protocol>";

            var projects = new Dictionary<string, Project>()
            {
                { "QAction_3", new Project("QAction_3", new[]{
                    new ProjectFile("QAction_3.cs", "using System;"),
                    new ProjectFile("Class1.cs", "using System; class Class1 {}")
                }) },
            };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public void ProtocolCompiler_ProtocolBuilder_TargetNotEmpty()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"">
            <![CDATA[using System;]]>
        </QAction>
	</QActions>
</Protocol>";

            var projects = new Dictionary<string, Project>()
            {
                { "QAction_1", new Project("QAction_1", new[]{ new ProjectFile("QAction_1.cs", "using System.Xml;") }) },
            };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            var exception = Assert.Throws<AggregateException>(() => builder.BuildAsync().Result);

            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOfType(exception.InnerException, typeof(AssemblerException));
            Assert.AreEqual("Cannot replace QAction 1, because the target XML node is not empty!", exception.InnerException.Message);
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_DllImportsAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" dllImport=""System.Data.dll"">
			<![CDATA[using System;]]>
		</QAction>
	</QActions>
</Protocol>";

            var projectFiles = new[] { new ProjectFile("QAction_1.cs", "using System;") };
            var references = new[] { new Reference("System.Data.dll"), new Reference("System.Xml.dll") };
            var project1 = new Project("QAction_1", projectFiles: projectFiles, references: references);

            var projects = new Dictionary<string, Project>()
            {
                { "QAction_1", project1 },
            };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_DllImports_NoDuplicateAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" dllImport=""System.Data.dll"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" dllImport=""System.Data.dll"">
			<![CDATA[using System;]]>
		</QAction>
	</QActions>
</Protocol>";

            var projectFiles = new[] { new ProjectFile("QAction_1.cs", "using System;") };
            var references = new[] { new Reference("System.Data.dll"), new Reference("System.Xml.dll") };
            var project1 = new Project("QAction_1", projectFiles: projectFiles, references: references);

            var projects = new Dictionary<string, Project>()
            {
                { "QAction_1", project1 },
            };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_DllImports_ProjectReference()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" dllImport=""MyLibrary.dll"">
			<![CDATA[using System;]]>
		</QAction>
	</QActions>
</Protocol>";

            var projectFiles = new[] { new ProjectFile("QAction_1.cs", "using System;") };
            var projectReferences = new[] { new ProjectReference("MyLibrary") };
            var project1 = new Project("QAction_1", projectFiles: projectFiles, projectReferences: projectReferences);

            var projects = new Dictionary<string, Project>()
            {
                { "QAction_1", project1 },
            };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_ClassLibraryAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""63000"" encoding=""csharp"" name=""** Auto-generated Class Library **"" options=""precompile"" />
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""63000"" encoding=""csharp"" name=""** Auto-generated Class Library **"" options=""precompile"">
			<![CDATA[namespace Skyline.DataMiner.Library { }]]>
		</QAction>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" dllImport=""System.Data.dll;[ProtocolName].[ProtocolVersion].QAction.63000.dll"">
			<![CDATA[using System;]]>
		</QAction>
	</QActions>
</Protocol>";

            var project63000 = new Project("QAction_63000", new[] { new ProjectFile("QAction_63000.cs", "namespace Skyline.DataMiner.Library { }") });

            var projectFiles = new[] { new ProjectFile("QAction_1.cs", "using System;") };
            var references = new[] { new Reference("System.Data.dll"), new Reference("System.Xml.dll") };
            var projectReferences = new[] { new ProjectReference("QAction_63000") };
            var project1 = new Project("QAction_1", projectFiles: projectFiles, projectReferences: projectReferences, references: references);

            var projects = new Dictionary<string, Project>()
            {
                { "QAction_63000", project63000 },
                { "QAction_1", project1 },
            };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_ReferencedQActionAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" options=""precompile"" />
		<QAction id=""2"" encoding=""csharp"" name=""QAction 2"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" options=""precompile"">
			<![CDATA[using System;]]>
		</QAction>
		<QAction id=""2"" encoding=""csharp"" name=""QAction 2"" dllImport=""[ProtocolName].[ProtocolVersion].QAction.1.dll"">
			<![CDATA[using System;]]>
		</QAction>
	</QActions>
</Protocol>";

            var project1 = new Project("QAction_1", new[] { new ProjectFile("QAction_1.cs", "using System;") });

            var projectReferences = new[] { new ProjectReference("QAction_1") };
            var project2 = new Project("QAction_2", projectFiles: new[] { new ProjectFile("QAction_2.cs", "using System;") }, projectReferences: projectReferences);

            var projects = new Dictionary<string, Project>()
                               {
                                   { "QAction_1", project1 },
                                   { "QAction_2", project2 },
                               };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_ReferencedQActionWithCustomNameAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" options=""precompile;dllName=Common.dll"" />
		<QAction id=""2"" encoding=""csharp"" name=""QAction 2"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" options=""precompile;dllName=Common.dll"">
			<![CDATA[using System;]]>
		</QAction>
		<QAction id=""2"" encoding=""csharp"" name=""QAction 2"" dllImport=""[ProtocolName].[ProtocolVersion].Common.dll"">
			<![CDATA[using System;]]>
		</QAction>
	</QActions>
</Protocol>";

            var project1 = new Project("QAction_1", new[] { new ProjectFile("QAction_1.cs", "using System;") });

            var projectReferences = new[] { new ProjectReference("QAction_1") };
            var project2 = new Project("QAction_2", projectFiles: new[] { new ProjectFile("QAction_2.cs", "using System;") }, projectReferences: projectReferences);

            var projects = new Dictionary<string, Project>()
                               {
                                   { "QAction_1", project1 },
                                   { "QAction_2", project2 },
                               };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public void ProtocolCompiler_ProtocolBuilder_MissingQAction()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" />
	</QActions>
</Protocol>";

            var projects = new Dictionary<string, Project>();
            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);
            var exception = Assert.Throws<AggregateException>(() => builder.BuildAsync().Result);

            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOfType(exception.InnerException, typeof(AssemblerException));
            Assert.AreEqual("Project with name 'QAction_1' could not be found!", exception.InnerException.Message);
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_MissingNameAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"">
			<![CDATA[using System;]]>
		</QAction>
	</QActions>
</Protocol>";

            var projects = new Dictionary<string, Project>()
            {
                { "QAction_1", new Project("QAction_1", new[]{ new ProjectFile("QAction_1.cs", "using System;") }) },
            };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_JScriptQActionAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""jscript"" name=""QAction 1"">
 			<![CDATA[
				id:123 = ""test"";
			]]>
		</QAction>
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""jscript"" name=""QAction 1"">
 			<![CDATA[
				id:123 = ""test"";
			]]>
		</QAction>
	</QActions>
</Protocol>";

            var projects = new Dictionary<string, Project>();

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_SpecialCharactersAsync()
        {
            string originalProtocol = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"" />
	</QActions>
</Protocol>";

            string expected = @"<Protocol>
	<QActions>
		<QAction id=""1"" encoding=""csharp"" name=""QAction 1"">
			<![CDATA[using Characterø;]]>
		</QAction>
	</QActions>
</Protocol>";

            var projects = new Dictionary<string, Project>()
            {
                { "QAction_1", new Project("QAction_1", new[]{ new ProjectFile("QAction_1.cs", "using Characterø;") }) },
            };

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects);

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_Solution_Build()
        {
            // arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));
            var path = FileSystem.Instance.Path.Combine(dir, "Protocol.sln");

            var solution = ProtocolSolution.Load(path);

            // act
            ProtocolBuilder builder = new ProtocolBuilder(solution);
            var buildResultItems = await builder.BuildAsync().ConfigureAwait(false);

            string result = buildResultItems.Document;

            // check
            string expected = @"<Protocol xmlns=""http://www.skyline.be/protocol"">
	<QActions>
		<QAction id=""63000"" name=""** Auto-generated Class Library **"" encoding=""csharp"" options=""precompile"">
			<![CDATA[namespace QAction_63000
{
    public class QAction_63000
    {
    }
}
]]>
		</QAction>
		<QAction id=""1"" name=""QA1"" encoding=""csharp"" triggers=""1"" options=""precompile"" dllImport=""Newtonsoft.Json.dll;[ProtocolName].[ProtocolVersion].QAction.63000.dll"">
			<![CDATA[namespace QAction_1
{
    public class Class1
    {
    }
}
]]>
		</QAction>
		<QAction id=""2"" name=""QA2"" encoding=""csharp"" triggers=""2"" dllImport=""[ProtocolName].[ProtocolVersion].QAction.1.dll;[ProtocolName].[ProtocolVersion].QAction.63000.dll"">
			<![CDATA[namespace QAction_2
{
    public class QAction_2
    {
    }
}
]]>
		</QAction>
		<QAction id=""3"" name=""QA3"" encoding=""csharp"" triggers=""3"" dllImport=""Microsoft.CSharp.dll;System.Data.dll;System.Data.DataSetExtensions.dll;System.Drawing.dll;System.IO.Compression.FileSystem.dll;System.Runtime.Caching.dll;System.Runtime.Serialization.dll;System.Xml.Linq.dll;SRM\SLSRMLibrary.dll;[ProtocolName].[ProtocolVersion].QAction.63000.dll"">
			<![CDATA[
//---------------------------------
// QAction_3.cs
//---------------------------------
namespace QAction_3
{
    public class QAction_3
    {
    }
}

//---------------------------------
// Class1.cs
//---------------------------------
namespace QAction_3
{
    public class Class1
    {
    }
}

//---------------------------------
// SubDir\Class2.cs
//---------------------------------
namespace QAction_3
{
    public class Class2
    {
    }
}
]]>
		</QAction>
		<QAction id=""4"" name=""QA4"" encoding=""jscript"" triggers=""4"">
			<![CDATA[
				id:123 = ""test"";
			]]>
		</QAction>
    </QActions>
</Protocol>";

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public async Task ProtocolCompiler_ProtocolBuilder_OverrideVersion()
        {
            string originalProtocol = @"<Protocol>
	<Version>1.0.0.1</Version>
</Protocol>";

            string expected = @"<Protocol>
	<Version>1.0.0.1_DIS</Version>
</Protocol>";

            var projects = new Dictionary<string, Project>(0);

            ProtocolBuilder builder = new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects, "1.0.0.1_DIS");

            string result = (await builder.BuildAsync().ConfigureAwait(false)).Document;

            Diff d = DiffBuilder.Compare(Input.FromString(expected))
                                .WithTest(Input.FromString(result)).Build();

            Assert.IsFalse(d.HasDifferences(), d.ToString());
        }

        [TestMethod]
        public void ProtocolCompiler_ProtocolBuilder_OverrideVersion_MissingVersionTag()
        {
            string originalProtocol = @"<Protocol>
</Protocol>";

            var projects = new Dictionary<string, Project>(0);

            Assert.Throws<AssemblerException>(() => new ProtocolBuilder(XmlDocument.Parse(originalProtocol), projects, "1.0.0.1_DIS"));
        }

        [TestMethod]
        public async Task ProtocolSolution_BuildAsync_SolutionLibraries()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Solutions\Solution4"));
            var path = FileSystem.Instance.Path.Combine(dir, "Protocol.sln");

            var solution = ProtocolSolution.Load(path);

            // Act
            ProtocolBuilder builder = new ProtocolBuilder(solution);
            var result = await builder.BuildAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Document.Should()
                  .ContainEquivalentOf(
                      "dllImport=\"SolutionLibraries\\ModSolutionLib\\Skyline.DataMiner.Dev.Utils.ModSolutionLib.dll\"");

            // Only needs to be referenced, shouldn't be part of the script itself
            result.Assemblies.Should().BeEmpty();
            result.DllAssemblies.Should().BeEmpty();
        }
    }
}
