namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Common.Exceptions;

    internal class LegacyStyleParser : IProjectParser
    {
        private static readonly XNamespace Msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
        private static readonly IFileSystem FileSystem = CICD.FileSystem.FileSystem.Instance;

        private readonly XDocument document;
        private readonly string projectDir;

        internal LegacyStyleParser(XDocument document, string projectDir)
        {
            this.document = document;
            this.projectDir = projectDir;
        }

        public ProjectStyle GetProjectStyle()
        {
            return ProjectStyle.Legacy;
        }

        public string GetAssemblyName()
        {
            return document
                .Element(Msbuild + "Project")
                ?.Elements(Msbuild + "PropertyGroup")
                .Elements(Msbuild + "AssemblyName")
                .FirstOrDefault()
                ?.Value;
        }

        public IEnumerable<Reference> GetReferences()
        {
            var references = document
                .Element(Msbuild + "Project")
                ?.Elements(Msbuild + "ItemGroup")
                .Elements(Msbuild + "Reference");

            if (references == null)
            {
                yield break;
            }

            foreach (var r in references)
            {
                string include = r.Attribute("Include")?.Value;
                string hintPath = r.Element(Msbuild + "HintPath")?.Value;

                if (String.IsNullOrWhiteSpace(include))
                {
                    // When updating a Reference that is defined in the Directory.Build.props file (e.g.: for a different HintPath)
                    include = r.Attribute("Update")?.Value;
                }

                yield return new Reference(include, hintPath);
            }
        }

        public IEnumerable<ProjectReference> GetProjectReferences()
        {
            IEnumerable<XElement> references = document
                .Element(Msbuild + "Project")
                ?.Elements(Msbuild + "ItemGroup")
                .Elements(Msbuild + "ProjectReference");

            if (references == null)
            {
                yield break;
            }

            foreach (var r in references)
            {
                string path = r.Attribute("Include")?.Value;
                string guid = r.Element(Msbuild + "Project")?.Value;
                string name = r.Element(Msbuild + "Name")?.Value;

                yield return new ProjectReference(name, path, guid);
            }
        }

        public IEnumerable<PackageReference> GetPackageReferences()
        {
            IEnumerable<XElement> references = document
               .Element(Msbuild + "Project")
               ?.Elements(Msbuild + "ItemGroup")
               .Elements(Msbuild + "PackageReference");

            if (references == null)
            {
                yield break;
            }

            if (references.Any())
            {
                foreach (var item in LoadPackageReferenceItems(references))
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in LoadPackagesConfigItems())
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<PackageReference> LoadPackagesConfigItems()
        {
            var packagesConfigFilePath = FileSystem.Path.Combine(projectDir, "packages.config");

            if (!FileSystem.File.Exists(packagesConfigFilePath))
            {
                yield break;
            }

            var xmlContent = FileSystem.File.ReadAllText(packagesConfigFilePath);
            var packagesConfigDoc = XDocument.Parse(xmlContent);
            // var packagesConfigDoc = XDocument.Load(packagesConfigFilePath);
            var ns = packagesConfigDoc.Root.GetDefaultNamespace();

            IEnumerable<XElement> packageReferences = packagesConfigDoc
                .Element(ns + "packages")
                ?.Elements(ns + "package");

            if (packageReferences == null)
            {
                yield break;
            }

            foreach (var r in packageReferences)
            {
                string name = r.Attribute("id")?.Value;
                string version = r.Attribute("version")?.Value;

                yield return new PackageReference(name, version);
            }
        }

        private static IEnumerable<PackageReference> LoadPackageReferenceItems(IEnumerable<XElement> references)
        {
            foreach (var r in references)
            {
                string name = r.Attribute("Include")?.Value;
                string version = r.Element(Msbuild + "Version")?.Value;

                if (String.IsNullOrWhiteSpace(name))
                {
                    // When updating a NuGet that is defined in the Directory.Build.props file
                    name = r.Attribute("Update")?.Value;
                }

                if (String.IsNullOrWhiteSpace(version))
                {
                    // When installed via CLI the version is added as an attribute instead of a tag.
                    version = r.Attribute("Version")?.Value;
                }

                yield return new PackageReference(name, version);
            }
        }

        public IEnumerable<ProjectFile> GetCompileFiles()
        {
            IEnumerable<string> files = document
                .Element(Msbuild + "Project")
                ?.Elements(Msbuild + "ItemGroup")
                .Elements(Msbuild + "Compile")
                .Select(refElem => refElem.Attribute("Include")?.Value);

            if (files == null)
            {
                yield break;
            }

            foreach (var f in files)
            {
                if (f == null)
                {
                    continue;
                }

                string relativePath = f.Replace("$(MSBuildThisFileDirectory)", String.Empty);
                string absolutePath = !FileSystem.Path.IsPathRooted(relativePath) ? FileSystem.Path.Combine(projectDir, relativePath) : relativePath;

                if (!FileSystem.File.Exists(absolutePath))
                {
                    throw new FileNotFoundException($"File '{relativePath}' was not found. Please add the file or remove it from the project.");
                }

                yield return new ProjectFile(relativePath, FileSystem.File.ReadAllText(FileSystem.Path.GetFullPath(absolutePath), Encoding.UTF8));
            }
        }

        public bool TryGetTargetFrameworkMoniker(out string targetFrameworkMoniker)
        {
            targetFrameworkMoniker = null;

            // Select the "Release" build configuration.
            var configurationGroups = document
                .Element(Msbuild + "Project")
                ?.Elements(Msbuild + "PropertyGroup")
                .Elements(Msbuild + "Configuration");

            if (configurationGroups == null)
            {
                return false;
            }

            return TryGetTargetFrameworkMonikerFromConfigurationGroups(configurationGroups, out targetFrameworkMoniker);
        }

        public bool TryGetTargetFrameworkFromDirectoryBuildProps(out string targetFrameworkMoniker)
        {
            // Not supported in legacy style projects.
            targetFrameworkMoniker = null;
            return false;
        }

        private static bool TryGetTargetFrameworkMonikerFromConfigurationGroups(IEnumerable<XElement> configurationGroups, out string targetFrameworkMoniker)
        {
            targetFrameworkMoniker = null;

            foreach (var configurationGroup in configurationGroups)
            {
                if (configurationGroup.Attribute("Condition")?.Value.Contains("'$(Configuration)' == ''") != true)
                {
                    continue;
                }

                var versionTag = configurationGroup.Parent?.Element(Msbuild + "TargetFrameworkVersion");

                if (versionTag == null || versionTag.Value.Length < 2)
                {
                    return false;
                }

                targetFrameworkMoniker = ".NETFramework,Version=" + versionTag.Value.Substring(1);
                return true;
            }

            return false;
        }

        public DataMinerProjectType? GetDataMinerProjectType() => null; // We don't support this in legacy style

        public IEnumerable<ProjectFile> GetSharedProjectCompileFiles()
        {
            IEnumerable<XElement> imports = document
                                           .Element(Msbuild + "Project")
                                           ?.Elements(Msbuild + "Import");

            if (imports == null)
            {
                yield break;
            }

            foreach (var import in imports)
            {
                foreach (ProjectFile projectFile in LoadFilesFromSharedProject(projectDir, import))
                {
                    yield return projectFile;
                }
            }
        }

        private static IEnumerable<ProjectFile> LoadFilesFromSharedProject(string projectDir, XElement import)
        {
            string project = import.Attribute("Project")?.Value;
            string label = import.Attribute("Label")?.Value;

            if (String.IsNullOrWhiteSpace(project) || String.IsNullOrWhiteSpace(label))
            {
                yield break;
            }

            if (!String.Equals(label, "Shared", StringComparison.OrdinalIgnoreCase) ||
                !project.EndsWith(".projitems", StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            string path = FileSystem.Path.Combine(projectDir, project);
            if (!FileSystem.File.Exists(path))
            {
                yield break;
            }

            var sharedProj = SharedProject.Load(path);

            foreach (var f in sharedProj.Files)
            {
                yield return f;
            }
        }
    }
}
