namespace DMApp.AutomationTests
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.DMApp.Automation;
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.Loggers;

    [TestClass]
    public class AppPackageCreatorForAutomationTests
    {
        [TestMethod]
        public async Task AddItemsAsync_ExpectedArgumentNullException()
        {
            // Arrange
            ILogCollector logCollector = new LogCollector();
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string repositoryPath = Path.GetFullPath(Path.Combine(baseDir, @"TestFiles\Solutions\Solution1"));
            const string packageName = "MyPackageName";
            var packageVersion = DMAppVersion.FromBuildNumber(1);
            var creator = AppPackageCreatorForAutomation.Factory.FromRepository(logCollector, repositoryPath, packageName, packageVersion);

            // Act
            Task Act() => creator.AddItemsAsync(null);

            // Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(Act);
        }
    }
}