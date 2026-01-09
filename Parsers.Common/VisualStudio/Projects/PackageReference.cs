namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    /// <summary>
    /// Represents a NuGet package reference.
    /// </summary>
    public class PackageReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageReference"/> class.
        /// </summary>
        /// <param name="name">The name of the package.</param>
        /// <param name="version">The version of the package.</param>
        public PackageReference(string name, string version)
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// Gets the name of the NuGet package.
        /// </summary>
        /// <value>The name of the NuGet package.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the version of the NuGet package.
        /// </summary>
        /// <value>The version of the NuGet package.</value>
        public string Version { get; }
    }
}