namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	/// <summary>
	/// Wrapper class around the ResourceManagerHelper.
	/// This was introduced to allow mocking of this class through the IResourceManager interface.
	/// </summary>
	public class ResourceManager : HelpedObject, IResourceManager
	{
		private readonly List<CachedEligibleResourceResult> cachedEligibleResourceResults = new List<CachedEligibleResourceResult>();

		public ResourceManager(Helpers helpers) : base(helpers)
		{

		}

		public void ClearCache()
		{
			cachedEligibleResourceResults.Clear();
		}

		public bool TryGetAllResourceFromPool(string poolName, out HashSet<OccupiedResource> resources)
		{
			try
			{
				resources = GetAllResourcesFromPool(poolName);
				return true;
			}
			catch (Exception ex)
			{
				Log(nameof(TryGetAllResourceFromPool), $"Exception occurred: {ex}");
				resources = new HashSet<OccupiedResource>();
				return false;
			}
		}

		public HashSet<OccupiedResource> GetAllResourcesFromPool(string poolName)
		{
			var resourcePool = helpers.ResourceManager.GetResourcePoolByName(poolName) ?? throw new NotFoundException($"Could not find resource pool {poolName}");

			var resources = helpers.ResourceManager.GetResources(ResourceExposers.PoolGUIDs.Contains(resourcePool.ID)).OfType<FunctionResource>().Select(r => new OccupiedResource(r)).ToHashSet();

			Log(nameof(GetAllResourcesFromPool), $"All resources from pool {poolName}: \n{string.Join(", ", resources.Select(r => r.Name))}");

			return resources;
		}

		public ResourcePool[] GetResourcePools(params ResourcePool[] filters)
		{
			return DataMinerInterface.ResourceManager.GetResourcePools(helpers, filters);
		}

		public IEnumerable<Resource> GetResources(FilterElement<Resource> filter)
		{
			return DataMinerInterface.ResourceManager.GetResources(helpers, filter);
		}

		public IEnumerable<Resource> GetResources(params Resource[] filters)
		{
			return DataMinerInterface.ResourceManager.GetResources(helpers, filters);
		}

		public ResourcePool GetResourcePoolByName(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return null;

			return DataMinerInterface.ResourceManager.GetResourcePools(helpers, new ResourcePool { Name = name }).FirstOrDefault();
		}

		public Resource[] GetFeenixSourceResources(Guid poolGuid, Guid feenixSourceParameterId)
		{
			return DataMinerInterface.ResourceManager.GetResources(helpers, ResourceExposers.PoolGUIDs.Contains(poolGuid)).Where(x => x.Capabilities.Any(c => c.CapabilityProfileID.Equals(feenixSourceParameterId) && c.Value.Discreets.Contains("True"))).ToArray();
		}

		public IEnumerable<Resource> GetResourcesByName(string name)
		{
			return DataMinerInterface.ResourceManager.GetResources(helpers, ResourceExposers.Name.Equal(name));
		}

		public ReservationInstance GetReservationInstance(Guid id)
		{
			return DataMinerInterface.ResourceManager.GetReservationInstance(helpers, id);
		}

		public void RemoveReservationInstances(params ReservationInstance[] reservationInstances)
		{
			DataMinerInterface.ResourceManager.RemoveReservationInstances(helpers, reservationInstances);
		}

		public List<ReservationInstance> GetReservationsUsingResource(Guid resourceId)
		{
			var resourceFilter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(resourceId);

			var reservations = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, resourceFilter).ToList();

			return reservations;
		}

		public List<OccupyingService> GetOccupyingServices(FunctionResource resource, DateTime start, DateTime end, Guid ignoreOrderId, params string[] ignoreServiceNames)
		{
			if (resource == null)
            {
				Log(nameof(GetOccupyingServices), $"No resource provided => no checking of occupying services required");
				return new List<OccupyingService>();
			}

			var resourceFilter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(resource.ID);
			var startFilter = ReservationInstanceExposers.Start.LessThan(end);
			var endFilter = ReservationInstanceExposers.End.GreaterThan(start);

			var services = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(resourceFilter, startFilter, endFilter)).Where(s => !ignoreServiceNames.Contains(s.Name)).ToList();

			var occupyingServices = new List<OccupyingService>();

			foreach (var service in services)
			{
				var contributingResourceFilter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(service.ID);

				var occupyingOrders = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(contributingResourceFilter)).ToList();
				if (occupyingOrders.Any(o => ignoreOrderId.Equals(o.ID))) continue;

				occupyingServices.Add(new OccupyingService(service, occupyingOrders));
			}

			return occupyingServices;
		}

		public Dictionary<string, HashSet<FunctionResource>> GetAvailableResources(IEnumerable<YleEligibleResourceContext> contexts, bool useCache = false)
		{
			ArgumentNullCheck.ThrowIfNull(contexts, nameof(contexts));

			helpers.LogMethodStart(nameof(ResourceManager), nameof(GetAvailableResources), out var stopwatch);

			var availableResourcesPerFunction = new Dictionary<string, HashSet<FunctionResource>>();

			var contextsToPoll = new List<EligibleResourceContext>();

			foreach (var context in contexts)
			{
				if (useCache)
				{
					var cachedResult = cachedEligibleResourceResults.FirstOrDefault(cr => cr.Context.CanBeUsedInsteadOf(context));

					if (cachedResult != null)
					{
						availableResourcesPerFunction.Add(context.FunctionDefinitionLabel, new HashSet<FunctionResource>(cachedResult.FunctionResources)); // WARNING: new list should be created to have new object reference to avoid changing the cached values

						Log(nameof(GetAvailableResources), $"Cache use enabled. Found cached result for\n{context.ToString()}\nresulting in: {string.Join(", ", cachedResult.FunctionResources.Select(r => r.Name))}");
					}
					else
					{
						contextsToPoll.Add(context.GetContextForGetEligibleResourceCall());
					}
				}
				else
				{
					contextsToPoll.Add(context.GetContextForGetEligibleResourceCall());
				}
			}

			if (!contextsToPoll.Any())
			{
				helpers.LogMethodCompleted(nameof(ResourceManager), nameof(GetAvailableResources), null, stopwatch);
				return availableResourcesPerFunction;
			}

			GetEligibleResources(contexts, useCache, availableResourcesPerFunction, contextsToPoll);

			helpers.LogMethodCompleted(nameof(ResourceManager), nameof(GetAvailableResources), null, stopwatch);

			return availableResourcesPerFunction;
		}

		private void GetEligibleResources(IEnumerable<YleEligibleResourceContext> contexts, bool useCache, Dictionary<string, HashSet<FunctionResource>> availableResourcesPerFunction, List<EligibleResourceContext> contextsToPoll)
		{
			var eligibleResourceResults1 = DataMinerInterface.ResourceManager.GetEligibleResources(helpers, contextsToPoll) ?? new List<EligibleResourceResult>();

			var eligibleResourceResults2 = RetryMissingEligibleResources(contextsToPoll, eligibleResourceResults1);

			var allEligibleResourceResults = eligibleResourceResults1.Concat(eligibleResourceResults2).ToList();

			foreach (var eligibleResourceResult in allEligibleResourceResults)
			{
				if (eligibleResourceResult == null)
				{
					helpers.Log(nameof(ResourceManager), nameof(GetAvailableResources), $"Eligible Resource Result is null");
					continue;
				}

				var linkedContext = contexts.Single(c => c.ContextId == eligibleResourceResult.ForContextId);

				var availableResourcesForThisFunction = eligibleResourceResult.EligibleResources != null ? eligibleResourceResult.EligibleResources.Select(r => (FunctionResource)r).ToList() : new List<FunctionResource>();

				availableResourcesPerFunction.Add(linkedContext.FunctionDefinitionLabel, new HashSet<FunctionResource>(availableResourcesForThisFunction)); // WARNING: new list should be created to have new object reference to avoid changing the cached values

				cachedEligibleResourceResults.Add(new CachedEligibleResourceResult(linkedContext, availableResourcesForThisFunction));

				Log(nameof(GetAvailableResources), $"Cache use {(useCache ? "enabled, unable to find cached result" : "disabled")}. Retrieved eligible resources for\n{linkedContext.ToString()}\nresulting in: {string.Join(", ", availableResourcesForThisFunction.Select(r => r.Name))}");
			}
		}

		private List<EligibleResourceResult> RetryMissingEligibleResources(List<EligibleResourceContext> contexts, List<EligibleResourceResult> originalEligibleResourceResults)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));
			if (originalEligibleResourceResults == null) throw new ArgumentNullException(nameof(originalEligibleResourceResults));

			var retriedEligibleResourceResults = new List<EligibleResourceResult>();

			var contextsWithMissingResults = contexts.Where(c => !originalEligibleResourceResults.Any(result => result.ForContextId == c.ContextId)).ToList();

			if (!contextsWithMissingResults.Any()) return retriedEligibleResourceResults;

			helpers.Log(nameof(ResourceManager), nameof(RetryMissingEligibleResources), $"Trace data last call: {SrmManagers.ResourceManager.GetTraceDataLastCall().ToString()}");

			helpers.Log(nameof(ResourceManager), nameof(RetryMissingEligibleResources), $"Retrying for {string.Join(", ", contextsWithMissingResults.Select(c => $"Service {c.ReservationIdToIgnore?.Id} node {c.NodeIdToIgnore}"))}");

			var newContextsToPoll = new List<EligibleResourceContext>();

			foreach (var contextsWithMissingResult in contextsWithMissingResults)
			{
				// Removing the serviceIdToIgnore and NodeIdToIgnore as they are causing issues
				newContextsToPoll.Add(new EligibleResourceContext
				{
					ContextId = contextsWithMissingResult.ContextId,
					TimeRange = contextsWithMissingResult.TimeRange,
					RequiredCapabilities = contextsWithMissingResult.RequiredCapabilities,
					ResourceFilter = contextsWithMissingResult.ResourceFilter
				});
			}

			if (newContextsToPoll.Any())
			{
				retriedEligibleResourceResults = DataMinerInterface.ResourceManager.GetEligibleResources(helpers, newContextsToPoll) ?? new List<EligibleResourceResult>();
			}

			return retriedEligibleResourceResults;
		}

		public HashSet<FunctionResource> GetConnectedResources(FunctionResource resourceToConnectWith, IEnumerable<FunctionResource> resourcesToCheck, Guid connectionCapabilityId)
		{
			if (resourceToConnectWith == null) return new HashSet<FunctionResource>();

			var selectableResources = new HashSet<FunctionResource>();

			var resourceConnectionsProfileParameter = new Profile.ProfileParameter
			{
				Id = connectionCapabilityId,
				Value = resourceToConnectWith.Name
			};

			foreach (var availableMatrixInputResource in resourcesToCheck)
			{
				if (availableMatrixInputResource.MatchesProfileParameter(helpers, resourceConnectionsProfileParameter, null))
				{
					selectableResources.Add(availableMatrixInputResource);
				}
			}

			Log(nameof(GetConnectedResources), $"Resources '{string.Join(", ", selectableResources.Select(r => r.Name))}' are connected to {resourceToConnectWith.Name} via parameter {connectionCapabilityId}");

			foreach (var selectableResource in selectableResources)
			{
				if (selectableResource.Properties == null) selectableResource.Properties = new List<ResourceManagerProperty>();

				var priorityProperty = selectableResource.Properties.SingleOrDefault(p => p.Name == "Priority");

				if (priorityProperty == null)
				{
					selectableResource.Properties.Add(new ResourceManagerProperty("Priority", "99")); // resources without priority get low priority

					Log(nameof(GetConnectedResources), $"Added prio 99 to resource {selectableResource.Name}");
				}
				else if (!int.TryParse(priorityProperty.Value, out var parsed))
				{
					Log(nameof(GetConnectedResources), $"Changed invalid prio value '{priorityProperty.Value}' to 99 for resource {selectableResource.Name}");

					priorityProperty.Value = "99";
				}
			}

			return selectableResources;
		}

		private class CachedEligibleResourceResult
		{
			public CachedEligibleResourceResult(YleEligibleResourceContext context, List<FunctionResource> result)
			{
				Context = context;
				FunctionResources = result;
			}

			public YleEligibleResourceContext Context { get; }

			public List<FunctionResource> FunctionResources { get; }
		}
	}
}
