namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [Serializable]
    public class ProfileInstanceNotFoundException : MediaServicesException
    {
        public ProfileInstanceNotFoundException()
        {
        }

        public ProfileInstanceNotFoundException(string message) : base(message)
        {
        }

        public ProfileInstanceNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        public ProfileInstanceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
