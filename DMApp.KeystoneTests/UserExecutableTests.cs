namespace Skyline.DataMiner.CICD.DMApp.Keystone.Tests
{
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.FileSystem;

    [TestClass()]
    public class UserExecutableTests
    {
        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            // remove tool manifest.
            FileSystem.Instance.Directory.DeleteDirectory(".config");
        }

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            // sets up the test framework with "local tools" support.

            DataReceivedEventHandler onOutput = (sender, e) =>
            {
            };

            DataReceivedEventHandler onError = (sender, e) =>
            {
            };

            var dotnet = DotnetFactory.Create(onOutput, onError);
            dotnet.Run("new tool-manifest", true);
        }

        [DataTestMethod]
        [DataRow("Net6")]
        [DataRow("NetFramework")]
        [DataRow("Rust")]
        [DataRow("Go")]
        [DataRow("WithUI")]
        public void WrapIntoDotnetToolTest_Integration_Core(string frameworkIdentifier)
        {
            // Arrange
            var fs = FileSystem.Instance;

            DataReceivedEventHandler onOutput = (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine(e.Data);
                }
            };

            DataReceivedEventHandler onError = (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.Error.WriteLine(e.Data); // Write the error data to the console
                }
            };

            var dotnet = DotnetFactory.Create(onOutput, onError);

            // BUG: cannot use --no-cache. always need different version or name. Cannot uninstall & reinstall same version with different content
            // https://github.com/dotnet/sdk/issues/34508

            string uniqueShort = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            string nameOfTool = $"Skyline.DataMiner.Keystone.Test.{frameworkIdentifier}.{uniqueShort}";
            string commandOfTool = "Net6IntegrationTest";

            string pathToUserExecutableDir = $"TestData/{frameworkIdentifier}Program";
            var tempDir = fs.Directory.CreateTemporaryDirectory();

            ToolMetaData toolMetaData = new ToolMetaData(commandOfTool, nameOfTool, "1.0.1", "SkylineCommunications", "Skyline Communications", tempDir);


            // Act
            try
            {
                UserExecutable executable = new UserExecutable();
                var result = executable.WrapIntoDotnetTool(fs, dotnet, pathToUserExecutableDir, toolMetaData);

                // Assert
                fs.File.Exists(result).Should().BeTrue();
                fs.Path.GetExtension(result).Should().Be(".nupkg");

                // Test installing this

                dotnet.Run($"tool install {nameOfTool} --add-source {tempDir} --no-cache");

                try
                {
                    // Test running this
                   var runResult = dotnet.Run($"tool run {commandOfTool} from{frameworkIdentifier}");

                    if (!String.IsNullOrWhiteSpace(runResult.errors))
                    {
                        Assert.Fail(runResult.errors);
                    }

                    runResult.output.Trim().Should().Be($"Hello World\r\nfrom{frameworkIdentifier}");
                }
                finally
                {
                    // test uninstalling this
                    dotnet.Run($"tool uninstall {nameOfTool}");
                }
            }
            finally
            {
                fs.Directory.DeleteDirectory(tempDir);
            }
        }

        [TestMethod()]
        public void WrapIntoDotnetToolTest_TestReturnPath_AllProvided()
        {
            // Arrange

            Mock<IFileSystem> fs = new Mock<IFileSystem>();
            Mock<IDotnet> dotnet = new Mock<IDotnet>();
            Mock<IDirectoryIO> directory = new Mock<IDirectoryIO>();
            Mock<IFileIO> file = new Mock<IFileIO>();
            Mock<IPathIO> path = new Mock<IPathIO>();
            fs.Setup(f => f.Directory).Returns(directory.Object);
            fs.Setup(f => f.File).Returns(file.Object);
            fs.Setup(f => f.Path).Returns(path.Object);

            string fullCommandName = "Skyline.DataMiner.Keystone.MyCommand";

            string pathToUserExecutableDir = "fakedir/somewhere/ubuntu";
            directory.Setup(d => d.IsDirectory(pathToUserExecutableDir)).Returns(true);
            directory.Setup(d => d.EnumerateFiles(pathToUserExecutableDir, "*.exe")).Returns(new[] { "MyCustomProgram.exe" });
            string outputPath = "TempDir";
            path.Setup(p => p.Combine(outputPath, $"{fullCommandName}.2.0.1.nupkg")).Returns($"{outputPath}/fakedir/somewhere/ubuntu/Skyline.DataMiner.Keystone.MyCommand.2.0.1.nupkg");

            ToolMetaData toolMetaData = new ToolMetaData("MyCommand", "Skyline.DataMiner.Keystone.MyCommand", "2.0.1", "SkylineCommunications", "Skyline Communications", outputPath);

            // Act
            UserExecutable executable = new UserExecutable();
            var result = executable.WrapIntoDotnetTool(fs.Object, dotnet.Object, pathToUserExecutableDir, toolMetaData);

            // Assert
            result.Should().Be("TempDir/fakedir/somewhere/ubuntu/Skyline.DataMiner.Keystone.MyCommand.2.0.1.nupkg");
        }

        [TestMethod()]
        public void WrapIntoDotnetToolTest_TestReturnPath_Default()
        {
            // Arrange

            Mock<IFileSystem> fs = new Mock<IFileSystem>();
            Mock<IDotnet> dotnet = new Mock<IDotnet>();
            Mock<IDirectoryIO> directory = new Mock<IDirectoryIO>();
            Mock<IFileIO> file = new Mock<IFileIO>();
            Mock<IPathIO> path = new Mock<IPathIO>();
            fs.Setup(f => f.Directory).Returns(directory.Object);
            fs.Setup(f => f.File).Returns(file.Object);
            fs.Setup(f => f.Path).Returns(path.Object);

            string fullCommandName = "Skyline.DataMiner.Keystone.MyCustomProgram";

            string pathToUserExecutableDir = "fakedir/somewhere/ubuntu";
            directory.Setup(d => d.IsDirectory(pathToUserExecutableDir)).Returns(true);
            directory.Setup(d => d.EnumerateFiles(pathToUserExecutableDir, "*.exe")).Returns(new[] { "MyCustomProgram.exe" });
            string outputPath = "TempDir";
            path.Setup(p => p.Combine(outputPath, $"{fullCommandName}.1.0.0.nupkg")).Returns($"{outputPath}/fakedir/somewhere/ubuntu/Skyline.DataMiner.Keystone.MyCustomProgram.1.0.0.nupkg");

            path.Setup(p => p.GetFileNameWithoutExtension("MyCustomProgram.exe")).Returns("MyCustomProgram");

            file.Setup(p => p.GetFileProductVersion("MyCustomProgram.exe")).Returns("1.0.0");

            ToolMetaData toolMetaData = new ToolMetaData(
                "",
                "",
                "",
                "",
                "",
                outputPath);

            // Act
            UserExecutable executable = new UserExecutable();
            var result = executable.WrapIntoDotnetTool(fs.Object, dotnet.Object, pathToUserExecutableDir, toolMetaData);

            // Assert
            result.Should().Be("TempDir/fakedir/somewhere/ubuntu/Skyline.DataMiner.Keystone.MyCustomProgram.1.0.0.nupkg");
        }
    }
}