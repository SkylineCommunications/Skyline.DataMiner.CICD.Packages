namespace Skyline.DataMiner.CICD.DMProtocol.DependencyResolution.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception raised when no valid pip executable can be found on the current system.
    /// </summary>
    [Serializable]
    public class PipNotFoundException : Exception
    {
        private const string DefaultMessage =
            "No valid pip executable found. Please make sure Python (including pip) is installed and available in the PATH.";

        /// <summary>
        /// Initializes a new instance of the <see cref="PipNotFoundException"/> class.
        /// </summary>
        public PipNotFoundException()
            : base(DefaultMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PipNotFoundException"/> class with a custom message.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        public PipNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PipNotFoundException"/> class with a custom message and inner exception.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        /// <param name="innerException">The inner exception.</param>
        public PipNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PipNotFoundException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected PipNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
