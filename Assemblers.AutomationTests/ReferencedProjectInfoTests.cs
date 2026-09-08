using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Packaging.Core;
using Skyline.DataMiner.CICD.Assemblers.Common;

namespace Assemblers.AutomationTests
{
    [TestClass]
    public class ReferencedProjectInfoTests
    {
        [TestMethod]
        public void ShouldHarvestAsNuGetAssemblies_PackableNonDataminer_ReturnsTrue()
        {
            var info = new ReferencedProjectInfo(
                "@\"C:\\tmp\\Lib.csproj",
                "My.Lib\"",
                "1.2.3",
                "netstandard2.0",
                "@\"C:\\tmp\\bin\\Debug\\netstandard2.0\\My.Lib.dll",
                "My.Lib",
                isPackable: true,
                generatePackageOnBuild: false,
                dataMinerType: "",
                directPackageReferences: new List<NuGet.Packaging.Core.PackageIdentity>());
            Assert.IsTrue(info.ShouldHarvestAsNuGetAssemblies());
        }
        [TestMethod]
        public void ShouldHarvestAsNuGetAssemblies_DataMinerProject_ReturnsFalse()
        {
            var info = new ReferencedProjectInfo(
                @"C:\tmp\Lib.csproj",
                "My.Lib", "1.2.3",
                "netstandard2.0",
                @"C:\tmp\bin\Debug\netstandard2.0\My.Lib.dll",
                "My.Lib",
                isPackable: true,
                generatePackageOnBuild: true,
                dataMinerType: "AutomationScript",
                directPackageReferences: new List<PackageIdentity>());

            Assert.IsFalse(info.ShouldHarvestAsNuGetAssemblies());
        }
        [TestMethod]
        public void GetDllImportRelativePath_FormatsExpectedPath()
        {
            var info = new ReferencedProjectInfo(
                @"C:\tmp\Lib.csproj",
                "Pkg.Id", "2.0.0",
                "netstandard2.0",
                @"C:\tmp\bin\Lib.dll",
                "Lib",
                isPackable: true,
                generatePackageOnBuild: false,
                dataMinerType: "",
                directPackageReferences: new List<PackageIdentity>());

            var path = info.GetDllImportRelativePath();

            Assert.AreEqual("Assemblies/ProtocolScripts/DllImport/Pkg.Id/2.0.0/lib/netstandard2.0/Lib.dll", path);
        }

    }

}
