namespace Skyline.DataMiner.CICD.Assemblers.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;
    using FluentAssertions.Equivalency;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    using Skyline.DataMiner.CICD.Common.NuGet;

    /// <summary>
    /// Test class for <see cref="PackageReferenceProcessor"/> to test scenarios related to solution wide package processing.
    /// </summary>
    [TestClass]
    public class PackageReferenceProcessorSolutionTests
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

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Protocol", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Skyline.DataMiner.Files.SLManagedAutomation", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Skyline.DataMiner.Files.SLMediationSnippets", new NuGetVersion("10.3.4.1"))
            };

            string targetFrameworkMoniker = ".NETFramework,Version=v4.6.2";

            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.ProtocolDevPackNuGetDependenciesIncludingTransitive);

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

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Skyline.DataMiner.Files.SLManagedScripting", new NuGetVersion("10.3.4.1")),
                new PackageIdentity("Skyline.DataMiner.Files.SLMediationSnippets", new NuGetVersion("10.3.4.1"))
            };

            string targetFrameworkMoniker = ".NETFramework,Version=v4.6.2";

            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

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

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
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
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathExportImport, "Skyline.DataMiner.Utils.ExportImport.dll"), null, false),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathExportImport, "Skyline.DataMiner.Utils.ExportImport.dll"), null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "Skyline.DataMiner.Utils.ExportImport.dll",
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_MulitpleVersionOfSameNuGet_LowestOnProject()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
            };

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
            };

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.8";

            const string pathJson = "newtonsoft.json\\13.0.4\\lib\\net45";

            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_MulitpleVersionOfSameNuGet_HighestOnProject()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
            };

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
            };

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.8";

            const string pathJson = "newtonsoft.json\\13.0.4\\lib\\net45";

            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

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

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
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
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathExportImport, "Skyline.DataMiner.Utils.ExportImport.dll"), null, false),

                    // Is a files package
                    new PackageAssemblyReference("SLManagedScripting.dll", null, true),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathExportImport, "Skyline.DataMiner.Utils.ExportImport.dll"), null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "Skyline.DataMiner.Utils.ExportImport.dll",

                    "SLManagedScripting.dll"
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

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

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
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
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathExportImport, "Skyline.DataMiner.Utils.ExportImport.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathMoq, "Moq.dll"), null, false),

                    // Dependencies from Moq
                    new PackageAssemblyReference(Path.Combine(pathCastleCore, "Castle.Core.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathThreading, "System.Threading.Tasks.Extensions.dll"), null, false),

                    // Dependencies from System.Threading.Tasks.Extensions
                    new PackageAssemblyReference(Path.Combine(pathCompiler, "System.Runtime.CompilerServices.Unsafe.dll"), null, false),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathExportImport, "Skyline.DataMiner.Utils.ExportImport.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathMoq, "Moq.dll"), null, false),

                    // Dependencies from Moq
                    new PackageAssemblyReference(Path.Combine(pathCastleCore, "Castle.Core.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathThreading, "System.Threading.Tasks.Extensions.dll"), null, false),

                    // Dependencies from System.Threading.Tasks.Extensions
                    new PackageAssemblyReference(Path.Combine(pathCompiler, "System.Runtime.CompilerServices.Unsafe.dll"), null, false),
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
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

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

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
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
            const string pathSharpZipLib = "sharpziplib\\1.0.0\\lib\\net45";
            const string pathNpoi = "npoi\\2.4.1\\lib\\net45";
            const string pathCoreDmsCommonNew = "skyline.dataminer.core.dataminersystem.common\\1.1.0.5\\lib\\net462";
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
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSharpZipLib, "ICSharpCode.SharpZipLib.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNpoi, "NPOI.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNpoi, "NPOI.OOXML.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNpoi, "NPOI.OpenXml4Net.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNpoi, "NPOI.OpenXmlFormats.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathCoreDmsCommonNew, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathCoreInterApp, "Skyline.DataMiner.Core.InterAppCalls.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathEvs, "Skyline.DataMiner.ConnectorAPI.EVS.IPD-VIA.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathOrder, "Skyline.DataMiner.ConnectorAPI.YLE.OrderManager.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathToolkit, "Skyline.DataMiner.Utils.InteractiveAutomationScriptToolkit.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathIntegrations, "Skyline.DataMiner.Utils.YLE.Integrations.dll"), null, false),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSharpZipLib, "ICSharpCode.SharpZipLib.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNpoi, "NPOI.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNpoi, "NPOI.OOXML.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNpoi, "NPOI.OpenXml4Net.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNpoi, "NPOI.OpenXmlFormats.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathCoreDmsCommonNew, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathCoreInterApp, "Skyline.DataMiner.Core.InterAppCalls.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathEvs, "Skyline.DataMiner.ConnectorAPI.EVS.IPD-VIA.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathOrder, "Skyline.DataMiner.ConnectorAPI.YLE.OrderManager.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathToolkit, "Skyline.DataMiner.Utils.InteractiveAutomationScriptToolkit.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathIntegrations, "Skyline.DataMiner.Utils.YLE.Integrations.dll"), null, false),
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
                },
                DllImportDirectoryReferences =
                {
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(projectPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_SolutionLibraries()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> topLevelProjectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Skyline.DataMiner.Utils.SecureCoding", new NuGetVersion("2.2.3")),
            };

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Skyline.DataMiner.Utils.SecureCoding", new NuGetVersion("2.2.3")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
                new PackageIdentity("Skyline.DataMiner.CICD.FileSystem", new NuGetVersion("1.4.2")),
                new PackageIdentity("AlphaFS.New", new NuGetVersion("2.3.0")),
                new PackageIdentity("System.Security.Cryptography.Pkcs", new NuGetVersion("10.0.5")),
                new PackageIdentity("Microsoft.Bcl.Cryptography", new NuGetVersion("10.0.8")),
                new PackageIdentity("System.Formats.Asn1", new NuGetVersion("10.0.8")),
                new PackageIdentity("System.Memory", new NuGetVersion("4.6.3")),
                new PackageIdentity("System.ValueTuple", new NuGetVersion("4.6.2")),
                new PackageIdentity("System.Buffers", new NuGetVersion("4.6.1")),
                new PackageIdentity("System.Numerics.Vectors", new NuGetVersion("4.6.1")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("6.1.2")),
            };

            using (var cacheContext = new SourceCacheContext())
            {
                cacheContext.MaxAge = DateTimeOffset.UtcNow;

                foreach (var package in solutionPackages)
                {
                    await packageReferenceProcessor.InstallPackageIfNotFound(package, cacheContext, CancellationToken.None);
                }
            }

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.8";

            const string pathJson = @"newtonsoft.json\13.0.4\lib\net45";
            const string pathSecureCoding = @"skyline.dataminer.utils.securecoding\2.2.3\lib\netstandard2.0";
            const string pathAlphaFs = @"alphafs.new\2.3.0\lib\net47";
            const string pathFileSystem = @"skyline.dataminer.cicd.filesystem\1.4.2\lib\netstandard2.0";
            const string pathSystemBuffers = @"system.buffers\4.6.1\lib\net462";
            const string pathSystemNumericsVectors = @"system.numerics.vectors\4.6.1\lib\net462";
            const string pathSystemRuntimeCompilerServicesUnsafe = @"system.runtime.compilerservices.unsafe\6.1.2\lib\net462";
            const string pathSystemMemory = @"system.memory\4.6.3\lib\net462";
            const string pathSystemFormatsAsn1 = @"system.formats.asn1\10.0.8\lib\net462";
            const string pathMicrosoftBclCryptography = @"microsoft.bcl.cryptography\10.0.8\lib\net462";
            const string pathSystemSecurityCryptographyPkcs = @"system.security.cryptography.pkcs\10.0.5\lib\net462";

            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                    pathSecureCoding,
                    pathAlphaFs,
                    pathFileSystem,
                    pathSystemBuffers,
                    pathSystemNumericsVectors,
                    pathSystemRuntimeCompilerServicesUnsafe,
                    pathSystemMemory,
                    pathSystemFormatsAsn1,
                    pathMicrosoftBclCryptography,
                    pathSystemSecurityCryptographyPkcs
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSecureCoding, "Skyline.DataMiner.Utils.SecureCoding.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathAlphaFs, "AlphaFS.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathFileSystem, "Skyline.DataMiner.CICD.FileSystem.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemBuffers, "System.Buffers.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemNumericsVectors, "System.Numerics.Vectors.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemRuntimeCompilerServicesUnsafe, "System.Runtime.CompilerServices.Unsafe.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemMemory, "System.Memory.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemFormatsAsn1, "System.Formats.Asn1.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathMicrosoftBclCryptography, "Microsoft.Bcl.Cryptography.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemSecurityCryptographyPkcs, "System.Security.Cryptography.Pkcs.dll"), null, false),
                 },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSecureCoding, "Skyline.DataMiner.Utils.SecureCoding.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathAlphaFs, "AlphaFS.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathFileSystem, "Skyline.DataMiner.CICD.FileSystem.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemBuffers, "System.Buffers.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemNumericsVectors, "System.Numerics.Vectors.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemRuntimeCompilerServicesUnsafe, "System.Runtime.CompilerServices.Unsafe.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemMemory, "System.Memory.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemFormatsAsn1, "System.Formats.Asn1.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathMicrosoftBclCryptography, "Microsoft.Bcl.Cryptography.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemSecurityCryptographyPkcs, "System.Security.Cryptography.Pkcs.dll"), null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "Skyline.DataMiner.Utils.SecureCoding.dll",
                    "AlphaFS.dll",
                    "Skyline.DataMiner.CICD.FileSystem.dll",
                    "System.Buffers.dll",
                    "System.Numerics.Vectors.dll",
                    "System.Runtime.CompilerServices.Unsafe.dll",
                    "System.Memory.dll",
                    "System.Formats.Asn1.dll",
                    "Microsoft.Bcl.Cryptography.dll",
                    "System.Security.Cryptography.Pkcs.dll",
                    "System.Transactions.dll",
                    "System.Numerics.dll",
                    "mscorlib.dll",
                    "System.ValueTuple.dll",
                    "System.Security.dll"
                },
                DllImportDirectoryReferencesAssembly =
                {
                },
                DllImportDirectoryReferences =
                {
                },
                DllImportFrameworkAssemblyReferences =
                {
                    "System.Transactions.dll",
                    "System.Numerics.dll",
                    "mscorlib.dll",
                    "System.ValueTuple.dll",
                    "System.Security.dll"
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(topLevelProjectPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        /// <summary>
        /// Tests a solution with two script projects (Project A and Project B) with overlapping but different package references.
        /// </summary>
        /// <remarks>
        /// ProjectB has a reference to a higher version of Newtonsoft.Json and indirectly references a higher version of Skyline.DataMiner.Core.DataMinerSystem.Common.
        /// </remarks>
        [TestMethod]
        public async Task ProcessAsyncTest_SolutionLibraries_ProjectA()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            // Project A top level package references:
            //├─ Newtonsoft.Json 13.0.3
            //└─ Skyline.DataMiner.Core.DataMinerSystem.Common 1.1.1.11
            //   ├─ Newtonsoft.Json[13.0.2, )
            //   └─ System.Threading.Tasks.Dataflow[7.0.0, )
            IList<PackageIdentity> topLevelProjectAPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.1.11")),
            };

            // Project B top level package references:
            //├─ Newtonsoft.Json 13.0.4
            //└─ Skyline.DataMiner.Core.InterAppCalls.Common 1.1.1.1
            //   ├─ Skyline.DataMiner.Core.DataMinerSystem.Common 1.1.2.2
            //   │  ├─ Newtonsoft.Json 13.0.2
            //   │  └─ System.Threading.Tasks.Dataflow 7.0.0
            //   └─ Skyline.DataMiner.Utils.SecureCoding 2.2.1
            //      ├─ Newtonsoft.Json 13.0.3
            //      ├─ Skyline.DataMiner.CICD.FileSystem 1.1.0
            //      │  └─ AlphaFS.New 2.3.0
            //      └─ System.Security.Cryptography.Pkcs 9.0.2
            //         └─ Microsoft.Bcl.Cryptography 9.0.2
            //            ├─ System.Formats.Asn1 9.0.2
            //            │  ├─ System.Buffers 4.5.1
            //            │  ├─ System.Memory 4.5.5
            //            │  │  ├─ System.Buffers 4.5.1
            //            │  │  ├─ System.Numerics.Vectors 4.5.0
            //            │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //            │  └─ System.ValueTuple 4.5.0
            //            ├─ System.Memory 4.5.5
            //            │  ├─ System.Buffers 4.5.1
            //            │  ├─ System.Numerics.Vectors 4.5.0
            //            │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //            └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            //IList<PackageIdentity> topLevelProjectBPackages = new List<PackageIdentity>
            //{
            //    new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
            //    new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
            //    new PackageIdentity("Skyline.DataMiner.Core.InterAppCalls.Common", new NuGetVersion("1.1.1.1")),
            //};

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
            {
                new PackageIdentity("AlphaFS.New", new NuGetVersion("2.3.0")),
                new PackageIdentity("Microsoft.Bcl.Cryptography", new NuGetVersion("9.0.2")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.2")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
                new PackageIdentity("Skyline.DataMiner.CICD.FileSystem", new NuGetVersion("1.1.0")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.1.11")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.2.2")),
                new PackageIdentity("Skyline.DataMiner.Core.InterAppCalls.Common", new NuGetVersion("1.1.1.1")),
                new PackageIdentity("Skyline.DataMiner.Utils.SecureCoding", new NuGetVersion("2.2.1")),
                new PackageIdentity("System.Buffers", new NuGetVersion("4.5.1")),
                new PackageIdentity("System.Formats.Asn1", new NuGetVersion("9.0.2")),
                new PackageIdentity("System.Memory", new NuGetVersion("4.5.5")),
                new PackageIdentity("System.Numerics.Vectors", new NuGetVersion("4.5.0")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("4.5.3")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("6.0.0")),
                new PackageIdentity("System.Security.Cryptography.Pkcs", new NuGetVersion("9.0.2")),
                new PackageIdentity("System.Threading.Tasks.Dataflow", new NuGetVersion("7.0.0")),
                new PackageIdentity("System.ValueTuple", new NuGetVersion("4.5.0")),

                // Dev pack & Files packages:
                new PackageIdentity("Skyline.DataMiner.Dev.Common", new NuGetVersion("10.1.11")),
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Skyline.DataMiner.Dev.Common", new NuGetVersion("10.3.5")),
                new PackageIdentity("Skyline.DataMiner.Files.DataMinerMessageBroker.API", new NuGetVersion("10.3.0.14")),
            };

            using (var cacheContext = new SourceCacheContext())
            {
                cacheContext.MaxAge = DateTimeOffset.UtcNow;

                foreach (var package in solutionPackages)
                {
                    await packageReferenceProcessor.InstallPackageIfNotFound(package, cacheContext, CancellationToken.None);
                }
            }

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.8";

            const string pathJson = @"newtonsoft.json\13.0.4\lib\net45";
            const string pathSystemThreadingTasksDataflow = @"system.threading.tasks.dataflow\7.0.0\lib\net462";
            const string pathSkylineDataMinerCoreDataMinerSystemCommon = @"skyline.dataminer.core.dataminersystem.common\1.1.2.2\lib\net462";

            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                    pathSystemThreadingTasksDataflow,
                    pathSkylineDataMinerCoreDataMinerSystemCommon
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTasksDataflow, "System.Threading.Tasks.Dataflow.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreDataMinerSystemCommon, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false)
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTasksDataflow, "System.Threading.Tasks.Dataflow.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreDataMinerSystemCommon, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false)
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "System.Threading.Tasks.Dataflow.dll",
                    "Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
                },
                DllImportDirectoryReferencesAssembly =
                {
                },
                DllImportDirectoryReferences =
                {
                },
                DllImportFrameworkAssemblyReferences =
                {
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(topLevelProjectAPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        /// <summary>
        /// Tests a solution with two script projects (Project A and Project B) with overlapping but different package references.
        /// </summary>
        /// <remarks>
        /// ProjectB has a reference to a higher version of Newtonsoft.Json and indirectly references a higher version of Skyline.DataMiner.Core.DataMinerSystem.Common.
        /// </remarks>
        [TestMethod]
        public async Task ProcessAsyncTest_SolutionLibraries_ProjectA_WithSrmNuget()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            // Project A top level package references:
            //├─ Newtonsoft.Json 13.0.3
            //└─ Skyline.DataMiner.Core.DataMinerSystem.Common 1.1.1.11
            //   ├─ Newtonsoft.Json[13.0.2, )
            //   └─ System.Threading.Tasks.Dataflow[7.0.0, )
            IList<PackageIdentity> topLevelProjectAPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.1.11")),
                new PackageIdentity("Skyline.DataMiner.Core.SRM.Utils.Dijkstra", new NuGetVersion("2.0.5")),
            };

            // Project B top level package references:
            //├─ Newtonsoft.Json 13.0.4
            //└─ Skyline.DataMiner.Core.InterAppCalls.Common 1.1.1.1
            //   ├─ Skyline.DataMiner.Core.DataMinerSystem.Common 1.1.2.2
            //   │  ├─ Newtonsoft.Json 13.0.2
            //   │  └─ System.Threading.Tasks.Dataflow 7.0.0
            //   └─ Skyline.DataMiner.Utils.SecureCoding 2.2.1
            //      ├─ Newtonsoft.Json 13.0.3
            //      ├─ Skyline.DataMiner.CICD.FileSystem 1.1.0
            //      │  └─ AlphaFS.New 2.3.0
            //      └─ System.Security.Cryptography.Pkcs 9.0.2
            //         └─ Microsoft.Bcl.Cryptography 9.0.2
            //            ├─ System.Formats.Asn1 9.0.2
            //            │  ├─ System.Buffers 4.5.1
            //            │  ├─ System.Memory 4.5.5
            //            │  │  ├─ System.Buffers 4.5.1
            //            │  │  ├─ System.Numerics.Vectors 4.5.0
            //            │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //            │  └─ System.ValueTuple 4.5.0
            //            ├─ System.Memory 4.5.5
            //            │  ├─ System.Buffers 4.5.1
            //            │  ├─ System.Numerics.Vectors 4.5.0
            //            │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //            └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            //IList<PackageIdentity> topLevelProjectBPackages = new List<PackageIdentity>
            //{
            //    new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
            //    new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
            //    new PackageIdentity("Skyline.DataMiner.Core.InterAppCalls.Common", new NuGetVersion("1.1.1.1")),
            //};

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
            {
                new PackageIdentity("AlphaFS.New", new NuGetVersion("2.3.0")),
                new PackageIdentity("Microsoft.Bcl.Cryptography", new NuGetVersion("9.0.2")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.2")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
                new PackageIdentity("QuickGraph", new NuGetVersion("3.6.61119.7")),
                new PackageIdentity("Skyline.DataMiner.CICD.FileSystem", new NuGetVersion("1.1.0")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.1.11")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.2.2")),
                new PackageIdentity("Skyline.DataMiner.Core.InterAppCalls.Common", new NuGetVersion("1.1.1.1")),
                new PackageIdentity("Skyline.DataMiner.Core.SRM.Utils.Dijkstra", new NuGetVersion("2.0.5")),
                new PackageIdentity("Skyline.DataMiner.Utils.SecureCoding", new NuGetVersion("2.2.1")),
                new PackageIdentity("System.Buffers", new NuGetVersion("4.5.1")),
                new PackageIdentity("System.Formats.Asn1", new NuGetVersion("9.0.2")),
                new PackageIdentity("System.Memory", new NuGetVersion("4.5.5")),
                new PackageIdentity("System.Numerics.Vectors", new NuGetVersion("4.5.0")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("4.5.3")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("6.0.0")),
                new PackageIdentity("System.Security.Cryptography.Pkcs", new NuGetVersion("9.0.2")),
                new PackageIdentity("System.Threading.Tasks.Dataflow", new NuGetVersion("7.0.0")),
                new PackageIdentity("System.ValueTuple", new NuGetVersion("4.5.0")),

                // Dev pack & Files packages:
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.6.1")),
                new PackageIdentity("Skyline.DataMiner.Dev.Common", new NuGetVersion("10.1.11")),
                new PackageIdentity("Skyline.DataMiner.Dev.Common", new NuGetVersion("10.3.5")),
                new PackageIdentity("Skyline.DataMiner.Dev.Common", new NuGetVersion("10.6.1")),
                new PackageIdentity("Skyline.DataMiner.Files.DataMinerMessageBroker.API", new NuGetVersion("10.3.0.14")),
            };

            using (var cacheContext = new SourceCacheContext())
            {
                cacheContext.MaxAge = DateTimeOffset.UtcNow;

                foreach (var package in solutionPackages)
                {
                    await packageReferenceProcessor.InstallPackageIfNotFound(package, cacheContext, CancellationToken.None);
                }
            }

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.8";

            const string pathJson = @"newtonsoft.json\13.0.4\lib\net45";
            const string pathSystemThreadingTasksDataflow = @"system.threading.tasks.dataflow\7.0.0\lib\net462";
            const string pathSkylineDataMinerCoreDataMinerSystemCommon = @"skyline.dataminer.core.dataminersystem.common\1.1.2.2\lib\net462";
            const string pathSkylineDataMinerCoreSrmUtilsDijkstra = @"SRM"; // This is a special case, this assembly is part of the SRM solution and is expected to be present in this folder.

            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathJson,
                    pathSystemThreadingTasksDataflow,
                    pathSkylineDataMinerCoreDataMinerSystemCommon,
                    pathSkylineDataMinerCoreSrmUtilsDijkstra
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTasksDataflow, "System.Threading.Tasks.Dataflow.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreDataMinerSystemCommon, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreSrmUtilsDijkstra, "SLDijkstraSearch.dll"), null, false)
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTasksDataflow, "System.Threading.Tasks.Dataflow.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreDataMinerSystemCommon, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false)
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "System.Threading.Tasks.Dataflow.dll",
                    "Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
                    "SLDijkstraSearch.dll",
                    // Framework assemblies:
                    "Microsoft.CSharp.dll",
                    "System.dll",
                    "System.Core.dll",
                    "System.Data.dll",
                    "System.Data.DataSetExtensions.dll",
                    "System.Runtime.Serialization.dll",
                    "System.Xml.dll",
                    "System.Xml.Linq.dll"
                },
                DllImportDirectoryReferencesAssembly =
                {
                },
                DllImportDirectoryReferences =
                {
                },
                DllImportFrameworkAssemblyReferences =
                {
                    "Microsoft.CSharp.dll",
                    "System.dll",
                    "System.Core.dll",
                    "System.Data.dll",
                    "System.Data.DataSetExtensions.dll",
                    "System.Runtime.Serialization.dll",
                    "System.Xml.dll",
                    "System.Xml.Linq.dll"
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(topLevelProjectAPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        /// <summary>
        /// Same as previous test case but now gets results for package B.
        /// </summary>
        [TestMethod]
        public async Task ProcessAsyncTest_SolutionLibraries_ProjectB()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            // Project A top level package references:
            //├─ Newtonsoft.Json 13.0.3
            //└─ Skyline.DataMiner.Core.DataMinerSystem.Common 1.1.1.11
            //   ├─ Newtonsoft.Json[13.0.2, )
            //   └─ System.Threading.Tasks.Dataflow[7.0.0, )
            //IList<PackageIdentity> topLevelProjectAPackages = new List<PackageIdentity>
            //{
            //    new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
            //    new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
            //    new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.1.11")),
            //};

            // Project B top level package references:
            //├─ Newtonsoft.Json 13.0.4
            //└─ Skyline.DataMiner.Core.InterAppCalls.Common 1.1.1.1
            //   ├─ Skyline.DataMiner.Core.DataMinerSystem.Common 1.1.2.2
            //   │  ├─ Newtonsoft.Json 13.0.2
            //   │  └─ System.Threading.Tasks.Dataflow 7.0.0
            //   └─ Skyline.DataMiner.Utils.SecureCoding 2.2.1
            //      ├─ Newtonsoft.Json 13.0.3
            //      ├─ Skyline.DataMiner.CICD.FileSystem 1.1.0
            //      │  └─ AlphaFS.New 2.3.0
            //      └─ System.Security.Cryptography.Pkcs 9.0.2
            //         └─ Microsoft.Bcl.Cryptography 9.0.2
            //            ├─ System.Formats.Asn1 9.0.2
            //            │  ├─ System.Buffers 4.5.1
            //            │  ├─ System.Memory 4.5.5
            //            │  │  ├─ System.Buffers 4.5.1
            //            │  │  ├─ System.Numerics.Vectors 4.5.0
            //            │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //            │  └─ System.ValueTuple 4.5.0
            //            ├─ System.Memory 4.5.5
            //            │  ├─ System.Buffers 4.5.1
            //            │  ├─ System.Numerics.Vectors 4.5.0
            //            │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //            └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            IList<PackageIdentity> topLevelProjectBPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
                new PackageIdentity("Skyline.DataMiner.Core.InterAppCalls.Common", new NuGetVersion("1.1.1.1")),
            };

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
            {
                new PackageIdentity("AlphaFS.New", new NuGetVersion("2.3.0")),
                new PackageIdentity("Microsoft.Bcl.Cryptography", new NuGetVersion("9.0.2")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.2")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
                new PackageIdentity("Skyline.DataMiner.CICD.FileSystem", new NuGetVersion("1.1.0")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.1.11")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.2.2")),
                new PackageIdentity("Skyline.DataMiner.Core.InterAppCalls.Common", new NuGetVersion("1.1.1.1")),
                new PackageIdentity("Skyline.DataMiner.Utils.SecureCoding", new NuGetVersion("2.2.1")),
                new PackageIdentity("System.Buffers", new NuGetVersion("4.5.1")),
                new PackageIdentity("System.Formats.Asn1", new NuGetVersion("9.0.2")),
                new PackageIdentity("System.Memory", new NuGetVersion("4.5.5")),
                new PackageIdentity("System.Numerics.Vectors", new NuGetVersion("4.5.0")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("4.5.3")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("6.0.0")),
                new PackageIdentity("System.Security.Cryptography.Pkcs", new NuGetVersion("9.0.2")),
                new PackageIdentity("System.Threading.Tasks.Dataflow", new NuGetVersion("7.0.0")),
                new PackageIdentity("System.ValueTuple", new NuGetVersion("4.5.0")),

                // Dev pack & Files packages:
                new PackageIdentity("Skyline.DataMiner.Dev.Common", new NuGetVersion("10.1.11")),
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Skyline.DataMiner.Dev.Common", new NuGetVersion("10.3.5")),
                new PackageIdentity("Skyline.DataMiner.Files.DataMinerMessageBroker.API", new NuGetVersion("10.3.0.14")),
            };

            using (var cacheContext = new SourceCacheContext())
            {
                cacheContext.MaxAge = DateTimeOffset.UtcNow;

                foreach (var package in solutionPackages)
                {
                    await packageReferenceProcessor.InstallPackageIfNotFound(package, cacheContext, CancellationToken.None);
                }
            }

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.8";

            const string pathAlphaFs = @"alphafs.new\2.3.0\lib\net47";
            const string pathMicrosoftBclCryptography = @"microsoft.bcl.cryptography\9.0.2\lib\net462";
            const string pathJson = @"newtonsoft.json\13.0.4\lib\net45";
            const string pathFileSystem = @"skyline.dataminer.cicd.filesystem\1.1.0\lib\netstandard2.0";
            const string pathSkylineDataMinerCoreDataMinerSystemCommon = @"skyline.dataminer.core.dataminersystem.common\1.1.2.2\lib\net462";
            const string pathSkylineDataMinerCoreInterAppCallsCommon = @"skyline.dataminer.core.interappcalls.common\1.1.1.1\lib\net462";
            const string pathSkylineDataMinerUtilsSecureCoding = @"skyline.dataminer.utils.securecoding\2.2.1\lib\netstandard2.0";
            const string pathSystemBuffers = @"system.buffers\4.5.1\lib\net461";
            const string pathSystemFormatsAsn1 = @"system.formats.asn1\9.0.2\lib\net462";
            const string pathSystemMemory = @"system.memory\4.5.5\lib\net461";
            const string pathSystemNumericsVectors = @"system.numerics.vectors\4.5.0\lib\net46";
            const string pathSystemRuntimeCompilerServicesUnsafe = @"system.runtime.compilerservices.unsafe\6.0.0\lib\net461";
            const string pathSystemSecurityCryptographyPkcs = @"system.security.cryptography.pkcs\9.0.2\lib\net462";
            const string pathSystemThreadingTasksDataflow = @"system.threading.tasks.dataflow\7.0.0\lib\net462";
            const string pathSystemValueTuple = @"system.valuetuple\4.5.0\lib\net47";

            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathAlphaFs,
                    pathMicrosoftBclCryptography,
                    pathJson,
                    pathFileSystem,
                    pathSystemThreadingTasksDataflow,
                    pathSkylineDataMinerCoreDataMinerSystemCommon,
                    pathSkylineDataMinerCoreInterAppCallsCommon,
                    pathSkylineDataMinerUtilsSecureCoding,
                    pathSystemBuffers,
                    pathSystemFormatsAsn1,
                    pathSystemMemory,
                    pathSystemNumericsVectors,
                    pathSystemRuntimeCompilerServicesUnsafe,
                    pathSystemSecurityCryptographyPkcs,
                    pathSystemValueTuple
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(Path.Combine(pathAlphaFs, "AlphaFS.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathMicrosoftBclCryptography, "Microsoft.Bcl.Cryptography.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathFileSystem, "Skyline.DataMiner.CICD.FileSystem.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTasksDataflow, "System.Threading.Tasks.Dataflow.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreDataMinerSystemCommon, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreInterAppCallsCommon, "Skyline.DataMiner.Core.InterAppCalls.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerUtilsSecureCoding, "Skyline.DataMiner.Utils.SecureCoding.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemBuffers, "System.Buffers.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemFormatsAsn1, "System.Formats.Asn1.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemMemory, "System.Memory.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemNumericsVectors, "System.Numerics.Vectors.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemRuntimeCompilerServicesUnsafe, "System.Runtime.CompilerServices.Unsafe.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemSecurityCryptographyPkcs, "System.Security.Cryptography.Pkcs.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemValueTuple, "System.ValueTuple.dll"), null, false)
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathAlphaFs, "AlphaFS.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathMicrosoftBclCryptography, "Microsoft.Bcl.Cryptography.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathFileSystem, "Skyline.DataMiner.CICD.FileSystem.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTasksDataflow, "System.Threading.Tasks.Dataflow.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreDataMinerSystemCommon, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreInterAppCallsCommon, "Skyline.DataMiner.Core.InterAppCalls.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerUtilsSecureCoding, "Skyline.DataMiner.Utils.SecureCoding.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemBuffers, "System.Buffers.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemFormatsAsn1, "System.Formats.Asn1.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemMemory, "System.Memory.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemNumericsVectors, "System.Numerics.Vectors.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemRuntimeCompilerServicesUnsafe, "System.Runtime.CompilerServices.Unsafe.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemSecurityCryptographyPkcs, "System.Security.Cryptography.Pkcs.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemValueTuple, "System.ValueTuple.dll"), null, false)
                },
                ProcessedAssemblies =
                {
                    "AlphaFS.dll",
                    "Microsoft.Bcl.Cryptography.dll",
                    "Newtonsoft.Json.dll",
                    "Skyline.DataMiner.CICD.FileSystem.dll",
                    "Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
                    "Skyline.DataMiner.Core.InterAppCalls.Common.dll",
                    "Skyline.DataMiner.Utils.SecureCoding.dll",
                    "System.Buffers.dll",
                    "System.Formats.Asn1.dll",
                    "System.Memory.dll",
                    "System.Numerics.Vectors.dll",
                    "System.Runtime.CompilerServices.Unsafe.dll",
                    "System.Security.Cryptography.Pkcs.dll",
                    "System.Threading.Tasks.Dataflow.dll",
                    "System.ValueTuple.dll",
                    // Framework assemblies:
                    "mscorlib.dll",
                    "System.dll",
                    "System.Transactions.dll",
                    "System.Numerics.dll",
                    "System.Security.dll"
                },
                DllImportDirectoryReferencesAssembly =
                {
                },
                DllImportDirectoryReferences =
                {
                },
                DllImportFrameworkAssemblyReferences =
                {
                    "mscorlib.dll",
                    "System.dll",
                    "System.Transactions.dll",
                    "System.Numerics.dll",
                    "System.Security.dll"
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(topLevelProjectBPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_SolutionLibraries_ProjectAReferencingHigherVersionOfSystemDiagnosticsSource()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            // Project A top level package references:
            //├─ Newtonsoft.Json 13.0.3
            //├─ Skyline.DataMiner.Core.DataMinerSystem.Common 1.1.3.8
            //│  ├─ Google.Protobuf 3.29.3
            //│  │  └─ System.Memory 4.5.3
            //│  │     ├─ System.Buffers 4.4.0
            //│  │     ├─ System.Numerics.Vectors 4.4.0
            //│  │     └─ System.Runtime.CompilerServices.Unsafe 4.5.2
            //│  ├─ Newtonsoft.Json 13.0.2
            //│  ├─ Serilog 4.0.0
            //│  │  ├─ System.Diagnostics.DiagnosticSource 8.0.1
            //│  │  │  ├─ System.Memory 4.5.5
            //│  │  │  │  ├─ System.Buffers 4.5.1
            //│  │  │  │  ├─ System.Numerics.Vectors 4.5.0
            //│  │  │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  │  │  └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            //│  │  └─ System.Threading.Channels 8.0.0
            //│  │     └─ System.Threading.Tasks.Extensions 4.5.4
            //│  │        └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  ├─ Serilog.Sinks.Async 2.0.0
            //│  │  └─ Serilog 4.0.0
            //│  │     ├─ System.Diagnostics.DiagnosticSource 8.0.1
            //│  │     │  ├─ System.Memory 4.5.5
            //│  │     │  │  ├─ System.Buffers 4.5.1
            //│  │     │  │  ├─ System.Numerics.Vectors 4.5.0
            //│  │     │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  │     │  └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            //│  │     └─ System.Threading.Channels 8.0.0
            //│  │        └─ System.Threading.Tasks.Extensions 4.5.4
            //│  │           └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  ├─ Serilog.Sinks.File 6.0.0
            //│  │  └─ Serilog 4.0.0
            //│  │     ├─ System.Diagnostics.DiagnosticSource 8.0.1
            //│  │     │  ├─ System.Memory 4.5.5
            //│  │     │  │  ├─ System.Buffers 4.5.1
            //│  │     │  │  ├─ System.Numerics.Vectors 4.5.0
            //│  │     │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  │     │  └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            //│  │     └─ System.Threading.Channels 8.0.0
            //│  │        └─ System.Threading.Tasks.Extensions 4.5.4
            //│  │           └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  ├─ System.Threading.Channels 8.0.0
            //│  │  └─ System.Threading.Tasks.Extensions 4.5.4
            //│  │     └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  └─ System.Threading.Tasks.Dataflow 7.0.0
            //└─ System.Diagnostics.DiagnosticSource 10.0.8
            //   ├─ System.Memory 4.6.3
            //   │  ├─ System.Buffers 4.6.1
            //   │  ├─ System.Numerics.Vectors 4.6.1
            //   │  └─ System.Runtime.CompilerServices.Unsafe 6.1.2
            //   └─ System.Runtime.CompilerServices.Unsafe 6.1.2
            //IList<PackageIdentity> topLevelProjectAPackages = new List<PackageIdentity>
            //{
            //    new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
            //    new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
            //    new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.3.8")),
            //    new PackageIdentity("System.Diagnostics.DiagnosticSource", new NuGetVersion("10.0.8")),
            //};

            // Project B top level package references:
            //├─ Newtonsoft.Json 13.0.4
            //└─ Skyline.DataMiner.Core.InterAppCalls.Common 1.1.1.1
            //   ├─ Skyline.DataMiner.Core.DataMinerSystem.Common 1.1.2.2
            //   │  ├─ Newtonsoft.Json 13.0.2
            //   │  └─ System.Threading.Tasks.Dataflow 7.0.0
            //   └─ Skyline.DataMiner.Utils.SecureCoding 2.2.1
            //      ├─ Newtonsoft.Json 13.0.3
            //      ├─ Skyline.DataMiner.CICD.FileSystem 1.1.0
            //      │  └─ AlphaFS.New 2.3.0
            //      └─ System.Security.Cryptography.Pkcs 9.0.2
            //         └─ Microsoft.Bcl.Cryptography 9.0.2
            //            ├─ System.Formats.Asn1 9.0.2
            //            │  ├─ System.Buffers 4.5.1
            //            │  ├─ System.Memory 4.5.5
            //            │  │  ├─ System.Buffers 4.5.1
            //            │  │  ├─ System.Numerics.Vectors 4.5.0
            //            │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //            │  └─ System.ValueTuple 4.5.0
            //            ├─ System.Memory 4.5.5
            //            │  ├─ System.Buffers 4.5.1
            //            │  ├─ System.Numerics.Vectors 4.5.0
            //            │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //            └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            IList<PackageIdentity> topLevelProjectBPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
                new PackageIdentity("Skyline.DataMiner.Core.InterAppCalls.Common", new NuGetVersion("1.1.1.1")),
            };

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
            {
                new PackageIdentity("AlphaFS.New", new NuGetVersion("2.3.0")),
                new PackageIdentity("Google.Protobuf", new NuGetVersion("3.29.3")),
                new PackageIdentity("Microsoft.Bcl.Cryptography", new NuGetVersion("9.0.2")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.2")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
                new PackageIdentity("Serilog", new NuGetVersion("4.0.0")),
                new PackageIdentity("Serilog.Sinks.Async", new NuGetVersion("2.0.0")),
                new PackageIdentity("Serilog.Sinks.File", new NuGetVersion("6.0.0")),
                new PackageIdentity("Skyline.DataMiner.CICD.FileSystem", new NuGetVersion("1.1.0")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.2.2")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.3.8")),
                new PackageIdentity("Skyline.DataMiner.Core.InterAppCalls.Common", new NuGetVersion("1.1.1.1")),
                new PackageIdentity("Skyline.DataMiner.Utils.SecureCoding", new NuGetVersion("2.2.1")),
                new PackageIdentity("System.Buffers", new NuGetVersion("4.4.0")),
                new PackageIdentity("System.Buffers", new NuGetVersion("4.5.1")),
                new PackageIdentity("System.Buffers", new NuGetVersion("4.6.1")),
                new PackageIdentity("System.Diagnostics.DiagnosticSource", new NuGetVersion("8.0.1")),
                new PackageIdentity("System.Diagnostics.DiagnosticSource", new NuGetVersion("10.0.8")),
                new PackageIdentity("System.Formats.Asn1", new NuGetVersion("9.0.2")),
                new PackageIdentity("System.Memory", new NuGetVersion("4.5.3")),
                new PackageIdentity("System.Memory", new NuGetVersion("4.5.5")),
                new PackageIdentity("System.Memory", new NuGetVersion("4.6.3")),
                new PackageIdentity("System.Numerics.Vectors", new NuGetVersion("4.4.0")),
                new PackageIdentity("System.Numerics.Vectors", new NuGetVersion("4.5.0")),
                new PackageIdentity("System.Numerics.Vectors", new NuGetVersion("4.6.1")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("4.5.2")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("4.5.3")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("6.0.0")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("6.1.2")),
                new PackageIdentity("System.Security.Cryptography.Pkcs", new NuGetVersion("9.0.2")),
                new PackageIdentity("System.Threading.Channels", new NuGetVersion("8.0.0")),
                new PackageIdentity("System.Threading.Tasks.Dataflow", new NuGetVersion("7.0.0")),
                new PackageIdentity("System.Threading.Tasks.Extensions", new NuGetVersion("4.5.4")),
                new PackageIdentity("System.ValueTuple", new NuGetVersion("4.5.0"))
            };

            using (var cacheContext = new SourceCacheContext())
            {
                cacheContext.MaxAge = DateTimeOffset.UtcNow;

                foreach (var package in solutionPackages)
                {
                    await packageReferenceProcessor.InstallPackageIfNotFound(package, cacheContext, CancellationToken.None);
                }
            }

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.8";

            const string pathAlphaFs = @"alphafs.new\2.3.0\lib\net47";
            const string pathGoogleProtobuf = @"google.protobuf\3.29.3\lib\net45";
            const string pathMicrosoftBclCryptography = @"microsoft.bcl.cryptography\9.0.2\lib\net462";
            const string pathNewtonsoftJson = @"newtonsoft.json\13.0.4\lib\net45";
            const string pathSerilog = @"serilog\4.0.0\lib\net471";
            const string pathSerilogSinksAsync = @"serilog.sinks.async\2.0.0\lib\net471";
            const string pathSerilogSinksFile = @"serilog.sinks.file\6.0.0\lib\net471";
            const string pathSkylineDataMinerCiCdFileSystem = @"skyline.dataminer.cicd.filesystem\1.1.0\lib\netstandard2.0";
            const string pathSkylineDataMinerCoreDataMinerSystemCommon = @"skyline.dataminer.core.dataminersystem.common\1.1.3.8\lib\net462";
            const string pathSkylineDataMinerCoreInterAppCallsCommon = @"skyline.dataminer.core.interappcalls.common\1.1.1.1\lib\net462";
            const string pathSecureCoding = @"skyline.dataminer.utils.securecoding\2.2.1\lib\netstandard2.0";
            const string pathSystemBuffers = @"system.buffers\4.6.1\lib\net462";
            const string pathSystemDiagnosticsDiagnosticSource = @"system.diagnostics.diagnosticsource\10.0.8\lib\net462";
            const string pathSystemFormatsAsn1 = @"system.formats.asn1\9.0.2\lib\net462";
            const string pathSystemMemory = @"system.memory\4.6.3\lib\net462";
            const string pathSystemNumericsVectors = @"system.numerics.vectors\4.6.1\lib\net462";
            const string pathSystemRuntimeCompilerServicesUnsafe = @"system.runtime.compilerservices.unsafe\6.1.2\lib\net462";
            const string pathSystemSecurityCryptographyPkcs = @"system.security.cryptography.pkcs\9.0.2\lib\net462";
            const string pathSystemThreadingChannels = @"system.threading.channels\8.0.0\lib\net462";
            const string pathSystemThreadingTasksDataflow = @"system.threading.tasks.dataflow\7.0.0\lib\net462";
            const string pathSystemThreadingTaskExtensions = @"system.threading.tasks.extensions\4.5.4\lib\net461";
            const string pathSystemValueTuple = @"system.valuetuple\4.5.0\lib\net47";

            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathAlphaFs,
                    pathGoogleProtobuf,
                    pathMicrosoftBclCryptography,
                    pathNewtonsoftJson,
                    pathSerilog,
                    pathSerilogSinksAsync,
                    pathSerilogSinksFile,
                    pathSkylineDataMinerCiCdFileSystem,
                    pathSkylineDataMinerCoreDataMinerSystemCommon,
                    pathSkylineDataMinerCoreInterAppCallsCommon,
                    pathSecureCoding,
                    pathSystemBuffers,
                    pathSystemDiagnosticsDiagnosticSource,
                    pathSystemFormatsAsn1,
                    pathSystemMemory,
                    pathSystemNumericsVectors,
                    pathSystemRuntimeCompilerServicesUnsafe,
                    pathSystemSecurityCryptographyPkcs,
                    pathSystemThreadingChannels,
                    pathSystemThreadingTasksDataflow,
                    pathSystemThreadingTaskExtensions,
                    pathSystemValueTuple,
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(Path.Combine(pathAlphaFs, "AlphaFS.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathGoogleProtobuf, "Google.Protobuf.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathMicrosoftBclCryptography, "Microsoft.Bcl.Cryptography.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNewtonsoftJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilog, "Serilog.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilogSinksAsync, "Serilog.Sinks.Async.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilogSinksFile, "Serilog.Sinks.File.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCiCdFileSystem, "Skyline.DataMiner.CICD.FileSystem.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreDataMinerSystemCommon, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreInterAppCallsCommon, "Skyline.DataMiner.Core.InterAppCalls.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSecureCoding, "Skyline.DataMiner.Utils.SecureCoding.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemBuffers, "System.Buffers.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemDiagnosticsDiagnosticSource, "System.Diagnostics.DiagnosticSource.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemFormatsAsn1, "System.Formats.Asn1.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemMemory, "System.Memory.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemNumericsVectors, "System.Numerics.Vectors.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemRuntimeCompilerServicesUnsafe, "System.Runtime.CompilerServices.Unsafe.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemSecurityCryptographyPkcs, "System.Security.Cryptography.Pkcs.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTasksDataflow, "System.Threading.Tasks.Dataflow.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTaskExtensions, "System.Threading.Tasks.Extensions.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingChannels, "System.Threading.Channels.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemValueTuple, "System.ValueTuple.dll"), null, false),
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathAlphaFs, "AlphaFS.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathGoogleProtobuf, "Google.Protobuf.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathMicrosoftBclCryptography, "Microsoft.Bcl.Cryptography.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNewtonsoftJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilog, "Serilog.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilogSinksAsync, "Serilog.Sinks.Async.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilogSinksFile, "Serilog.Sinks.File.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCiCdFileSystem, "Skyline.DataMiner.CICD.FileSystem.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreDataMinerSystemCommon, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreInterAppCallsCommon, "Skyline.DataMiner.Core.InterAppCalls.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSecureCoding, "Skyline.DataMiner.Utils.SecureCoding.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemBuffers, "System.Buffers.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemDiagnosticsDiagnosticSource, "System.Diagnostics.DiagnosticSource.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemFormatsAsn1, "System.Formats.Asn1.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemMemory, "System.Memory.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemNumericsVectors, "System.Numerics.Vectors.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemRuntimeCompilerServicesUnsafe, "System.Runtime.CompilerServices.Unsafe.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemSecurityCryptographyPkcs, "System.Security.Cryptography.Pkcs.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTasksDataflow, "System.Threading.Tasks.Dataflow.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTaskExtensions, "System.Threading.Tasks.Extensions.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingChannels, "System.Threading.Channels.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemValueTuple, "System.ValueTuple.dll"), null, false),
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "Google.Protobuf.dll",
                    "Serilog.dll",
                    "System.Diagnostics.DiagnosticSource.dll",
                    "System.Threading.Channels.dll",
                    "System.Memory.dll",
                    "System.Buffers.dll",
                    "System.Numerics.Vectors.dll",
                    "System.Runtime.CompilerServices.Unsafe.dll",
                    "Serilog.Sinks.Async.dll",
                    "Serilog.Sinks.File.dll",
                    "System.Threading.Tasks.Extensions.dll",
                    "System.Threading.Tasks.Dataflow.dll",
                    "Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
                    "Skyline.DataMiner.Utils.SecureCoding.dll",
                    "AlphaFS.dll",
                    "Skyline.DataMiner.CICD.FileSystem.dll",
                    "System.Formats.Asn1.dll",
                    "System.Security.Cryptography.Pkcs.dll",
                    "Microsoft.Bcl.Cryptography.dll",
                    "System.ValueTuple.dll",
                    // Framework assemblies
                    "mscorlib.dll",
                    "Microsoft.CSharp.dll",
                    "System.Core.dll",
                    "System.dll",
                    "System.Numerics.dll",
                    "System.Transactions.dll",
                    "System.Security.dll",
                    "Skyline.DataMiner.Core.InterAppCalls.Common.dll"
                },
                DllImportDirectoryReferencesAssembly =
                {
                },
                DllImportDirectoryReferences =
                {
                },
                DllImportFrameworkAssemblyReferences =
                {
                    "mscorlib.dll",
                    "Microsoft.CSharp.dll",
                    "System.Core.dll",
                    "System.dll",
                    "System.Numerics.dll",
                    "System.Transactions.dll",
                    "System.Security.dll"
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(topLevelProjectBPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, ExcludeAssemblyPath);
        }

        [TestMethod]
        public async Task ProcessAsyncTest_SolutionLibraries_ProjectAReferencingHigherVersionOfSerilogSinksFile()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            // Project A top level package references:
            //├─ Newtonsoft.Json 13.0.3
            //├─ Skyline.DataMiner.Core.DataMinerSystem.Common 1.1.3.8
            //│  ├─ Google.Protobuf 3.29.3
            //│  │  └─ System.Memory 4.5.3
            //│  │     ├─ System.Buffers 4.4.0
            //│  │     ├─ System.Numerics.Vectors 4.4.0
            //│  │     └─ System.Runtime.CompilerServices.Unsafe 4.5.2
            //│  ├─ Newtonsoft.Json 13.0.2
            //│  ├─ Serilog 4.0.0
            //│  │  ├─ System.Diagnostics.DiagnosticSource 8.0.1
            //│  │  │  ├─ System.Memory 4.5.5
            //│  │  │  │  ├─ System.Buffers 4.5.1
            //│  │  │  │  ├─ System.Numerics.Vectors 4.5.0
            //│  │  │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  │  │  └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            //│  │  └─ System.Threading.Channels 8.0.0
            //│  │     └─ System.Threading.Tasks.Extensions 4.5.4
            //│  │        └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  ├─ Serilog.Sinks.Async 2.0.0
            //│  │  └─ Serilog 4.0.0
            //│  │     ├─ System.Diagnostics.DiagnosticSource 8.0.1
            //│  │     │  ├─ System.Memory 4.5.5
            //│  │     │  │  ├─ System.Buffers 4.5.1
            //│  │     │  │  ├─ System.Numerics.Vectors 4.5.0
            //│  │     │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  │     │  └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            //│  │     └─ System.Threading.Channels 8.0.0
            //│  │        └─ System.Threading.Tasks.Extensions 4.5.4
            //│  │           └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  ├─ Serilog.Sinks.File 6.0.0
            //│  │  └─ Serilog 4.0.0
            //│  │     ├─ System.Diagnostics.DiagnosticSource 8.0.1
            //│  │     │  ├─ System.Memory 4.5.5
            //│  │     │  │  ├─ System.Buffers 4.5.1
            //│  │     │  │  ├─ System.Numerics.Vectors 4.5.0
            //│  │     │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  │     │  └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            //│  │     └─ System.Threading.Channels 8.0.0
            //│  │        └─ System.Threading.Tasks.Extensions 4.5.4
            //│  │           └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  ├─ System.Threading.Channels 8.0.0
            //│  │  └─ System.Threading.Tasks.Extensions 4.5.4
            //│  │     └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //│  └─ System.Threading.Tasks.Dataflow 7.0.0
            //└─ Serilog.Sinks.File 7.0.0
            //   └─ Serilog 4.2.0
            //      ├─ System.Diagnostics.DiagnosticSource 8.0.1
            //      │  ├─ System.Memory 4.5.5
            //      │  │  ├─ System.Buffers 4.5.1
            //      │  │  ├─ System.Numerics.Vectors 4.5.0
            //      │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //      │  └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            //      └─ System.Threading.Channels 8.0.0
            //         └─ System.Threading.Tasks.Extensions 4.5.4
            //            └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //IList<PackageIdentity> topLevelProjectAPackages = new List<PackageIdentity>
            //{
            //    new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
            //    new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
            //    new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.3.8")),
            //    new PackageIdentity("Serilog.Sinks.File", new NuGetVersion("7.0.0")),
            //};

            // Project B top level package references:
            //├─ Newtonsoft.Json 13.0.4
            //└─ Skyline.DataMiner.Core.InterAppCalls.Common 1.1.1.1
            //   ├─ Skyline.DataMiner.Core.DataMinerSystem.Common 1.1.2.2
            //   │  ├─ Newtonsoft.Json 13.0.2
            //   │  └─ System.Threading.Tasks.Dataflow 7.0.0
            //   └─ Skyline.DataMiner.Utils.SecureCoding 2.2.1
            //      ├─ Newtonsoft.Json 13.0.3
            //      ├─ Skyline.DataMiner.CICD.FileSystem 1.1.0
            //      │  └─ AlphaFS.New 2.3.0
            //      └─ System.Security.Cryptography.Pkcs 9.0.2
            //         └─ Microsoft.Bcl.Cryptography 9.0.2
            //            ├─ System.Formats.Asn1 9.0.2
            //            │  ├─ System.Buffers 4.5.1
            //            │  ├─ System.Memory 4.5.5
            //            │  │  ├─ System.Buffers 4.5.1
            //            │  │  ├─ System.Numerics.Vectors 4.5.0
            //            │  │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //            │  └─ System.ValueTuple 4.5.0
            //            ├─ System.Memory 4.5.5
            //            │  ├─ System.Buffers 4.5.1
            //            │  ├─ System.Numerics.Vectors 4.5.0
            //            │  └─ System.Runtime.CompilerServices.Unsafe 4.5.3
            //            └─ System.Runtime.CompilerServices.Unsafe 6.0.0
            IList<PackageIdentity> topLevelProjectBPackages = new List<PackageIdentity>
            {
                new PackageIdentity("Skyline.DataMiner.Dev.Automation", new NuGetVersion("10.3.5")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
                new PackageIdentity("Skyline.DataMiner.Core.InterAppCalls.Common", new NuGetVersion("1.1.1.1")),
            };

            IList<PackageIdentity> solutionPackages = new List<PackageIdentity>
            {
                new PackageIdentity("AlphaFS.New", new NuGetVersion("2.3.0")),
                new PackageIdentity("Google.Protobuf", new NuGetVersion("3.29.3")),
                new PackageIdentity("Microsoft.Bcl.Cryptography", new NuGetVersion("9.0.2")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.2")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.3")),
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion("13.0.4")),
                new PackageIdentity("Serilog", new NuGetVersion("4.0.0")),
                new PackageIdentity("Serilog", new NuGetVersion("4.2.0")),
                new PackageIdentity("Serilog.Sinks.Async", new NuGetVersion("2.0.0")),
                new PackageIdentity("Serilog.Sinks.File", new NuGetVersion("6.0.0")),
                new PackageIdentity("Serilog.Sinks.File", new NuGetVersion("7.0.0")),
                new PackageIdentity("Skyline.DataMiner.CICD.FileSystem", new NuGetVersion("1.1.0")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.2.2")),
                new PackageIdentity("Skyline.DataMiner.Core.DataMinerSystem.Common", new NuGetVersion("1.1.3.8")),
                new PackageIdentity("Skyline.DataMiner.Core.InterAppCalls.Common", new NuGetVersion("1.1.1.1")),
                new PackageIdentity("Skyline.DataMiner.Utils.SecureCoding", new NuGetVersion("2.2.1")),
                new PackageIdentity("System.Buffers", new NuGetVersion("4.4.0")),
                new PackageIdentity("System.Buffers", new NuGetVersion("4.5.1")),
                new PackageIdentity("System.Diagnostics.DiagnosticSource", new NuGetVersion("8.0.1")),
                new PackageIdentity("System.Formats.Asn1", new NuGetVersion("9.0.2")),
                new PackageIdentity("System.Memory", new NuGetVersion("4.5.3")),
                new PackageIdentity("System.Memory", new NuGetVersion("4.5.5")),
                new PackageIdentity("System.Numerics.Vectors", new NuGetVersion("4.4.0")),
                new PackageIdentity("System.Numerics.Vectors", new NuGetVersion("4.5.0")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("4.5.2")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("4.5.3")),
                new PackageIdentity("System.Runtime.CompilerServices.Unsafe", new NuGetVersion("6.0.0")),
                new PackageIdentity("System.Security.Cryptography.Pkcs", new NuGetVersion("9.0.2")),
                new PackageIdentity("System.Threading.Channels", new NuGetVersion("8.0.0")),
                new PackageIdentity("System.Threading.Tasks.Dataflow", new NuGetVersion("7.0.0")),
                new PackageIdentity("System.Threading.Tasks.Extensions", new NuGetVersion("4.5.4")),
                new PackageIdentity("System.ValueTuple", new NuGetVersion("4.5.0"))
            };

            using (var cacheContext = new SourceCacheContext())
            {
                cacheContext.MaxAge = DateTimeOffset.UtcNow;

                foreach (var package in solutionPackages)
                {
                    await packageReferenceProcessor.InstallPackageIfNotFound(package, cacheContext, CancellationToken.None);
                }
            }

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.8";

            const string pathAlphaFs = @"alphafs.new\2.3.0\lib\net47";
            const string pathGoogleProtobuf = @"google.protobuf\3.29.3\lib\net45";
            const string pathMicrosoftBclCryptography = @"microsoft.bcl.cryptography\9.0.2\lib\net462";
            const string pathNewtonsoftJson = @"newtonsoft.json\13.0.4\lib\net45";
            const string pathSerilog = @"serilog\4.2.0\lib\net471";
            const string pathSerilogSinksAsync = @"serilog.sinks.async\2.0.0\lib\net471";
            const string pathSerilogSinksFile = @"serilog.sinks.file\7.0.0\lib\net471";
            const string pathSkylineDataMinerCiCdFileSystem = @"skyline.dataminer.cicd.filesystem\1.1.0\lib\netstandard2.0";
            const string pathSkylineDataMinerCoreDataMinerSystemCommon = @"skyline.dataminer.core.dataminersystem.common\1.1.3.8\lib\net462";
            const string pathSkylineDataMinerCoreInterAppCallsCommon = @"skyline.dataminer.core.interappcalls.common\1.1.1.1\lib\net462";
            const string pathSkylineDataMinerUtilsSecureCoding = @"skyline.dataminer.utils.securecoding\2.2.1\lib\netstandard2.0";
            const string pathSystemBuffers = @"system.buffers\4.5.1\lib\net461";
            const string pathSystemDiagnosticsDiagnosticSource = @"system.diagnostics.diagnosticsource\8.0.1\lib\net462";
            const string pathSystemFormatsAsn1 = @"system.formats.asn1\9.0.2\lib\net462";
            const string pathSystemMemory = @"system.memory\4.5.5\lib\net461";
            const string pathSystemNumericsVectors = @"system.numerics.vectors\4.5.0\lib\net46";
            const string pathSystemRuntimeCompilerServicesUnsafe = @"system.runtime.compilerservices.unsafe\6.0.0\lib\net461";
            const string pathSystemSecurityCryptographyPkcs = @"system.security.cryptography.pkcs\9.0.2\lib\net462";
            const string pathSystemThreadingChannels = @"system.threading.channels\8.0.0\lib\net462";
            const string pathSystemThreadingTasksDataflow = @"system.threading.tasks.dataflow\7.0.0\lib\net462";
            const string pathSystemThreadingTaskExtensions = @"system.threading.tasks.extensions\4.5.4\lib\net461";
            const string pathSystemValueTuple = @"system.valuetuple\4.5.0\lib\net47";
            
            var expectedResult = new NuGetPackageAssemblyData
            {
                ImplicitDllImportDirectoryReferences =
                {
                    pathNewtonsoftJson,
                    pathGoogleProtobuf,
                    pathSerilog,
                    pathSystemDiagnosticsDiagnosticSource,
                    pathSystemThreadingChannels,
                    pathSystemMemory,
                    pathSystemBuffers,
                    pathSystemNumericsVectors,
                    pathSystemRuntimeCompilerServicesUnsafe,
                    pathSerilogSinksAsync,
                    pathSerilogSinksFile,
                    pathSystemThreadingTaskExtensions,
                    pathSystemThreadingTasksDataflow,
                    pathSkylineDataMinerCoreDataMinerSystemCommon,
                    pathSkylineDataMinerUtilsSecureCoding,
                    pathAlphaFs,
                    pathSkylineDataMinerCiCdFileSystem,
                    pathSystemFormatsAsn1,
                    pathMicrosoftBclCryptography,
                    pathSystemSecurityCryptographyPkcs,
                    pathSystemValueTuple,
                    pathSkylineDataMinerCoreInterAppCallsCommon
                },
                DllImportNugetAssemblyReferences =
                {
                    new PackageAssemblyReference(Path.Combine(pathAlphaFs, "AlphaFS.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNewtonsoftJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCiCdFileSystem, "Skyline.DataMiner.CICD.FileSystem.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemBuffers, "System.Buffers.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemNumericsVectors, "System.Numerics.Vectors.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemRuntimeCompilerServicesUnsafe, "System.Runtime.CompilerServices.Unsafe.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemMemory, "System.Memory.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathGoogleProtobuf, "Google.Protobuf.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemDiagnosticsDiagnosticSource, "System.Diagnostics.DiagnosticSource.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTasksDataflow, "System.Threading.Tasks.Dataflow.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTaskExtensions, "System.Threading.Tasks.Extensions.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingChannels, "System.Threading.Channels.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilog, "Serilog.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilogSinksAsync, "Serilog.Sinks.Async.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilogSinksFile, "Serilog.Sinks.File.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreDataMinerSystemCommon, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemValueTuple, "System.ValueTuple.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemFormatsAsn1, "System.Formats.Asn1.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathMicrosoftBclCryptography, "Microsoft.Bcl.Cryptography.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemSecurityCryptographyPkcs, "System.Security.Cryptography.Pkcs.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerUtilsSecureCoding, "Skyline.DataMiner.Utils.SecureCoding.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreInterAppCallsCommon, "Skyline.DataMiner.Core.InterAppCalls.Common.dll"), null, false)
                },
                NugetAssemblies =
                {
                    new PackageAssemblyReference(Path.Combine(pathAlphaFs, "AlphaFS.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathNewtonsoftJson, "Newtonsoft.Json.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCiCdFileSystem, "Skyline.DataMiner.CICD.FileSystem.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemBuffers, "System.Buffers.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemNumericsVectors, "System.Numerics.Vectors.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemRuntimeCompilerServicesUnsafe, "System.Runtime.CompilerServices.Unsafe.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemMemory, "System.Memory.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathGoogleProtobuf, "Google.Protobuf.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemDiagnosticsDiagnosticSource, "System.Diagnostics.DiagnosticSource.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTasksDataflow, "System.Threading.Tasks.Dataflow.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingTaskExtensions, "System.Threading.Tasks.Extensions.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemThreadingChannels, "System.Threading.Channels.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilog, "Serilog.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilogSinksAsync, "Serilog.Sinks.Async.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSerilogSinksFile, "Serilog.Sinks.File.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreDataMinerSystemCommon, "Skyline.DataMiner.Core.DataMinerSystem.Common.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemValueTuple, "System.ValueTuple.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemFormatsAsn1, "System.Formats.Asn1.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathMicrosoftBclCryptography, "Microsoft.Bcl.Cryptography.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSystemSecurityCryptographyPkcs, "System.Security.Cryptography.Pkcs.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerUtilsSecureCoding, "Skyline.DataMiner.Utils.SecureCoding.dll"), null, false),
                    new PackageAssemblyReference(Path.Combine(pathSkylineDataMinerCoreInterAppCallsCommon, "Skyline.DataMiner.Core.InterAppCalls.Common.dll"), null, false)
                },
                ProcessedAssemblies =
                {
                    "Newtonsoft.Json.dll",
                    "Google.Protobuf.dll",
                    "Serilog.dll",
                    "System.Diagnostics.DiagnosticSource.dll",
                    "System.Threading.Channels.dll",
                    "System.Memory.dll",
                    "System.Buffers.dll",
                    "System.Numerics.Vectors.dll",
                    "System.Runtime.CompilerServices.Unsafe.dll",
                    "Serilog.Sinks.Async.dll",
                    "Serilog.Sinks.File.dll",
                    "System.Threading.Tasks.Extensions.dll",
                    "System.Threading.Tasks.Dataflow.dll",
                    "Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
                    "Skyline.DataMiner.Utils.SecureCoding.dll",
                    "AlphaFS.dll",
                    "Skyline.DataMiner.CICD.FileSystem.dll",
                    "System.Formats.Asn1.dll",
                    "System.Security.Cryptography.Pkcs.dll",
                    "Microsoft.Bcl.Cryptography.dll",
                    "System.ValueTuple.dll",
                    // Framework assemblies
                    "mscorlib.dll",
                    "Microsoft.CSharp.dll",
                    "System.Core.dll",
                    "System.dll",
                    "System.Numerics.dll",
                    "System.Transactions.dll",
                    "System.Security.dll",
                    "Skyline.DataMiner.Core.InterAppCalls.Common.dll"
                },
                DllImportDirectoryReferencesAssembly =
                {
                },
                DllImportDirectoryReferences =
                {
                },
                DllImportFrameworkAssemblyReferences =
                {
                    "mscorlib.dll",
                    "Microsoft.CSharp.dll",
                    "System.Core.dll",
                    "System.dll",
                    "System.Numerics.dll",
                    "System.Transactions.dll",
                    "System.Security.dll"
                }
            };

            // Act
            var result = await packageReferenceProcessor.ProcessAsync(topLevelProjectBPackages, solutionPackages, targetFrameworkMoniker, DevPackHelper.AutomationDevPackNuGetDependenciesIncludingTransitive);

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