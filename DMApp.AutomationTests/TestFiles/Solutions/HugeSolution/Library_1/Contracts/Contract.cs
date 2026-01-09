namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Flags]
    public enum GlobalEventLevelReceptionConfigurations
    {
        None = 0,
        GlobalEventLevelReceptionUsageAllowed = 1,
        PromoteGlobalEventLevelReceptionAllowed = 2,
    }

    public class Contract
	{
        public Contract()
        {
            ServiceFilters = new ServiceFilter[0];
        }

		public string ID { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

        /// <summary>
        /// Gets or sets the company linked to this contract.
        /// </summary>
		public string Company { get; set; }

        /// <summary>
        /// Gets or sets the id of the view that is associated with the company linked to this contract.
        /// </summary>
        public int CompanySecurityViewId { get; set; }

		/// <summary>
		/// Gets or sets the list of companies that are linked to the company of this contract.
		/// </summary>
		public Company[] LinkedCompanies { get; set; }

        /// <summary>
        /// Gets or sets the start time for this contract.
        /// </summary>
		public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the end time for this contract.
        /// </summary>
		public DateTime End { get; set; }

        /// <summary>
        /// Gets or sets the status for this contract.
        /// </summary>
		public ContractStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the type of this contract.
        /// </summary>
		public ContractType Type { get; set; }

        /// <summary>
        /// Gets or sets the event id of the event linked to this contract.
        /// </summary>
		public string LinkedEventId { get; set; }

        /// <summary>
        /// Gets or sets if all services that are not listed in the ServiceFilters are fully allowed for this contract.
        /// </summary>
        public bool AllowUnfilteredServices { get; set; }

        /// <summary>
        /// Gets or sets the list of services that are allowed for this contract with additional filters on specific profile parameters.
        /// </summary>
        public ServiceFilter[] ServiceFilters { get; set; }

        /// <summary>
		/// Contains the configuration of global event level receptions.
		/// </summary>
		public GlobalEventLevelReceptionConfigurations ContractGlobalEventLevelReceptionConfiguration { get; set; }

        /// <summary>
        /// Verifies if the service using the provided service definition is allowed for the provided user.
        /// </summary>
        /// <param name="serviceDefinition">The service definition.</param>
        /// <param name="user">The user.</param>
        /// <returns>A boolean indicating if the service is allowed.</returns>
        public bool IsServiceAllowed(ServiceDefinition serviceDefinition, UserInfo user)
        {
            if (serviceDefinition.IsIntegrationOnly) return false;
            if (serviceDefinition.IsMcrOnly && !user.IsMcrUser) return false;

            return IsServiceAllowed(serviceDefinition.Name);
        }

        /// <summary>
        /// Verifies if audio processing is allowed for this contract.
        /// </summary>
        /// <returns>A boolean indicating if the audio processing is allowed.</returns>
        public bool IsAudioProcessingAllowed()
        {
            return IsServiceAllowed("_Audio Processing");
        }

        /// <summary>
        /// Verifies if video processing is allowed for this contract.
        /// </summary>
        /// <returns>A boolean indicating if the video processing is allowed.</returns>
        public bool IsVideoProcessingAllowed()
        {
            return IsServiceAllowed("_Video Processing");
        }

        /// <summary>
        /// Filters the provided service definitions and returns those that are allowed for the provided user.
        /// </summary>
        /// <returns>A list of allowed service definitions.</returns>
        public List<ServiceDefinition> FilterServiceDefinitions(IReadOnlyList<ServiceDefinition> serviceDefinitions, UserInfo user)
        {
            if (Status != ContractStatus.Open) return new List<ServiceDefinition>();
            return serviceDefinitions.Where(x => IsServiceAllowed(x, user)).ToList();
        }

        /// <summary>
        /// Verifies if the provided profile parameter is allowed to configure in this contract.
        /// </summary>
        /// <param name="serviceDefinition">The service definition.</param>
        /// <param name="profileParameter">The profile parameter.</param>
        /// <param name="user">The user.</param>
        /// <returns>A boolean indicating if this contract allows configuration of this profile parameter.</returns>
        public bool IsProfileParameterAllowed(ServiceDefinition serviceDefinition, ProfileParameter profileParameter, UserInfo user)
        {
			// TODO check if this if-clause can be removed, it causes profile parameters to be invisible in LOF for integration orders (e.g.: Areena destination in Feenix order)
            //if (!IsServiceAllowed(serviceDefinition, user)) return false;

            var serviceFilter = ServiceFilters.FirstOrDefault(f => f.ServiceDefinitionName == serviceDefinition.Name);
            if (serviceFilter == null) return AllowUnfilteredServices;

            var profileParameterFilter = serviceFilter.ProfileParameterFilters.FirstOrDefault(p => p.ProfileParameterName == profileParameter.Name);
            if (profileParameterFilter == null) return serviceFilter.AllowUnfilteredProfileParameters;

            return true;
        }

        /// <summary>
        /// Filters the allowed discreet values for the provided profile parameter.
        /// </summary>
        /// <param name="serviceDefinition">The service definition.</param>
        /// <param name="profileParameter">The profile parameter.</param>
        /// <param name="user">The user.</param>
        /// <returns>A list of allowed profile parameter values.</returns>
        public List<string> FilterProfileParameterValues(ServiceDefinition serviceDefinition, ProfileParameter profileParameter, UserInfo user)
        {
            var serviceFilter = ServiceFilters.FirstOrDefault(f => f.ServiceDefinitionName == serviceDefinition.Name);
            if (serviceFilter == null)
            {
                return FilterValuesWithoutServiceFilter(profileParameter);
            }

            var profileParameterFilter = serviceFilter.ProfileParameterFilters.FirstOrDefault(p => p.ProfileParameterName == profileParameter.Name);
            if (profileParameterFilter == null)
            {
                return FilterValuesWithoutProfileParameterFilter(serviceFilter, profileParameter);
            }
            else
            {
                return FilterValues(profileParameterFilter, profileParameter, user);
            }
        }

        private List<string> FilterValuesWithoutServiceFilter(ProfileParameter profileParameter)
		{
			return AllowUnfilteredServices ? profileParameter.Discreets.Select(d => d.DisplayValue).ToList() : new List<string>();
		}

        private static List<string> FilterValuesWithoutProfileParameterFilter(ServiceFilter serviceFilter, ProfileParameter profileParameter)
		{
			return serviceFilter.AllowUnfilteredProfileParameters
				? profileParameter.Discreets.Select(d => d.DisplayValue).ToList()
				: new List<string>();
		}

        private static List<string> FilterValues(ProfileParameterFilter profileParameterFilter, ProfileParameter profileParameter, UserInfo user)
		{
            var allowedProfileParameterValues = new List<string>();
            if (profileParameterFilter.AllowUnfilteredProfileParameterValues) allowedProfileParameterValues.AddRange(profileParameter.Discreets.Select(d => d.DisplayValue));

            foreach (var profileparameterValueFilter in profileParameterFilter.ProfileParameterValueFilters)
            {
                if (!user.IsMcrUser && profileparameterValueFilter.IsMcrOnly)
                {
                    allowedProfileParameterValues.Remove(profileparameterValueFilter.Value);
                    continue;
                }

                if (!allowedProfileParameterValues.Contains(profileparameterValueFilter.Value)) allowedProfileParameterValues.Add(profileparameterValueFilter.Value);
            }

            return allowedProfileParameterValues;
        }

        private bool IsServiceAllowed(string serviceDefinitionName)
        {
            if (Status != ContractStatus.Open) return false;

            ServiceFilter serviceFilter = null;
            if (ServiceFilters != null) serviceFilter = ServiceFilters.FirstOrDefault(f => f.ServiceDefinitionName == serviceDefinitionName);

            if (serviceFilter == null) return AllowUnfilteredServices;

            // TODO: simultaneous and fixed price check are not yet included

            return true;
        }
    }
}