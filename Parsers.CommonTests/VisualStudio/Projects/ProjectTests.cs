#pragma warning disable CS0618 // Type or member is obsolete
namespace Parsers.CommonTests.VisualStudio.Projects
{
    using System.Linq;
    using System.Reflection;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;

    [TestClass]
    public class ProjectTests
    {
        [TestMethod]
        public void Load_GeneralValid()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting"));
            var path = FileSystem.Instance.Path.Combine(dir, "Basic.csproj");

            // Act
            var result = Project.Load(path, "Basic");

            // Assert
            result.Should().NotBeNull();
            result.AssemblyName.Should().BeEquivalentTo("Basic2");
            result.ProjectName.Should().BeEquivalentTo("Basic");
            result.ProjectDirectory.Should().BeEquivalentTo(dir);
            result.DataMinerProjectType.Should().BeNull();
            result.Path.Should().BeEquivalentTo(path);
            result.References.Should().NotBeNullOrEmpty();
            result.PackageReferences.Should().NotBeNullOrEmpty();
            result.Files.Should().NotBeNullOrEmpty();
            result.ProjectReferences.Should().NotBeNullOrEmpty();
            result.TargetFrameworkMoniker.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void Load_GeneralValid_LoadViaPathOnly()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting"));
            var path = FileSystem.Instance.Path.Combine(dir, "Basic.csproj");

            // Act
            var result = Project.Load(path);

            // Assert
            result.Should().NotBeNull();
            result.AssemblyName.Should().BeEquivalentTo("Basic2");
            result.ProjectName.Should().BeEquivalentTo("Basic");
            result.ProjectDirectory.Should().BeEquivalentTo(dir);
            result.DataMinerProjectType.Should().BeNull();
            result.Path.Should().BeEquivalentTo(path);
            result.References.Should().NotBeNullOrEmpty();
            result.PackageReferences.Should().NotBeNullOrEmpty();
            result.Files.Should().NotBeNullOrEmpty();
            result.ProjectReferences.Should().NotBeNullOrEmpty();
            result.TargetFrameworkMoniker.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        [DataRow("TFM_Valid.csproj", ".NETFramework,Version=4.7")]
        [DataRow("TFM_InvalidVersion.csproj", ".NETFramework,Version=4.6.2")]
        [DataRow("TFM_NoConfiguration.csproj", ".NETFramework,Version=4.6.2")]
        [DataRow("TFM_NoVersion.csproj", ".NETFramework,Version=4.6.2")]
        [DataRow("TFM_NoVersion.csproj", ".NETFramework,Version=4.6.2")]
        [DataRow("SdkStyle.csproj", ".NETFramework,Version=v4.6.2")]
        public void Load_TargetFrameWorkMoniker(string fileName, string expectedResult)
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\TFM"));
            var path = FileSystem.Instance.Path.Combine(dir, fileName);

            // Act
            var result = Project.Load(path, "name");

            // Assert
            result.TargetFrameworkMoniker.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        [DataRow("Files_Valid.csproj", 2)]
        [DataRow("Files_NoFiles.csproj", 0)]
        [DataRow("SharedProject.projitems", 2)]
        [DataRow("SharedProject.shproj", 2)]
        [DataRow("Files_ValidSharedProject.csproj", 4)]
        [DataRow("Files_ValidSharedProject.csproj", 4)]
        [DataRow(@"SdkStyle\SdkStyle.csproj", 2)]
        public void Load_Files_Amount(string fileName, int expectedResult)
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\Files"));
            var path = FileSystem.Instance.Path.Combine(dir, fileName);

            // Act
            var result = Project.Load(path, "name");

            // Assert
            result.Files.Should().HaveCount(expectedResult);
        }

        [TestMethod]
        [DataRow("References_Valid.csproj", 8)]
        [DataRow("References_NoReferences.csproj", 0)]
        [DataRow("References_Single.csproj", 1)]
        [DataRow("SdkStyle.csproj", 1)]
        public void Load_References_Amount(string fileName, int expectedResult)
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\References"));
            var path = FileSystem.Instance.Path.Combine(dir, fileName);

            // Act
            var result = Project.Load(path, "name");

            // Assert
            result.References.Should().HaveCount(expectedResult);
        }

        [TestMethod]
        [DataRow("References_Single.csproj", "MyReference", "..\\MyHintPath.dll")]
        [DataRow("References_Single_Update.csproj", "MyReference", "..\\MyOtherHintPath.dll")]
        [DataRow("References_Single_NoOverride.csproj", "MyReference", "..\\MyHintPath.dll")]
        [DataRow("SdkStyle.csproj", "MyCustomDll", "..\\Dlls\\MyCustomDll.dll")]

        public void Load_References_SingleReference(string fileName, string expectedName, string expectedHintPath)
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\References"));
            var path = FileSystem.Instance.Path.Combine(dir, fileName);

            // Act
            var result = Project.Load(path, "name");

            // Assert
            result.References.Should().HaveCount(1);

            var reference = result.References.First();

            reference.Name.Should().BeEquivalentTo(expectedName);
            reference.HintPath.Should().BeEquivalentTo(expectedHintPath);
        }

        [TestMethod]
        [DataRow("ProjectReferences_Valid.csproj", 2)]
        [DataRow("ProjectReferences_NoProjectReferences.csproj", 0)]
        [DataRow("ProjectReferences_Single.csproj", 1)]
        [DataRow("SdkStyle.csproj", 1)]

        public void Load_ProjectReferences_Amount(string fileName, int expectedResult)
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\ProjectReferences"));
            var path = FileSystem.Instance.Path.Combine(dir, fileName);

            // Act
            var result = Project.Load(path, "name");

            // Assert
            result.ProjectReferences.Should().HaveCount(expectedResult);
        }

        [TestMethod]
        [DataRow("ProjectReferences_Single.csproj", "MyFirstProjectReference", "..\\SubFolder\\MyProjectReference.csproj", "{798B58BA-BAB8-4C52-8C48-1A8AF2B5CCCA}")]
        [DataRow("ProjectReferences_Single_NoOverride.csproj", "MyFirstProjectReference", "..\\SubFolder\\MyProjectReference.csproj", "{798B58BA-BAB8-4C52-8C48-1A8AF2B5CCCA}")]
        [DataRow("SdkStyle.csproj", "AutomationScript_ClassLibrary", "..\\AutomationScript_ClassLibrary\\AutomationScript_ClassLibrary.csproj", "00000000-0000-0000-0000-000000000000")]
        public void Load_ProjectReferences_SingleReference(string fileName, string expectedName, string expectedPath, string expectedGuid)
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\ProjectReferences"));
            var path = FileSystem.Instance.Path.Combine(dir, fileName);

            // Act
            var result = Project.Load(path, "name");

            // Assert
            result.ProjectReferences.Should().HaveCount(1);

            var projectReference = result.ProjectReferences.First();

            projectReference.Name.Should().BeEquivalentTo(expectedName);
            projectReference.Path.Should().BeEquivalentTo(expectedPath);
            projectReference.Guid.Should().BeEquivalentTo(expectedGuid);
        }

        [TestMethod]
        [DataRow("PackageReferences_Valid.csproj", 2)]
        [DataRow("PackagesConfig\\PackageReferences_Valid_PackagesConfig.csproj", 1)]
        [DataRow("PackageReferences_NoPackageReferences.csproj", 0)]
        [DataRow("PackageReferences_Single.csproj", 1)]
        [DataRow("PackageReferences_Single_CLI.csproj", 1)]
        [DataRow("SdkStyle.csproj", 2)]
        public void Load_PackageReferences_Amount(string fileName, int expectedResult)
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\PackageReferences"));
            var path = FileSystem.Instance.Path.Combine(dir, fileName);

            // Act
            var result = Project.Load(path, "name");

            // Assert
            result.PackageReferences.Should().HaveCount(expectedResult);
        }

        [TestMethod]
        [DataRow("PackageReferences_Single.csproj", "StyleCop.Analyzers", "1.1.118")]
        [DataRow("PackageReferences_Single_Update.csproj", "StyleCop.Analyzers", "1.2.0-beta.507")]
        [DataRow("PackageReferences_Single_CLI.csproj", "Skyline.DataMiner.Dev.Automation", "10.0.0.5")]
        public void Load_PackageReferences_SingleReference(string fileName, string expectedName, string expectedVersion)
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\PackageReferences"));
            var path = FileSystem.Instance.Path.Combine(dir, fileName);

            // Act
            var result = Project.Load(path, "name");

            // Assert
            result.PackageReferences.Should().HaveCount(1);

            var packageReference = result.PackageReferences.First();

            packageReference.Name.Should().BeEquivalentTo(expectedName);
            packageReference.Version.Should().BeEquivalentTo(expectedVersion);
        }

        [TestMethod]
        public void Load_SpecialCharsSupported()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\SpecialChar"));
            var path = FileSystem.Instance.Path.Combine(dir, "SpecialChar.csproj");

            // Act
            var result = Project.Load(path, "Basic");

            // Assert
            result.Should().NotBeNull();
            result.Files.Should().NotBeNull();
            result.Files.Should().HaveCount(1);

            var fileContent = result.Files.First().Content;

            fileContent.Should().Contain("engine.GenerateInformation(\"test ›\");");
        }

        [TestMethod]
        public void SLDisCompiler_ProjectFile_Load1()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Protocol\Solution1"));
            var path = FileSystem.Instance.Path.Combine(dir, "QAction_1", "QAction_1.csproj");

            var project = Project.Load(path, "QAction_1");

            Assert.IsInstanceOfType(project, typeof(Project));

            Assert.AreEqual(path, project.Path);
            Assert.AreEqual("QAction_1", project.AssemblyName);

            Assert.AreEqual(2, project.ProjectReferences.Count());

            var refHelper = project.ProjectReferences.FirstOrDefault(r => r.Name == "QAction_Helper");
            Assert.AreEqual(@"..\QAction_Helper\QAction_Helper.csproj", refHelper.Path);
            Assert.AreEqual(@"{31b1ef6a-2e94-4f70-9b05-f297ab3b6c69}", refHelper.Guid);

            CollectionAssert.AreEquivalent(new[] { "Newtonsoft.Json.dll", "System.dll", "System.Xml.dll" }, project.References.Select(r => r.GetDllName()).ToArray());
            CollectionAssert.AreEquivalent(new[] { "Class1.cs", "Properties\\AssemblyInfo.cs" }, project.Files.Select(f => f.Name).ToArray());
        }

        [TestMethod]
        public void SLDisCompiler_ProjectFile_Load2()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Protocol\Solution1"));
            var path = FileSystem.Instance.Path.Combine(dir, "QAction_2", "QAction_2.csproj");

            var project = Project.Load(path, "QAction_2");

            Assert.IsInstanceOfType(project, typeof(Project));

            Assert.AreEqual(path, project.Path);
            Assert.AreEqual("QAction_2", project.AssemblyName);

            Assert.AreEqual(3, project.ProjectReferences.Count());

            var refQA1 = project.ProjectReferences.FirstOrDefault(r => r.Name == "QAction_1");
            Assert.AreEqual(@"..\QAction_1\QAction_1.csproj", refQA1.Path);
            Assert.AreEqual(@"{20481214-4655-4c51-97aa-5da92296cbcf}", refQA1.Guid);

            var refHelper = project.ProjectReferences.FirstOrDefault(r => r.Name == "QAction_Helper");
            Assert.AreEqual(@"..\QAction_Helper\QAction_Helper.csproj", refHelper.Path);
            Assert.AreEqual(@"{31b1ef6a-2e94-4f70-9b05-f297ab3b6c69}", refHelper.Guid);

            CollectionAssert.AreEquivalent(new[] { "System.dll" }, project.References.Select(r => r.GetDllName()).ToArray());
            CollectionAssert.AreEquivalent(new[] { "QAction_2.cs" }, project.Files.Select(f => f.Name).ToArray());
        }

        [TestMethod]
        public void SLDisCompiler_ProjectFile_Load3()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Protocol\Solution1"));
            var path = FileSystem.Instance.Path.Combine(dir, "QAction_3", "QAction_3.csproj");

            var project = Project.Load(path, "QAction_3");

            Assert.IsInstanceOfType(project, typeof(Project));

            Assert.AreEqual(path, project.Path);
            Assert.AreEqual("QAction_3", project.AssemblyName);

            Assert.AreEqual(2, project.ProjectReferences.Count());

            var refHelper = project.ProjectReferences.FirstOrDefault(r => r.Name == "QAction_Helper");
            Assert.AreEqual(@"..\QAction_Helper\QAction_Helper.csproj", refHelper.Path);
            Assert.AreEqual(@"{31b1ef6a-2e94-4f70-9b05-f297ab3b6c69}", refHelper.Guid);

            CollectionAssert.AreEquivalent(new[] { "System.dll" }, project.References.Select(r => r.GetDllName()).ToArray());
            CollectionAssert.AreEquivalent(new[] { "QAction_3.cs", "Class1.cs", "SubDir\\Class2.cs" }, project.Files.Select(f => f.Name).ToArray());
        }
    }
}