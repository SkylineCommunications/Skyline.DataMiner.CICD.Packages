namespace Skyline.DataMiner.CICD.Assemblers.Common.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using FluentAssertions;
    using FluentAssertions.Equivalency;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NuGet.Packaging.Core;
    using NuGet.Versioning;

    using Skyline.DataMiner.CICD.Common.NuGet;

    [TestClass]
    public class PackageReferenceProcessorTests
    {
        [TestMethod]
        public async Task ProcessAsyncTest_FilesNuGet_OnlyReturnsAssemblyName_Protocol()
        {
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Protocol", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Skyline.DataMiner.Files.SLManagedAutomation", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Skyline.DataMiner.Files.SLMediationSnippets", new NuGetVersion("10.3.4.1"))
            };

            string targetFrameworkMoniker = ".NETFramework,Version=v4.6.2";

            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, targetFrameworkMoniker, DevPackHelper.ProtocolDevPackNuGetDependenciesIncludingTransitive);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.ImplicitDllImportDirectoryReferences.Count);
            Assert.AreEqual(0, result.DllImportFrameworkAssemblyReferences.Count);
            Assert.AreEqual(0, result.DllImportDirectoryReferences.Count);
            Assert.AreEqual(0, result.DllImportDirectoryReferencesAssembly.Count);

            Assert.AreEqual(0, result.NugetAssemblies.Count);    // Assembly must not be included in package, only needs to be added to dllImport.
            Assert.AreEqual(2, result.DllImportNugetAssemblyReferences.Count);

            Assert.AreEqual(2, result.ProcessedAssemblies.Count);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_FilesNuGet_OnlyReturnsAssemblyName_Automation()
        {
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Skyline.DataMiner.Files.SLManagedScripting", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Skyline.DataMiner.Files.SLMediationSnippets", new NuGetVersion("10.3.4.1"))
            };

            string targetFrameworkMoniker = ".NETFramework,Version=v4.6.2";

            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.ImplicitDllImportDirectoryReferences.Count);
            Assert.AreEqual(0, result.DllImportFrameworkAssemblyReferences.Count);
            Assert.AreEqual(0, result.DllImportDirectoryReferences.Count);
            Assert.AreEqual(0, result.DllImportDirectoryReferencesAssembly.Count);

            Assert.AreEqual(0, result.NugetAssemblies.Count);    // Assembly must not be included in package, only needs to be added to dllImport.
            Assert.AreEqual(2, result.DllImportNugetAssemblyReferences.Count);

            Assert.AreEqual(2, result.ProcessedAssemblies.Count);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_CommonScenario()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Skyline.DataMiner.Utils.ExportImport", new NuGetVersion("1.0.0")),
            };

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.6.2";

            const string pathJson = "newtonsoft.json\\13.0.3\\lib\\net45";
            const string pathExportImport = "skyline.dataminer.utils.exportimport\\1.0.0\\lib\\netstandard2.0";
            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                    pathExportImport,
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathExportImport + "\\Skyline.DataMiner.Utils.ExportImport.dll", null, false),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathExportImport + "\\Skyline.DataMiner.Utils.ExportImport.dll", null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "Skyline.DataMiner.Utils.ExportImport.dll",
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, targetFrameworkMoniker);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_UseOfOtherDevPackFile()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Skyline.DataMiner.Files.SLManagedScripting", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Skyline.DataMiner.Utils.ExportImport", new NuGetVersion("1.0.0")),
            };

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.6.2";

            const string pathJson = "newtonsoft.json\\13.0.3\\lib\\net45";
            const string pathExportImport = "skyline.dataminer.utils.exportimport\\1.0.0\\lib\\netstandard2.0";
            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                    pathExportImport,
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathExportImport + "\\Skyline.DataMiner.Utils.ExportImport.dll", null, false),

                    // Is a files package
                    new PackageAssemblyReference("SLManagedScripting.dll", null, true),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathExportImport + "\\Skyline.DataMiner.Utils.ExportImport.dll", null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "Skyline.DataMiner.Utils.ExportImport.dll",

                    "SLManagedScripting.dll"
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, targetFrameworkMoniker);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_UnitTestScenario()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Skyline.DataMiner.Utils.ExportImport", new NuGetVersion("1.0.0")),
                new PackageIdentity("Moq", new NuGetVersion("4.18.4"))
            };

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.6.2";

            const string pathJson = "newtonsoft.json\\13.0.3\\lib\\net45";
            const string pathExportImport = "skyline.dataminer.utils.exportimport\\1.0.0\\lib\\netstandard2.0";
            const string pathMoq = "moq\\4.18.4\\lib\\net462";
            const string pathCastleCore = "castle.core\\5.1.1\\lib\\net462";
            const string pathThreading = "system.threading.tasks.extensions\\4.5.4\\lib\\net461";
            const string pathCompiler = "system.runtime.compilerservices.unsafe\\4.5.3\\lib\\net461";
            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                    pathExportImport,
                    pathMoq,
                    pathCastleCore,
                    pathThreading,
                    pathCompiler,
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathExportImport + "\\Skyline.DataMiner.Utils.ExportImport.dll", null, false),
                    new PackageAssemblyReference(pathMoq + "\\Moq.dll", null, false),

                    // Dependencies from Moq
                    new PackageAssemblyReference(pathCastleCore + "\\Castle.Core.dll", null, false),
                    new PackageAssemblyReference(pathThreading + "\\System.Threading.Tasks.Extensions.dll", null, false),

                    // Dependencies from System.Threading.Tasks.Extensions
                    new PackageAssemblyReference(pathCompiler + "\\System.Runtime.CompilerServices.Unsafe.dll", null, false),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathExportImport + "\\Skyline.DataMiner.Utils.ExportImport.dll", null, false),
                    new PackageAssemblyReference(pathMoq + "\\Moq.dll", null, false),

                    // Dependencies from Moq
                    new PackageAssemblyReference(pathCastleCore + "\\Castle.Core.dll", null, false),
                    new PackageAssemblyReference(pathThreading + "\\System.Threading.Tasks.Extensions.dll", null, false),

                    // Dependencies from System.Threading.Tasks.Extensions
                    new PackageAssemblyReference(pathCompiler + "\\System.Runtime.CompilerServices.Unsafe.dll", null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "Skyline.DataMiner.Utils.ExportImport.dll",
                    "Moq.dll",
                    
                    // Dependencies from Moq
                    "Castle.Core.dll",
                    "System.Threading.Tasks.Extensions.dll",

                    // Dependencies from System.Threading.Tasks.Extensions
                    "System.Runtime.CompilerServices.Unsafe.dll",

                    "System.Configuration.dll",
                    "mscorlib.dll",
                },
                DllImportFrameworkAssemblyReferences =
                {
                    "System.Configuration.dll",
                    "mscorlib.dll",
                },
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, targetFrameworkMoniker);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_UnitTestScenario_Yle_Library()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("NPOI", new NuGetVersion("2.4.1")),
                new PackageIdentity("Skyline.DataMiner.ConnectorAPI.EVS.IPD-VIA", new NuGetVersion("1.0.0.4-Test1")),
                new PackageIdentity("Skyline.DataMiner.ConnectorAPI.YLE.OrderManager", new NuGetVersion("1.0.0.2-Test1")),
                new PackageIdentity("Skyline.DataMiner.Utils.InteractiveAutomationScriptToolkit", new NuGetVersion("6.1.0")),
                new PackageIdentity("Skyline.DataMiner.Utils.YLE.Integrations", new NuGetVersion("1.0.1.6-Test1")),
            };

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.7.2";

            const string pathJson = "newtonsoft.json\\13.0.3\\lib\\net45";
            const string pathJsonOld = "newtonsoft.json\\13.0.2\\lib\\net45";
            const string pathSharpZipLib = "sharpziplib\\1.0.0\\lib\\net45";
            const string pathNpoi = "npoi\\2.4.1\\lib\\net45";
            const string pathCoreDmsCommonNew = "skyline.dataminer.core.dataminersystem.common\\1.1.0.5\\lib\\net462";
            const string pathCoreDmsCommonOld = "skyline.dataminer.core.dataminersystem.common\\1.0.0.2\\lib\\net462";
            const string pathCoreInterApp = "skyline.dataminer.core.interappcalls.common\\1.0.0.2\\lib\\net462";
            const string pathEvs = "skyline.dataminer.connectorapi.evs.ipd-via\\1.0.0.4-test1\\lib\\net472";
            const string pathOrder = "skyline.dataminer.connectorapi.yle.ordermanager\\1.0.0.2-test1\\lib\\net472";
            const string pathToolkit = "skyline.dataminer.utils.interactiveautomationscripttoolkit\\6.1.0\\lib\\net462";
            const string pathIntegrations = "skyline.dataminer.utils.yle.integrations\\1.0.1.6-test1\\lib\\net472";
            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                    pathSharpZipLib,
                    pathNpoi,
                    pathCoreDmsCommonNew,
                    pathCoreInterApp,
                    pathEvs,
                    pathOrder,
                    pathToolkit,
                    pathIntegrations
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathSharpZipLib + "\\ICSharpCode.SharpZipLib.dll", null, false),
                    new PackageAssemblyReference(pathNpoi + "\\NPOI.dll", null, false),
                    new PackageAssemblyReference(pathNpoi + "\\NPOI.OOXML.dll", null, false),
                    new PackageAssemblyReference(pathNpoi + "\\NPOI.OpenXml4Net.dll", null, false),
                    new PackageAssemblyReference(pathNpoi + "\\NPOI.OpenXmlFormats.dll", null, false),
                    new PackageAssemblyReference(pathCoreDmsCommonNew + "\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll", null, false),
                    new PackageAssemblyReference(pathCoreInterApp + "\\Skyline.DataMiner.Core.InterAppCalls.Common.dll", null, false),
                    new PackageAssemblyReference(pathEvs + "\\Skyline.DataMiner.ConnectorAPI.EVS.IPD-VIA.dll", null, false),
                    new PackageAssemblyReference(pathOrder + "\\Skyline.DataMiner.ConnectorAPI.YLE.OrderManager.dll", null, false),
                    new PackageAssemblyReference(pathToolkit + "\\Skyline.DataMiner.Utils.InteractiveAutomationScriptToolkit.dll", null, false),
                    new PackageAssemblyReference(pathIntegrations + "\\Skyline.DataMiner.Utils.YLE.Integrations.dll", null, false),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathSharpZipLib + "\\ICSharpCode.SharpZipLib.dll", null, false),
                    new PackageAssemblyReference(pathNpoi + "\\NPOI.dll", null, false),
                    new PackageAssemblyReference(pathNpoi + "\\NPOI.OOXML.dll", null, false),
                    new PackageAssemblyReference(pathNpoi + "\\NPOI.OpenXml4Net.dll", null, false),
                    new PackageAssemblyReference(pathNpoi + "\\NPOI.OpenXmlFormats.dll", null, false),
                    new PackageAssemblyReference(pathCoreDmsCommonNew + "\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll", null, false),
                    new PackageAssemblyReference(pathCoreInterApp + "\\Skyline.DataMiner.Core.InterAppCalls.Common.dll", null, false),
                    new PackageAssemblyReference(pathEvs + "\\Skyline.DataMiner.ConnectorAPI.EVS.IPD-VIA.dll", null, false),
                    new PackageAssemblyReference(pathOrder + "\\Skyline.DataMiner.ConnectorAPI.YLE.OrderManager.dll", null, false),
                    new PackageAssemblyReference(pathToolkit + "\\Skyline.DataMiner.Utils.InteractiveAutomationScriptToolkit.dll", null, false),
                    new PackageAssemblyReference(pathIntegrations + "\\Skyline.DataMiner.Utils.YLE.Integrations.dll", null, false),
                    new PackageAssemblyReference(pathCoreDmsCommonOld + "\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll", null, false),
                    new PackageAssemblyReference(pathJsonOld + "\\Newtonsoft.Json.dll", null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "ICSharpCode.SharpZipLib.dll",

                    "NPOI.dll",

                    // Dependencies of NPOI
                    "NPOI.OOXML.dll",
                    "NPOI.OpenXml4Net.dll",
                    "NPOI.OpenXmlFormats.dll",

                    "Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
                    "Skyline.DataMiner.Core.InterAppCalls.Common.dll",
                    "Skyline.DataMiner.ConnectorAPI.EVS.IPD-VIA.dll",
                    "Skyline.DataMiner.ConnectorAPI.YLE.OrderManager.dll",
                    "Skyline.DataMiner.Utils.InteractiveAutomationScriptToolkit.dll",
                    "Skyline.DataMiner.Utils.YLE.Integrations.dll",
                },
                DllImportDirectoryReferencesAssembly =
                {
                    [pathCoreDmsCommonOld + "\\"] = pathCoreDmsCommonOld + "\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
                    [pathJsonOld + "\\"] = pathJsonOld + "\\Newtonsoft.Json.dll",
                },
                DllImportDirectoryReferences =
                {
                    pathCoreDmsCommonOld + "\\",
                    pathJsonOld + "\\",
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, targetFrameworkMoniker);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_SolutionLibraries()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Skyline.DataMiner.Dev.Utils.ModSolutionLib", new NuGetVersion("1.0.0")),
            };

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.8";

            const string pathJson = "newtonsoft.json\\13.0.3\\lib\\net45";
            const string pathSolutionLib = "skyline.dataminer.dev.utils.modsolutionlib\\1.0.0\\lib\\netstandard2.0";
            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                    pathSolutionLib,
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference("SolutionLibraries\\ModSolutionLib\\Skyline.DataMiner.Dev.Utils.ModSolutionLib.dll", null, false),
                 },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",

                    "Skyline.DataMiner.Dev.Utils.ModSolutionLib.dll",
                },
                DllImportDirectoryReferencesAssembly =
                {
                },
                DllImportDirectoryReferences =
                {
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, targetFrameworkMoniker);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        private static EquivalencyAssertionOptions<NuGetPackageAssemblyData> ExcludeAssemblyPath(EquivalencyAssertionOptions<NuGetPackageAssemblyData> arg)
        {
            arg.Excluding(x => x.Path.EndsWith("AssemblyPath"));
            return arg;
        }
    }
}