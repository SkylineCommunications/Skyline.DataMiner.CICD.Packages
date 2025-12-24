namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.SolutionParser.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a project in a solution file.
    /// </summary>
    internal class SlnProject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlnProject"/> class.
        /// </summary>
        /// <param name="typeGuid">The type GUID.</param>
        /// <param name="name">The project name.</param>
        /// <param name="path">The project path.</param>
        /// <param name="guid">The project GUID.</param>
        public SlnProject(Guid typeGuid, string name, string path, Guid guid)
        {
            TypeGuid = typeGuid;
            Name = name;
            Path = path;
            Guid = guid;
        }

        /// <summary>
        /// Gets the project type GUID.
        /// </summary>
        /// <value>The project type GUID.</value>
        public Guid TypeGuid { get; }

        /// <summary>
        /// Gets the project GUID.
        /// </summary>
        /// <value>The project GUID.</value>
        public Guid Guid { get; }

        /// <summary>
        /// Gets the project name.
        /// </summary>
        /// <value>The project name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the project path.
        /// </summary>
        /// <value>The project path.</value>
        public string Path { get; }

        /// <summary>
        /// Gets the project sections.
        /// </summary>
        /// <value>The project sections.</value>
        public ICollection<SlnProjectSection> ProjectSections { get; } = new List<SlnProjectSection>();
    }
}