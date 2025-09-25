namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio
{
    using System;
    using System.Diagnostics;

    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.SolutionParser.Model;

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
        /// <param name="slnProject">The project.</param>
        internal ProjectInSolution(Solution solution, SlnProject slnProject) : base(solution, slnProject.Guid, slnProject.Name, slnProject.Path)
        {
        }
    }
}