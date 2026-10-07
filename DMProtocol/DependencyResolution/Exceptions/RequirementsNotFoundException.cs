namespace Skyline.DataMiner.CICD.DMProtocol.DependencyResolution.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception raised when the provided requirements file does not exist.
    /// </summary>
    [Serializable]
    public class RequirementsNotFoundException : Exception
    {
        /// <summary>
        /// Gets the path of the requirements file that could not be found.
        /// </summary>
        public string RequirementsFilePath { get; }

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
        /// Initializes a new instance of the <see cref="RequirementsNotFoundException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected RequirementsNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            RequirementsFilePath = info.GetString(nameof(RequirementsFilePath));
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(RequirementsFilePath), RequirementsFilePath);
            base.GetObjectData(info, context);
        }
    }
}
