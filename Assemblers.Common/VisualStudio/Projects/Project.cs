namespace Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using Microsoft.Build.Evaluation;

    using NuGet.Packaging;

    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Represents a Visual Studio project file.
    /// </summary>
    public class Project
    {
        private static readonly IFileSystem FileSystem = CICD.FileSystem.FileSystem.Instance;
        private readonly ICollection<ProjectFile> _files = new List<ProjectFile>();
        private readonly ICollection<Reference> _references = new List<Reference>();
        private readonly ICollection<ProjectReference> _projectReferences = new List<ProjectReference>();
        private readonly ICollection<PackageReference> _packageReferences = new List<PackageReference>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class with the specified name and project files.
        /// </summary>
        /// <param name="name">The project name.</param>
        /// <param name="files">The project files.</param>
        public Project(string name, ICollection<ProjectFile> files)
        {
            AssemblyName = name;
            _files = files;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class with the specified name, path, project files, references, package references and project references.
        /// This will not load in the data if this path would be an existing file.
        /// </summary>
        /// <param name="name">The project name.</param>
        /// <param name="path">The project file path.</param>
        /// <param name="tfm">The Target Framework Moniker.</param>
        /// <param name="projectFiles">The project files.</param>
        /// <param name="references">The references.</param>
        /// <param name="packageReferences">The package references.</param>
        /// <param name="projectReferences">The project references.</param>
        [EditorBrowsable(EditorBrowsableState.Never)] // Used by DIS
        public Project(string name, string path = null, string tfm = null, ICollection<ProjectFile> projectFiles = null, ICollection<Reference> references = null, ICollection<PackageReference> packageReferences = null, ICollection<ProjectReference> projectReferences = null)
        {
            AssemblyName = name;
            Path = path;
            TargetFrameworkMoniker = tfm;

            if (projectFiles != null)
            {
                _files = projectFiles;
            }

            if (references != null)
            {
                _references = references;
            }

            if (packageReferences != null)
            {
                _packageReferences = packageReferences;
            }

            if (_projectReferences != null)
            {
                _projectReferences = projectReferences;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        private Project()
        {
        }

        /// <summary>
        /// Gets the project name.
        /// </summary>
        public string ProjectName { get; private set; }

        /// <summary>
        /// Gets the project directory.
        /// </summary>
        public string ProjectDirectory { get; private set; }

        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public string AssemblyName { get; private set; }

        /// <summary>
        /// Gets the project path.
        /// </summary>
        /// <value>The project path.</value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the target framework moniker (TFM).
        /// </summary>
        /// <value>The target framework moniker (TFM).</value>
        public string TargetFrameworkMoniker { get; private set; }

        /// <summary>
        /// Gets the DataMiner project type.
        /// </summary>
        public DataMinerProjectType? DataMinerProjectType { get; private set; }

        /// <summary>
        /// Gets the project C# files. These only include the ones that will be compiled. Files marked as e.g. content won't be included in this list.
        /// </summary>
        /// <value>The project C# files.</value>
        public IEnumerable<ProjectFile> Files => _files;

        /// <summary>
        /// Gets the project references.
        /// </summary>
        /// <value>The project references.</value>
        public IEnumerable<Reference> References => _references;

        /// <summary>
        /// Gets the project references.
        /// </summary>
        /// <value>The project references.</value>
        public IEnumerable<ProjectReference> ProjectReferences => _projectReferences;

        /// <summary>
        /// Gets the package references.
        /// </summary>
        /// <value>The package references.</value>
        public IEnumerable<PackageReference> PackageReferences => _packageReferences;

        /// <summary>
        /// Gets the style of the project.
        /// </summary>
        public ProjectStyle ProjectStyle { get; private set; }

        /// <summary>
        /// Loads the projects with the specified path.
        /// </summary>
        /// <param name="path">The path of the project file to load.</param>
        /// <returns>The loaded project.</returns>
        /// <exception cref="FileNotFoundException">The file specified in <paramref name="path"/> does not exist.</exception>
        public static Project Load(string path)
        {
            // Make sure to use the full path
            path = FileSystem.Path.GetFullPath(path);

            if (!FileSystem.File.Exists(path))
            {
                throw new FileNotFoundException("Could not find project file: " + path);
            }

            // .shproj files import Visual Studio CodeSharing targets that may not be available
            // outside of Visual Studio (e.g. when using the .NET SDK MSBuild). Redirect to the
            // referenced .projitems file which contains the actual compile items.
            if (FileSystem.Path.GetExtension(path).Equals(".shproj", StringComparison.OrdinalIgnoreCase))
            {
                string projItemsPath = TryGetProjItemsPath(path);
                if (projItemsPath != null)
                {
                    return Load(projItemsPath);
                }
            }

            using (var projectCollection = new ProjectCollection())
            {
                projectCollection.DisableMarkDirty = true;

                var loadedProject = projectCollection.LoadProject(path);

                string projectName = loadedProject.GetPropertyValue("MSBuildProjectName");

                try
                {
                    var project = new Project
                    {
                        AssemblyName = loadedProject.GetProperty("AssemblyName")?.EvaluatedValue,
                        Path = path,
                        ProjectStyle = loadedProject.Imports.Any(i => i.SdkResult != null) ? ProjectStyle.Sdk : ProjectStyle.Legacy,
                        ProjectDirectory = loadedProject.DirectoryPath,
                        ProjectName = projectName,
                        TargetFrameworkMoniker = loadedProject.GetPropertyValue("TargetFrameworkMoniker"),
                        DataMinerProjectType = DataMinerProjectTypeConverter.ToEnum(loadedProject.GetProperty("DataMinerType")?.EvaluatedValue)
                    };

                    project._references.AddRange(loadedProject.GetItems("Reference")
                                                              .Select(r => new Reference(r.EvaluatedInclude, r.GetMetadataValue("HintPath"))));
                    project._projectReferences.AddRange(loadedProject.GetItems("ProjectReference")
                                                                     .Select(r =>
                                                                     {
                                                                         string name = r.GetMetadataValue("Name");
                                                                         return new ProjectReference(
                                                                             String.IsNullOrEmpty(name)
                                                                                 ? FileSystem.Path.GetFileNameWithoutExtension(r.EvaluatedInclude)
                                                                                 : name,
                                                                             r.EvaluatedInclude,
                                                                             r.GetMetadataValue("Project"));
                                                                     }));
                    bool isCpm = String.Equals(loadedProject.GetPropertyValue("ManagePackageVersionsCentrally"), "true", StringComparison.OrdinalIgnoreCase);
                    Dictionary<string, string> packageVersions = null;
                    if (isCpm)
                    {
                        packageVersions = loadedProject.GetItems("PackageVersion")
                                                       .ToDictionary(pv => pv.EvaluatedInclude, pv => pv.GetMetadataValue("Version"), StringComparer.OrdinalIgnoreCase);
                    }

                    project._packageReferences.AddRange(loadedProject.GetItems("PackageReference")
                                                                     .Select(r =>
                                                                     {
                                                                         string version = r.GetMetadataValue("Version");

                                                                         if (isCpm && String.IsNullOrEmpty(version))
                                                                         {
                                                                             string versionOverride = r.GetMetadataValue("VersionOverride");
                                                                             if (!String.IsNullOrEmpty(versionOverride))
                                                                             {
                                                                                 version = versionOverride;
                                                                             }
                                                                             else if (packageVersions.TryGetValue(r.EvaluatedInclude, out string centralVersion))
                                                                             {
                                                                                 version = centralVersion;
                                                                             }
                                                                         }

                                                                         return new PackageReference(r.EvaluatedInclude, version);
                                                                     }));

                    project._files.AddRange(loadedProject.GetItems("Compile")
                                                        .Select(i => new ProjectFile(i.EvaluatedInclude, FileSystem.File.ReadAllText(i.GetMetadataValue("FullPath")))));
                    return project;
                }
                catch (Exception e)
                {
                    throw new AssemblerException($"Failed to load project '{projectName}' ({path}).", e);
                }
            }
        }

        private static string TryGetProjItemsPath(string shprojPath)
        {
            try
            {
                XDocument doc = XDocument.Load(shprojPath);
                XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

                XElement sharedImport = doc.Root?.Elements(ns + "Import")
                    .FirstOrDefault(e => String.Equals(e.Attribute("Label")?.Value, "Shared", StringComparison.OrdinalIgnoreCase));

                string relativePath = sharedImport?.Attribute("Project")?.Value;

                if (!String.IsNullOrEmpty(relativePath))
                {
                    string dir = FileSystem.Path.GetDirectoryName(shprojPath);
                    string fullPath = FileSystem.Path.GetFullPath(FileSystem.Path.Combine(dir, relativePath));

                    if (FileSystem.File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }
            catch
            {
                // Fall through to return null so the caller can attempt normal loading.
            }

            return null;
        }
    }
}