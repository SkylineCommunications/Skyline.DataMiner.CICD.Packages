namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
    using System;

    public class ServiceFilter
    {
        public ServiceFilter()
        {
            ServiceDefinitionName = String.Empty;
            SimultaneousServices = SimultaneousServices.Unlimited;
            IsPartOfFixedPrice = true;
            AllowUnfilteredProfileParameters = false;
            ProfileParameterFilters = new ProfileParameterFilter[0];
        }

        /// <summary>
        /// Gets or sets the name of the service definition.
        /// </summary>
        public string ServiceDefinitionName { get; set; }

        /// <summary>
        /// Gets or sets how many simultaneous services can be created using this service definition.
        /// </summary>
        public SimultaneousServices SimultaneousServices { get; set; }

        /// <summary>
        /// Gets or sets if this service is part of a fixed price.
        /// </summary>
        public bool IsPartOfFixedPrice { get; set; }

        /// <summary>
        /// Gets or sets if all profile parameters that are not listed in the ProfileParameterFilters are fully allowed.
        /// </summary>
        public bool AllowUnfilteredProfileParameters { get; set; }

        /// <summary>
        /// Gets or sets the list of profile parameters that are allowed for this service with additional filters on specific values.
        /// </summary>
        public ProfileParameterFilter[] ProfileParameterFilters { get; set; }
    }
}