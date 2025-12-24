namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
    using System;

    public class ServiceFunctionNotFoundException : MediaServicesException
    {
        public ServiceFunctionNotFoundException()
        {

        }

        public ServiceFunctionNotFoundException(Guid id)
            : base($"Unable to find Function for Service with Id: {id}")
        {

        }

        public ServiceFunctionNotFoundException(string name)
            : base ($"Unable to find Function for Service with name {name}")
        {

        }

        public ServiceFunctionNotFoundException(Guid id, string name)
            : base($"Unable to find Function for service with name {name} and Id {id}")
        {

        }
    }
}
