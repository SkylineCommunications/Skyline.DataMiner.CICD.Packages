namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects
{
    /// <summary>
    /// Represents a project file of a Visual Studio solution.
    /// </summary>
    public class ProjectFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectFile"/> class with the specified name and content.
        /// </summary>
        /// <param name="name">The name of the project file.</param>
        /// <param name="content">The content of the project file.</param>
        public ProjectFile(string name, string content)
        {
            Name = name;
            Content = content;
        }

        /// <summary>
        /// Gets the name of the project file.
        /// </summary>
        /// <value>The name of the project file.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the content of the project file.
        /// </summary>
        /// <value>The content of the project file.</value>
        public string Content { get; }
    }
}