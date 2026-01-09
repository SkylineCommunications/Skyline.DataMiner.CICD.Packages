namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents a project in a solution file.
    /// </summary>
    [DebuggerDisplay("Project: {Name}")]
    public class ProjectInSolution : SolutionItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInSolution"/> class.
        /// </summary>
        /// <param name="solution">The solution this project is part of.</param>
        /// <param name="guid">The GUID of the project.</param>
        /// <param name="name">The name of the project.</param>
        /// <param name="path">The path of the project file.</param>
        internal ProjectInSolution(Solution solution, Guid guid, string name, string path) : base(solution, guid, name, path)
        {
        }
    }
}