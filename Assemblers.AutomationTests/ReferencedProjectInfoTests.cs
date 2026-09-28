using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skyline.DataMiner.CICD.Assemblers.Common;

namespace Assemblers.AutomationTests
{
    [TestClass]
    public class ReferencedProjectInfoTests
    {
        [TestMethod]
        public void ShouldHarvestAssembly_NonDataMinerLibrary_ReturnsTrue()
        {
            var info = new ReferencedProjectInfo(
                @"C:\tmp\Lib.csproj")
            {
                DataMinerType = string.Empty,
                OutputType = "Library",
            };

            Assert.IsTrue(info.ShouldHarvestAssembly());
        }

        [TestMethod]
        public void ShouldHarvestAssembly_DataMinerProject_ReturnsFalse()
        {
            var info = new ReferencedProjectInfo(
                @"C:\tmp\Lib.csproj")
            {
                DataMinerType = "AutomationScript",
                OutputType = "Library",
            };

            Assert.IsFalse(info.ShouldHarvestAssembly());
        }

        [TestMethod]
        public void GetDllImportRelativePath_FormatsExpectedPath()
        {
            var info = new ReferencedProjectInfo(
                @"C:\tmp\Lib.csproj")
            {
                PackageId = "Pkg.Id",
                TargetFramework = "netstandard2.0",
                AssemblyName = "Lib",
                AssemblyVersion = "2.0.0",
            };

            var path = info.GetDllImportRelativePath();

            Assert.AreEqual(
                "pkg.id/2.0.0/lib/netstandard2.0/Lib.dll",
                path);
        }
    }
}