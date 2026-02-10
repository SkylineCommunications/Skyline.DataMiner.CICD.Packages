namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Parser for Directory.Packages.props files used in Central Package Management.
    /// </summary>
    internal static class DirectoryPackagesPropsParser
    {
        private static readonly IFileSystem FileSystem = CICD.FileSystem.FileSystem.Instance;
        private const string DirectoryPackagesPropsFileName = "Directory.Packages.props";

        /// <summary>
        /// Attempts to find and parse the Directory.Packages.props file for a given project directory.
        /// </summary>
        /// <param name="projectDir">The project directory to start searching from.</param>
        /// <param name="packageVersions">Dictionary of package versions found in Directory.Packages.props.</param>
        /// <returns>True if Directory.Packages.props was found and parsed; otherwise, false.</returns>
        public static bool TryGetPackageVersions(string projectDir, out Dictionary<string, string> packageVersions)
        {
            packageVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string directoryPackagesPropsPath = FindDirectoryPackagesProps(projectDir);
            if (String.IsNullOrEmpty(directoryPackagesPropsPath))
            {
                return false;
            }

            try
            {
                var xmlContent = FileSystem.File.ReadAllText(directoryPackagesPropsPath, Encoding.UTF8);
                var document = XDocument.Parse(xmlContent);

                // Check if Central Package Management is enabled
                var manageCentrallyEnabled = document
                    .Element("Project")
                    ?.Elements("PropertyGroup")
                    .Elements("ManagePackageVersionsCentrally")
                    .FirstOrDefault()
                    ?.Value;

                if (!String.Equals(manageCentrallyEnabled, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                ParsePackageVersions(packageVersions, document);
                ParseGlobalPackageReferences(packageVersions, document);

                return packageVersions.Count > 0;
            }
            catch
            {
                // If parsing fails, return false
                return false;
            }
        }

        private static void ParsePackageVersions(Dictionary<string, string> packageVersions, XDocument document)
        {
            var packageVersionElements = document
                                         .Element("Project")
                                         ?.Elements("ItemGroup")
                                         .Elements("PackageVersion");

            if (packageVersionElements == null)
            {
                return;
            }

            foreach (var packageVersionElement in packageVersionElements)
            {
                string include = packageVersionElement.Attribute("Include")?.Value;
                string version = packageVersionElement.Attribute("Version")?.Value;

                if (!String.IsNullOrWhiteSpace(include) && !String.IsNullOrWhiteSpace(version))
                {
                    // Store the version, later entries override earlier ones
                    packageVersions[include] = version;
                }
            }
        }

        private static void ParseGlobalPackageReferences(Dictionary<string, string> packageVersions, XDocument document)
        {
            var globalPackageReferenceElements = document
                                                 .Element("Project")
                                                 ?.Elements("ItemGroup")
                                                 .Elements("GlobalPackageReference");

            if (globalPackageReferenceElements == null)
            {
                return;
            }

            foreach (var globalPackageElement in globalPackageReferenceElements)
            {
                string include = globalPackageElement.Attribute("Include")?.Value;
                string version = globalPackageElement.Attribute("Version")?.Value;

                if (!String.IsNullOrWhiteSpace(include) && !String.IsNullOrWhiteSpace(version))
                {
                    packageVersions[include] = version;
                }
            }
        }

        /// <summary>
        /// Finds the Directory.Packages.props file by searching up the directory tree from the project directory.
        /// </summary>
        /// <param name="projectDir">The starting directory.</param>
        /// <returns>The full path to the Directory.Packages.props file, or null if not found.</returns>
        private static string FindDirectoryPackagesProps(string projectDir)
        {
            string currentDir = projectDir;

            while (!String.IsNullOrEmpty(currentDir))
            {
                string directoryPackagesPropsPath = FileSystem.Path.Combine(currentDir, DirectoryPackagesPropsFileName);
                if (FileSystem.File.Exists(directoryPackagesPropsPath))
                {
                    return directoryPackagesPropsPath;
                }

                string parentDir = FileSystem.Path.GetDirectoryName(currentDir);
                
                // Prevent infinite loop if we reach the root
                if (String.Equals(currentDir, parentDir, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                currentDir = parentDir;
            }

            return null;
        }
    }
}
