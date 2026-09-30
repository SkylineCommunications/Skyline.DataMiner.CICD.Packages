namespace DMProtocolTests
{
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.DMProtocol;
    using Skyline.DataMiner.CICD.Loggers;

    [TestClass]
    public class FactoryTests
    {
        [TestMethod]
        public async Task FromRepositoryAsyncTestAsync()
        {
            // Arrange
            LogCollector logCollector = new LogCollector();
            string repositoryPath = @"TestFiles\VisualStudio\Protocol";

            // Act
            var builder = await ProtocolPackageCreator.Factory.FromRepositoryAsync(logCollector, repositoryPath);
            string protocolXmlContent = Encoding.UTF8.GetString(builder.ProtocolContent, 0, builder.ProtocolContent.Length);

            // Assert
            Assert.AreEqual("ExampleProtocol", builder.Name);
            Assert.AreEqual("1.0.0.1", builder.Version);

            // Verify if shared project code is included.
            bool containsSharedProjectCode = protocolXmlContent.Contains("public class Utility");
            Assert.IsTrue(containsSharedProjectCode);

            // Verify if subfolder content is included.
            bool subfolderCsFileCode = protocolXmlContent.Contains("public class SubfolderClass");
            Assert.IsTrue(subfolderCsFileCode);

            Assert.HasCount(2, builder.Assemblies);

            // Verify NuGet assembly is included.
            var nugetAssembly = builder.Assemblies.FirstOrDefault(a => a.DestinationFolderPath.Equals("C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\advancedstringbuilder\\0.1.0\\lib\\net45"));
            Assert.IsNotNull(nugetAssembly);

            // Verify assembly from DLLs folder is included.
            Assert.IsNotNull(builder.Assemblies.FirstOrDefault(a => a.AssemblyFilePath.EndsWith("ClassLibX.dll")));

            // Verify if Trending template is present
            Assert.HasCount(1, builder.TrendTemplates);
        }

        [TestMethod]
        public async Task FromRepositoryAsyncTestOverrideVersionAsync()
        {
            // Arrange
            LogCollector logCollector = new LogCollector();
            string repositoryPath = @"TestFiles\VisualStudio\Protocol";

            // Act
            var builder = await ProtocolPackageCreator.Factory.FromRepositoryAsync(logCollector, repositoryPath, "1.0.0.1_DEV");
            string protocolXmlContent = Encoding.UTF8.GetString(builder.ProtocolContent, 0, builder.ProtocolContent.Length);

            // Assert
            Assert.AreEqual("ExampleProtocol", builder.Name);
            Assert.AreEqual("1.0.0.1_DEV", builder.Version);

            // Verify if shared project code is included.
            bool containsSharedProjectCode = protocolXmlContent.Contains("public class Utility");
            Assert.IsTrue(containsSharedProjectCode);

            // Verify if subfolder content is included.
            bool subfolderCsFileCode = protocolXmlContent.Contains("public class SubfolderClass");
            Assert.IsTrue(subfolderCsFileCode);

            Assert.HasCount(2, builder.Assemblies);

            // Verify NuGet assembly is included.
            var nugetAssembly = builder.Assemblies.FirstOrDefault(a => a.DestinationFolderPath.Equals("C:\\Skyline DataMiner\\ProtocolScripts\\DllImport\\advancedstringbuilder\\0.1.0\\lib\\net45"));
            Assert.IsNotNull(nugetAssembly);

            // Verify assembly from DLLs folder is included.
            Assert.IsNotNull(builder.Assemblies.FirstOrDefault(a => a.AssemblyFilePath.EndsWith("ClassLibX.dll")));

            // Verify if Trending template is present
            Assert.HasCount(1, builder.TrendTemplates);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task FromRepositoryAsync_WithScripts_EmbedsScriptDependenciesAsync()
        {
            // Arrange
            LogCollector logCollector = new LogCollector();
            string repositoryPath = @"TestFiles\VisualStudio\ProtocolWithScripts";

            // Act
            var package = await ProtocolPackageCreator.Factory.FromRepositoryAsync(logCollector, repositoryPath);
            byte[] packageBytes = package.CreatePackage();

            // Assert
            using (var memoryStream = new System.IO.MemoryStream(packageBytes))
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                var entryNames = archive.Entries.Select(e => e.FullName).ToList();

                Assert.IsTrue(entryNames.Any(n => n == "Scripts/edc76df5-0d81-43a8-9b22-ffdd0b2fb2e2/manifest.json"));
                Assert.IsTrue(entryNames.Any(n => n == "Scripts/edc76df5-0d81-43a8-9b22-ffdd0b2fb2e2/run/main.py"));
                Assert.IsTrue(entryNames.Any(n => n.StartsWith("Scripts/edc76df5-0d81-43a8-9b22-ffdd0b2fb2e2/dependencies/") && n.EndsWith(".whl")));
            }

            // Verify the base protocol content is still present alongside the embedded scripts.
            Assert.AreEqual("ExampleProtocol", package.Name);
        }
    }
}