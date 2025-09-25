namespace Skyline.DataMiner.CICD.Assemblers.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using NuGet.Packaging.Core;

    using Skyline.DataMiner.CICD.Assemblers.Common;
    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Provides functionality to filter assemblies based on target framework, package references, and build result items.
    /// </summary>
    public static class AssemblyFilter
    {
        /// <summary>
        /// Filters assemblies based on the specified criteria.
        /// </summary>
        /// <param name="targetFrameworkMoniker">The target framework moniker.</param>
        /// <param name="packageReferenceProcessor">The package reference processor.</param>
        /// <param name="buildResultItems">The build result items.</param>
        /// <param name="dllImports">A set of DLL imports.</param>
        /// <param name="packageIdentities">A list of package identities.</param>
        /// <returns>The filtered NuGet package assembly data.</returns>
        public static async Task<NuGetPackageAssemblyData> FilterAsync(string targetFrameworkMoniker, PackageReferenceProcessor packageReferenceProcessor, BuildResultItems buildResultItems, HashSet<string> dllImports, IList<PackageIdentity> packageIdentities)
        {
            var nugetAssemblyData = await packageReferenceProcessor.ProcessAsync(packageIdentities, targetFrameworkMoniker,
                Skyline.DataMiner.CICD.Common.NuGet.DevPackHelper.ProtocolDevPackNuGetDependenciesIncludingTransitive).ConfigureAwait(false);

            ProcessFrameworkAssemblies(dllImports, nugetAssemblyData);
            ProcessLibAssemblies(buildResultItems, dllImports, nugetAssemblyData);

            return nugetAssemblyData;
        }

        /// <summary>
        /// Adds a new DLL import to the set if it does not already contain an import with the same name.
        /// </summary>
        /// <param name="dllImports">A set of DLL imports.</param>
        /// <param name="newImport">The new import to add.</param>
        private static bool AddToDllImport(HashSet<string> dllImports, string newImport)
        {
            if (!dllImports.Select(FileSystem.Instance.Path.GetFileName).Contains(FileSystem.Instance.Path.GetFileName(newImport)))
            {
                dllImports.Add(newImport);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processes NuGet assembly references for DLL import and updates the set of DLL imports and directory references accordingly.
        /// </summary>
        /// <param name="dllImports">A set of DLL imports.</param>
        /// <param name="nugetAssemblyData">The NuGet package assembly data.</param>
        private static void ProcessDllImportNuGetAssemblyReferences(HashSet<string> dllImports, NuGetPackageAssemblyData nugetAssemblyData)
        {
            if (nugetAssemblyData.DllImportNugetAssemblyReferences.Count <= 0)
            {
                return;
            }

            var directoriesWithExplicitDllImport = new HashSet<string>();
            var potentialRemainingDirectoryImports = new List<string>();
            var assemblies = new Dictionary<string, List<PackageAssemblyReference>>();

            foreach (var libItem in nugetAssemblyData.DllImportNugetAssemblyReferences)
            {
                string assemblyName = FileSystem.Instance.Path.GetFileName(libItem.DllImport);

                if (assemblies.TryGetValue(assemblyName, out var entries))
                {
                    entries.Add(libItem);
                }
                else
                {
                    assemblies[assemblyName] = new List<PackageAssemblyReference> { libItem };
                }
            }

            foreach (var assembly in assemblies)
            {
                var packagesContainingAssembly = assembly.Value;

                if (packagesContainingAssembly.Count == 1)
                {
                    var libItem = packagesContainingAssembly[0];
                    if (AddToDllImport(dllImports, libItem.DllImport))
                    {
                        directoriesWithExplicitDllImport.Add(libItem.DllImport.Substring(0, libItem.DllImport.Length - assembly.Key.Length));
                    }
                    else
                    {
                        potentialRemainingDirectoryImports.Add(FileSystem.Instance.Path.GetDirectoryName(libItem.DllImport) + "\\");
                    }
                }
                else
                {
                    PackageAssemblyReference mostRecentLibItem = SelectMostRecentVersion(packagesContainingAssembly);
                    if (mostRecentLibItem == null)
                    {
                        continue;
                    }

                    if (AddToDllImport(dllImports, mostRecentLibItem.DllImport))
                    {
                        directoriesWithExplicitDllImport.Add(mostRecentLibItem.DllImport.Substring(0, mostRecentLibItem.DllImport.Length - assembly.Key.Length));
                    }
                    else
                    {
                        potentialRemainingDirectoryImports.Add(FileSystem.Instance.Path.GetDirectoryName(mostRecentLibItem.DllImport) + "\\");
                    }

                    foreach (var libItem in packagesContainingAssembly)
                    {
                        if (libItem != mostRecentLibItem)
                        {
                            string directoryPath = libItem.DllImport.Substring(0, libItem.DllImport.Length - assembly.Key.Length);
                            potentialRemainingDirectoryImports.Add(directoryPath);
                        }
                    }
                }
            }

            foreach (var directoryPath in potentialRemainingDirectoryImports)
            {
                if (!directoriesWithExplicitDllImport.Contains(directoryPath))
                {
                    nugetAssemblyData.DllImportDirectoryReferences.Add(directoryPath);
                }
            }
        }

        /// <summary>
        /// Processes framework assemblies and adds them to the set of DLL imports if not already present.
        /// </summary>
        /// <param name="dllImports">A set of DLL imports.</param>
        /// <param name="nugetAssemblyData">The NuGet package assembly data.</param>
        private static void ProcessFrameworkAssemblies(HashSet<string> dllImports, NuGetPackageAssemblyData nugetAssemblyData)
        {
            foreach (var frameworkAssembly in nugetAssemblyData.DllImportFrameworkAssemblyReferences)
            {
                if (!Helper.QActionDefaultImportDLLs.Any(a => String.Equals(a, frameworkAssembly, StringComparison.OrdinalIgnoreCase)))
                {
                    dllImports.Add(frameworkAssembly);
                }
            }
        }

        /// <summary>
        /// Processes library assemblies and updates the build result items and DLL imports accordingly.
        /// </summary>
        /// <param name="buildResultItems">The build result items.</param>
        /// <param name="dllImports">A set of DLL imports.</param>
        /// <param name="nugetAssemblyData">The NuGet package assembly data.</param>
        private static void ProcessLibAssemblies(BuildResultItems buildResultItems, HashSet<string> dllImports, NuGetPackageAssemblyData nugetAssemblyData)
        {
            ProcessDllImportNuGetAssemblyReferences(dllImports, nugetAssemblyData);

            foreach (var dir in nugetAssemblyData.DllImportDirectoryReferences)
            {
                if (!Helper.QActionDefaultImportDLLs.Any(d => String.Equals(d, dir, StringComparison.OrdinalIgnoreCase)))
                {
                    dllImports.Add(dir);
                }
            }

            foreach (var libItem in nugetAssemblyData.NugetAssemblies)
            {
                if (!dllImports.Contains(FileSystem.Instance.Path.GetFileName(libItem.AssemblyPath)) && buildResultItems.Assemblies.FirstOrDefault(b => b.AssemblyPath == libItem.AssemblyPath) == null)
                {
                    buildResultItems.Assemblies.Add(libItem);
                }
            }
        }

        /// <summary>
        /// Selects the most recent version of a package assembly reference from a list.
        /// </summary>
        /// <param name="packagesContainingAssembly">The list of package assembly references.</param>
        /// <returns>The most recent package assembly reference, or null if the list is empty.</returns>
        private static PackageAssemblyReference SelectMostRecentVersion(List<PackageAssemblyReference> packagesContainingAssembly)
        {
            PackageAssemblyReference mostRecentLibItem = null;
            Version mostRecentVersion = null;

            foreach (var libItem in packagesContainingAssembly)
            {
                var version = AssemblyName.GetAssemblyName(libItem.AssemblyPath).Version;
                if (mostRecentVersion == null || version > mostRecentVersion)
                {
                    mostRecentVersion = version;
                    mostRecentLibItem = libItem;
                }
            }

            return mostRecentLibItem;
        }
    }
}