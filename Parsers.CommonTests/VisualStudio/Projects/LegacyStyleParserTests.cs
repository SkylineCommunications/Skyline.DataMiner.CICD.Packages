namespace Parsers.CommonTests.VisualStudio.Projects
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;

    [TestClass]
    public class LegacyStyleParserTests
    {
        [TestMethod]
        public void GetCompileFilesTest()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\ProjectsForTesting\Files"));
            var path = FileSystem.Instance.Path.Combine(dir, "Files_UnknownFile.csproj");

            var xmlContent = FileSystem.Instance.File.ReadAllText(path, Encoding.UTF8);
            var document = XDocument.Parse(xmlContent);
            LegacyStyleParser legacyStyleParser = new LegacyStyleParser(document, dir);

            // Act
            Action action = () => _ = legacyStyleParser.GetCompileFiles().ToList();
            
            // Assert
            action.Should().Throw<FileNotFoundException>();
        }
    }
}