namespace Assemblers.ProtocolTests
{
    using System;
    using System.Linq;
    using System.Reflection;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Protocol;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Common.Exceptions;

    [TestClass]
    public class ProtocolSolutionScriptsTests
    {
        [TestMethod]
        public void ProtocolSolution_Scripts_HappyPath()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, "TestFiles", "Protocol", "SolutionWithScripts"));
            var path = FileSystem.Instance.Path.Combine(dir, "Protocol.sln");

            // Act
            var solution = ProtocolSolution.Load(path);

            // Assert
            solution.Scripts.Should().HaveCount(1);

            var script = solution.Scripts.First();
            script.Id.Should().Be("sample-scripted-connector");
            script.DisplayName.Should().Be("Sample Scripted Connector");
            script.Guid.Should().Be(Guid.Parse("edc76df5-0d81-43a8-9b22-ffdd0b2fb2e2"));
            script.ProjectDirectory.Should().Be(FileSystem.Instance.Path.Combine(dir, "ScriptedConnector_1"));
            script.Manifest.Should().NotBeNull();
            script.Manifest.Project.Name.Should().Be("Sample Scripted Connector");
        }

        [TestMethod]
        public void ProtocolSolution_Scripts_NoScriptsDeclared_ReturnsEmptyCollection()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, "TestFiles", "Protocol", "Solution1"));
            var path = FileSystem.Instance.Path.Combine(dir, "Protocol.sln");

            // Act
            var solution = ProtocolSolution.Load(path);

            // Assert
            solution.Scripts.Should().BeEmpty();
        }

        [TestMethod]
        public void ProtocolSolution_Scripts_DeclaredScriptWithoutMatchingFolder_Throws()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, "TestFiles", "Protocol", "SolutionWithMissingScript"));
            var path = FileSystem.Instance.Path.Combine(dir, "Protocol.sln");

            // Act
            Action act = () => ProtocolSolution.Load(path);

            // Assert
            act.Should().Throw<ParserException>();
        }

        [TestMethod]
        public void ProtocolSolution_Scripts_DuplicateGuidDeclared_Throws()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, "TestFiles", "Protocol", "SolutionWithDuplicateScriptGuid"));
            var path = FileSystem.Instance.Path.Combine(dir, "Protocol.sln");

            // Act
            Action act = () => ProtocolSolution.Load(path);

            // Assert
            act.Should().Throw<ParserException>();
        }
    }
}
