namespace Skyline.DataMiner.CICD.Assemblers.Common
{
    /// <summary>
    /// Represents an assembly reference of a NuGet package.
    /// </summary>
    public class PackageAssemblyReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageAssemblyReference"/> class.
        /// </summary>
        /// <param name="dllImport">The import value for the QAction@dllImport attribute or Exe/Param element.</param>
        /// <param name="assemblyPath">The assembly file path.</param>
        public PackageAssemblyReference(string dllImport, string assemblyPath)
        {
            DllImport = dllImport;
            AssemblyPath = assemblyPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageAssemblyReference"/> class.
        /// </summary>
        /// <param name="dllImport">The import value for the QAction@dllImport attribute or Exe/Param element.</param>
        /// <param name="assemblyPath">The assembly file path.</param>
        /// <param name="isFilesPackage">Determine if this is a package of the Files folder.</param>
        public PackageAssemblyReference(string dllImport, string assemblyPath, bool isFilesPackage) : this(dllImport, assemblyPath)
        {
            IsFilesPackage = isFilesPackage;
        }

        /// <summary>
        /// Gets the dllImport value.
        /// </summary>
        /// <value>The dllImport value.</value>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <description>This is the value after C:\Skyline DataMiner\ProtocolScripts\DllImport\.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public string DllImport { get; }

        /// <summary>
        /// Gets the Local path for the reference.
        /// </summary>
        /// <value>The Local path for the reference.</value>
        public string AssemblyPath { get; }

        /// <summary>
        /// Defines if this references is a 'Skyline.DataMiner.Files' package.
        /// </summary>
        public bool IsFilesPackage { get; }

        public override string ToString()
        {
            return $"{DllImport}|{AssemblyPath}|{IsFilesPackage}";
        }
    }
}
