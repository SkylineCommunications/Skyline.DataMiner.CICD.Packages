namespace Assemblers.ProtocolTests.DependencyResolution
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;
    using Skyline.DataMiner.CICD.Assemblers.Protocol.DependencyResolution;
    using Skyline.DataMiner.CICD.FileSystem;

    [TestClass]
    public class RequirementsMarkerFilterTests
    {
        private string _tempDirectory;

        [TestInitialize]
        public void TestInitialize()
        {
            _tempDirectory = FileSystem.Instance.Directory.CreateTemporaryDirectory();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (FileSystem.Instance.Directory.Exists(_tempDirectory))
            {
                FileSystem.Instance.Directory.DeleteDirectory(_tempDirectory);
            }
        }

        [TestMethod]
        public void CreateFilteredRequirementsFile_MarkerGatedDependency_OnlyKeptForMatchingPlatform()
        {
            // Arrange: a requirement that should only apply on Windows, and one that always applies.
            string requirementsFilePath = FileSystem.Instance.Path.Combine(_tempDirectory, "requirements.txt");
            FileSystem.Instance.File.WriteAllText(
                requirementsFilePath,
                "requests" + Environment.NewLine +
                "colorama; sys_platform == \"win32\"" + Environment.NewLine +
                "uvloop; sys_platform == \"linux\"");

            // Act
            string windowsFilteredPath = RequirementsMarkerFilter.CreateFilteredRequirementsFile(FileSystem.Instance, requirementsFilePath, "3.14", SupportedPlatform.Windows, _tempDirectory);
            string linuxFilteredPath = RequirementsMarkerFilter.CreateFilteredRequirementsFile(FileSystem.Instance, requirementsFilePath, "3.14", SupportedPlatform.Linux, _tempDirectory);

            // Assert
            string windowsContent = FileSystem.Instance.File.ReadAllText(windowsFilteredPath);
            windowsContent.Should().Contain("requests");
            windowsContent.Should().Contain("colorama");
            windowsContent.Should().NotContain("uvloop");

            string linuxContent = FileSystem.Instance.File.ReadAllText(linuxFilteredPath);
            linuxContent.Should().Contain("requests");
            linuxContent.Should().Contain("uvloop");
            linuxContent.Should().NotContain("colorama");
        }

        [TestMethod]
        public void CreateFilteredRequirementsFile_KeptRequirement_HasMarkerStripped()
        {
            // Arrange: pip would otherwise re-evaluate this marker against its own host environment, which can
            // disagree with the target platform being resolved for.
            string requirementsFilePath = FileSystem.Instance.Path.Combine(_tempDirectory, "requirements.txt");
            FileSystem.Instance.File.WriteAllText(requirementsFilePath, "colorama; sys_platform == \"win32\"");

            // Act
            string filteredPath = RequirementsMarkerFilter.CreateFilteredRequirementsFile(FileSystem.Instance, requirementsFilePath, "3.14", SupportedPlatform.Windows, _tempDirectory);

            // Assert
            string content = FileSystem.Instance.File.ReadAllText(filteredPath);
            content.Trim().Should().Be("colorama");
        }

        [TestMethod]
        public void CreateFilteredRequirementsFile_CommentsAndBlankLines_AreIgnored()
        {
            // Arrange
            string requirementsFilePath = FileSystem.Instance.Path.Combine(_tempDirectory, "requirements.txt");
            FileSystem.Instance.File.WriteAllText(
                requirementsFilePath,
                "# a comment" + Environment.NewLine +
                Environment.NewLine +
                "requests");

            // Act
            string filteredPath = RequirementsMarkerFilter.CreateFilteredRequirementsFile(FileSystem.Instance, requirementsFilePath, "3.14", SupportedPlatform.Windows, _tempDirectory);

            // Assert
            string content = FileSystem.Instance.File.ReadAllText(filteredPath);
            content.Trim().Should().Be("requests");
        }
    }
}
