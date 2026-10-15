namespace Assemblers.AutomationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Automation;
    using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects;
    using Skyline.DataMiner.CICD.Packages.TestHelpers;
    using Skyline.DataMiner.CICD.Parsers.Automation.Xml;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    [TestClass]
    [DoNotParallelize]
    public class BuildOnlyAutomationScriptBuilderTests
    {
        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task BuildAsync_SelectedBuildOnlyPackage_DiscardsOldImportsAndPayload(bool populatedCache)
        {
            using var fixture = new BuildOnlyPackageFixture("analyzers/dotnet/cs/Fixture.Generator.dll");
            if (populatedCache)
            {
                await fixture.PopulateCacheAsync();
            }
            else
            {
                Directory.GetFileSystemEntries(fixture.CachePath).Should().BeEmpty();
            }

            var project = new Project(
                "BuildOnlyScript",
                tfm: BuildOnlyPackageFixture.TargetFrameworkMoniker,
                projectFiles: new[] { new ProjectFile("Script.cs", "using Skyline.DataMiner.Automation; namespace Fixture { public class Script { public void Run(IEngine engine) { } } }") },
                packageReferences: fixture.ProjectPackages.Select(package => new PackageReference(package.Id, package.Version.ToString())).ToList());
            var projects = new Dictionary<string, Project> { { project.AssemblyName, project } };
            string xml = @"<DMSScript options=""272"" xmlns=""http://www.skyline.be/automation"">
  <Name>BuildOnlyScript</Name>
  <Description></Description>
  <Type>Automation</Type>
  <Author>SkylineCommunications</Author>
  <CheckSets>FALSE</CheckSets>
  <Folder></Folder>
  <Protocols></Protocols>
  <Memory></Memory>
  <Parameters></Parameters>
  <Script>
    <Exe id=""1"" type=""csharp"">
      <Value><![CDATA[[Project:BuildOnlyScript]]]></Value>
      <Message></Message>
    </Exe>
  </Script>
</DMSScript>";
            var script = new Script(XmlDocument.Parse(xml));
            var builder = new AutomationScriptBuilder(script, projects, new List<Script> { script }, fixture.RootPath);

            var result = await builder.BuildAsync().ConfigureAwait(false);

            var references = XDocument.Parse(result.Document).Descendants()
                .Where(element => element.Name.LocalName == "Param" && (string?)element.Attribute("type") == "ref")
                .Select(element => element.Value)
                .ToArray();
            references.Should().HaveCount(3);
            references.Should().NotContain(reference => reference.IndexOf(BuildOnlyPackageFixture.PackageId, StringComparison.OrdinalIgnoreCase) >= 0);
            references.Should().NotContain(reference => reference.IndexOf("Fixture.Generator", StringComparison.OrdinalIgnoreCase) >= 0);
            references.Should().Contain(reference => reference.EndsWith(@"\fixture.runtime\1.0.0\lib\net48\Fixture.Runtime.dll", StringComparison.OrdinalIgnoreCase));
            result.Assemblies.Select(reference => reference.DllImport).Should().BeEquivalentTo(new[]
            {
                @"fixture.left\1.0.0\lib\net48\Fixture.Left.dll",
                @"fixture.right\1.0.0\lib\net48\Fixture.Right.dll",
                @"fixture.runtime\1.0.0\lib\net48\Fixture.Runtime.dll",
            });
            result.Assemblies.Should().OnlyContain(reference => File.Exists(reference.AssemblyPath));
            result.DllAssemblies.Should().BeEmpty();
        }
    }
}
