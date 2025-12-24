namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
    using System;

    public class Company
    {
        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the company.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the View associated with this company.
        /// </summary>
        public int SecurityViewId { get; set; }

        /// <summary>
        /// Gets or sets the linked companies.
        /// </summary>
        public string LinkedCompanies { get; set; }
    }
}
