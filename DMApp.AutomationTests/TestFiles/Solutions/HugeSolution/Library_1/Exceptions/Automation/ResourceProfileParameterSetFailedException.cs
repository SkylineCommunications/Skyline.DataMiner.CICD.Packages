namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [Serializable]
    public class ResourceProfileParameterSetFailedException : MediaServicesException
    {
        public ResourceProfileParameterSetFailedException()
        {
        }

        public ResourceProfileParameterSetFailedException(string message) : base(message)
        {
        }

        public ResourceProfileParameterSetFailedException(string message, Exception inner) : base(message, inner)
        {
        }

        public ResourceProfileParameterSetFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
