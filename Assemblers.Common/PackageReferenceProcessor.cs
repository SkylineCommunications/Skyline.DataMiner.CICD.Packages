namespace Skyline.DataMiner.CICD.Assemblers.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Packaging.Signing;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGet.Resolver;
    using NuGet.Versioning;

    using Skyline.DataMiner.CICD.Common.NuGet;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    /// <summary>
    /// Determines all references required for a specific NuGet package.
    /// </summary>
    public class PackageReferenceProcessor
    {
        private readonly ILogCollector logCollector;
        private readonly ISettings settings;
        private readonly ILogger nuGetLogger;
        private readonly ICollection<SourceRepository> repositories;
        private readonly SourceRepository rootRepository;

        // V3 package path resolver
        private readonly VersionFolderPathResolver versionFolderPathResolver;
        private readonly FrameworkReducer frameworkReducer;
        private readonly SourceRepositoryProvider sourceRepositoryProvider;
        private readonly ClientPolicyContext clientPolicyContext;

        private readonly IFileSystem _fileSystem = FileSystem.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageReferenceProcessor"/> class.
        /// </summary>
        [Obsolete("Use the constructor with the directory path to be able to read out the nuget.config.")]
        public PackageReferenceProcessor() : this(directoryForNuGetConfig: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageReferenceProcessor"/> class.
        /// </summary>
        /// <param name="directoryForNuGetConfig">Directory path (to find the NuGet.config)</param>
        public PackageReferenceProcessor(string directoryForNuGetConfig)
        {
            nuGetLogger = NullLogger.Instance;

            // Start with the lowest settings. It will automatically look at the other NuGet.config files it can find on the default locations
            settings = Settings.LoadDefaultSettings(root: directoryForNuGetConfig);

            clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, nuGetLogger);

            var provider = new PackageSourceProvider(settings);
            sourceRepositoryProvider = new SourceRepositoryProvider(provider, Repository.Provider.GetCoreV3());

            NuGetRootPath = SettingsUtility.GetGlobalPackagesFolder(settings);

            // Add global packages to be the first repository as it speeds up everything when reading from disk then via internet.
            var repos = sourceRepositoryProvider.GetRepositories().ToList();
            rootRepository = new SourceRepository(new PackageSource(NuGetRootPath), Repository.Provider.GetCoreV3());
            repos.Insert(0, rootRepository);
            repositories = repos;

            // https://docs.microsoft.com/en-us/nuget/consume-packages/managing-the-global-packages-and-cache-folders
            versionFolderPathResolver = new VersionFolderPathResolver(NuGetRootPath);

            frameworkReducer = new FrameworkReducer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageReferenceProcessor"/> class.
        /// </summary>
        /// <param name="logCollector">The log collector.</param>
        /// <exception cref="ArgumentNullException"><paramref name="logCollector"/> is <see langword="null"/>.</exception>
        [Obsolete("Use the constructor with the solution directory path to be able to read out the nuget.config from the solution.")]
        public PackageReferenceProcessor(ILogCollector logCollector) : this(logCollector, directoryForNuGetConfig: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageReferenceProcessor"/> class.
        /// </summary>
        /// <param name="logCollector">The log collector.</param>
        /// <param name="directoryForNuGetConfig">Directory path (to find the NuGet.config)</param>
        /// <exception cref="ArgumentNullException"><paramref name="logCollector"/> is <see langword="null"/>.</exception>
        public PackageReferenceProcessor(ILogCollector logCollector, string directoryForNuGetConfig) : this(directoryForNuGetConfig)
        {
            this.logCollector = logCollector ?? throw new ArgumentNullException(nameof(logCollector));

            foreach (SourceRepository sourceRepository in repositories)
            {
                LogDebug($"Source: [{sourceRepository.PackageSource?.Name}] {sourceRepository.PackageSource?.Source}");
            }

            LogDebug($"NuGet Root Path: {NuGetRootPath}");
        }

        /// <summary>
        /// Gets the NuGet root path.
        /// </summary>
        /// <value>The NuGet root path.</value>
        public string NuGetRootPath { get; }

        /// <summary>
        /// Processes the NuGet packages.
        /// </summary>
        /// <param name="projectPackages">The NuGet packages referenced by the project.</param>
        /// <param name="targetFrameworkMoniker">The target framework moniker.</param>
        /// <returns>The assembly info of the processed packages.</returns>
        /// <exception cref="InvalidOperationException">Cannot find the package with the identity.</exception>
        public async Task<NuGetPackageAssemblyData> ProcessAsync(IList<PackageIdentity> projectPackages, string targetFrameworkMoniker)
        {
            return await ProcessAsync(projectPackages, targetFrameworkMoniker, new List<string>());
        }

        /// <summary>
        /// Processes the NuGet packages.
        /// </summary>
        /// <param name="projectPackages">The NuGet packages referenced by the project.</param>
        /// <param name="targetFrameworkMoniker">The target framework moniker.</param>
        /// <param name="defaultIncludedFilesNuGetPackages">Specifies the NuGet package IDs that are included by default.</param>
        /// <returns>The assembly info of the processed packages.</returns>
        /// <exception cref="InvalidOperationException">Cannot find the package with the identity.</exception>
        public async Task<NuGetPackageAssemblyData> ProcessAsync(IList<PackageIdentity> projectPackages, string targetFrameworkMoniker, IReadOnlyCollection<string> defaultIncludedFilesNuGetPackages)
        {
            var cancellationToken = CancellationToken.None;

            var provider = DefaultFrameworkNameProvider.Instance;
            NuGetFramework nugetFramework = NuGetFramework.ParseFrameworkName(targetFrameworkMoniker, provider);

            using (var cacheContext = new SourceCacheContext())
            {
                cacheContext.MaxAge = DateTimeOffset.UtcNow;

                var allDependenciesPackageInfos = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

                var filteredProjectPackages = new List<PackageIdentity>();
                foreach (var projectPackage in projectPackages)
                {
                    // Verify whether the specified package version actually is available.
                    var packageIdentity = await GetPackageIdentityAsync(projectPackage, cacheContext, cancellationToken).ConfigureAwait(false);

                    if (packageIdentity is null)
                    {
                        throw new InvalidOperationException($"Cannot find package {projectPackage.Id}, version {projectPackage.Version}.");
                    }

                    if (DevPackHelper.DevPackNuGetPackages.Contains(packageIdentity.Id) || defaultIncludedFilesNuGetPackages.Contains(packageIdentity.Id))
                    {
                        continue;
                    }

                    filteredProjectPackages.Add(packageIdentity);

                    await AddPackagesAndDependenciesAsync(packageIdentity, cacheContext, nugetFramework, nuGetLogger, repositories, allDependenciesPackageInfos, cancellationToken).ConfigureAwait(false);
                }

                var unifiedPackages = GetResolvedPackages(sourceRepositoryProvider, nuGetLogger, filteredProjectPackages, allDependenciesPackageInfos);

                var references = await ProcessPackagesAsync(unifiedPackages, allDependenciesPackageInfos, nugetFramework, defaultIncludedFilesNuGetPackages);

                return references;
            }
        }

        /// <summary>
        /// Determines which packages to install after resolving.
        /// </summary>
        /// <param name="sourceRepositoryProvider">Source repository provider.</param>
        /// <param name="logger">The NuGet Logger</param>
        /// <param name="extensions">The NuGet packages.</param>
        /// <param name="allPackagesAndDependencies">All top level packages and their dependencies.</param>
        /// <returns>The resolved NuGet packages.</returns>
        private static IEnumerable<PackageIdentity> GetResolvedPackages(ISourceRepositoryProvider sourceRepositoryProvider,
                                                                              ILogger logger, IList<PackageIdentity> extensions,
                                                                              HashSet<SourcePackageDependencyInfo> allPackagesAndDependencies)
        {
            if (allPackagesAndDependencies.Count == 0)
            {
                // If no dependencies, then return the main package(s) at least.
                return extensions;
            }

            // Create a package resolver context (this is used to help figure out which actual package versions to install).
            var resolverContext = new PackageResolverContext(
                DependencyBehavior.Lowest,
                extensions.Select(identity => identity.Id).Distinct(),
                Enumerable.Empty<string>(),
                Enumerable.Empty<PackageReference>(),
                Enumerable.Empty<PackageIdentity>(),
                allPackagesAndDependencies,
                sourceRepositoryProvider.GetRepositories().Select(s => s.PackageSource),
                logger);

            var resolver = new PackageResolver();

            // Work out the actual set of packages to install.
            IEnumerable<PackageIdentity> packagesToInstall = resolver.Resolve(resolverContext, CancellationToken.None)
                                                                                 .Select(identity => allPackagesAndDependencies.Single(info => PackageIdentityComparer.Default.Equals(info, identity)));

            return packagesToInstall;
        }

        private async Task<NuGetPackageAssemblyData> ProcessPackagesAsync(IEnumerable<PackageIdentity> resolvedPackages, HashSet<SourcePackageDependencyInfo> filteredAllPackages, NuGetFramework nugetFramework, IReadOnlyCollection<string> defaultIncludedFilesNuGetPackages)
        {
            var nugetPackageAssemblies = new NuGetPackageAssemblyData();

            if (resolvedPackages == null || !resolvedPackages.Any())
            {
                return nugetPackageAssemblies;
            }

            HashSet<string> processedPackages = new HashSet<string>();

            // Resolved Packages are top level NuGet Packages
            await ProcessResolvedPackagesAsync(resolvedPackages, nugetPackageAssemblies, processedPackages, nugetFramework, defaultIncludedFilesNuGetPackages);

            // Remaining Packages are Dependencies
            await ProcessRemainingPackagesAsync(filteredAllPackages, nugetPackageAssemblies, processedPackages, nugetFramework);

            return nugetPackageAssemblies;
        }

        /// <summary>
        /// Processes the resolved/unified packages.
        /// </summary>
        /// <param name="resolvedPackages">The resolved packages.</param>
        /// <param name="nugetPackageAssemblies">The NuGet package assemblies.</param>
        /// <param name="processedPackages">The processed packages.</param>
        /// <param name="nugetFramework">The NuGet framework.</param>
        /// <param name="defaultIncludedFilesNuGetPackages">The default NuGet "Skyline.DataMiner.Files." NuGet packages that are already referenced by default.</param>
        private async Task ProcessResolvedPackagesAsync(IEnumerable<PackageIdentity> resolvedPackages, NuGetPackageAssemblyData nugetPackageAssemblies, ISet<string> processedPackages, NuGetFramework nugetFramework, IReadOnlyCollection<string> defaultIncludedFilesNuGetPackages)
        {
            // For all assemblies in the resolved package list we provide the reference to the assembly.
            foreach (var resolvedPackage in resolvedPackages)
            {
                string packageKey = resolvedPackage.Id.ToLower() + "\\" + resolvedPackage.Version.ToString().ToLower();
                processedPackages.Add(packageKey);

                using (PackageReaderBase packageReader = GetPackageReader(resolvedPackage))
                {
                    if (packageReader.GetDevelopmentDependency() || (NuGetHelper.IsDevPackNuGetPackage(resolvedPackage.Id) && defaultIncludedFilesNuGetPackages.Contains(resolvedPackage.Id)))
                    {
                        continue;
                    }

                    var libItems = packageReader.GetLibItems().ToList();

                    var nearestLibItems = frameworkReducer.GetNearest(nugetFramework, libItems.Select(x => x.TargetFramework));
                    var filteredLibItems = await ExtractPrimaryAssembliesAsync(libItems, nearestLibItems, packageReader);

                    if (filteredLibItems.Any())
                    {
                        // Lib items corresponding to the nuspec file.
                        // See: https://docs.microsoft.com/en-us/nuget/reference/nuspec#including-assembly-files
                        foreach (var filteredLibItem in filteredLibItems)
                        {
                            string assemblyName = _fileSystem.Path.GetFileName(filteredLibItem);
                            string dllImportDirectory = packageKey + "\\" + _fileSystem.Path.GetDirectoryName(filteredLibItem).Replace("/", "\\");

                            nugetPackageAssemblies.ProcessedAssemblies.Add(assemblyName);

                            if (!resolvedPackage.Id.StartsWith(DevPackHelper.FilesPrefix))
                            {
                                nugetPackageAssemblies.ImplicitDllImportDirectoryReferences.Add(dllImportDirectory);
                            }

                            string fullPath = null;
                            string dllImportValue;
                            bool isFilePackage = false;
                            bool dontAddToPackageToInstall = false;

                            if (resolvedPackage.Id.StartsWith(DevPackHelper.FilesPrefix))
                            {
                                // Full path is not set as it should not be included.
                                dllImportValue = assemblyName;
                                isFilePackage = true;
                                dontAddToPackageToInstall = true;
                            }
                            else if (NuGetHelper.CustomNuGetPackages.TryGetValue(resolvedPackage.Id, out (string Path, bool InDllImportDirectory) info))
                            {
                                dllImportValue = info.Path;
                                isFilePackage = !info.InDllImportDirectory;
                                dontAddToPackageToInstall = true;
                            }
                            else if (NuGetHelper.IsSolutionLibraryNuGetPackage(resolvedPackage.Id, out string name))
                            {
                                // Use the name of the solution library as the folder name. Everything after 'Skyline.DataMiner.Dev.Utils.' is considered the name.
                                dllImportValue = $"SolutionLibraries\\{name}\\{assemblyName}";
                                dontAddToPackageToInstall = true;
                            }
                            else
                            {
                                fullPath = _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(NuGetRootPath, resolvedPackage.Id.ToLower(), resolvedPackage.Version.ToString().ToLower(), filteredLibItem));
                                dllImportValue = dllImportDirectory + "\\" + assemblyName; // fileInfo.Name
                            }

                            var packageAssemblyReference = new PackageAssemblyReference(dllImportValue, fullPath, isFilePackage);

                            // Needs to be added as a reference in the dllImport attribute/script references.
                            nugetPackageAssemblies.DllImportNugetAssemblyReferences.Add(packageAssemblyReference);

                            if (dontAddToPackageToInstall)
                            {
                                // Should not be provided in the package to install.
                                continue;
                            }

                            nugetPackageAssemblies.NugetAssemblies.Add(packageAssemblyReference);
                        }
                    }

                    // Frameworkitems are items that are that are part of the targeted .NET framework. These are specified in the nuspec file.
                    // See: https://docs.microsoft.com/en-us/nuget/reference/nuspec#framework-assembly-references
                    var frameworkItems = packageReader.GetFrameworkItems().ToList();
                    var nearestFramework = frameworkReducer.GetNearest(nugetFramework, frameworkItems.Select(x => x.TargetFramework));

                    var filteredFrameworkItems = frameworkItems
                        .Where(x => x.TargetFramework.Equals(nearestFramework))
                        .SelectMany(x => x.Items)
                        .Select(x => x + ".dll")
                        .ToList();

                    nugetPackageAssemblies.ProcessedAssemblies.AddRange(filteredFrameworkItems);
                    nugetPackageAssemblies.DllImportFrameworkAssemblyReferences.AddRange(filteredFrameworkItems);
                }
            }
        }

        private static async Task<IEnumerable<string>> ExtractPrimaryAssembliesAsync(List<FrameworkSpecificGroup> libItems, NuGetFramework nearestVersion, PackageReaderBase packageReader)
        {
            if (nearestVersion == null)
            {
                return Array.Empty<string>();
            }

            var nearestLibItems = libItems.FirstOrDefault(x => x.TargetFramework.Equals(nearestVersion));

            if (nearestLibItems?.Items == null || !nearestLibItems.Items.Any())
            {
                return Array.Empty<string>();
            }

            // Determine short folder name as follows because nearestVersion.GetShortFolderName(); could give a different result (e.g. net40) compared to what is used in the actual package (e.g. net4).
            var firstItem = nearestLibItems.Items.First();
            var firstItemParts = firstItem.Split('/');

            string shortFolderName = null;

            if (firstItemParts.Length > 1)
            {
                shortFolderName = firstItemParts[1];
            }

            if (shortFolderName == null)
            {
                return Array.Empty<string>();
            }

            var filteredLibItems = new List<string>();
            foreach (var libItem in nearestLibItems.Items)
            {
                if (!libItem.EndsWith(".dll"))
                {
                    // Only process DLL files.
                    continue;
                }

                string prefix = "lib/" + shortFolderName + '/';

                // Safeguard.
                if (!libItem.StartsWith(prefix))
                {
                    continue;
                }

                string subPath = libItem.Substring(prefix.Length);
                int subfolderNameIndex = subPath.IndexOf('/');

                if (subfolderNameIndex != -1)
                {
                    string subfolderName = subPath.Substring(0, subfolderNameIndex);

                    // Verify whether the assembly is a satellite assembly.
                    // If it is a satellite assembly, do not include it.
                    // For more info about satellite assemblies, refer to https://learn.microsoft.com/en-us/dotnet/core/extensions/create-satellite-assemblies.
                    var satelliteAssemblies = await packageReader.GetSatelliteFilesAsync(subfolderName, CancellationToken.None);
                    var satelliteAssembliesList = satelliteAssemblies.ToList();

                    if (satelliteAssembliesList.ToList().Count == 0 || !satelliteAssembliesList.Contains(libItem))
                    {
                        filteredLibItems.Add(libItem);
                    }
                }
                else
                {
                    // Root item, cannot be a satellite assembly.
                    filteredLibItems.Add(libItem);
                }
            }

            return filteredLibItems;
        }

        /// <summary>
        /// Processes the remaining packages skipping the ones that are already processes.
        /// </summary>
        /// <param name="allPackages">All packages.</param>
        /// <param name="nugetPackageAssemblies">The NuGet package assemblies.</param>
        /// <param name="processedPackages">The processed packages.</param>
        /// <param name="nugetFramework">The NuGet framework.</param>
        private async Task ProcessRemainingPackagesAsync(HashSet<SourcePackageDependencyInfo> allPackages, NuGetPackageAssemblyData nugetPackageAssemblies, ICollection<string> processedPackages, NuGetFramework nugetFramework)
        {
            // For all assemblies that are not in the resolved package list, we provide the folder where the assembly can be found.
            foreach (var packageToInstall in allPackages)
            {
                string packageKey = packageToInstall.Id.ToLower() + "\\" + packageToInstall.Version.ToString().ToLower();
                if (processedPackages.Contains(packageKey))
                {
                    continue;
                }

                using (PackageReaderBase packageReader = GetPackageReader(packageToInstall))
                {
                    if (packageReader.GetDevelopmentDependency() || NuGetHelper.IsDevPackNuGetPackage(packageToInstall.Id))
                    {
                        continue;
                    }

                    var libItems = packageReader.GetLibItems().ToList();
                    var nearestLibItems = frameworkReducer.GetNearest(nugetFramework, libItems.Select(x => x.TargetFramework));
                    var filteredLibItems = await ExtractPrimaryAssembliesAsync(libItems, nearestLibItems, packageReader);

                    if (filteredLibItems.Any())
                    {
                        var firstFilteredLibItem = filteredLibItems.First();
                        string dllImportDirectory = packageKey + "\\" + _fileSystem.Path.GetDirectoryName(firstFilteredLibItem).Replace("/", "\\");

                        // Add the directory to be added to the dllImport attribute so the assembly can be found at runtime.
                        if (!nugetPackageAssemblies.ImplicitDllImportDirectoryReferences.Contains(dllImportDirectory))
                        {
                            nugetPackageAssemblies.DllImportDirectoryReferences.Add(dllImportDirectory + "\\");

                            // For Automation scripts we cannot provide a path, so instead the first assembly is provided (as this will result in the path of that assembly to be used as a hint path as well).
                            nugetPackageAssemblies.DllImportDirectoryReferencesAssembly.Add(dllImportDirectory + "\\", dllImportDirectory + "\\" + _fileSystem.Path.GetFileName(firstFilteredLibItem));
                        }

                        // Add all assemblies so these get included in the dllImports folder to be loaded at runtime.
                        foreach (var filteredLibItem in filteredLibItems)
                        {
                            var fullPath = _fileSystem.Path.Combine(NuGetRootPath, packageToInstall.Id.ToLower(), packageToInstall.Version.ToString().ToLower(), filteredLibItem);
                            var dllImportValue = dllImportDirectory + "\\" + _fileSystem.Path.GetFileName(filteredLibItem);

                            nugetPackageAssemblies.NugetAssemblies.Add(new PackageAssemblyReference(dllImportValue, fullPath));
                        }
                    }

                    var frameworkItems = packageReader.GetFrameworkItems().ToList();
                    var nearestFramework = frameworkReducer.GetNearest(nugetFramework, frameworkItems.Select(x => x.TargetFramework));

                    var filteredFrameworkItems = frameworkItems
                        .Where(x => x.TargetFramework.Equals(nearestFramework))
                        .SelectMany(x => x.Items)
                        .Select(x => x + ".dll");

                    nugetPackageAssemblies.ProcessedAssemblies.AddRange(filteredFrameworkItems);
                }
            }
        }

        private PackageReaderBase GetPackageReader(PackageIdentity packageToInstall)
        {
            var installedPath = versionFolderPathResolver.GetInstallPath(packageToInstall.Id, packageToInstall.Version);

            PackageReaderBase packageReader = new PackageFolderReader(installedPath);

            return packageReader;
        }

        /// <summary>
        /// Retrieves the package.
        /// </summary>
        /// <param name="nugetPackage">The package for which the identity should be retrieved.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>The package identity.</returns>
        private async Task<PackageIdentity> GetPackageIdentityAsync(
          PackageIdentity nugetPackage, SourceCacheContext cache, CancellationToken cancelToken)
        {
            if (nugetPackage.HasVersion)
            {
                return nugetPackage;
            }

            foreach (var sourceRepository in repositories)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                FindPackageByIdResource findPackageResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancelToken).ConfigureAwait(false);
                var allVersions = await findPackageResource.GetAllVersionsAsync(nugetPackage.Id, cache, nuGetLogger, cancelToken).ConfigureAwait(false);

                // No version; choose the latest, allow pre-release if configured.
                NuGetVersion selected = allVersions.LastOrDefault();

                return new PackageIdentity(nugetPackage.Id, selected);
            }

            return null;
        }

        /// <summary>
        /// Searches the package dependency graph for the chain of all packages to install.
        /// </summary>
        private async Task AddPackagesAndDependenciesAsync(PackageIdentity package, SourceCacheContext cacheContext, NuGetFramework framework, ILogger logger, ICollection<SourceRepository> repositories, ISet<SourcePackageDependencyInfo> allPackages, CancellationToken cancellationToken)
        {
            if (allPackages.Contains(package))
            {
                // Package was already processed.
                return;
            }

            await InstallPackageIfNotFound(package, cacheContext, cancellationToken);

            if (NuGetHelper.SkipPackageDependencies(package.Id))
            {
                // Add it to the allPackages as the PackageResolver later on needs it (in case there would be multiple versions).
                SourceRepository packageSourceRepository = null;
                foreach (var sourceRepository in repositories)
                {
                    FindPackageByIdResource findPackageResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken).ConfigureAwait(false);
                    var exists = await findPackageResource.DoesPackageExistAsync(package.Id, package.Version, cacheContext, nuGetLogger, cancellationToken).ConfigureAwait(false);

                    if (exists)
                    {
                        packageSourceRepository = sourceRepository;
                        break;
                    }
                }

                if (packageSourceRepository == null)
                {
                    logCollector?.ReportError($"PackageReferenceProcessor|InstallPackageIfNotFound|Unable to find package '{package.Id}' with version '{package.Version}");
                    return;
                }

                allPackages.Add(new SourcePackageDependencyInfo(package.Id, package.Version, Array.Empty<PackageDependency>(), true,
                    packageSourceRepository));
                return;
            }

            foreach (var sourceRepository in repositories)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Get the dependency info for the package.
                var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>(cancellationToken).ConfigureAwait(false);
                var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                    package,
                    framework,
                    cacheContext,
                    logger,
                    cancellationToken).ConfigureAwait(false);

                if (dependencyInfo == null)
                {
                    continue;
                }

                var filteredDependencyInfo = dependencyInfo.Dependencies.Where(d => !NuGetHelper.IsDevPackNuGetPackage(d.Id));

                var filteredDependencies = new SourcePackageDependencyInfo(dependencyInfo.Id, dependencyInfo.Version, filteredDependencyInfo, dependencyInfo.Listed, dependencyInfo.Source);

                allPackages.Add(filteredDependencies);

                // Process dependencies of dependency.
                foreach (var dependency in dependencyInfo.Dependencies)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (NuGetHelper.IsDevPackNuGetPackage(dependency.Id))
                    {
                        continue;
                    }

                    await AddPackagesAndDependenciesAsync(
                        new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
                        cacheContext,
                        framework,
                        logger,
                        repositories,
                        allPackages,
                        cancellationToken).ConfigureAwait(false);
                }

                break;
            }
        }

        private async Task InstallPackageIfNotFound(PackageIdentity packageToInstall, SourceCacheContext cacheContext, CancellationToken cancelToken)
        {
            var existsResource = await rootRepository.GetResourceAsync<FindLocalPackagesResource>(cancelToken);
            if (existsResource.Exists(packageToInstall, nuGetLogger, cancelToken))
            {
                // Package is already installed.
                return;
            }

            LogDebug($"InstallPackageIfNotFound|Installing package: {packageToInstall.Id} - {packageToInstall.Version}");

            PackageSource packageSource = null;
            // Figure out which packageSource is needed
            foreach (var sourceRepository in repositories)
            {
                FindPackageByIdResource findPackageResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancelToken).ConfigureAwait(false);
                var exists = await findPackageResource.DoesPackageExistAsync(packageToInstall.Id, packageToInstall.Version, cacheContext, nuGetLogger, cancelToken).ConfigureAwait(false);

                if (exists)
                {
                    packageSource = sourceRepository.PackageSource;
                    break;
                }
            }

            if (packageSource == null)
            {
                logCollector?.ReportError($"PackageReferenceProcessor|InstallPackageIfNotFound|Unable to find package '{packageToInstall.Id}' with version '{packageToInstall.Version}");
                return;
            }

            var repository = Repository.Factory.GetCoreV3(packageSource);
            var resource = await repository.GetResourceAsync<DownloadResource>(cancelToken);

            try
            {
                using (DownloadResourceResult downloadResourceResult = await resource.GetDownloadResourceResultAsync(
                           packageToInstall,
                           new PackageDownloadContext(cacheContext),
                           SettingsUtility.GetGlobalPackagesFolder(settings),
                           nuGetLogger,
                           cancelToken))
                {
                    // Add it to the global package folder
                    using (DownloadResourceResult result = await GlobalPackagesFolderUtility.AddPackageAsync(
                               packageSource.Source,
                               packageToInstall,
                               downloadResourceResult.PackageStream,
                               NuGetRootPath,
                               Guid.Empty,
                               clientPolicyContext,
                               nuGetLogger,
                               CancellationToken.None))
                    {
                        LogDebug($"InstallPackageIfNotFound|Finished installing package {packageToInstall.Id} - {packageToInstall.Version} with status: " + result?.Status);
                    }
                }
            }
            catch
            {
                LogDebug("Retrying to add package without caching");
                string tempDir = FileSystem.Instance.Directory.CreateTemporaryDirectory();

                try
                {
                    // Retrying without cache
                    using (DownloadResourceResult downloadResourceResult = await resource.GetDownloadResourceResultAsync(
                               packageToInstall,
                               new PackageDownloadContext(cacheContext, tempDir, true),
                               SettingsUtility.GetGlobalPackagesFolder(settings),
                               nuGetLogger,
                               cancelToken))
                    {
                        // Add it to the global package folder
                        using (DownloadResourceResult result = await GlobalPackagesFolderUtility.AddPackageAsync(
                                   packageSource.Source,
                                   packageToInstall,
                                   downloadResourceResult.PackageStream,
                                   NuGetRootPath,
                                   Guid.Empty,
                                   clientPolicyContext,
                                   nuGetLogger,
                                   CancellationToken.None))
                        {
                            LogDebug($"InstallPackageIfNotFound|Finished installing package {packageToInstall.Id} - {packageToInstall.Version} with status: " + result?.Status);
                        }
                    }
                }
                finally
                {
                    FileSystem.Instance.Directory.DeleteDirectory(tempDir);
                }
            }
        }

        private void LogDebug(string message)
        {
            logCollector?.ReportDebug($"PackageReferenceProcessor|{message}");
        }
    }
}