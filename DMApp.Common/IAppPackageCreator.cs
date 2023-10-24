namespace Skyline.DataMiner.CICD.DMApp.Common
{
    using System;
    using System.Threading.Tasks;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.Loggers;

    /// <summary>
    /// Package creator interface.
    /// </summary>
    public interface IAppPackageCreator
    {
        /// <summary>
        /// Gets the log collector.
        /// </summary>
        /// <value>The log collector.</value>
        ILogCollector LogCollector { get; }

        /// <summary>
        /// Gets the path of the folder containing the items to be added to the package.
        /// </summary>
        /// <value>The path of the folder containing the items to be added to the package.</value>
        string RepositoryPath { get; }

        /// <summary>
        /// Gets the package name.
        /// </summary>
        /// <value>The package name.</value>
        string PackageName { get; }

        /// <summary>
        /// Gets the package version.
        /// </summary>
        /// <value>The package version.</value>
        DMAppVersion PackageVersion { get; }

        /// <summary>
        /// Adds the items to the package.
        /// </summary>
        /// <param name="appPackageBuilder">The builder to which the content needs to be added.</param>
        /// <returns>Task that adds the content of the <see cref="RepositoryPath"/> directory to the package builder.</returns>
        Task AddItemsAsync(AppPackage.AppPackageBuilder appPackageBuilder);

        /// <summary>
        /// Builds the package.
        /// </summary>
        /// <returns>Task that creates the <see cref="IAppPackage"/> instance.</returns>
        /// <remarks>In case you directly want to create a package, call the <see cref="CreateAsync()"/> method instead.</remarks>
        Task<IAppPackage> BuildPackageAsync();

        /// <summary>
        /// Creates a package.
        /// </summary>
        /// <returns>The package bytes.</returns>
        Task<byte[]> CreateAsync();

        /// <summary>
        /// Creates a package.
        /// </summary>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="packageFileName">The package file name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="destinationFolder"/> or <paramref name="packageFileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="destinationFolder"/>.</exception>
        /// <returns>Task that creates a package with that contains the content the <see cref="RepositoryPath"/> directory.</returns>
        Task CreateAsync(string destinationFolder, DMAppFileName packageFileName);
    }
}