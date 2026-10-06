namespace Skyline.DataMiner.CICD.Packages.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Packaging.Signing;
    using NuGet.Protocol;
    using NuGet.Versioning;

    public sealed class BuildOnlyPackageFixture : IDisposable
    {
        public const string PackageId = "Fixture.BuildOnly";
        public const string SelectedVersion = "2.0.0";
        public const string RuntimePackageId = "Fixture.Runtime";
        public const string TargetFrameworkMoniker = ".NETFramework,Version=v4.8";

        private readonly List<PackageIdentity> packages = new();
        private readonly string? previousPackagesPath;

        public BuildOnlyPackageFixture(string selectedAsset = "build/Fixture.BuildOnly.targets")
        {
            RootPath = Path.Combine(Path.GetTempPath(), "DataMiner.BuildOnlyTests", Guid.NewGuid().ToString("N"));
            FeedPath = Path.Combine(RootPath, "feed");
            CachePath = Path.Combine(RootPath, "cache");
            Directory.CreateDirectory(FeedPath);
            Directory.CreateDirectory(CachePath);

            var config = new XDocument(
                new XElement("configuration",
                    new XElement("packageSources",
                        new XElement("clear"),
                        new XElement("add", new XAttribute("key", "fixture"), new XAttribute("value", FeedPath))),
                    new XElement("packageSourceMapping", new XElement("clear")),
                    new XElement("config",
                        new XElement("add", new XAttribute("key", "globalPackagesFolder"), new XAttribute("value", CachePath)))));
            config.Save(Path.Combine(RootPath, "NuGet.Config"));

            CreatePackage(PackageId, "1.0.0", "lib/net48/Fixture.BuildOnly.dll");
            CreatePackage(PackageId, "1.1.0-beta", "lib/net48/Fixture.BuildOnly.dll");
            CreatePackage(RuntimePackageId, "1.0.0", "lib/net48/Fixture.Runtime.dll");
            CreatePackage(PackageId, SelectedVersion, selectedAsset, (RuntimePackageId, "1.0.0"));
            CreatePackage("Fixture.Left", "1.0.0", "lib/net48/Fixture.Left.dll", ("fixture.buildonly", "1.0.0"));
            CreatePackage("Fixture.Right", "1.0.0", "lib/net48/Fixture.Right.dll", ("FIXTURE.BUILDONLY", "1.1.0-beta"));

            previousPackagesPath = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            Environment.SetEnvironmentVariable("NUGET_PACKAGES", CachePath);
        }

        public string RootPath { get; }

        public string FeedPath { get; }

        public string CachePath { get; }

        public IList<PackageIdentity> ProjectPackages => new List<PackageIdentity>
        {
            new PackageIdentity("fIxTuRe.BuIlDoNlY", NuGetVersion.Parse(SelectedVersion)),
            new PackageIdentity("Fixture.Left", NuGetVersion.Parse("1.0.0")),
            new PackageIdentity("Fixture.Right", NuGetVersion.Parse("1.0.0")),
        };

        public async Task PopulateCacheAsync()
        {
            var settings = Settings.LoadDefaultSettings(RootPath);
            var policy = ClientPolicyContext.GetClientPolicy(settings, NullLogger.Instance);

            foreach (var package in packages)
            {
                using var stream = File.OpenRead(Path.Combine(FeedPath, $"{package.Id}.{package.Version}.nupkg"));
                using var result = await GlobalPackagesFolderUtility.AddPackageAsync(
                    FeedPath, package, stream, CachePath, Guid.Empty, policy, NullLogger.Instance, CancellationToken.None).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("NUGET_PACKAGES", previousPackagesPath);
            Directory.Delete(RootPath, recursive: true);
        }

        private void CreatePackage(string id, string version, string asset, params (string Id, string Version)[] dependencies)
        {
            var package = new PackageBuilder
            {
                Id = id,
                Version = NuGetVersion.Parse(version),
                Description = "Isolated build-only dependency regression fixture.",
            };
            package.Authors.Add("SkylineCommunications");

            if (dependencies.Length > 0)
            {
                package.DependencyGroups.Add(new PackageDependencyGroup(
                    NuGetFramework.ParseFolder("net48"),
                    dependencies.Select(dependency => new PackageDependency(dependency.Id, VersionRange.Parse(dependency.Version)))));
            }

            string sourcePath = Path.Combine(RootPath, "source", id, version, asset.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            if (asset.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(Assembly.GetExecutingAssembly().Location, sourcePath);
            }
            else
            {
                File.WriteAllText(sourcePath, asset.EndsWith(".targets", StringComparison.OrdinalIgnoreCase) ? "<Project />" : String.Empty);
            }

            package.Files.Add(new PhysicalPackageFile { SourcePath = sourcePath, TargetPath = asset });
            using var stream = File.Create(Path.Combine(FeedPath, $"{id}.{version}.nupkg"));
            package.Save(stream);
            packages.Add(new PackageIdentity(id, package.Version));
        }
    }
}
