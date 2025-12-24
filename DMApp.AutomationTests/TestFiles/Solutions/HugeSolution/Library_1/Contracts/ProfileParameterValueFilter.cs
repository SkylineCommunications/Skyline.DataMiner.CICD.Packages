namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
    using System;

    public class ProfileParameterValueFilter
    {
        public ProfileParameterValueFilter()
        {
            Value = String.Empty;
            IsMcrOnly = false;
        }

        /// <summary>
        /// Gets or sets the value for this profile parameter.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets if this profile parameter value is only allowed for MCR users.
        /// </summary>
        public bool IsMcrOnly { get; set; }
    }
}