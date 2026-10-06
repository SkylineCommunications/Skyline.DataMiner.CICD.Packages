namespace Skyline.DataMiner.CICD.Assemblers.Common.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Packages.TestHelpers;

    [TestClass]
    [DoNotParallelize]
    public class BuildOnlyPackageReferenceProcessorTests
    {
        [TestMethod]
        [DataRow("build/Fixture.BuildOnly.targets", false)]
        [DataRow("build/Fixture.BuildOnly.targets", true)]
        [DataRow("analyzers/dotnet/cs/Fixture.Generator.dll", false)]
        [DataRow("analyzers/dotnet/cs/Fixture.Generator.dll", true)]
        [DataRow("lib/net48/_._", false)]
        [DataRow("lib/net48/_._", true)]
        [DataRow("ref/net48/_._", false)]
        [DataRow("ref/net48/_._", true)]
        [DataRow("runtimes/win-x64/native/Fixture.Native.dll", false)]
        [DataRow("runtimes/win-x64/native/Fixture.Native.dll", true)]
        [DataRow("lib/net10.0/Fixture.BuildOnly.dll", false)]
        [DataRow("lib/net10.0/Fixture.BuildOnly.dll", true)]
        public async Task ProcessAsync_SelectedPackageWithoutApplicableAssemblies_DoesNotRestoreDiscardedVersions(string selectedAsset, bool populatedCache)
        {
            using (var fixture = new BuildOnlyPackageFixture(selectedAsset))
            {
                if (populatedCache)
                {
                    await fixture.PopulateCacheAsync();
                }
                else
                {
                    Directory.GetFileSystemEntries(fixture.CachePath).Should().BeEmpty();
                }

                var processor = new PackageReferenceProcessor(fixture.RootPath);
                processor.NuGetRootPath.TrimEnd(Path.DirectorySeparatorChar).Should().Be(fixture.CachePath);

                var result = await processor.ProcessAsync(fixture.ProjectPackages, BuildOnlyPackageFixture.TargetFrameworkMoniker);

                result.DllImportNugetAssemblyReferences.Should().NotContain(reference => IsBuildOnlyReference(reference.DllImport));
                result.DllImportDirectoryReferences.Should().NotContain(reference => IsBuildOnlyReference(reference));
                result.DllImportDirectoryReferencesAssembly.Should().NotContain(reference => IsBuildOnlyReference(reference.Key) || IsBuildOnlyReference(reference.Value));
                result.ImplicitDllImportDirectoryReferences.Should().NotContain(reference => IsBuildOnlyReference(reference));
                result.NugetAssemblies.Should().NotContain(reference => IsBuildOnlyReference(reference.DllImport));
                result.ProcessedAssemblies.Should().NotContain("Fixture.Generator.dll");

                result.DllImportNugetAssemblyReferences.Select(reference => reference.DllImport).Should().BeEquivalentTo(new[]
                {
                    @"fixture.left\1.0.0\lib\net48\Fixture.Left.dll",
                    @"fixture.right\1.0.0\lib\net48\Fixture.Right.dll",
                    @"fixture.runtime\1.0.0\lib\net48\Fixture.Runtime.dll",
                });
                result.NugetAssemblies.Select(reference => reference.DllImport).Should().BeEquivalentTo(
                    result.DllImportNugetAssemblyReferences.Select(reference => reference.DllImport));
                result.NugetAssemblies.Should().OnlyContain(reference => File.Exists(reference.AssemblyPath));
            }
        }

        [TestMethod]
        [DataRow("lib/net48/Fixture.BuildOnly.dll")]
        [DataRow("ref/net48/Fixture.BuildOnly.dll")]
        [DataRow("runtimes/win-x64/lib/net48/Fixture.BuildOnly.dll")]
        public async Task ProcessAsync_SelectedPackageWithApplicableAssemblies_PreservesLegacyDiscardedVersionHints(string selectedAsset)
        {
            using (var fixture = new BuildOnlyPackageFixture(selectedAsset))
            {
                var processor = new PackageReferenceProcessor(fixture.RootPath);

                var result = await processor.ProcessAsync(fixture.ProjectPackages, BuildOnlyPackageFixture.TargetFrameworkMoniker);

                result.DllImportDirectoryReferences.Should().Contain(new[]
                {
                    @"fixture.buildonly\1.0.0\lib\net48\",
                    @"fixture.buildonly\1.1.0-beta\lib\net48\",
                });
                result.DllImportDirectoryReferencesAssembly.Values.Should().Contain(new[]
                {
                    @"fixture.buildonly\1.0.0\lib\net48\Fixture.BuildOnly.dll",
                    @"fixture.buildonly\1.1.0-beta\lib\net48\Fixture.BuildOnly.dll",
                });
                result.NugetAssemblies.Select(reference => reference.DllImport).Should().Contain(new[]
                {
                    @"fixture.buildonly\1.0.0\lib\net48\Fixture.BuildOnly.dll",
                    @"fixture.buildonly\1.1.0-beta\lib\net48\Fixture.BuildOnly.dll",
                });

                if (selectedAsset.StartsWith("lib/", StringComparison.Ordinal))
                {
                    result.DllImportNugetAssemblyReferences.Select(reference => reference.DllImport).Should().Contain(
                        @"fixture.buildonly\2.0.0\lib\net48\Fixture.BuildOnly.dll");
                }
            }
        }

        [TestMethod]
        public async Task ProcessAsync_FreshAndPopulatedCaches_ProduceTheSameReferencesAndPayloadIdentities()
        {
            string[] fresh;
            using (var fixture = new BuildOnlyPackageFixture())
            {
                var processor = new PackageReferenceProcessor(fixture.RootPath);
                fresh = Normalize(await processor.ProcessAsync(fixture.ProjectPackages, BuildOnlyPackageFixture.TargetFrameworkMoniker));
            }

            using (var fixture = new BuildOnlyPackageFixture())
            {
                await fixture.PopulateCacheAsync();
                var processor = new PackageReferenceProcessor(fixture.RootPath);
                var populated = Normalize(await processor.ProcessAsync(fixture.ProjectPackages, BuildOnlyPackageFixture.TargetFrameworkMoniker));

                populated.Should().BeEquivalentTo(fresh);
                populated.Should().NotContain(reference => IsBuildOnlyReference(reference));
            }
        }

        [TestMethod]
        public async Task ProcessAsync_UnreadableSelectedPackageMetadata_IsAnErrorNotAnEmptyAssetGroup()
        {
            using (var fixture = new BuildOnlyPackageFixture())
            {
                await fixture.PopulateCacheAsync();
                string metadata = Path.Combine(fixture.CachePath, "fixture.buildonly", BuildOnlyPackageFixture.SelectedVersion, "fixture.buildonly.nuspec");
                File.WriteAllText(metadata, "<package>");
                var processor = new PackageReferenceProcessor(fixture.RootPath);

                Func<Task> process = () => processor.ProcessAsync(fixture.ProjectPackages, BuildOnlyPackageFixture.TargetFrameworkMoniker);

                var exception = await process.Should().ThrowAsync<NuGet.Protocol.Core.Types.FatalProtocolException>();
                exception.Which.InnerException.Should().BeOfType<System.Xml.XmlException>();
            }
        }

        private static bool IsBuildOnlyReference(string reference)
        {
            return reference.IndexOf(BuildOnlyPackageFixture.PackageId, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string[] Normalize(NuGetPackageAssemblyData data)
        {
            return data.DllImportNugetAssemblyReferences.Select(reference => "explicit:" + reference.DllImport)
                .Concat(data.DllImportDirectoryReferences.Select(reference => "hint:" + reference))
                .Concat(data.DllImportDirectoryReferencesAssembly.Select(reference => "hint-assembly:" + reference.Key + ":" + reference.Value))
                .Concat(data.ImplicitDllImportDirectoryReferences.Select(reference => "implicit:" + reference))
                .Concat(data.NugetAssemblies.Select(reference => "payload:" + reference.DllImport))
                .Concat(data.DllImportFrameworkAssemblyReferences.Select(reference => "framework:" + reference))
                .Concat(data.ProcessedAssemblies.Select(reference => "processed:" + reference))
                .OrderBy(reference => reference, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
