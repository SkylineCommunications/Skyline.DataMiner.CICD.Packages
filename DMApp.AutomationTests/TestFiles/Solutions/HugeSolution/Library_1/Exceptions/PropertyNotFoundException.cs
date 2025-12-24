namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
    using System;

    public class PropertyNotFoundException : MediaServicesException
    {
        public PropertyNotFoundException()
        {
        }

        public PropertyNotFoundException(string propertyName, string className)
            : base($"Unable to find property with name {propertyName} in class {className}")
        {
        }

        public PropertyNotFoundException(string message)
            : base(message)
        {
        }
    }
}