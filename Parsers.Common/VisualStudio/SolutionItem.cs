namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Represents a solution item.
    /// </summary>
    public class SolutionItem
    {
        private readonly ICollection<SolutionItem> _children = new List<SolutionItem>();
        private readonly IFileSystem _fileSystem = FileSystem.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionItem"/> class.
        /// </summary>
        /// <param name="solution">The solution this item is part of.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="name">The mane.</param>
        /// <param name="path">The path.</param>
        protected SolutionItem(Solution solution, Guid guid, string name, string path)
        {
            var updatedPath = path;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                updatedPath = path.Replace('\\', '/');
            }

            Solution = solution;
            Guid = guid;
            Name = name;
            RelativePath = updatedPath;
        }

        /// <summary>
        /// Gets the solution this item is part of.
        /// </summary>
        /// <value>The solution this item is part of.</value>
        public Solution Solution { get; }

        /// <summary>
        /// Gets the item GUID.
        /// </summary>
        /// <value>The item GUID.</value>
        public Guid Guid { get; }

        /// <summary>
        /// Gets the item name.
        /// </summary>
        /// <value>The item name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the relative path of the item to the solution.
        /// </summary>
        /// <value>The relative path of the item to the solution.</value>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the absolute path of the item.
        /// </summary>
        /// <value>The absolute path of the item.</value>
        public string AbsolutePath => _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(Solution.SolutionDirectory, RelativePath));

        /// <summary>
        /// Gets the parent item.
        /// </summary>
        /// <value>The parent item.</value>
        public SolutionFolder Parent { get; internal set; }

        /// <summary>
        /// Gets the child items.
        /// </summary>
        /// <value>The child items.</value>
        public IEnumerable<SolutionItem> Children => _children;

        internal void AddChild(SolutionItem item)
        {
            _children.Add(item);
        }
    }
}