using System.Text.RegularExpressions;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Skyline.DataMiner.CICD.DMApp.Common;
using Skyline.DataMiner.CICD.DMApp.Keystone;
using Skyline.DataMiner.CICD.FileSystem;

namespace Skyline.DataMiner.CICD.DMApp.Keystone.Tests
{
    [TestClass()]
    public class UserExecutableTests
    {

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            // sets up the test framework with "local tools" support.
            var dotnet = DotnetFactory.Create();
            dotnet.Run("new tool-manifest", out _, out _);
        }

        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            // remove tool manifest.
            FileSystem.FileSystem.Instance.Directory.DeleteDirectory(".config");
        }

        [TestMethod()]
        public void WrapIntoDotnetToolTest_TestReturnPath()
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


            ToolMetaData toolMetaData = new ToolMetaData("MyCommand", "Skyline.DataMiner.Keystone.MyCommand", "2.0.1", "SkylineCommunications", "Skyline Communications");

            // Act
            UserExecutable executable = new UserExecutable();
            var result = executable.WrapIntoDotnetTool(fs.Object, outputPath, dotnet.Object, pathToUserExecutableDir, toolMetaData);

            // Assert
            result.Should().Be("TempDir/fakedir/somewhere/ubuntu/Skyline.DataMiner.Keystone.MyCommand.2.0.1.nupkg");
        }

        [DataTestMethod]
        [DataRow("Net6")]
        [DataRow("NetFramework")]
        [DataRow("Rust")]
        [DataRow("Go")]
        public void WrapIntoDotnetToolTest_Integration_Core(string frameworkIdentifier)
        {
            // Arrange
            var fs = FileSystem.FileSystem.Instance;
            var dotnet = DotnetFactory.Create();

            // BUG: cannot use --no-cache. always need different version or name. Cannot uninstall & reinstall same version with different content
            // https://github.com/dotnet/sdk/issues/34508

            string uniqueShort = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            string nameOfTool = $"Skyline.DataMiner.Keystone.Test.{frameworkIdentifier}.{uniqueShort}";
            string commandOfTool = "Net6IntegrationTest";

            string pathToUserExecutableDir = $"TestData/{frameworkIdentifier}Program";
            ToolMetaData toolMetaData = new ToolMetaData(commandOfTool, nameOfTool, "1.0.1", "SkylineCommunications", "Skyline Communications");

            var tempDir = fs.Directory.CreateTemporaryDirectory();

            // Act
            try
            {
                UserExecutable executable = new UserExecutable();
                var result = executable.WrapIntoDotnetTool(fs, tempDir, dotnet, pathToUserExecutableDir, toolMetaData);

                // Assert
                fs.File.Exists(result).Should().BeTrue();
                fs.Path.GetExtension(result).Should().Be(".nupkg");

                // Test installing this


                string outputInstall, errorsInstall;
                dotnet.Run($"tool install {nameOfTool} --add-source {tempDir} --no-cache", out outputInstall, out errorsInstall);

                Console.WriteLine("---------");
                Console.WriteLine("install out: " + outputInstall);
                Console.WriteLine("install err: " + errorsInstall);
                Console.WriteLine("---------");
                try
                {
                    // Test running this
                    string outputRun, errorsRun;
                    dotnet.Run($"tool run {commandOfTool} from{frameworkIdentifier}", out outputRun, out errorsRun);

                    Console.WriteLine("---------");
                    Console.WriteLine("run out: " + outputRun);
                    Console.WriteLine("run err: " + errorsRun);
                    Console.WriteLine("---------");

                    if (!String.IsNullOrWhiteSpace(errorsRun))
                    {
                        Assert.Fail(errorsRun);
                    }

                    outputRun.Trim().Should().Be($"Hello World\r\nfrom{frameworkIdentifier}");
                }
                finally
                {
                    // test uninstalling this
                    string outputUninstall, errorsUninstall;
                    dotnet.Run($"tool uninstall {nameOfTool}", out outputUninstall, out errorsUninstall);

                    Console.WriteLine("---------");
                    Console.WriteLine("uninstall out: " + outputUninstall);
                    Console.WriteLine("uninstall err: " + errorsUninstall);
                    Console.WriteLine("---------");
                }
            }
            finally
            {
                fs.Directory.DeleteDirectory(tempDir);
            }
        }


    }
}