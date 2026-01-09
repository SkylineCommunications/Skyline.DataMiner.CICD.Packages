namespace Skyline.DataMiner.CICD.Assemblers.Common
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents NuGet package assembly data.
    /// </summary>
    public class NuGetPackageAssemblyData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetPackageAssemblyData"/> class.
        /// </summary>
        public NuGetPackageAssemblyData()
        {
            DllImportNugetAssemblyReferences = new List<PackageAssemblyReference>();
            DllImportFrameworkAssemblyReferences = new HashSet<string>();
            DllImportDirectoryReferences = new HashSet<string>();
            DllImportDirectoryReferencesAssembly = new Dictionary<string, string>();

            ImplicitDllImportDirectoryReferences = new HashSet<string>();

            NugetAssemblies = new List<PackageAssemblyReference>();

            ProcessedAssemblies = new HashSet<string>();
        }

        /// <summary>
        /// Gets all the NuGet package assemblies that need to be provided in the protocol package.
        /// </summary>
        /// <value>The NuGet package assemblies that need to be provided in the protocol package.</value>
        public IList<PackageAssemblyReference> NugetAssemblies { get; }

        /// <summary>
        /// Gets the NuGet package assemblies that need to be provided in the dllImportAttribute.
        /// </summary>
        /// <value>The NuGet package assemblies that need to be provided in the dllImportAttribute.</value>
        public IList<PackageAssemblyReference> DllImportNugetAssemblyReferences { get; }

        /// <summary>
        /// Gets the framework assemblies that need to be provided in the dllImport attribute.
        /// </summary>
        /// <value>The framework assemblies that need to be provided in the dllImport attribute.</value>
        public ISet<string> DllImportFrameworkAssemblyReferences { get; }

        /// <summary>
        /// Gets the directory paths that need to be added to the dllImport attribute.
        /// This is always windows style.
        /// </summary>
        /// <value>The directory paths that need to be added to the dllImport attribute.</value>
        public ISet<string> DllImportDirectoryReferences { get; }

        /// <summary>
        /// Gets a map of a folder and an assembly that resides in that folder.
        /// </summary>
        /// <value>A map of a folder and an assembly that resides in that folder.</value>
        /// <remarks>For Automation scripts, providing a directory is not supported. Therefore, a DLL of that exists in the specified folder must be provided. This map stores the name of an assembly that exists in the specified folder.</remarks>
        public IDictionary<string, string> DllImportDirectoryReferencesAssembly { get; }

        /// <summary>
        /// Gets the directory paths that are implicitly already covered because an assembly from this folder has already been added to the dllImport attribute.
        /// </summary>
        /// <value>The directory paths that are implicitly already covered because an assembly from this folder has already been added to the dllImport attribute.</value>
        public ISet<string> ImplicitDllImportDirectoryReferences { get; }

        /// <summary>
        /// Gets the names of all assemblies that have been processed.
        /// </summary>
        /// <value>The names of all assemblies that have been processed.</value>
        public ISet<string> ProcessedAssemblies { get; }

        /// <summary>
        /// Returns a string that provides a detailed, multi-line summary of the current assembly and reference
        /// collections managed by this instance.
        /// </summary>
        /// <remarks>The returned string is intended for diagnostic or debugging purposes and includes all
        /// relevant reference collections tracked by the instance. The format may change if the structure of the
        /// collections changes.</remarks>
        /// <returns>A formatted string containing counts and lists of NuGet assemblies, DllImport references, directory
        /// references, and processed assemblies. Each collection is displayed on a separate line for readability.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"NuGetAssemblies: {NugetAssemblies.Count}");
            foreach (PackageAssemblyReference reference in NugetAssemblies)
            {
                sb.AppendLine($"\t{reference}");
            }

            sb.AppendLine();
            sb.AppendLine($"DllImportNugetAssemblyReferences: {DllImportNugetAssemblyReferences.Count}");
            foreach (PackageAssemblyReference reference in DllImportNugetAssemblyReferences)
            {
                sb.AppendLine($"\t{reference}");
            }

            sb.AppendLine();
            sb.AppendLine($"DllImportFrameworkAssemblyReferences: {DllImportFrameworkAssemblyReferences.Count}");
            foreach (string reference in DllImportFrameworkAssemblyReferences)
            {
                sb.AppendLine($"\t{reference}");
            }

            sb.AppendLine();
            sb.AppendLine($"DllImportDirectoryReferences: {DllImportDirectoryReferences.Count}");
            foreach (string reference in DllImportDirectoryReferences)
            {
                sb.AppendLine($"\t{reference}");
            }

            sb.AppendLine();
            sb.AppendLine($"DllImportDirectoryReferencesAssembly: {DllImportDirectoryReferencesAssembly.Count}");
            foreach (var pair in DllImportDirectoryReferencesAssembly)
            {
                sb.AppendLine($"\t{pair.Key}|{pair.Value}");
            }

            sb.AppendLine();
            sb.AppendLine($"ImplicitDllImportDirectoryReferences: {ImplicitDllImportDirectoryReferences.Count}");
            foreach (string reference in ImplicitDllImportDirectoryReferences)
            {
                sb.AppendLine($"\t{reference}");
            }

            sb.AppendLine();
            sb.AppendLine($"ProcessedAssemblies: {ProcessedAssemblies.Count}");
            foreach (string processedAssembly in ProcessedAssemblies)
            {
                sb.AppendLine($"\t{processedAssembly}");
            }

            return sb.ToString();
        }
    }
}