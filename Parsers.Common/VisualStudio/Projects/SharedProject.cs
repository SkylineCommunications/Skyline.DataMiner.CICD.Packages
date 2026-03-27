namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    using Microsoft.Build.Evaluation;

    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Parsers.Common.Exceptions;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;

    /// <summary>
    /// Represents a Visual Studio project file.
    /// </summary>
    public class SharedProject
    {
        private static readonly IFileSystem FileSystem = CICD.FileSystem.FileSystem.Instance;
        private readonly ICollection<ProjectFile> _files = new List<ProjectFile>();
        private readonly ICollection<ProjectReference> _projectReferences = new List<ProjectReference>(); // Only supports referencing other shared projects.

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        private SharedProject()
        {
        }

        /// <summary>
        /// Gets the project path.
        /// </summary>
        /// <value>The project path.</value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the project files.
        /// </summary>
        /// <value>The project files.</value>
        public IEnumerable<ProjectFile> Files => _files;

        /// <summary>
        /// Gets the project references.
        /// </summary>
        /// <value>The project references.</value>
        public IEnumerable<ProjectReference> ProjectReferences => _projectReferences;

        /// <summary>
        /// Loads the projects with the specified path.
        /// </summary>
        /// <param name="path">The path of the project file to load.</param>
        /// <returns>The loaded project.</returns>
        /// <exception cref="FileNotFoundException">The file specified in <paramref name="path"/> does not exist.</exception>
        public static SharedProject Load(string path)
        {
            if (!FileSystem.File.Exists(path))
            {
                throw new FileNotFoundException("Could not find project file: " + path);
            }

            using (var projectCollection = new ProjectCollection())
            {
                projectCollection.DisableMarkDirty = true;

                var loadedProject = projectCollection.LoadProject(path);

                string projectName = loadedProject.GetPropertyValue("MSBuildProjectName");

                try
                {
                    var project = new SharedProject
                    {
                        Path = path
                    };

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
                    project._files.AddRange(loadedProject.GetItems("Compile")
                                                        .Select(i => new ProjectFile(i.EvaluatedInclude, FileSystem.File.ReadAllText(i.GetMetadataValue("FullPath")))));
                    return project;
                }
                catch (Exception e)
                {
                    throw new ParserException($"Failed to load project '{projectName}' ({path}).", e);
                }
            }
        }
    }
}