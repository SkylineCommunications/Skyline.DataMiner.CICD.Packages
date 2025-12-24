namespace Parsers.ProtocolTests.VisualStudio
{
    using System;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Protocol.VisualStudio;

    [TestClass]
    public class ProtocolSolutionTests
    {
        [TestMethod]
        public void ProtocolSolution_Solution_Load()
        {
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"TestFiles\Protocol\Solution1"));
            var path = FileSystem.Instance.Path.Combine(dir, "Protocol.sln");

            var solution = ProtocolSolution.Load(path);

            solution.Should().NotBeNull();
            solution.SolutionPath.Should().BeEquivalentTo(path);
            solution.SolutionDirectory.Should().BeEquivalentTo(FileSystem.Instance.Path.GetDirectoryName(path));

            solution.Projects.Should().HaveCount(5);
            solution.QActions.Should().HaveCount(5);

            var qa1 = solution.QActions.FirstOrDefault(q => q.Id == 1);
            qa1.Should().NotBeNull();
            qa1.Files.Should().HaveCount(1);
            qa1.Files[0].Code.Should().NotBeNullOrEmpty();
            qa1.DllImports.Should().BeEquivalentTo("Newtonsoft.Json.dll", "System.dll", "System.Xml.dll", "[ProtocolName].[ProtocolVersion].QAction.63000.dll");

            var qa2 = solution.QActions.FirstOrDefault(q => q.Id == 2);
            qa2.Should().NotBeNull();
            qa2.Files.Should().HaveCount(1);
            qa2.Files[0].Code.Should().NotBeNullOrEmpty();
            qa2.DllImports.Should().BeEquivalentTo("System.dll", "[ProtocolName].[ProtocolVersion].QAction.1.dll", "[ProtocolName].[ProtocolVersion].QAction.63000.dll");

            var qa3 = solution.QActions.FirstOrDefault(q => q.Id == 3);
            qa3.Should().NotBeNull();
            qa3.Files.Should().HaveCount(3);
            qa3.Files.All(x => !String.IsNullOrEmpty(x.Code)).Should().BeTrue();
            qa3.Files[0].Code.Should().NotBeNullOrEmpty();
            qa3.Files[1].Code.Should().NotBeNullOrEmpty();
            qa3.Files[2].Code.Should().NotBeNullOrEmpty();
            qa3.DllImports.Should().BeEquivalentTo("System.dll", "[ProtocolName].[ProtocolVersion].QAction.63000.dll");

            var qa4 = solution.QActions.FirstOrDefault(q => q.Id == 4);
            qa4.Should().NotBeNull();
            qa4.Files.All(x => !String.IsNullOrEmpty(x.Code)).Should().BeTrue();
            qa4.DllImports.Should().BeEmpty();

            var qa63000 = solution.QActions.FirstOrDefault(q => q.Id == 63000);
            qa63000.Should().NotBeNull();
            qa63000.Files.Should().HaveCount(1);
            qa63000.Files[0].Code.Should().NotBeNullOrEmpty();
            qa63000.DllImports.Should().BeEquivalentTo("System.dll");
        }
    }
}
