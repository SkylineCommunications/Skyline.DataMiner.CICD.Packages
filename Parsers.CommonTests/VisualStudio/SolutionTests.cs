namespace Parsers.CommonTests.VisualStudio
{
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio;

    [TestClass]
    public class SolutionTests
    {
        [TestMethod]
        public void Solution_Load_Filter()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Solutions\SolutionWithFilter"));
            var path = FileSystem.Instance.Path.Combine(dir, "Filter1.slnf");

            // Act
            Solution solution = Solution.Load(path, true);

            // Assert
            solution.Should().NotBeNull();
            solution.Projects.Should().HaveCount(1);
            solution.Projects.First().Name.Should().Be("Project1");
        }

        [TestMethod]
        public void Solution_Load_Slnx()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Solutions\SolutionWithFilter"));
            var path = FileSystem.Instance.Path.Combine(dir, "SolutionWithFilter.slnx");

            // Act
            Solution solution = Solution.Load(path, true);

            // Assert
            solution.Should().NotBeNull();
            solution.Projects.Should().HaveCount(2);
        }

        [TestMethod]
        public void Solution_Load_Sln()
        {
            // Arrange
            var baseDir = FileSystem.Instance.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dir = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(baseDir, @"VisualStudio\TestFiles\Solutions\SolutionWithFilter"));
            var path = FileSystem.Instance.Path.Combine(dir, "SolutionWithFilter.sln");

            // Act
            Solution solution = Solution.Load(path, true);

            // Assert
            solution.Should().NotBeNull();
            solution.Projects.Should().HaveCount(2);
        }
    }
}