namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ResourceAssignment
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Time;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class DtrResourceAssignmentHandler : ResourceAssignmentHandler
	{
		public DtrResourceAssignmentHandler(Helpers helpers, Service service, Order.Order orderContainingService, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null) : base(helpers, service, orderContainingService, overwrittenFunctionTimeRanges)
		{
		}

		public override void AssignResources()
		{
			AssignResourcesWithDtr();
		}

		protected override HashSet<FunctionResource> FilterInterService(Function.Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			var filteredResources = resourcesToFilter;

			filteredResources = RemoveResourcesOccupiedByOtherServicesWithinOrder(function, filteredResources);

			filteredResources = RemoveResourcesWithoutSelectableRoutingResource(function, filteredResources);

			return filteredResources;
		}

		protected override HashSet<FunctionResource> FilterIntraService(Function.Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			return resourcesToFilter;
		}

		protected override HashSet<FunctionResource> FilterForFixedTieLines(Function.Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			return resourcesToFilter;
		}
	}
}
