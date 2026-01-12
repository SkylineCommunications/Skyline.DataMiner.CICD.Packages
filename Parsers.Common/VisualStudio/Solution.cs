namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;

    using Microsoft.VisualStudio.SolutionPersistence;
    using Microsoft.VisualStudio.SolutionPersistence.Model;
    using Microsoft.VisualStudio.SolutionPersistence.Serializer;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;
    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;

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
        /// <exception cref="System.IO.FileNotFoundException">The specified solution file does not exist.</exception>
        protected Solution(string path) : this(path, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Solution"/> class with the specified path.
        /// Not all projects will be able to be loaded with the <see cref="LoadProject"/> method.
        /// </summary>
        /// <param name="path">The path to the solution file.</param>
        /// <param name="allowAll">Allow all projects, regardless of type.</param>
        /// <exception cref="System.IO.FileNotFoundException">The specified solution file does not exist.</exception>
        private Solution(string path, bool allowAll)
        {
            if (!_fileSystem.File.Exists(path))
            {
                throw new System.IO.FileNotFoundException("Could not find the specified solution file '" + path + "'.");
            }

            // Make sure to use the full path
            path = _fileSystem.Path.GetFullPath(path);

            SolutionPath = path;
            SolutionDirectory = _fileSystem.Path.GetDirectoryName(path);

            LoadSolution(allowAll);
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
        /// <exception cref="System.IO.FileNotFoundException">The specified solution file does not exist.</exception>
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
        /// <exception cref="System.IO.FileNotFoundException">The specified solution file does not exist.</exception>
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
                project = Project.Load(projectInSolution.AbsolutePath);
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

        private void LoadSolution(bool allowAll)
        {
            string solutionPath = SolutionPath;
            string[] filteredProjects = Array.Empty<string>();
            if (FileSystem.Instance.Path.GetExtension(SolutionPath) == ".slnf")
            {
                (solutionPath, filteredProjects) = ParseSolutionFilterFile();

                if (!FileSystem.Instance.File.Exists(solutionPath))
                {
                    throw new System.IO.FileNotFoundException($"Could not find the solution file '{solutionPath}' from the specified solution filter file '{SolutionPath}'.");
                }
            }

            ISolutionSerializer serializer = SolutionSerializers.GetSerializerByMoniker(solutionPath);

            if (serializer == null)
            {
                throw new NotSupportedException($"The solution file '{solutionPath}' is not supported by any available serializer.");
            }

            SolutionModel solution = serializer.OpenAsync(solutionPath, CancellationToken.None).Result;

            Dictionary<Guid, SolutionFolder> solutionFolderMap = new Dictionary<Guid, SolutionFolder>(solution.SolutionFolders.Count);

            ProcessSolutionFolders(solution, solutionFolderMap);
            ProcessSolutionProjects(solution, solutionFolderMap, allowAll, filteredProjects);
        }

        private (string solutionPath, string[] filteredProjects) ParseSolutionFilterFile()
        {
            // .slnf files are solution filter files, which reference a main solution file and the projects to load.
            string slnfJson = FileSystem.Instance.File.ReadAllText(SolutionPath);

            JsonElement filter = JsonElement.Parse(slnfJson);

            if (!filter.TryGetProperty("solution", out JsonElement solutionObject) || solutionObject.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("Invalid solution filter file, no solution property found.");
            }

            if (!solutionObject.TryGetProperty("path", out JsonElement pathObject) || pathObject.ValueKind != JsonValueKind.String ||
                !solutionObject.TryGetProperty("projects", out JsonElement projectsObject) || projectsObject.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("Invalid solution filter file, no path or projects properties found.");
            }

            string solutionPath = pathObject.GetString();
            if (String.IsNullOrWhiteSpace(solutionPath))
            {
                throw new JsonException("Invalid solution filter file, no valid path found.");
            }

            solutionPath = FileSystem.Instance.Path.GetFullPath(FileSystem.Instance.Path.Combine(FileSystem.Instance.Path.GetDirectoryName(SolutionPath), solutionPath));

            string[] filteredProjects = projectsObject.EnumerateArray()
                                                      .Select(projectPath => projectPath.GetString())
                                                      .Where(projectPath => !String.IsNullOrWhiteSpace(projectPath))
                                                      .ToArray();
            return (solutionPath, filteredProjects);
        }

        private void ProcessSolutionFolders(SolutionModel solution, Dictionary<Guid, SolutionFolder> solutionFolderMap)
        {
            if (solution.SolutionFolders.Count == 0)
            {
                return;
            }

            foreach (var folder in solution.SolutionFolders)
            {
                SolutionFolder solutionFolder = new SolutionFolder(this, folder.Id, folder.ActualDisplayName, folder.Name);
                solutionFolderMap[folder.Id] = solutionFolder;

                if (folder.Files?.Count > 0)
                {
                    // Add folder content.
                    foreach (var file in folder.Files)
                    {
                        solutionFolder.AddFile(new SolutionFileEntry(this, file));
                    }
                }

                AddSolutionItem(solutionFolder.Guid, solutionFolder);
            }

            // Establish folder hierarchy after adding all folders.
            foreach (var folder in solution.SolutionFolders)
            {
                if (folder.Parent != null && solutionFolderMap.TryGetValue(folder.Parent.Id, out var parentFolder) && solutionFolderMap.TryGetValue(folder.Id, out var childFolder))
                {
                    parentFolder.AddChild(childFolder);
                    childFolder.Parent = parentFolder;
                }
            }
        }

        private void ProcessSolutionProjects(SolutionModel solution, Dictionary<Guid, SolutionFolder> solutionFolderMap, bool allowAll,
            string[] filteredProjects)
        {
            if (solution.SolutionProjects.Count == 0)
            {
                return;
            }

            foreach (var solutionProject in solution.SolutionProjects)
            {
                if (!allowAll &&
                    solutionProject.TypeId != SolutionProjectTypeIDs.MsBuildProject &&
                    solutionProject.TypeId != SolutionProjectTypeIDs.NetCoreProject)
                {
                    // When not allowing all, skip unsupported project types.
                    continue;
                }
                
                if (filteredProjects.Any() && !filteredProjects.Any(filteredProject =>
                    {
                        var normalizedProjectPath = FileSystem.Instance.Path.GetFullPath(solutionProject.FilePath)
                                                              .TrimEnd(FileSystem.Instance.Path.DirectorySeparatorChar,
                                                                  FileSystem.Instance.Path.AltDirectorySeparatorChar);
                        var normalizedFilterPath = FileSystem.Instance.Path.GetFullPath(filteredProject)
                                                             .TrimEnd(FileSystem.Instance.Path.DirectorySeparatorChar,
                                                                 FileSystem.Instance.Path.AltDirectorySeparatorChar);
                        return normalizedProjectPath.EndsWith(normalizedFilterPath);
                    }))
                {
                    // If filtered projects are specified, skip projects not in the filter.
                    continue;
                }

                SolutionItem solutionItem = new ProjectInSolution(this, solutionProject.Id, solutionProject.ActualDisplayName, solutionProject.FilePath);

                if (solutionProject.Parent is SolutionFolderModel parentFolder)
                {
                    var parentSolutionFolder = solutionFolderMap[parentFolder.Id];

                    solutionItem.Parent = parentSolutionFolder;

                    parentSolutionFolder.AddChild(solutionItem);
                }

                AddSolutionItem(solutionItem.Guid, solutionItem);
            }
        }
    }
}