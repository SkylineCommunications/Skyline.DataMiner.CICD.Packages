namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ResourceAssignment
{
	using System.Diagnostics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DTR;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Time;
	using Function = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function.Function;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using VirtualPlatform = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Library;

	[Flags]
	public enum FilterOptions
	{
		None = 0,
		IntraService = 1, // check conditions within the service itself to filter resources
		InterService = 2, // check conditions across services within the order to filter resources
		FixedTieLines = 4, // check fixed tie line logic
	}

	/* TERMINOLOGY
	 * Available resource = resource that is available for the full time range of the service.
	 * Matching resource = resource that matches all profile parameters for a given function.
	 * Filtered resource = resource that meets certain custom filter requirements depending on the service and function
	 * Prioritized resources = resources in a collection ordered by descending priority
	 * Selectable resource = available + matching + filtered + prioritized. When assigning a resource to a function, this is the only valid category.
	 */

	public abstract class ResourceAssignmentHandler
	{
		private const string YleHelsinkiDestinationLocationUutisalue = "Uutisalue";

		protected const string LimitedCapacity = "Limited Capacity";

		protected readonly Helpers helpers;
		protected readonly IEngine engine;
		protected readonly DisplayedService service;
		protected readonly Order order;
		protected readonly List<string> handledFunctionLabels = new List<string>();

		protected DateTime? utcNow = null;

		// Private fields to limit the amount of GetEligibleResources calls in the AvailableResources property getter
		private readonly Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges;
		private Dictionary<string, TimeRangeUtc> functionTimeRangesForResourceRequest;
		private Dictionary<string, HashSet<FunctionResource>> availableResources;
		private Dictionary<string, HashSet<OccupiedResource>> allResources;

		protected ResourceAssignmentHandler(Helpers helpers, Service service, Order orderContainingService, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.engine = helpers.Engine;

			this.service = service as DisplayedService ?? throw new ArgumentNullException(nameof(service));
			this.order = orderContainingService ?? throw new ArgumentNullException(nameof(orderContainingService));

			this.overwrittenFunctionTimeRanges = overwrittenFunctionTimeRanges;
		}

		/// <summary>
		/// Resources available within service time slot, sorted per function label.
		/// </summary>
		///<remarks>Updates automatically if service timing changed.</remarks>
		public Dictionary<string, HashSet<FunctionResource>> AvailableResources
		{
			get
			{
				var newFunctionTimeRangesForResourceRequest = GetFunctionTimeRangesForResourceRequest();

				bool functionTimeRangesChanged = functionTimeRangesForResourceRequest != null && functionTimeRangesForResourceRequest.Any(f => !f.Value.Equals(newFunctionTimeRangesForResourceRequest[f.Key]));

				bool availableResourcesUpdateRequired = availableResources == null || functionTimeRangesForResourceRequest == null || functionTimeRangesChanged;

				if (!availableResourcesUpdateRequired)
				{
					Log($"{nameof(AvailableResources)}.Get", $"No available resources update required, returning cached available resources: \n{string.Join("\n", availableResources.Select(r => $"{r.Key}: {string.Join(", ", r.Value.Select(res => res.Name))}"))}");

					return availableResources.ToDictionary(pair => pair.Key, pair => new HashSet<FunctionResource>(pair.Value));
					// create a copy of the cached resource lists to avoid that the user edits the cached value			
				}

				functionTimeRangesForResourceRequest = newFunctionTimeRangesForResourceRequest;

				availableResources = service.GetAvailableResourcesPerFunctionBasedOnTiming(helpers, functionTimeRangesForResourceRequest);

				var functionLabels = service.Functions.Select(f => f.Definition.Label).ToList();
				bool functionIsMissingFromResponse = functionLabels.Any(label => !availableResources.ContainsKey(label));
				int counter = 0;
				while (functionIsMissingFromResponse && counter <= 5)
				{
					Log($"{nameof(AvailableResources)}.Get", $"A function is missing from the response, retry {counter}");

					availableResources = service.GetAvailableResourcesPerFunctionBasedOnTiming(helpers, functionTimeRangesForResourceRequest);

					functionIsMissingFromResponse = functionLabels.Any(label => !availableResources.ContainsKey(label));
					counter++;
				}

				foreach (var function in service.Functions)
				{
					if (!availableResources.ContainsKey(function.Definition.Label)) throw new FunctionNotFoundException(function.Definition.Label, availableResources.Keys, true);

					Log($"{nameof(AvailableResources)}.Get", $"Available resources for function {function.Definition.Label} from {functionTimeRangesForResourceRequest[function.Definition.Label].Start} - {functionTimeRangesForResourceRequest[function.Definition.Label].Stop}, ignoring service {service.Name} ({service.Id}): {string.Join(",", availableResources[function.Definition.Label].Select(r => r.Name))}", function.Name);
				}

				return availableResources.ToDictionary(pair => pair.Key, pair => new HashSet<FunctionResource>(pair.Value));
				// create a copy of the cached resource lists to avoid that the user edits the cached value
			}
		}

		public Dictionary<string, HashSet<OccupiedResource>> AllResources
		{
			get
			{
				if (allResources == null)
				{
					allResources = new Dictionary<string, HashSet<OccupiedResource>>();

					foreach (var function in service.Functions)
					{
						if (helpers.ResourceManager.TryGetAllResourceFromPool(function.Definition.ResourcePool, out var resourcesForFunction))
						{
							allResources.Add(function.Definition.Label, resourcesForFunction);
						}
						else
						{
							allResources.Add(function.Definition.Label, new HashSet<OccupiedResource>());
						}
					}
				}

				Log($"{nameof(AllResources)}.Get", $"All resources: \n{string.Join("\n", allResources.Select(r => $"{r.Key}: {string.Join(", ", r.Value.Select(res => res.Name))}"))}");

				return allResources;
			}
		}

		public Dictionary<string, bool> FunctionAssignments { get; private set; } = new Dictionary<string, bool>();

		public bool AssignmentSuccessful
		{
			get
			{
				return FunctionAssignments.Values.All(x => x);
			}
		}

		public static ResourceAssignmentHandler Factory(Helpers helpers, Service service, Order orderContainingService, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (service == null) throw new ArgumentNullException(nameof(service));
			if (orderContainingService == null) throw new ArgumentNullException(nameof(orderContainingService));
			if (service.Definition == null) throw new ArgumentException("Service Definition is null", nameof(service));

			if (service.Definition.IsDummy) return null; // Resource assignment for dummy services is not applicable

			switch (service.Definition.VirtualPlatform)
			{
				case VirtualPlatform.Routing:
					// Only to be used by service controller in Update Service script
					// Book Services uses GenerateRoutingServicesTask
					return new RoutingResourceAssignmentHandler(helpers, service, orderContainingService, overwrittenFunctionTimeRanges);

				case VirtualPlatform.ReceptionSatellite:
					return new SatRxResourceAssignmentHandler(helpers, service, orderContainingService, overwrittenFunctionTimeRanges);

				case VirtualPlatform.TransmissionSatellite:
					return new SatTxResourceAssignmentHandler(helpers, service, orderContainingService, overwrittenFunctionTimeRanges);

				case VirtualPlatform.ReceptionFixedService:
				case VirtualPlatform.ReceptionFiber when service.Definition.Description == LimitedCapacity:
				case VirtualPlatform.TransmissionFiber when service.Definition.Description == LimitedCapacity:
				case VirtualPlatform.AudioProcessing:
				case VirtualPlatform.ReceptionIp when service.Definition.Id == ServiceDefinitionGuids.IpReceptionRtmp:
				case VirtualPlatform.ReceptionIp when service.Definition.Id == ServiceDefinitionGuids.IpReceptionSrt:
					return new DtrResourceAssignmentHandler(helpers, service, orderContainingService, overwrittenFunctionTimeRanges);

				default:
					return new DefaultResourceAssignmentHandler(helpers, service, orderContainingService, overwrittenFunctionTimeRanges);
			}
		}

		public abstract void AssignResources();

		/// <summary>
		/// Gets a list of resources that match the profile parameters in <paramref name="function"/>.
		/// </summary>
		/// <param name="function">The function containing the profile parameters to filter.</param>
		/// <param name="resourcesToMatch">The resources that will be filtered.</param>
		/// <remarks>See terminology at top of this class for more info.</remarks>
		public HashSet<FunctionResource> MatchResources(Function function, HashSet<FunctionResource> resourcesToMatch)
		{
			if (function == null) throw new ArgumentNullException(nameof(function));
			if (resourcesToMatch == null) throw new ArgumentNullException(nameof(resourcesToMatch));

			LogMethodStart(nameof(MatchResources), out var stopwatch, service.Name);

			var functionProfileParametersToMatch = function.Capabilities.Where(c => !string.IsNullOrWhiteSpace(c.StringValue)).ToList();

			Log(nameof(MatchResources), $"Capabilities taken into account: {string.Join(",", functionProfileParametersToMatch.Select(c => $"{c.Name}={c.StringValue}"))}", function.Name);

			var timeDynamicProfileParameters = GetTimeDynamicProfileParameters(resourcesToMatch, functionProfileParametersToMatch);

			var matchingResources = MatchResourcesToTimeDynamicProfileParameters(resourcesToMatch, function, timeDynamicProfileParameters);

			matchingResources = MatchResourcesToNonTimeDynamicProfileParameters(function, matchingResources, timeDynamicProfileParameters, out var nonMatchingResources);

			Log(nameof(MatchResources), $"Non-matching resources: {string.Join(", ", nonMatchingResources.Select(r => r.Name))}", function.Name);

			Log(nameof(MatchResources), $"Matching resources: {string.Join(", ", matchingResources.Select(r => r.Name))}", function.Name);

			LogMethodCompleted(nameof(MatchResources), null, stopwatch);

			return matchingResources;
		}

		private Dictionary<string, TimeRangeUtc> GetFunctionTimeRangesForResourceRequest()
		{
			var serviceTimeRange = GetServiceTimeRange();

			var functionTimeRanges = new Dictionary<string, TimeRangeUtc>();

			foreach (var function in service.Functions)
			{
				if (function.EnforceSelectedResource || overwrittenFunctionTimeRanges is null || !overwrittenFunctionTimeRanges.TryGetValue(function.Definition.Label, out var overwrittenFunctionTimeRange))
				{
					functionTimeRanges.Add(function.Definition.Label, serviceTimeRange);
				}
				else
				{
					var earliestStart = new[] { serviceTimeRange.Start, overwrittenFunctionTimeRange.Start }.Min();
					var latestEnd = new[] { serviceTimeRange.Stop, overwrittenFunctionTimeRange.Stop }.Max();

					functionTimeRanges.Add(function.Definition.Label, new TimeRangeUtc(earliestStart, latestEnd));
				}
			}

			Log(nameof(GetFunctionTimeRangesForResourceRequest), $"Service time range is {serviceTimeRange}\nFunction time ranges for resource request are \n{string.Join("\n", functionTimeRanges.Select(f => $"{f.Key} {f.Value}"))}", service.Name);

			return functionTimeRanges;
		}

		private TimeRangeUtc GetServiceTimeRange()
		{
			if (order.StartNow)
			{
				DateTime start = default(DateTime);
				try
				{
					utcNow = utcNow ?? DateTime.UtcNow;

					// the actual start time of a service in a start now order will be earlier than the configured start time

					var earliestPossibleServiceStartTime = service.StartWithPreRoll.ToUniversalTime().Add(-TimeSpan.FromMinutes(Order.StartNowDelayInMinutes));

					start = earliestPossibleServiceStartTime < utcNow.Value ? utcNow.Value : earliestPossibleServiceStartTime;

					return new TimeRangeUtc(start, service.EndWithPostRoll.ToUniversalTime());
				}
				catch (Exception)
				{
					Log(nameof(GetServiceTimeRange), $"Exception while creating timerange for service {service.Name} (part of startnow order) for values from {start.ToFullDetailString()} until {nameof(service.EndWithPostRoll)}={service.EndWithPostRoll.ToFullDetailString()}");
					throw;
				}
			}
			else
			{
				try
				{
					return new TimeRangeUtc(service.StartWithPreRoll.ToUniversalTime(), service.EndWithPostRoll.ToUniversalTime());
				}
				catch (Exception)
				{
					Log(nameof(GetServiceTimeRange), $"Exception while creating timerange for service {service.Name} with {nameof(service.StartWithPreRoll)}={service.StartWithPreRoll.ToFullDetailString()} and {nameof(service.EndWithPostRoll)}={service.EndWithPostRoll.ToFullDetailString()}");
					throw;
				}
			}
		}

		private HashSet<FunctionResource> MatchResourcesToNonTimeDynamicProfileParameters(Function function, HashSet<FunctionResource> resourcesToMatch, HashSet<ProfileParameter> timeDynamicProfileParameters, out List<FunctionResource> nonMatchingResources)
		{
			nonMatchingResources = new List<FunctionResource>();
			var matchingResources = new HashSet<FunctionResource>();

			LogMethodStart(nameof(MatchResourcesToNonTimeDynamicProfileParameters), out var stopwatch, service.Name);

			var timeRange = new TimeRangeUtc(service.StartWithPreRoll.ToUniversalTime(), service.EndWithPostRoll.ToUniversalTime());

			var nonTimeDynamicFunctionProfileParameters = function.Capabilities.Where(fc => !timeDynamicProfileParameters.Contains(fc)).ToList();

			Log(nameof(MatchResourcesToNonTimeDynamicProfileParameters), $"Capabilities taken into account: {string.Join(",", nonTimeDynamicFunctionProfileParameters.Select(c => $"{c.Name}={c.StringValue}"))}", function.Name);

			foreach (var resource in resourcesToMatch)
			{
				if (resource.MatchesProfileParameters(helpers, nonTimeDynamicFunctionProfileParameters, timeRange))
				{
					matchingResources.Add(resource);
				}
				else
				{
					nonMatchingResources.Add(resource);
				}
			}

			LogMethodCompleted(nameof(MatchResourcesToNonTimeDynamicProfileParameters), null, stopwatch);

			return matchingResources;
		}

		private HashSet<FunctionResource> MatchResourcesToTimeDynamicProfileParameters(HashSet<FunctionResource> resourceToMatch, Function function, HashSet<ProfileParameter> timeDynamicProfileParameters)
		{
			LogMethodStart(nameof(MatchResourcesToTimeDynamicProfileParameters), out var stopwatch, service.Name);

			Log(nameof(MatchResourcesToTimeDynamicProfileParameters), $"Time Dynamic capabilities taken into account: {string.Join(",", timeDynamicProfileParameters.Select(c => $"{c.Name}={c.StringValue}"))}", function.Name);

			if (!timeDynamicProfileParameters.Any())
			{
				LogMethodCompleted(nameof(MatchResourcesToTimeDynamicProfileParameters), null, stopwatch);
				return resourceToMatch;
			}

			var reservationIdToIgnore = service.IsBooked && function.Resource != null ? service.Id : (Guid?)null;
			var nodeIdToIgnore = service.IsBooked && function.Resource != null ? function.NodeId : (int?)null;

			var context = function.Definition.GetEligibleResourceContext(helpers, service.StartWithPreRoll.ToUniversalTime(), service.EndWithPostRoll.ToUniversalTime(), reservationIdToIgnore, nodeIdToIgnore, timeDynamicProfileParameters);

			var eligibleResourceResult = helpers.ResourceManager.GetAvailableResources(context.Yield(), true);

			var matchingResources = eligibleResourceResult[function.Definition.Label];

			var matchingResourcesIds = matchingResources.Select(r => r.ID).ToList();
			var nonMatchingResources = resourceToMatch.Where(r => !matchingResourcesIds.Contains(r.ID)).ToList();

			Log(nameof(MatchResourcesToTimeDynamicProfileParameters), $"Non-matching resources: {string.Join(", ", nonMatchingResources.Select(r => r.Name))}", function.Name);

			Log(nameof(MatchResourcesToTimeDynamicProfileParameters), $"Matching resources: {string.Join(", ", matchingResources.Select(r => r.Name))}", function.Name);

			LogMethodCompleted(nameof(MatchResourcesToTimeDynamicProfileParameters), null, stopwatch);

			return matchingResources;
		}

		private HashSet<ProfileParameter> GetTimeDynamicProfileParameters(HashSet<FunctionResource> resourcesToMatch, List<ProfileParameter> functionProfileParameters)
		{
			var timeDynamicProfileParameters = new HashSet<ProfileParameter>();

			var functionCapabilityIds = functionProfileParameters.Select(fc => fc.Id).ToList();

			foreach (var resource in resourcesToMatch)
			{
				var resourceCapabilities = resource.Capabilities.Where(rc => functionCapabilityIds.Contains(rc.CapabilityProfileID)).ToList();

				var timeDynamicCapabilities = resourceCapabilities.Where(c => c.IsTimeDynamic);

				foreach (var timeDynamicCapability in timeDynamicCapabilities)
				{
					timeDynamicProfileParameters.Add(functionProfileParameters.First(fc => fc.Id == timeDynamicCapability.CapabilityProfileID));
				}
			}

			bool resourcesHaveTimeDynamicCapabilities = timeDynamicProfileParameters.Any();

			Log(nameof(GetTimeDynamicProfileParameters), $"Resources to match {(resourcesHaveTimeDynamicCapabilities ? "have" : "do not have")} time dependent/dynamic capabilities {(resourcesHaveTimeDynamicCapabilities ? $": {string.Join(", ", timeDynamicProfileParameters.Select(pp => $"{pp.Name}='{pp.StringValue}'"))}" : string.Empty)}");

			return timeDynamicProfileParameters;
		}

		/// <remarks>See terminology at top of this class for more info.</remarks>
		public HashSet<FunctionResource> FilterResources(Function function, HashSet<FunctionResource> resourcesToFilter, FilterOptions requiredFiltering = FilterOptions.IntraService | FilterOptions.InterService)
		{
			if (function == null) throw new ArgumentNullException(nameof(function));
			if (resourcesToFilter == null) throw new ArgumentNullException(nameof(resourcesToFilter));

			var filteredResources = resourcesToFilter;

			if (requiredFiltering.HasFlag(FilterOptions.IntraService)) filteredResources = FilterIntraService(function, filteredResources);

			if (requiredFiltering.HasFlag(FilterOptions.InterService)) filteredResources = FilterInterService(function, filteredResources);

			if (requiredFiltering.HasFlag(FilterOptions.FixedTieLines)) filteredResources = FilterForFixedTieLines(function, filteredResources);

			return filteredResources;
		}

		/// <summary>
		/// Filters resources based on data from other services within the order.
		/// </summary>
		protected abstract HashSet<FunctionResource> FilterInterService(Function function, HashSet<FunctionResource> resourcesToFilter);

		/// <summary>
		/// Filters resources based on data from within the same service.
		/// </summary>
		protected abstract HashSet<FunctionResource> FilterIntraService(Function function, HashSet<FunctionResource> resourcesToFilter);

		protected abstract HashSet<FunctionResource> FilterForFixedTieLines(Function function, HashSet<FunctionResource> resourcesToFilter);

		/// <summary>
		/// Gets a list with all resources from <paramref name="resourcesToPrioritize"/> ordered from high to low priority.
		/// </summary>
		/// <param name="resourcesToPrioritize">The resources that should be ordered.</param>
		/// <remarks>See terminology at top of this class for more info.</remarks>
		public HashSet<FunctionResource> PrioritizeResources(HashSet<FunctionResource> resourcesToPrioritize)
		{
			if (resourcesToPrioritize == null) throw new ArgumentNullException(nameof(resourcesToPrioritize));

			if (ServiceIsYleHelsinkiUutisalueDestination(service))
			{
				PrioritizeDestinationResourcesLinkedToExistingRoutingServices(resourcesToPrioritize);
			}

			if (service.Definition.VirtualPlatform == VirtualPlatform.Destination)
			{
				PrioritizeLinkedResource(resourcesToPrioritize, ResourcePropertyNames.PrioritizedDestinationResource);
			}
			else if (ServiceCategorizer.IsMessiLiveRecording(helpers, service))
			{
				PrioritizeLinkedResource(resourcesToPrioritize, ResourcePropertyNames.PrioritizedMessiLiveRecordingResource);
			}
			else
			{
				//Nothing
			}

			var prioritizedResources = new HashSet<FunctionResource>(resourcesToPrioritize.OrderBy(r => r.GetPriority()));

			Log(nameof(PrioritizeResources), $"Prioritized resources: {string.Join(", ", prioritizedResources.Select(r => r.Name))}");

			return prioritizedResources;
		}

		private void PrioritizeLinkedResource(HashSet<FunctionResource> resourcesToPrioritize, string prioritizedResourcePropertyName)
		{
			// DCP203205

			var prioritizedResourceName = order.SourceService.Functions.Select(f => f.Resource?.GetResourcePropertyStringValue(prioritizedResourcePropertyName)).FirstOrDefault(prioritizedDestinationResource => !string.IsNullOrWhiteSpace(prioritizedDestinationResource));

			var prioritizedResource = resourcesToPrioritize.SingleOrDefault(r => r.Name == prioritizedResourceName);

			if (prioritizedResource != null)
			{
				prioritizedResource.UpdatePriority(-1);

				Log(nameof(PrioritizeLinkedResource), $"Set priority of prioritized resource {prioritizedResource.Name} to -1");
			}
			else
			{
				Log(nameof(PrioritizeLinkedResource), $"Unable to find prioritized resource {prioritizedResourceName}");
			}
		}

		private void PrioritizeDestinationResourcesLinkedToExistingRoutingServices(HashSet<FunctionResource> destinationResources)
		{
			// UMX Line Reuse case 2 specifies that the Destination resource should be selected based on the already booked tie line for the Messi News recording.

			var existingRoutingServices = order.AllServices.Where(s => s.Definition.VirtualPlatform == VirtualPlatform.Routing).ToList();
			var matrixOutputSdiResources = existingRoutingServices.Select(rs => rs.Functions.Single(f => f.Id == FunctionGuids.MatrixOutputSdi).Resource).Where(r => r != null).ToList();

			Log(nameof(PrioritizeDestinationResourcesLinkedToExistingRoutingServices), $"Matrix Output SDI resources in order: {string.Join(", ", matrixOutputSdiResources.Select(r => r.Name))}");

			foreach (var matrixOutputResource in matrixOutputSdiResources)
			{
				foreach (var destinationResource in destinationResources)
				{
					if (matrixOutputResource.HasCapabilityDiscreetValue(ProfileParameterGuids.ResourceOutputConnectionsSdi, destinationResource.Name))
					{
						destinationResource.UpdatePriority(0);

						Log(nameof(PrioritizeDestinationResourcesLinkedToExistingRoutingServices), $"Set Destination Resource {destinationResource.Name} priority to 0 (highest prio), because it is connected to already used routing resource {matrixOutputResource.Name}");
					}
				}
			}
		}

		protected bool ServiceIsYleHelsinkiUutisalueDestination(Service service)
		{
			var destinationLocation = service.Functions.SelectMany(f => f.Parameters).FirstOrDefault(p => p != null && p.Id == ProfileParameterGuids.YleHelsinkiDestinationLocation);

			bool serviceIsYleHelsinkiUutisalueDestination;
			if (destinationLocation == null)
			{
				serviceIsYleHelsinkiUutisalueDestination = false;
			}
			else
			{
				serviceIsYleHelsinkiUutisalueDestination = service.Definition.VirtualPlatform == VirtualPlatform.Destination && destinationLocation.StringValue.Equals(YleHelsinkiDestinationLocationUutisalue, StringComparison.InvariantCultureIgnoreCase);
			}

			Log(nameof(ServiceIsYleHelsinkiUutisalueDestination), $"Service is{(serviceIsYleHelsinkiUutisalueDestination ? string.Empty : " not")} a YLE Helsinki Uutisalue Destination");

			return serviceIsYleHelsinkiUutisalueDestination;
		}

		/// <summary>
		/// Gets a list of resources that are selectable for the given <paramref name="function"/>.
		/// </summary>
		/// <param name="function">The function for which to get the selectable resources.</param>
		/// <param name="requiredFiltering">A flags enum indicating which filtering is required.</param>
		/// <param name="useAvailableResources">An optional boolean indicating if available resources should be used or all resources.</param>
		/// <remarks>See terminology at top of this class for more info.</remarks>
		public HashSet<FunctionResource> GetSelectableResources(Function function, FilterOptions requiredFiltering = FilterOptions.IntraService | FilterOptions.InterService | FilterOptions.FixedTieLines, bool useAvailableResources = true)
		{
			LogMethodStart(nameof(GetSelectableResources), out var stopwatch, function.Definition.Label);

			var availableResourcesForFunction = useAvailableResources ? AvailableResources[function.Definition.Label] : Enumerable.ToHashSet(AllResources[function.Definition.Label].Cast<FunctionResource>());

			var matchingAvailableResources = MatchResources(function, availableResourcesForFunction);

			if (!matchingAvailableResources.Any())
			{
				LogOccupiedMatchingResources(function);
			}

			var filteredMatchingAvailableResources = FilterResources(function, matchingAvailableResources, requiredFiltering);

			var prioritizedFilteredMatchingAvailableResources = PrioritizeResources(filteredMatchingAvailableResources);

			Log(nameof(GetSelectableResources), $"Selectable resources: {string.Join(", ", prioritizedFilteredMatchingAvailableResources.Select(r => r.Name))}", function.Definition.Label);

			LogMethodCompleted(nameof(GetSelectableResources), function.Definition.Label, stopwatch);

			return prioritizedFilteredMatchingAvailableResources;
		}

		private void LogOccupiedMatchingResources(Function function)
		{
			LogMethodStart(nameof(LogOccupiedMatchingResources), out var stopwatch, function.Definition.Label);

			var allFunctionResources = Enumerable.ToHashSet(AllResources[function.Definition.Label].Cast<FunctionResource>());

			var allMatchingResources = MatchResources(function, allFunctionResources);

			foreach (var matchingOccupiedResource in allMatchingResources.Select(r => new OccupiedResource(r)))
			{
				var occupyingServices = helpers.ResourceManager.GetOccupyingServices(matchingOccupiedResource, service.StartWithPreRoll, service.EndWithPostRoll, order.Id, service.Name);

				Log(nameof(LogOccupiedMatchingResources), $"Matching unavailable resource {matchingOccupiedResource.Name} is occupied by '{string.Join("\n", occupyingServices.Select(o => o.ToString()))}'");
			}

			LogMethodCompleted(nameof(LogOccupiedMatchingResources), function.Definition.Label, stopwatch);
		}

		public FunctionResource SelectCurrentOrNewResource(DisplayedFunction function, HashSet<FunctionResource> selectableResources, out bool functionResourceChanged)
		{
			if (function == null) throw new ArgumentNullException(nameof(function));
			if (selectableResources == null) throw new ArgumentNullException(nameof(selectableResources));

			var currentResourceId = function.Resource?.ID ?? Guid.Empty;
			FunctionResource selectedMatchingResource = null;

			if (!function.RequiresResource || function.IsDummy)
			{
				Log(nameof(SelectCurrentOrNewResource), $"Function does not require resource or is dummy, setting resource to None", function.Definition.Label);
				selectedMatchingResource = null;
				FunctionAssignments[function.Definition.Label] = true;
			}
			else if (function.EnforceSelectedResource)
			{
				Log(nameof(SelectCurrentOrNewResource), $"Trying to enforce {function.ResourceName} ...", function.Definition.Label);
				selectedMatchingResource = EnforceSelectedResource(selectableResources, function);
			}
			else
			{
				Log(nameof(SelectCurrentOrNewResource), $"No need to enforce {function.ResourceName} ...", function.Definition.Label);
				selectedMatchingResource = AssignResourceNoEnforcement(selectableResources, function);
			}

			functionResourceChanged = (selectedMatchingResource?.ID ?? Guid.Empty) != currentResourceId;

			Log(nameof(SelectCurrentOrNewResource), $"Selected resource '{selectedMatchingResource?.Name}'", function.Definition.Label);

			Log(nameof(SelectCurrentOrNewResource), $"Resource assignment is {(AssignmentSuccessful ? "still" : "no longer")} successful", function.Definition.Label);

			return selectedMatchingResource;
		}

		private FunctionResource EnforceSelectedResource(HashSet<FunctionResource> selectableResources, DisplayedFunction function)
		{
			bool noResourceAssigned = function.Resource == null;
			bool currentResourceIsSelectable = function.Resource != null && selectableResources.Contains(function.Resource);
			FunctionResource selectedMatchingResource = null;

			if (noResourceAssigned)
			{
				if (function.ResourceSelectionMandatory)
				{
					selectedMatchingResource = GetFirstAlphabeticalHighestPriorityResource(selectableResources);
					FunctionAssignments[function.Definition.Label] = selectedMatchingResource != null;

					Log(nameof(SelectCurrentOrNewResource), $"Not allowed to enforce keeping resource {function.ResourceName} because resource selection is mandatory, selected '{selectedMatchingResource?.Name}'", function.Definition.Label);
				}
				else
				{
					selectedMatchingResource = null;

					Log(nameof(SelectCurrentOrNewResource), $"Enforced no resource assigned", function.Definition.Label);
				}
			}
			else if (currentResourceIsSelectable)
			{
				selectedMatchingResource = function.Resource;
				FunctionAssignments[function.Definition.Label] = selectedMatchingResource != null;

				Log(nameof(SelectCurrentOrNewResource), $"Enforced keeping resource {function.ResourceName}", function.Definition.Label);
			}
			else
			{
				selectedMatchingResource = GetFirstAlphabeticalHighestPriorityResource(selectableResources);
				FunctionAssignments[function.Definition.Label] = selectedMatchingResource != null;

				Log(nameof(SelectCurrentOrNewResource), $"Unable to enforce keeping resource {function.ResourceName}, selected '{selectedMatchingResource?.Name}' instead", function.Definition.Label);
			}

			return selectedMatchingResource;
		}

		private FunctionResource AssignResourceNoEnforcement(HashSet<FunctionResource> selectableResources, DisplayedFunction function)
		{
			bool noResourceAssigned = function.Resource == null;
			bool currentResourceIsSelectable = function.Resource != null && selectableResources.Contains(function.Resource);
			FunctionResource selectedMatchingResource = null;

			if (noResourceAssigned || !currentResourceIsSelectable)
			{
				if (FunctionGuids.AllMatrixGuids.Contains(function.Id))
				{
					// This case is to avoid assigning resources to matrix functions in a satellite or audio processing service while the functions connected to those matrix functions are the same device.
					// e.g.: Sat RX service: |ABS-3| - |ABS-3 3W| - |ETL Main Input 48.ABS-3 3W| - |ETL Main Output 40.IRD 155| - |IRD 155.Demodulating| - |None| - |None| - |IRD 155.Decoding|

					var linkedMatrixFunction = service.Functions.SingleOrDefault(f => f.Id == FunctionGuids.MatrixFunctionPairs[function.Id] && service.Definition.FunctionsAreConnected(function, f));

					bool serviceContainsMoreThanJustTwoMatrixFunctions = service.Functions.Except(new[] { function, linkedMatrixFunction }).Any();

					if (linkedMatrixFunction != null && serviceContainsMoreThanJustTwoMatrixFunctions && service.MatrixInputAndOutputAreSameDevice(function, linkedMatrixFunction))
					{
						selectedMatchingResource = null;
					}
					else
					{
						selectedMatchingResource = GetFirstAlphabeticalHighestPriorityResource(selectableResources);
						FunctionAssignments[function.Definition.Label] = selectedMatchingResource != null;

						Log(nameof(SelectCurrentOrNewResource), $"No enforcing, selected new resource '{selectedMatchingResource?.Name}'", function.Definition.Label);
					}
				}
				else
				{
					selectedMatchingResource = GetFirstAlphabeticalHighestPriorityResource(selectableResources);
					FunctionAssignments[function.Definition.Label] = selectedMatchingResource != null;

					Log(nameof(SelectCurrentOrNewResource), $"No enforcing, selected new resource '{selectedMatchingResource?.Name}'", function.Definition.Label);
				}
			}
			else
			{
				selectedMatchingResource = function.Resource;
				Log(nameof(SelectCurrentOrNewResource), $"No enforcing, keeping resource '{selectedMatchingResource?.Name}'", function.Definition.Label);
			}

			return selectedMatchingResource;
		}

		private FunctionResource GetFirstAlphabeticalHighestPriorityResource(HashSet<FunctionResource> resources)
		{
			if (resources == null || !resources.Any()) return null;

			var automaticallySelectableResources = resources.Where(x => !x.IsManualResource()).ToList();
			if (!automaticallySelectableResources.Any())
			{
				Log(nameof(GetFirstAlphabeticalHighestPriorityResource), $"No resources available for automatic assignment from the list of available resources {String.Join(", ", resources.Select(x => x.Name))}");
				return null;
			}

			int highestPriority = automaticallySelectableResources.Select(r => r.GetPriority()).Min();

			var orderedHighestPriorityResources = automaticallySelectableResources.Where(r => r.GetPriority() == highestPriority).OrderBy(r => r.Name).ToList();

			Log(nameof(GetFirstAlphabeticalHighestPriorityResource), $"Alphabetically ordered highest priority (={highestPriority}) resources are {string.Join(", ", orderedHighestPriorityResources.Select(r => r.Name))}");

			return orderedHighestPriorityResources.FirstOrDefault();
		}

		/// <summary>
		/// Executes DTR logic to update selected resources.
		/// </summary>
		/// <returns>A collection of functions that have been handled by the DTR logic.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="FunctionNotFoundException"/>
		/// <exception cref="Exceptions.ProfileParameterNotFoundException"/>
		public IEnumerable<Function> ExecuteDtr(DisplayedFunction function)
		{
			if (function == null) throw new ArgumentNullException(nameof(function));

			LogMethodStart(nameof(ExecuteDtr), out var stopwatch, function.Name);

			var updatedFunctions = SetCapabilitiesOnRelatedFunctions(function).OrderBy(f => f.ConfigurationOrder).Cast<DisplayedFunction>().ToList();

			foreach (var updatedFunction in updatedFunctions)
			{
				updatedFunction.SelectableResources = GetSelectableResources(updatedFunction);
				updatedFunction.Resource = SelectCurrentOrNewResource(updatedFunction, updatedFunction.SelectableResources, out var resourceChanged);
			}

			foreach (var updatedFunction in updatedFunctions) ExecuteDtr(updatedFunction);

			handledFunctionLabels.AddRange(updatedFunctions.Select(f => f.Definition.Label));

			LogMethodCompleted(nameof(ExecuteDtr), function.Definition.Label, stopwatch);

			return updatedFunctions;
		}

		protected void AssignResourcesWithDtr()
		{
			LogMethodStart(nameof(AssignResourcesWithDtr), out var stopwatch, service.Name);

			var unhandledFunctions = service.Functions.Where(f => !handledFunctionLabels.Contains(f.Definition.Label)).OrderBy(f => f.ConfigurationOrder).Cast<DisplayedFunction>().ToList();

			while (unhandledFunctions.Any())
			{
				var functionToHandle = unhandledFunctions[0];
				handledFunctionLabels.Add(functionToHandle.Definition.Label);
				Log(nameof(AssignResourcesWithDtr), $"Handling function {functionToHandle.Definition.Label} ...", service.Name);

				if (functionToHandle.RequiresResource)
				{
					functionToHandle.SelectableResources = GetSelectableResources(functionToHandle);

					functionToHandle.Resource = SelectCurrentOrNewResource(functionToHandle, functionToHandle.SelectableResources, out bool resourceChanged);

					ExecuteDtr(functionToHandle);
				}
				else
				{
					Log(nameof(AssignResourcesWithDtr), $"Function {functionToHandle.Definition.Label} does not require a resource");
					functionToHandle.Resource = null;
				}

				unhandledFunctions = service.Functions.Where(f => !handledFunctionLabels.Contains(f.Definition.Label)).OrderBy(f => f.ConfigurationOrder).Cast<DisplayedFunction>().ToList();

				Log(nameof(AssignResourcesWithDtr), $"Handling function {functionToHandle.Name} completed", service.Name);
			}

			Log(nameof(AssignResourcesWithDtr), $"Resource assignment result: {string.Join(";", service.Functions.Select(f => $"{f.Definition.Label}={f.ResourceName}"))}", service.Name);

			LogMethodCompleted(nameof(AssignResourcesWithDtr), service.Name, stopwatch);
		}

		protected Function GetFirstFunctionToConfigure()
		{
			var orderedFunctions = service.Functions.OrderBy(f => f.ConfigurationOrder).ToList();
			Function firstFunctionToConfigure;
			int index = 0;
			do
			{
				firstFunctionToConfigure = orderedFunctions[index++];
			}
			while (index < orderedFunctions.Count && firstFunctionToConfigure != null && (!firstFunctionToConfigure.RequiresResource || firstFunctionToConfigure.IsDummy));

			helpers.Log(nameof(ResourceAssignmentHandler), nameof(GetFirstFunctionToConfigure), $"Found {(firstFunctionToConfigure == null ? "no" : firstFunctionToConfigure.Name)} function with lowest ConfigurationOrder {(firstFunctionToConfigure == null ? string.Empty : firstFunctionToConfigure.ConfigurationOrder.ToString())} between functions {string.Join(";", service.Functions.Select(f => $"{f.Name} (config order {f.ConfigurationOrder})"))}");

			if (firstFunctionToConfigure == null)
			{
				return null;
			}

			return firstFunctionToConfigure;
		}

		/// <summary>
		/// Removes resources that are selected by other services within order but are not booked yet.
		/// </summary>
		/// <example>When adding an order with 2 destinations of the same type, the second destination should not be able to select the (unbooked) resource selected by the first destination.</example>
		protected HashSet<FunctionResource> RemoveResourcesOccupiedByOtherServicesWithinOrder(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			if (function == null) throw new ArgumentNullException(nameof(function));
			if (resourcesToFilter == null) throw new ArgumentNullException(nameof(resourcesToFilter));

			var filteredResources = new HashSet<FunctionResource>();
			var removedResources = new HashSet<FunctionResource>();

			var alreadySelectedResourcesByOtherOverlappingServicesInOrder = order.GetAllServiceResourcesOverlappingWith(service).ToList();

			Log(nameof(RemoveResourcesOccupiedByOtherServicesWithinOrder), $"Resources selected by other services with overlapping timing within order: {string.Join(",", alreadySelectedResourcesByOtherOverlappingServicesInOrder.Select(r => r.Name))}", function.Name);

			foreach (var resource in resourcesToFilter)
			{
				bool resourceIsAlreadySelectedByOtherServiceWithinOrder = alreadySelectedResourcesByOtherOverlappingServicesInOrder.Contains(resource);
				bool resourceHasBigConcurrency = resource.MaxConcurrency > 1;
				if (!resourceIsAlreadySelectedByOtherServiceWithinOrder || resourceHasBigConcurrency)
				{
					filteredResources.Add(resource);
				}
				else
				{
					removedResources.Add(resource);
				}
			}

			Log(nameof(RemoveResourcesOccupiedByOtherServicesWithinOrder), $"Removed following resources selected by other services within order while filtering: {string.Join(",", removedResources.Select(r => r.Name))}", function.Name);

			Log(nameof(RemoveResourcesOccupiedByOtherServicesWithinOrder), $"Filtered resources: {string.Join(",", filteredResources.Select(r => r.Name))}", function.Name);

			return filteredResources;
		}

		protected HashSet<FunctionResource> RemoveResourcesWithoutSelectableRoutingResource(Function function, HashSet<FunctionResource> resourcesToFilter)
		{
			ArgumentNullCheck.ThrowIfNull(function, nameof(function));
			ArgumentNullCheck.ThrowIfNull(resourcesToFilter, nameof(resourcesToFilter));

			LogMethodStart(nameof(RemoveResourcesWithoutSelectableRoutingResource), out var stopwatch, function.Name);

			var routingChildService = service.Children.FirstOrDefault(c => c.Definition.VirtualPlatform == VirtualPlatform.Routing);
			bool serviceHasRoutingChild = routingChildService != null;
			bool functionIsLastFunctionThatRequiresResource = service.FunctionIsLastResourceRequiringFunctionInDefinition(helpers, function);
			var parentRoutingService = order.AllServices.SingleOrDefault(s => s.Children.Contains(service) && s.Definition.VirtualPlatform == VirtualPlatform.Routing);
			bool serviceHasRoutingParent = parentRoutingService != null;
			bool functionIsFirstFunctionThatRequiresResource = service.FunctionIsFirstResourceRequiringFunctionInDefinition(helpers, function);

			Service connectedRoutingService;
			Function connectedRoutingFunction;
			Guid resourceConnectionsProfileParameterId;
			if (functionIsLastFunctionThatRequiresResource && serviceHasRoutingChild)
			{
				connectedRoutingService = routingChildService;
				connectedRoutingFunction = routingChildService.Functions.Single(f => routingChildService.Definition.FunctionIsFirst(f));
				resourceConnectionsProfileParameterId = ProfileParameterGuids.ResourceInputConnectionsSdi;

				Log(nameof(RemoveResourcesWithoutSelectableRoutingResource), $"Function is last resource-requiring function within service and is connected to a function of child routing service {connectedRoutingService.Name}, filtering based on resource availability of that function required.", function.Name);
			}
			else if (functionIsFirstFunctionThatRequiresResource && serviceHasRoutingParent)
			{
				connectedRoutingService = parentRoutingService;
				connectedRoutingFunction = parentRoutingService.Functions.Single(f => parentRoutingService.Definition.FunctionIsLast(f));
				resourceConnectionsProfileParameterId = ProfileParameterGuids.ResourceOutputConnectionsSdi;

				Log(nameof(RemoveResourcesWithoutSelectableRoutingResource), $"Function is first resource-requiring function within service and is connected to a function of parent routing service {connectedRoutingService.Name}, filtering based on resource availability of that function required.", function.Name);
			}
			else
			{
				Log(nameof(RemoveResourcesWithoutSelectableRoutingResource), $"Function is not connected to a neighbor routing function, no filtering required.", function.Name);
				return resourcesToFilter;
			}

			var routingFunctionTimeRanges = connectedRoutingService.Functions.ToDictionary(f => f.Definition.Label, f => functionTimeRangesForResourceRequest[function.Definition.Label]);

			var availableConnectedRoutingResources = connectedRoutingService.GetAvailableResourcesPerFunctionBasedOnTiming(helpers, routingFunctionTimeRanges)[connectedRoutingFunction.Definition.Label];
			// Warning: connected routing service timings might not yet be in sync with the current service. Therefore we overwrite the relevant function time range to make sure we're working with the same values. Also take the start now use case into account.

			availableConnectedRoutingResources.UnionWith(GetOccupiedTieLineResourcesWithinOrder(connectedRoutingFunction.Id));

			var filteredResources = new HashSet<FunctionResource>();
			var removedResources = new HashSet<FunctionResource>();
			var resourcesWithoutConnectionsCapability = new HashSet<FunctionResource>();

			foreach (var resource in resourcesToFilter)
			{
				bool resourceHasAvailableConnectedRoutingResource = false;

				if (FunctionRemainsFirstOrLastResourceRequiringFunctionAfterResourceWouldBeSelected(function, functionIsLastFunctionThatRequiresResource, functionIsFirstFunctionThatRequiresResource, resource))
				{
					resourceHasAvailableConnectedRoutingResource = TryFindAvailableConnectedRoutingResource(resourceConnectionsProfileParameterId, availableConnectedRoutingResources, resourcesWithoutConnectionsCapability, resource);

					if (resourceHasAvailableConnectedRoutingResource)
					{
						filteredResources.Add(resource);
					}
					else
					{
						removedResources.Add(resource);
					}
				}
				else
				{
					// no filtering required

					filteredResources.Add(resource);
				}
			}

			Log(nameof(RemoveResourcesWithoutSelectableRoutingResource), $"Unable to find connections capability on routing resources: '{string.Join(", ", resourcesWithoutConnectionsCapability.Select(r => r.Name))}'", function.Name);

			Log(nameof(RemoveResourcesWithoutSelectableRoutingResource), $"Removed following resources with unavailable connected routing resource while filtering: '{string.Join(", ", removedResources.Select(r => r.Name))}'", function.Name);

			Log(nameof(RemoveResourcesWithoutSelectableRoutingResource), $"Filtered resources: '{string.Join(", ", filteredResources.Select(r => r.Name))}'", function.Name);

			LogMethodCompleted(nameof(RemoveResourcesWithoutSelectableRoutingResource), function.Name, stopwatch);

			return filteredResources;
		}

		private static bool TryFindAvailableConnectedRoutingResource(Guid resourceConnectionsProfileParameterId, HashSet<FunctionResource> availableConnectedRoutingResources, HashSet<FunctionResource> resourcesWithoutConnectionsCapability, FunctionResource resource)
		{
			foreach (var routingResource in availableConnectedRoutingResources)
			{
				var connectionsCapability = routingResource.Capabilities.SingleOrDefault(c => c.CapabilityProfileID == resourceConnectionsProfileParameterId);
				if (connectionsCapability == null)
				{
					resourcesWithoutConnectionsCapability.Add(routingResource);
					continue;
				}

				bool resourceAndRoutingResourceAreConnected = connectionsCapability.Value.Discreets.Contains(resource.Name);
				if (resourceAndRoutingResourceAreConnected)
				{
					return true;
				}
			}

			return false;
		}

		private bool FunctionRemainsFirstOrLastResourceRequiringFunctionAfterResourceWouldBeSelected(Function function, bool functionIsLastFunctionThatRequiresResource, bool functionIsFirstFunctionThatRequiresResource, FunctionResource resource)
		{
			var resourceUpdateInfo = new ResourceUpdateInfo
			{
				ServiceDefinitionName = service.Definition.Name,
				UpdatedResourceFunctionLabel = function.Definition.Label,
				UpdatedResource = resource,
			};

			var dummyFilter = ResourceUpdateHandler.GetResourceCapabilityFilters(resourceUpdateInfo, service).SingleOrDefault(rcf => rcf.CapabilityParameterName == "_Dummy");
			if (dummyFilter is null) return true;

			var functionToApplyFilterOn = service.Functions.SingleOrDefault(f => f.Definition.Label == dummyFilter.FunctionLabel);
			if (functionToApplyFilterOn is null) throw new FunctionNotFoundException(dummyFilter.FunctionLabel);

			bool dummyParameterWouldChangeValue = functionToApplyFilterOn.IsDummy.ToString() != dummyFilter.CapabilityParameterValue;

			bool lastResourceRequiringFunctionWouldChange = functionIsLastFunctionThatRequiresResource && service.Definition.GetFunctionPosition(function) < service.Definition.GetFunctionPosition(functionToApplyFilterOn);
			bool firstResourceRequiringFunctionWouldChange = functionIsFirstFunctionThatRequiresResource && service.Definition.GetFunctionPosition(function) > service.Definition.GetFunctionPosition(functionToApplyFilterOn);

			bool currentFunctionIsStillTheFirstOrLastResourceRequiringFunctionAfterThisResourceWouldBeSelected = !dummyParameterWouldChangeValue || (!lastResourceRequiringFunctionWouldChange && !firstResourceRequiringFunctionWouldChange);

			return currentFunctionIsStillTheFirstOrLastResourceRequiringFunctionAfterThisResourceWouldBeSelected;
		}

		/// <summary>
		/// Gets the resources for the given function ID that are being used in tie lines within the order for the same source.
		/// </summary>
		/// <param name="routingFunctionId">The ID of the function for which to find occupied tie line resources. Should be either <see cref="FunctionGuids.MatrixInputSdi"/> or <see cref="FunctionGuids.MatrixOutputSdi"/></param>
		/// <returns></returns>
		private List<FunctionResource> GetOccupiedTieLineResourcesWithinOrder(Guid routingFunctionId)
		{
			// This method is used to make sure a UMX destination resource is selectable in UpdateService while its connected routing resource is already used by a tie line within the same order.
			// This UMX destination resource should be selectable because part of the tie line can and should be reused.

			List<FunctionResource> tieLineResourcesWithinOrder;

			LogMethodStart(nameof(GetOccupiedTieLineResourcesWithinOrder), out var stopwatch);

			var liveVideoOrder = new LiveVideoOrder(helpers, order);

			var source = liveVideoOrder.GetSource(service);

			var routingServiceChainsUsingTieLine = liveVideoOrder.GetRoutingServiceChainsConnectedToSameSourceAs(source).Where(rcs => rcs.InputRoutingService != null).ToList();

			if (routingFunctionId == FunctionGuids.MatrixInputSdi)
			{
				var outputRoutingServicesMatrixInputSdiResources = routingServiceChainsUsingTieLine.Select(rcs => rcs.OutputRoutingService.MatrixInputSdi).Where(resource => resource != null).ToList();

				tieLineResourcesWithinOrder = outputRoutingServicesMatrixInputSdiResources;

				Log(nameof(GetOccupiedTieLineResourcesWithinOrder), $"Resources '{string.Join(", ", outputRoutingServicesMatrixInputSdiResources.Select(r => r.Name))}' are matrix input resources for output routing services (part of tie line) within the order and are considered to be available.");
			}
			else if (routingFunctionId == FunctionGuids.MatrixOutputSdi)
			{
				var inputRoutingServicesMatrixOutputSdiResources = routingServiceChainsUsingTieLine.Select(rcs => rcs.InputRoutingService.MatrixOutputSdi).Where(resource => resource != null).ToList();

				tieLineResourcesWithinOrder = inputRoutingServicesMatrixOutputSdiResources;

				Log(nameof(GetOccupiedTieLineResourcesWithinOrder), $"Resources '{string.Join(", ", inputRoutingServicesMatrixOutputSdiResources.Select(r => r.Name))}' are matrix output resources for input routing services (part of tie line) within the order and are considered to be available.");
			}
			else
			{
				throw new ArgumentException($"ID {routingFunctionId} is not supported. It should be either {FunctionGuids.MatrixInputSdi} (Matrix Input SDI) or {FunctionGuids.MatrixOutputSdi} (Matrix Output SDI)", nameof(routingFunctionId));
			}

			LogMethodCompleted(nameof(GetOccupiedTieLineResourcesWithinOrder), null, stopwatch);

			return tieLineResourcesWithinOrder;
		}

		private IEnumerable<ResourceCapabilityFilter> GetResourceCapabilityFilters(Function functionTriggeringDtr)
		{
			var resourceUpdateInfo = new ResourceUpdateInfo { ServiceDefinitionName = service.Definition.Name, UpdatedResource = (Resource)functionTriggeringDtr.Resource, UpdatedResourceFunctionLabel = functionTriggeringDtr.Definition.Label, ConnectedResource = functionTriggeringDtr.Id == FunctionGuids.Decoding ? service.Functions.FirstOrDefault(f => f.Id == FunctionGuids.Demodulating)?.Resource : null };

			var resourceCapabilityFilters = ResourceUpdateHandler.GetResourceCapabilityFilters(resourceUpdateInfo, service);
			return resourceCapabilityFilters;
		}

		/// <summary>
		/// Updates the profile parameter capability for this function in case the value has changed.
		/// </summary>
		/// <param name="functionToUpdate">The function to update.</param>
		/// <param name="resourceCapabilityFilter">The capability filter.</param>
		/// <returns>A boolean indicating if the value was updated or not.</returns>
		private bool TrySetCapabilityOnRelatedFunction(Function functionToUpdate, ResourceCapabilityFilter resourceCapabilityFilter)
		{
			var functionToUpdateAllProfileParameters = functionToUpdate.Parameters.Concat(functionToUpdate.InterfaceParameters).ToList();

			var profileParameterToUpdate = functionToUpdateAllProfileParameters.FirstOrDefault(p => p.Name == resourceCapabilityFilter.CapabilityParameterName);
			if (profileParameterToUpdate == null) throw new ProfileParameterNotFoundException(resourceCapabilityFilter.CapabilityParameterName, functionToUpdate.Name, functionToUpdateAllProfileParameters);

			if (profileParameterToUpdate.Value == null || !profileParameterToUpdate.Value.Equals(resourceCapabilityFilter.CapabilityParameterValue))
			{
				profileParameterToUpdate.Value = resourceCapabilityFilter.CapabilityParameterValue;
				Log(nameof(TrySetCapabilityOnRelatedFunction), $"Setting capability {profileParameterToUpdate.Name} on function {functionToUpdate.Definition.Label} to '{profileParameterToUpdate.StringValue}'");

				return true;
			}

			Log(nameof(TrySetCapabilityOnRelatedFunction), $"Capability {profileParameterToUpdate.Name} on function {functionToUpdate.Definition.Label} not changed, value is already '{profileParameterToUpdate.StringValue}'");
			return false;
		}

		private List<Function> SetCapabilitiesOnRelatedFunctions(Function functionTriggeringDtr)
		{
			if (functionTriggeringDtr == null) throw new ArgumentNullException(nameof(functionTriggeringDtr));

			var updatedFunctions = new List<Function>();

			var resourceCapabilityFilters = GetResourceCapabilityFilters(functionTriggeringDtr).ToList();

			foreach (var resourceCapabilityFilter in resourceCapabilityFilters)
			{
				var functionToUpdate = service.Functions.SingleOrDefault(f => f.Definition.Label == resourceCapabilityFilter.FunctionLabel);

				if (TrySetCapabilityOnRelatedFunction(functionToUpdate, resourceCapabilityFilter)) updatedFunctions.Add(functionToUpdate);
			}

			return updatedFunctions;
		}

		protected void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers.Log(GetType().Name, nameOfMethod, message, nameOfObject);
		}

		protected void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch, string nameOfObject = null)
		{
			Log(nameOfMethod, "Start", nameOfObject);

			stopwatch = Stopwatch.StartNew();
		}

		protected void LogMethodCompleted(string nameOfMethod, string nameOfObject = null, Stopwatch stopwatch = null)
		{
			Log(nameOfMethod, $"Completed [{stopwatch?.Elapsed}]", nameOfObject);
		}
	}
}
