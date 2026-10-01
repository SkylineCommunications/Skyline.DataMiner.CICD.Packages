namespace Skyline.DataMiner.CICD.DMProtocol.DependencyResolution.Exceptions
{
    using System;
    using System.Runtime.Serialization;

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
        /// Initializes a new instance of the <see cref="ConflictingDependenciesException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ConflictingDependenciesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PipStandardOutput = info.GetString(nameof(PipStandardOutput));
            PipStandardError = info.GetString(nameof(PipStandardError));
        }

        /// <summary>
        /// Gets the standard output produced by pip during the dry-run install that detected the conflict.
        /// </summary>
        public string PipStandardOutput { get; }

        /// <summary>
        /// Gets the standard error produced by pip during the dry-run install that detected the conflict.
        /// </summary>
        public string PipStandardError { get; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(PipStandardOutput), PipStandardOutput);
            info.AddValue(nameof(PipStandardError), PipStandardError);
            base.GetObjectData(info, context);
        }
    }
}
