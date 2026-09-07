using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace Skyline.DataMiner.CICD.Assemblers.Common
{
    /// <summary>
    /// Represents information about a referenced project.
    /// </summary>
    public sealed class ReferencedProjectInfo
    {
        /// <summary>
        /// Project path of the referenced project.
        /// </summary>
        public string ProjectPath { get; set; } = string.Empty;
        /// <summary>
        /// Package ID of the referenced project.
        /// </summary>
        public string PackageId { get; set; }
        /// <summary>
        /// Package version of the referenced project.
        /// </summary>
        public string PackageVersion { get; set; } = string.Empty;
        /// <summary>
        /// Target framework of the referenced project.
        /// </summary>
        public string TargetFramework { get; set; } = string.Empty;
        /// <summary>
        /// Target path of the referenced project.
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;
        /// <summary>
        /// Assembly name of the referenced project.
        /// </summary>
        public string AssemblyName { get; set; } = string.Empty;
        /// <summary>
        /// Indicates whether the referenced project is packable.
        /// </summary>
        public bool IsPackable { get; set; }
        /// <summary>
        /// Indicates whether to generate a package on build for the referenced project.
        /// </summary>
        public bool GeneratePackageOnBuild { get; set; }
        /// <summary>
        /// Indicates the type of DataMiner.
        /// </summary>
        public string DataMinerType { get; set; }
     
        /// <summary>
        /// Tells the system which dependencies/references to include.
        /// </summary>
        public IReadOnlyList<PackageIdentity> DirectPackageReferences { get; }
        /// <summary>
        /// Constructor for the <see cref="ReferencedProjectInfo"/> class.
        /// </summary>
        public ReferencedProjectInfo(string projectPath, string packageId, string packageVersion, string targetFramework, string targetPath,
                                 string assemblyName, bool isPackable, bool generatePackageOnBuild, string dataMinerType,
                                  IReadOnlyList<PackageIdentity> directPackageReferences)
        {
            ProjectPath = projectPath;
            PackageId = packageId ?? string.Empty;
            PackageVersion = packageVersion ?? string.Empty;
            TargetFramework = targetFramework ?? string.Empty;
            TargetPath = targetPath ?? string.Empty;
            AssemblyName = assemblyName ?? string.Empty;
            IsPackable = isPackable;
            GeneratePackageOnBuild = generatePackageOnBuild;
            DataMinerType = dataMinerType ?? string.Empty;
            DirectPackageReferences = directPackageReferences ?? new List<PackageIdentity>();
        }
        /// <summary>
        /// Checks if the referenced project is a DataMiner project based on the DataMinerType property.
        /// </summary>
        public bool IsDataMinerProject => !string.IsNullOrWhiteSpace(DataMinerType);

        /// <summary>
        /// Checks if the referenced project should be harvested as NuGet assemblies.
        /// </summary>
        public bool ShouldHarvestAsNuGetAssemblies() => !IsDataMinerProject && (IsPackable || GeneratePackageOnBuild);

        /// <summary>
        /// Gets the relative path for DllImport based on the package information.
        /// </summary>
        public string GetDllImportRelativePath()
        {
            if (string.IsNullOrWhiteSpace(PackageId) ||
                string.IsNullOrWhiteSpace(PackageVersion) ||
                string.IsNullOrWhiteSpace(TargetFramework) ||
                string.IsNullOrWhiteSpace(AssemblyName))
            {
                throw new InvalidOperationException("PackageId, PackageVersion, TargetFramework and AssemblyName are required.");
            }

            var fileName = AssemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? AssemblyName : AssemblyName + ".dll";
            return $"Assemblies/ProtocolScripts/DllImport/{PackageId}/{PackageVersion}/lib/{TargetFramework}/{fileName}";
        }
        /// <summary>
        /// Gets the full path to the source assembly based on the target path.
        /// </summary>
        public string GetSourceAssemblyPath()
        {
            if (!string.IsNullOrWhiteSpace(TargetPath))
            {
                return Path.GetFullPath(TargetPath);
            }

            throw new InvalidOperationException("TargetPath is required to resolve the source assembly.");
        }
        /// <summary>
        /// Gets the DllImport information as a tuple containing the relative path and the source assembly path.
        /// </summary>
        public (string DllImportRelativePath, string SourceAssemblyPath) ToDllImportInfo()
            => (GetDllImportRelativePath(), GetSourceAssemblyPath());
    }

}
