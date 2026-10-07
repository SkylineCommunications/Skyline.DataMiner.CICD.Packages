namespace Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception raised when a connector script's <c>manifest.json</c> is missing, malformed, or fails validation.
    /// </summary>
    [Serializable]
    public class InvalidManifestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidManifestException"/> class.
        /// </summary>
        /// <param name="message">A message describing why the manifest is invalid.</param>
        public InvalidManifestException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidManifestException"/> class.
        /// </summary>
        /// <param name="message">A message describing why the manifest is invalid.</param>
        /// <param name="innerException">The exception that caused this exception (e.g. a JSON parsing error).</param>
        public InvalidManifestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidManifestException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected InvalidManifestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
