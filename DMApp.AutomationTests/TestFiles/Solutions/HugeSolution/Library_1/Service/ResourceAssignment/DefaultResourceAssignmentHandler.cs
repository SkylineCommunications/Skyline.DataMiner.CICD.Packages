namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ResourceAssignment
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.Filters;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Time;
	using Function = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class DefaultResourceAssignmentHandler : ResourceAssignmentHandler
	{
		public DefaultResourceAssignmentHandler(Helpers helpers, Service service, Order orderContainingService, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null) : base(helpers, service, orderContainingService, overwrittenFunctionTimeRanges)
		{
		}

		public override void AssignResources()
		{
			LogMethodStart(nameof(AssignResources), out var stopwatch, service.Name);

			foreach (var function in service.Functions.OfType<DisplayedFunction>())
			{
				Log(nameof(AssignResources), $"Handling function {function.Name}");

				function.SelectableResources = GetSelectableResources(function);

				function.Resource = SelectCurrentOrNewResource(function, function.SelectableResources, out bool resourceChanged);

				handledFunctionLabels.Add(function.Definition.Label);
			}

			Log(nameof(AssignResources), $"Resource assignment result: {string.Join(";", service.Functions.Select(f => $"{f.Definition.Label}={f.ResourceName}"))}", service.Name);

			LogMethodCompleted(nameof(AssignResources), service.Name, stopwatch);
		}

		protected override HashSet<FunctionResource> FilterInterService(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			var filteredResources = resourcesToFilter;

			filteredResources = RemoveResourcesOccupiedByOtherServicesWithinOrder(function, filteredResources);

			filteredResources = RemoveResourcesWithoutSelectableRoutingResource(function, filteredResources);

			return filteredResources;
		}

		protected override HashSet<FunctionResource> FilterIntraService(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			return resourcesToFilter;
		}

		protected override HashSet<FunctionResource> FilterForFixedTieLines(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			var filteredResources = resourcesToFilter;

			bool destinationResourceFilteringRequired = ServiceIsYleHelsinkiUutisalueDestination(service);
			if (destinationResourceFilteringRequired)
			{
				filteredResources = FilterDestinationResources(filteredResources);
			}

			return filteredResources;
		}

		private HashSet<FunctionResource> FilterDestinationResources(HashSet<FunctionResource> resourcesToFilter)
		{
			LogMethodStart(nameof(FilterDestinationResources), out var stopwatch, service.Name);

			Service sourceService = GetSourceService();
			Function lastSourceFunction = GetLastSourceFunction(sourceService);

			if (lastSourceFunction == null)
			{
				Log(nameof(FilterDestinationResources), $"Last source function is null");
				LogMethodCompleted(nameof(FilterDestinationResources), service.Name, stopwatch);
				return resourcesToFilter;
			}

			bool lastSourceFunctionRequiresFixedTieLine = lastSourceFunction.Resource != null && lastSourceFunction.Resource.RequiresSpecificTieLine();

			bool mcrHasOverruledFixedTieLine = service.Functions.Single().McrHasOverruledFixedTieLineLogic;
			// this flag can be set to true by the service controller in Update Service script

			Log(nameof(FilterDestinationResources), $"Source function {lastSourceFunction.Name} resource {lastSourceFunction.ResourceName} {(lastSourceFunctionRequiresFixedTieLine ? "needs" : "does not need")} a fixed tie line. MCR has{(mcrHasOverruledFixedTieLine ? string.Empty : " not")} overruled fixed tie line logic");

			if (lastSourceFunctionRequiresFixedTieLine && !mcrHasOverruledFixedTieLine)
			{
				if (TryFindDestinationResource(lastSourceFunction, resourcesToFilter, out FunctionResource destinationResources)) return new HashSet<FunctionResource> { destinationResources };

				Log(nameof(FilterDestinationResources), $"Unable to find matching fixed tie line destination resource that is linked to source service resource {lastSourceFunction.Resource?.Name}");
			}
			else if (!mcrHasOverruledFixedTieLine)
			{
				Log(nameof(FilterDestinationResources), $"MCR has not overruled fixed tie line logic, removing all fixed tie line resources...");

				resourcesToFilter = Enumerable.ToHashSet(resourcesToFilter.Where(r => string.IsNullOrWhiteSpace(r.GetResourcePropertyStringValue(ResourcePropertyNames.FixedTieLineSource))));
			}
			else
			{
				Log(nameof(FilterDestinationResources), $"MCR has overruled fixed tie line logic, no extra filtering required...");
			}

			Log(nameof(FilterDestinationResources), $"Filtered resources: {string.Join(", ", resourcesToFilter.Select(r => r.Name))}");

			LogMethodCompleted(nameof(FilterDestinationResources), service.Name, stopwatch);

			return resourcesToFilter;
		}

		private Service GetSourceService()
		{
			if (service.BackupType == BackupType.Active)
			{
				return order.Sources.SingleOrDefault(s => s.BackupType == BackupType.Active);
			}
			else
			{
				return order.Sources.SingleOrDefault(s => s.BackupType == BackupType.None);
			}
		}

		private static Function GetLastSourceFunction(Service sourceService)
		{
			return sourceService != null ? sourceService.Functions.FirstOrDefault(f => sourceService.Definition.FunctionIsLast(f)) : null;
		}

		private bool TryFindDestinationResource(Function lastSourceFunction, HashSet<FunctionResource> resourcesToFilter, out FunctionResource destinationResource)
		{
			destinationResource = null;
			foreach (var destinationFilteredResource in resourcesToFilter)
			{
				if (destinationFilteredResource == null) continue;

				// Checking if a destination resource contains a property which value is equal to the fixed tieline source resource name.
				string destinationResourceFixedTieLinePropertyValue = destinationFilteredResource.GetResourcePropertyStringValue(ResourcePropertyNames.FixedTieLineSource);
				if (destinationResourceFixedTieLinePropertyValue.Equals(lastSourceFunction.Resource?.Name, System.StringComparison.InvariantCultureIgnoreCase))
				{
					Log(nameof(FilterDestinationResources), $"Found matching fixed tie line destination resource {destinationFilteredResource.Name} that is linked to source service resource {lastSourceFunction.Resource?.Name}");
					LogMethodCompleted(nameof(FilterDestinationResources), service.Name);
					destinationResource = destinationFilteredResource;
					return true;
				}
			}

			return false;
		}
	}
}
