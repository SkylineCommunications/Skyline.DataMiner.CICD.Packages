namespace Skyline.DataMiner.CICD.DMApp.Common
{
    using System;
    using System.Threading.Tasks;

    using Skyline.AppInstaller;
    using Skyline.DataMiner.CICD.Common;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    /// <summary>
    /// Base class for package creators. Deriving classes implement the <see cref="IAppPackageCreator.AddItemsAsync"/> method which will process the items in the content folder and adds these to the package builder.
    /// </summary>
    public abstract class AppPackageCreator : IAppPackageCreator
    {
        /// <summary>
        /// Gets the <see cref="IFileSystem"/> that is used throughout the class.
        /// </summary>
        protected readonly IFileSystem FileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppPackageCreator"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="logCollector">The log collector.</param>
        /// <param name="repositoryPath">The repository path.</param>
        /// <param name="packageName">The name of the package.</param>
        /// <param name="packageVersion">The version of the package.</param>
        /// <exception cref="ArgumentNullException"><paramref name="logCollector"/>, <paramref name="repositoryPath"/>, <paramref name="packageName"/>, <paramref name="packageVersion"/> or <paramref name="packageName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="packageName"/>, <paramref name="packageVersion"/> or <paramref name="packageName"/> is empty or whitespace.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">The directory specified in <paramref name="repositoryPath"/> does not exist.</exception>
        protected AppPackageCreator(IFileSystem fileSystem, ILogCollector logCollector, string repositoryPath, string packageName, DMAppVersion packageVersion)
        {
            if (logCollector == null)
            {
                throw new ArgumentNullException(nameof(logCollector));
            }

            if (repositoryPath == null)
            {
                throw new ArgumentNullException(nameof(repositoryPath));
            }

            if (packageName == null)
            {
                throw new ArgumentNullException(nameof(packageName));
            }

            if (packageVersion == null)
            {
                throw new ArgumentNullException(nameof(packageVersion));
            }

            if (String.IsNullOrWhiteSpace(packageName))
            {
                throw new ArgumentException("Value can not be empty or whitespace.", nameof(packageName));
            }

            if (!fileSystem.Directory.Exists(repositoryPath))
            {
                throw new System.IO.DirectoryNotFoundException("The specified directory '" + repositoryPath + "' does not exist.");
            }

            RepositoryPath = fileSystem.Path.GetFullPath(repositoryPath);
            LogCollector = logCollector;
            FileSystem = fileSystem;

            PackageName = packageName;
            PackageVersion = packageVersion;
        }

        /// <summary>
        /// Gets the log collector.
        /// </summary>
        /// <value>The log collector.</value>
        public ILogCollector LogCollector { get; }

        /// <summary>
        /// Gets the path of the folder containing the items to be added to the package.
        /// </summary>
        /// <value>The path of the folder containing the items to be added to the package.</value>
        public string RepositoryPath { get; }

        /// <summary>
        /// Gets the package name.
        /// </summary>
        /// <value>The package name.</value>
        public string PackageName { get; }

        /// <summary>
        /// Gets the package version.
        /// </summary>
        /// <value>The package version.</value>
        public DMAppVersion PackageVersion { get; }

        /// <summary>
        /// Adds the content of the <see cref="RepositoryPath"/> directory to the package builder.
        /// </summary>
        /// <param name="appPackageBuilder">The builder to which the content needs to be added.</param>
        /// <returns>Task that adds the content of the <see cref="RepositoryPath"/> directory to the package builder.</returns>
        public abstract Task AddItemsAsync(AppPackage.AppPackageBuilder appPackageBuilder);

        /// <summary>
        /// Builds the package.
        /// </summary>
        /// <returns>Task that creates the <see cref="IAppPackage"/> instance.</returns>
        /// <remarks>In case you directly want to create a package, call the <see cref="CreateAsync()"/> method instead.</remarks>
        public async Task<IAppPackage> BuildPackageAsync()
        {
            LogCollector.ReportStatus("Package Info: PackageName '" + PackageName + "' - packageVersion '" + PackageVersion + "'");

            // Create a new package builder.
            var appPackageBuilder = new AppPackage.AppPackageBuilder(PackageName, PackageVersion.ToString(), GlobalDefaults.MinimumSupportDataMinerVersionForDMApp);

            // Add items from the content folder to the package via the builder.
            await AddItemsAsync(appPackageBuilder);

            // Build the package.
            var appPackage = appPackageBuilder.Build();
            return appPackage;
        }

        /// <summary>
        /// Creates a package with that contains the content the <see cref="RepositoryPath"/> directory.
        /// </summary>
        /// <returns>The package bytes.</returns>
        /// <remarks>The method both builds and creates the package.</remarks>
        public async Task<byte[]> CreateAsync()
        {
            IAppPackage appPackage = await BuildPackageAsync();

            return appPackage.CreatePackage();
        }

        /// <summary>
        /// Creates a package with that contains the content the <see cref="RepositoryPath"/> directory.
        /// </summary>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="packageFileName">The package file name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="destinationFolder"/> or <paramref name="packageFileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="destinationFolder"/> is empty or white space.</exception>
        /// <returns>Task that creates a package with that contains the content the <see cref="RepositoryPath"/> directory.</returns>
        public Task CreateAsync(string destinationFolder, DMAppFileName packageFileName)
        {
            if (destinationFolder == null)
            {
                throw new ArgumentNullException(nameof(destinationFolder));
            }

            if (packageFileName == null)
            {
                throw new ArgumentNullException(nameof(packageFileName));
            }

            if (String.IsNullOrWhiteSpace(destinationFolder))
            {
                throw new ArgumentException("The destination folder must not be empty or white space.", nameof(destinationFolder));
            }

            return CreatePackageAsync(destinationFolder, packageFileName);
        }

        private async Task CreatePackageAsync(string destinationFolder, DMAppFileName packageFileName)
        {
            IAppPackage appPackage = await BuildPackageAsync();

            // Serialize the package.
            appPackage.CreatePackage(FileSystem.Path.Combine(destinationFolder, packageFileName.ToString()));
        }
    }
}