namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using ResourcePoolNotFoundException = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ResourcePoolNotFoundException;
	using ServiceNotFoundException = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceNotFoundException;
	using VirtualPlatform = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.VirtualPlatform;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using System.Text;
	using Skyline.DataMiner.Net.Time;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public class RoutingServiceChain
	{
		private readonly Helpers helpers;
		private readonly LiveVideoOrder liveVideoOrder;

		private HashSet<FunctionResource> availableMatrixInputSdiResources;
		private HashSet<FunctionResource> availableMatrixOutputSdiResources;

		public RoutingServiceChain(Helpers helpers, LiveVideoService inputService, RoutingService inputRoutingService, RoutingService connectingRoutingService, RoutingService outputRoutingService, RoutingRequiringService outputService, LiveVideoOrder liveVideoOrder)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.liveVideoOrder = liveVideoOrder ?? throw new ArgumentNullException(nameof(liveVideoOrder));

			InputService = inputService;
			OutputService = outputService;

			InputRoutingService = inputRoutingService;
			ConnectingRoutingService = connectingRoutingService;
			OutputRoutingService = outputRoutingService;

			RoutingResourceChain = new RoutingResourceChain
			{
				IsValid = true,
				FirstMatrixRequired = inputRoutingService?.MatrixInputSdi != null || inputRoutingService?.MatrixOutputSdi != null,
				FirstMatrix = new Matrix
				{
					Input = inputRoutingService?.MatrixInputSdi,
					Output = inputRoutingService?.MatrixOutputSdi,
				},
				ConnectingMatrixRequired = connectingRoutingService?.MatrixInputSdi != null || connectingRoutingService?.MatrixOutputSdi != null,
				ConnectingMatrix = new Matrix
				{
					Input = connectingRoutingService?.MatrixInputSdi,
					Output = connectingRoutingService?.MatrixOutputSdi,
				},
				LastMatrixRequired = outputRoutingService?.MatrixInputSdi != null || outputRoutingService?.MatrixOutputSdi != null,
				LastMatrix = new Matrix
				{
					Input = outputRoutingService?.MatrixInputSdi,
					Output = outputRoutingService?.MatrixOutputSdi,
				},
			};

			InitializeValidity();
		}

		public RoutingServiceChain(Helpers helpers, LiveVideoService inputService, RoutingRequiringService outputService, LiveVideoOrder liveVideoOrder) : this(helpers, inputService, null, null, null, outputService, liveVideoOrder)
		{
		}

		/// <summary>
		/// Indicates if this Routing Service Chain is valid and can be reused.
		/// </summary>
		public bool IsValid { get; private set; }

		public LiveVideoService InputService { get; private set; }

		public RoutingService SharedRoutingService { get; private set; }

		private FunctionResource Input { get; set; }

		public RoutingRequiringService OutputService { get; private set; }

		public FunctionResource Output { get; private set; }

		public RoutingService InputRoutingService { get; private set; }

		public RoutingService ConnectingRoutingService { get; private set; }

		public RoutingService OutputRoutingService { get; private set; }

		public List<LiveVideoService> AllServices
		{
			get
			{
				var allServices = new List<LiveVideoService>();

				if (InputService != null) allServices.Add(InputService);
				if (InputRoutingService != null) allServices.Add(InputRoutingService);
				if (ConnectingRoutingService != null) allServices.Add(ConnectingRoutingService);
				if (OutputRoutingService != null) allServices.Add(OutputRoutingService);
				if (OutputService != null) allServices.Add(OutputService);

				return allServices;
			}
		}

		public List<FunctionResource> GetAllServiceResources()
		{
			return AllServices.SelectMany(s => s.Service.Functions).Where(f => f.Resource != null).Select(f => f.Resource).ToList();
		}

		public List<RoutingService> AllRoutingServices
		{
			get
			{
				var allRoutingServices = new List<RoutingService>();

				if (InputRoutingService != null) allRoutingServices.Add(InputRoutingService);
				if (ConnectingRoutingService != null) allRoutingServices.Add(ConnectingRoutingService);
				if (OutputRoutingService != null) allRoutingServices.Add(OutputRoutingService);

				return allRoutingServices;
			}
		}

		public bool RoutingResourcesAreEnforced => AllRoutingServices.SelectMany(s => s.Service.Functions).Any(f => f.EnforceSelectedResource);

		public List<FunctionResource> GetAllRoutingServiceResources()
		{
			return AllRoutingServices.SelectMany(s => s.Service.Functions).Where(f => f.Resource != null).Select(f => f.Resource).ToList();
		}

		public bool UsesMultipleRoutingServices => AllRoutingServices.Count > 1;

		public bool UsesConnectingRoutingService => ConnectingRoutingService != null;

		public RoutingResourceChain RoutingResourceChain { get; private set; }

		public List<RoutingResourceChain> SelectableRoutingResourceChains { get; } = new List<RoutingResourceChain>();

		public void InitializeSelectableRoutingResourceChains()
		{
			LogMethodStart(nameof(InitializeSelectableRoutingResourceChains), out var stopwatch, OutputService.Service.Name);

			SelectableRoutingResourceChains.Clear();

			GetAvailableMatrixSdiInputsAndOutputs();

			var selectableInputResourcesForTheChain = GetSelectableInputResources(availableMatrixInputSdiResources);
			var selectableOutputResourcesForTheChain = GetSelectableOutputResources(availableMatrixOutputSdiResources);

			foreach (var input in selectableInputResourcesForTheChain)
			{
				Input = input;

				bool usingDummyInput = false;

				if (Input is null)
				{
					// To make sure all routing services are created as we would expect when input service and output service resources are assigned.
					// To avoid having to add routing services during running order.
					// Implemented as part of DCP192579.

					usingDummyInput = true;
					Input = GetDummyInputRoutingInputResource();
				}

				CombineInputWithAllOutputs(selectableOutputResourcesForTheChain, usingDummyInput);
			}

			Log(nameof(InitializeSelectableRoutingResourceChains), $"Selectable Routing Resource Chains between {InputService.Service.Name} and {OutputService.Service.Name}:" + (SelectableRoutingResourceChains.Any() ? "\n" : String.Empty) + string.Join("\n", SelectableRoutingResourceChains.Select(rcs => rcs.ToString())));

			IsValid = SelectableRoutingResourceChains.Contains(RoutingResourceChain);

			LogMethodCompleted(nameof(InitializeSelectableRoutingResourceChains), stopwatch);
		}

		private void CombineInputWithAllOutputs(HashSet<FunctionResource> selectableOutputResourcesForTheChain, bool usingDummyInput)
		{
			foreach (var output in selectableOutputResourcesForTheChain)
			{
				Output = output;

				bool isSpecialMessiNewsRecordingCase = OutputService.Service.Definition.Id == ServiceDefinitionGuids.RecordingMessiNews && Output == null;
				if (isSpecialMessiNewsRecordingCase)
				{
					// Special use case: tie line resources should be assigned even though the messi news recording resource is null, therefore we select a dummy recording resource to allow the generate routing flow to do its work. We remove this resource from the service again at the end of this method. 
					SelectDummyResourceForMessiNewsRecordingWithoutResource();
				}

				var chainsForThisInputOutputCombination = new List<RoutingResourceChain>();

				var onlyOneRoutingServiceRequired = Input == null || Output == null || Input.IsResourceFromSameElementAs(Output);
				if (onlyOneRoutingServiceRequired)
				{
					var routingResourceChain = new RoutingResourceChain
					{
						IsValid = true,
						LastMatrixRequired = true,
						LastMatrix = new Matrix
						{
							Input = Input,
							Output = Output,
						},
					};

					chainsForThisInputOutputCombination.Add(routingResourceChain);

					Log(nameof(CombineInputWithAllOutputs), $"Resources '{Input?.Name}' and '{Output?.Name}' are part of the same matrix, considered routing resource chain {routingResourceChain} as selectable");
				}
				else
				{
					Log(nameof(CombineInputWithAllOutputs), $"Resources '{Input?.Name}' and '{Output?.Name}' are NOT from the same matrix.");

					// the input and output are not from the same matrix
					// multiple routing services will be needed in this case 
					// this could be 2 matrices in case there are tie lines between the input and output matrix or there could be an additional hop needed to connect both matrices to one another

					var availableTieLines = GetAvailableRoutingResourceChains();

					// if there are no available tie lines we will create a new (incomplete) one with only the input and output
					// we also don't need to check if similar tie lines can be reused as they should not be available as well
					if (!availableTieLines.Any())
					{
						var invalidRoutingResourceChain = new RoutingResourceChain
						{
							IsValid = false,
							FirstMatrixRequired = true,
							FirstMatrix = new Matrix
							{
								Input = Input,
							},
							LastMatrixRequired = true,
							LastMatrix = new Matrix
							{
								Output = Output,
							}
						};

						chainsForThisInputOutputCombination.Add(invalidRoutingResourceChain);

						Log(nameof(CombineInputWithAllOutputs), $"Found no available tie line between '{Input?.Name}' and '{Output?.Name}', created invalid routing resource chain {invalidRoutingResourceChain}");
					}
					else
					{
						chainsForThisInputOutputCombination.AddRange(availableTieLines);
					}
				}

				ResetDummiesToNull(chainsForThisInputOutputCombination, usingDummyInput, isSpecialMessiNewsRecordingCase);

				SelectableRoutingResourceChains.AddRange(chainsForThisInputOutputCombination);
			}
		}

		private void ResetDummiesToNull(List<RoutingResourceChain> chainsForThisInputOutputCombination, bool usingDummyInput, bool isSpecialMessiNewsRecordingCase)
		{
			if (isSpecialMessiNewsRecordingCase)
			{
				foreach (var chain in chainsForThisInputOutputCombination)
				{
					chain.LastMatrix.Output = null;
				}

				Log(nameof(ResetDummiesToNull), $"Output service is Messi News Recording with resource None. Routing generation complete. Set Output routing service matrix output SDI resource to None to match recording service resource.");
			}

			if (usingDummyInput)
			{
				foreach (var chain in chainsForThisInputOutputCombination)
				{
					if (chain.FirstMatrixRequired)
					{
						chain.FirstMatrix.Input = null;
					}
					else
					{
						chain.LastMatrix.Input = null;
					}
				}
			}
		}

		private FunctionResource GetDummyInputRoutingInputResource()
		{
			var lastFunction = InputService.Service.GetLastResourceRequiringFunction(helpers);

			var dummyInputServiceOutputResource = helpers.GetResourceAssignmentHandler(InputService.Service, liveVideoOrder.Order).GetSelectableResources(lastFunction, ResourceAssignment.FilterOptions.None, false).FirstOrDefault() ?? throw new ResourceNotFoundException($"Unable to find a dummy resource for input service {InputService.Service.Name}");

			var matrixInputSdiResourcePool = helpers.ResourceManager.GetResourcePoolByName(helpers.ServiceDefinitionManager.RoutingServiceDefinition.FunctionDefinitions.Single(f => f.Id == FunctionGuids.MatrixInputSdi).ResourcePool);

			var allInputRoutingResources = helpers.ResourceManager.GetResources(ResourceExposers.PoolGUIDs.Contains(matrixInputSdiResourcePool.ID)).Cast<FunctionResource>().ToList();

			var unavailableInputResourcesForTheChain = helpers.ResourceManager.GetConnectedResources(dummyInputServiceOutputResource, allInputRoutingResources, ProfileParameterGuids.ResourceInputConnectionsSdi);

			var dummyInputRoutingServiceInputResource = unavailableInputResourcesForTheChain.FirstOrDefault() ?? throw new ResourceNotFoundException("Unable to find a dummy input resource for the input routing service");

			Log(nameof(GetDummyInputRoutingInputResource), $"Input service {InputService.Service.Name} has output resource none. Using unavailable resource {dummyInputRoutingServiceInputResource.Name} as dummy instead.");

			return dummyInputRoutingServiceInputResource;
		}

		private void GetAvailableMatrixSdiInputsAndOutputs()
		{
			LogMethodStart(nameof(GetAvailableMatrixSdiInputsAndOutputs), out var stopwatch);

			var matrixInputSdiContext = GetMatrixEligibleResourceContext(FunctionGuids.MatrixInputSdi);

			var matrixOutputSdiContext = GetMatrixEligibleResourceContext(FunctionGuids.MatrixOutputSdi);

			var availableResources = helpers.ResourceManager.GetAvailableResources(new List<YleEligibleResourceContext> { matrixInputSdiContext, matrixOutputSdiContext }, true);

			availableMatrixInputSdiResources = availableResources[matrixInputSdiContext.FunctionDefinitionLabel];
			availableMatrixOutputSdiResources = availableResources[matrixOutputSdiContext.FunctionDefinitionLabel];

			// Consider resources from other chains with same source to be available so they can be shared
			// WARNING: Check if the services in this order are the only occupying services for the resources they are using

			var additionalAvailableMatrixOutputSdiResources = GetAvailableMatrixOutputSdiResourcesUsedInThisOrder(out var occupiedMatrixSdiOutputResources);

			SharedRoutingService = GetSharedRoutingService();
			if (SharedRoutingService != null)
			{
				Log(nameof(GetAvailableMatrixSdiInputsAndOutputs), $"Found possible Shared Routing Service {SharedRoutingService.Service.Name} using Input Resource: {SharedRoutingService.MatrixInputSdi.Name} and Ouptut Resource: {SharedRoutingService.MatrixOutputSdi.Name}");

				availableMatrixInputSdiResources.Add(SharedRoutingService.MatrixInputSdi);
				availableMatrixOutputSdiResources.Add(SharedRoutingService.MatrixOutputSdi);
			}

			/*
			Log(nameof(GetAvailableMatrixSdiInputsAndOutputs), $"Considering resources available from all routing service chains with input service {InputService?.Service?.Name}: '{string.Join(";", routingServiceChainsForSameInputService)}'");

			var otherAvailableMatrixInputSdiResources = routingServiceChainsForSameInputService.SelectMany(rsc => rsc.AllRoutingServices).Select(rs => rs.MatrixInputSdi).Where(resource => resource != null).Distinct().ToList();
			var otherAvailableMatrixOutputSdiResources = routingServiceChainsForSameInputService.SelectMany(rsc => rsc.AllRoutingServices).Select(rs => rs.MatrixOutputSdi).Where(resource => resource != null).Distinct().ToList();

			availableMatrixInputSdiResources.UnionWith(otherAvailableMatrixInputSdiResources);
			*/
			availableMatrixOutputSdiResources.UnionWith(additionalAvailableMatrixOutputSdiResources);

			var routingServiceChainsForOtherInputService = liveVideoOrder.GetRoutingServiceChainsConnectedToOtherSourceThan(OutputService);

			Log(nameof(GetAvailableMatrixSdiInputsAndOutputs), $"Considering resources unavailable from all routing service chains connected to other source: '{string.Join(";", routingServiceChainsForOtherInputService)}'");

			var otherUnavailableMatrixInputSdiResources = routingServiceChainsForOtherInputService.SelectMany(rsc => rsc.AllRoutingServices).Select(rs => rs.MatrixInputSdi).Where(resource => resource != null && resource.MaxConcurrency == 1).ToList();
			var otherUnavailableMatrixOutputSdiResources = routingServiceChainsForOtherInputService.SelectMany(rsc => rsc.AllRoutingServices).Select(rs => rs.MatrixOutputSdi).Where(resource => resource != null && resource.MaxConcurrency == 1).ToList();

			availableMatrixInputSdiResources.ExceptWith(otherUnavailableMatrixInputSdiResources);
			availableMatrixOutputSdiResources.ExceptWith(otherUnavailableMatrixOutputSdiResources);

			Log(nameof(GetAvailableMatrixSdiInputsAndOutputs), $"All available matrix input SDI resources: '{string.Join(";", availableMatrixInputSdiResources.Select(r => r.Name))}'");
			Log(nameof(GetAvailableMatrixSdiInputsAndOutputs), $"All available matrix output SDI resources: '{string.Join(";", availableMatrixOutputSdiResources.Select(r => r.Name))}'");

			LogMethodCompleted(nameof(GetAvailableMatrixSdiInputsAndOutputs), stopwatch);
		}

		private HashSet<FunctionResource> GetAvailableMatrixOutputSdiResourcesUsedInThisOrder(out HashSet<FunctionResource> occupiedMatrixSdiOutputResources)
		{
			occupiedMatrixSdiOutputResources = new HashSet<FunctionResource>();

			var routingServiceChainsForSameInputService = liveVideoOrder.GetRoutingServiceChainsWithSameInputServiceAs(InputService.Service.Id);

			var additionalAvailableMatrixOutputSdiResources = new HashSet<FunctionResource>();
			foreach (var routingServiceChain in routingServiceChainsForSameInputService)
			{
				foreach (var routingService in routingServiceChain.AllRoutingServices)
				{
					// Matrix Input SDI resource availibility should not be checked because of the high concurrency

					if (routingService.MatrixOutputSdi != null)
					{
						var occupyingServices = helpers.ResourceManager.GetOccupyingServices(routingService.MatrixOutputSdi, OutputService.Service.StartWithPreRoll, OutputService.Service.EndWithPostRoll, liveVideoOrder.Order.Id, routingService.Service.Name);

						Log(nameof(GetAvailableMatrixOutputSdiResourcesUsedInThisOrder), $"Routing resource {routingService.MatrixOutputSdi.Name} from chain {routingServiceChain} has occupying services over {OutputService.Service.StartWithPreRoll.ToFullDetailString()} until {OutputService.Service.EndWithPostRoll.ToFullDetailString()}: '{string.Join(", ", occupyingServices.Select(os => os.ToString()))}'");

						if (!occupyingServices.Any())
						{
							// if there are no occupying services for the matrix output SDI resource, consider it as available to enable routing service sharing
							additionalAvailableMatrixOutputSdiResources.Add(routingService.MatrixOutputSdi);
						}
						else
						{
							occupiedMatrixSdiOutputResources.Add(routingService.MatrixOutputSdi);
						}
					}
				}
			}

			Log(nameof(GetAvailableMatrixOutputSdiResourcesUsedInThisOrder), $"All non-available matrix output SDI resources from other chains: '{string.Join(", ", occupiedMatrixSdiOutputResources.Select(r => r.Name))}'");

			Log(nameof(GetAvailableMatrixOutputSdiResourcesUsedInThisOrder), $"All additional available matrix output SDI resources from other chains: '{string.Join(", ", additionalAvailableMatrixOutputSdiResources.Select(r => r.Name))}'");

			return additionalAvailableMatrixOutputSdiResources;
		}

		private RoutingService GetSharedRoutingService()
		{
			LogMethodStart(nameof(GetSharedRoutingService), out var stopwatch);

			// Check if shared source input
			if (!InputService.Service.IsSharedSource)
			{
				Log(nameof(GetSharedRoutingService), $"No need to search for shared routing service, source service is not a shared source.");
				LogMethodCompleted(nameof(GetSharedRoutingService), stopwatch);
				return null;
			}

			// Check if shared source requires fixed tie line
			var tieLineProperty = InputService.OutputResource.Properties.FirstOrDefault(p => p.Name.Equals(ResourcePropertyNames.RequiresSpecificTieLine));
			if (tieLineProperty == null)
			{
				Log(nameof(GetSharedRoutingService), $"No need to search for shared routing service, output resource of shared source does not have the tie line property.");
				LogMethodCompleted(nameof(GetSharedRoutingService), stopwatch);
				return null;
			}

			if (!Boolean.TryParse(tieLineProperty.Value, out bool requiresTieLine) || !requiresTieLine)
			{
				Log(nameof(GetSharedRoutingService), $"No need to search for shared routing service, output resource of shared source does not require a fixed tie line.");
				LogMethodCompleted(nameof(GetSharedRoutingService), stopwatch);
				return null;
			}

			var linkedOrderIds = InputService.Service.OrderReferences;
			Log(nameof(GetSharedRoutingService), $"Order references: {String.Join(", ", linkedOrderIds)}");

			foreach (var linkedOrderId in linkedOrderIds)
			{
				if (liveVideoOrder.Order.Id.Equals(linkedOrderId))
				{
					Log(nameof(GetSharedRoutingService), $"No need to analyze order with ID {linkedOrderId} for shared routing services.");
					continue; // Only consider routing services from other orders as possible shared routing services
				}

				var order = helpers.OrderManager.GetOrder(linkedOrderId);
				var sharedSourceService = order.Sources.FirstOrDefault(x => x.Id.Equals(InputService.Service.Id));
				if (sharedSourceService == null)
				{
					Log(nameof(GetSharedRoutingService), $"UNEXPECTED: Shared source {InputService.Service.Id} is linked with Order {order.Name}, but the order doesn't use the shared source service");
					continue;
				}

				if (TryGetFirstRoutingInFixedTieLine(sharedSourceService.Children, out var sharedRoutingRoutingService))
				{
					return sharedRoutingRoutingService;
				}

				Log(nameof(GetSharedRoutingService), $"Found no possible shared routing service in order with ID: {linkedOrderId}");
			}

			LogMethodCompleted(nameof(GetSharedRoutingService), stopwatch);

			return null;
		}

		private bool TryGetFirstRoutingInFixedTieLine(IEnumerable<Service> childrenOfSharedSource, out RoutingService sharedRoutingRoutingService)
		{
			// Get first routing service in fixed tie line from service to output service

			sharedRoutingRoutingService = null;

			LogMethodStart(nameof(TryGetFirstRoutingInFixedTieLine), out var stopwatch);

			foreach (var child in childrenOfSharedSource)
			{
				if (child.Definition.VirtualPlatform != VirtualPlatform.Routing) continue;
				if (liveVideoOrder.Order.AllServices.Contains(child)) continue; // Don't mark a routing service of this order as shared routing

				var outputResource = child.Functions.Last().Resource;
				if (outputResource == null) continue;

				var isSpecificTieLineProperty = outputResource.Properties.FirstOrDefault(x => x.Name.Equals(ResourcePropertyNames.IsSpecificTieLine));
				if (isSpecificTieLineProperty == null) continue;
				if (!Boolean.TryParse(isSpecificTieLineProperty.Value, out bool isSpecificTieLine) || !isSpecificTieLine) continue;

				var sharedRoutingReservation = helpers.ResourceManager.GetReservationInstance(child.Id);
				var sharedRoutingService = Service.FromReservationInstance(helpers, sharedRoutingReservation);

				sharedRoutingRoutingService = new RoutingService(helpers, sharedRoutingService, liveVideoOrder);

				Log(nameof(GetSharedRoutingService), $"Found possible routing service to share: {sharedRoutingService.Name} and ID: {sharedRoutingService.Id}");

				LogMethodCompleted(nameof(TryGetFirstRoutingInFixedTieLine), stopwatch);

				return true;
			}

			return false;
		}

		private TimeRangeUtc GetServiceTimeRange(Service service)
		{
			if (liveVideoOrder.Order.StartNow)
			{
				DateTime start = DateTime.UtcNow;
				try
				{
					// the actual start time of a service in a start now order will be earlier than the configured start time

					var earliestPossibleServiceStartTime = service.StartWithPreRoll.ToUniversalTime().Add(-TimeSpan.FromMinutes(Order.Order.StartNowDelayInMinutes));

					start = earliestPossibleServiceStartTime < DateTime.UtcNow ? DateTime.UtcNow : earliestPossibleServiceStartTime;

					return new TimeRangeUtc(start, service.EndWithPostRoll.ToUniversalTime());
				}
				catch (Exception)
				{
					Log(nameof(GetServiceTimeRange), $"Exception while creating TimeRangeUtc object for service {service.Name} in StartNow order. Used start {start.ToFullDetailString()} and end with postroll {service.EndWithPostRoll.ToFullDetailString()}");
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
					Log(nameof(GetServiceTimeRange), $"Exception while creating TimeRangeUtc object for service {service.Name} based on its start with preroll  {service.StartWithPreRoll.ToFullDetailString()} and its end with postroll {service.EndWithPostRoll.ToFullDetailString()}");
					throw;
				}
			}
		}

		private HashSet<FunctionResource> GetSelectableInputResources(HashSet<FunctionResource> availableInputResources)
		{
			var selectableInputResources = helpers.ResourceManager.GetConnectedResources(InputService?.OutputResource, availableInputResources, ProfileParameterGuids.ResourceInputConnectionsSdi);

			FilterResourcesOnFeedTypeCapability(selectableInputResources);

			if (!selectableInputResources.Any())
			{
				selectableInputResources.Add(null);

				Log(nameof(GetSelectableInputResources), $"No selectable Matrix Input SDI resources connected to '{InputService.OutputResource?.Name}', going through the flow with a null value");

				try
				{
					var matrixInputSdiResourcePool = helpers.ResourceManager.GetResourcePoolByName(helpers.ServiceDefinitionManager.RoutingServiceDefinition.FunctionDefinitions.Single(fd => fd.Id == FunctionGuids.MatrixInputSdi).ResourcePool);

					var allMatrixInputSdiResources = helpers.ResourceManager.GetResources(ResourceExposers.PoolGUIDs.Contains(matrixInputSdiResourcePool.ID)).Cast<FunctionResource>().ToList();

					var occupiedConnectedInputResources = helpers.ResourceManager.GetConnectedResources(InputService?.OutputResource, allMatrixInputSdiResources, ProfileParameterGuids.ResourceInputConnectionsSdi);

					foreach (var occupiedResource in occupiedConnectedInputResources.Select(r => new OccupiedResource(r)))
					{
						var occupyingServices = helpers.ResourceManager.GetOccupyingServices(occupiedResource, OutputService.Service.StartWithPreRoll, OutputService.Service.EndWithPostRoll, liveVideoOrder.Order.Id);

						Log(nameof(GetSelectableInputResources), $"Connected occupied resource {occupiedResource.Name} is occupied by '{string.Join("\n", occupyingServices.Select(o => o.ToString()))}'");
					}
				}
				catch (Exception)
				{
					Log(nameof(GetSelectableInputResources), $"Unable to get occupying services for occupied connected resources.");
				}
			}

			return selectableInputResources;
		}

		private HashSet<FunctionResource> GetSelectableOutputResources(HashSet<FunctionResource> availableOutputResources)
		{
			if (OutputService.Parent is RoutingService && OutputService.Parent.OutputResource != null)
			{
				// Matrix output SDI resources have concurrency 1, therefore we need to include the current assigned Matrix Output SDI resource of the output routing service 
				availableOutputResources.Add(OutputService.Parent.OutputResource);

				Log(nameof(GetSelectableOutputResources), $"Considering currently assigned Matrix Output SDI resource {OutputService.Parent.OutputResource?.Name} on the Output Routing Service as available");
			}

			var selectableOutputResources = helpers.ResourceManager.GetConnectedResources(OutputService?.InputResource, availableOutputResources, ProfileParameterGuids.ResourceOutputConnectionsSdi);

			if (!selectableOutputResources.Any())
			{
				selectableOutputResources.Add(null);

				Log(nameof(GetSelectableOutputResources), $"No selectable Matrix Output SDI resources connected to '{OutputService.InputResource?.Name}', going through the flow with a null value");

				try
				{
					var matrixOutputSdiResourcePool = helpers.ResourceManager.GetResourcePoolByName(helpers.ServiceDefinitionManager.RoutingServiceDefinition.FunctionDefinitions.Single(fd => fd.Id == FunctionGuids.MatrixOutputSdi).ResourcePool);

					var allMatrixOutputSdiResources = helpers.ResourceManager.GetResources(ResourceExposers.PoolGUIDs.Contains(matrixOutputSdiResourcePool.ID)).Cast<FunctionResource>().ToList();

					var occupiedConnectedOutputResources = helpers.ResourceManager.GetConnectedResources(OutputService?.InputResource, allMatrixOutputSdiResources, ProfileParameterGuids.ResourceOutputConnectionsSdi);

					foreach (var occupiedResource in occupiedConnectedOutputResources.Select(r => new OccupiedResource(r)))
					{
						var occupyingServices = helpers.ResourceManager.GetOccupyingServices(occupiedResource, OutputService.Service.StartWithPreRoll, OutputService.Service.EndWithPostRoll, liveVideoOrder.Order.Id);

						Log(nameof(GetSelectableOutputResources), $"Connected occupied resource {occupiedResource.Name} is occupied by '{string.Join("\n", occupyingServices.Select(o => o.ToString()))}'");
					}
				}
				catch (Exception)
				{
					Log(nameof(GetSelectableOutputResources), $"Unable to get occupying services for occupied connected resources.");
				}
			}

			return selectableOutputResources;
		}

		/// <summary>
		/// Used by ServiceController in UpdateService script to fill resource dropdowns
		/// </summary>
		/// <param name="routingService"></param>
		/// <param name="function"></param>
		/// <param name="filterForFixedTieLines"></param>
		/// <returns></returns>
		public HashSet<FunctionResource> GetSelectableRoutingResources(Service routingService, Function function, bool filterForFixedTieLines)
		{
			if (routingService == null) throw new ArgumentNullException(nameof(routingService));
			if (function == null) throw new ArgumentNullException(nameof(function));
			if (!UsesRoutingService(routingService.Id)) throw new ArgumentException($"Service {routingService.Name} is not part of the routing chain", nameof(routingService));

			var selectableRoutingResourceChains = RetrieveAvailableRoutingResourceChains(InputRoutingService?.MatrixInputSdi, OutputRoutingService?.MatrixOutputSdi, true, filterForFixedTieLines);

			if (routingService.Id == InputRoutingService?.Service?.Id)
			{
				helpers?.Log(nameof(RoutingServiceChain), nameof(GetSelectableRoutingResources), $"Service is first routing service in chain, returning all selectable first matrix outputs", routingService.Name);

				return Enumerable.ToHashSet(selectableRoutingResourceChains.Where(t => t.ConnectingMatrixRequired == UsesConnectingRoutingService).Select(t => t.FirstMatrix.Output));
			}

			if (routingService.Id == ConnectingRoutingService?.Service?.Id)
			{
				bool functionIsMatrixInput = function.Id == FunctionGuids.MatrixInputSdi;
				var selectableResources = Enumerable.ToHashSet(selectableRoutingResourceChains.Where(t => t.ConnectingMatrixRequired).Select(t => functionIsMatrixInput ? t.ConnectingMatrix.Input : t.ConnectingMatrix.Output));

				helpers?.Log(nameof(RoutingServiceChain), nameof(GetSelectableRoutingResources), $"Service is connecting routing service in chain, returning all selectable connecting matrix {(functionIsMatrixInput ? "inputs" : "outputs")}: {string.Join(",", selectableResources.Select(r => r?.Name))}", routingService.Name);

				return selectableResources;
			}

			if (routingService.Id == OutputRoutingService?.Service?.Id)
			{
				helpers?.Log(nameof(RoutingServiceChain), nameof(GetSelectableRoutingResources), $"Service is last routing service in chain, returning all selectable last matrix inputs", routingService.Name);

				return selectableRoutingResourceChains.Where(t => t.ConnectingMatrixRequired == UsesConnectingRoutingService).Select(t => t.LastMatrix.Input).ToHashSet();
			}

			throw new ServiceNotFoundException(routingService.Id);
		}

		public void SetMatchingResourceOnNeighborServiceConnectedFunction(Service routingService, Function function)
		{
			if (routingService == null) throw new ArgumentNullException(nameof(routingService));
			if (function == null) throw new ArgumentNullException(nameof(function));
			if (!UsesRoutingService(routingService.Id)) throw new ArgumentException($"Service {routingService.Name} is not part of the routing chain", nameof(routingService));

			if (function.Resource == null)
			{
				liveVideoOrder.ClearMatchingResourceOnNeighborServiceConnectedFunction(routingService, function);
				return;
			}

			var availableRoutingResourceChains = RetrieveAvailableRoutingResourceChains(InputRoutingService?.MatrixInputSdi, OutputRoutingService?.MatrixOutputSdi, true, false);

			var selectedRoutingResourceChain = SelectRoutingResourceChainUsingResourceAndAvoidingOtherChanges(availableRoutingResourceChains, function.Resource);
			if (selectedRoutingResourceChain == null)
			{
				Log(nameof(SetMatchingResourceOnNeighborServiceConnectedFunction), $"No tie line found using resource {function.ResourceName} from function {function.Name} in service {routingService.Name}");
				return;
			}

			Log(nameof(SetMatchingResourceOnNeighborServiceConnectedFunction), $"Selected tie line using resource {function.ResourceName} from function {function.Name} in service {routingService.Name}: {selectedRoutingResourceChain.ToString()}");

			Apply(selectedRoutingResourceChain, out var removedServices);
		}

		public bool UsesService(Guid serviceId)
		{
			return AllServices.Exists(s => s.Service.Id == serviceId);
		}

		public bool UsesRoutingService(Guid routingServiceId)
		{
			if (InputRoutingService != null && InputRoutingService.Service.Id == routingServiceId) return true;
			if (OutputRoutingService != null && OutputRoutingService.Service.Id == routingServiceId) return true;
			if (ConnectingRoutingService != null && ConnectingRoutingService.Service.Id == routingServiceId) return true;

			return false;
		}

		public bool UsesRoutingService(RoutingService routingService)
		{
			return UsesRoutingService(routingService.Service.Id);
		}

		public override string ToString()
		{
			var sb = new StringBuilder($"({(IsValid ? "Valid" : "Invalid")}) [{InputService?.Service?.Name}][{InputService?.OutputResource?.Name ?? "None"}]");

			if (InputRoutingService != null)
			{
				sb.Append($" ==> [{InputRoutingService?.MatrixInputSdi?.Name ?? "None"}][{InputRoutingService?.Service?.Name}][{InputRoutingService?.MatrixOutputSdi?.Name ?? "None"}]");
			}

			if (ConnectingRoutingService != null)
			{
				sb.Append($" ==> [{ConnectingRoutingService?.MatrixInputSdi?.Name ?? "None"}][{ConnectingRoutingService?.Service?.Name}][{ConnectingRoutingService?.MatrixOutputSdi?.Name ?? "None"}]");
			}

			sb.Append($" ==> [{OutputRoutingService?.MatrixInputSdi?.Name ?? "None"}][{OutputRoutingService?.Service?.Name}][{OutputRoutingService?.MatrixOutputSdi?.Name ?? "None"}]");

			sb.Append($" ==> [{OutputService?.InputResource?.Name ?? "None"}][{OutputService?.Service?.Name}]");

			return sb.ToString();
		}

		/// <summary>
		/// Checks if this routing service chain is valid, meaning it has the correct routing services based on the input and output service.
		/// Routing service chain can only be used as a reference for another routing service chain in case it's valid.
		/// </summary>
		/// <returns></returns>
		private void InitializeValidity()
		{
			if (Input == null || InputService == null ||
				Output == null || OutputService == null)
			{
				IsValid = false;
				return;
			}

			if (InputRoutingService == null && OutputRoutingService == null && ConnectingRoutingService == null)
			{
				IsValid = false;
				return;
			}

			if (Input.IsResourceFromSameElementAs(Output))
			{
				if (InputRoutingService != null || ConnectingRoutingService != null)
				{
					IsValid = false;
					return;
				}

				if (OutputRoutingService == null ||
					OutputRoutingService.MatrixInputSdi == null || !OutputRoutingService.MatrixInputSdi.Equals(Input) ||
					OutputRoutingService.MatrixOutputSdi == null || !OutputRoutingService.MatrixOutputSdi.Equals(Output))
				{
					IsValid = false;
					return;
				}
			}
			else
			{
				// when tielines are used the validity will initially be set to false as this needs to be regenerated and checked afterwards
				IsValid = false;
				return;
			}

			IsValid = true;
		}

		private RoutingResourceChain SelectRoutingResourceChainUsingResourceAndAvoidingOtherChanges(IEnumerable<RoutingResourceChain> selectableRoutingResourceChains, FunctionResource resource)
		{
			if (selectableRoutingResourceChains == null) throw new ArgumentNullException(nameof(selectableRoutingResourceChains));
			if (resource == null) throw new ArgumentNullException(nameof(resource));

			var routingResourceChainUsingResource = selectableRoutingResourceChains.Where(t => t.AllResources.Exists(r => r.GUID == resource.GUID)).ToList();

			return SelectRoutingResourceChainsAvoidingChanges(routingResourceChainUsingResource, out int amountOfChanges).FirstOrDefault();
		}

		private List<RoutingResourceChain> SelectRoutingResourceChainsAvoidingChanges(IEnumerable<RoutingResourceChain> selectableRoutingResourceChains, out int leastAmountOfChanges)
		{
			var routingResourceChainWithLeastAmountOfChanges = new List<RoutingResourceChain>();

			leastAmountOfChanges = 10;
			foreach (var selectableResourceChain in selectableRoutingResourceChains)
			{
				int amountOfChanges = RoutingResourceChain.GetAmountOfChangesComparedTo(selectableResourceChain);

				if (amountOfChanges <= leastAmountOfChanges)
				{
					if (amountOfChanges < leastAmountOfChanges) routingResourceChainWithLeastAmountOfChanges.Clear();

					routingResourceChainWithLeastAmountOfChanges.Add(selectableResourceChain);
					leastAmountOfChanges = amountOfChanges;
				}
			}

			Log(nameof(SelectRoutingResourceChainsAvoidingChanges), $"Resource chains {string.Join("\n", routingResourceChainWithLeastAmountOfChanges.Select(rrc => rrc.ToString()))} \nhave {leastAmountOfChanges} changes compared to current routing chain {RoutingResourceChain}");

			return routingResourceChainWithLeastAmountOfChanges;
		}

		private List<RoutingResourceChain> GetHighestPriorityRoutingResourceChains(List<RoutingResourceChain> routingResourceChains)
		{
			if (routingResourceChains is null) throw new ArgumentNullException(nameof(routingResourceChains));
			if (!routingResourceChains.Any()) return new List<RoutingResourceChain>();

			int bestCombinedResourcePriority = routingResourceChains.Select(rrc => rrc.CombinedResourcePriority).Min(); // lower prio is better

			var highestCombinedResourcePriorityRoutingResourcChains = routingResourceChains.Where(rrc => rrc.CombinedResourcePriority == bestCombinedResourcePriority).ToList();

			Log(nameof(GetHighestPriorityRoutingResourceChains), $"Routing resource chains with best combined resource prio {bestCombinedResourcePriority}:\n'{string.Join("\n", highestCombinedResourcePriorityRoutingResourcChains)}'");

			return highestCombinedResourcePriorityRoutingResourcChains;
		}

		private RoutingResourceChain GetRandomHighestPriorityRoutingResourceChain(List<RoutingResourceChain> routingResourceChains)
		{
			if (!routingResourceChains.Any()) throw new ArgumentException($"Collection contains no elements", nameof(routingResourceChains));

			var highestPriorityRoutingResourceChains = GetHighestPriorityRoutingResourceChains(routingResourceChains);

			var random = new Random();

			var bestRoutingResourceChain = highestPriorityRoutingResourceChains[random.Next(highestPriorityRoutingResourceChains.Count)];

			Log(nameof(GetRandomHighestPriorityRoutingResourceChain), $"Selected random best routing resource chain {bestRoutingResourceChain}");

			return bestRoutingResourceChain;
		}

		private bool TryGetPrioritizedRoutingResourceChain(List<RoutingResourceChain> routingResourceChains, out RoutingResourceChain prioritizedRoutingResourceChain)
		{
			//DCP203205

			prioritizedRoutingResourceChain = null;

			var prioritizedResourceName = InputService.Service.Functions.Select(f => f.Resource?.GetResourcePropertyStringValue(ResourcePropertyNames.PrioritizedTieLine)).FirstOrDefault(prioritizedTieLine => !string.IsNullOrWhiteSpace(prioritizedTieLine));

			if (prioritizedResourceName is null)
			{
				Log(nameof(TryGetPrioritizedRoutingResourceChain), $"No prioritized tie line found on service {InputService.Service.Name} resources: {string.Join(", ", InputService.Service.Functions.Select(f => f.ResourceName))}");
				return false;
			}

			var routingResourceChainsUsingThePrioritizedResource = routingResourceChains.Where(rrc => rrc.AllResources.Select(rr => rr.Name).Contains(prioritizedResourceName)).ToList();

			var highestPriorityChains = GetHighestPriorityRoutingResourceChains(routingResourceChainsUsingThePrioritizedResource);

			prioritizedRoutingResourceChain = highestPriorityChains.FirstOrDefault();

			Log(nameof(TryGetPrioritizedRoutingResourceChain), $"Used '{prioritizedResourceName}' to find routing resource chain '{prioritizedRoutingResourceChain}'");

			return prioritizedRoutingResourceChain != null;
		}

		private void UpdateRoutingServicesStartAndEndTimes()
		{
			var inputAndOutputServices = new List<Service> { InputService?.Service, OutputService?.Service }.Where(s => s != null && !s.IsSharedSource).ToList();

			var earliestService = inputAndOutputServices.OrderBy(s => s.Start).First();
			var lastService = inputAndOutputServices.OrderByDescending(s => s.End).First();

			foreach (var routingService in AllRoutingServices)
			{
				if (routingService.Service.Start != earliestService.Start)
				{
					routingService.Service.Start = earliestService.Start;

					Log(nameof(UpdateRoutingServicesStartAndEndTimes), $"Updated routing service {routingService.Service.Name} start time to {routingService.Service.Start}, based on service {earliestService.Name}");
				}

				if (routingService.Service.End != lastService.End)
				{
					routingService.Service.End = lastService.End;

					Log(nameof(UpdateRoutingServicesStartAndEndTimes), $"Updated routing service {routingService.Service.Name} end time to {routingService.Service.End}, based on service {lastService.Name}");
				}

				Log(nameof(UpdateRoutingServicesStartAndEndTimes), $"Service {routingService.Service.Name} timing: {Service.TimingInfoToString(routingService.Service)}");
			}
		}

		private void SelectDummyResourceForMessiNewsRecordingWithoutResource()
		{
			// For Messi News Recordings we always need to book a HMX -> UMX line even if the resource for the recording service is null.
			// Therefore we use a random 'UMX Output X.AirSpeed Y' resource and consider it as the output resource of the output routing service, this way the routing will be generated as usual.

			var routingServiceDefinition = helpers.ServiceDefinitionManager.RoutingServiceDefinition ?? throw new ServiceDefinitionNotFoundException("Unable to find Routing SD");

			var matrixOutputSdiResourcePoolName = routingServiceDefinition.FunctionDefinitions.FirstOrDefault(f => f.Id == FunctionGuids.MatrixOutputSdi)?.ResourcePool;

			var matrixOutputSdiResourcePool = DataMinerInterface.ResourceManager.GetResourcePools(helpers, new ResourcePool { Name = matrixOutputSdiResourcePoolName }).FirstOrDefault() ?? throw new ResourcePoolNotFoundException();

			var dummyResource = (FunctionResource)DataMinerInterface.ResourceManager.GetResources(helpers, ResourceExposers.PoolGUIDs.Contains(matrixOutputSdiResourcePool.GUID).AND(ResourceExposers.Name.Contains("AirSpeed"))).FirstOrDefault();

			Output = dummyResource;

			Log(nameof(SelectDummyResourceForMessiNewsRecordingWithoutResource), $"Output service {OutputService.Service.Name} is Messi News Recording with resource None. Selected resource {Output?.Name} as dummy to generate correct routing");
		}

		/// <summary>
		/// Used to filter the resources based on _FeedType capability to make sure the correct routing input is selected.
		/// This is only applicable in case of a Messi News recording for Plasma orders using Fixed Line LY source.
		/// </summary>
		/// <param name="resources">The list of available resources.</param>
		private void FilterResourcesOnFeedTypeCapability(HashSet<FunctionResource> resources)
		{
			var hasMessiNewsOutput = OutputService?.Service?.Definition?.Id.Equals(ServiceDefinitionGuids.RecordingMessiNews) == true;
			if (!hasMessiNewsOutput) return;

			var source = liveVideoOrder.GetSource(InputService);
			if (source == null) return;

			var isPlasmaMessiNewsOutput = OutputService.Service.IntegrationType == IntegrationType.Plasma;
			if (!isPlasmaMessiNewsOutput) return;

			Log(nameof(FilterResourcesOnFeedTypeCapability), "Plasma Messi News recording case");

			// the logic for plasma only applies in case the source is a fixed line LY source
			var isFixedLineLySource = source.Service?.Definition?.Id.Equals(ServiceDefinitionGuids.FixedLineLy) == true;
			if (!isFixedLineLySource) return;

			var function = OutputService.Service.Functions.FirstOrDefault();
			var feedTypeProfileParameter = function?.Parameters?.FirstOrDefault(p => p.Id == ProfileParameterGuids._FeedType);
			var feedTypeProfileParameterValue = feedTypeProfileParameter?.StringValue;
			if (String.IsNullOrWhiteSpace(feedTypeProfileParameterValue) || feedTypeProfileParameterValue == "None")
			{
				Log(nameof(FilterResourcesOnFeedTypeCapability), $"No Feed Type capability filter");
				return;
			}
			else
			{
				Log(nameof(FilterResourcesOnFeedTypeCapability), $"Feed Type capability filter: {feedTypeProfileParameterValue}");
			}

			resources.RemoveWhere(r => r is null || !r.MatchesProfileParameter(helpers, feedTypeProfileParameter));

			Log(nameof(FilterResourcesOnFeedTypeCapability), $"Resources matching Feed Type {feedTypeProfileParameterValue}: '{string.Join(", ", resources.Select(r => r.Name))}'");
		}

		private YleEligibleResourceContext GetMatrixEligibleResourceContext(Guid matrixFunctionGuid)
		{
			var outputTimeRange = GetServiceTimeRange(OutputService.Service);

			var matrixFunctionDefinition = helpers.ServiceDefinitionManager.RoutingServiceDefinition.FunctionDefinitions.SingleOrDefault(fd => fd.Id == matrixFunctionGuid) ?? throw new FunctionDefinitionNotFoundException($"Unable to find function definition with ID {matrixFunctionGuid} between function definitions {string.Join(", ", helpers.ServiceDefinitionManager.RoutingServiceDefinition.FunctionDefinitions.Select(fd => $"{fd.Name}({fd.Id})"))}");

			var context = matrixFunctionDefinition.GetEligibleResourceContext(helpers, outputTimeRange.Start, outputTimeRange.Stop);

			return context;
		}

		private void AddRoutingService(RoutingService routingService)
		{
			if (OutputRoutingService == null)
			{
				OutputRoutingService = routingService;

				Log(nameof(AddRoutingService), $"Added service {OutputRoutingService.Service.Name} ({OutputRoutingService.MatrixInputSdi?.Name},{OutputRoutingService.MatrixOutputSdi?.Name}) as output routing service");
			}
			else if (InputRoutingService == null)
			{
				InputRoutingService = routingService;

				Log(nameof(AddRoutingService), $"Added service {InputRoutingService.Service.Name} ({InputRoutingService.MatrixInputSdi?.Name},{InputRoutingService.MatrixOutputSdi?.Name}) as input routing service");
			}
			else
			{
				ConnectingRoutingService = InputRoutingService;
				InputRoutingService = routingService;

				Log(nameof(AddRoutingService), $"Added service {ConnectingRoutingService.Service.Name} ({ConnectingRoutingService.MatrixInputSdi?.Name},{ConnectingRoutingService.MatrixOutputSdi?.Name}) as connecting routing service");
			}
		}

		public void TryShareServicesWith(RoutingServiceChain other, out List<RoutingService> removedServices)
		{
			removedServices = new List<RoutingService>();

			if (other == null) return;

			LogMethodStart(nameof(TryShareServicesWith), out var stopwatch);

			var otherRoutingServiceChainRoutingServices = other.AllRoutingServices;

			// start with the output resources and go through the chain in reverse order
			var routingServicesInCurrentChain = AllRoutingServices.ToList();
			routingServicesInCurrentChain.Reverse();

			LiveVideoService previous = OutputService;
			foreach (var routingService in routingServicesInCurrentChain)
			{
				Log(nameof(TryShareServicesWith), $"Checking if routing service {routingService} matches with a routing service from other existing chain {other}", OutputService.Service.Name);

				var matchingRoutingServiceFromOtherChain = otherRoutingServiceChainRoutingServices.FirstOrDefault(r => r.HasMatchingResources(routingService));
				if (matchingRoutingServiceFromOtherChain != null && !matchingRoutingServiceFromOtherChain.Equals(routingService))
				{
					if (matchingRoutingServiceFromOtherChain.Service.IsBooked || !routingService.Service.IsBooked)
					{
						// Case 1: other chain has a service that this chain should use as well

						UseServiceFromOtherChain(removedServices, previous, routingService, matchingRoutingServiceFromOtherChain);
						liveVideoOrder.UpdateRoutingServiceTimings(matchingRoutingServiceFromOtherChain);
					}
					else
					{
						// Case 2: this chain has a service that other chain should use as well

						LetOtherChainUseServiceFromThisChain(removedServices, routingService, matchingRoutingServiceFromOtherChain);
						liveVideoOrder.UpdateRoutingServiceTimings(routingService);
					}

					LogMethodCompleted(nameof(TryShareServicesWith));
					return;
				}
				else
				{
					Log(nameof(TryShareServicesWith), $"No match");
				}

				previous = routingService;
			}

			LogMethodCompleted(nameof(TryShareServicesWith), stopwatch);
		}

		private void UseServiceFromOtherChain(List<RoutingService> removedServices, LiveVideoService previous, RoutingService routingService, RoutingService matchingRoutingServiceFromOtherChain)
		{
			Log(nameof(UseServiceFromOtherChain), $"Changing parent for {previous.Service.Name} from {routingService.Service.Name} to {matchingRoutingServiceFromOtherChain.Service.Name}, which is a matching routing service from another chain");

			// Update the parent to this reusable routing service
			liveVideoOrder.SetParent(previous, matchingRoutingServiceFromOtherChain);

			// Update routing service chain properties
			OutputService.InitializeCurrentRoutingServiceChain();

			removedServices.Add(routingService);
		}

		private void LetOtherChainUseServiceFromThisChain(List<RoutingService> removedServices, RoutingService routingService, RoutingService matchingRoutingServiceFromOtherChain)
		{
			// Case 2: this chain has a service that other chain should use as well

			foreach (var matchingRoutingChild in matchingRoutingServiceFromOtherChain.Children.ToList())
			{
				Log(nameof(LetOtherChainUseServiceFromThisChain), $"Changing parent for {matchingRoutingChild.Service.Name} from {matchingRoutingServiceFromOtherChain.Service.Name} to {routingService.Service.Name}, because the old parent is not booked and the new one is already booked");

				liveVideoOrder.SetParent(matchingRoutingChild, routingService);

				// Update routing service chain properties
				foreach (var routingServiceChain in liveVideoOrder.GetRoutingServiceChainsForService(matchingRoutingChild.Service.Id))
				{
					routingServiceChain.OutputService.InitializeCurrentRoutingServiceChain();
				}
			}

			removedServices.Add(matchingRoutingServiceFromOtherChain);
		}

		public void Apply(RoutingResourceChain routingResourceChain, out List<RoutingService> removedServices)
		{
			if (routingResourceChain is null) routingResourceChain = new RoutingResourceChain { LastMatrixRequired = true };

			removedServices = new List<RoutingService>();

			RoutingResourceChain = routingResourceChain;

			ApplyFirstMatrix(routingResourceChain, removedServices);

			ApplyConnectingMatrix(routingResourceChain, removedServices);

			ApplyLastMatrix(routingResourceChain, removedServices);

			if (OutputRoutingService != null && InputRoutingService == null && ConnectingRoutingService == null)
			{
				// in case of only 1 routing

				liveVideoOrder.SetParent(OutputService, OutputRoutingService);
				liveVideoOrder.SetParent(OutputRoutingService, InputService);
			}
			else
			{
				// in case of multiple routings

				liveVideoOrder.SetParent(OutputService, OutputRoutingService);
				liveVideoOrder.SetParent(InputRoutingService, InputService);

				if (ConnectingRoutingService != null)
				{
					liveVideoOrder.SetParent(OutputRoutingService, ConnectingRoutingService);
					liveVideoOrder.SetParent(ConnectingRoutingService, InputRoutingService);
				}
				else
				{
					liveVideoOrder.SetParent(OutputRoutingService, InputRoutingService);
				}
			}

			IsValid = true;
		}

		private RoutingService ApplyResourcesToRoutingService(Helpers helpers, bool routingServiceIsRequired, FunctionResource inputResourceToApply, FunctionResource outputResourceToApply, RoutingService routingServiceToApplyTo, List<RoutingService> removedServices, LiveVideoOrder liveVideoOrder, RoutingRequiringService outputService)
		{
			if (routingServiceIsRequired)
			{
				bool newRoutingServiceGenerationRequired = routingServiceToApplyTo is null;

				Log(nameof(ApplyResourcesToRoutingService), $"Applying Resource {inputResourceToApply} and {outputResourceToApply} to service {outputService}");

				if (SharedRoutingService != null && SharedRoutingService.InputResource?.ID == inputResourceToApply?.ID && SharedRoutingService.OutputResource?.ID == outputResourceToApply?.ID)
				{
					Log(nameof(ApplyResourcesToRoutingService), $"Using Shared routing service with Order References: {String.Join(", ", SharedRoutingService.Service.OrderReferences)}");

					// Start change tracking
					SharedRoutingService.Service.AcceptChanges(helpers);

					SharedRoutingService.Service.OrderReferences.Add(liveVideoOrder.Order.Id);
					SharedRoutingService.Service.IsSharedSource = true;

					routingServiceToApplyTo = SharedRoutingService;

					liveVideoOrder.LiveVideoServices.Add(routingServiceToApplyTo);

					helpers.Log(nameof(RoutingServiceChain), nameof(ApplyResourcesToRoutingService), $"Used shared routing service {routingServiceToApplyTo.Service.Name} with resources '{routingServiceToApplyTo.MatrixInputSdi?.Name}' and '{routingServiceToApplyTo.MatrixOutputSdi?.Name}'");

					return routingServiceToApplyTo;
				}

				if (routingServiceToApplyTo != null)
				{
					newRoutingServiceGenerationRequired = !TryApplyToRoutingService(helpers, inputResourceToApply, outputResourceToApply, routingServiceToApplyTo);
				}

				if (newRoutingServiceGenerationRequired)
				{
					routingServiceToApplyTo = GenerateNewRoutingService(helpers, inputResourceToApply, outputResourceToApply, routingServiceToApplyTo, liveVideoOrder, outputService);
				}

				return routingServiceToApplyTo;
			}
			else
			{
				HandleNotRequiredRoutingService(helpers, routingServiceToApplyTo, removedServices);

				return null;
			}
		}

		private static RoutingService GenerateNewRoutingService(Helpers helpers, FunctionResource inputResourceToApply, FunctionResource outputResourceToApply, RoutingService routingServiceToApplyTo, LiveVideoOrder liveVideoOrder, RoutingRequiringService outputService)
		{
			helpers.Log(nameof(RoutingServiceChain), nameof(GenerateNewRoutingService), $"A routing service {(routingServiceToApplyTo != null ? "already exists" : "does not yet exist")}. Its matrix input is {(routingServiceToApplyTo?.MatrixInputSdiIsValid ?? false ? "valid" : "invalid")}. Its matrix output is {(routingServiceToApplyTo?.MatrixOutputSdiIsValid ?? false ? "valid" : "invalid")}.");

			// if there isn't an output routing service yet then a new one needs to be created
			routingServiceToApplyTo = RoutingService.GenerateNewRoutingService(helpers, inputResourceToApply, outputResourceToApply, outputService);

			liveVideoOrder.LiveVideoServices.Add(routingServiceToApplyTo);

			helpers.Log(nameof(RoutingServiceChain), nameof(GenerateNewRoutingService), $"Generated new routing service {routingServiceToApplyTo.Service.Name} with resources '{routingServiceToApplyTo.MatrixInputSdi?.Name}' and '{routingServiceToApplyTo.MatrixOutputSdi?.Name}'");
			return routingServiceToApplyTo;
		}

		private static bool TryApplyToRoutingService(Helpers helpers, FunctionResource inputResourceToApply, FunctionResource outputResourceToApply, RoutingService routingServiceToApplyTo)
		{
			helpers.Log(nameof(RoutingServiceChain), nameof(TryApplyToRoutingService), $"Routing service {routingServiceToApplyTo.Service.Name} function config: {nameof(RoutingService.MatrixInputSdiIsValid)}={routingServiceToApplyTo.MatrixInputSdiIsValid}, {nameof(RoutingService.MatrixInputSdiEnforceNone)}={routingServiceToApplyTo.MatrixInputSdiEnforceNone}, {nameof(RoutingService.MatrixOutputSdiIsValid)}={routingServiceToApplyTo.MatrixOutputSdiIsValid}; {nameof(RoutingService.MatrixOutputSdiEnforceNone)}={routingServiceToApplyTo.MatrixOutputSdiEnforceNone}");

			if (!routingServiceToApplyTo.MatrixInputSdiIsValid && !routingServiceToApplyTo.MatrixInputSdiEnforceNone)
			{
				routingServiceToApplyTo.MatrixInputSdi = inputResourceToApply;
			}

			if (!routingServiceToApplyTo.MatrixOutputSdiIsValid && !routingServiceToApplyTo.MatrixOutputSdiEnforceNone)
			{
				routingServiceToApplyTo.MatrixOutputSdi = outputResourceToApply;
			}

			bool inputsAreNull = routingServiceToApplyTo.MatrixInputSdi is null && inputResourceToApply is null;
			bool inputsAreNotNullAndEqual = routingServiceToApplyTo.MatrixInputSdi != null && routingServiceToApplyTo.MatrixInputSdi.Equals(inputResourceToApply);
			bool nullIsEnforcedOnInput = routingServiceToApplyTo.MatrixInputSdi is null && routingServiceToApplyTo.MatrixInputSdiEnforceNone;

			bool outputsAreNull = routingServiceToApplyTo.MatrixOutputSdi is null && outputResourceToApply is null;
			bool outputsAreNotNullAndEqual = routingServiceToApplyTo.MatrixOutputSdi != null && routingServiceToApplyTo.MatrixOutputSdi.Equals(outputResourceToApply);
			bool nullIsEnforcedOnOutput = routingServiceToApplyTo.MatrixOutputSdi is null && routingServiceToApplyTo.MatrixOutputSdiEnforceNone;

			bool successfullyAppliedToExistingRouting = (inputsAreNull || inputsAreNotNullAndEqual || nullIsEnforcedOnInput) && (outputsAreNull || outputsAreNotNullAndEqual || nullIsEnforcedOnOutput);

			if (successfullyAppliedToExistingRouting)
			{
				helpers.Log(nameof(RoutingServiceChain), nameof(TryApplyToRoutingService), $"Updated resources on existing routing service {routingServiceToApplyTo.Service.Name} to '{routingServiceToApplyTo.MatrixInputSdi?.Name}' and '{routingServiceToApplyTo.MatrixOutputSdi?.Name}'");
			}

			return successfullyAppliedToExistingRouting;
		}

		private static void HandleNotRequiredRoutingService(Helpers helpers, RoutingService routingServiceToApplyTo, List<RoutingService> removedServices)
		{
			if (routingServiceToApplyTo != null && !routingServiceToApplyTo.Children.Any())
			{
				removedServices.Add(routingServiceToApplyTo);

				helpers.Log(nameof(RoutingServiceChain), nameof(ApplyResourcesToRoutingService), $"Removed existing routing service {routingServiceToApplyTo.Service.Name}");
			}
		}

		private void ApplyLastMatrix(RoutingResourceChain routingResourceChain, List<RoutingService> removedServices)
		{
			OutputRoutingService = ApplyResourcesToRoutingService(helpers, routingResourceChain.LastMatrixRequired, routingResourceChain.LastMatrix.Input, routingResourceChain.LastMatrix.Output, OutputRoutingService, removedServices, liveVideoOrder, OutputService);

			Log(nameof(ApplyLastMatrix), $"Output routing service is '{OutputRoutingService?.Service?.Name}' with resources '{OutputRoutingService?.MatrixInputSdi?.Name}' and '{OutputRoutingService?.MatrixOutputSdi?.Name}'");
		}

		private void ApplyConnectingMatrix(RoutingResourceChain routingResourceChain, List<RoutingService> removedServices)
		{
			ConnectingRoutingService = ApplyResourcesToRoutingService(helpers, routingResourceChain.ConnectingMatrixRequired, routingResourceChain.ConnectingMatrix.Input, routingResourceChain.ConnectingMatrix.Output, ConnectingRoutingService, removedServices, liveVideoOrder, OutputService);

			Log(nameof(ApplyConnectingMatrix), $"Connecting routing service is '{ConnectingRoutingService?.Service?.Name}' with resources '{ConnectingRoutingService?.MatrixInputSdi?.Name}' and '{ConnectingRoutingService?.MatrixOutputSdi?.Name}'");
		}

		private void ApplyFirstMatrix(RoutingResourceChain routingResourceChain, List<RoutingService> removedServices)
		{
			InputRoutingService = ApplyResourcesToRoutingService(helpers, routingResourceChain.FirstMatrixRequired, routingResourceChain.FirstMatrix.Input, routingResourceChain.FirstMatrix.Output, InputRoutingService, removedServices, liveVideoOrder, OutputService);

			Log(nameof(ApplyFirstMatrix), $"Input routing service is '{InputRoutingService?.Service?.Name}' with resources '{InputRoutingService?.MatrixInputSdi?.Name}' and '{InputRoutingService?.MatrixOutputSdi?.Name}'");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S103:Lines should not be too long", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S1541:Methods and properties should not be too complex", Justification = "<Pending>")]
		public RoutingResourceChain SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains(List<RoutingServiceChain> otherExistingRoutingServiceChains, out RoutingServiceChain matchingRoutingServiceChain)
		{
			// Return variables
			matchingRoutingServiceChain = null;
			RoutingResourceChain resourceChainWithMostOverlap = null;

			Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Current chain is {ToString()}");

			if (AllServices.TrueForAll(s => s.Service.IsBooked))
			{
				bool currentChainIsSelectable = RoutingResourceChain.IsValid;
				if (currentChainIsSelectable)
				{
					if (RoutingResourcesAreEnforced)
					{
						// Case 2: Routing service resource changed via Update Service
						// Expected result: Compare the current chain to existing routing chains to find shareable routings

						Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Case 2: a routing resource from a fully booked chain has changed. \nCurrent chain is selectable. \nCurrent chain is {RoutingResourceChain}. \nLooking for existing chains to share routing services with...");

						// Default value in case there are no existing chains to compare with
						resourceChainWithMostOverlap = RoutingResourceChain;

						int highestOverlappingResourceCount = 0;
						int minimumRequiredOverlappingResourceCountForSharing = 1;
						foreach (var existingRoutingServiceChain in otherExistingRoutingServiceChains)
						{
							if (existingRoutingServiceChain.OutputService.Equals(OutputService)) continue; // do not compare the current chain to itself, because it will always have a overlapping resource count of at least 1. 

							var overlappingResourceCount = RoutingResourceChain.GetAmountOfOverlappingResourcesComparedTo(existingRoutingServiceChain.RoutingResourceChain);

							Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Existing routing resource chain {existingRoutingServiceChain.RoutingResourceChain} has overlapping resource count = {overlappingResourceCount}.");

							if (minimumRequiredOverlappingResourceCountForSharing <= overlappingResourceCount && highestOverlappingResourceCount < overlappingResourceCount)
							{
								matchingRoutingServiceChain = existingRoutingServiceChain; // existing service chain can be used for sharing
								highestOverlappingResourceCount = overlappingResourceCount;
								resourceChainWithMostOverlap = RoutingResourceChain; // keep current resource chain as it is defined by MCR

								Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Found existing routing service chain {matchingRoutingServiceChain} with new highest overlap count {highestOverlappingResourceCount}");
							}
						}
					}
					else
					{
						// Case 3: No resource changes happened to any service in the chain
						// Expected result: Compare the current chain to existing routing chains to find shareable routings
						// Only change the current chain if any better and shareable chain can be found

						Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Case 3: no resource changes happened for a fully booked chain. \nCurrent chain is selectable. \nCurrent chain is {RoutingResourceChain}. \nLooking for existing chains to share routing services with...");

						int highestOverlappingResourceCount = 0;
						int minimumRequiredOverlappingMatrixResourcePairsForSharing = 1;
						var bestSelectableRoutingResourceChains = new List<SelectableRoutingResourceChain>();

						foreach (var existingRoutingServiceChain in otherExistingRoutingServiceChains)
						{
							if (existingRoutingServiceChain.OutputService.Equals(OutputService)) continue; // do not compare the current chain to itself, because it will always have a overlapping resource count of at least 1. 

							Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Existing routing service chain to compare to is {existingRoutingServiceChain.ToString()}");

							foreach (var selectableRoutingResourceChain in SelectableRoutingResourceChains)
							{
								var overlappingMatrixResourcePairCount = selectableRoutingResourceChain.GetAmountOfOverlappingMatrixResourcePairsComparedTo(existingRoutingServiceChain.RoutingResourceChain);

								Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Selectable routing resource chain {selectableRoutingResourceChain} has overlapping matrix resoruce resource pair count = {overlappingMatrixResourcePairCount}.");

								if (minimumRequiredOverlappingMatrixResourcePairsForSharing <= overlappingMatrixResourcePairCount && highestOverlappingResourceCount <= overlappingMatrixResourcePairCount)
								{
									if (highestOverlappingResourceCount < overlappingMatrixResourcePairCount) bestSelectableRoutingResourceChains.Clear();

									bestSelectableRoutingResourceChains.Add(new SelectableRoutingResourceChain
									{
										RoutingResourceChain = selectableRoutingResourceChain,
										ExistingRoutingServiceChain = existingRoutingServiceChain,
									});

									highestOverlappingResourceCount = overlappingMatrixResourcePairCount;

									Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Found routing resource chain {resourceChainWithMostOverlap} with overlap count {highestOverlappingResourceCount}");
								}
							}
						}

						bool currentChainIsShareable = bestSelectableRoutingResourceChains.Select(srrc => srrc.RoutingResourceChain).Contains(RoutingResourceChain);
						bool thereAreBetterSelectableRoutingResourceChainsThanTheCurrentOne = bestSelectableRoutingResourceChains.Any() && !currentChainIsShareable;

						if (thereAreBetterSelectableRoutingResourceChainsThanTheCurrentOne)
						{
							Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Better selectable routing resource chains exist than the current one.");

							if (TryGetPrioritizedRoutingResourceChain(bestSelectableRoutingResourceChains.Select(srrc => srrc.RoutingResourceChain).ToList(), out var prioritizedRoutingResourceChain))
							{
								resourceChainWithMostOverlap = prioritizedRoutingResourceChain;
							}
							else
							{
								resourceChainWithMostOverlap = GetRandomHighestPriorityRoutingResourceChain(bestSelectableRoutingResourceChains.Select(srrc => srrc.RoutingResourceChain).ToList());
							}

							matchingRoutingServiceChain = bestSelectableRoutingResourceChains.First(srrc => srrc.RoutingResourceChain.Equals(resourceChainWithMostOverlap)).ExistingRoutingServiceChain;

							Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Selected routing resource chain {resourceChainWithMostOverlap} from which we can share services with {matchingRoutingServiceChain}");
						}
						else
						{
							Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"No better selectable routing resource chains exist than the current one.");

							resourceChainWithMostOverlap = RoutingResourceChain;

							if (currentChainIsShareable)
							{
								matchingRoutingServiceChain = bestSelectableRoutingResourceChains.First(srrc => srrc.RoutingResourceChain.Equals(resourceChainWithMostOverlap)).ExistingRoutingServiceChain;
							}

							Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Current routing resource chain is {(currentChainIsShareable ? String.Empty : "not ")}shareable {(currentChainIsShareable ? $"with {matchingRoutingServiceChain}" : String.Empty)}");
						}
					}
				}
				else
				{
					// Case 0: Input service resource changed
					// Expected Result: 
					//		Part I: Find the selectable routing chain (for the new input service resource) with the least changes compared to the current one (for the previous input service resource)
					//		Part II: Compare the selected routing chain to existing routing chains to find shareable routings

					// Case 1: End point service resource changed via Update Service
					// Expected result:
					//		Part I: Find the selectable routing chain (for the new end point service resource) with the least changes compared to the current one (for the previous end point service resource)
					//		Part II: Compare the selected routing chain to existing routing chains to find shareable routings

					Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Cases 0/1 : an input service resource or output service resource from a fully booked chain has changed. \nCurrent chain is not selectable. \nCurrent chain is {RoutingResourceChain}. \nComparing selectable chains to existing chains to find a chain that can be shared...");

					var selectableRoutingResourceChains = SelectRoutingResourceChainsAvoidingChanges(SelectableRoutingResourceChains, out int amountOfChanges);

					int highestOverlappingResourceCount = 0;
					int minimumRequiredOverlappingResourceCountForSharing = 1;
					var bestSelectableRoutingResourceChains = new List<SelectableRoutingResourceChain>();

					foreach (var existingRoutingServiceChain in otherExistingRoutingServiceChains)
					{
						if (existingRoutingServiceChain.OutputService.Equals(OutputService)) continue; // do not compare the current chain to itself, because it will always have a overlapping resource count of at least 1. 

						Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Existing routing service chain to compare to is {existingRoutingServiceChain.ToString()}");

						foreach (var selectableRoutingResourceChain in selectableRoutingResourceChains)
						{
							var overlappingResourceCount = selectableRoutingResourceChain.GetAmountOfOverlappingResourcesComparedTo(existingRoutingServiceChain.RoutingResourceChain);

							Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Selectable routing resource chain {selectableRoutingResourceChain} has overlapping resource count = {overlappingResourceCount}.");

							if (minimumRequiredOverlappingResourceCountForSharing <= overlappingResourceCount && highestOverlappingResourceCount <= overlappingResourceCount)
							{
								if (highestOverlappingResourceCount < overlappingResourceCount) bestSelectableRoutingResourceChains.Clear();

								bestSelectableRoutingResourceChains.Add(new SelectableRoutingResourceChain
								{
									RoutingResourceChain = selectableRoutingResourceChain,
									ExistingRoutingServiceChain = existingRoutingServiceChain,
								});

								highestOverlappingResourceCount = overlappingResourceCount;

								Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Found routing resource chain {selectableRoutingResourceChain} with overlap count {highestOverlappingResourceCount}");
							}
						}
					}

					if (bestSelectableRoutingResourceChains.Any())
					{
						if (TryGetPrioritizedRoutingResourceChain(bestSelectableRoutingResourceChains.Select(srrc => srrc.RoutingResourceChain).ToList(), out var prioritizedRoutingResourceChain))
						{
							resourceChainWithMostOverlap = prioritizedRoutingResourceChain;
						}
						else
						{
							resourceChainWithMostOverlap = GetRandomHighestPriorityRoutingResourceChain(bestSelectableRoutingResourceChains.Select(srrc => srrc.RoutingResourceChain).ToList());
						}

						matchingRoutingServiceChain = bestSelectableRoutingResourceChains.First(srrc => srrc.RoutingResourceChain.Equals(resourceChainWithMostOverlap)).ExistingRoutingServiceChain;

						Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Selected routing resource chain {resourceChainWithMostOverlap} from which can share services with {matchingRoutingServiceChain}");
					}
					else
					{
						// Default value in case there are no existing chains to compare with
						resourceChainWithMostOverlap = GetRandomHighestPriorityRoutingResourceChain(selectableRoutingResourceChains);
					}
				}
			}
			else
			{
				// Case 4 & 5: New end point service added via LOF edit with or without resource selection in the UI. Or new processing service generated.
				// Expected result: selectable routing chains should be compared to existing routing chains to find shareable routings

				// only share if an entire routing service would have same resources. e.g.: InputRoutingService Matrix Input SDI and Matrix Output SDI should be same

				Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Cases 4/5 : New non-fully booked chain. \nComparing selectable chains to existing chains to find a chain that can be shared...");

				var bestSelectableRoutingResourceChains = new List<SelectableRoutingResourceChain>();

				int highestOverlappingMatrixResourcePairCount = 0;
				int minimumRequiredOverlappingMatrixResourcePairCountForSharing = 1;

				foreach (var existingRoutingServiceChain in otherExistingRoutingServiceChains)
				{
					Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Existing routing service chain to compare to is {existingRoutingServiceChain.ToString()}");

					foreach (var selectableRoutingResourceChain in SelectableRoutingResourceChains)
					{
						var overlappingMatrixResourcePairCount = selectableRoutingResourceChain.GetAmountOfOverlappingMatrixResourcePairsComparedTo(existingRoutingServiceChain.RoutingResourceChain);

						Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Selectable routing resource chain {selectableRoutingResourceChain} has overlapping matrix resource pair count = {overlappingMatrixResourcePairCount}.");

						if (minimumRequiredOverlappingMatrixResourcePairCountForSharing <= overlappingMatrixResourcePairCount && highestOverlappingMatrixResourcePairCount <= overlappingMatrixResourcePairCount)
						{
							if (highestOverlappingMatrixResourcePairCount < overlappingMatrixResourcePairCount) bestSelectableRoutingResourceChains.Clear();

							bestSelectableRoutingResourceChains.Add(new SelectableRoutingResourceChain
							{
								RoutingResourceChain = selectableRoutingResourceChain,
								ExistingRoutingServiceChain = existingRoutingServiceChain,
							});

							highestOverlappingMatrixResourcePairCount = overlappingMatrixResourcePairCount;

							Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Found routing resource chain {resourceChainWithMostOverlap} with overlap count {highestOverlappingMatrixResourcePairCount}");
						}
					}
				}

				if (bestSelectableRoutingResourceChains.Any())
				{
					if (TryGetPrioritizedRoutingResourceChain(bestSelectableRoutingResourceChains.Select(srrc => srrc.RoutingResourceChain).ToList(), out var prioritizedRoutingResourceChain))
					{
						resourceChainWithMostOverlap = prioritizedRoutingResourceChain;
					}
					else
					{
						resourceChainWithMostOverlap = GetRandomHighestPriorityRoutingResourceChain(bestSelectableRoutingResourceChains.Select(srrc => srrc.RoutingResourceChain).ToList());
					}

					matchingRoutingServiceChain = bestSelectableRoutingResourceChains.First(srrc => srrc.RoutingResourceChain.Equals(resourceChainWithMostOverlap)).ExistingRoutingServiceChain;

					Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Selected routing resource chain {resourceChainWithMostOverlap} from which can share services with {matchingRoutingServiceChain}");
				}
				else
				{
					// Default value in case there are no existing chains to compare with

					if (TryGetPrioritizedRoutingResourceChain(SelectableRoutingResourceChains, out var prioritizedRoutingResourceChain))
					{
						resourceChainWithMostOverlap = prioritizedRoutingResourceChain;
					}
					else
					{
						resourceChainWithMostOverlap = GetRandomHighestPriorityRoutingResourceChain(SelectableRoutingResourceChains);
					}
				}
			}

			bool existingRoutingServiceChainCanBeShared = matchingRoutingServiceChain != null;
			if (existingRoutingServiceChainCanBeShared)
			{
				// Existing routing service chain can be shared

				return resourceChainWithMostOverlap;
			}
			else if (otherExistingRoutingServiceChains.Any() && otherExistingRoutingServiceChains.All(rr => rr.IsValid))
			{
				// No existing chain found to share routing services with

				// There are existing chains, but we're not able to share routing services with any of them, so we need to avoid using resources from those existing chains in this chain.

				var routingResourcesWithLowConcurrencyFromExistingChains = otherExistingRoutingServiceChains.SelectMany(x => x.GetAllRoutingServiceResources()).Where(r => r.MaxConcurrency == 1).ToList();

				Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Existing chains exist but cannot be shared. Selected resource chain should not use the same low-concurrency routing resources as the existing chains: '{string.Join(", ", routingResourcesWithLowConcurrencyFromExistingChains.Select(r => r.Name))}'");

				if (resourceChainWithMostOverlap.AllResources.Intersect(routingResourcesWithLowConcurrencyFromExistingChains).Any())
				{
					SelectableRoutingResourceChains.RemoveAll(rrc => rrc.AllResources.Exists(r => routingResourcesWithLowConcurrencyFromExistingChains.Contains(r)));

					Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Selected resource chain uses an already occupied resource, so we need to select a new one. Remaining selectable routing resource chains: '{string.Join("\n", SelectableRoutingResourceChains.Select(rcs => rcs.ToString()))}'");

					if (TryGetPrioritizedRoutingResourceChain(SelectableRoutingResourceChains, out var prioritizedRoutingResourceChain))
					{
						resourceChainWithMostOverlap = prioritizedRoutingResourceChain;
					}
					else
					{
						resourceChainWithMostOverlap = GetRandomHighestPriorityRoutingResourceChain(SelectableRoutingResourceChains);
					}
				}
				else
				{
					Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Selected resource chain is fine");
				}
			}
			else
			{
				// nothing
			}

			Log(nameof(SelectRoutingResourceChainWithMostOverlapToOtherRoutingServiceChains), $"Final result: selected resource chain is {resourceChainWithMostOverlap}");

			return resourceChainWithMostOverlap;
		}

		/// <summary>
		/// Used by order update handler to generate routing
		/// </summary>
		public List<RoutingResourceChain> GetAvailableRoutingResourceChains()
		{
			var outputServiceStart = OutputService.Service.StartWithPreRoll.ToUniversalTime();
			var outputServiceEnd = OutputService.Service.EndWithPostRoll.ToUniversalTime();

			bool filterForFixedTieLineRequired = AllRoutingServices.SelectMany(s => s.Service.Functions).All(f => !f.McrHasOverruledFixedTieLineLogic);

			Log(nameof(GetAvailableRoutingResourceChains), $"Filtering for fixed tie lines {(filterForFixedTieLineRequired ? string.Empty : "not ")}required, because MCR has {(filterForFixedTieLineRequired ? "not " : string.Empty)}overridden fixed tie line logic.");

			var availableRoutingResourceChains = RetrieveAvailableRoutingResourceChains(Input, Output, false, filterForFixedTieLineRequired);

			Log(nameof(GetAvailableRoutingResourceChains), $"Found {availableRoutingResourceChains.Count} available tie lines between {Input?.Name} and {Output?.Name}");

			foreach (var availableRoutingResourceChain in availableRoutingResourceChains) Log(nameof(GetAvailableRoutingResourceChains), $"Available tie line: {availableRoutingResourceChain}");

			return availableRoutingResourceChains;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inputMatrixInputResource">The first resource of the tie line.</param>
		/// <param name="outputMatrixOutputResource">The last resource of the tie line.</param>
		/// <param name="allowMatrixInputAndOrMatrixOutputToBeNull"></param>
		/// <param name="filterForFixedTieLines">A boolean saying the fixed tie line logic should be applied or not.</param>
		/// <returns></returns>
		private List<RoutingResourceChain> RetrieveAvailableRoutingResourceChains(FunctionResource inputMatrixInputResource, FunctionResource outputMatrixOutputResource, bool allowMatrixInputAndOrMatrixOutputToBeNull = false, bool filterForFixedTieLines = true)
		{
			if (!allowMatrixInputAndOrMatrixOutputToBeNull && (inputMatrixInputResource == null || outputMatrixOutputResource == null)) return new List<RoutingResourceChain>();

			//ConsiderOtherResourcesAsAvailable(otherRoutingServiceChainsForSameInputService, availableInputs, availableOutputs);

			// find the input and output resources connected to one another directly (tie lines)
			var tieLinesWithoutHop = RetrieveRoutingResourceChainsWithTwoHops(inputMatrixInputResource, outputMatrixOutputResource, filterForFixedTieLines);
			if (!allowMatrixInputAndOrMatrixOutputToBeNull && tieLinesWithoutHop.Any()) return tieLinesWithoutHop;

			helpers.Log(nameof(RoutingServiceChain), nameof(RetrieveAvailableRoutingResourceChains), $"No chains found with two hops");

			// find the input and output resources connected to one another with an additional hop
			var tieLinesWithHop = RetrieveRoutingResourceChainsWithThreeHops(inputMatrixInputResource, outputMatrixOutputResource);
			if (!allowMatrixInputAndOrMatrixOutputToBeNull) return tieLinesWithHop;

			return tieLinesWithoutHop.Concat(tieLinesWithHop).ToList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S103:Lines should not be too long", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S1541:Methods and properties should not be too complex", Justification = "<Pending>")]
		private List<RoutingResourceChain> RetrieveRoutingResourceChainsWithThreeHops(FunctionResource inputMatrixInputResource = null, FunctionResource outputMatrixOutputResource = null, bool filterForFixedTieLine = true)
		{
			LogMethodStart(nameof(RetrieveRoutingResourceChainsWithThreeHops), out var stopWatch);

			if (availableMatrixInputSdiResources is null || availableMatrixOutputSdiResources is null)
			{
				GetAvailableMatrixSdiInputsAndOutputs();
			}

			var routingResourceChains = new List<RoutingResourceChain>();

			bool retrieveTieLinesForSpecificInput = inputMatrixInputResource != null;
			bool retrieveTieLinesForSpecificOutput = outputMatrixOutputResource != null;

			foreach (var input in availableMatrixInputSdiResources)
			{
				if (retrieveTieLinesForSpecificInput && input.GUID != inputMatrixInputResource.GUID) continue;

				var availableOutputsFromInputMatrix = availableMatrixOutputSdiResources.Where(o => o.IsResourceFromSameElementAs(input)).ToList();

				if (filterForFixedTieLine)
				{
					availableOutputsFromInputMatrix = FilterForFixedTieLinesForSource(input, availableOutputsFromInputMatrix);
				}

				var availableOutputsFromOtherMatricesThanInput = availableMatrixOutputSdiResources.Where(o => !o.IsResourceFromSameElementAs(input)).ToList();
				foreach (var output in availableOutputsFromOtherMatricesThanInput)
				{
					if (retrieveTieLinesForSpecificOutput && output.GUID != outputMatrixOutputResource.GUID) continue;

					var availableInputsFromOutputMatrix = availableMatrixInputSdiResources.Where(i => i.IsResourceFromSameElementAs(output));

					var availableOutputsFromOtherMatricesThanInputAndOutput = availableMatrixOutputSdiResources.Where(r => !r.IsResourceFromSameElementAs(input) && !r.IsResourceFromSameElementAs(output)).ToList();

					if (filterForFixedTieLine)
					{
						availableOutputsFromOtherMatricesThanInputAndOutput = availableOutputsFromOtherMatricesThanInputAndOutput.Where(r => !r.GetResourcePropertyBooleanValue(ResourcePropertyNames.IsSpecificTieLine)).ToList();
					}

					var availableInputsFromOtherMatricesThanInputAndOutput = availableMatrixInputSdiResources.Where(r => !r.IsResourceFromSameElementAs(input) && !r.IsResourceFromSameElementAs(output)).ToList();

					if (filterForFixedTieLine)
					{
						availableInputsFromOtherMatricesThanInputAndOutput = availableInputsFromOtherMatricesThanInputAndOutput.Where(r => !r.GetResourcePropertyBooleanValue(ResourcePropertyNames.RequiresSpecificTieLine)).ToList();
					}

					// retrieve the available tie line pairs for the input and output matrix
					var availableTieLinePairsForInputMatrix = RetrieveAvailableTieLineInputAndOutputPairs(availableInputsFromOtherMatricesThanInputAndOutput, availableOutputsFromInputMatrix);
					var availableTieLinePairsForOutputMatrix = RetrieveAvailableTieLineInputAndOutputPairs(availableInputsFromOutputMatrix, availableOutputsFromOtherMatricesThanInputAndOutput);
					if (!availableTieLinePairsForInputMatrix.Any() || !availableTieLinePairsForOutputMatrix.Any()) continue;

					// merge the pairs that use a matching matrix hop
					foreach (var inputMatrixPair in availableTieLinePairsForInputMatrix)
					{
						// get the input from the other matrix (used as hop)
						var otherMatrixInput = inputMatrixPair.input;

						foreach (var outputMatrixPair in availableTieLinePairsForOutputMatrix)
						{
							// get the output from the other matrix (used as hop)
							var otherMatrixOutput = outputMatrixPair.output;

							// only merge in case the input and output are from the same matrix
							if (otherMatrixInput.IsResourceFromSameElementAs(otherMatrixOutput))
							{
								routingResourceChains.Add(new RoutingResourceChain
								{
									IsValid = true,
									FirstMatrixRequired = true,
									FirstMatrix = new Matrix
									{
										Input = input,
										Output = inputMatrixPair.output,
									},
									ConnectingMatrixRequired = true,
									ConnectingMatrix = new Matrix
									{
										Input = otherMatrixInput,
										Output = otherMatrixOutput,
									},
									LastMatrixRequired = true,
									LastMatrix = new Matrix
									{
										Input = outputMatrixPair.input,
										Output = output
									},
								});
							}
						}
					}
				}
			}

			LogMethodCompleted(nameof(RetrieveRoutingResourceChainsWithThreeHops), stopWatch);

			return routingResourceChains.Distinct().ToList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S1541:Methods and properties should not be too complex", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S103:Lines should not be too long", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		private List<RoutingResourceChain> RetrieveRoutingResourceChainsWithTwoHops(FunctionResource inputMatrixInputResource = null, FunctionResource outputMatrixOutputResource = null, bool filterForFixedTieLine = true)
		{
			Log(nameof(RetrieveRoutingResourceChainsWithTwoHops), $"Retrieving routing resource chains between input matrix input resource '{inputMatrixInputResource?.Name}' and output matrix output resource '{outputMatrixOutputResource?.Name}'");

			if (availableMatrixInputSdiResources is null || availableMatrixOutputSdiResources is null)
			{
				GetAvailableMatrixSdiInputsAndOutputs();
			}

			var routingResourceChains = new List<RoutingResourceChain>();

			bool retrieveTieLinesForSpecificInput = inputMatrixInputResource != null;
			bool retrieveTieLinesForSpecificOutput = outputMatrixOutputResource != null;

			var outputsFromInputMatrixWithoutAvailableConnectedInputFromOutputMatrix = new List<FunctionResource>(); // for logging/debug purposes

			bool endpointRequiresFixedTieLine = outputMatrixOutputResource != null && outputMatrixOutputResource.GetResourcePropertyBooleanValue(ResourcePropertyNames.RequiresSpecificTieLine);

			foreach (var input in availableMatrixInputSdiResources)
			{
				if (retrieveTieLinesForSpecificInput && input.GUID != inputMatrixInputResource.GUID) continue;

				var availableOutputsFromInputMatrix = availableMatrixOutputSdiResources.Where(o => o.IsResourceFromSameElementAs(input)).ToList();

				Log(nameof(RetrieveRoutingResourceChainsWithTwoHops), $"Filtering for fixed tie lines is {(filterForFixedTieLine ? "enabled" : "disabled")}");

				if (filterForFixedTieLine && !endpointRequiresFixedTieLine)
				{
					// do not filter here when the endpoint service requires a fixed tie line

					availableOutputsFromInputMatrix = FilterForFixedTieLinesForSource(input, availableOutputsFromInputMatrix);
				}

				Log(nameof(RetrieveRoutingResourceChainsWithTwoHops), $"Available outputs from input matrix: {string.Join(";", availableOutputsFromInputMatrix.Select(r => r.Name))}");

				var availableOutputsFromOtherMatrices = availableMatrixOutputSdiResources.Where(o => !o.IsResourceFromSameElementAs(input)).ToList();

				Log(nameof(RetrieveRoutingResourceChainsWithTwoHops), $"Available outputs from matrices other than input matrix: {string.Join(";", availableOutputsFromOtherMatrices.Select(r => r.Name))}");

				foreach (var outputFromOtherMatrix in availableOutputsFromOtherMatrices)
				{
					if (retrieveTieLinesForSpecificOutput && outputFromOtherMatrix.GUID != outputMatrixOutputResource.GUID) continue;

					var availableInputsFromOutputMatrix = availableMatrixInputSdiResources.Where(i => i.IsResourceFromSameElementAs(outputFromOtherMatrix)).ToList();

					if (filterForFixedTieLine && endpointRequiresFixedTieLine)
					{
						// endpoint fixed tie line has priority over source fixed tie line, so always filter here

						availableInputsFromOutputMatrix = FilterForFixedTieLinesForEndPoint(outputFromOtherMatrix, availableInputsFromOutputMatrix);
					}

					Log(nameof(RetrieveRoutingResourceChainsWithTwoHops), $"Available inputs from output matrix: {string.Join(";", availableInputsFromOutputMatrix.Select(r => r.Name))}");

					foreach (var availableOutputFromInputMatrix in availableOutputsFromInputMatrix)
					{
						var inputsFromOutputMatrixConnectedToOutputFromInputMatrix = FilterInputsConnectedToOutput(availableOutputFromInputMatrix, availableInputsFromOutputMatrix);

						if (!inputsFromOutputMatrixConnectedToOutputFromInputMatrix.Any()) outputsFromInputMatrixWithoutAvailableConnectedInputFromOutputMatrix.Add(availableOutputFromInputMatrix);

						foreach (var availableInputFromOutputMatrix in inputsFromOutputMatrixConnectedToOutputFromInputMatrix)
						{
							routingResourceChains.Add(new RoutingResourceChain
							{
								IsValid = true,
								FirstMatrixRequired = true,
								FirstMatrix = new Matrix
								{
									Input = input,
									Output = availableOutputFromInputMatrix
								},
								LastMatrixRequired = true,
								LastMatrix = new Matrix
								{
									Input = availableInputFromOutputMatrix,
									Output = outputFromOtherMatrix
								},
							});
						}
					}
				}
			}

			Log(nameof(RetrieveRoutingResourceChainsWithTwoHops), $"Outputs from the input matrix without available connected inputs from output matrix: '{string.Join(";", outputsFromInputMatrixWithoutAvailableConnectedInputFromOutputMatrix.Select(r => r.Name))}'");

			return routingResourceChains.Distinct().ToList();
		}

		/// <summary>
		/// Returns a filtered list based on resource properties and capabilities for fixed tie lines.
		/// </summary>
		/// <param name="inputMatrixInputResource">The input resource from the input matrix.</param>
		/// <param name="availableOutputsFromInputMatrix">All output resources from the input matrix to be filtered.</param>
		/// <returns>A new filtered list of resources.</returns>
		private List<FunctionResource> FilterForFixedTieLinesForSource(FunctionResource inputMatrixInputResource, List<FunctionResource> availableOutputsFromInputMatrix)
		{
			List<FunctionResource> filteredAvailableOutputsFromInputMatrix;

			bool inputRequiresSpecificTieLine = inputMatrixInputResource.GetResourcePropertyBooleanValue(ResourcePropertyNames.RequiresSpecificTieLine);
			if (inputRequiresSpecificTieLine)
			{
				// only keep the resources that are specifically connected to the input. These are fixed tie line resources for this input

				filteredAvailableOutputsFromInputMatrix = availableOutputsFromInputMatrix.Where(r => r.HasCapabilityDiscreetValue(ProfileParameterGuids.FixedTieLineSource, inputMatrixInputResource.Name)).ToList();

				var removedOutputsFromInputMatrix = availableOutputsFromInputMatrix.Except(filteredAvailableOutputsFromInputMatrix).ToList();

				Log(nameof(FilterForFixedTieLinesForSource), $"Removed non fixed tie line resources and fixed tie line resources not connected to {inputMatrixInputResource.Name}: '{string.Join(", ", removedOutputsFromInputMatrix.Select(r => r.Name))}'");
			}
			else
			{
				// only keep the resources that
				// - are not part of fixed tie lines
				// - are already in use by the order (to allow reusing existing tie lines)

				var inUseOutputResources = liveVideoOrder.GetRoutingServiceChainsConnectedToSameSourceAs(OutputService).SelectMany(x => x.AllRoutingServices.Select(y => y.MatrixOutputSdi)).ToList();
				Log(nameof(FilterForFixedTieLinesForSource), $"Currently used matrix output resources {string.Join(", ", inUseOutputResources.Select(r => r?.Name))}");

				filteredAvailableOutputsFromInputMatrix = availableOutputsFromInputMatrix.Where(r => !r.GetResourcePropertyBooleanValue(ResourcePropertyNames.IsSpecificTieLine) || inUseOutputResources.Contains(r)).ToList();

				var removedResources = availableOutputsFromInputMatrix.Except(filteredAvailableOutputsFromInputMatrix).ToList();

				Log(nameof(FilterForFixedTieLinesForSource), $"Removed fixed tie line resources {string.Join(", ", removedResources.Select(r => r.Name))}");
			}

			Log(nameof(FilterForFixedTieLinesForSource), $"Input matrix input resource {inputMatrixInputResource.Name} requires {(inputRequiresSpecificTieLine ? string.Empty : "no ")}specific tie line. Filtered input matrix output resources: '{string.Join(";", filteredAvailableOutputsFromInputMatrix.Select(r => r.Name))}'");

			return filteredAvailableOutputsFromInputMatrix;
		}

		/// <summary>
		/// Returns a filtered list based on resource properties and capabilities for fixed tie lines.
		/// </summary>
		/// <param name="outputMatrixOutputResource">The input resource from the input matrix.</param>
		/// <param name="availableInputsFromOutputMatrix">All output resources from the input matrix to be filtered.</param>
		/// <returns>A new filtered list of resources.</returns>
		private List<FunctionResource> FilterForFixedTieLinesForEndPoint(FunctionResource outputMatrixOutputResource, List<FunctionResource> availableInputsFromOutputMatrix)
		{
			List<FunctionResource> filteredAvailableInputsFromOutputMatrix;

			bool outputRequiresSpecificTieLine = outputMatrixOutputResource.GetResourcePropertyBooleanValue(ResourcePropertyNames.RequiresSpecificTieLine);
			if (outputRequiresSpecificTieLine)
			{
				// only keep the resources that are specifically connected to the output. These are fixed tie line resources for this output

				filteredAvailableInputsFromOutputMatrix = availableInputsFromOutputMatrix.Where(r => r.HasCapabilityDiscreetValue(ProfileParameterGuids.FixedTieLineSource, outputMatrixOutputResource.Name)).ToList();

				var removedInputsFromOutputMatrix = availableInputsFromOutputMatrix.Except(filteredAvailableInputsFromOutputMatrix).ToList();

				Log(nameof(FilterForFixedTieLinesForEndPoint), $"Removed non fixed tie line resources and fixed tie line resources not connected to {outputMatrixOutputResource.Name}: '{string.Join(", ", removedInputsFromOutputMatrix.Select(r => r.Name))}'");
			}
			else
			{
				// only keep the resources that are not part of fixed tie lines and are already in use by the order (reuse existing tie lines)
				var inUseOutputResources = liveVideoOrder.GetRoutingServiceChainsConnectedToSameSourceAs(OutputService).SelectMany(x => x.AllRoutingServices.Select(y => y.MatrixOutputSdi)).ToList();
				Log(nameof(FilterForFixedTieLinesForEndPoint), $"Currently used matrix output resources {string.Join(", ", inUseOutputResources.Select(r => r?.Name))}");

				filteredAvailableInputsFromOutputMatrix = availableInputsFromOutputMatrix.Where(r => !r.GetResourcePropertyBooleanValue(ResourcePropertyNames.IsSpecificTieLine) || inUseOutputResources.Contains(r)).ToList();

				var removedResources = availableInputsFromOutputMatrix.Except(filteredAvailableInputsFromOutputMatrix).ToList();

				Log(nameof(FilterForFixedTieLinesForEndPoint), $"Removed fixed tie line resources {string.Join(", ", removedResources.Select(r => r.Name))}");
			}

			Log(nameof(FilterForFixedTieLinesForEndPoint), $"Output matrix output resource {outputMatrixOutputResource.Name} requires {(outputRequiresSpecificTieLine ? string.Empty : "no ")}specific tie line. Filtered output matrix input resources: '{string.Join(";", filteredAvailableInputsFromOutputMatrix.Select(r => r.Name))}'");

			return filteredAvailableInputsFromOutputMatrix;
		}

		private static List<InputOutputPair> RetrieveAvailableTieLineInputAndOutputPairs(IEnumerable<FunctionResource> inputs, IEnumerable<FunctionResource> outputs)
		{
			var pairs = new List<InputOutputPair>();

			foreach (var output in outputs)
			{
				var filteredInputs = FilterInputsConnectedToOutput(output, inputs);
				foreach (var input in filteredInputs) pairs.Add(new InputOutputPair(input, output));
			}

			return pairs;
		}

		private static List<FunctionResource> FilterInputsConnectedToOutput(FunctionResource output, IEnumerable<FunctionResource> availableInputs)
		{
			// ResourceOutputConnections capability is used to indicate what resource this output is connected to
			var outputCapability = output.Capabilities.FirstOrDefault(c => c.CapabilityProfileID == ProfileParameterGuids.ResourceOutputConnectionsSdi);
			if (outputCapability?.Value.Discreets == null) return new List<FunctionResource>();

			var filteredInputs = new List<FunctionResource>();

			// get the eligible input resources where the name is included in the capabilities of the output
			var eligibleInputs = availableInputs.Where(r => r?.Name != null && outputCapability.Value.Discreets.Contains(r.Name));
			foreach (var input in eligibleInputs)
			{
				// ResourceInputConnections capability is used to indicate what resource this input is connected to
				var inputCapability = input.Capabilities.FirstOrDefault(c => c.CapabilityProfileID == ProfileParameterGuids.ResourceInputConnectionsSdi);
				if (inputCapability?.Value.Discreets == null) continue;

				// if input also links the output as a possible capability then this is a valid connection
				if (inputCapability.Value.Discreets.Contains(output.Name)) filteredInputs.Add(input);
			}

			return filteredInputs;
		}

		private void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers.Log(nameof(RoutingServiceChain), nameOfMethod, message, nameOfObject);
		}

		private void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch, string nameOfObject = null)
		{
			helpers.LogMethodStart(nameof(RoutingServiceChain), nameOfMethod, out stopwatch, nameOfObject);
		}

		private void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch = null)
		{
			helpers.LogMethodCompleted(nameof(RoutingServiceChain), nameOfMethod, null, stopwatch);
		}
	}
}