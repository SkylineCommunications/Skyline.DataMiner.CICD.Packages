namespace Skyline.DataMiner.CICD.DMApp.Protocol
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.DMProtocol;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    using static Skyline.AppInstaller.AppPackage;

    /// <summary>
    /// Represents a creator for application packages specifically for Protocols within the DataMiner System.
    /// </summary>
    public class AppPackageCreatorForProtocol : AppPackageCreator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppPackageCreatorForProtocol"/> class.
        /// </summary>
        /// <param name="fileSystem">File system interface to manage file operations.</param>
        /// <param name="logCollector">Log collector to capture logs during operations.</param>
        /// <param name="directoryPath">The directory path where packages are to be created.</param>
        /// <param name="packageName">The name of the package.</param>
        /// <param name="packageVersion">The version of the package.</param>
        public AppPackageCreatorForProtocol(IFileSystem fileSystem, ILogCollector logCollector, string directoryPath, string packageName, DMAppVersion packageVersion) : base(fileSystem, logCollector, directoryPath, packageName, packageVersion)
        {
        }

        /// <summary>
        /// Asynchronously adds items to the application package.
        /// </summary>
        /// <param name="appPackageBuilder">The application package builder to which the items are added.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task AddItemsAsync(AppPackageBuilder appPackageBuilder)
        {
            if (appPackageBuilder == null) throw new ArgumentNullException(nameof(appPackageBuilder));

            // Check the content of Keystones folder
            // Check the bin/release folders, if nothing else is there, then we can try to build those projects in release mode.

            string pathToKeystonesInSolution = FileSystem.Path.Combine(RepositoryPath, "Keystones");

            if (FileSystem.Directory.Exists(pathToKeystonesInSolution))
            {
                foreach (var keystonePath in FileSystem.Directory.EnumerateDirectories(pathToKeystonesInSolution))
                {
                    // Requires the solution to be pre-build for now

                    string pathToKeystoneRelease = FileSystem.Path.Combine(keystonePath, "bin", "Release");
                    string pathToKeystoneDebug = FileSystem.Path.Combine(keystonePath, "bin", "Debug");

                    string validPath;
                    if (FileSystem.Directory.Exists(pathToKeystoneRelease))
                    {
                        validPath = pathToKeystoneRelease;

                    }
                    else if (FileSystem.Directory.Exists(pathToKeystoneDebug))
                    {
                        validPath = pathToKeystoneDebug;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Error {keystonePath}: A connector solution with keystones must be compiled using the default output path into the bin folder.");
                    }

                    var keystones = FileSystem.Directory.EnumerateFiles(validPath, "*.nupkg", System.IO.SearchOption.TopDirectoryOnly);

                    // TODO: grab highest version
                    foreach (var keystone in keystones)
                    {
                        // Workaround quickly
                        appPackageBuilder.WithKeystone(keystone);
                    }                
                }
            }

            IAppPackageProtocol protocolPackage;


            protocolPackage = await ProtocolPackageCreator.Factory.FromRepositoryAsync(LogCollector, RepositoryPath);

            Console.WriteLine($"Creating dmapp from connector solution {RepositoryPath}");

            Console.WriteLine($"ProtocolPath: {protocolPackage.ProtocolPath}");
            Console.WriteLine($"Name: {protocolPackage.Name}");
            Console.WriteLine($"Version: {protocolPackage.Version}");
            Console.WriteLine($"ProtocolContent length: {protocolPackage.ProtocolContent.Length}");
            Console.WriteLine($".Assemblies.Count: {protocolPackage.Assemblies.Count}");

            appPackageBuilder.WithProtocol(protocolPackage);
        }

        /// <summary>
        /// Contains methods to facilitate the creation of <see cref="AppPackageCreatorForProtocol"/> instances.
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// Creates an instance of <see cref="AppPackageCreatorForProtocol"/> from a repository with specified parameters.
            /// </summary>
            /// <param name="filesystem">File system interface to manage file operations.</param>
            /// <param name="log">Log collector to capture logs during operations.</param>
            /// <param name="directoryPath">The directory path where packages are to be created.</param>
            /// <param name="packageName">The name of the package.</param>
            /// <param name="packageVersion">The version of the package.</param>
            /// <returns>Returns a new instance of <see cref="AppPackageCreatorForProtocol"/>.</returns>
            public static IAppPackageCreator FromRepository(IFileSystem filesystem, ILogCollector log, string directoryPath, string packageName, DMAppVersion packageVersion)
            {
                return new AppPackageCreatorForProtocol(filesystem, log, directoryPath, packageName, packageVersion);
            }
        }
    }
}