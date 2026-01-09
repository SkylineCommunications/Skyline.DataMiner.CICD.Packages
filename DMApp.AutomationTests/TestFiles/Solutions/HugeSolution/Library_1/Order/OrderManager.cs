namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reservations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Library.Reservation;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.Library.Solutions.SRM.Model;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.Events;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.ReservationAction;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using DataMinerInterface = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface.DataMinerInterface;
	using Service = Service.Service;

	public class OrderManager : HelpedObject, IOrderManager
	{
		public static readonly string OrderBookingManagerElementName = "Order Booking Manager";

		internal const string OrderAttachmentsDirectory = @"C:\Skyline DataMiner\Documents\RESERVATIONINSTANCE_ATTACHMENTS";

		public OrderManager(Helpers helpers) : base(helpers)
		{
		}

		/// <summary>
		/// Gets the Order for the given ID.
		/// </summary>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="ReservationNotFoundException"/>
		public Order GetOrder(Guid id, bool forceServiceReservationsToOverwriteServiceConfig = false, bool skipGettingEvent = false)
		{
			if (id == Guid.Empty) throw new ArgumentException("ID is empty", nameof(id));

			var reservation = helpers.ReservationManager.GetReservation(id) ?? throw new ReservationNotFoundException(id);

			return GetOrder(reservation, forceServiceReservationsToOverwriteServiceConfig, skipGettingEvent);
		}

		/// <summary>
		/// Creates an Order object based on the given reservation.
		/// </summary>
		/// <param name="reservation">The reservation representing the Order.</param>
		/// <param name="forceServiceReservationsToOverwriteServiceConfig">An optional flag used when creating the Service objects. If true, services will be created based on their reservation. If false, a property in the service configuration will be checked to determine if services should be created based on service config or reservation.</param>
		/// <param name="skipGettingEvent"></param>
		/// <returns></returns>
		public Order GetOrder(ReservationInstance reservation, bool forceServiceReservationsToOverwriteServiceConfig = false, bool skipGettingEvent = false)
		{
			using (StartPerformanceLogging())
			{
				// Note that no exception will be thrown for not finding the Event, the property will contain null in that case.

				ArgumentNullCheck.ThrowIfNull(reservation, nameof(reservation));

				DateTime convertedStartTime = reservation.Start.FromReservation().Truncate(TimeSpan.FromSeconds(1)).Truncate(TimeSpan.FromMilliseconds(1));
				DateTime convertedEndTime = reservation.End.FromReservation().Truncate(TimeSpan.FromSeconds(1)).Truncate(TimeSpan.FromMilliseconds(1));

				Order order = new Order
				{
					Id = reservation.ID,
					Reservation = reservation,
					RecurringSequenceInfo = GetRecurringSequenceInfo(reservation),
					IntegrationType = GetIntegrationType(reservation),
				};

				order.Start = convertedStartTime.Add(reservation.GetPreRoll());
				order.End = convertedEndTime.Subtract(reservation.GetPostRoll());
				order.RecurringSequenceInfo = GetRecurringSequenceInfo(reservation);
				order.Status = GetOrderStatus(reservation);
				order.Comments = GetStringProperty(reservation, LiteOrder.PropertyNameComments);
				order.Type = GetOrderType(reservation);
				order.SportsPlanning = GetSportsPlanning(reservation);
				order.NewsInformation = GetNewsInformation(reservation);
				order.SetUserGroupIds(GetUserGroupIds(reservation));
				order.IsInternal = GetIsInternal(reservation);
				order.PlasmaId = GetStringProperty(reservation, LiteOrder.PropertyNamePlasmaId);
				order.EditorialObjectId = GetStringProperty(reservation, LiteOrder.PropertyNameEditorialObjectId);
				order.YleId = GetStringProperty(reservation, LiteOrder.PropertyNameYleId);
				order.CreatedByUserName = GetStringProperty(reservation, LiteOrder.PropertyNameCreatedBy);
				order.CreatedByEmail = GetStringProperty(reservation, LiteOrder.PropertyNameCreatedByEmail);
				order.CreatedByPhone = GetStringProperty(reservation, LiteOrder.PropertyNameCreatedByPhone);
				order.LastUpdatedBy = GetStringProperty(reservation, LiteOrder.PropertyNameLastUpdatedBy);
				order.LastUpdatedByEmail = GetStringProperty(reservation, LiteOrder.PropertyNameLastUpdatedByEmail);
				order.LastUpdatedByPhone = GetStringProperty(reservation, LiteOrder.PropertyNameLastUpdatedByPhone);
				order.McrOperatorNotes = GetStringProperty(reservation, LiteOrder.PropertyNameMcrOperatorNotes);
				order.MediaOperatorNotes = GetStringProperty(reservation, LiteOrder.PropertyNameMediaOperatorNotes);
				order.ErrorDescription = GetStringProperty(reservation, LiteOrder.PropertyNameErrorDescription);
				order.Company = GetStringProperty(reservation, LiteOrder.PropertyNameCustomer);
				order.ReasonForCancellationOrRejection = GetStringProperty(reservation, LiteOrder.PropertyNameReasonForCancellationOrRejection);
				order.StartNow = Boolean.TryParse(GetStringProperty(reservation, LiteOrder.PropertyNameStartnow), out var parsedStartNow) && parsedStartNow;
				order.ConvertedFromRunningToStartNow = Boolean.TryParse(GetStringProperty(reservation, LiteOrder.PropertyNameConvertedFromRunningStartnow), out var parsedConvertedFromRunningToStartNow) && parsedConvertedFromRunningToStartNow;
				order.PreviousRunningOrderId = Guid.TryParse(GetStringProperty(reservation, LiteOrder.PropertyNamePreviousRunningOrderId), out var parsedPreviousRunningOrderId) ? parsedPreviousRunningOrderId : Guid.Empty;
				order.BillingInfo = GetBillingInfo(reservation);
				order.LateChange = Boolean.TryParse(GetStringProperty(reservation, LiteOrder.PropertyNameMcrLateChange), out var parsedLateChange) && parsedLateChange;
				order.PublicationStart = double.TryParse(reservation.GetStringProperty(LiteOrder.PropertyNamePublicationStart), out double parsedPublicationStart) ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(parsedPublicationStart) : default;
				order.PublicationEnd = double.TryParse(reservation.GetStringProperty(LiteOrder.PropertyNamePublicationEnd), out double parsedPublicationEnd) ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(parsedPublicationEnd) : default;

				var eventId = GetOrderEventGuid(reservation);
				order.Event = skipGettingEvent ? null : helpers.EventManager.GetEvent(eventId) ?? throw new EventNotFoundException(eventId);

				order.Contract = order.Event?.Contract;

				Log(nameof(GetOrder), $"Order Reservation Security View IDs: '{string.Join(", ", reservation.SecurityViewIDs)}'");

				// temporary email for investigation purposes
				if (!reservation.SecurityViewIDs.Any()) NotificationManager.SendMailToSkylineDevelopers(helpers, $"Missing Security View IDs Detected", $"Order {reservation.Name} ({reservation.ID} has no security view IDs");

				order.SetSecurityViewIds(reservation.SecurityViewIDs);

				Log(nameof(GetOrder), $"Summary of properties taken from Order reservation: Recurrence={order.RecurringSequenceInfo?.ToString()}, SecurityViewIds='{string.Join(",", order.SecurityViewIds)}', {nameof(order.Status)}={order.Status.GetDescription()}", order.Name);

				var serviceReservationInstance = reservation as ServiceReservationInstance ?? throw new ArgumentException($"Unable to cast to {nameof(ServiceReservationInstance)}", nameof(reservation));

				var orderServiceDefinition = DataMinerInterface.ServiceManager.GetServiceDefinition(helpers, serviceReservationInstance.ServiceDefinitionID);
				order.Definition = new ServiceDefinition { Id = serviceReservationInstance.ServiceDefinitionID, BookingManagerElementName = SrmConfiguration.OrderBookingManagerElementName, Diagram = orderServiceDefinition?.Diagram ?? throw new ServiceDefinitionNotFoundException($"Unable to retrieve service definition {serviceReservationInstance.ServiceDefinitionID}") };

				if (!order.Definition.IsValid(helpers)) throw new InvalidServiceDefinitionException($"Order definition {serviceReservationInstance.ServiceDefinitionID} is not valid");

				Log(nameof(GetOrder), $"Retrieved Service Definition: {order.Definition}");

				order.Sources = helpers.ServiceManager.GetOrderServices(serviceReservationInstance, forceServiceReservationsToOverwriteServiceConfig, orderServiceDefinition);

				GetOrderName(reservation, order);

				order.Subtype = order.AllServices.Exists(s => s.Definition.VirtualPlatform == YLE.ServiceDefinition.VirtualPlatform.VizremStudio) ? OrderSubType.Vizrem : OrderSubType.Normal;
				order.InitializeAudioConfigCopyFromSource(helpers);
				order.VerifySatelliteRxSynposisAttachments();
				order.SetServiceDisplayNames();

				return order;
			}
		}

		private static void GetOrderName(ReservationInstance reservation, Order order)
		{
			bool isRecurringOrder = order.RecurringSequenceInfo.Recurrence.IsConfigured;
			bool isEurovisionOrder = order.IntegrationType == IntegrationType.Eurovision || order.AllServices.Exists(s => s.IntegrationType == IntegrationType.Eurovision);

			if ((isRecurringOrder || isEurovisionOrder) && reservation.Name.Contains(" ["))
			{
				int indexOfLastOpeningBracket = reservation.Name.LastIndexOf(" [", StringComparison.InvariantCulture);
				order.ManualName = reservation.Name.Substring(0, indexOfLastOpeningBracket);
				order.NamePostFix = reservation.Name.Substring(indexOfLastOpeningBracket);
			}
			else
			{
				order.ManualName = reservation.Name;
			}
		}

		public LiteOrder GetLiteOrder(ReservationInstance reservation, bool skipGettingEvent = false)
		{
			if (reservation == null) throw new ArgumentNullException(nameof(reservation));

			DateTime convertedStartWithPreRoll = reservation.Start.FromReservation().Truncate(TimeSpan.FromSeconds(1)).Truncate(TimeSpan.FromMilliseconds(1));
			DateTime convertedEndWithPostRoll = reservation.End.FromReservation().Truncate(TimeSpan.FromSeconds(1)).Truncate(TimeSpan.FromMilliseconds(1));

			var liteOrder = new LiteOrder
			{
				Id = reservation.ID,
				ManualName = reservation.Name,
				Start = convertedStartWithPreRoll.Add(reservation.GetPreRoll()),
				End = convertedEndWithPostRoll.Subtract(reservation.GetPostRoll()),
				Status = GetOrderStatus(reservation),
				Comments = GetStringProperty(reservation, LiteOrder.PropertyNameComments),
				Type = GetOrderType(reservation),
				IntegrationType = GetIntegrationType(reservation),
				Event = skipGettingEvent ? null : helpers.EventManager.GetEvent(GetOrderEventGuid(reservation)),
				SportsPlanning = GetSportsPlanning(reservation),
				NewsInformation = GetNewsInformation(reservation),
				IsInternal = GetIsInternal(reservation),
				PlasmaId = GetStringProperty(reservation, LiteOrder.PropertyNamePlasmaId),
				EditorialObjectId = GetStringProperty(reservation, LiteOrder.PropertyNameEditorialObjectId),
				YleId = GetStringProperty(reservation, LiteOrder.PropertyNameYleId),
				CreatedByUserName = GetStringProperty(reservation, LiteOrder.PropertyNameCreatedBy),
				CreatedByEmail = GetStringProperty(reservation, LiteOrder.PropertyNameCreatedByEmail),
				CreatedByPhone = GetStringProperty(reservation, LiteOrder.PropertyNameCreatedByPhone),
				LastUpdatedBy = GetStringProperty(reservation, LiteOrder.PropertyNameLastUpdatedBy),
				LastUpdatedByEmail = GetStringProperty(reservation, LiteOrder.PropertyNameLastUpdatedByEmail),
				LastUpdatedByPhone = GetStringProperty(reservation, LiteOrder.PropertyNameLastUpdatedByPhone),
				McrOperatorNotes = GetStringProperty(reservation, LiteOrder.PropertyNameMcrOperatorNotes),
				MediaOperatorNotes = GetStringProperty(reservation, LiteOrder.PropertyNameMediaOperatorNotes),
				ErrorDescription = GetStringProperty(reservation, LiteOrder.PropertyNameErrorDescription),
				Company = GetStringProperty(reservation, LiteOrder.PropertyNameCustomer),
				ReasonForCancellationOrRejection = GetStringProperty(reservation, LiteOrder.PropertyNameReasonForCancellationOrRejection),
				StartNow = bool.TryParse(GetStringProperty(reservation, LiteOrder.PropertyNameStartnow), out var parsedStartNow) && parsedStartNow,
				ConvertedFromRunningToStartNow = Boolean.TryParse(GetStringProperty(reservation, LiteOrder.PropertyNameConvertedFromRunningStartnow), out var parsedConvertedFromRunningToStartNow) && parsedConvertedFromRunningToStartNow,
				PreviousRunningOrderId = Guid.TryParse(GetStringProperty(reservation, LiteOrder.PropertyNamePreviousRunningOrderId), out var parsedPreviousRunningOrderId) ? parsedPreviousRunningOrderId : Guid.Empty,
				BillingInfo = GetBillingInfo(reservation),
				Reservation = reservation
			};

			liteOrder.SetUserGroupIds(GetUserGroupIds(reservation));
			liteOrder.SetSecurityViewIds(reservation.SecurityViewIDs);

			return liteOrder;
		}

		/// <summary>
		/// Retrieves the General Order information, but does not retrieve the underlying services.
		/// This method was added to improve performance.
		/// </summary>
		/// <param name="id">Id of the Order.</param>
		/// <param name="skipGettingEvent"></param>
		/// <returns>Order with all of its properties filled out except for its Services.</returns>
		public LiteOrder GetLiteOrder(Guid id, bool skipGettingEvent = false)
		{
			var reservation = helpers.ReservationManager.GetReservation(id) ?? throw new ReservationNotFoundException(id);

			return GetLiteOrder(reservation, skipGettingEvent);
		}

		/// <summary>
		/// Returns the reservation instance for the given plasma id.
		/// </summary>
		/// <param name="programId"></param>
		/// <param name="plasmaId">The plasma id.</param>
		/// <returns>A reservation instance.</returns>
		public ReservationInstance GetPlasmaReservationInstance(string programId, string plasmaId)
		{
			if (String.IsNullOrWhiteSpace(programId)) return null;

			var reservation = DataMinerInterface.ResourceManager.GetReservationInstancesByProperty(helpers, LiteOrder.PropertyNameEditorialObjectId, programId).SingleOrDefault();

			if (reservation is null && !string.IsNullOrWhiteSpace(plasmaId))
			{
				// backwards compatibility
				reservation = DataMinerInterface.ResourceManager.GetReservationInstancesByProperty(helpers, LiteOrder.PropertyNamePlasmaId, plasmaId).SingleOrDefault();
			}

			return reservation;
		}

		public Order GetPlasmaOrder(string programId, string plasmaId)
		{
			var reservation = GetPlasmaReservationInstance(programId, plasmaId);
			if (reservation is null) return null;

			return GetOrder(reservation);
		}

		/// <summary>
		/// Returns the reservation instance for the given feenix yle id.
		/// </summary>
		/// <param name="yleId">The yle id.</param>
		/// <returns>A reservation instance.</returns>
		public ReservationInstance GetFeenixReservationInstance(string yleId)
		{
			return DataMinerInterface.ResourceManager.GetReservationInstancesByProperty(helpers, LiteOrder.PropertyNameYleId, yleId).FirstOrDefault();
		}

		/// <summary>
		/// Used to retrieve an Order based on its YLE ID.
		/// The YLE ID property is set through the Plasma and Feenix Integration.
		/// </summary>
		/// <param name="yleId">Uniquely identifies the YLE Order.</param>
		/// <returns>Null if no order could be found with the specified ID, else the order with the ID.</returns>
		public Order GetFeenixOrder(string yleId)
		{
			var reservationInstance = GetFeenixReservationInstance(yleId);
			if (reservationInstance == null) return null;

			return GetOrder(reservationInstance);
		}

		/// <summary>
		/// Used to retrieve an Order based on its Transmission Number.
		/// The Transmission Number property is set through the HandleIntegrationUpdate script.
		/// Multiple orders a can have the same Transmission Number if they use the same Eurovision Event Level Reception.
		/// </summary>
		/// <param name="transmissionNumber">Number that identifies a Eurovision Order.</param>
		/// <returns></returns>
		public List<Order> GetEurovisionOrders(string transmissionNumber)
		{
			IEnumerable<ReservationInstance> reservationInstances = DataMinerInterface.ResourceManager.GetReservationInstancesByProperty(helpers, LiteOrder.PropertyNameEurovisionTransmissionNumber, transmissionNumber);
			reservationInstances = reservationInstances.Where(x => x.TryGetPropertyValue(LiteOrder.PropertyNameType, out string type) && type.Equals("Video")).ToList();

			if (!reservationInstances.Any()) return new List<Order>();

			List<Order> orders = new List<Order>();
			foreach (ReservationInstance reservationInstance in reservationInstances)
			{
				orders.Add(GetOrder(reservationInstance));
			}

			return orders;
		}

		/// <summary>
		/// Used to retrieve an Order based on its Work Order Id which is saved under the Eurovision Id property.
		/// The Work Order ID property is set through the CustomerUI or the Eurovision integration.
		/// Only manually created orders are requested for which the synopsis update has not yet been received.
		/// </summary>
		/// <param name="workOrderId">ID that identifies a Eurovision Order.</param>
		/// <returns></returns>
		public List<Order> GetManualEurovisionOrders(string workOrderId)
		{
			IEnumerable<ReservationInstance> reservationInstances = DataMinerInterface.ResourceManager.GetReservationInstancesByProperty(helpers, LiteOrder.PropertyNameEurovisionId, workOrderId);

			reservationInstances = reservationInstances.Where(x => x.TryGetPropertyValue(LiteOrder.PropertyNameType, out string type) && type.Equals("Video") && x.TryGetPropertyValue(LiteOrder.PropertyNameEurovisionTransmissionNumber, out string transmissionNumber) && string.IsNullOrWhiteSpace(transmissionNumber)).ToList();

			if (!reservationInstances.Any()) return new List<Order>();

			List<Order> orders = new List<Order>();
			foreach (ReservationInstance reservationInstance in reservationInstances)
			{
				orders.Add(GetOrder(reservationInstance));
			}

			return orders;
		}

		public IEnumerable<ReservationInstance> GetAllFutureAndOngoingVideoReservations()
		{
			return DataMinerInterface.ResourceManager.GetReservationInstances(helpers, ReservationInstanceExposers.Properties.DictStringField("Type").Equal("Video").AND(ReservationInstanceExposers.End.GreaterThan(DateTime.Now)));
		}

		public List<Order> GetAllOrdersWithinTimeFrame(DateTime start, DateTime end)
		{
			IEnumerable<ReservationInstance> allOrderReservations = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, ReservationInstanceExposers.Start.GreaterThanOrEqual(start).AND(ReservationInstanceExposers.End.LessThanOrEqual(end).AND(ReservationInstanceExposers.Properties.DictStringField("Type").Equal("Video"))));
			if (allOrderReservations == null) return new List<Order>();

			Order convertedOrder = null;
			List<Order> allOrders = new List<Order>();
			foreach (var orderReservation in allOrderReservations)
			{
				convertedOrder = GetOrder(orderReservation);
				if (convertedOrder != null) allOrders.Add(convertedOrder);
			}

			return allOrders;
		}

		public IEnumerable<ReservationInstance> GetAllReservationInstancesFromTemplate(Guid templateId)
		{
			return DataMinerInterface.ResourceManager.GetReservationInstancesByProperty(helpers, LiteOrder.PropertyNameFromTemplate, templateId.ToString());
		}

		public IEnumerable<Task> ChangeOrderAndServiceTiming(Order order)
		{
			var tasks = new List<Task>();

			var services = order.AllServices;
			foreach (var service in services)
			{
				if (service == null || service.IsDummy) continue;
				if (service.IsSharedSource) continue;
				if (!service.IsBooked) continue;

				if (service.Definition.VirtualPlatform.Equals("Routing"))
				{
					// Get the new end time of the parent service
					service.End = services.FirstOrDefault(x => x.Children.Contains(service))?.End ?? throw new ServiceNotFoundException($"Unable to find parent of {service.Name}", true);
				}

				Log(nameof(ChangeOrderAndServiceTiming), "Generate extend service time task for " + service.Name);
				var extendServiceTimeTask = new ChangeServiceTimeTask(helpers, service);
				tasks.Add(extendServiceTimeTask);
				if (!extendServiceTimeTask.Execute())
				{
					Log(nameof(ChangeOrderAndServiceTiming), "Generate change service end time task failed");
					return tasks;
				}
			}

			Log(nameof(ChangeOrderAndServiceTiming), "Generate change order end time task for " + order.Name);
			var changeOrderEndTimeTask = new ChangeOrderTimeTask(helpers, order);
			tasks.Add(changeOrderEndTimeTask);
			if (!changeOrderEndTimeTask.Execute())
			{
				Log(nameof(ChangeOrderAndServiceTiming), "Generate change order end time task failed");
				return tasks;
			}

			return tasks;
		}

		public void ChangeOrderTime(Order order)
		{
			using (StartPerformanceLogging())
			{
				if (order == null) throw new ArgumentNullException(nameof(order));

				var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(order.Definition.BookingManagerElementName));

				order.Reservation = helpers.ReservationManager.GetReservation(order.Id) ?? throw new ReservationNotFoundException(order.Name);
				if (order.Reservation.Status == ReservationStatus.Ended)
				{
					Log(nameof(ChangeOrderTime), $"Order has already been ended for a while (Reservation status: {order.Reservation.Status.GetDescription()}), no timing update allowed");
					return;
				}

				var reservationPreRoll = order.Reservation.GetPreRoll();
				var reservationStart = order.Reservation.Start.FromReservation().Add(reservationPreRoll);

				var changeOrderTimeInputData = new ChangeTimeInputData
				{
					// Start time may not be changed of an ongoing order.
					StartDate = order.ShouldBeRunning ? reservationStart : order.Start,
					PreRoll = order.ShouldBeRunning ? reservationPreRoll : order.PreRoll,
					EndDate = order.End,
					PostRoll = order.PostRoll,
					IsSilent = true
				};

				var now = DateTime.Now;
				if (order.ShouldBeRunning && order.Reservation.Status == ReservationStatus.Pending && changeOrderTimeInputData.StartDate <= now)
				{
					/* Repair situation where a reservation is considered to be running but is still somehow in pending state.
					This will move the reservation to ongoing again. */
					changeOrderTimeInputData.StartDate = now + bookingManager.EventReschedulingDelay;
				}

				if (changeOrderTimeInputData.StartDate.Kind != DateTimeKind.Local)
				{
					changeOrderTimeInputData.StartDate = changeOrderTimeInputData.StartDate.ToLocalTime();
					Log(nameof(ChangeOrderTime), $"Had to change start date argument to local time: {changeOrderTimeInputData.StartDate.ToFullDetailString()}", order.Name);
				}

				if (changeOrderTimeInputData.EndDate.Kind != DateTimeKind.Local)
				{
					changeOrderTimeInputData.EndDate = changeOrderTimeInputData.EndDate.ToLocalTime();
					Log(nameof(ChangeOrderTime), $"Had to change end date argument to local time: {changeOrderTimeInputData.EndDate.ToFullDetailString()}", order.Name);
				}

				Log(nameof(ChangeOrderTime), $"Changing order timing to Start: {changeOrderTimeInputData.StartDate.ToFullDetailString()}; End: {changeOrderTimeInputData.EndDate.ToFullDetailString()}; Preroll: {changeOrderTimeInputData.PreRoll}; Postroll: {changeOrderTimeInputData.PostRoll}", order.Name);

				order.Reservation = DataMinerInterface.BookingManager.ChangeTime(helpers, bookingManager, order.Reservation, changeOrderTimeInputData);
			}
		}

		public void ChangeOrderName(Order order)
		{
			using (StartPerformanceLogging())
			{
				if (order == null) throw new ArgumentNullException(nameof(order));

				var reservation = helpers.ReservationManager.GetReservation(order.Id) as ServiceReservationInstance ?? throw new ReservationNotFoundException(order.Id);

				Log(nameof(ChangeOrderName), $"Changing order name from {reservation.Name} to {order.Name}");

				order.Reservation = DataMinerInterface.BookingManager.ChangeName(helpers, reservation, order.Name);
			}
		}

		public bool TryExtendOrder(Order order, TimeSpan timeToAdd)
		{
			if (order == null) throw new ArgumentNullException("order");

			try
			{
				var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(order.Definition.BookingManagerElementName));

				var reservation = helpers.ReservationManager.GetReservation(order.Id) ?? throw new ReservationNotFoundException("Unable to get reservation for order " + order.Name);

				var extendInputData = new ExtendBookingInputData
				{
					IsSilent = true,
					TimeToAdd = timeToAdd
				};

				order.Reservation = DataMinerInterface.BookingManager.Extend(helpers, bookingManager, reservation, extendInputData);

				return true;
			}
			catch (Exception ex)
			{
				Log(nameof(TryExtendOrder), $"Exception while extending order: {ex}");
				return false;
			}
		}

		/// <summary>
		/// Waits until the main booking has the actual desired booking life cycle. Matching with the action (start/stop) of the order reservation.
		/// </summary>
		/// <param name="helpers">Helpers class.</param>
		/// <param name="order">Order to update.</param>
		/// <param name="action">Will be filled in when the status update is called via Handle Service Action, otherwise should be empty.</param>
		/// <param name="orderReservation">The actual reservation instance of the order it self, when reaching the correct booking life cycle the reservation instance will be up to date.</param>
		public static bool TryWaitingUntilOrderHasValidBookingLifeCycle(Helpers helpers, Order order, LsoEnhancedAction action, out ReservationInstance orderReservation)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (order == null) throw new ArgumentNullException(nameof(order));
			if (action == null) throw new ArgumentNullException(nameof(action));

			helpers.LogMethodStart(nameof(Order), nameof(TryWaitingUntilOrderHasValidBookingLifeCycle), out var stopWatch, order.Name);

			orderReservation = null;

			try
			{
				int retries = 0;
				bool triggeredByStart = action.Event == SrmEvent.START;
				bool triggeredByStop = action.Event == SrmEvent.STOP;
				bool triggeredByPostRollEnd = action.Event == SrmEvent.STOP_BOOKING_WITH_POSTROLL;

				do
				{
					orderReservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, order.Id);
					if (orderReservation == null)
					{
						retries++;
						Thread.Sleep(100);
						continue;
					}

					var bookingLifeCycle = orderReservation.GetBookingLifeCycle();

					if (triggeredByStart && bookingLifeCycle == GeneralStatus.Running)
					{
						helpers.Log(nameof(Order), nameof(TryWaitingUntilOrderHasValidBookingLifeCycle), $"Order {order.Name} is containing a running booking life cycle", order.Name);
						helpers.LogMethodCompleted(nameof(Order), nameof(TryWaitingUntilOrderHasValidBookingLifeCycle), order.Name, stopWatch);
						return true;
					}
					else if (triggeredByStop && bookingLifeCycle == GeneralStatus.Stopping)
					{
						helpers.Log(nameof(Order), nameof(TryWaitingUntilOrderHasValidBookingLifeCycle), $"Order {order.Name} is containing a stopping booking life cycle", order.Name);
						helpers.LogMethodCompleted(nameof(Order), nameof(TryWaitingUntilOrderHasValidBookingLifeCycle), order.Name, stopWatch);
						return true;
					}
					else if (triggeredByPostRollEnd && bookingLifeCycle == GeneralStatus.Completed)
					{
						helpers.Log(nameof(Order), nameof(TryWaitingUntilOrderHasValidBookingLifeCycle), $"Order {order.Name} is containing a completed booking life cycle", order.Name);
						helpers.LogMethodCompleted(nameof(Order), nameof(TryWaitingUntilOrderHasValidBookingLifeCycle), order.Name, stopWatch);
						return true;
					}
					else
					{
						//Nothing
					}

					retries++;
					Thread.Sleep(100);

				} while (retries < 10);

				helpers.LogMethodCompleted(nameof(Order), nameof(TryWaitingUntilOrderHasValidBookingLifeCycle), order.Name, stopWatch);

				return false;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Order), nameof(TryWaitingUntilOrderHasValidBookingLifeCycle), $"Order {order.Name} encountered an issue while waiting on the correct booking life cycle: {e}", order.Name);
				return false;
			}
		}

		public IEnumerable<Task> DeleteOrder(Guid orderId, List<Guid> serviceIdsToKeep = null)
		{
			var order = GetOrder(orderId);
			return DeleteOrder(order, serviceIdsToKeep);
		}

		public IEnumerable<Task> DeleteOrder(Order order, List<Guid> serviceIdsToKeep = null)
		{
			var tasks = new List<Task>();

			var reservation = helpers.ReservationManager.GetReservation(order.Id) as ServiceReservationInstance;
			helpers.Log(nameof(OrderManager), nameof(DeleteOrder), "Reservation: " + reservation.ID, order.Name);

			var eventId = GetOrderEventGuid(reservation);
			helpers.Log(nameof(OrderManager), nameof(DeleteOrder), "Event: " + eventId, order.Name);

			// Delete Order from Event
			helpers.Log(nameof(OrderManager), nameof(DeleteOrder), "Delete order from event task", order.Name);
			var deleteOrderFromEventTask = new DeleteOrderFromEventTask(helpers, eventId, order.Id);
			tasks.Add(deleteOrderFromEventTask);
			if (!deleteOrderFromEventTask.Execute())
			{
				helpers.Log(nameof(OrderManager), nameof(DeleteOrder), "Delete order from event task failed", order.Name);
				return tasks;
			}

			List<Guid> linkedServiceIds = reservation.ResourcesInReservationInstance.Select(x => x.GUID).ToList();

			Dictionary<string, Guid> nodeLabelsWithResources = new Dictionary<string, Guid>();
			var serviceDefinition = helpers.ServiceDefinitionManager.GetRawServiceDefinition(reservation.ServiceDefinitionID);
			foreach (var node in serviceDefinition.Diagram.Nodes)
			{
				nodeLabelsWithResources.Add(node.Label, Guid.Empty);
			}

			var releaseContributingsTask = new ReleaseContributingServiceFromOrderTask(helpers, order, nodeLabelsWithResources);
			if (!releaseContributingsTask.Execute())
			{
				helpers.Log(nameof(OrderManager), nameof(DeleteOrder), $"Unable to release contributing resources from order: {order.Name}");
			}

			// Delete Services
			foreach (Guid serviceId in linkedServiceIds)
			{
				bool serviceSuccessfullyDeleted = TryDeleteService(order, serviceId, serviceIdsToKeep, out var deleteServiceTasks);

				tasks.AddRange(deleteServiceTasks);

				if (!serviceSuccessfullyDeleted)
				{
					helpers.Log(nameof(OrderManager), nameof(DeleteOrder), $"Unable to delete services: {String.Join(", ", deleteServiceTasks.Where(x => x.Status == Tasks.Status.Fail).Select(x => x.Description))}");
					return tasks;
				}
			}

			helpers.OrderManagerElement.DeleteServiceConfigurations(order.Id);

			// Delete Order from Booking Manager
			helpers.Log(nameof(OrderManager), "DeleteOrder", "Delete order task");
			var deleteOrderTask = new DeleteOrderTask(helpers, order);
			tasks.Add(deleteOrderTask);
			if (!deleteOrderTask.Execute())
			{
				helpers.Log(nameof(OrderManager), "DeleteOrder", "Delete order task failed");
				return tasks;
			}

			// remove the reference from the Order Manager element
			helpers.OrderManagerElement.DeleteOrderManagerReference(order.Id);

			helpers.Log(nameof(OrderManager), "DeleteOrder", "All tasks executed");
			return tasks;
		}

		private bool TryDeleteService(Order order, Guid serviceId, List<Guid> serviceIdsToKeep, out IEnumerable<Task> tasks)
		{
			tasks = new List<Task>();

			Service service = null;
			var serviceReservation = helpers.ResourceManager.GetReservationInstance(serviceId);
			if (serviceReservation != null) service = Service.FromReservationInstance(helpers, serviceReservation);

			if (service == null)
			{
				helpers.Log(nameof(OrderManager), nameof(DeleteOrder), $"Unable to retrieve service with ID {serviceId}, service was already removed");
				return false;
			}

			bool serviceNeedToBeKept = serviceIdsToKeep != null && serviceIdsToKeep.Contains(serviceId);
			if ((service.IsSharedSource && helpers.ServiceManager.ServiceIsUsedByOtherOrders(serviceReservation, new[] { order.Id })) || serviceNeedToBeKept)
			{
				// This means this service was added to another order and should not be removed
				service.OrderReferences.Remove(order.Id);
				service.UpdateOrderReferencesProperty(helpers, serviceReservation);
				return true;
			}

			var deleteServiceUserTask = new DeleteServiceUserTasksTask(helpers, service, order);
			tasks.Add(deleteServiceUserTask);
			if (!deleteServiceUserTask.Execute())
			{
				helpers.Log(nameof(OrderManager), nameof(DeleteOrder), "Delete service user tasks task failed");
				return false;
			}

			var deleteFromEvsTask = new DeleteFromEvsTask(helpers, service);
			tasks.Add(deleteFromEvsTask);
			if (!deleteFromEvsTask.Execute())
			{
				Log(nameof(DeleteOrder), "Delete EVS Recording Session Task failed");
				return false;
			}

			var deleteServiceTask = new DeleteServiceTask(helpers, service, order, serviceReservation);
			tasks.Add(deleteServiceTask);
			if (!deleteServiceTask.Execute())
			{
				Log(nameof(DeleteOrder), "Delete Service Task failed");
				return false;
			}

			return true;
		}

		internal void DeleteOrderReservation(Guid orderId)
		{
			var reservation = helpers.ReservationManager.GetReservation(orderId);
			DeleteOrderReservation(reservation);
		}

		private void DeleteOrderReservation(ReservationInstance reservation)
		{
			var bookingManager = new BookingManager((Engine)helpers.Engine, helpers.Engine.FindElement(OrderBookingManagerElementName)) { AllowPostroll = false, AllowPreroll = false };

			if (reservation.Status == ReservationStatus.Ongoing)
			{
				try
				{
					DataMinerInterface.BookingManager.Finish(helpers, bookingManager, reservation);
				}
				catch (Exception)
				{
					// ignore as reservation will be removed anyway in case of exception
				}
			}

			if (reservation.Status == ReservationStatus.Pending || reservation.Status == ReservationStatus.Confirmed || reservation.Status == ReservationStatus.Interrupted)
			{
				try
				{
					DataMinerInterface.BookingManager.Cancel(helpers, bookingManager, reservation);
				}
				catch (Exception)
				{
					// ignore as reservation will be removed anyway in case of exception
				}
			}

			try
			{
				DataMinerInterface.BookingManager.Delete(helpers, bookingManager, reservation);

				helpers.OrderManagerElement.DeleteServiceConfigurations(reservation.ID);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(OrderManager), "DeleteOrderReservation", $"Exception deleting reservation '{reservation.ID}': {e}");
			}
		}

		/// <summary>
		/// Retrieves the Services that were booked and are not present in the new Order.
		/// </summary>
		/// <param name="order">New order.</param>
		/// <param name="existingOrder">Optional order for comparison.</param>
		/// <returns>Services that were booked and are not present in the new Order.</returns>
		internal List<Service> GetServicesToRemove(Order order, Order existingOrder = null)
		{
			var existingOrderServices = existingOrder == null ? FlattenServices(helpers.ServiceManager.GetOrderServices(order.Id)) : existingOrder.AllServices;

			var newOrderServices = order.AllServices.ToList();

			// A Service should be removed when it has a valid GUID (non-empty) and no service with that GUID is present in the new Order
			var servicesToRemove = new List<Service>();
			foreach (var existingService in existingOrderServices)
			{
				if (!newOrderServices.Any(s => s.Id == existingService.Id)) servicesToRemove.Add(existingService);
			}

			return servicesToRemove;
		}

		/// <summary>
		/// Creates a list containing the provided services and all their underlying child services.
		/// </summary>
		/// <remarks>To get all Services in an Order, you can use the AllServices property on Order.</remarks>
		/// <param name="services">Services to check. These services are included in the returned collection.</param>
		/// <returns>Collection containing the provided services and all of their underlying child services.</returns>
		public static List<YLE.Service.Service> FlattenServices(IEnumerable<YLE.Service.Service> services)
		{
			List<YLE.Service.Service> flattenedServices = new List<YLE.Service.Service>();
			foreach (YLE.Service.Service service in services)
			{
				flattenedServices.Add(service);
				flattenedServices.AddRange(FlattenServices(service.Children));
			}

			return flattenedServices;
		}

		public void UpdateServiceOrderIds(Order order)
		{
			foreach (var service in order.AllServices)
			{
				service.OrderReferences.Clear();

				if (!service.IsBooked) continue;

				service.OrderReferences.Add(order.Id);
				service.AddOrUpdateOrderIdsProperty(helpers);
			}
		}

		private static OrderType GetOrderType(ReservationInstance reservationInstance)
		{
			if (!reservationInstance.Properties.Dictionary.TryGetValue(LiteOrder.PropertyNameType, out var type))
			{
				throw new PropertyNotFoundException($"Reservation {reservationInstance.Name} ({reservationInstance.ID}) does not contain a Type property");
			}

			return (OrderType)Enum.Parse(typeof(OrderType), Convert.ToString(type));
		}

		private static RecurringSequenceInfo GetRecurringSequenceInfo(ReservationInstance reservation)
		{
			RecurringSequenceInfo recurringSequenceInfo = new RecurringSequenceInfo();
			if (reservation.Properties.Dictionary.TryGetValue(LiteOrder.PropertyNameRecurrence, out object recurrenceObject))
			{
				recurringSequenceInfo = JsonConvert.DeserializeObject<RecurringSequenceInfo>(Convert.ToString(recurrenceObject));
				recurringSequenceInfo.Recurrence.IsConfigured = true;

				var templateIdString = GetStringProperty(reservation, LiteOrder.PropertyNameFromTemplate);
				if (!string.IsNullOrWhiteSpace(templateIdString))
				{
					recurringSequenceInfo.TemplateId = Guid.Parse(templateIdString);
				}
			}

			return recurringSequenceInfo;
		}

		private BillingInfo GetBillingInfo(ReservationInstance reservation)
		{
			BillingInfo billingInfo = new BillingInfo();

			try
			{
				string serializedBillingInfo = GetStringProperty(reservation, LiteOrder.PropertyNameBillingInfo);

				if (String.IsNullOrWhiteSpace(serializedBillingInfo))
				{
					//If billing info property on order reservation has invalid value, fix it by setting it to default value

					billingInfo = new BillingInfo { BillableCompany = "YLE", CustomerCompany = string.Empty };

					DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(helpers, reservation, new Dictionary<string, object> { { LiteOrder.PropertyNameBillingInfo, JsonConvert.SerializeObject(billingInfo, Formatting.None) } });
				}
				else
				{
					billingInfo = JsonConvert.DeserializeObject<BillingInfo>(serializedBillingInfo);
				}
			}
			catch (Exception)
			{
				// No action
			}

			return billingInfo;
		}

		private static IntegrationType GetIntegrationType(ReservationInstance reservationInstance)
		{
			object type = null;
			if (!reservationInstance.Properties.Dictionary.TryGetValue(LiteOrder.PropertyNameIntegration, out type))
			{
				return IntegrationType.None;
			}

			return EnumExtensions.GetEnumValueFromDescription<IntegrationType>(Convert.ToString(type));
		}

		private static SportsPlanning GetSportsPlanning(ReservationInstance reservationInstance)
		{
			SportsPlanning sportsPlanning = new SportsPlanning();

			foreach (var property in reservationInstance.Properties.Dictionary)
			{
				switch (property.Key)
				{
					case LiteOrder.PropertyNameSportsplanningSport:
						sportsPlanning.Sport = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningDescr:
						sportsPlanning.Description = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningCommentary:
						sportsPlanning.Commentary = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningCommentary2:
						sportsPlanning.Commentary2 = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningCompetitionTime:
						sportsPlanning.CompetitionTime = Convert.ToDouble(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningJournalist1:
						sportsPlanning.JournalistOne = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningJournalist2:
						sportsPlanning.JournalistTwo = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningJournalist3:
						sportsPlanning.JournalistThree = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningLocation:
						sportsPlanning.Location = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningTechResources:
						sportsPlanning.TechnicalResources = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningLivehighlights:
						sportsPlanning.LiveHighlightsFile = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningReqBroadcastTime:
						sportsPlanning.RequestedBroadcastTime = Convert.ToDouble(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningProdNrPlasmaId:
						sportsPlanning.ProductionNumberPlasmaId = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningProdNrCeiton:
						sportsPlanning.ProductNumberCeiton = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningCostDep:
						sportsPlanning.CostDepartment = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameSportsplanningAdditionalInformation:
						sportsPlanning.AdditionalInformation = Convert.ToString(property.Value);
						break;

					default:
						// nothing
						break;
				}
			}

			return sportsPlanning;
		}

		private static NewsInformation GetNewsInformation(ReservationInstance reservationInstance)
		{
			NewsInformation newsInformation = new NewsInformation();

			foreach (var property in reservationInstance.Properties.Dictionary)
			{
				switch (property.Key)
				{
					case LiteOrder.PropertyNameNewsInformationNewsCameraOperator:
						newsInformation.NewsCameraOperator = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameNewsInformationJournalist:
						newsInformation.Journalist = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameNewsInformationVirveCommandGroupOne:
						newsInformation.VirveCommandGroupOne = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameNewsInformationVirveCommandGroupTwo:
						newsInformation.VirveCommandGroupTwo = Convert.ToString(property.Value);
						break;
					case LiteOrder.PropertyNameNewsInformationAdditionalInformation:
						newsInformation.AdditionalInformation = Convert.ToString(property.Value);
						break;
					default:
						// nothing
						break;
				}
			}

			return newsInformation;
		}

		private static HashSet<int> GetUserGroupIds(ReservationInstance reservation)
		{
			HashSet<int> userGroupIds = new HashSet<int>();

			object reservationUserGroups = null;
			if (reservation.Properties.Dictionary.TryGetValue(LiteOrder.PropertyNameUsergroups, out reservationUserGroups))
			{
				try
				{
					userGroupIds = new HashSet<int>(Convert.ToString(reservationUserGroups).Replace("'", String.Empty).Split(new[] { ',' }).Select(x => Convert.ToInt32(x)));
				}
				catch (Exception)
				{
					return userGroupIds;
				}
			}

			return userGroupIds;
		}

		private static bool GetIsInternal(ReservationInstance reservation)
		{
			if (reservation.Properties.Dictionary.TryGetValue(LiteOrder.PropertyNameInternal, out var isInternal))
			{
				return Boolean.TryParse(Convert.ToString(isInternal), out var result) && result;
			}

			return false;
		}

		private static string GetStringProperty(ReservationInstance reservation, string propertyName)
		{
			if (reservation.Properties.Dictionary.TryGetValue(propertyName, out var property))
			{
				return property.ToString();
			}

			return String.Empty;
		}

		private static Status GetOrderStatus(ReservationInstance reservationInstance)
		{
			object status = null;
			if (!reservationInstance.Properties.Dictionary.TryGetValue(LiteOrder.PropertyNameStatus, out status))
			{
				throw new StatusPropertyNotFoundNotFoundException(reservationInstance.Name, reservationInstance.ID);
			}

			return EnumExtensions.GetEnumValueFromDescription<Status>(Convert.ToString(status));
		}

		public static Guid GetOrderEventGuid(ReservationInstance reservationInstance)
		{
			if (reservationInstance is null) throw new ArgumentNullException(nameof(reservationInstance));

			if (!reservationInstance.Properties.Dictionary.TryGetValue(LiteOrder.PropertyNameEventId, out object eventId))
			{
				return Guid.Empty;
			}

			if (!Guid.TryParse(eventId.ToString(), out var eventGuid))
			{
				//return Guid.Empty;
				throw new InvalidOperationException($"Unable to parse GUID: '{eventId.ToString()}'");
			}

			return eventGuid;
		}

		internal ReservationInstance AddOrUpdateOrderReservation(Order order)
		{
			var bookingManagerElement = helpers.Engine.FindElement(order.Definition.BookingManagerElementName) ?? throw new BookingManagerNotFoundException(order.Definition.BookingManagerElementName, order.Id);

			var bookingManager = new BookingManager((Engine)helpers.Engine, bookingManagerElement) { AllowPostroll = true, AllowPreroll = true, CustomProperties = true };

			var bookingManagerInfo = new BookingManagerInfo
			{
				Element = order.Definition.BookingManagerElementName,
				Action = order.Id == Guid.Empty ? BookingOperationAction.New : BookingOperationAction.Edit,
				TableIndex = order.Id == Guid.Empty ? null : order.Id.ToString()
			};

			try
			{
				var bookingData = order.GetBookingDataForBooking(helpers);
				var serviceFunctions = order.GetServiceFunctions(helpers);
				var orderProperties = order.GetPropertiesForBooking(helpers, bookingManager.Properties);

				if (bookingManagerInfo.Action == BookingOperationAction.New)
				{
					order.Reservation = DataMinerInterface.BookingManager.CreateNewBooking(helpers, bookingManager, bookingData, serviceFunctions, null, orderProperties);

					helpers.AddOrderReferencesForLogging(order.Reservation.ID);
				}
				else
				{
					order.Reservation = DataMinerInterface.BookingManager.EditBooking(helpers, bookingManager, order.Id, bookingData, serviceFunctions, null, orderProperties);
				}
			}
			catch (Exception e)
			{
				throw new CreateNewBookingFailedException($"{(bookingManagerInfo.Action == BookingOperationAction.New ? "Creating" : "Editing")} booking for order {order.Name} failed", e);
			}

			order.Reservation = order.Reservation.ChangeStateToConfirmedWithRetry(helpers, bookingManager);

			order.Id = order.Reservation.ID;
			order.CreatedByUserName = Convert.ToString(order.Reservation.GetPropertyByName(LiteOrder.PropertyNameCreatedBy));

			foreach (var service in order.AllServices)
			{
				if (!service.IsSharedSource)
				{
					Log(nameof(AddOrUpdateOrderReservation), $"Clearing order references for non-shared service: {service.Name}");
					service.OrderReferences.Clear();
				}

				service.OrderReferences.Add(order.Id);
				if (service.IsBooked) service.UpdateOrderReferencesProperty(helpers); // Running edit: Still needed as some of the already running services are nearly untouched during book services flow.
			}

			if (!helpers.OrderManagerElement.AddOrUpdateServiceConfigurations(order)) throw new ServiceConfigurationsUpdateFailedException(order.Name);

			order.UpdateSecurityViewIds(helpers, new HashSet<int>(order.SecurityViewIds));

			TryMoveExternalJsonFileToCorrectDirectory(order);

			return order.Reservation;
		}

		private void TryMoveExternalJsonFileToCorrectDirectory(Order order)
		{
			try
			{
				bool orderWasCreatedFromJson = !string.IsNullOrWhiteSpace(order.ExternalJsonFilePath);
				bool jsonIsStoredInCorrectDirectory = orderWasCreatedFromJson && order.ExternalJsonFilePath.Contains(order.Id.ToString());

				if (orderWasCreatedFromJson && !jsonIsStoredInCorrectDirectory && File.Exists(order.ExternalJsonFilePath))
				{
					string fileName = Path.GetFileName(order.ExternalJsonFilePath);
					string dedicatedOrderAttachmentsDirectory = Path.Combine(OrderAttachmentsDirectory, order.Id.ToString());

					if (!Directory.Exists(dedicatedOrderAttachmentsDirectory))
					{
						Directory.CreateDirectory(dedicatedOrderAttachmentsDirectory);
					}

					string destinationFilePath = Path.Combine(dedicatedOrderAttachmentsDirectory, fileName);

					File.Move(order.ExternalJsonFilePath, destinationFilePath);

					var message = new SetDataMinerInfoMessage
					{
						What = (int)NotifyType.SendDmsFileChange,
						StrInfo1 = destinationFilePath,
						IInfo2 = (int)NotifyType.FileAdd
					};

					helpers.Engine.SendSLNetSingleResponseMessage(message);
				}
			}
			catch (Exception e)
			{
				Log(nameof(TryMoveExternalJsonFileToCorrectDirectory), $"Exception occurred: {e}", order.Name);
			}
		}

		internal void AddSatelliteReceptionSynopsisAttachments(Order order)
		{
			bool serviceConfigUpdateRequired = false;

			var satelliteReceptions = order.AllServices.Where(x => x.Definition.VirtualPlatform == YLE.ServiceDefinition.VirtualPlatform.ReceptionSatellite);

			int initialAmountOfAttachments = order.AllServices.Sum(s => s.SynopsisFiles.Count());

			foreach (var satRxService in satelliteReceptions)
			{
				string orderAttachmentsFolder = CreateOrderAttachmentsFolder(order);
				var invalidFiles = new List<string>();
				var copiedFiles = CopySynopsisFiles(satRxService, orderAttachmentsFolder);

				serviceConfigUpdateRequired |= ServiceConfigUpdatesRequired(copiedFiles, invalidFiles);

				RemoveInvalidFilesFromSynopsis(satRxService, invalidFiles);
				UpdateSynopsisFilePaths(satRxService, copiedFiles);
			}

			int actualAmountOfAttachments = order.AllServices.Sum(s => s.SynopsisFiles.Count());

			UpdateOrderConfigurationIfRequired(order, serviceConfigUpdateRequired);
			UpdateAttachmentCountIfChanged(order, initialAmountOfAttachments != actualAmountOfAttachments);
		}

		private string CreateOrderAttachmentsFolder(Order order)
		{
			string orderAttachmentsFolder = Path.Combine(OrderAttachmentsDirectory, order.Id.ToString());
			if (!Directory.Exists(orderAttachmentsFolder))
			{
				Directory.CreateDirectory(orderAttachmentsFolder);
			}
			return orderAttachmentsFolder;
		}

		private Dictionary<string, string> CopySynopsisFiles(Service satRxService, string orderAttachmentsFolder)
		{
			Dictionary<string, string> copiedFiles = new Dictionary<string, string>();
			foreach (var synopsisFilePath in satRxService.SynopsisFiles.ToList())
			{
				var fileName = Path.GetFileName(synopsisFilePath);
				if (!fileName.StartsWith("Synopsis_"))
				{
					fileName = "Synopsis_" + fileName;
				}

				string destinationFilePath = Path.Combine(orderAttachmentsFolder, fileName);
				if (File.Exists(destinationFilePath))
				{
					continue; // File was already available
				}

				if (!File.Exists(synopsisFilePath))
				{
					satRxService.SynopsisFiles.Remove(synopsisFilePath);
				}
				else
				{
					File.Move(synopsisFilePath, destinationFilePath);

					var message = new SetDataMinerInfoMessage
					{
						What = (int)NotifyType.SendDmsFileChange,
						StrInfo1 = destinationFilePath,
						IInfo2 = (int)NotifyType.FileAdd
					};

					helpers.Engine.SendSLNetSingleResponseMessage(message);

					Log(nameof(CopySynopsisFiles), $"Moved file {synopsisFilePath} to {destinationFilePath} and triggered DataMiner sync");

					copiedFiles.Add(synopsisFilePath, destinationFilePath);
				}
			}

			return copiedFiles;
		}

		private bool ServiceConfigUpdatesRequired(Dictionary<string, string> copiedFiles, List<string> invalidFiles)
		{
			return copiedFiles.Any() || invalidFiles.Any();
		}

		private void RemoveInvalidFilesFromSynopsis(Service satRxService, List<string> invalidFiles)
		{
			foreach (var invalidFile in invalidFiles)
			{
				satRxService.SynopsisFiles.Remove(invalidFile);
			}
		}

		private void UpdateSynopsisFilePaths(Service satRxService, Dictionary<string, string> copiedFiles)
		{
			foreach (var kvp in copiedFiles)
			{
				satRxService.SynopsisFiles.Remove(kvp.Key);
				satRxService.SynopsisFiles.Add(kvp.Value);
			}
		}

		private void UpdateOrderConfigurationIfRequired(Order order, bool serviceConfigUpdateRequired)
		{
			if (serviceConfigUpdateRequired)
			{
				order.UpdateServiceConfigurationProperty(helpers);
			}
		}

		private void UpdateAttachmentCountIfChanged(Order order, bool attachmentsChanged)
		{
			if (attachmentsChanged)
			{
				order.UpdateAttachmentCount(helpers);
			}
		}

		internal void RemoveSatelliteReceptionSynopsisAttachments(Order order)
		{
			bool attachmentsChanged = false;
			foreach (var satRxService in order.AllServices.Where(x => x?.Definition.VirtualPlatform == YLE.ServiceDefinition.VirtualPlatform.ReceptionSatellite))
			{
				History.ServiceChange change = (History.ServiceChange)satRxService.Change;
				var changedSynopsisFiles = change.CollectionChanges.FirstOrDefault(x => x.CollectionName.Equals(nameof(satRxService.SynopsisFiles)));

				if (changedSynopsisFiles == null)
				{
					helpers.Log(nameof(OrderManager), nameof(RemoveSatelliteReceptionSynopsisAttachments), $"Unable to retrieve changes for SynopsisFiles collection");
					continue;
				}

				foreach (string removedSynopsisFile in changedSynopsisFiles.Changes.Where(x => x.Type == History.CollectionChangeType.Remove).Select(x => x.ItemIdentifier))
				{
					helpers.Log(nameof(OrderManager), nameof(RemoveSatelliteReceptionSynopsisAttachments), $"User removed Synopsis file: {removedSynopsisFile}");
					if (!File.Exists(removedSynopsisFile)) continue;

					File.Delete(removedSynopsisFile);
					attachmentsChanged = true;

					var message = new SetDataMinerInfoMessage
					{
						What = (int)NotifyType.SendDmsFileChange,
						StrInfo1 = removedSynopsisFile,
						IInfo2 = (int)NotifyType.FileRemoved
					};

					helpers.Engine.SendSLNetSingleResponseMessage(message);

					Log(nameof(RemoveSatelliteReceptionSynopsisAttachments), $"Succesfully removed Synopsis file: {removedSynopsisFile}");
				}
			}

			if (attachmentsChanged) order.UpdateAttachmentCount(helpers);
		}

		/// <summary>
		/// This method will start the Order and its services when the StartNow property on the Order is set and all usertasks have been completed.
		/// </summary>
		/// <param name="helpers"></param>
		/// <param name="order">Order to check and start.</param>
		public void TryStartOrderNow(Helpers helpers, Order order)
		{
			if (!order.StartNow) return;
			if (order.AllServices.Exists(x => x.UserTasks.Exists(y => y.Status != UserTasks.UserTaskStatus.Complete)))
			{
				helpers.Log(nameof(OrderManager), nameof(TryStartOrderNow), "not all user tasks are completed");
				return; // An order should only start now when all user tasks are completed
			}

			// Start Services Now
			foreach (var service in order.AllServices)
			{
				if (service.IsOrShouldBeRunning || (order.PreviousRunningOrderId != Guid.Empty && !service.ShouldStartDirectly)) continue;

				service.StartNow(helpers);
			}

			// Start Order Now
			StartOrderNow(helpers, order);

			// Update Service Configuration so it contains the latest service start times
			order.UpdateServiceConfigurationProperty(this.helpers);
		}

		public void StartOrderNow(Helpers helpers, Order order, bool clearStartNowFlag = true)
		{
			var bookingManagerElement = this.helpers.Engine.FindElement(order.Definition.BookingManagerElementName);
			if (bookingManagerElement == null) throw new BookingManagerNotFoundException(order.Definition.BookingManagerElementName, order.Id);

			var bookingManager = new BookingManager((Engine)this.helpers.Engine, bookingManagerElement) { AllowPostroll = true, AllowPreroll = true, CustomProperties = true };

			var orderReservation = DataMinerInterface.ResourceManager.GetReservationInstance(this.helpers, order.Id);

			// Clear StartNow flag
			if (clearStartNowFlag)
			{
				try
				{
					DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(this.helpers, orderReservation, new Dictionary<string, object> { { LiteOrder.PropertyNameStartnow, false.ToString() } });
					order.StartNow = false;
				}
				catch (Exception e)
				{
					helpers.Log(nameof(OrderManager), nameof(StartOrderNow), $"Updating StartNow flag on the ReservationInstance failed: {e}");
				}
			}

			bookingManager.EventReschedulingDelay = TimeSpan.Zero;
			if (!order.ShouldBeRunning && DataMinerInterface.BookingManager.TryStart(this.helpers, bookingManager, ref orderReservation))
			{
				helpers.Log(nameof(OrderManager), nameof(StartOrderNow), order.Name + " was started");
				helpers.Log(nameof(OrderManager), nameof(StartOrderNow), "new start time: " + orderReservation.Start);

				order.Start = DateTimeExtensions.RoundToMinutes(orderReservation.Start.FromReservation());
			}
			else
			{
				helpers.Log(nameof(OrderManager), nameof(StartOrderNow), "Unable to start Order " + order.Name + $" now | Should order already be running: {order.ShouldBeRunning}");
			}
		}

		internal void UpdateExistingServicesSecurityViewIds(Order order, Order existingOrder)
		{
			if (order == null || existingOrder == null)
			{
				Log(nameof(UpdateExistingServicesSecurityViewIds), $"Order is null: {order == null} , existing order is null: {existingOrder == null}");
				return;
			}

			foreach (var service in existingOrder.AllServices)
			{
				service.UpdateSecurityViewIds(helpers, order.SecurityViewIds);
			}
		}

		/// <summary>
		/// Method called by the booking manager to update Operational UI filter property.
		/// </summary>
		/// <param name="orderReservationGuid">Guid of the Order Reservation Instance.</param>
		public bool UpdateUIProperties(Guid orderReservationGuid)
		{
			Order order = GetOrder(orderReservationGuid);
			if (order == null) return false;

			return order.UpdateUiProperties(helpers);
		}

		public bool UpdateEventNameProperty(Guid orderReservationId, string eventName)
		{
			ReservationInstance orderReservation = helpers.ServiceManager.GetReservation(orderReservationId);

			if (orderReservation == null)
			{
				return false;
			}

			DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(helpers, orderReservation, new Dictionary<string, object> { { LiteOrder.PropertyNameEventName, eventName } });

			return true;
		}
	}
}