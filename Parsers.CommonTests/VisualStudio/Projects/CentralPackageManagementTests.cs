namespace Parsers.CommonTests.VisualStudio.Projects
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;

    [TestClass]
    public class CentralPackageManagementTests
    {
        [TestMethod]
        public void Load_CPMProject_VersionsResolvedFromDirectoryPackagesProps()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\CentralPackageManagement"));
            var path = FileSystem.Instance.Path.Combine(dir, "CPMProject.csproj");

            // Act
            var result = Project.Load(path);

            // Assert
            result.Should().NotBeNull();
            result.AssemblyName.Should().BeEquivalentTo("CPMTestProject");
            result.PackageReferences.Should().NotBeNullOrEmpty();

            var newtonsoftRef = result.PackageReferences.FirstOrDefault(p => p.Name == "Newtonsoft.Json");
            newtonsoftRef.Should().NotBeNull();
            newtonsoftRef.Version.Should().BeEquivalentTo("13.0.3", "version should come from Directory.Packages.props");

            var systemTextJsonRef = result.PackageReferences.FirstOrDefault(p => p.Name == "System.Text.Json");
            systemTextJsonRef.Should().NotBeNull();
            systemTextJsonRef.Version.Should().BeEquivalentTo("8.0.0", "version should come from Directory.Packages.props");
        }

        [TestMethod]
        public void Load_CPMProjectWithOverride_VersionOverrideTakesPrecedence()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\CentralPackageManagement"));
            var path = FileSystem.Instance.Path.Combine(dir, "CPMProjectWithOverride.csproj");

            // Act
            var result = Project.Load(path);

            // Assert
            result.Should().NotBeNull();
            result.AssemblyName.Should().BeEquivalentTo("CPMTestProjectWithOverride");
            result.PackageReferences.Should().NotBeNullOrEmpty();

            var newtonsoftRef = result.PackageReferences.FirstOrDefault(p => p.Name == "Newtonsoft.Json");
            newtonsoftRef.Should().NotBeNull();
            newtonsoftRef.Version.Should().BeEquivalentTo("12.0.3", "VersionOverride should take precedence over Directory.Packages.props");

            var fluentAssertionsRef = result.PackageReferences.FirstOrDefault(p => p.Name == "FluentAssertions");
            fluentAssertionsRef.Should().NotBeNull();
            fluentAssertionsRef.Version.Should().BeEquivalentTo("6.12.0", "version should come from Directory.Packages.props when no override is specified");
        }

        [TestMethod]
        public void Load_ProjectWithoutCPM_WorksAsNormalWithExplicitVersions()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\PackageReferences"));
            var path = FileSystem.Instance.Path.Combine(dir, "PackageReferences_Valid.csproj");
            List<PackageReference> expectedReferences = new List<PackageReference>
            {
                new PackageReference("Skyline.DataMiner.Dev.Automation", "10.0.0.5"),
                new PackageReference("StyleCop.Analyzers", "1.2.0-beta.507")
            };

            // Act
            var result = Project.Load(path);

            // Assert
            result.Should().NotBeNull();
            result.PackageReferences.Should().NotBeNullOrEmpty();
            result.PackageReferences.Should().HaveCount(2);
            result.PackageReferences.Should().BeEquivalentTo(expectedReferences);
        }
    }
}
