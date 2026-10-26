namespace Assemblers.AutomationTests
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Automation;
    using Skyline.DataMiner.CICD.Assemblers.Common;

    [TestClass]
    public class ReferenceProjectEvaluationCacheTests
    {
        [TestMethod]
        [DataRow(null)]
        [DataRow("AutomationScriptLibrary")]
        public void Evaluate_RepeatedReference_EvaluatesOnceIncludingNonHarvestableProjects(string dataMinerType)
        {
            int evaluations = 0;
            var cache = new ReferenceProjectEvaluationCache((path, framework, configuration) =>
            {
                evaluations++;
                return new ReferencedProjectInfo(path)
                {
                    OutputType = "Library",
                    DataMinerType = dataMinerType,
                };
            });
            string path = Path.Combine(Path.GetTempPath(), "HarvestCacheLibrary.csproj");

            var first = cache.Evaluate(path, "net48", "Debug");
            var repeated = cache.Evaluate(path, "net48", "Debug");

            Assert.AreEqual(1, evaluations);
            Assert.AreSame(first, repeated);
            Assert.AreEqual(string.IsNullOrEmpty(dataMinerType), repeated.ShouldHarvestAssembly());
        }

        [TestMethod]
        public void Evaluate_DifferentFrameworkConfigurationOrPath_EvaluatesSeparately()
        {
            int evaluations = 0;
            var cache = new ReferenceProjectEvaluationCache((path, framework, configuration) =>
            {
                evaluations++;
                return new ReferencedProjectInfo(path)
                {
                    TargetFramework = framework,
                    Configuration = configuration,
                };
            });
            string path = Path.Combine(Path.GetTempPath(), "HarvestCacheLibrary.csproj");

            cache.Evaluate(path, "net48", "Debug");
            var otherFramework = cache.Evaluate(path, "netstandard2.0", "Debug");
            var otherConfiguration = cache.Evaluate(path, "net48", "Release");
            cache.Evaluate(Path.Combine(Path.GetTempPath(), "OtherHarvestCacheLibrary.csproj"), "net48", "Debug");

            Assert.AreEqual(4, evaluations);
            Assert.AreEqual("netstandard2.0", otherFramework.TargetFramework);
            Assert.AreEqual("Release", otherConfiguration.Configuration);
        }

        [TestMethod]
        public void Clear_NextBuild_ReevaluatesReferences()
        {
            int evaluations = 0;
            var cache = new ReferenceProjectEvaluationCache((path, framework, configuration) =>
            {
                evaluations++;
                return new ReferencedProjectInfo(path);
            });
            string path = Path.Combine(Path.GetTempPath(), "HarvestCacheLibrary.csproj");
            var firstBuild = cache.Evaluate(path, "net48", "Debug");

            cache.Clear();
            var nextBuild = cache.Evaluate(path, "net48", "Debug");

            Assert.AreEqual(2, evaluations);
            Assert.AreNotSame(firstBuild, nextBuild);
        }

        [TestMethod]
        public void Evaluate_RealLibraryConfigurations_PreservesConfigurationSpecificOutputs()
        {
            string directory = Path.Combine(Path.GetTempPath(), "HarvestCache-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                string path = Path.Combine(directory, "Library.csproj");
                File.WriteAllText(path, """
                    <Project Sdk="Microsoft.NET.Sdk">
                      <PropertyGroup>
                        <TargetFramework>net48</TargetFramework>
                        <AssemblyName Condition="'$(Configuration)' == 'Debug'">DebugLibrary</AssemblyName>
                        <AssemblyName Condition="'$(Configuration)' == 'Release'">ReleaseLibrary</AssemblyName>
                      </PropertyGroup>
                    </Project>
                    """);
                var cache = new ReferenceProjectEvaluationCache();

                var debug = cache.Evaluate(path, "net48", "Debug");
                var release = cache.Evaluate(path, "net48", "Release");

                Assert.IsTrue(debug.ShouldHarvestAssembly());
                Assert.IsTrue(release.ShouldHarvestAssembly());
                StringAssert.EndsWith(debug.TargetPath, Path.Combine("Debug", "net48", "DebugLibrary.dll"));
                StringAssert.EndsWith(release.TargetPath, Path.Combine("Release", "net48", "ReleaseLibrary.dll"));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
