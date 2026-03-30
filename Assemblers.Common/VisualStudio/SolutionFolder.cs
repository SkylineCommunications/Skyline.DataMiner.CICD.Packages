namespace Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents a solution folder.
    /// </summary>
    [DebuggerDisplay("Folder: {Name}")]
    public class SolutionFolder : SolutionItem
    {
        private readonly ICollection<SolutionFileEntry> _files = new List<SolutionFileEntry>();
        
        internal SolutionFolder(Solution solution, Guid guid, string name, string path) : base(solution, guid, name, path)
        {
        }

        /// <summary>
        /// Gets the files included in this folder.
        /// </summary>
        /// <value>The files included in this folder.</value>
        public IEnumerable<SolutionFileEntry> Files => _files;

        /// <summary>
        /// Gets the subdirectories.
        /// </summary>
        /// <value>The subdirectories.</value>
        public IEnumerable<SolutionFolder> SubFolders => Children.OfType<SolutionFolder>();

        /// <summary>
        /// Gets the subprojects.
        /// </summary>
        /// <value>The subprojects.</value>
        public IEnumerable<ProjectInSolution> SubProjects => Children.OfType<ProjectInSolution>();

        /// <summary>
        /// Gets the subdirectories with the specified name.
        /// </summary>
        /// <param name="name">The name of the subdirectory.</param>
        /// <returns>The specified folder.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
        public SolutionFolder GetSubFolder(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
            }

            return SubFolders.FirstOrDefault(folder => String.Equals(folder.Name, name));
        }

        /// <summary>
        /// Retrieves the descendant folders.
        /// </summary>
        /// <returns>The descendant folders.</returns>
#pragma warning disable S4049
        public IEnumerable<SolutionFolder> GetDescendantFolders()
        {
            var queue = new Queue<SolutionFolder>(SubFolders);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                yield return item;

                foreach (var child in item.SubFolders)
                {
                    queue.Enqueue(child);
                }
            }
        }
#pragma warning restore S4049

        /// <summary>
        /// Retrieves the descendant projects.
        /// </summary>
        /// <returns>The descendant projects.</returns>
#pragma warning disable S4049
        public IEnumerable<ProjectInSolution> GetDescendantProjects() => SubProjects.Concat(GetDescendantFolders().SelectMany(c => c.SubProjects));
#pragma warning restore S4049

        internal void AddFile(SolutionFileEntry item)
        {
            _files.Add(item);
        }
    }
}
