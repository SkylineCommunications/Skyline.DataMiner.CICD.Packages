namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using NuGet.Frameworks;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Common.Exceptions;

    internal class SdkStyleParser : IProjectParser
    {
        private static readonly IFileSystem FileSystem = CICD.FileSystem.FileSystem.Instance;

        private readonly XDocument document;
        private readonly string projectDir;

        internal SdkStyleParser(XDocument document, string projectDir)
        {
            this.document = document ?? throw new ArgumentNullException(nameof(document));
            this.projectDir = projectDir;
        }

        public ProjectStyle GetProjectStyle()
        {
            return ProjectStyle.Sdk;
        }

        public string GetAssemblyName()
        {
            // Note: by default this element is not present. If not present it is the same as the MSBuild project name (handled in higher level).
            return document
                .Element("Project")
                ?.Elements("PropertyGroup")
                .Elements("AssemblyName")
                .FirstOrDefault()
                ?.Value;
        }

        public IEnumerable<Reference> GetReferences()
        {
            var references = document
                .Element("Project")
                ?.Elements("ItemGroup")
                .Elements("Reference");

            if (references == null)
            {
                yield break;
            }

            foreach (var r in references)
            {
                string include = r.Attribute("Include")?.Value;
                string hintPath = r.Element("HintPath")?.Value;

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
                .Element("Project")
                ?.Elements("ItemGroup")
                .Elements("ProjectReference");

            if (references == null)
            {
                yield break;
            }

            foreach (var r in references)
            {
                string path = r.Attribute("Include")?.Value;

                // Name is not provided in .csproj file. Name is for now taken from file name. This might need to change if the assembly name does not match the project name.
                string name = path;
                if (path.Contains("\\"))
                {
                    name = name.Substring(path.LastIndexOf("\\") + 1);
                }

                name = name.Replace(".csproj", "");

                string guid = Guid.Empty.ToString(); // Is not provided in .csproj file.

                yield return new ProjectReference(name, path, guid);
            }
        }

        public IEnumerable<PackageReference> GetPackageReferences()
        {
            IEnumerable<XElement> references = document
               .Element("Project")
               ?.Elements("ItemGroup")
               .Elements("PackageReference");

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
        }

        public IEnumerable<ProjectFile> GetCompileFiles()
        {
            // https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview#default-includes-and-excludes
            // Note: current implementation does not take into account possible usage of EnableDefaultItems and EnableDefaultCompileItems.
            string[] files = FileSystem.Directory.GetFiles(projectDir, "*.cs", System.IO.SearchOption.TopDirectoryOnly);

            int relativePathOffset = projectDir.Length + 1;

            foreach (var file in files)
            {
                string relativePath = file.Substring(relativePathOffset);
                yield return new ProjectFile(relativePath, FileSystem.File.ReadAllText(file, System.Text.Encoding.UTF8));
            }

            var directories = FileSystem.Directory.EnumerateDirectories(projectDir);

            foreach (var directory in directories)
            {
                string directoryName = directory.Substring(relativePathOffset);

                if (directoryName.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                    directoryName.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
                    directoryName.Equals(".vs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string[] filesInDirectory = FileSystem.Directory.GetFiles(directory, "*.cs", System.IO.SearchOption.AllDirectories);

                foreach (var file in filesInDirectory)
                {
                    string relativePath = file.Substring(relativePathOffset);
                    yield return new ProjectFile(relativePath, FileSystem.File.ReadAllText(file, System.Text.Encoding.UTF8));
                }
            }
        }

        public string GetTargetFrameworkMoniker()
        {
            var propertyGroups = document
                                 ?.Element("Project")
                                 ?.Elements("PropertyGroup");

            if (propertyGroups == null)
            {
                throw new ParserException("No PropertyGroup tags found in the csproj file!");
            }

            foreach (XElement propertyGroup in propertyGroups)
            {
                var targetFrameworkElement = propertyGroup.Element("TargetFramework") ?? propertyGroup.Element("TargetFrameworks");

                if (targetFrameworkElement == null)
                {
                    continue;
                }

                // SDK style projects support multi-targeting. Return first item.
                string tfms = targetFrameworkElement.Value;

                // https://learn.microsoft.com/en-us/dotnet/standard/frameworks
                string sdkStyleTfm = tfms.Split(';')[0];
                var tfm = NuGetFramework.ParseFolder(sdkStyleTfm);

                return tfm.DotNetFrameworkName;
            }

            throw new ParserException("No TargetFramework tag found in the csproj file!");
        }

        public DataMinerProjectType? GetDataMinerProjectType()
        {
            var propertyGroups = document
                                 ?.Element("Project")
                                 ?.Elements("PropertyGroup");

            if (propertyGroups == null)
            {
                throw new ParserException("No PropertyGroup tags found in the csproj file!");
            }

            foreach (XElement propertyGroup in propertyGroups)
            {
                var typeElement = propertyGroup.Element("DataMinerType");

                if (typeElement == null)
                {
                    continue;
                }

                return DataMinerProjectTypeConverter.ToEnum(typeElement.Value);
            }

            // Tag does not exist.
            return null;
        }

        public IEnumerable<ProjectFile> GetSharedProjectCompileFiles()
        {
            IEnumerable<XElement> imports = document
                                           .Element("Project")
                                           ?.Elements("Import");

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

        private static IEnumerable<PackageReference> LoadPackageReferenceItems(IEnumerable<XElement> references)
        {
            foreach (var r in references)
            {
                string name = r.Attribute("Include")?.Value;
                string version = r.Element("Version")?.Value;

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
    }
}
