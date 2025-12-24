namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ResourceAssignment
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Time;
	using Function = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using VirtualPlatform = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform;

	public class SatTxResourceAssignmentHandler : ResourceAssignmentHandler
	{
		public SatTxResourceAssignmentHandler(Helpers helpers, Service service, Order.Order orderContainingService, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null) : base(helpers, service, orderContainingService, overwrittenFunctionTimeRanges)
		{
			if (service.Definition.VirtualPlatform != VirtualPlatform.TransmissionSatellite) throw new ArgumentException("Service is not a satellite transmission", nameof(service));
		}

		public override void AssignResources()
		{
			AssignResourcesWithDtr();
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
			if (function.Id == FunctionGuids.GenericModulating)
			{
				Log(nameof(FilterResources), $"Function is Generic Modulating, extra filtering based on Antenna resource availability required", function.Name);

				var antennaFunction = service.Functions.Single(f => f.Id == FunctionGuids.Antenna);
				var antennas = GetSelectableResources(antennaFunction);

				var filteredResources = new HashSet<FunctionResource>();
				var removedResources = new HashSet<FunctionResource>();
				foreach (var modulator in resourcesToFilter)
				{
					var resourceInputConnectionsLbandProfileParameterToMatch = new ProfileParameter
					{
						Id = ProfileParameterGuids.ResourceInputConnectionsLband,
						Name = "ResourceInputConnections_LBand",
						Value = modulator.Name
					};

					if (antennas.Any(a => a.MatchesProfileParameter(helpers, resourceInputConnectionsLbandProfileParameterToMatch)))
					{
						filteredResources.Add(modulator);
					}
					else
					{
						removedResources.Add(modulator);
					}
				}

				Log(nameof(FilterResources), $"Removed following resources while filtering: {string.Join(",", removedResources.Select(r => r.Name))}", function.Name);

				Log(nameof(FilterResources), $"Filtered capabilities-matching resources: {string.Join(",", filteredResources.Select(r => r.Name))}", function.Name);

				return filteredResources;
			}
			else
			{
				Log(nameof(FilterResources), $"No extra filtering based other resource availability required", function.Name);

				return resourcesToFilter;
			}
		}

		protected override HashSet<FunctionResource> FilterForFixedTieLines(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			return resourcesToFilter;
		}

		private List<FunctionResource> GetResourcesThatAreConnectedToSelectableEncodingResources(List<FunctionResource> modulatingResources, out List<FunctionResource> removedResources)
		{
			if (modulatingResources == null) throw new ArgumentNullException(nameof(modulatingResources));

			var filteredMatchingResources = new List<FunctionResource>();
			removedResources = new List<FunctionResource>();

			var encodingResources = GetSelectableResources(EncodingFunction).ToList();

            var timeRange = new TimeRangeUtc(service.StartWithPreRoll.ToUniversalTime(), service.EndWithPostRoll.ToUniversalTime());

            foreach (var modulatingResource in modulatingResources)
			{
				bool connectedEncodingResourceIsAvailable = false;
				foreach (var encodingResource in encodingResources)
				{
					bool encodingResourceIsConnectedToModulatingResource = encodingResource.MatchesProfileParameter(helpers,new ProfileParameter
					{
						Id = ProfileParameterGuids.ResourceInputConnectionsAsi,
						Name = "ResourceInputConnections_ASI",
						Value = modulatingResource.Name
					}, 
                    timeRange);

					if (encodingResourceIsConnectedToModulatingResource)
					{
						connectedEncodingResourceIsAvailable = true;
						break;
					}
				}

				if (connectedEncodingResourceIsAvailable)
				{
					filteredMatchingResources.Add(modulatingResource);
				}
				else
				{
					removedResources.Add(modulatingResource);
				}
			}

			return filteredMatchingResources;
		}

		private Function EncodingFunction => service.Functions.Single(f => f.Id == FunctionGuids.GenericEncoding);
	}
}
