namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class SelectableRoutingResourceChain
	{
		public RoutingResourceChain RoutingResourceChain { get; set; }

		public RoutingServiceChain ExistingRoutingServiceChain { get; set; }

		public int CombinedResourcePriority => RoutingResourceChain.AllResources.Select(r => r.GetPriority()).Sum();
	}
}
