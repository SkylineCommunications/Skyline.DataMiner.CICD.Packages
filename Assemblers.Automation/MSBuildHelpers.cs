
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Evaluation;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Skyline.DataMiner.CICD.Assemblers.Common;

namespace Skyline.DataMiner.CICD.Assemblers.Automation
{
    /// <summary>
    /// Provides helper methods for evaluating MSBuild projects.
    /// </summary>
    public static class MSBuildHelpers
    {
        /// <summary>
        /// Evaluates a referenced project and returns information about it.
        /// </summary>
        /// <param name="referencedProjectFullPath">The full path to the referenced project file.</param>
        /// <param name="singleTargetFramework">The target framework to evaluate.</param>
        /// <returns>The evaluated project information, or null if the project is invalid.</returns>
        public static ReferencedProjectInfo EvaluateReferenceProject(string referencedProjectFullPath, string singleTargetFramework)
        {
            if (string.IsNullOrWhiteSpace(referencedProjectFullPath))
                return null;

            var pc = new ProjectCollection();
            pc.DisableMarkDirty = true;

            // First evaluate without forcing a TFM.
            // This is needed to discover TargetFramework / TargetFrameworks
            // from the referenced project itself.
            var outerProject = new Microsoft.Build.Evaluation.Project(
                referencedProjectFullPath,
                new Dictionary<string, string>(),
                null,
                pc);

            var selectedTargetFramework = ResolveTargetFramework(outerProject, singleTargetFramework);
            pc.UnloadProject(outerProject);
            Microsoft.Build.Evaluation.Project msproj;

            if (!string.IsNullOrWhiteSpace(selectedTargetFramework))
            {
                var globalProperties = new Dictionary<string, string>
                {
                    ["TargetFramework"] = selectedTargetFramework,
                };

                msproj = new Microsoft.Build.Evaluation.Project(referencedProjectFullPath, globalProperties, null, pc);
            }
            else
            {
                msproj = new Microsoft.Build.Evaluation.Project(referencedProjectFullPath, new Dictionary<string, string>(), null, pc);
            }

            string Get(string name) => msproj.GetPropertyValue(name) ?? string.Empty;

            var packageId = Get("PackageId");

            if (string.IsNullOrWhiteSpace(packageId))
            {
                packageId = Get("AssemblyName");
            }

            if (string.IsNullOrWhiteSpace(packageId))
            {
                packageId = Path.GetFileNameWithoutExtension(referencedProjectFullPath);
            }

            var packageVersion = Get("PackageVersion");
            var targetFramework = Get("TargetFramework");

            var targetPath = Get("TargetPath");
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                var targetDir = Get("TargetDir");
                var assemblyName = Get("AssemblyName");
                if (!string.IsNullOrWhiteSpace(assemblyName) && !string.IsNullOrWhiteSpace(targetDir))
                {
                    var assemblyNameFile = assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? assemblyName : $"{assemblyName}.dll";
                    targetPath = Path.Combine(targetDir, assemblyNameFile);
                }
            }
            string assemblyVersion = string.Empty;

            if (!string.IsNullOrWhiteSpace(targetPath) && File.Exists(targetPath))
            {
                assemblyVersion = AssemblyName
                    .GetAssemblyName(targetPath)
                    .Version?
                    .ToString() ?? string.Empty;
            }
            bool isCpm = string.Equals(Get("ManagePackageVersionsCentrally"), "true", StringComparison.OrdinalIgnoreCase);
            Dictionary<string, string> centralVersions = null;
            if (isCpm)
            {
                centralVersions = msproj.GetItems("PackageVersion")
                    .ToDictionary(i => i.EvaluatedInclude, i => i.GetMetadataValue("Version"), StringComparer.OrdinalIgnoreCase);
            }
            var directPackages = new List<PackageIdentity>();



            foreach (var item in msproj.GetItems("PackageReference"))
            {
                var id = item.EvaluatedInclude;

                if (string.IsNullOrWhiteSpace(id))
                    continue;

                var version = item.GetMetadataValue("Version");

                if (string.IsNullOrWhiteSpace(version))
                {
                    version = item.GetMetadataValue("VersionOverride");
                }

                if (string.IsNullOrWhiteSpace(version) &&
                    centralVersions != null &&
                    centralVersions.TryGetValue(id, out var centralVersion))
                {
                    version = centralVersion;
                }

                if (string.IsNullOrWhiteSpace(version))
                {
                    continue;
                }

                if (!NuGetVersion.TryParse(version, out var nugetVersion))
                    continue;

                directPackages.Add(new PackageIdentity(id, nugetVersion));
            }


            string dataMinerType = Get("DataMinerType");
            var outputType = Get("OutputType");
            return new ReferencedProjectInfo(
     Path.GetFullPath(referencedProjectFullPath))
            {
                PackageId = packageId,
                TargetFramework = targetFramework,
                PackageVersion = packageVersion,
                TargetPath = targetPath,
                AssemblyName = Get("AssemblyName"),
                DataMinerType = dataMinerType,
                OutputType = outputType,
                AssemblyVersion = assemblyVersion,
                DirectPackageReferences = directPackages,
            };
        }
        /// <summary>
        /// creates a synthetic package assembly reference from the referenced project information.
        /// </summary>
        /// <returns>the synthetic package assembly reference, or null if the referenced project is invalid.</returns>
        public static PackageAssemblyReference CreateSyntheticPackageAssemblyReference(ReferencedProjectInfo referencedProjectInfo)
        {
            if (referencedProjectInfo == null || !referencedProjectInfo.ShouldHarvestAssembly()) return null;
            var dllImportInfo = referencedProjectInfo.GetDllImportRelativePath().Replace('\\', '/');
            var assemblyPath = referencedProjectInfo.GetSourceAssemblyPath();
            return new PackageAssemblyReference(dllImportInfo, Path.GetFullPath(assemblyPath));
        }
        private static string ResolveTargetFramework(Microsoft.Build.Evaluation.Project project, string requestedTargetFramework)
        {
            var targetFramework = project.GetPropertyValue("TargetFramework");

            var targetFrameworks = project.GetPropertyValue("TargetFrameworks");

            var declaredFrameworks = new List<string>();

            if (!string.IsNullOrWhiteSpace(targetFramework))
            {
                declaredFrameworks.Add(targetFramework.Trim());
            }
            else if (!string.IsNullOrWhiteSpace(targetFrameworks))
            {
                declaredFrameworks.AddRange(
                    targetFrameworks
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            if (declaredFrameworks.Count == 0)
            {
                return string.Empty;
            }

            // Single-target project.
            if (declaredFrameworks.Count == 1)
            {
                return declaredFrameworks[0];
            }

            // Multi-target project without a requested parent TFM.
            // Keep deterministic behavior, but production harvesting
            // should normally always provide the parent TFM.
            if (string.IsNullOrWhiteSpace(requestedTargetFramework))
            {
                return declaredFrameworks[0];
            }

            // Prefer exact match.
            var exactMatch = declaredFrameworks.FirstOrDefault(
                x => string.Equals(
                    x,
                    requestedTargetFramework,
                    StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null)
            {
                return exactMatch;
            }

            // Otherwise let NuGet determine the nearest compatible TFM.
            var requestedFramework =
                ParseNuGetFramework(requestedTargetFramework);

            var candidates = declaredFrameworks
                .Select(x => new
                {
                    Name = x,
                    Framework = ParseNuGetFramework(x),
                })
                .ToList();

            var reducer = new FrameworkReducer();

            var nearestFramework =
                reducer.GetNearest(
                    requestedFramework,
                    candidates.Select(x => x.Framework));

            if (nearestFramework == null)
            {
                return string.Empty;
            }

            return candidates
                .First(x => x.Framework.Equals(nearestFramework))
                .Name;
        }

        private static NuGetFramework ParseNuGetFramework(string framework)
        {
            if (framework.StartsWith(".", StringComparison.Ordinal) ||
                framework.Contains(","))
            {
                return NuGetFramework.Parse(framework);
            }

            return NuGetFramework.ParseFolder(framework);
        }

    }


}

