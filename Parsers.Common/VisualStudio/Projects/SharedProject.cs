namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Skyline.DataMiner.CICD.Parsers.Common.Extensions;
    using Skyline.DataMiner.CICD.FileSystem;

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

            string projectDir = FileSystem.Path.GetDirectoryName(path);
            var xmlContent = FileSystem.File.ReadAllText(path, Encoding.UTF8);
            var document = XDocument.Parse(xmlContent);
            // var document = XDocument.Load(path);

            IProjectParser parser = ProjectParserFactory.GetParser(document, projectDir);

            var project = new SharedProject
            {
                Path = path
            };

            project._projectReferences.AddRange(parser.GetProjectReferences());

            project._files.AddRange(parser.GetCompileFiles());
            project._files.AddRange(parser.GetSharedProjectCompileFiles());

            return project;
        }
    }
}