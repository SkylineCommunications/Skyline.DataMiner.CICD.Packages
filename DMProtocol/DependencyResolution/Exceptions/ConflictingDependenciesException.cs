namespace Skyline.DataMiner.CICD.DMProtocol.DependencyResolution.Exceptions
{
    using System;

    /// <summary>
    /// Exception raised when pip detects conflicting dependencies, making it impossible to resolve a valid set of wheels.
    /// </summary>
    [Serializable]
    public class ConflictingDependenciesException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictingDependenciesException"/> class.
        /// </summary>
        /// <param name="pipStandardOutput">The standard output produced by pip.</param>
        /// <param name="pipStandardError">The standard error produced by pip.</param>
        public ConflictingDependenciesException(string pipStandardOutput, string pipStandardError)
            : base($"Conflicting dependencies found:{Environment.NewLine}{pipStandardError}{Environment.NewLine}{Environment.NewLine}Pip output for diagnostics:{Environment.NewLine}{pipStandardOutput}")
        {
            PipStandardOutput = pipStandardOutput;
            PipStandardError = pipStandardError;
        }

        /// <summary>
        /// Gets the standard output produced by pip during the dry-run install that detected the conflict.
        /// </summary>
        public string PipStandardOutput { get; }

        /// <summary>
        /// Gets the standard error produced by pip during the dry-run install that detected the conflict.
        /// </summary>
        public string PipStandardError { get; }
    }
}
