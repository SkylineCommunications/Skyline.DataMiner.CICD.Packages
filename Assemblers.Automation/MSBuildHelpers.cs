using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
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
        /// <returns>The evaluated project information, or null if the project is invalid.</returns>
        public static ReferencedProjectInfo EvaluateReferenceProject(string referencedProjectFullPath)
        {
            if (string.IsNullOrWhiteSpace(referencedProjectFullPath))
                return null;

             var pc = new ProjectCollection();
            pc.DisableMarkDirty = true;
            var msproj = pc.LoadProject(referencedProjectFullPath);
            string Get(string name) => msproj.GetPropertyValue(name) ?? string.Empty;

            var packageId = Get("PackageId");
            if(string.IsNullOrWhiteSpace(packageId))
            {
                packageId = Get("AssemblyName") ?? Path.GetFileNameWithoutExtension(referencedProjectFullPath);
            }

            var packageVersion = Get("PackageVersion");
            var targetFramework = string.IsNullOrWhiteSpace(Get("TargetFramework")) ? Get("TargetFrameworks") : Get("TargetFramework");

            var targetPath = Get("TargetPath");
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                var targetDir = Get("TargetDir");
                var assemblyName = Get("AssemblyName");
                if(!string.IsNullOrWhiteSpace(assemblyName) && !string.IsNullOrWhiteSpace(targetDir))
                {
                    var assemblyNameFile=assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? assemblyName : $"{assemblyName}.dll";
                    targetPath=Path.Combine(targetDir, assemblyNameFile);
                }
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
                var id= item.EvaluatedInclude;
                var version = item.GetMetadataValue("Version");
                if(isCpm && string.IsNullOrWhiteSpace(version))
                {
                    var versionOverride = item.GetMetadataValue("VersionOverride");
                    if(!string.IsNullOrWhiteSpace(versionOverride))
                    {
                        version = versionOverride;
                    }
                    else if (centralVersions != null && centralVersions.TryGetValue(id, out var centralVersion))
                    {
                        version = centralVersion;
                    }
                }
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(version)) continue;
                if(!NuGetVersion.TryParse(version, out var nv)) nv= NuGetVersion.Parse(version);
                directPackages.Add(new PackageIdentity(id, nv));

            }
            bool isPackable = bool.TryParse(Get("IsPackable"), out var isPackableValue) && isPackableValue;
            bool genPkgOnBuild = bool.TryParse(Get("GeneratePackageOnBuild"), out var gp) && gp;
            string isDataMiner = Get("DataMinerType");

            return new ReferencedProjectInfo(
                projectPath: Path.GetFullPath(referencedProjectFullPath),
                packageId: packageId,
                packageVersion: packageVersion,
                targetFramework: targetFramework,
                targetPath: targetPath,
                assemblyName: Get("AssemblyName"),
                dataMinerType: isDataMiner,
                isPackable: isPackable,
                generatePackageOnBuild: genPkgOnBuild,
                directPackageReferences: directPackages);
        }
        /// <summary>
        /// creates a synthetic package assembly reference from the referenced project information.
        /// </summary>
        /// <returns>the synthetic package assembly reference, or null if the referenced project is invalid.</returns>
        public static PackageAssemblyReference CreateSyntheticPackageAssembyReference(ReferencedProjectInfo referencedProjectInfo)
        {
            if(referencedProjectInfo == null || !referencedProjectInfo.ShouldHarvestAsNuGetAssemblies()) return null;
            var dllImportInfo = referencedProjectInfo.GetDllImportRelativePath().Replace('\\', '/');
            var assemblyPath = referencedProjectInfo.GetSourceAssemblyPath();
            return new PackageAssemblyReference(dllImportInfo, Path.GetFullPath(assemblyPath));
        }
    }
   
}
