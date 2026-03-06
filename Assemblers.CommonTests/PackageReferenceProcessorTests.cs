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
            Assert.IsEmpty(result.ImplicitDllImportDirectoryReferences);
            Assert.IsEmpty(result.DllImportFrameworkAssemblyReferences);
            Assert.IsEmpty(result.DllImportDirectoryReferences);
            Assert.IsEmpty(result.DllImportDirectoryReferencesAssembly);

            Assert.IsEmpty(result.NugetAssemblies);    // Assembly must not be included in package, only needs to be added to dllImport.
            Assert.HasCount(2, result.DllImportNugetAssemblyReferences);

            Assert.HasCount(2, result.ProcessedAssemblies);
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
            Assert.IsEmpty(result.ImplicitDllImportDirectoryReferences);
            Assert.IsEmpty(result.DllImportFrameworkAssemblyReferences);
            Assert.IsEmpty(result.DllImportDirectoryReferences);
            Assert.IsEmpty(result.DllImportDirectoryReferencesAssembly);

            Assert.IsEmpty(result.NugetAssemblies);    // Assembly must not be included in package, only needs to be added to dllImport.
            Assert.HasCount(2, result.DllImportNugetAssemblyReferences);

            Assert.HasCount(2, result.ProcessedAssemblies);
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
            const string pathExportImport = "skyline.dataminer.utils.exportimport\\1.0.0\\lib\\netstandard2.0";
            const string pathSolutionLib = "skyline.dataminer.dev.utils.modsolutionlib\\1.0.0\\lib\\netstandard2.0";
            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                    pathExportImport,
                    pathSolutionLib,
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathExportImport + "\\Skyline.DataMiner.Utils.ExportImport.dll", null, false),
                    new PackageAssemblyReference("SolutionLibraries\\ModSolutionLib\\Skyline.DataMiner.Dev.Utils.ModSolutionLib.dll", null, false),
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

        [TestMethod]
        public async Task ProcessAsyncTest_SolutionLibraries2()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Utils.Solutions.MediaOps.Live", new NuGetVersion("1.0.0")),
                new PackageIdentity("Skyline.DataMiner.Dev.Utils.Solutions.MediaOps.Plan", new NuGetVersion("1.5.0")),
            };

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.8";

            /*
             *<Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\alphafs.new\2.3.0\lib\net47\AlphaFS.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\newtonsoft.json\13.0.3\lib\net45\Newtonsoft.Json.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\sharpziplib\1.3.3\lib\net45\ICSharpCode.SharpZipLib.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.cicd.filesystem\1.1.0\lib\netstandard2.0\Skyline.DataMiner.CICD.FileSystem.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\SolutionLibraries\SDM.Abstractions\Skyline.DataMiner.Dev.Utils.SDM.Abstractions.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.sdm.sourcegenerator.runtime\1.0.1-rc1\lib\netstandard2.0\Skyline.DataMiner.SDM.SourceGenerator.Runtime.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.sdm\1.0.1-rc1\lib\net48\Skyline.DataMiner.SDM.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.sdm.registration.common\2.0.0\lib\net48\Skyline.DataMiner.SDM.Registration.Common.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.utils.dom\10.5.2.5\lib\net472\Skyline.DataMiner.Utils.DOM.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.utils.performanceanalyzer\3.0.2\lib\net48\Skyline.DataMiner.Utils.PerformanceAnalyzer.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\system.buffers\4.5.1\lib\net461\System.Buffers.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\system.numerics.vectors\4.5.0\lib\net46\System.Numerics.Vectors.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\system.runtime.compilerservices.unsafe\6.0.0\lib\net461\System.Runtime.CompilerServices.Unsafe.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\system.memory\4.5.5\lib\net461\System.Memory.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\microsoft.extensions.primitives\8.0.0\lib\net462\Microsoft.Extensions.Primitives.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\system.diagnostics.diagnosticsource\8.0.0\lib\net462\System.Diagnostics.DiagnosticSource.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\system.threading.tasks.dataflow\7.0.0\lib\net462\System.Threading.Tasks.Dataflow.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.core.dataminersystem.common\1.1.3.7\lib\net462\Skyline.DataMiner.Core.DataMinerSystem.Common.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\SolutionLibraries\Solutions.Categories\Skyline.DataMiner.Dev.Utils.Solutions.Categories.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\system.threading.tasks.extensions\4.5.4\lib\net461\System.Threading.Tasks.Extensions.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\microsoft.bcl.asyncinterfaces\8.0.0\lib\net462\Microsoft.Bcl.AsyncInterfaces.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\microsoft.extensions.dependencyinjection.abstractions\8.0.0\lib\net462\Microsoft.Extensions.DependencyInjection.Abstractions.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\microsoft.extensions.dependencyinjection\8.0.0\lib\net462\Microsoft.Extensions.DependencyInjection.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\microsoft.extensions.logging.abstractions\8.0.0\lib\net462\Microsoft.Extensions.Logging.Abstractions.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\system.valuetuple\4.5.0\lib\net47\System.ValueTuple.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\microsoft.extensions.options\8.0.0\lib\net462\Microsoft.Extensions.Options.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\microsoft.extensions.logging\8.0.0\lib\net462\Microsoft.Extensions.Logging.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\system.formats.asn1\9.0.2\lib\net462\System.Formats.Asn1.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\microsoft.bcl.cryptography\9.0.2\lib\net462\Microsoft.Bcl.Cryptography.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\system.security.cryptography.pkcs\9.0.2\lib\net462\System.Security.Cryptography.Pkcs.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.utils.securecoding\2.2.1\lib\netstandard2.0\Skyline.DataMiner.Utils.SecureCoding.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.core.interappcalls.common\1.1.1.1\lib\net462\Skyline.DataMiner.Core.InterAppCalls.Common.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\skyline.dataminer.connectorapi.skylinelockmanager\1.3.4\lib\net462\Skyline.DataMiner.ConnectorAPI.SkylineLockManager.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\SolutionLibraries\Solutions.MediaOps.Live\Skyline.DataMiner.Dev.Utils.Solutions.MediaOps.Live.dll</Param>
               <Param type="ref">C:\Skyline DataMiner\ProtocolScripts\DllImport\SolutionLibraries\Solutions.MediaOps.Plan\Skyline.DataMiner.Dev.Utils.Solutions.MediaOps.Plan.dll</Param>
               
             *
             */

            const string pathAlphaFs = "alphafs.new\\2.3.0\\lib\\net47";
            const string pathJson = "newtonsoft.json\\13.0.3\\lib\\net45";
            const string pathSharpZipLib = "sharpziplib\\1.3.3\\lib\\net45";
            const string pathFileSystem = "skyline.dataminer.cicd.filesystem\\1.1.0\\lib\\netstandard2.0";
            const string pathSdmSourcegenerator = "skyline.dataminer.sdm.sourcegenerator.runtime\\1.0.1-rc1\\lib\\netstandard2.0";
            const string pathSdm = "skyline.dataminer.sdm\\1.0.1-rc1\\lib\\net48";
            const string pathSdmRegistration = "skyline.dataminer.sdm.registration.common\\2.0.0\\lib\\net48";
            const string pathDom = "skyline.dataminer.utils.dom\\10.5.2.5\\lib\\net472";
            const string pathPerformanceAnalyzer = "skyline.dataminer.utils.performanceanalyzer\\3.0.2\\lib\\net48";
            const string pathBuffers = "system.buffers\\4.5.1\\lib\\net461";
            const string pathNumericsVectors = "system.numerics.vectors\\4.5.0\\lib\\net46";
            const string pathRuntimeCompilerservicesUnsafe = "system.runtime.compilerservices.unsafe\\6.0.0\\lib\\net461";
            const string pathMemory = "system.memory\\4.5.5\\lib\\net461";
            const string pathPrimitives = "microsoft.extensions.primitives\\8.0.0\\lib\\net462";
            const string pathDiagnosticSource = "system.diagnostics.diagnosticsource\\8.0.0\\lib\\net462";
            const string pathDataflow = "system.threading.tasks.dataflow\\7.0.0\\lib\\net462";
            const string pathCoreDmsCommon = "skyline.dataminer.core.dataminersystem.common\\1.1.3.7\\lib\\net462";
            const string pathThreadingTasksExtensions = "system.threading.tasks.extensions\\4.5.4\\lib\\net461";
            const string pathBcl = "microsoft.bcl.asyncinterfaces\\8.0.0\\lib\\net462";
            const string pathDiAbstraction = "microsoft.extensions.dependencyinjection.abstractions\\8.0.0\\lib\\net462";
            const string pathDi = "microsoft.extensions.dependencyinjection\\8.0.0\\lib\\net462";
            const string pathLoggingAbstraction = "microsoft.extensions.logging.abstractions\\8.0.0\\lib\\net462";
            const string pathLogging = "microsoft.extensions.logging\\8.0.0\\lib\\net462";
            const string pathValueTuple = "system.valuetuple\\4.5.0\\lib\\net47";
            const string pathOptions = "microsoft.extensions.options\\8.0.0\\lib\\net462";
            const string pathFormatsAsn1 = "system.formats.asn1\\9.0.2\\lib\\net462";
            const string pathCryptography = "microsoft.bcl.cryptography\\9.0.2\\lib\\net462";
            const string pathSecurityCryptographyPkcs = "system.security.cryptography.pkcs\\9.0.2\\lib\\net462";
            const string pathSecureCoding = "skyline.dataminer.utils.securecoding\\2.2.1\\lib\\netstandard2.0";
            const string pathCoreInterapp = "skyline.dataminer.core.interappcalls.common\\1.1.1.1\\lib\\net462";
            const string pathConnectorApiLock = "skyline.dataminer.connectorapi.skylinelockmanager\\1.3.4\\lib\\net462";

            const string pathSolLibSdmAbstractions = "skyline.dataminer.dev.utils.sdm.abstractions\\1.0.1\\lib\\net48";
            const string pathSolLibCategories = "skyline.dataminer.dev.utils.solutions.categories\\1.1.0\\lib\\net48";
            const string pathSolLibMediaOpsLive = "skyline.dataminer.dev.utils.solutions.mediaops.live\\1.0.0\\lib\\net48";
            const string pathSolLibMediaOpsPlan = "skyline.dataminer.dev.utils.solutions.mediaops.plan\\1.5.0\\lib\\net48";

            const string scriptPathSolLibSdmAbstractions = "SolutionLibraries\\SDM.Abstractions\\Skyline.DataMiner.Dev.Utils.SDM.Abstractions.dll";
            const string scriptPathSolLibCategories = "SolutionLibraries\\Solutions.Categories\\Skyline.DataMiner.Dev.Utils.Solutions.Categories.dll";
            const string scriptPathSolLibMediaOpsLive = "SolutionLibraries\\Solutions.MediaOps.Live\\Skyline.DataMiner.Dev.Utils.Solutions.MediaOps.Live.dll";
            const string scriptPathSolLibMediaOpsPlan = "SolutionLibraries\\Solutions.MediaOps.Plan\\Skyline.DataMiner.Dev.Utils.Solutions.MediaOps.Plan.dll";

            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathAlphaFs,
                    pathJson,
                    pathSharpZipLib,
                    pathFileSystem,
                    pathSdmSourcegenerator,
                    pathSdm,
                    pathSdmRegistration,
                    pathDom,
                    pathPerformanceAnalyzer,
                    pathBuffers,
                    pathNumericsVectors,
                    pathRuntimeCompilerservicesUnsafe,
                    pathMemory,
                    pathPrimitives,
                    pathDiagnosticSource,
                    pathDataflow,
                    pathCoreDmsCommon,
                    pathThreadingTasksExtensions,
                    pathBcl,
                    pathDiAbstraction,
                    pathDi,
                    pathLoggingAbstraction,
                    pathLogging,
                    pathValueTuple,
                    pathOptions,
                    pathFormatsAsn1,
                    pathCryptography,
                    pathSecurityCryptographyPkcs,
                    pathSecureCoding,
                    pathCoreInterapp,
                    pathConnectorApiLock,

                    pathSolLibSdmAbstractions,
                    pathSolLibCategories,
                    pathSolLibMediaOpsLive,
                    pathSolLibMediaOpsPlan
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(pathAlphaFs + "\\AlphaFS.dll", null, false),
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathSharpZipLib + "\\ICSharpCode.SharpZipLib.dll", null, false),
                    new PackageAssemblyReference(pathFileSystem + "\\Skyline.DataMiner.CICD.FileSystem.dll", null, false),
                    new PackageAssemblyReference(pathSdmSourcegenerator + "\\Skyline.DataMiner.SDM.SourceGenerator.Runtime.dll", null, false),
                    new PackageAssemblyReference(pathSdm + "\\Skyline.DataMiner.SDM.dll", null, false),
                    new PackageAssemblyReference(pathSdmRegistration + "\\Skyline.DataMiner.SDM.Registration.Common.dll", null, false),
                    new PackageAssemblyReference(pathDom + "\\Skyline.DataMiner.Utils.DOM.dll", null, false),
                    new PackageAssemblyReference(pathPerformanceAnalyzer + "\\Skyline.DataMiner.Utils.PerformanceAnalyzer.dll", null, false),
                    new PackageAssemblyReference(pathBuffers + "\\System.Buffers.dll", null, false),
                    new PackageAssemblyReference(pathNumericsVectors + "\\System.Numerics.Vectors.dll", null, false),
                    new PackageAssemblyReference(pathRuntimeCompilerservicesUnsafe + "\\System.Runtime.CompilerServices.Unsafe.dll", null, false),
                    new PackageAssemblyReference(pathMemory + "\\System.Memory.dll", null, false),
                    new PackageAssemblyReference(pathPrimitives + "\\Microsoft.Extensions.Primitives.dll", null, false),
                    new PackageAssemblyReference(pathDiagnosticSource + "\\System.Diagnostics.DiagnosticSource.dll", null, false),
                    new PackageAssemblyReference(pathDataflow + "\\System.Threading.Tasks.Dataflow.dll", null, false),
                    new PackageAssemblyReference(pathCoreDmsCommon + "\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll", null, false),
                    new PackageAssemblyReference(pathThreadingTasksExtensions + "\\System.Threading.Tasks.Extensions.dll", null, false),
                    new PackageAssemblyReference(pathBcl + "\\Microsoft.Bcl.AsyncInterfaces.dll", null, false),
                    new PackageAssemblyReference(pathDiAbstraction + "\\Microsoft.Extensions.DependencyInjection.Abstractions.dll", null, false),
                    new PackageAssemblyReference(pathDi + "\\Microsoft.Extensions.DependencyInjection.dll", null, false),
                    new PackageAssemblyReference(pathLoggingAbstraction + "\\Microsoft.Extensions.Logging.Abstractions.dll", null, false),
                    new PackageAssemblyReference(pathLogging + "\\Microsoft.Extensions.Logging.dll", null, false),
                    new PackageAssemblyReference(pathValueTuple + "\\System.ValueTuple.dll", null, false),
                    new PackageAssemblyReference(pathOptions + "\\Microsoft.Extensions.Options.dll", null, false),
                    new PackageAssemblyReference(pathFormatsAsn1 + "\\System.Formats.Asn1.dll", null, false),
                    new PackageAssemblyReference(pathCryptography + "\\Microsoft.Bcl.Cryptography.dll", null, false),
                    new PackageAssemblyReference(pathSecurityCryptographyPkcs + "\\System.Security.Cryptography.Pkcs.dll", null, false),
                    new PackageAssemblyReference(pathSecureCoding + "\\Skyline.DataMiner.Utils.SecureCoding.dll", null, false),
                    new PackageAssemblyReference(pathCoreInterapp + "\\Skyline.DataMiner.Core.InterAppCalls.Common.dll", null, false),
                    new PackageAssemblyReference(pathConnectorApiLock + "\\Skyline.DataMiner.ConnectorAPI.SkylineLockManager.dll", null, false),

                    new PackageAssemblyReference(scriptPathSolLibSdmAbstractions, null, false),
                    new PackageAssemblyReference(scriptPathSolLibCategories, null, false),
                    new PackageAssemblyReference(scriptPathSolLibMediaOpsLive, null, false),
                    new PackageAssemblyReference(scriptPathSolLibMediaOpsPlan, null, false),

                    new PackageAssemblyReference("skyline.dataminer.sdm\\1.0.1-rc1\\lib\\net48\\Skyline.DataMiner.SDM.SourceGenerator.Runtime.dll", null, false)
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(pathAlphaFs + "\\AlphaFS.dll", null, false),
                    new PackageAssemblyReference(pathJson + "\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference(pathSharpZipLib + "\\ICSharpCode.SharpZipLib.dll", null, false),
                    new PackageAssemblyReference(pathFileSystem + "\\Skyline.DataMiner.CICD.FileSystem.dll", null, false),
                    new PackageAssemblyReference(pathSdmSourcegenerator + "\\Skyline.DataMiner.SDM.SourceGenerator.Runtime.dll", null, false),
                    new PackageAssemblyReference(pathSdm + "\\Skyline.DataMiner.SDM.dll", null, false),
                    new PackageAssemblyReference(pathSdmRegistration + "\\Skyline.DataMiner.SDM.Registration.Common.dll", null, false),
                    new PackageAssemblyReference(pathDom + "\\Skyline.DataMiner.Utils.DOM.dll", null, false),
                    new PackageAssemblyReference(pathPerformanceAnalyzer + "\\Skyline.DataMiner.Utils.PerformanceAnalyzer.dll", null, false),
                    new PackageAssemblyReference(pathBuffers + "\\System.Buffers.dll", null, false),
                    new PackageAssemblyReference(pathNumericsVectors + "\\System.Numerics.Vectors.dll", null, false),
                    new PackageAssemblyReference(pathRuntimeCompilerservicesUnsafe + "\\System.Runtime.CompilerServices.Unsafe.dll", null, false),
                    new PackageAssemblyReference(pathMemory + "\\System.Memory.dll", null, false),
                    new PackageAssemblyReference(pathPrimitives + "\\Microsoft.Extensions.Primitives.dll", null, false),
                    new PackageAssemblyReference(pathDiagnosticSource + "\\System.Diagnostics.DiagnosticSource.dll", null, false),
                    new PackageAssemblyReference(pathDataflow + "\\System.Threading.Tasks.Dataflow.dll", null, false),
                    new PackageAssemblyReference(pathCoreDmsCommon + "\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll", null, false),
                    new PackageAssemblyReference(pathThreadingTasksExtensions + "\\System.Threading.Tasks.Extensions.dll", null, false),
                    new PackageAssemblyReference(pathBcl + "\\Microsoft.Bcl.AsyncInterfaces.dll", null, false),
                    new PackageAssemblyReference(pathDiAbstraction + "\\Microsoft.Extensions.DependencyInjection.Abstractions.dll", null, false),
                    new PackageAssemblyReference(pathDi + "\\Microsoft.Extensions.DependencyInjection.dll", null, false),
                    new PackageAssemblyReference(pathLoggingAbstraction + "\\Microsoft.Extensions.Logging.Abstractions.dll", null, false),
                    new PackageAssemblyReference(pathLogging + "\\Microsoft.Extensions.Logging.dll", null, false),
                    new PackageAssemblyReference(pathValueTuple + "\\System.ValueTuple.dll", null, false),
                    new PackageAssemblyReference(pathOptions + "\\Microsoft.Extensions.Options.dll", null, false),
                    new PackageAssemblyReference(pathFormatsAsn1 + "\\System.Formats.Asn1.dll", null, false),
                    new PackageAssemblyReference(pathCryptography + "\\Microsoft.Bcl.Cryptography.dll", null, false),
                    new PackageAssemblyReference(pathSecurityCryptographyPkcs + "\\System.Security.Cryptography.Pkcs.dll", null, false),
                    new PackageAssemblyReference(pathSecureCoding + "\\Skyline.DataMiner.Utils.SecureCoding.dll", null, false),
                    new PackageAssemblyReference(pathCoreInterapp + "\\Skyline.DataMiner.Core.InterAppCalls.Common.dll", null, false),
                    new PackageAssemblyReference(pathConnectorApiLock + "\\Skyline.DataMiner.ConnectorAPI.SkylineLockManager.dll", null, false),

                    // Dependencies with lower versions
                    new PackageAssemblyReference("newtonsoft.json\\13.0.2\\lib\\net45\\Newtonsoft.Json.dll", null, false),
                    new PackageAssemblyReference("skyline.dataminer.core.dataminersystem.common\\1.1.2.2\\lib\\net462\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll", null, false),
                    new PackageAssemblyReference("system.runtime.compilerservices.unsafe\\4.5.3\\lib\\net461\\System.Runtime.CompilerServices.Unsafe.dll", null, false),
                    new PackageAssemblyReference("skyline.dataminer.core.dataminersystem.common\\1.1.3.5\\lib\\net462\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll", null, false),
                    new PackageAssemblyReference("skyline.dataminer.core.dataminersystem.common\\1.1.3.3\\lib\\net462\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll", null, false),
                    new PackageAssemblyReference("skyline.dataminer.sdm\\1.0.1-rc1\\lib\\net48\\Skyline.DataMiner.SDM.SourceGenerator.Runtime.dll", null, false),
                },
                ProcessedAssemblies =
                {
                    "AlphaFS.dll",
                    "Newtonsoft.Json.dll",
                    "ICSharpCode.SharpZipLib.dll",
                    "Skyline.DataMiner.CICD.FileSystem.dll",
                    "Skyline.DataMiner.SDM.SourceGenerator.Runtime.dll",
                    "Skyline.DataMiner.SDM.dll",
                    "Skyline.DataMiner.SDM.Registration.Common.dll",
                    "Skyline.DataMiner.Utils.DOM.dll",
                    "Skyline.DataMiner.Utils.PerformanceAnalyzer.dll",
                    "System.Buffers.dll",
                    "System.Numerics.Vectors.dll",
                    "System.Runtime.CompilerServices.Unsafe.dll",
                    "System.Memory.dll",
                    "Microsoft.Extensions.Primitives.dll",
                    "System.Diagnostics.DiagnosticSource.dll",
                    "System.Threading.Tasks.Dataflow.dll",
                    "Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
                    "System.Threading.Tasks.Extensions.dll",
                    "Microsoft.Bcl.AsyncInterfaces.dll",
                    "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
                    "Microsoft.Extensions.DependencyInjection.dll",
                    "Microsoft.Extensions.Logging.Abstractions.dll",
                    "Microsoft.Extensions.Logging.dll",
                    "System.ValueTuple.dll",
                    "Microsoft.Extensions.Options.dll",
                    "System.Formats.Asn1.dll",
                    "Microsoft.Bcl.Cryptography.dll",
                    "System.Security.Cryptography.Pkcs.dll",
                    "Skyline.DataMiner.Utils.SecureCoding.dll",
                    "Skyline.DataMiner.Core.InterAppCalls.Common.dll",
                    "Skyline.DataMiner.ConnectorAPI.SkylineLockManager.dll",
                    
                    "Skyline.DataMiner.Dev.Utils.SDM.Abstractions.dll",
                    "Skyline.DataMiner.Dev.Utils.Solutions.Categories.dll",
                    "Skyline.DataMiner.Dev.Utils.Solutions.MediaOps.Live.dll",
                    "Skyline.DataMiner.Dev.Utils.Solutions.MediaOps.Plan.dll",

                    "System.Transactions.dll",
                    "mscorlib.dll",
                    "System.Numerics.dll",
                    "System.dll",
                    "System.ComponentModel.DataAnnotations.dll",
                    "System.Security.dll"
                },
                DllImportDirectoryReferencesAssembly =
                {
                    ["newtonsoft.json\\13.0.2\\lib\\net45\\"] = "newtonsoft.json\\13.0.2\\lib\\net45\\Newtonsoft.Json.dll",
                    ["skyline.dataminer.core.dataminersystem.common\\1.1.2.2\\lib\\net462\\"] = "skyline.dataminer.core.dataminersystem.common\\1.1.2.2\\lib\\net462\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
                    ["system.runtime.compilerservices.unsafe\\4.5.3\\lib\\net461\\"] = "system.runtime.compilerservices.unsafe\\4.5.3\\lib\\net461\\System.Runtime.CompilerServices.Unsafe.dll",
                    ["skyline.dataminer.core.dataminersystem.common\\1.1.3.5\\lib\\net462\\"] = "skyline.dataminer.core.dataminersystem.common\\1.1.3.5\\lib\\net462\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
                    ["skyline.dataminer.core.dataminersystem.common\\1.1.3.3\\lib\\net462\\"] = "skyline.dataminer.core.dataminersystem.common\\1.1.3.3\\lib\\net462\\Skyline.DataMiner.Core.DataMinerSystem.Common.dll",

                    ["skyline.dataminer.dev.utils.solutions.mediaops.live\\0.0.0\\lib\\net48\\"] = "skyline.dataminer.dev.utils.solutions.mediaops.live\\0.0.0\\lib\\net48\\Skyline.DataMiner.Dev.Utils.Solutions.MediaOps.Live.dll",
                },
                DllImportDirectoryReferences =
                {
                    "newtonsoft.json\\13.0.2\\lib\\net45\\",
                    "skyline.dataminer.core.dataminersystem.common\\1.1.2.2\\lib\\net462\\",
                    "system.runtime.compilerservices.unsafe\\4.5.3\\lib\\net461\\",
                    "skyline.dataminer.core.dataminersystem.common\\1.1.3.5\\lib\\net462\\",
                    "skyline.dataminer.core.dataminersystem.common\\1.1.3.3\\lib\\net462\\",

                    "skyline.dataminer.dev.utils.solutions.mediaops.live\\0.0.0\\lib\\net48\\"
                },
                DllImportFrameworkAssemblyReferences =
                {
                    "System.Transactions.dll",
                    "mscorlib.dll",
                    "System.Numerics.dll",
                    "System.dll",
                    "System.ComponentModel.DataAnnotations.dll",
                    "System.Security.dll"
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