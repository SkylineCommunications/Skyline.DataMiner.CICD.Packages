namespace Skyline.DataMiner.CICD.Assemblers.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the result items of a build.
    /// </summary>
    public class BuildResultItems
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildResultItems"/> class.
        /// </summary>
        public BuildResultItems()
        {
            Assemblies = new List<PackageAssemblyReference>();
            DllAssemblies = new List<DllAssemblyReference>();
        }

        /// <summary>
        /// Gets or sets the protocol or Automation script XML content.
        /// </summary>
        /// <value>The protocol or Automation script XML content.</value>
        public string Document { get; set; }

        /// <summary>
        /// Gets the assemblies of the used NuGet packages.
        /// </summary>
        /// <value>The assemblies of the used NuGet packages.</value>
        public ICollection<PackageAssemblyReference> Assemblies { get; }

        /// <summary>
        /// Gets the assemblies of the used DLLs (non-NuGet packages).
        /// </summary>
        public ICollection<DllAssemblyReference> DllAssemblies { get; }
    }
}
