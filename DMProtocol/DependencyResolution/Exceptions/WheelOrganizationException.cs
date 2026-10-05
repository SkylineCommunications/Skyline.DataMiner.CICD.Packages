namespace Skyline.DataMiner.CICD.DMProtocol.DependencyResolution.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception raised when a downloaded wheel file cannot be moved or deleted while organizing resolved wheels into
    /// their platform-specific and universal directories.
    /// </summary>
    [Serializable]
    public class WheelOrganizationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WheelOrganizationException"/> class.
        /// </summary>
        /// <param name="wheelFileName">The file name of the wheel that could not be moved or deleted.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public WheelOrganizationException(string wheelFileName, string message, Exception innerException)
            : base(message, innerException)
        {
            WheelFileName = wheelFileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WheelOrganizationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected WheelOrganizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            WheelFileName = info.GetString(nameof(WheelFileName));
        }

        /// <summary>
        /// Gets the file name of the wheel that could not be moved or deleted.
        /// </summary>
        public string WheelFileName { get; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(WheelFileName), WheelFileName);
            base.GetObjectData(info, context);
        }
    }
}
