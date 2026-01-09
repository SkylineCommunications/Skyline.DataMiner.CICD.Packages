namespace Skyline.DataMiner.CICD.Parsers.Common.VisualStudio
{
    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Represents a solution file entry.
    /// </summary>
    public class SolutionFileEntry
    {
        private readonly IFileSystem _fileSystem = FileSystem.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFileEntry"/> class.
        /// </summary>
        /// <param name="solution">The solution this entry is part of.</param>
        /// <param name="path">The relative path of the entry.</param>
        public SolutionFileEntry(Solution solution, string path)
        {
            Solution = solution;
            RelativePath = path;
        }

        /// <summary>
        /// Gets the solution this entry is part of.
        /// </summary>
        /// <value>The solution this entry is part of.</value>
        public Solution Solution { get; }

        /// <summary>
        /// Gets the relative path of the entry.
        /// </summary>
        /// <value>The relative path of the entry.</value>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the absolute path of the entry.
        /// </summary>
        /// <value>The absolute path of the entry.</value>
        public string AbsolutePath => _fileSystem.Path.Combine(Solution.SolutionDirectory, RelativePath);

        /// <summary>
        /// Gets the file name of the entry.
        /// </summary>
        /// <value>The file name of the entry.</value>
        public string FileName => _fileSystem.Path.GetFileName(RelativePath);
    }
}