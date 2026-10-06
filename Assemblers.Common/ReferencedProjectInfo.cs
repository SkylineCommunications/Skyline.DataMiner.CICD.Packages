using System;
using System.Collections.Generic;
using System.IO;
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
        /// Gets or sets the NuGet package version of the referenced project.
        /// </summary>
        public string PackageVersion { get; set; } = string.Empty;
        /// <summary>
        /// Target framework of the referenced project.
        /// </summary>
        public string TargetFramework { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the evaluated build configuration.
        /// </summary>
        public string Configuration { get; set; } = string.Empty;
        /// <summary>
        /// Target path of the referenced project.
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;
        /// <summary>
        /// Assembly name of the referenced project.
        /// </summary>
        public string AssemblyName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates the type of DataMiner.
        /// </summary>
        public string DataMinerType { get; set; }
        /// <summary>
        /// Indicates the type of Output.
        /// </summary>
        public string OutputType { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the version read from the built assembly.
        /// </summary>

        public string AssemblyVersion { get; set; } = string.Empty;

        /// <summary>
        /// Tells the system which dependencies/references to include.
        /// </summary>
        public IReadOnlyList<PackageIdentity> DirectPackageReferences { get; set; } = Array.Empty<PackageIdentity>();
        /// <summary>
        /// Constructor for the <see cref="ReferencedProjectInfo"/> class.
        /// </summary>
        public ReferencedProjectInfo(string projectPath)
        {;
            ProjectPath = projectPath;
        }
        /// <summary>
        /// Checks if the referenced project is a DataMiner project based on the DataMinerType property.
        /// </summary>
        public bool IsDataMinerProject => !string.IsNullOrWhiteSpace(DataMinerType);
        /// <summary>
        /// Checks if the referenced project is a Library project based on the OutputType property.
        /// </summary>
        public bool IsLibraryProject => string.Equals(OutputType, "Library", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Determines whether the referenced project should be harvested as a DllImport assembly.
        /// </summary>
        public bool ShouldHarvestAssembly() => !IsDataMinerProject && IsLibraryProject;

        /// <summary>
        /// Gets the relative path for DllImport based on the package information.
        /// </summary>
        public string GetDllImportRelativePath()
        {
            if (string.IsNullOrWhiteSpace(PackageId) ||
                string.IsNullOrWhiteSpace(AssemblyVersion) ||
                string.IsNullOrWhiteSpace(TargetFramework) ||
                string.IsNullOrWhiteSpace(AssemblyName))
            {
                throw new InvalidOperationException("PackageId, AssemblyVersion, TargetFramework and AssemblyName are required.");
            }

            var fileName = AssemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? AssemblyName : AssemblyName + ".dll";
            return $"{PackageId.ToLowerInvariant()}/{AssemblyVersion}/lib/{TargetFramework}/{fileName}";
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
