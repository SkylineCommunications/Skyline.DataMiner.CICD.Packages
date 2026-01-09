namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class VizremStudioService : VizremConverterRequiringService
	{
		public VizremStudioService(Helpers helpers, Service service, LiveVideoOrder liveVideoOrder)
			: base(helpers, service, liveVideoOrder)
		{
		}
	}
}