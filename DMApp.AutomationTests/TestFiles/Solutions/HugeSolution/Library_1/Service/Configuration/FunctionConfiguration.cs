namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class FunctionConfiguration
    {
        /// <summary>
        /// The ID of the function.
        /// </summary>
        public Guid Id { get; set; }

        public string Name { get; set; }

        /// <summary>
		/// The profile parameters configured for this function.
		/// The key is the id of the profile parameter and the value is the actual value that was configured.
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<Guid, object> ProfileParameters { get; set; }

        /// <summary>
		/// The resource assigned to this function
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid ResourceId { get; set; }

        /// <summary>
        /// The resource name assigned to this function
        /// </summary>
        public string ResourceName { get; set; }

		public bool RequiresResource { get; set; }

		public bool ConfiguredByMcr { get; set; } = false;

		public bool McrHasOverruledFixedTieLineLogic { get; set; } = false;

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}
    }
}
