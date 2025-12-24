namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.SolutionParser;

    /// <summary>
    /// Represents a Visual Studio solution.
    /// </summary>
    public class Solution
    {
        private readonly Dictionary<Guid, Project> _loadedProjects = new Dictionary<Guid, Project>();
        private readonly Dictionary<Guid, SolutionItem> _solutionItems = new Dictionary<Guid, SolutionItem>();
        private readonly IFileSystem _fileSystem = FileSystem.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="Solution"/> class with the specified path.
        /// </summary>
        /// <param name="path">The path to the solution file.</param>
        /// <exception cref="FileNotFoundException">The specified solution file does not exist.</exception>
        protected Solution(string path) : this(path, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Solution"/> class with the specified path.
        /// Not all projects will be able to be loaded with the <see cref="LoadProject"/> method.
        /// </summary>
        /// <param name="path">The path to the solution file.</param>
        /// <param name="allowAll">Allow all projects, regardless of type.</param>
        /// <exception cref="FileNotFoundException">The specified solution file does not exist.</exception>
        private Solution(string path, bool allowAll)
        {
            if (!_fileSystem.File.Exists(path))
            {
                throw new FileNotFoundException("Could not find the specified solution file '" + path + "'.");
            }

            // Make sure to use the full path
            path = _fileSystem.Path.GetFullPath(path);

            SolutionPath = path;
            SolutionDirectory = _fileSystem.Path.GetDirectoryName(path);

            Load(allowAll);
        }

        /// <summary>
        /// Gets the solution file path.
        /// </summary>
        /// <value>The solution file path.</value>
        public string SolutionPath { get; }

        /// <summary>
        /// Gets the solution directory.
        /// </summary>
        /// <value>The solution directory.</value>
        public string SolutionDirectory { get; }

        /// <summary>
        /// Gets the solution items.
        /// </summary>
        /// <value>The solution items.</value>
        public IReadOnlyDictionary<Guid, SolutionItem> SolutionItems => _solutionItems;

        /// <summary>
        /// Gets the folders.
        /// </summary>
        /// <value>The folders.</value>
        public IEnumerable<SolutionFolder> Folders => SolutionItems.Values.OfType<SolutionFolder>();

        /// <summary>
        /// Gets the projects.
        /// </summary>
        /// <value>The projects.</value>
        public IEnumerable<ProjectInSolution> Projects => SolutionItems.Values.OfType<ProjectInSolution>();

        /// <summary>
        /// Loads the specified solution path.
        /// </summary>
        /// <param name="solutionPath">The solution path.</param>
        /// <param name="logCollector">The log collector.</param>
        /// <returns>Parsed solution.</returns>
        /// <exception cref="ArgumentException">Value cannot be null or whitespace. - solutionPath</exception>
        /// <exception cref="FileNotFoundException">The specified solution file does not exist.</exception>
        public static Solution Load(string solutionPath, ILogCollector logCollector = null)
        {
            if (String.IsNullOrWhiteSpace(solutionPath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(solutionPath));
            }

            logCollector?.ReportDebug($"Creating solution from '{solutionPath}'.");

            return new Solution(solutionPath);
        }

        /// <summary>
        /// Loads the specified solution path.
        /// </summary>
        /// <param name="solutionPath">The solution path.</param>
        /// <param name="allowAll">Allow all projects, regardless of type.</param>
        /// <param name="logCollector">The log collector.</param>
        /// <returns>Parsed solution.</returns>
        /// <exception cref="ArgumentException">Value cannot be null or whitespace. - solutionPath</exception>
        /// <exception cref="FileNotFoundException">The specified solution file does not exist.</exception>
        public static Solution Load(string solutionPath, bool allowAll, ILogCollector logCollector = null)
        {
            if (String.IsNullOrWhiteSpace(solutionPath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(solutionPath));
            }

            logCollector?.ReportDebug($"Creating solution from '{solutionPath}'.");

            return new Solution(solutionPath, allowAll);
        }

        /// <summary>
        /// Loads the project of the solution.
        /// </summary>
        /// <param name="projectInSolution">The project of the solution.</param>
        /// <returns>The loaded project.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="projectInSolution"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">Specified project is not supported to be loaded.</exception>
        public Project LoadProject(ProjectInSolution projectInSolution)
        {
            if (projectInSolution is null)
            {
                throw new ArgumentNullException(nameof(projectInSolution));
            }

            if (!Project.SupportedProjectExtensions.Contains(_fileSystem.Path.GetExtension(projectInSolution.AbsolutePath)))
            {
                throw new NotSupportedException($"Project {projectInSolution.Name} is not supported to be loaded.");
            }

            if (!_loadedProjects.TryGetValue(projectInSolution.Guid, out Project project))
            {
                project = Project.Load(projectInSolution.AbsolutePath, projectInSolution.Name);
                _loadedProjects.Add(projectInSolution.Guid, project);
            }

            return project;
        }

        /// <summary>
        /// Retrieves the subfolder with the specified name.
        /// </summary>
        /// <param name="name">The name of the subfolder.</param>
        /// <returns>The specified subfolder.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
        public SolutionFolder GetSubFolder(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
            }

            return Folders.FirstOrDefault(x => String.Equals(x.Name, name) && x.Parent == null);
        }

        private void AddSolutionItem(Guid guid, SolutionItem item)
        {
            _solutionItems[guid] = item;
        }

        private void Load(bool allowAll)
        {
            var content = _fileSystem.File.ReadAllText(SolutionPath);
            var parser = new Parser(content);

            var projects = parser.ParseProjects();
            var globalSections = parser.ParseGlobalSections();

            foreach (var p in projects)
            {
                ProcessProject(p, allowAll);
            }

            var nestedProjects = globalSections.FirstOrDefault(section => String.Equals(section.Name, "NestedProjects", StringComparison.OrdinalIgnoreCase));
            if (nestedProjects == null)
            {
                return;
            }

            foreach (var e in nestedProjects.Entries)
            {
                if (Guid.TryParse(e.Value, out var parentGuid) && SolutionItems.TryGetValue(parentGuid, out var parent) && parent is SolutionFolder parentFolder
                    && Guid.TryParse(e.Key, out var childGuid) && SolutionItems.TryGetValue(childGuid, out var child))
                {
                    parentFolder.AddChild(child);
                    child.Parent = parentFolder;
                }
            }
        }

        private void ProcessProject(SolutionParser.Model.SlnProject p, bool allowAll)
        {
            SolutionItem solutionItem;

            if (p.TypeGuid == SolutionProjectTypeIDs.SolutionFolder)
            {
                SolutionFolder solutionFolder = new SolutionFolder(this, p);
                solutionItem = solutionFolder;

                var solutionItemsSection = p.ProjectSections.FirstOrDefault(x => String.Equals(x.Name, "SolutionItems", StringComparison.OrdinalIgnoreCase));
                if (solutionItemsSection != null)
                {
                    foreach (var e in solutionItemsSection.Entries)
                    {
                        solutionFolder.AddFile(new SolutionFileEntry(this, e.Value));
                    }
                }
            }
            else if (allowAll)
            {
                // Allow all projects to be added
                solutionItem = new ProjectInSolution(this, p);
            }
            else if (p.TypeGuid == SolutionProjectTypeIDs.MsBuildProject || p.TypeGuid == SolutionProjectTypeIDs.NetCoreProject)
            {
                // Allow only the C# projects that can be loaded with Project.Load
                solutionItem = new ProjectInSolution(this, p);
            }
            else
            {
                return;
            }

            AddSolutionItem(solutionItem.Guid, solutionItem);
        }
    }
}