namespace Assemblers.ProtocolTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NuGet.Packaging.Core;
    using NuGet.Versioning;

    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.Assemblers.Protocol;
    using Skyline.DataMiner.CICD.Loggers;

    [TestClass]
    public class AssemblyFilterTests
    {
        [TestMethod]
        public async Task ProcessAsyncTest_DuplicateFrameworkScenario()
        {
            // Arrange
            var packageReferenceProcessor = new PackageReferenceProcessor(directoryForNuGetConfig: null);

            IList<PackageIdentity> projectPackages = new List<PackageIdentity>
            {
                new PackageIdentity("microsoft.extensions.http", new NuGetVersion("6.0.0")),
                new PackageIdentity("nats.client", new NuGetVersion("1.1.0")),
            };

            const string targetFrameworkMoniker = ".NETFramework,Version=v4.6.2";

            BuildResultItems buildResultItems = new BuildResultItems();
            HashSet<string> dllImports = new HashSet<string>();

            // Act
            var result = await AssemblyFilter.FilterAsync(targetFrameworkMoniker, packageReferenceProcessor, buildResultItems, dllImports,
                projectPackages);

            // Assert
            // Make sure there is only one System.Net.Http.dll
            // NuGetAssemblies and NetFramework cannot both have System.Net.Http which causes incorrect behavior down the line.
            // BuildResultItems
            //  AssemblyPath: c:\Users\{user}\.nuget\packages\system.net.http\4.3.4\lib\net46\System.Net.Http.dll
            //  dllImports: system.net.http\4.3.4\lib\net46\System.Net.Http.dll
            // dllImports hashset:
            //  System.Net.Http.dll

            result.Should().NotBeNull();
            // dllImports needs priority
            dllImports.Should().Contain("System.Net.Http.dll");
            // buildResultItems should not have system.net.http
            // dllImports should not have duplicate system.net.http
            dllImports.Should().NotContain(@"system.net.http\4.3.4\lib\net46\System.Net.Http.dll");

            // dllImports should still have added the folder, in order to access other dll's from the same nuget
            dllImports.Should().Contain(@"system.net.http\4.3.4\lib\net46\");

            var unexpectedPackageReference =
                new PackageAssemblyReference(@"system.net.http\4.3.4\lib\net46\System.Net.Http.dll", String.Empty, false);
            buildResultItems.Assemblies.Should().NotContainEquivalentOf(unexpectedPackageReference,
                options => options.Excluding(reference => reference.AssemblyPath));

            // Best effort to save time.
            dllImports.Count.Should().Be(26);
            buildResultItems.Assemblies.Count.Should().Be(22);
        }
    }
}