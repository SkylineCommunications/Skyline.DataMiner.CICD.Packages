namespace Skyline.DataMiner.CICD.DMProtocol.DependencyResolution.Exceptions
{
    using System;

    /// <summary>
    /// Exception raised when the provided requirements file does not exist.
    /// </summary>
    [Serializable]
    public class RequirementsNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsNotFoundException"/> class.
        /// </summary>
        /// <param name="requirementsFilePath">The path of the requirements file that could not be found.</param>
        public RequirementsNotFoundException(string requirementsFilePath)
            : base($"Requirements file '{requirementsFilePath}' not found.")
        {
            RequirementsFilePath = requirementsFilePath;
        }

        /// <summary>
        /// Gets the path of the requirements file that could not be found.
        /// </summary>
        public string RequirementsFilePath { get; }
    }
}
