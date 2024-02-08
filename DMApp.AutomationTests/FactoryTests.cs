#pragma warning disable SA1201
#pragma warning disable SA1203

namespace DMApp.AutomationTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.DMApp.Automation;
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.Loggers;

    [TestClass]
    public class FactoryTests
    {
        private static readonly LogCollector LogCollector = new LogCollector();
        private const string PackageName = "MyPackage";

        #region FromRepository

        private static readonly DMAppVersion PackageVersion = DMAppVersion.FromBuildNumber(1);

        [TestMethod]
        public async Task FromRepositoryReturnsCorrectPackageName()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            var creator = AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, PackageVersion);

            // Act.
            var package = await creator.BuildPackageAsync();

            // Assert.
            Assert.AreEqual(PackageName, package.Name);
        }

        [TestMethod]
        public async Task FromRepositoryReturnsCorrectPackageVersion()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            var creator = AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, PackageVersion);

            // Act.
            var package = await creator.BuildPackageAsync();

            // Assert.
            Assert.AreEqual("0.0.0-CU1", package.Version);
        }

        [TestMethod]
        public async Task FromRepositorySolution1ReturnsPackageWith2Scripts()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            var creator = AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, PackageVersion);

            // Act.
            var package = await creator.BuildPackageAsync();

            // Assert.
            Assert.AreEqual(2, package.AutomationScripts.Count);
            var scriptNames = package.AutomationScripts.Select(s => s.Name).ToList();
            scriptNames.Should().BeEquivalentTo(new List<string> { "Script_1", "Script_2" });
        }

        [TestMethod]
        public async Task FromRepositorySolution1WithSelectedScriptReturnsPackageWithSingleScript()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            var creator = AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, PackageVersion, new List<string> { "Script_1" });

            // Act.
            var package = await creator.BuildPackageAsync();

            // Assert.
            Assert.AreEqual(1, package.AutomationScripts.Count);
            var scriptNames = package.AutomationScripts.Select(s => s.Name).ToList();
            scriptNames.Should().Equal(new List<string> { "Script_1" });
        }

        [TestMethod]
        public async Task FromRepositorySolution4WithNuGetPackagesResultsInPackageWithAssembliesOfNuGetPackages()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution4"));

            var creator = AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, PackageVersion);

            // Act.
            var package = await creator.BuildPackageAsync();

            // Assert.
            var automationScript = package.AutomationScripts.FirstOrDefault();
            Assert.IsNotNull(automationScript);
            var includedAssemblies = automationScript.Assemblies.Select(a => a.DestinationFolderPath).ToList();

            var expectedAssemblies = new List<string>
            {
                @"C:\Skyline DataMiner\ProtocolScripts\DllImport\advancedstringbuilder\0.1.0\lib\net45",
                @"C:\Skyline DataMiner\ProtocolScripts\DllImport\newtonsoft.json\12.0.1\lib\net45",
                @"C:\Skyline DataMiner\ProtocolScripts\DllImport\newtonsoft.json.bson\1.0.2\lib\net45",
            };

            includedAssemblies.Should().BeEquivalentTo(expectedAssemblies);
        }

        [TestMethod]
        public async Task FromRepositorySolution3WithSharedProject()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution3"));

            var creator = AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, PackageVersion);

            // Act.
            var package = await creator.BuildPackageAsync();

            // Assert.
            var automationScript = package.AutomationScripts.FirstOrDefault(s => s.Name == "Script_1");

            Assert.IsNotNull(automationScript);

            string scriptContent = Encoding.UTF8.GetString(automationScript.ScriptContent);

            Assert.IsTrue(scriptContent.Contains("public bool TestSharedCall()"));
        }

        [TestMethod]
        public async Task FromRepositorySolution2WithCustomDllResultsInPackageWithCustomDllAssembly()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution2"));

            var creator = AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, PackageVersion);

            // Act.
            var package = await creator.BuildPackageAsync();

            // Assert.
            var automationScript = package.AutomationScripts.FirstOrDefault(a => a.Name == "Script_1");
            Assert.IsNotNull(automationScript);
            Assert.AreEqual(1, automationScript.Assemblies.Count);

            var includedAssembly = automationScript.Assemblies.FirstOrDefault();
            Assert.IsNotNull(includedAssembly);

            var fileName = Path.GetFileName(includedAssembly.AssemblyFilePath);

            Assert.AreEqual("MyCustomDll.dll", fileName);
        }

        #region Input validation

        [TestMethod]
        public void FromRepository_Default_ExpectedNoException()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, PackageVersion);

            // Assert.
            act.Should().NotThrow();
        }

        [TestMethod]
        public void FromRepository_NullLogCollector_ExpectedArgumentNullException()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromRepository(null, repositoryPath, PackageName, PackageVersion);

            // Assert.
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void FromRepository_NullRepositoryPath_ExpectedArgumentNullException()
        {
            // Arrange.

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, null, PackageName, PackageVersion);

            // Assert.
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void FromRepository_NullPackageName_ExpectedArgumentNullException()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, null, PackageVersion);

            // Assert.
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void FromRepository_NullPackageVersion_ExpectedArgumentNullException()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, null);

            // Assert.
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void FromRepository_NullScriptNames_ExpectedNoException()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, PackageVersion, null);

            // Assert.
            act.Should().NotThrow("default value is also null.");
        }

        [TestMethod]
        public void FromRepository_EmptyPackageName_ExpectedArgumentException()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, String.Empty, PackageVersion);

            // Assert.
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be empty or whitespace.*packageName*");
        }

        [TestMethod]
        public void FromRepository_WhitespacePackageName_ExpectedArgumentException()
        {
            // Arrange.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, "   ", PackageVersion);

            // Assert.
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be empty or whitespace.*packageName*");
        }

        #endregion

        #endregion

        #region FromSkylinePipeline

        private const string Tag = "1.0.0.1";
        private const int BuildNumber = 1;

        #region Input validation

        [TestMethod]
        public void FromSkylinePipeline_Default_ExpectedNoException()
        {
            // Arrange.

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromSkylinePipeline(LogCollector, "C:\\", PackageName, Tag, BuildNumber);

            // Assert.
            act.Should().NotThrow();
        }

        [TestMethod]
        public void FromSkylinePipeline_NullLogCollector_ExpectedArgumentNullException()
        {
            // Arrange.

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromSkylinePipeline(null, "C:\\", PackageName, Tag, BuildNumber);

            // Assert.
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void FromSkylinePipeline_NullWorkspace_ExpectedArgumentNullException()
        {
            // Arrange.

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromSkylinePipeline(LogCollector, null, PackageName, Tag, BuildNumber);

            // Assert.
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("ABC")]
        public void FromSkylinePipeline_InvalidWorkspace_ExpectedDirectoryNotFoundException(string workspace)
        {
            // Arrange.

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromSkylinePipeline(LogCollector, workspace, PackageName, Tag, BuildNumber);

            // Assert.
            act.Should().ThrowExactly<DirectoryNotFoundException>();
        }

        [TestMethod]
        public void FromSkylinePipeline_NullPackageName_ExpectedArgumentNullException()
        {
            // Arrange.

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromSkylinePipeline(LogCollector, "C:\\", null, Tag, BuildNumber);

            // Assert.
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void FromSkylinePipeline_EmptyPackageName_ExpectedArgumentException()
        {
            // Arrange.

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromSkylinePipeline(LogCollector, "C:\\", String.Empty, Tag, BuildNumber);

            // Assert.
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be empty or whitespace.*packageName*");
        }

        [TestMethod]
        public void FromSkylinePipeline_WhitespacePackageName_ExpectedArgumentException()
        {
            // Arrange.

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromSkylinePipeline(LogCollector, "C:\\", "   ", Tag, BuildNumber);

            // Assert.
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be empty or whitespace.*packageName*");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("null")]
        public void FromSkylinePipeline_NullTag_ExpectedNoException(string tag)
        {
            // Arrange.

            // Act.
            Action act = () => AppPackageCreatorForAutomation.Factory.FromSkylinePipeline(LogCollector, "C:\\", PackageName, tag, BuildNumber);

            // Assert.
            act.Should().NotThrow("tag is being used to determine if release or not.");
        }

        #endregion

        #endregion

        #region Performance

        [TestMethod]
        public async Task FromRepository_Performance()
        {
            // Arrange.;
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\HugeSolution"));

            var creator = AppPackageCreatorForAutomation.Factory.FromRepository(LogCollector, repositoryPath, PackageName, PackageVersion);

            // Act.
            Stopwatch sw = Stopwatch.StartNew();
            await creator.BuildPackageAsync();
            sw.Stop();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Total time: " + sw.Elapsed);
            sb.AppendLine("LOGGING");
            sb.AppendLine(String.Join(Environment.NewLine, LogCollector.Logging));

            // Assert.
            Assert.IsTrue(sw.Elapsed < TimeSpan.FromMinutes(1), sb.ToString());
            ////Assert.Inconclusive(sb.ToString());
        }

        #endregion
    }
}