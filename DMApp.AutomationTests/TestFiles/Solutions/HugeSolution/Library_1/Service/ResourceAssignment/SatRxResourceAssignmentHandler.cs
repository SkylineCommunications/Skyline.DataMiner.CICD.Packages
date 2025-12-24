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

	public class SatRxResourceAssignmentHandler : ResourceAssignmentHandler
	{
		public SatRxResourceAssignmentHandler(Helpers helpers, Service service, Order.Order orderContainingService, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null) : base(helpers, service, orderContainingService, overwrittenFunctionTimeRanges)
		{
			if (service.Definition.VirtualPlatform != VirtualPlatform.ReceptionSatellite) throw new ArgumentException("Service is not a satellite reception", nameof(service));
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
			var filteredResources = resourcesToFilter;
			var allRemovedResources = new HashSet<FunctionResource>();

			if (function.Id == FunctionGuids.Antenna)
			{
				Log(nameof(FilterResources), $"Function is Antenna, extra filtering based on matrix input LBand and matrix output LBand resource availability required", function.Name);

				filteredResources = GetAntennaResourcesThatAreConnectedToSelectableMatrixOutputResources(filteredResources, out var removedMatchingResources);

				filteredResources = GetAntennaResourcesThatAreConnectedToSelectableMatrixInputResources(filteredResources, out var removedMatchingResourcesPart2);

				allRemovedResources = removedMatchingResources;
				allRemovedResources.UnionWith(removedMatchingResourcesPart2);
			}
			else if (function.Id == FunctionGuids.MatrixOutputLband)
			{
				Log(nameof(FilterResources), $"Function is Matrix Output LBand, extra filtering based on demodulating and matrix input LBand resource availability required", function.Name);

				filteredResources = GetResourcesThatAreConnectedToSelectableDemodulatingResources(filteredResources, out var removedMatchingResources);

				filteredResources = GetMatrixOutputResourcesThatAreConnectedToSelectableMatrixInputResources(filteredResources, out var removedMatchingResourcesPart2);

				allRemovedResources = removedMatchingResources;
				allRemovedResources.UnionWith(removedMatchingResourcesPart2);
			}
			else if (function.Id == FunctionGuids.Demodulating)
			{
				Log(nameof(FilterResources), $"Function is Demodulating, extra filtering based on decoding resource availability required", function.Name);

				filteredResources = GetResourcesThatAreConnectedToSelectableDecodingResources(filteredResources, out allRemovedResources);
			}
			else
			{
				Log(nameof(FilterResources), $"No extra filtering based on other resource availability required", function.Name);
			}

			Log(nameof(FilterResources), $"Removed following resources while filtering: {string.Join(",", allRemovedResources.Select(r => r.Name))}", function.Name);

			Log(nameof(FilterResources), $"Filtered capabilities-matching resources: {string.Join(",", filteredResources.Select(r => r.Name))}", function.Name);

			return filteredResources;
		}

		protected override HashSet<FunctionResource> FilterForFixedTieLines(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			return resourcesToFilter;
		}

		private HashSet<FunctionResource> GetResourcesThatAreConnectedToSelectableDecodingResources(HashSet<FunctionResource> demodulatingResources, out HashSet<FunctionResource> removedResources)
		{
			ArgumentNullCheck.ThrowIfNull(demodulatingResources, nameof(demodulatingResources));

			var filteredMatchingResources = new HashSet<FunctionResource>();
			removedResources = new HashSet<FunctionResource>();

			var decodingResources = GetSelectableResources(DecodingFunction).ToList();

			var timeRange = new TimeRangeUtc(service.StartWithPreRoll.ToUniversalTime(), service.EndWithPostRoll.ToUniversalTime());

			foreach (var demodulatingResource in demodulatingResources)
			{
				bool connectedDecodingResourceIsAvailable = false;
				foreach (var decodingResource in decodingResources)
				{
					bool decodingResourceIsConnectedToDemodulatingResource = decodingResource.MatchesProfileParameter(helpers, new ProfileParameter
					{
						Id = ProfileParameterGuids.ResourceInputConnectionsAsi,
						Name = "ResourceInputConnections_ASI",
						Value = demodulatingResource.Name
					},
					timeRange);

					if (decodingResourceIsConnectedToDemodulatingResource)
					{
						connectedDecodingResourceIsAvailable = true;
						break;
					}
				}

				if (connectedDecodingResourceIsAvailable)
				{
					filteredMatchingResources.Add(demodulatingResource);
				}
				else
				{
					removedResources.Add(demodulatingResource);
				}
			}

			Log(nameof(GetResourcesThatAreConnectedToSelectableDecodingResources), $"Removed resources: '{string.Join(", ", removedResources.Select(r => r.Name))}'");

			Log(nameof(GetResourcesThatAreConnectedToSelectableDecodingResources), $"Remaining resources: '{string.Join(", ", filteredMatchingResources.Select(r => r.Name))}'");

			return filteredMatchingResources;
		}

		private HashSet<FunctionResource> GetResourcesThatAreConnectedToSelectableDemodulatingResources(HashSet<FunctionResource> matrixOutputLbandResources, out HashSet<FunctionResource> removedResources)
		{
			if (matrixOutputLbandResources == null) throw new ArgumentNullException(nameof(matrixOutputLbandResources));

			var filteredMatchingResources = new HashSet<FunctionResource>();
			removedResources = new HashSet<FunctionResource>();

			if (!matrixOutputLbandResources.Any())
			{
				Log(nameof(GetResourcesThatAreConnectedToSelectableDemodulatingResources), $"No resources to filter");
				return filteredMatchingResources;
			}

			var demodulatingResources = GetSelectableResources(DemodulatingFunction);
			var timeRange = new TimeRangeUtc(service.StartWithPreRoll.ToUniversalTime(), service.EndWithPostRoll.ToUniversalTime());
			foreach (var matrixOutputLbandResource in matrixOutputLbandResources)
			{
				if (matrixOutputLbandResource == null) continue;

				bool connectedDemodulatingResourceIsAvailable = IsDemodulationResourceAvailable(matrixOutputLbandResource, demodulatingResources, timeRange);
				if (connectedDemodulatingResourceIsAvailable)
				{
					filteredMatchingResources.Add(matrixOutputLbandResource);
				}
				else
				{
					removedResources.Add(matrixOutputLbandResource);
				}
			}

			Log(nameof(GetResourcesThatAreConnectedToSelectableDemodulatingResources), $"Removed resources: '{string.Join(", ", removedResources.Select(r => r.Name))}'");

			Log(nameof(GetResourcesThatAreConnectedToSelectableDemodulatingResources), $"Remaining resources: '{string.Join(", ", filteredMatchingResources.Select(r => r.Name))}'");

			return filteredMatchingResources;
		}

		private bool IsDemodulationResourceAvailable(FunctionResource matrixOutputLbandResource, HashSet<FunctionResource> demodulatingResources, TimeRangeUtc timeRange)
		{
			foreach (var demodulatingResource in demodulatingResources)
			{
				if (demodulatingResource == null) continue;

				bool demodulatingResourceIsConnectedToMatrixOutputLbandResource = demodulatingResource.MatchesProfileParameter(helpers, new ProfileParameter
				{
					Id = ProfileParameterGuids.ResourceInputConnectionsLband,
					Name = "ResourceInputConnections_LBand",
					Value = matrixOutputLbandResource.Name
				},
				timeRange);

				if (demodulatingResourceIsConnectedToMatrixOutputLbandResource)
				{
					return true;
				}
			}

			return false;
		}

		private HashSet<FunctionResource> GetMatrixOutputResourcesThatAreConnectedToSelectableMatrixInputResources(HashSet<FunctionResource> matrixOutputLbandResources, out HashSet<FunctionResource> removedResources)
		{
			ArgumentNullCheck.ThrowIfNull(matrixOutputLbandResources, nameof(matrixOutputLbandResources));

			var filteredMatchingResources = new HashSet<FunctionResource>();
			removedResources = new HashSet<FunctionResource>();

			if (!matrixOutputLbandResources.Any())
			{
				Log(nameof(GetMatrixOutputResourcesThatAreConnectedToSelectableMatrixInputResources), $"No resources to filter");
				return filteredMatchingResources;
			}

			var matrixInputResources = GetSelectableResources(MatrixInputLbandFunction);

			var timeRange = new TimeRangeUtc(service.StartWithPreRoll.ToUniversalTime(), service.EndWithPostRoll.ToUniversalTime());

			foreach (var matrixOutputLbandResource in matrixOutputLbandResources.Where(r => r != null))
			{
				bool anyMatchingMatrixInputResourceOfSameMatrixAvailable = false;
				foreach (var matrixInputResource in matrixInputResources)
				{
					if (matrixInputResource == null) continue;

					var matrixProfileParameter = new ProfileParameter
					{
						Id = ProfileParameterGuids._Matrix,
						Name = "_Matrix",
						Value = matrixOutputLbandResource.Properties.SingleOrDefault(p => p.Name == ResourcePropertyNames.Matrix || p.Name == ResourcePropertyNames._Matrix)?.Value
					};

					bool matrixInputResourceIsConnectedToMatrixOutputLbandResource = matrixInputResource.MatchesProfileParameter(helpers, matrixProfileParameter, timeRange);

					if (matrixInputResourceIsConnectedToMatrixOutputLbandResource)
					{
						anyMatchingMatrixInputResourceOfSameMatrixAvailable = true;
						break;
					}
				}

				if (anyMatchingMatrixInputResourceOfSameMatrixAvailable)
				{
					filteredMatchingResources.Add(matrixOutputLbandResource);
				}
				else
				{
					removedResources.Add(matrixOutputLbandResource);
				}
			}

			Log(nameof(GetMatrixOutputResourcesThatAreConnectedToSelectableMatrixInputResources), $"Removed resources: '{string.Join(", ", removedResources.Select(r => r.Name))}'");

			Log(nameof(GetMatrixOutputResourcesThatAreConnectedToSelectableMatrixInputResources), $"Remaining resources: '{string.Join(", ", filteredMatchingResources.Select(r => r.Name))}'");

			return filteredMatchingResources;
		}

		private HashSet<FunctionResource> GetAntennaResourcesThatAreConnectedToSelectableMatrixOutputResources(HashSet<FunctionResource> antennaResources, out HashSet<FunctionResource> removedResources)
		{
			ArgumentNullCheck.ThrowIfNull(antennaResources, nameof(antennaResources));

			var filteredMatchingResources = new HashSet<FunctionResource>();
			removedResources = new HashSet<FunctionResource>();

			if (!antennaResources.Any())
			{
				Log(nameof(GetAntennaResourcesThatAreConnectedToSelectableMatrixInputResources), $"No resources to filter");
				return filteredMatchingResources;
			}

			var matrixOutputLbandResources = GetSelectableResources(MatrixOutputLbandFunction);

			var timeRange = new TimeRangeUtc(service.StartWithPreRoll.ToUniversalTime(), service.EndWithPostRoll.ToUniversalTime());

			foreach (var antennaResource in antennaResources.Where(r => r != null))
			{
				bool connectedResourceIsAvailable = false;
				foreach (var matrixOutputLbandResource in matrixOutputLbandResources)
				{
					if (matrixOutputLbandResource == null) continue;

					bool matrixOutputLbandResourceIsConnectedToAntennaResource = matrixOutputLbandResource.MatchesProfileParameter(helpers, new ProfileParameter
					{
						Id = ProfileParameterGuids.ResourceInputConnectionsLband,
						Name = "ResourceInputConnections_LBand",
						Value = antennaResource.Name
					},
					timeRange);

					if (matrixOutputLbandResourceIsConnectedToAntennaResource)
					{
						connectedResourceIsAvailable = true;
						break;
					}
				}

				if (connectedResourceIsAvailable)
				{
					filteredMatchingResources.Add(antennaResource);
				}
				else
				{
					removedResources.Add(antennaResource);
				}
			}

			Log(nameof(GetAntennaResourcesThatAreConnectedToSelectableMatrixOutputResources), $"Removed resources: '{string.Join(", ", removedResources.Select(r => r.Name))}'");

			Log(nameof(GetAntennaResourcesThatAreConnectedToSelectableMatrixOutputResources), $"Remaining resources: '{string.Join(", ", filteredMatchingResources.Select(r => r.Name))}'");

			return filteredMatchingResources;
		}

		private HashSet<FunctionResource> GetAntennaResourcesThatAreConnectedToSelectableMatrixInputResources(HashSet<FunctionResource> antennaResources, out HashSet<FunctionResource> removedResources)
		{
			ArgumentNullCheck.ThrowIfNull(antennaResources, nameof(antennaResources));

			var filteredMatchingResources = new HashSet<FunctionResource>();
			removedResources = new HashSet<FunctionResource>();

			if (!antennaResources.Any())
			{
				Log(nameof(GetAntennaResourcesThatAreConnectedToSelectableMatrixInputResources), $"No resources to filter");
				return filteredMatchingResources;
			}

			var matrixInputLbandResources = GetSelectableResources(MatrixInputLbandFunction);

			var timeRange = new TimeRangeUtc(service.StartWithPreRoll.ToUniversalTime(), service.EndWithPostRoll.ToUniversalTime());

			foreach (var antennaResource in antennaResources.Where(r => r != null))
			{
				bool connectedResourceIsAvailable = false;
				foreach (var matrixInputLbandResource in matrixInputLbandResources)
				{
					if (matrixInputLbandResource == null) continue;

					bool matrixInputLbandResourceIsConnectedToAntennaResource = matrixInputLbandResource.MatchesProfileParameter(helpers, new ProfileParameter
					{
						Id = ProfileParameterGuids.ResourceInputConnectionsLband,
						Name = "ResourceInputConnections_LBand",
						Value = antennaResource.Name
					},
					timeRange);

					if (matrixInputLbandResourceIsConnectedToAntennaResource)
					{
						connectedResourceIsAvailable = true;
						break;
					}
				}

				if (connectedResourceIsAvailable)
				{
					filteredMatchingResources.Add(antennaResource);
				}
				else
				{
					removedResources.Add(antennaResource);
				}
			}

			Log(nameof(GetAntennaResourcesThatAreConnectedToSelectableMatrixInputResources), $"Removed resources: '{string.Join(", ", removedResources.Select(r => r.Name))}'");

			Log(nameof(GetAntennaResourcesThatAreConnectedToSelectableMatrixInputResources), $"Remaining resources: '{string.Join(", ", filteredMatchingResources.Select(r => r.Name))}'");

			return filteredMatchingResources;
		}

		private Function MatrixInputLbandFunction => service.Functions.Single(f => f.Id == FunctionGuids.MatrixInputLband);

		private Function MatrixOutputLbandFunction => service.Functions.Single(f => f.Id == FunctionGuids.MatrixOutputLband);

		/// <summary>
		/// Gets the Demodulating function from the Satellite RX Service.
		/// </summary>
		private Function DemodulatingFunction => service.Functions.Single(f => f.Id == FunctionGuids.Demodulating);

		/// <summary>
		/// Gets the Decoding function from the Satellite RX Service.
		/// </summary>
		private Function DecodingFunction => service.Functions.Single(f => f.Id == FunctionGuids.Decoding);
	}
}
