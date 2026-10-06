namespace Assemblers.AutomationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using FluentAssertions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Automation;
    using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects;
    using Skyline.DataMiner.CICD.Packages.TestHelpers;
    using Skyline.DataMiner.CICD.Parsers.Automation.Xml;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    [TestClass]
    [DoNotParallelize]
    public class HarvestedBuildOnlyPackageTests
    {
        private static readonly XNamespace AutomationNamespace = "http://www.skyline.be/automation";

        [TestMethod]
        [DataRow("Automation", "Transitive", false)]
        [DataRow("Automation", "Transitive", true)]
        [DataRow("Automation", "Diamond", false)]
        [DataRow("Automation", "Diamond", true)]
        [DataRow("Automation", "MultiTarget", false)]
        [DataRow("Automation", "MultiTarget", true)]
        [DataRow("GQI", "Transitive", false)]
        [DataRow("GQI", "Transitive", true)]
        [DataRow("GQI", "Diamond", false)]
        [DataRow("GQI", "Diamond", true)]
        [DataRow("GQI", "MultiTarget", false)]
        [DataRow("GQI", "MultiTarget", true)]
        public async Task BuildAsync_HarvestedLibraries_PreserveOutputsWithoutDiscardedBuildOnlyAssets(
            string kind, string graph, bool populatedCache)
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

            var libraries = new List<string>();
            string shared = CreateLibrary(fixture, "Shared", "net48", new[]
            {
                new PackageReference("Fixture.Right", "1.0.0"),
            });
            libraries.Add(shared);
            string left = CreateLibrary(fixture, "Left", "net48", new[]
            {
                new PackageReference("Fixture.Left", "1.0.0"),
            }, shared);
            libraries.Add(left);

            string[] rootReferences = { left };
            if (graph == "Diamond")
            {
                string right = CreateLibrary(fixture, "Right", "net48", Array.Empty<PackageReference>(), shared);
                libraries.Add(right);
                rootReferences = new[] { left, right };
            }
            else if (graph == "MultiTarget")
            {
                string multi = CreateLibrary(fixture, "Multi", "net10.0;net48", Array.Empty<PackageReference>(), left);
                libraries.Add(multi);
                rootReferences = new[] { multi };
            }

            var project = CreateScriptProject(fixture, "BuildOnly" + kind, rootReferences,
                new[] { new PackageReference(BuildOnlyPackageFixture.PackageId, BuildOnlyPackageFixture.SelectedVersion) }, kind == "GQI");
            Script script = CreateScript(project.AssemblyName, kind == "GQI");
            var builder = new AutomationScriptBuilder(script,
                new Dictionary<string, Project> { [project.AssemblyName] = project }, new[] { script }, fixture.RootPath);

            var result = await builder.BuildAsync().ConfigureAwait(false);

            var document = XDocument.Parse(result.Document);
            var references = GetReferences(document);
            references.Should().NotContain(value => value.IndexOf(BuildOnlyPackageFixture.PackageId, StringComparison.OrdinalIgnoreCase) >= 0);
            references.Should().NotContain(value => value.IndexOf("Fixture.Generator", StringComparison.OrdinalIgnoreCase) >= 0);
            result.Assemblies.Should().HaveCount(libraries.Count + 3);
            result.Assemblies.Should().NotContain(value => value.DllImport.IndexOf(BuildOnlyPackageFixture.PackageId, StringComparison.OrdinalIgnoreCase) >= 0);
            result.Assemblies.Should().NotContain(value => value.DllImport.IndexOf("net10.0", StringComparison.OrdinalIgnoreCase) >= 0);
            foreach (string library in libraries)
            {
                string name = Path.GetFileNameWithoutExtension(library);
                string output = Path.Combine(Path.GetDirectoryName(library)!, "bin", "Debug", "net48", name + ".dll");
                result.Assemblies.Should().ContainSingle(value => value.AssemblyPath == output);
                references.Should().ContainSingle(value => value.EndsWith(@"\" + name + ".dll", StringComparison.OrdinalIgnoreCase));
            }

            foreach (string name in new[] { "Fixture.Left.dll", "Fixture.Right.dll", "Fixture.Runtime.dll" })
            {
                result.Assemblies.Should().ContainSingle(value => value.DllImport.EndsWith(name, StringComparison.OrdinalIgnoreCase));
            }

            result.Assemblies.Should().OnlyContain(value => File.Exists(value.AssemblyPath));
            result.DllAssemblies.Should().BeEmpty();
            if (kind == "GQI")
            {
                document.Descendants(AutomationNamespace + "Param").Single(value => (string?)value.Attribute("type") == "preCompile").Value.Should().Be("true");
                document.Descendants(AutomationNamespace + "Param").Single(value => (string?)value.Attribute("type") == "libraryName").Value.Should().Be(project.AssemblyName);
                document.Descendants(AutomationNamespace + "Value").Single().Value.Should().Contain("IGQIDataSource");
            }
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task BuildAsync_SolutionRunner_UnifiesDependenciesFromAllHarvestedLibraries(bool populatedCache)
        {
            using var fixture = new BuildOnlyPackageFixture();
            if (populatedCache)
            {
                await fixture.PopulateCacheAsync();
            }

            string oldLibrary = CreateLibrary(fixture, "Older", "net48", new[]
            {
                new PackageReference(BuildOnlyPackageFixture.PackageId, "1.0.0"),
            });
            string selectedLibrary = CreateLibrary(fixture, "Selected", "net48", new[]
            {
                new PackageReference(BuildOnlyPackageFixture.PackageId, BuildOnlyPackageFixture.SelectedVersion),
            });
            var first = CreateScriptProject(fixture, "First", new[] { oldLibrary }, Array.Empty<PackageReference>());
            var second = CreateScriptProject(fixture, "Second", new[] { selectedLibrary }, Array.Empty<PackageReference>());
            var projects = new[] { first, second };
            var scripts = projects.Select(project => CreateScript(project.AssemblyName)).ToArray();

            foreach (var project in projects)
            {
                var script = scripts.Single(value => value.Name == project.AssemblyName);
                var builder = new AutomationScriptBuilder("HarvestingSolution", script,
                    new Dictionary<string, Project> { [project.AssemblyName] = project }, projects, scripts, fixture.RootPath);

                var result = await builder.BuildAsync().ConfigureAwait(false);

                var document = XDocument.Parse(result.Document);
                document.Root!.Element(AutomationNamespace + "SolutionId")!.Value.Should().Be("HarvestingSolution");
                GetReferences(document).Should().NotContain(value => value.IndexOf(BuildOnlyPackageFixture.PackageId, StringComparison.OrdinalIgnoreCase) >= 0);
                result.Assemblies.Should().HaveCount(2);
                result.Assemblies.Should().ContainSingle(value => value.DllImport.EndsWith("Fixture.Runtime.dll", StringComparison.OrdinalIgnoreCase));
                result.Assemblies.Should().ContainSingle(value => value.DllImport.EndsWith(project == first ? "Older.dll" : "Selected.dll", StringComparison.OrdinalIgnoreCase));
            }
        }

        [TestMethod]
        public async Task BuildAsync_MissingHarvestedOutput_DoesNotSilentlyDropRequiredLibrary()
        {
            using var fixture = new BuildOnlyPackageFixture();
            string library = CreateLibrary(fixture, "Missing", "net48", Array.Empty<PackageReference>());
            File.Delete(Path.Combine(Path.GetDirectoryName(library)!, "bin", "Debug", "net48", "Missing.dll"));
            var project = CreateScriptProject(fixture, "MissingOutput", new[] { library }, Array.Empty<PackageReference>());
            Script script = CreateScript(project.AssemblyName);
            var builder = new AutomationScriptBuilder(script,
                new Dictionary<string, Project> { [project.AssemblyName] = project }, new[] { script }, fixture.RootPath);

            Func<Task> build = async () => await builder.BuildAsync();

            await build.Should().ThrowAsync<FileNotFoundException>().WithMessage("*Missing.dll*");
        }

        [TestMethod]
        public async Task BuildAsync_ReleaseProject_UsesReleaseHarvestedOutput()
        {
            using var fixture = new BuildOnlyPackageFixture();
            string library = CreateLibrary(fixture, "ReleaseLibrary", "net48", Array.Empty<PackageReference>());
            string directory = Path.GetDirectoryName(library)!;
            string release = Path.Combine(directory, "bin", "Release", "net48", "ReleaseLibrary.dll");
            Directory.CreateDirectory(Path.GetDirectoryName(release)!);
            File.Move(Path.Combine(directory, "bin", "Debug", "net48", "ReleaseLibrary.dll"), release);
            string scriptDirectory = Path.Combine(fixture.RootPath, "ReleaseScript");
            Directory.CreateDirectory(scriptDirectory);
            string projectFile = Path.Combine(scriptDirectory, "ReleaseScript.csproj");
            File.WriteAllText(Path.Combine(scriptDirectory, "Script.cs"), "public sealed class Script { }");
            new XDocument(new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net48"), new XElement("Configuration", "Release"),
                    new XElement("DataMinerType", "AutomationScript")),
                new XElement("ItemGroup", new XElement("ProjectReference", new XAttribute("Include", library)))))
                .Save(projectFile);
            var project = Project.Load(projectFile);
            Script script = CreateScript(project.AssemblyName);
            var builder = new AutomationScriptBuilder(script,
                new Dictionary<string, Project> { [project.AssemblyName] = project }, new[] { script }, fixture.RootPath);

            var result = await builder.BuildAsync().ConfigureAwait(false);

            result.Assemblies.Should().ContainSingle(value => value.AssemblyPath == release);
            GetReferences(XDocument.Parse(result.Document)).Should().ContainSingle(value => value.EndsWith(@"\ReleaseLibrary.dll", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        [Timeout(30000)]
        public async Task BuildAsync_CyclicProjectReferences_TerminatesAndRetainsEachLibraryOnce()
        {
            using var fixture = new BuildOnlyPackageFixture();
            string first = CreateLibrary(fixture, "CycleFirst", "net48", Array.Empty<PackageReference>());
            string second = CreateLibrary(fixture, "CycleSecond", "net48", Array.Empty<PackageReference>(), first);
            var xml = XDocument.Load(first);
            xml.Root!.Add(new XElement("ItemGroup", new XElement("ProjectReference", new XAttribute("Include", second))));
            xml.Save(first);
            var project = CreateScriptProject(fixture, "Cycle", new[] { first }, Array.Empty<PackageReference>());
            Script script = CreateScript(project.AssemblyName);
            var builder = new AutomationScriptBuilder(script,
                new Dictionary<string, Project> { [project.AssemblyName] = project }, new[] { script }, fixture.RootPath);

            var result = await builder.BuildAsync().ConfigureAwait(false);

            result.Assemblies.Should().HaveCount(2);
            GetReferences(XDocument.Parse(result.Document)).Should().HaveCount(2);
        }

        [TestMethod]
        [DataRow("net10.0")]
        [DataRow("net10.0;net9.0")]
        public async Task BuildAsync_IncompatibleLibraryFramework_FailsExplicitly(string frameworks)
        {
            using var fixture = new BuildOnlyPackageFixture();
            string library = CreateLibrary(fixture, "Incompatible", frameworks, Array.Empty<PackageReference>());
            var project = CreateScriptProject(fixture, "IncompatibleScript", new[] { library }, Array.Empty<PackageReference>());
            Script script = CreateScript(project.AssemblyName);
            var builder = new AutomationScriptBuilder(script,
                new Dictionary<string, Project> { [project.AssemblyName] = project }, new[] { script }, fixture.RootPath);

            Func<Task> build = async () => await builder.BuildAsync();

            await build.Should().ThrowAsync<InvalidOperationException>().WithMessage("*compatible*");
        }

        [TestMethod]
        public async Task BuildAsync_LegacyLibrary_PreservesDllAndDependencies()
        {
            using var fixture = new BuildOnlyPackageFixture();
            string library = CreateLibrary(fixture, "LegacyLibrary", "net48",
                new[] { new PackageReference("Fixture.Left", "1.0.0") });
            var xml = XDocument.Load(library);
            xml.Root!.Attribute("Sdk")!.Remove();
            var properties = xml.Root.Element("PropertyGroup")!;
            properties.Element("TargetFramework")!.Remove();
            properties.Add(new XElement("TargetFrameworkVersion", "v4.8"),
                new XElement("Configuration", new XAttribute("Condition", "'$(Configuration)' == ''"), "Debug"),
                new XElement("OutputPath", @"bin\$(Configuration)\net48\"));
            xml.Root.Add(new XElement("ItemGroup", new XElement("Compile", new XAttribute("Include", "LegacyLibrary.cs"))),
                new XElement("Import", new XAttribute("Project", @"$(MSBuildToolsPath)\Microsoft.CSharp.targets")));
            xml.Save(library);
            var project = CreateScriptProject(fixture, "LegacyScript", new[] { library },
                new[] { new PackageReference(BuildOnlyPackageFixture.PackageId, BuildOnlyPackageFixture.SelectedVersion) });
            Script script = CreateScript(project.AssemblyName);
            var builder = new AutomationScriptBuilder(script,
                new Dictionary<string, Project> { [project.AssemblyName] = project }, new[] { script }, fixture.RootPath);

            var result = await builder.BuildAsync();

            result.Assemblies.Should().ContainSingle(value => value.DllImport.Replace('\\', '/') ==
                "fixture.library.legacylibrary/1.2.3.4/lib/net48/LegacyLibrary.dll");
            result.Assemblies.Should().ContainSingle(value => value.DllImport.EndsWith("Fixture.Left.dll", StringComparison.OrdinalIgnoreCase));
            result.Assemblies.Should().ContainSingle(value => value.DllImport.EndsWith("Fixture.Runtime.dll", StringComparison.OrdinalIgnoreCase));
            GetReferences(XDocument.Parse(result.Document)).Should().NotContain(value =>
                value.IndexOf(BuildOnlyPackageFixture.PackageId, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static Project CreateScriptProject(BuildOnlyPackageFixture fixture, string name, string[] libraries, PackageReference[] packages, bool gqi = false)
        {
            string source = gqi
                ? "using Skyline.DataMiner.Analytics.GenericInterface; public sealed class Source : IGQIDataSource { public GQIColumn[] GetColumns() { return new GQIColumn[0]; } public GQIPage GetNextPage(GetNextPageInputArgs args) { return new GQIPage(new GQIRow[0]); } }"
                : "using Skyline.DataMiner.Automation; public sealed class Script { public void Run(IEngine engine) { } }";
            return new Project(name, path: Path.Combine(fixture.RootPath, name + ".csproj"),
                tfm: BuildOnlyPackageFixture.TargetFrameworkMoniker,
                projectFiles: new[] { new ProjectFile("Script.cs", source) },
                packageReferences: packages,
                projectReferences: libraries.Select(path => new ProjectReference(Path.GetFileNameWithoutExtension(path), path, path)).ToArray());
        }

        private static Script CreateScript(string name, bool gqi = false)
        {
            var exe = new XElement(AutomationNamespace + "Exe", new XAttribute("id", "1"), new XAttribute("type", "csharp"),
                new XElement(AutomationNamespace + "Value", new XCData("[Project:" + name + "]")));
            if (gqi)
            {
                exe.Add(new XElement(AutomationNamespace + "Param", new XAttribute("type", "preCompile"), "true"));
                exe.Add(new XElement(AutomationNamespace + "Param", new XAttribute("type", "libraryName"), name));
            }

            var document = new XDocument(new XElement(AutomationNamespace + "DMSScript",
                new XElement(AutomationNamespace + "Name", name), new XElement(AutomationNamespace + "Script", exe)));
            return new Script(XmlDocument.Parse(document.ToString(SaveOptions.DisableFormatting)));
        }

        private static string[] GetReferences(XDocument document)
        {
            return document.Descendants(AutomationNamespace + "Param")
                .Where(value => (string?)value.Attribute("type") == "ref").Select(value => value.Value).ToArray();
        }

        private static string CreateLibrary(BuildOnlyPackageFixture fixture, string name, string frameworks, PackageReference[] packages, params string[] references)
        {
            string directory = Path.Combine(fixture.RootPath, name);
            Directory.CreateDirectory(directory);
            var properties = new XElement("PropertyGroup",
                new XElement(frameworks.Contains(";") ? "TargetFrameworks" : "TargetFramework", frameworks),
                new XElement("Configuration", "Debug"),
                new XElement("AssemblyName", name), new XElement("PackageId", "Fixture.Library." + name),
                new XElement("OutputType", "Library"), new XElement("IsPackable", "false"));
            var project = new XDocument(new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"), properties,
                new XElement("ItemGroup", packages.Select(package => new XElement("PackageReference",
                    new XAttribute("Include", package.Name), new XAttribute("Version", package.Version)))),
                new XElement("ItemGroup", references.Select(reference => new XElement("ProjectReference", new XAttribute("Include", reference))))));
            string path = Path.Combine(directory, name + ".csproj");
            project.Save(path);
            string source = "[assembly:System.Reflection.AssemblyVersion(\"1.2.3.4\")] public sealed class " + name + " { }";
            File.WriteAllText(Path.Combine(directory, name + ".cs"), source);
            string frameworkReference = Path.Combine(AppContext.BaseDirectory, "ReferenceAssemblies", "net48", "mscorlib.dll");
            File.Exists(frameworkReference).Should().BeTrue("the restored Framework reference pack must be copied to each test host");
            var compilation = CSharpCompilation.Create(name, new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(frameworkReference) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            foreach (string framework in frameworks.Split(';'))
            {
                string output = Path.Combine(directory, "bin", "Debug", framework, name + ".dll");
                Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                using var stream = File.Create(output);
                var emitted = compilation.Emit(stream);
                emitted.Success.Should().BeTrue("library compilation diagnostics: {0}", String.Join(Environment.NewLine, emitted.Diagnostics));
            }

            return path;
        }
    }
}
