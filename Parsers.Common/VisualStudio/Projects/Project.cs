namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    using NuGet.Frameworks;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Common.Exceptions;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;

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
        /// Gets the type of projects that are supported to be loaded.
        /// </summary>
        internal static readonly string[] SupportedProjectExtensions =
        {
            ".csproj",
            ".projitems",
            ".shproj"
        };

        internal static readonly string[] SharedProjectExtensions =
        {
            ".projitems",
            ".shproj"
        };

        /// <summary>
        /// Gets the project name.
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Gets the project directory.
        /// </summary>
        public string ProjectDirectory { get; set; }

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
        public DataMinerProjectType? DataMinerProjectType { get; set; }

        /// <summary>
        /// Gets the project C# files.
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
        /// <param name="projectName">The name of the project.</param>
        /// <returns>The loaded project.</returns>
        /// <exception cref="FileNotFoundException">The file specified in <paramref name="path"/> does not exist.</exception>
        [Obsolete("Use the Load method with only the path argument.")]
        public static Project Load(string path, string projectName)
        {
            return Load(path);
        }

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

            string projectDir = FileSystem.Path.GetDirectoryName(path);
            string projectName = FileSystem.Path.GetFileNameWithoutExtension(path);

            string extension = FileSystem.Path.GetExtension(path);
            if (!SupportedProjectExtensions.Contains(extension))
            {
                throw new NotImplementedException("Project Load does not support this project type.");
            }

            try
            {
                var xmlContent = FileSystem.File.ReadAllText(path, Encoding.UTF8);
                var document = XDocument.Parse(xmlContent);

                IProjectParser parser = ProjectParserFactory.GetParser(document, projectDir);

                string name = projectName;
                string assemblyName = parser.GetAssemblyName();
                if (!String.IsNullOrEmpty(assemblyName))
                {
                    name = assemblyName;
                }

                var project = new Project
                {
                    AssemblyName = name,
                    Path = path,
                    ProjectStyle = parser.GetProjectStyle(),
                    ProjectDirectory = projectDir,
                    ProjectName = projectName,
                };

                project._references.AddRange(parser.GetReferences());
                project._projectReferences.AddRange(parser.GetProjectReferences());
                project._packageReferences.AddRange(parser.GetPackageReferences());

                var files = parser.GetCompileFiles().ToList();

                project._files.AddRange(files);
                project._files.AddRange(parser.GetSharedProjectCompileFiles());

                // Shared projects do not have TFM, inherit from referencing project.
                if (!SharedProjectExtensions.Contains(extension))
                {
                    if (parser.TryGetTargetFrameworkMoniker(out string targetFrameworkMoniker))
                    {
                        project.TargetFrameworkMoniker = targetFrameworkMoniker;
                    }
                    else if(parser.TryGetTargetFrameworkFromDirectoryBuildProps(out targetFrameworkMoniker))
                    {
                        project.TargetFrameworkMoniker = targetFrameworkMoniker;
                    }
                    else
                    {
                        throw new ParserException($"Could not determine Target Framework Moniker for project '{projectName}' ({path}).");
                    }
                }

                project.DataMinerProjectType = parser.GetDataMinerProjectType();

                return project;
            }
            catch (Exception e)
            {
                throw new ParserException($"Failed to load project '{projectName}' ({path}).", e);
            }
        }
    }
}