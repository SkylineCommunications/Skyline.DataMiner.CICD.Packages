namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceConfigurations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reservations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.AssignProfilesAndResources;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.ReservationAction;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Time;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using DataMinerInterface = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface.DataMinerInterface;
	using Function = Function.Function;
	using ReservationNotFoundException = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ReservationNotFoundException;
	using ServiceDefinition = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition.ServiceDefinition;
	using ServiceDefinitionNotFoundException = Exceptions.ServiceDefinition.ServiceDefinitionNotFoundException;
	using ServiceNotFoundException = Exceptions.ServiceNotFoundException;
	using SrmProperty = Skyline.DataMiner.Library.Solutions.SRM.Model.Properties.Property;
	using VirtualPlatform = ServiceDefinition.VirtualPlatform;

	public class ServiceManager : IServiceManager
	{
		public static readonly string SourceServiceSystemFunctionId = "c7e8648e-7522-4724-99a8-74e48a45f380";

		public static readonly string TransmissionServiceSystemFunctionId = "147f77f6-74d6-4802-8603-5042a6e0ad5d";

		private static readonly TimeSpan PlasmaServicePreRoll = TimeSpan.Zero;

		private static readonly TimeSpan PlasmaServicePostRoll = TimeSpan.Zero;

		private static readonly TimeSpan FeenixServicePreRoll = TimeSpan.Zero;

		private static readonly TimeSpan FeenixServicePostRoll = TimeSpan.Zero;

		private static readonly TimeSpan MessiLiveRecordingPreRoll = TimeSpan.FromMinutes(5);

		private static readonly TimeSpan MessiNewsRecordingPreRoll = TimeSpan.Zero;

		private static readonly TimeSpan MessiLiveRecordingPostRoll = TimeSpan.FromMinutes(5);

		private static readonly TimeSpan MessiNewsRecordingPostRoll = TimeSpan.Zero;

		private static readonly Dictionary<VirtualPlatform, TimeSpan> preRollDurations = new Dictionary<VirtualPlatform, TimeSpan>
		{
			{ VirtualPlatform.ReceptionSatellite, TimeSpan.FromMinutes(30) },
			{ VirtualPlatform.ReceptionFiber, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.ReceptionMicrowave, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.ReceptionIp, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.ReceptionLiveU, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.ReceptionFixedLine, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.ReceptionFixedService, TimeSpan.FromMinutes(0) },
			{ VirtualPlatform.Routing, new TimeSpan(hours: 0, minutes: 15, seconds: 0) },
			{ VirtualPlatform.Recording, new TimeSpan(hours: 0, minutes: 30, seconds: 0) },
			{ VirtualPlatform.VideoProcessing, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.AudioProcessing, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.GraphicsProcessing, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.TransmissionSatellite, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.TransmissionIp, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.TransmissionLiveU, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.TransmissionMicrowave, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.TransmissionEurovision, TimeSpan.FromMinutes(30) },
			{ VirtualPlatform.TransmissionFiber, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.Destination, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.VizremStudio, TimeSpan.FromMinutes(15) },
			{ VirtualPlatform.VizremFarm, TimeSpan.FromMinutes(15) },
			{ VirtualPlatform.VizremNC2Converter, TimeSpan.FromMinutes(15) },
		};

		private static readonly Dictionary<VirtualPlatform, TimeSpan> postRollDurations = new Dictionary<VirtualPlatform, TimeSpan>
		{
			{ VirtualPlatform.ReceptionSatellite, TimeSpan.FromMinutes(30) },
			{ VirtualPlatform.ReceptionFiber, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.ReceptionMicrowave, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.ReceptionIp, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.ReceptionLiveU, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.ReceptionFixedLine, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.ReceptionFixedService, TimeSpan.FromMinutes(0) },
			{ VirtualPlatform.Routing, TimeSpan.FromMinutes(30) },
			{ VirtualPlatform.Recording, TimeSpan.FromMinutes(30) },
			{ VirtualPlatform.VideoProcessing, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.AudioProcessing, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.GraphicsProcessing, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.TransmissionSatellite, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.TransmissionIp, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.TransmissionLiveU, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.TransmissionMicrowave, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.TransmissionEurovision, TimeSpan.FromMinutes(30) },
			{ VirtualPlatform.TransmissionFiber, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.Destination, TimeSpan.FromMinutes(5) },
			{ VirtualPlatform.VizremStudio, TimeSpan.FromMinutes(15) },
			{ VirtualPlatform.VizremFarm, TimeSpan.FromMinutes(15) },
			{ VirtualPlatform.VizremNC2Converter, TimeSpan.FromMinutes(15) },
		};

		private readonly IEngine engine;

		public ServiceManager(Helpers helpers)
		{
			Helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.engine = helpers.Engine;
		}

		public Helpers Helpers { get; }

		public bool TryGetService(Guid serviceId, out Service service)
		{
			try
			{
				service = GetService(serviceId);
				return true;
			}
			catch
			{
				service = null;
				return false;
			}
		}

		public Service GetService(Guid serviceId)
		{
			var reservationInstance = GetReservation(serviceId) ?? throw new ReservationNotFoundException(serviceId);

			var service = Service.FromReservationInstance(Helpers, reservationInstance);

			return service;
		}

		/// <summary>
		/// Retrieves the services part of the Reservation with the supplied ID.
		/// If the supplied ID is empty or non-existent an empty list will be returned.
		/// </summary>
		/// <param name="orderId">ID of the order.</param>
		/// <returns>List of services part of the order.</returns>
		public List<Service> GetOrderServices(Guid orderId)
		{
			List<Service> services = new List<Service>();
			if (orderId.Equals(Guid.Empty)) return services;

			ReservationInstance reservationInstance = GetReservation(orderId);
			if (reservationInstance == null) return services;

			ServiceReservationInstance serviceReservationInstance = reservationInstance as ServiceReservationInstance ?? throw new InvalidOperationException(String.Format("Reservation with id: {0} is not a ServiceReservationInstance", orderId));

			services = GetOrderServices(serviceReservationInstance);
			return services;
		}

		/// <summary>
		/// Gets the Service objects based on the Order reservation.
		/// </summary>
		/// <param name="orderReservationInstance"></param>
		/// <param name="forceReservationInstanceToOverwriteServiceConfiguration">If true, services will be created based on their reservation. If false, a property in the service configuration will be checked to determine if services should be created based on service config or reservation.</param>
		/// <param name="orderServiceDefinition">Retrieved order service definition.</param>
		public List<Service> GetOrderServices(ServiceReservationInstance orderReservationInstance, bool forceReservationInstanceToOverwriteServiceConfiguration = false, Net.ServiceManager.Objects.ServiceDefinition orderServiceDefinition = null)
		{
			if (orderReservationInstance == null) throw new ArgumentNullException(nameof(orderReservationInstance));

			LogMethodStarted(nameof(GetOrderServices), out var stopwatch);

			orderServiceDefinition = orderServiceDefinition ?? DataMinerInterface.ServiceManager.GetServiceDefinition(Helpers, orderReservationInstance.ServiceDefinitionID) ?? throw new ServiceDefinitionNotFoundException($"Unable to find service definition {orderReservationInstance.ServiceDefinitionID}");

			if (!Helpers.OrderManagerElement.TryGetServiceConfigurations(orderReservationInstance.ID, out var orderServiceConfigurations))
			{
				Log(nameof(GetOrderServices), $"Couldn't find any service configuration for order: {orderReservationInstance.Name}");
				throw new ServiceConfigurationNotFoundException(orderReservationInstance.ID);
			}

			var serviceDefinitionNodeIds = orderServiceDefinition.Diagram.Nodes.Select(node => node.ID).ToList();
			var serviceConfigurationNodeIds = orderServiceConfigurations.Keys;
			var serviceConfigurationNodeIdsNotPresentInServiceDefinition = serviceConfigurationNodeIds.Except(serviceDefinitionNodeIds).ToList();
			bool nodeIdsMismatch = serviceConfigurationNodeIdsNotPresentInServiceDefinition.Any();
			if (nodeIdsMismatch) throw new NodeIdMismatchException($"Service configuration nodes '{string.Join(", ", serviceConfigurationNodeIdsNotPresentInServiceDefinition)}' cannot be found in the service definition");

			var orderResourceUsageDefinitions = orderReservationInstance.ResourcesInReservationInstance.Select(r => r as ServiceResourceUsageDefinition).Where(r => r != null).ToList();
			Log(nameof(GetOrderServices), $"Resources in order reservation: '{string.Join(";", orderResourceUsageDefinitions.Select(x => $"Node {x.ServiceDefinitionNodeID} = {x.NodeConfiguration.ResourceID}"))}'");

			var services = new List<Service>();

			foreach (var node in orderServiceDefinition.Diagram.Nodes)
			{
				var service = BuildService(forceReservationInstanceToOverwriteServiceConfiguration, orderServiceConfigurations, orderResourceUsageDefinitions, node);

				services.Add(service);
			}

			foreach (var edge in orderServiceDefinition.Diagram.Edges)
			{
				var fromService = GetAllServices(services).SingleOrDefault(s => s.NodeId == edge.FromNodeID) ?? throw new ServiceNotFoundException($"Unable to find service with Node ID {edge.FromNodeID}", true);
				var toService = services.SingleOrDefault(s => s.NodeId == edge.ToNodeID) ?? throw new ServiceNotFoundException($"Unable to find service with Node ID {edge.ToNodeID}", true);

				fromService.Children.Add(toService);

				services.Remove(toService);
			}

			Log(nameof(GetOrderServices), "Found services: " + string.Join(";", OrderManager.FlattenServices(services).Select(x => x.Name)));

			LogMethodCompleted(nameof(GetOrderServices), null, stopwatch);

			return services;
		}

		private Service BuildService(bool forceReservationInstanceToOverwriteServiceConfiguration, Dictionary<int, ServiceConfiguration> orderServiceConfigurations, List<ServiceResourceUsageDefinition> orderResourceUsageDefinitions, Net.ServiceManager.Objects.Node node)
		{
			if (!orderServiceConfigurations.TryGetValue(node.ID, out var serviceConfiguration)) throw new ArgumentException($"Unable to find service configuration for node ID {node.ID}", nameof(orderServiceConfigurations));

			LogMethodStarted(nameof(BuildService), out var stopwatch);

			var service = new DisplayedService(Helpers, node.ID, node.Label, serviceConfiguration);

			// check if the service is booked
			var serviceResourceUsageDefinition = orderResourceUsageDefinitions.FirstOrDefault(s => s.ServiceDefinitionNodeID == node.ID);

			var serviceReservation = FindReservationInstance(serviceResourceUsageDefinition, serviceConfiguration);

			if (serviceReservation != null)
			{
				Log(nameof(GetOrderServices), $"A reservation instance with ID {serviceReservation.ID} and name {serviceReservation.Name} exists for node {node.ID}");
				UpdateServiceWithReservationInstance(service, serviceReservation, forceReservationInstanceToOverwriteServiceConfiguration);
			}

			LogMethodCompleted(nameof(BuildService), service.Name, stopwatch);

			return service;
		}

		private void UpdateServiceWithReservationInstance(Service service, ReservationInstance serviceReservation, bool forceReservationInstanceToOverwriteServiceConfiguration)
		{
			service.Id = serviceReservation.ID; // Fallback to make sure service configuration and reservation are in sync
			service.IsBooked = true;
			service.IsPreliminary = false;

			var contributingResource = DataMinerInterface.ResourceManager.GetResource(Helpers, serviceReservation.ID);
			service.ContributingResource = contributingResource;
			service.ResourcePool = DataMinerInterface.ResourceManager.GetResourcePool(Helpers, contributingResource.PoolGUIDs.FirstOrDefault());

			// These properties are ALWAYS taken from reservationInstance because they are not available in ServiceConfiguration
			service.HasResourcesAssigned = serviceReservation.ResourcesInReservationInstance.Any();
			service.UserTasks = Helpers.UserTaskManager.GetUserTasks(service).ToList();
			service.OrderReferences = serviceReservation.GetOrderReferences();
			service.LinkedServiceId = serviceReservation.GetLinkedServiceId();
			service.SecurityViewIds = new HashSet<int>(serviceReservation.SecurityViewIDs);

			Log(nameof(GetOrderServices), "Updated service object with properties from reservation instance (default part)", service.Name);

			if (forceReservationInstanceToOverwriteServiceConfiguration)
			{
				ForceUpdateServiceWithReservationInstance(service, serviceReservation);
			}
		}

		private void ForceUpdateServiceWithReservationInstance(Service service, ReservationInstance serviceReservation)
		{
			var bookedService = Service.FromReservationInstance(Helpers, serviceReservation);

			// only retrieve resource and function parameters from the reservation when the reservation object itself is needed
			// this is typically only the case to verify the order changes (from LiveOrderForm or UpdateService script) with the existing one
			// the scripts only update the service configuration and not the reservationInstance itself

			Log(nameof(GetOrderServices), "Updating functions with values from reservation instance", service.Name);

			foreach (var bookedFunction in bookedService.Functions)
			{
				var function = service.Functions.FirstOrDefault(f => f.NodeId == bookedFunction.NodeId);
				if (function == null)
				{
					function = bookedFunction;
					service.Functions.Add(bookedFunction);
				}
				else
				{
					UpdateFunctionWithBookedFunction(function, bookedFunction);
				}

				Log(nameof(GetOrderServices), $"Function summary: {function.Configuration}");
			}
		}

		private void UpdateFunctionWithBookedFunction(Function function, Function bookedFunction)
		{
			function.Resource = bookedFunction.Resource;

			foreach (var bookedParameter in bookedFunction.Parameters)
			{
				var functionParameter = function.Parameters.FirstOrDefault(p => p.Id == bookedParameter.Id);
				if (functionParameter == null) function.Parameters.Add(bookedParameter);
				else functionParameter.Value = bookedParameter.Value;
			}

			foreach (var bookedInterfaceParameter in bookedFunction.InterfaceParameters)
			{
				var functionInterfaceParameter = function.InterfaceParameters.FirstOrDefault(p => p.Id == bookedInterfaceParameter.Id);
				if (functionInterfaceParameter == null) function.InterfaceParameters.Add(bookedInterfaceParameter);
				else functionInterfaceParameter.Value = bookedInterfaceParameter.Value;
			}
		}

		/// <summary>
		/// Tries to find the <see cref="ReservationInstance"/> linked to the given <paramref name="serviceResourceUsageDefinition"/> or <paramref name="serviceConfiguration"/> (in that order).
		/// </summary>
		/// <remarks>Looking for the reservation based on the configuration is a fall-back mechanism to avoid wrong values for the IsBooked and Id property of the Service object.</remarks>
		private ReservationInstance FindReservationInstance(ServiceResourceUsageDefinition serviceResourceUsageDefinition, ServiceConfiguration serviceConfiguration)
		{
			if (TryGetReservationBasedOnResourceUsageDefinition(serviceResourceUsageDefinition, out var reservation)) return reservation;

			if (TryGetReservationBasedOnConfiguration(serviceConfiguration, out reservation)) return reservation;

			return null;
		}

		/// <summary>
		/// Tries to find a <see cref="ReservationInstance"/> with an ID or name equal to the ID or name of the <paramref name="serviceConfiguration"/>.
		/// </summary>
		private bool TryGetReservationBasedOnConfiguration(ServiceConfiguration serviceConfiguration, out ReservationInstance reservation)
		{
			if (serviceConfiguration == null)
			{
				Log(nameof(TryGetReservationBasedOnConfiguration), $"{nameof(serviceConfiguration)} is null, unable to find reservation.");

				reservation = null;
				return false;
			}

			reservation = DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, serviceConfiguration.Id);
			if (reservation != null)
			{
				Log(nameof(TryGetReservationBasedOnConfiguration), $"Found reservation based on {nameof(serviceConfiguration)} ID {serviceConfiguration.Id}.");
				return true;
			}

			reservation = DataMinerInterface.ResourceManager.GetReservationInstances(Helpers, ReservationInstanceExposers.Name.Equal(serviceConfiguration.Name)).FirstOrDefault();

			bool foundReservation = reservation != null;

			if (foundReservation)
			{
				Log(nameof(TryGetReservationBasedOnConfiguration), $"Found reservation based on {nameof(serviceConfiguration)} name {serviceConfiguration.Name}.");
			}
			else
			{
				Log(nameof(TryGetReservationBasedOnConfiguration), $"Found no reservation with ID {serviceConfiguration.Id} or name {serviceConfiguration.Name}.");
			}

			return foundReservation;
		}

		/// <summary>
		/// Tries to find a <see cref="ReservationInstance"/> with an ID equal to the ID of the <paramref name="serviceResourceUsageDefinition"/>.
		/// </summary>
		private bool TryGetReservationBasedOnResourceUsageDefinition(ServiceResourceUsageDefinition serviceResourceUsageDefinition, out ReservationInstance reservation)
		{
			if (serviceResourceUsageDefinition == null)
			{
				Log(nameof(TryGetReservationBasedOnResourceUsageDefinition), $"{nameof(serviceResourceUsageDefinition)} is null, unable to find reservation.");
				reservation = null;
				return false;
			}

			reservation = DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, serviceResourceUsageDefinition.GUID);

			bool foundReservation = reservation != null;

			Log(nameof(TryGetReservationBasedOnResourceUsageDefinition), $"Found {(foundReservation ? string.Empty : "no ")}reservation based on {nameof(serviceResourceUsageDefinition)} ID {serviceResourceUsageDefinition.GUID}.");

			return foundReservation;
		}

		/// <summary>
		/// Updates the timing on the given Service.
		/// </summary>
		/// <param name="service">The service for which the timing needs to be updated.</param>
		/// <returns>A boolean indicating if the update was successful.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ReservationNotFoundException"/>
		public bool TryChangeServiceTime(Service service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			var reservation = GetReservation(service.Id) ?? throw new ReservationNotFoundException(service.Name);
			if (reservation.Status == ReservationStatus.Ended)
			{
				Helpers?.Log(nameof(OrderManager), nameof(TryChangeServiceTime), $"Service has already been ended for a while (Reservation status: {reservation.Status.GetDescription()}), no timing update allowed");
				return false;
			}

			var bookingManager = new BookingManager((Engine)engine, engine.FindElement(service.Definition.BookingManagerElementName));

			var changeTimeInputData = new ChangeTimeInputData
			{
				// Start time may not be changed of an ongoing service.
				StartDate = service.IsOrShouldBeRunning || reservation.GetBookingLifeCycle() == GeneralStatus.Running ? reservation.Start.FromReservation().Add(reservation.GetPreRoll()) : service.Start,
				PreRoll = service.IsOrShouldBeRunning || reservation.GetBookingLifeCycle() == GeneralStatus.Running ? reservation.GetPreRoll() : service.PreRoll,
				EndDate = service.End,
				PostRoll = service.PostRoll,
				IsSilent = true
			};

			if (changeTimeInputData.StartDate.Kind != DateTimeKind.Local)
			{
				changeTimeInputData.StartDate = new DateTime(changeTimeInputData.StartDate.Ticks, DateTimeKind.Local);
				Log(nameof(TryChangeServiceTime), $"Had to change start date argument to local time: {changeTimeInputData.StartDate.ToFullDetailString()}", service.Name);
			}

			if (changeTimeInputData.EndDate.Kind != DateTimeKind.Local)
			{
				changeTimeInputData.EndDate = new DateTime(changeTimeInputData.EndDate.Ticks, DateTimeKind.Local);
				Log(nameof(TryChangeServiceTime), $"Had to change end date argument to local time: {changeTimeInputData.EndDate.ToFullDetailString()}", service.Name);
			}

			Log(nameof(TryChangeServiceTime), $"Changing timing to {changeTimeInputData.ToString()}", service.Name);

			try
			{
				if (service.IsOrShouldBeRunning)
				{
					Log(nameof(TryChangeServiceTime), $"Service should be running, double checking for SRM status {GeneralStatus.Running}", service.Name);

					service.WaitForSrmStatus(Helpers, ReservationStatus.Ongoing, out reservation);
				}

				reservation = DataMinerInterface.BookingManager.ChangeTime(Helpers, bookingManager, reservation, changeTimeInputData);

				Log(nameof(TryChangeServiceTime), $"WARNING: Reservation has{(reservation.IsQuarantined ? string.Empty : "not ")} gone into Quarantined state");

				return true;
			}
			catch (Exception ex)
			{
				Log(nameof(TryChangeServiceTime), $"Exception occurred: {ex}", service.Name);
				return false;
			}
		}

		public TimeRangeUtc GetOverwrittenTimeRange(Service service)
		{
			var window = Helpers.OrderManagerElement.GetServiceResourceAllocationWindow(service.Definition.Name, service.StartWithPreRoll, service.EndWithPostRoll);

			Log(nameof(GetOverwrittenTimeRange), $"Overwritten time range for service definition {service.Definition.Name}, start={service.StartWithPreRoll.ToFullDetailString()}, end={service.EndWithPostRoll.ToFullDetailString()} is {window}");

			return window;
		}

		public bool TryExtendService(Service service, TimeSpan timeToAdd)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));
			if (timeToAdd <= TimeSpan.Zero) throw new ArgumentException("Time to add must be positive", nameof(timeToAdd));

			var reservation = GetReservation(service.Id);
			if (reservation == null) throw new ReservationNotFoundException(service.Name);

			var bookingManager = new BookingManager((Engine)engine, engine.FindElement(service.Definition.BookingManagerElementName));

			var extendInputData = new ExtendBookingInputData
			{
				TimeToAdd = timeToAdd,
				IsSilent = true
			};

			return DataMinerInterface.BookingManager.TryExtend(Helpers, bookingManager, ref reservation, extendInputData);
		}

		public bool TryUpdateResources(Service service, List<Function> functionsToUpdate)
		{
			return TryUpdateResourcesAndOrProfileParameters(service, functionsToUpdate, ResourceAndProfileParameterUpdateOptions.Resources);
		}

		public bool TryUpdateProfileParameters(Service service, List<Function> functionsToUpdate)
		{
			return TryUpdateResourcesAndOrProfileParameters(service, functionsToUpdate, ResourceAndProfileParameterUpdateOptions.ProfileParameters);
		}

		public bool TryUpdateResourcesAndProfileParameters(Service service, List<Function> functionsToUpdate)
		{
			return TryUpdateResourcesAndOrProfileParameters(service, functionsToUpdate, ResourceAndProfileParameterUpdateOptions.Resources | ResourceAndProfileParameterUpdateOptions.ProfileParameters);
		}

		public bool TryReleaseResources(Service service, List<Function> functionsToUpdate)
		{
			return TryUpdateResourcesAndOrProfileParameters(service, functionsToUpdate, ResourceAndProfileParameterUpdateOptions.Resources | ResourceAndProfileParameterUpdateOptions.SkipResourceIsAvailableCheck);
		}

		private bool TryUpdateResourcesAndOrProfileParameters(Service service, List<Function> functionsToUpdate, ResourceAndProfileParameterUpdateOptions options)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));
			if (functionsToUpdate == null) throw new ArgumentNullException(nameof(functionsToUpdate));

			LogMethodStarted(nameof(TryUpdateResourcesAndOrProfileParameters), out var stopwatch, service.Name);

			try
			{
				var reservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, service.Id) as ServiceReservationInstance;
				if (reservationInstance == null)
				{
					Log(nameof(TryUpdateResourcesAndOrProfileParameters), "ReservationInstance could not be retrieved", service.Name);
					return false;
				}

				List<AssignResourceRequest> requests = DetermineAssignResourceRequests(service, functionsToUpdate, options);

				Log(nameof(TryUpdateResourcesAndOrProfileParameters), "Setting resources to " + string.Join(";", functionsToUpdate.Select(f => $"{f.Definition.Label}:{f.Resource?.Name}")), service.Name);

				reservationInstance.AssignResources((Engine)engine, requests.ToArray());

				LogMethodCompleted(nameof(TryUpdateResourcesAndOrProfileParameters), service.Name);

				return true;
			}
			catch (Exception e)
			{
				Log(nameof(TryUpdateResourcesAndOrProfileParameters), $"Something went wrong: {e}", service.Name);
				LogMethodCompleted(nameof(TryUpdateResourcesAndOrProfileParameters), service.Name);
				return false;
			}
		}

		private List<AssignResourceRequest> DetermineAssignResourceRequests(Service service, List<Function> functionsToUpdate, ResourceAndProfileParameterUpdateOptions options)
		{
			var availableResourcesPerFunction = new Dictionary<string, HashSet<FunctionResource>>();
			if (options.HasFlag(ResourceAndProfileParameterUpdateOptions.Resources) && !options.HasFlag(ResourceAndProfileParameterUpdateOptions.SkipResourceIsAvailableCheck))
			{
				availableResourcesPerFunction = service.GetAvailableResourcesPerFunctionBasedOnTiming(Helpers);
			}

			var requests = new List<AssignResourceRequest>();

			foreach (var function in functionsToUpdate)
			{
				var request = new AssignResourceRequest
				{
					TargetNodeLabel = function.Definition.Label,
					NewResourceId = function.Resource?.ID ?? Guid.Empty
				};

				if (options.HasFlag(ResourceAndProfileParameterUpdateOptions.Resources) && !options.HasFlag(ResourceAndProfileParameterUpdateOptions.SkipResourceIsAvailableCheck))
				{
					bool selectedResourceIsAvailable = IsSelectedResourceAvailable(service, availableResourcesPerFunction, function);
					if (!selectedResourceIsAvailable)
					{
						Log(nameof(TryUpdateResourcesAndOrProfileParameters), $"Function {function.Definition.Label} selected resource {function.ResourceName} is no longer available, setting to None", service.Name);
						function.Resource = null;
					}
				}

				if (options.HasFlag(ResourceAndProfileParameterUpdateOptions.ProfileParameters))
				{
					foreach (var parameter in function.Parameters)
					{
						var paramToUpdate = new Library.Solutions.SRM.Model.Parameter
						{
							Id = parameter.Id,
							Value = Convert.ToString(parameter.Value)
						};

						request.OverriddenParameters.Add(paramToUpdate);
					}
				}

				requests.Add(request);
			}

			return requests;
		}

		private bool IsSelectedResourceAvailable(Service service, Dictionary<string, HashSet<FunctionResource>> availableResourcesPerFunction, Function function)
		{
			if (availableResourcesPerFunction.TryGetValue(function.Definition.Label, out var availableResources))
			{
				return availableResources.Contains(function.Resource);
			}
			else
			{
				Log(nameof(TryUpdateResourcesAndOrProfileParameters), $"Eligible resources call did not return a collection for function {function.Name} {function.Id}", service.Name);
				return false;
			}
		}

		public void UpdateCapabilities(Service service)
		{
			var bookingManagerInfo = new BookingManagerInfo
			{
				Action = BookingOperationAction.Edit,
				Element = service.Definition.BookingManagerElementName,
				ServiceId = service.Id.ToString()
			};

			// Using most recent version SLSRMLibrary.dll
			var srmBookingConfiguration = new SrmBookingConfiguration(service.Id, bookingManagerInfo, (Engine)engine);

			foreach (var function in service.Functions.Where(f => f.Change.Summary is FunctionChangeSummary functionChangeSummary && functionChangeSummary.ProfileParameterChangeSummary.CapabilitiesChanged))
			{
				engine.Log(nameof(UpdateCapabilities) + "| Handling Function " + function.Name);

				var srmResourceUsageConfiguration = srmBookingConfiguration.GetResource(function.NodeId);

				engine.Log(nameof(UpdateCapabilities) + "| Found Resource " + srmResourceUsageConfiguration.Resource.Name);

				foreach (var changedCapability in function.Parameters.Where(p => p.IsCapability && p.Change.Summary.IsChanged))
				{
					engine.Log(nameof(UpdateCapabilities) + "| Handling changed capability " + changedCapability.Name);

					srmResourceUsageConfiguration.SetParameter(changedCapability.Id, changedCapability.StringValue);

					engine.Log("Function " + function.Name + " set capability " + changedCapability.Name + " to value " + changedCapability.StringValue);
				}
			}
		}

		/// <summary>
		/// Updates all custom properties for the given service.
		/// </summary>
		/// <param name="service">The service for which to update the custom properties.</param>
		/// <param name="order"></param>
		/// <returns>A boolean indicating if all custom properties have been successfully updated.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ReservationNotFoundException"/>
		public bool TryUpdateAllCustomProperties(Service service, Order order)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			var reservation = GetReservation(service.Id) ?? throw new ReservationNotFoundException(service.Name);

			var bookingManager = new BookingManager((Engine)engine, engine.FindElement(service.Definition.BookingManagerElementName));
			var customProperties = service.GetPropertiesForBooking(bookingManager.Properties, order, Helpers);

			var updatedProperties = new Dictionary<string, object>();
			foreach (var property in customProperties)
			{
				if (String.IsNullOrEmpty(property.Value)) continue;
				updatedProperties[property.Name] = property.Value;
			}

			try
			{
				if (updatedProperties.Any()) DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(Helpers, reservation, updatedProperties);
				return true;
			}
			catch (Exception e)
			{
				Log(nameof(TryUpdateAllCustomProperties), $"Failed to add or update properties for service {service.Name}: {e}");
				return false;
			}
		}

		public void AddOrUpdateServiceReservation(Service service, Order order)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));
			if (order == null) throw new ArgumentNullException(nameof(order));

			LogMethodStarted(nameof(AddOrUpdateServiceReservation), out var stopwatch, service.Name);

			var bookingManager = new BookingManager((Engine)engine, engine.FindElement(service.Definition.BookingManagerElementName)) { CustomProperties = true, CustomEvents = true };
			var bookingAction = service.IsBooked ? BookingOperationAction.Edit : BookingOperationAction.New;

			var input = new CreateOrEditBookingInput(Helpers, service, bookingManager, order);

			if (bookingAction == BookingOperationAction.New)
			{
				service.ReservationInstance = DataMinerInterface.BookingManager.CreateNewBooking(Helpers, bookingManager, input.BookingData, input.Functions, input.CustomEvents, input.Properties) as ServiceReservationInstance;
			}
			else
			{
				service.ReservationInstance = DataMinerInterface.BookingManager.EditBooking(Helpers, bookingManager, service.Id, input.BookingData, input.Functions, input.CustomEvents, input.Properties) as ServiceReservationInstance;
			}

			service.ReservationInstance = service.ReservationInstance.ChangeStateToConfirmedWithRetry(Helpers, bookingManager) as ServiceReservationInstance;

			if (service.LinkedServiceId != Guid.Empty) UpdateLinkedServiceIdProperty(service.ReservationInstance, order, service);

			service.Id = service.ReservationInstance.ID;
			service.IsBooked = true;

			service.SecurityViewIds = service.IsSharedSource ? service.SecurityViewIds : new HashSet<int>(order.SecurityViewIds);
			Log(nameof(AddOrUpdateServiceReservation), $"Setting SecurityViewIds to {string.Join(",", service.SecurityViewIds)}, based on SecurityViewIds of {(service.IsSharedSource ? "Shared Source" : "Order")}", service.Name);

			service.UpdateSecurityViewIds(Helpers, service.SecurityViewIds);

			// Update MCR Description for all child services if service is routing
			if (service.Definition?.VirtualPlatform == VirtualPlatform.Routing)
			{
				foreach (var childService in OrderManager.FlattenServices(service.Children))
				{
					UpdateMcrDescription(childService, order);
				}
			}

			LogMethodCompleted(nameof(AddOrUpdateServiceReservation), service.Name, stopwatch);
		}

		public void UpdateShortDescription(Service service, Order order)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));
			if (order == null) throw new ArgumentNullException(nameof(order));

			LogMethodStarted(nameof(UpdateShortDescription), out var stopwatch, service.Name);

			var reservation = service.ReservationInstance ?? DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, service.Id);
			if (reservation == null)
			{
				Log(nameof(UpdateShortDescription), "ReservationInstance could not be retrieved", service.Name);
				LogMethodCompleted(nameof(UpdateShortDescription), service.Name);
				return;
			}

			string shortDescriptionToSet = service.GetShortDescription(order).Clean(true);

			DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(Helpers, reservation, new Dictionary<string, object>
			{
				{ ServicePropertyNames.ShortDescription, shortDescriptionToSet}
			});

			Log(nameof(UpdateShortDescription), $"Successfully set {ServicePropertyNames.ShortDescription} property to '{shortDescriptionToSet}'", service.Name);

			LogMethodCompleted(nameof(UpdateShortDescription), service.Name, stopwatch);
		}

		public void UpdateMcrDescription(Service service, Order order)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));
			if (order == null) throw new ArgumentNullException(nameof(order));

			LogMethodStarted(nameof(UpdateMcrDescription), out var stopwatch, service.Name);

			var reservation = service.ReservationInstance ?? DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, service.Id);
			if (reservation == null)
			{
				Log(nameof(UpdateMcrDescription), "ReservationInstance could not be retrieved", service.Name);
				LogMethodCompleted(nameof(UpdateMcrDescription), service.Name);
				return;
			}

			string mcrDescription = service.GetMcrDescription(Helpers, order);
			Log(nameof(UpdateMcrDescription), $"Updating MCR Description property for service {service.Name} to {mcrDescription}", service.Name);
			DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(Helpers, reservation, new Dictionary<string, object>
			{
				{ ServicePropertyNames.McrDescription, mcrDescription }
			});

			LogMethodCompleted(nameof(UpdateMcrDescription), service.Name, stopwatch);
		}

		public void CancelService(Guid serviceGuid, Guid orderGuid)
		{
			ReservationInstance reservation = GetReservation(serviceGuid);
			if (reservation == null) throw new ReservationNotFoundException(serviceGuid);

			string bookingManagerElementName = GetServiceBookingManagerElementName(reservation);
			if (String.IsNullOrWhiteSpace(bookingManagerElementName))
				throw new BookingManagerNotFoundException(reservation.ID);

			var bookingManager = new BookingManager((Engine)engine, engine.FindElement(bookingManagerElementName));

			bool isSharedSource = reservation.GetBooleanProperty(ServicePropertyNames.IsGlobalEventLevelReceptionPropertyName);
			if (isSharedSource && ServiceIsUsedByOtherOrders(reservation, new[] { orderGuid }))
				throw new InvalidOperationException(String.Format("Shared Source Service {0} was not deleted as it is still being used by other Orders", reservation.Name));

			try
			{
				DataMinerInterface.BookingManager.Cancel(Helpers, bookingManager, reservation);
			}
			catch (PropertyUpdateException)
			{
				// Do nothing, this does not cause any issues when canceling a Service.
			}
		}

		public bool ServiceIsUsedByOtherOrders(Guid reservationInstanceId, IEnumerable<Guid> orderIdsToIgnore)
		{
			var reservationInstance = Helpers.ResourceManager.GetReservationInstance(reservationInstanceId);
			return ServiceIsUsedByOtherOrders(reservationInstance, orderIdsToIgnore);
		}

		public bool ServiceIsUsedByOtherOrders(ReservationInstance serviceReservation, IEnumerable<Guid> orderIdsToIgnore)
		{
			if (serviceReservation == null) throw new ArgumentException(nameof(serviceReservation));
			if (orderIdsToIgnore == null) throw new ArgumentNullException(nameof(orderIdsToIgnore));

			Log(nameof(ServiceIsUsedByOtherOrders), $"Searching for orders that use service {serviceReservation.Name}, ignoring: {String.Join(", ", orderIdsToIgnore)}");

			// Check for booked orders that use the Service resource as contributing resource
			var reservationsThatUseServiceResource = DataMinerInterface.ResourceManager.GetReservationInstances(Helpers, ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(serviceReservation.ID)).Where(x => !orderIdsToIgnore.Contains(x.ID));
			Log(nameof(ServiceIsUsedByOtherOrders), $"Service {serviceReservation.Name} is being used by {reservationsThatUseServiceResource.Count()} booked order(s) ({String.Join(", ", reservationsThatUseServiceResource.Select(x => x.ID))})");
			if (reservationsThatUseServiceResource.Any()) return true;

			// Use orderReferences to check for possible preliminary orders that use the Shared Source
			var possiblePreliminaryOrderReferences = GetOrderReferences(serviceReservation).Where(x => !reservationsThatUseServiceResource.Select(y => y.ID).Contains(x) && !orderIdsToIgnore.Contains(x));
			Log(nameof(ServiceIsUsedByOtherOrders), $"Service {serviceReservation.Name} is possibly being used by {possiblePreliminaryOrderReferences.Count()} preliminary order(s) ({String.Join(", ", possiblePreliminaryOrderReferences)})");
			if (!possiblePreliminaryOrderReferences.Any()) return false;

			foreach (Guid possiblePreliminaryOrderId in possiblePreliminaryOrderReferences)
			{
				// If preliminary order exists and uses the service -> return true
				try
				{
					var order = Helpers.OrderManager.GetOrder(possiblePreliminaryOrderId);
					if (order == null) continue;

					if (order.AllServices.Exists(x => x.Id.Equals(serviceReservation.ID)))
					{
						Log(nameof(ServiceIsUsedByOtherOrders), $"Service {serviceReservation.Name} is at least being used by preliminary order {order.Id}");
						return true;
					}
				}
				catch (ReservationNotFoundException)
				{
					// Order could not be retrieved
				}
			}

			return false;
		}

		public void DeleteService(Guid serviceId, Guid orderId, ReservationInstance alreadyRetrievedReservationInstance = null, bool skipCheckingIfServiceIsUsedByOtherOrders = false)
		{
			var reservation = alreadyRetrievedReservationInstance ?? GetReservation(serviceId);
			if (reservation == null)
			{
				Log(nameof(DeleteService), $"Reservation instance cannot be found {serviceId}");
				return;
			}

			bool isSharedSource = reservation.GetBooleanProperty(ServicePropertyNames.IsGlobalEventLevelReceptionPropertyName);
			if (isSharedSource && !SharedSourceCanBeRemoved(reservation, serviceId, orderId, skipCheckingIfServiceIsUsedByOtherOrders)) return;

			string bookingManagerElementName = GetServiceBookingManagerElementName(reservation);
			if (string.IsNullOrWhiteSpace(bookingManagerElementName)) throw new BookingManagerNotFoundException(reservation.ID);

			var bookingManager = new BookingManager((Engine)engine, engine.FindElement(bookingManagerElementName));

			if (reservation.Status == ReservationStatus.Ongoing)
			{
				try
				{
					reservation = DataMinerInterface.BookingManager.Finish(Helpers, bookingManager, reservation);
				}
				catch (Exception e)
				{
					Log(nameof(DeleteService), $"Finishing service failed: {e}", reservation.Name);
				}
			}
			else if (reservation.Status == ReservationStatus.Pending || reservation.Status == ReservationStatus.Confirmed || reservation.Status == ReservationStatus.Interrupted)
			{
				try
				{
					reservation = DataMinerInterface.BookingManager.Cancel(Helpers, bookingManager, reservation);
				}
				catch (Exception e)
				{
					Log(nameof(DeleteService), $"Canceling service failed: {e}", reservation.Name);
				}
			}
			else
			{
				// Service reservation is already stopped
			}

			try
			{
				DataMinerInterface.BookingManager.Delete(Helpers, bookingManager, reservation);
				Log(nameof(DeleteService), $"Service was deleted successfully", reservation.Name);
			}
			catch (Exception e)
			{
				// only delete that fails should throw an exception
				Log(nameof(DeleteService), $"Deleting service failed: {e}", reservation.Name);
				throw;
			}
		}

		private bool SharedSourceCanBeRemoved(ReservationInstance reservation, Guid serviceId, Guid orderId, bool skipCheckingIfServiceIsUsedByOtherOrders)
		{
			HashSet<Guid> orderReferences = GetOrderReferences(reservation);
			Log(nameof(SharedSourceCanBeRemoved), $"Current Order references: {String.Join(", ", orderReferences)}; Order Id: {orderId}");
			if (orderReferences.Contains(orderId))
			{
				Log(nameof(SharedSourceCanBeRemoved), $"Removing Order reference {orderId} from Shared Source {serviceId}");
				orderReferences.Remove(orderId);
				Service.UpdateOrderReferencesProperty(Helpers, reservation, orderReferences);
			}

			Log(nameof(SharedSourceCanBeRemoved), $"{nameof(skipCheckingIfServiceIsUsedByOtherOrders)} is {skipCheckingIfServiceIsUsedByOtherOrders}.");

			if (!skipCheckingIfServiceIsUsedByOtherOrders && ServiceIsUsedByOtherOrders(reservation, new[] { orderId }))
			{
				Log(nameof(SharedSourceCanBeRemoved), $"Service is a Shared Source and used by other orders and therefore will not be deleted", reservation.Name);
				return false;
			}

			return true;
		}

		public IEnumerable<ServiceReservationInstance> GetSharedSourceReservations()
		{
			LogMethodStarted(nameof(GetSharedSourceReservations), out var stopwatch);

			ServiceReservationInstance[] sharedSourceReservationInstances = DataMinerInterface.ResourceManager.GetReservationInstances(Helpers, ReservationInstanceExposers.Properties.StringField(ServicePropertyNames.IsGlobalEventLevelReceptionPropertyName).Equal("True")).Cast<ServiceReservationInstance>().ToArray();

			Log(nameof(GetSharedSourceReservations), $"Retrieved {sharedSourceReservationInstances.Length} Shared Sources");
			LogMethodCompleted(nameof(GetSharedSourceReservations), null, stopwatch);

			return sharedSourceReservationInstances;
		}

		public IEnumerable<ServiceReservationInstance> GetSharedSourceReservationsEndingInFuture()
		{
			LogMethodStarted(nameof(GetSharedSourceReservationsEndingInFuture), out var stopwatch);

			var isEventLevelReceptionFilter = ReservationInstanceExposers.Properties.StringField(ServicePropertyNames.IsGlobalEventLevelReceptionPropertyName).Equal("True");
			var endsInFutureFilter = ReservationInstanceExposers.End.GreaterThanOrEqual(DateTime.UtcNow);
			var serviceDefinitionFilter = ServiceReservationInstanceExposers.ServiceDefinitionID.NotEqual(ServiceDefinitionGuids.RoutingServiceDefinitionId); // Additional filter to exclude shared routing services

			var sharedSourceReservationInstances = DataMinerInterface.ResourceManager.GetReservationInstances(Helpers, isEventLevelReceptionFilter.AND(endsInFutureFilter).AND(serviceDefinitionFilter)).Cast<ServiceReservationInstance>().ToArray();

			Log(nameof(GetSharedSourceReservationsEndingInFuture), $"Retrieved {sharedSourceReservationInstances.Length} Shared Sources: {String.Join(", ", sharedSourceReservationInstances.Select(x => x.Name))}");

			LogMethodCompleted(nameof(GetSharedSourceReservationsEndingInFuture), null, stopwatch);

			return sharedSourceReservationInstances;
		}

		/// <summary>
		/// Retrieves ServiceReservationInstances that overlap with the given timespan and haven't been promoted yet.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ServiceReservationInstance> GetShareableSourceReservations(DateTime orderStartTime, DateTime orderEndTime)
		{
			using (MetricLogger.StartNew(Helpers, nameof(ServiceManager), nameof(GetShareableSourceReservations)))
			{
				var isNotEventLevelReceptionFilter = ReservationInstanceExposers.Properties.StringField(ServicePropertyNames.IsGlobalEventLevelReceptionPropertyName).Equal("False");
				var startTimeFilter = ReservationInstanceExposers.Start.LessThanOrEqual(orderStartTime);
				var endTimeFilter = ReservationInstanceExposers.End.GreaterThanOrEqual(orderEndTime);

				FilterElement<ReservationInstance> serviceDefinitionFilter = null;
				foreach (var sourceServiceDefinition in Helpers.ServiceDefinitionManager.GetReceptionServiceDefinitions().Select(x => x.Id))
				{
					if (serviceDefinitionFilter == null)
					{
						serviceDefinitionFilter = ServiceReservationInstanceExposers.ServiceDefinitionID.NotEqual(ServiceDefinitionGuids.RoutingServiceDefinitionId);
					}
					else
					{
						serviceDefinitionFilter = serviceDefinitionFilter.OR(ServiceReservationInstanceExposers.ServiceDefinitionID.NotEqual(ServiceDefinitionGuids.RoutingServiceDefinitionId));
					}
				}

				var sharedSourceReservationInstances = DataMinerInterface.ResourceManager.GetReservationInstances(Helpers, isNotEventLevelReceptionFilter.AND(startTimeFilter).AND(endTimeFilter).AND(serviceDefinitionFilter)).Cast<ServiceReservationInstance>().ToArray();

				Log(nameof(GetShareableSourceReservations), $"Retrieved {sharedSourceReservationInstances.Length} Shared Sources: {String.Join(", ", sharedSourceReservationInstances.Select(x => x.Name))}");

				return sharedSourceReservationInstances;
			}
		}

		/// <summary>
		/// Get all global event level receptions.
		/// </summary>
		/// <returns>A collection of existing global event level reception services.</returns>
		public IEnumerable<Service> GetSharedSourceServices()
		{
			var globalEventLevelReservations = GetSharedSourceReservations();

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var globalEventLevelReceptions = new List<Service>();
			foreach (var reservationInstance in globalEventLevelReservations)
			{
				var service = Service.FromReservationInstance(Helpers, reservationInstance);
				if (service != null)
				{
					globalEventLevelReceptions.Add(service);
				}
			}

			stopwatch.Stop();
			Log(nameof(GetSharedSourceServices), $"Created Global reception services from the reservations [{stopwatch.Elapsed}]");

			return globalEventLevelReceptions;
		}

		/// <summary>
		/// Returns a list that contains all services and their child services.
		/// </summary>
		/// <param name="services">List of services.</param>
		/// <returns>List of services including child services.</returns>
		private List<Service> GetAllServices(IEnumerable<Service> services)
		{
			List<Service> retrievedServices = new List<Service>();
			foreach (Service service in services)
			{
				retrievedServices.Add(service);
				retrievedServices.AddRange(GetAllServices(service.Children));
			}

			return retrievedServices;
		}

		public ReservationInstance GetReservation(Guid id)
		{
			return DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, id);
		}

		private static HashSet<Guid> GetOrderReferences(ReservationInstance serviceReservationInstance)
		{
			HashSet<Guid> references = new HashSet<Guid>();
			string rawOrderReferences = serviceReservationInstance.GetStringProperty(ServicePropertyNames.OrderIdsPropertyName);
			foreach (string rawOrderReference in rawOrderReferences.Split(';'))
			{
				if (!Guid.TryParse(rawOrderReference, out Guid parsedReference)) continue;
				references.Add(parsedReference);
			}

			return references;
		}

		/// <summary>
		/// Gets all services that use a specific resource within a specific timespan.
		/// </summary>
		/// <param name="resource">The resource used for the filter.</param>
		/// <param name="start">The start of the timespan.</param>
		/// <param name="end">The end of the timespan.</param>
		/// <returns>A collection of services.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		public IEnumerable<Service> GetServicesWithSpecificResourceAndWithinTimeSpan(Resource resource, DateTime start, DateTime end)
		{
			return GetReservationsWithSpecificResourceAndWithinTimeSpan(resource, start, end).Select(x => Service.FromReservationInstance(Helpers, x));
		}

		public IEnumerable<ReservationInstance> GetReservationsWithSpecificResourceAndWithinTimeSpan(Resource resource, DateTime start, DateTime end)
		{
			if (resource == null) throw new ArgumentNullException(nameof(resource));
			else if (resource.ID == Guid.Empty) throw new ArgumentException("ID property is null or empty", nameof(resource));
			else
			{
				// nothing
			}

			if (end < start) throw new ArgumentException("End date is before start date", nameof(end));

			var resourceFilter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(resource.ID);
			var startFilter = ReservationInstanceExposers.Start.LessThanOrEqual(end);
			var endFilter = ReservationInstanceExposers.End.GreaterThanOrEqual(start);

			return DataMinerInterface.ResourceManager.GetReservationInstances(Helpers, new ANDFilterElement<ReservationInstance>(resourceFilter, startFilter, endFilter));
		}

		private void UpdateLinkedServiceIdProperty(ReservationInstance currentBookedReservation, Order order, Service service)
		{
			LogMethodStarted(nameof(UpdateLinkedServiceIdProperty), out var stopwatch, service.Name);

			try
			{
				Service linkedService = null;
				var allOrderServices = order.AllServices;

				linkedService = allOrderServices.FirstOrDefault(s => s != null && s.LinkedServiceId == service.Id);
				if (linkedService == null)
				{
					LogMethodCompleted(nameof(UpdateLinkedServiceIdProperty), service.Name, stopwatch);
					return;
				}

				linkedService.LinkedServiceId = currentBookedReservation.ID;

				var linkedReservation = GetReservation(linkedService.Id);
				if (linkedReservation == null || !linkedService.IsBooked)
				{
					LogMethodCompleted(nameof(UpdateLinkedServiceIdProperty), service.Name, stopwatch);
					return;
				}

				DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(Helpers, linkedReservation, new Dictionary<string, object> { { ServicePropertyNames.LinkedServiceIdPropertyName, linkedService.LinkedServiceId.ToString() } });
			}
			catch (Exception e)
			{
				Log(nameof(UpdateLinkedServiceIdProperty), $"Exception during updating linked service id property: {e}");
				// this doesn't throw an exception as the VerifyServiceTask will catch any errors in the service reservation
			}

			LogMethodCompleted(nameof(UpdateLinkedServiceIdProperty), service.Name, stopwatch);
		}

		public void PromoteToSharedSource(Service service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			var reservation = GetReservation(service.Id);
			if (reservation == null) throw new ReservationNotFoundException(service.Id);

			reservation.Properties.AddOrUpdate(ServicePropertyNames.IsGlobalEventLevelReceptionPropertyName, service.IsSharedSource.ToString());
			DataMinerInterface.ResourceManager.AddOrUpdateReservationInstances(Helpers, reservation);
		}

		public List<Function> GetDefaultFunctions(ServiceDefinition serviceDefinition)
		{
			if (serviceDefinition == null) throw new ArgumentNullException(nameof(serviceDefinition));

			var functions = new List<Function>();

			foreach (var node in serviceDefinition.Diagram.Nodes)
			{
				var functionDefinition = serviceDefinition.FunctionDefinitions.FirstOrDefault(f => f.Id == node.Configuration.FunctionID) ?? throw new FunctionNotFoundException(node.Configuration.FunctionID);

				var function = new DisplayedFunction(Helpers, node, functionDefinition);

				functions.Add(function);
			}

			return functions;
		}

		public List<Function> GetFunctions(ServiceReservationInstance reservationInstance, ServiceDefinition serviceDefinition)
		{
			if (reservationInstance == null) throw new ArgumentNullException(nameof(reservationInstance));
			if (serviceDefinition == null) throw new ArgumentNullException(nameof(serviceDefinition));

			LogMethodStarted(nameof(GetFunctions), out var stopwatch);

			if (reservationInstance.IsQuarantined)
			{
				// When the service reservation instance is quarantined, no valid function data is available.
				Log(nameof(GetFunctions), $"Reservation {reservationInstance.Name}({reservationInstance.ID}) is quarantined");
				LogMethodCompleted(nameof(GetFunctions), null, stopwatch);
				return new List<Function>();
			}

			var functions = new List<Function>();
			foreach (var node in serviceDefinition.Diagram.Nodes)
			{
				var functionDefinition = serviceDefinition.FunctionDefinitions.Single(fd => fd.Label == node.Label);

				var function = new DisplayedFunction(Helpers, reservationInstance, node, functionDefinition);

				function.EnforceSelectedResource = true;

				functions.Add(function);
			}

			LogMethodCompleted(nameof(GetFunctions), null, stopwatch);

			return functions;
		}

		private string GetServiceBookingManagerElementName(ReservationInstance reservation)
		{
			if (reservation.Properties == null)
			{
				return null;
			}

			var virtualPlatformProperty = reservation.Properties.FirstOrDefault(p => p.Key == "Virtual Platform");
			if (virtualPlatformProperty.Value == null)
			{
				return null;
			}

			var bookingManager = Helpers.ServiceDefinitionManager.GetBookingManager(EnumExtensions.GetEnumValueFromDescription<VirtualPlatform>((string)virtualPlatformProperty.Value));
			if (bookingManager == null)
			{
				return null;
			}

			return bookingManager.ElementName;
		}

		/// <summary>
		/// Gets the Pre Roll duration for the provided Service Definition.
		/// </summary>
		/// <param name="serviceDefinition">Service Definition for which the pre roll is requested.</param>
		/// <param name="integrationType">Optional IntegrationType of the service. Affects the pre roll for Plasma Services.</param>
		/// <param name="primarySourcePreRollDuration">Pre Roll of the Primary Source Service. In case the Pre Roll of the Primary Source Service itself is requested, this value is ignored.</param>
		/// <returns>TimeSpan of the Pre Roll.</returns>
		public static TimeSpan GetPreRollDuration(ServiceDefinition serviceDefinition, IntegrationType integrationType = IntegrationType.None, TimeSpan primarySourcePreRollDuration = default(TimeSpan))
		{
			if (integrationType == IntegrationType.Plasma)
			{
				return PlasmaServicePreRoll;
			}
			else if (integrationType == IntegrationType.Feenix)
			{
				return FeenixServicePreRoll;
			}
			else if (serviceDefinition.Id == ServiceDefinitionGuids.RecordingMessiLive || serviceDefinition.Id == ServiceDefinitionGuids.RecordingMessiLiveBackup)
			{
				return MessiLiveRecordingPreRoll;
			}
			else if (serviceDefinition.Id == ServiceDefinitionGuids.RecordingMessiNews)
			{
				return MessiNewsRecordingPreRoll;
			}
			else if (preRollDurations.TryGetValue(serviceDefinition.VirtualPlatform, out var preroll))
			{
				return preroll;
			}
			else
			{
				return primarySourcePreRollDuration;
			}
		}

		/// <summary>
		/// Gets the Post Roll duration for the provided Service Definition.
		/// </summary>
		/// <param name="serviceDefinition">Service Definition for which the post roll is requested.</param>
		/// <param name="integrationType">Optional IntegrationType of the service. Affects the post roll for Plasma Services.</param>
		/// <param name="primarySourcePostRollDuration">Post Roll of the Primary Source Service. In case the Post Roll of the Primary Source Service itself is requested, this value is ignored.</param>
		/// <returns>TimeSpan of the Post Roll.</returns>
		public static TimeSpan GetPostRollDuration(ServiceDefinition serviceDefinition, IntegrationType integrationType = IntegrationType.None, TimeSpan primarySourcePostRollDuration = default(TimeSpan))
		{
			if (integrationType == IntegrationType.Plasma)
			{
				return PlasmaServicePostRoll;
			}
			else if (integrationType == IntegrationType.Feenix)
			{
				return FeenixServicePostRoll;
			}
			else if (serviceDefinition.Id == ServiceDefinitionGuids.RecordingMessiLive || serviceDefinition.Id == ServiceDefinitionGuids.RecordingMessiLiveBackup)
			{
				return MessiLiveRecordingPostRoll;
			}
			else if (serviceDefinition.Id == ServiceDefinitionGuids.RecordingMessiNews)
			{
				return MessiNewsRecordingPostRoll;
			}
			else if (postRollDurations.TryGetValue(serviceDefinition.VirtualPlatform, out var postroll))
			{
				return postroll;
			}
			else
			{
				return primarySourcePostRollDuration;
			}
		}

		private void Log(string methodName, string message, string objectName = null)
		{
			Helpers.Log(nameof(ServiceManager), methodName, message, objectName);
		}

		private void LogMethodStarted(string methodName, out Stopwatch stopwatch, string objectName = null)
		{
			Helpers.LogMethodStart(nameof(ServiceManager), methodName, out stopwatch, objectName);
		}

		private void LogMethodCompleted(string methodName, string objectName = null, Stopwatch stopwatch = null)
		{
			Helpers.LogMethodCompleted(nameof(ServiceManager), methodName, objectName, stopwatch);
		}

		private sealed class CreateOrEditBookingInput
		{
			public CreateOrEditBookingInput(Helpers helpers, Service service, BookingManager bookingManager, Order order)
			{
				helpers.LogMethodStart(nameof(CreateOrEditBookingInput), nameof(CreateOrEditBookingInput), out var stopwatch, service.Name);

				BookingData = service.GetBookingDataForBooking(helpers);
				Functions = service.GetFunctionsForBooking(helpers, true);
				Properties = service.GetPropertiesForBooking(bookingManager.Properties.Where(x => !String.IsNullOrWhiteSpace(x.Name)).ToList(), order, helpers);
				CustomEvents = service.GetCustomEventsForBooking(helpers, bookingManager.Events);

				helpers.LogMethodCompleted(nameof(CreateOrEditBookingInput), nameof(CreateOrEditBookingInput), service.Name, stopwatch);
			}

			public Booking BookingData { get; }

			public IEnumerable<Library.Solutions.SRM.Model.Function> Functions { get; }

			public IEnumerable<SrmProperty> Properties { get; }

			public IEnumerable<Library.Solutions.SRM.Model.Events.Event> CustomEvents { get; }
		}
	}
}