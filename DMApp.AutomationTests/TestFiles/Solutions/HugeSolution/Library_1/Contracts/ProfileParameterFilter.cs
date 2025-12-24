namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
    using System;

    public class ProfileParameterFilter
    {
        public ProfileParameterFilter()
        {
            ProfileParameterName = String.Empty;
            AllowUnfilteredProfileParameterValues = false;
            ProfileParameterValueFilters = new ProfileParameterValueFilter[0];
        }

        /// <summary>
        /// Gets or sets the name of the profile parameter.
        /// </summary>
        public string ProfileParameterName { get; set; }

        /// <summary>
        /// Gets or sets if all profile parameter values that are not listed in the ProfileParameterValueFilters are fully allowed.
        /// </summary>
        public bool AllowUnfilteredProfileParameterValues { get; set; }

        /// <summary>
        /// Gets or sets the list of profile parameter values that are allowed for this profile parameter.
        /// </summary>
        public ProfileParameterValueFilter[] ProfileParameterValueFilters { get; set; }
    }
}