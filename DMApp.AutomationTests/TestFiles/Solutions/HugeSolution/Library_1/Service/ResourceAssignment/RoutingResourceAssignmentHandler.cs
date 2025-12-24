namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ResourceAssignment
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Time;
	using Function = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	/* IMPORTANT
	 * This class should only be used for resource dropdown filtering in UI scripts like UpdateService and UpdateELRs.
	 * Assigning resources using this class is not supported.*/

	public class RoutingResourceAssignmentHandler : ResourceAssignmentHandler
	{
		private readonly LiveVideoOrder liveVideoOrder;

		public RoutingResourceAssignmentHandler(Helpers helpers, Service service, Order orderContainingService, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null) : base(helpers, service, orderContainingService, overwrittenFunctionTimeRanges)
		{
			liveVideoOrder = new LiveVideoOrder(helpers, order);
		}

		public override void AssignResources()
		{
			// not supported, this specific routing class should only be used to find selectable resources

			LogMethodStart(nameof(AssignResources), out var stopwatch ,service.Name);
			Log(nameof(AssignResources),$"Assigning resources for routing services is not supported", service.Name);
			LogMethodCompleted(nameof(AssignResources), service.Name, stopwatch);
		}

		protected override HashSet<FunctionResource> FilterInterService(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			HashSet<FunctionResource> filteredResources;
			if (ConnectedNeighborServiceIsRouting(function))
			{
				// Use the tie line logic to check which resources should be selectable
				filteredResources = GetSelectableTieLineResources(function);
			}
			else
			{
				filteredResources = RemoveResourcesThatAreNotConnectedToNeighborServiceConnectedFunction(function, resourcesToFilter);

				filteredResources = RemoveResourcesOccupiedByOtherServicesWithinOrder(function, filteredResources);
			}

			return filteredResources;
		}

		protected override HashSet<FunctionResource> FilterIntraService(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			return resourcesToFilter;
		}

		protected override HashSet<FunctionResource> FilterForFixedTieLines(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			return resourcesToFilter;
		}

		/// <summary>
		/// Gets the available tieline resources for the given matrix SDI function in the given routing service, taking into account the availability of the full tieline. Only applicable for functions that are connected to a neighbor routing service.
		/// </summary>
		/// <param name="function">The matrix SDI function for which to get the available tie line resources.</param>
		/// <returns>A collection of Resources that can be selected for <paramref name="function"/></returns>
		public HashSet<FunctionResource> GetSelectableTieLineResources(Function function)
		{
			if (function == null) throw new ArgumentNullException(nameof(function));

			var selectableResources = new HashSet<FunctionResource>();

			var routingServiceChains = liveVideoOrder.GetRoutingServiceChainsForService(service.Id);
			foreach (var routingServiceChain in routingServiceChains)
			{
				selectableResources.UnionWith(routingServiceChain.GetSelectableRoutingResources(service, function, false));
			}

			return selectableResources;
		}

		/// <summary>
		/// Gets the selected resource for the function of the parent/child service that is connected to <paramref name="function"/>, and removes the resources from <paramref name="resourcesToFilter"/> that are not connected to that selected resource.
		/// </summary>
		/// <param name="function">The function for which the given selectable resources should be filtered.</param>
		/// <param name="resourcesToFilter">The collection of selectable resources to filter.</param>
		/// <remarks>This method is used to avoid that a user can select a routing resource that is not connected to for example the Destination resource.</remarks>
		private HashSet<FunctionResource> RemoveResourcesThatAreNotConnectedToNeighborServiceConnectedFunction(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			if (service.Definition.VirtualPlatform != VirtualPlatform.Routing) throw new NotImplementedException("This method should only be called for Routing services.");
			if (function == null) throw new ArgumentNullException(nameof(function));
			if (function.Id != FunctionGuids.MatrixInputSdi && function.Id != FunctionGuids.MatrixOutputSdi) throw new ArgumentException("Function should be Matrix Input SDI or Matrix output SDI.", nameof(function));

			var filteredResources = new HashSet<FunctionResource>();
			var removedResources = new HashSet<FunctionResource>();

			bool functionIsMatrixInput = FunctionGuids.AllMatrixInputGuids.Contains(function.Id);

			var resourceInputOrOutputConnectionsSdiProfileParameterGuid = functionIsMatrixInput ? ProfileParameterGuids.ResourceInputConnectionsSdi : ProfileParameterGuids.ResourceOutputConnectionsSdi;

			var neighborServiceConnectedFunctionSelectedResource = functionIsMatrixInput ? GetSelectedResourceForLastFunctionInParentService() : GetSelectedResourceForFirstFunctionInChildService();

			foreach (var routingResource in resourcesToFilter)
			{
				try
				{
					var connectedResourceCapability = routingResource.Capabilities.SingleOrDefault(c => c.CapabilityProfileID == resourceInputOrOutputConnectionsSdiProfileParameterGuid);
					if (connectedResourceCapability == null)
					{
						Log(nameof(RemoveResourcesThatAreNotConnectedToNeighborServiceConnectedFunction), $"Unable to find ResourceInputOrOutputConnections_SDI capability (ID={resourceInputOrOutputConnectionsSdiProfileParameterGuid}) on resource {routingResource?.Name}");
						continue;
					}

					var connectedResourceNames = connectedResourceCapability.Value.Discreets;

					bool routingResourceAndNeighborResourceAreConnected = connectedResourceNames.Contains(neighborServiceConnectedFunctionSelectedResource?.Name);

					if (routingResourceAndNeighborResourceAreConnected) filteredResources.Add(routingResource);
					else removedResources.Add(routingResource);
				}
				catch (Exception e)
				{
					Log(nameof(RemoveResourcesThatAreNotConnectedToNeighborServiceConnectedFunction), $"Error while checking if available resource {routingResource?.Name} is connected to resource {neighborServiceConnectedFunctionSelectedResource?.Name}: {e}");
				}
			}

			Log(nameof(RemoveResourcesThatAreNotConnectedToNeighborServiceConnectedFunction), $"Removed following routing resources who are not connected to {neighborServiceConnectedFunctionSelectedResource?.Name} while filtering: {string.Join(",", removedResources.Select(r => r.Name))}", function.Name);

			Log(nameof(RemoveResourcesThatAreNotConnectedToNeighborServiceConnectedFunction), $"Filtered resources: {string.Join(",", filteredResources.Select(r => r.Name))}", function.Name);

			return filteredResources;
		}

		public bool ConnectedNeighborServiceIsRouting(Function function)
		{
			bool functionIsFirst = service.Definition.FunctionIsFirst(function);
			bool functionIsLast = service.Definition.FunctionIsLast(function);
			if (!functionIsFirst && !functionIsLast) throw new ArgumentException("Function is neither first or last in the service definition", nameof(function));

			var neighborService = functionIsFirst ? order.AllServices.SingleOrDefault(s => s.Children.Contains(service)) : service.Children.FirstOrDefault();

			return neighborService?.Definition?.VirtualPlatform == VirtualPlatform.Routing;
		}

		private FunctionResource GetSelectedResourceForLastFunctionInParentService()
		{
			var parentService = order.AllServices.SingleOrDefault(s => s.Children.Contains(service));
			if (parentService == null) return null;

			return parentService.LastResourceRequiringFunction?.Resource;
		}

		private FunctionResource GetSelectedResourceForFirstFunctionInChildService()
		{
			var childService = service.Children.FirstOrDefault();
			if (childService == null) return null;

			return childService.FirstResourceRequiringFunction?.Resource;
		}

		private List<RoutingServiceChain> GetRoutingChainsForSameInputService()
		{
			var routingChainsForSameInputService = liveVideoOrder.GetRoutingServiceChainsWithSameInputServiceAs(service.Id);

			Log(nameof(GetRoutingChainsForSameInputService), $"Found routing chains to '{string.Join(", ", routingChainsForSameInputService.Select(rcs => rcs.OutputService.Service.Name))}'");

			return routingChainsForSameInputService;
		}
	}
}
