namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using NPOI.SS.Formula.Functions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceUpdates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceDefinitionTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.AssignProfilesAndResources;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Service = Service.Service;

	public abstract class OrderUpdateHandler
	{
		[Flags]
		public enum OptionFlags
		{
			None = 0,
			ForceAllOrderCustomPropertiesUpdate = 1,
			SkipDetermineOrderDefinition = 2,
			SkipServiceResourceAssignment = 4,
			SkipAddOrUpdateOrderDefinition = 8,
			SkipAddOrUpdateEvent = 16,
			SkipContributingResourceAssignment = 32,
			ForceGetExistingEvent = 64,
			SkipUpdatingOrderManager = 128,
			SkipGenerateProcessing = 256,
		}

		protected readonly Order order;
		protected Order existingOrder;
		protected Event existingEvent;
		protected bool isNewOrder;
		protected readonly bool isHighPriority;
		protected readonly bool processChronologically;
		protected readonly OptionFlags options;
		protected readonly List<Task> tasks = new List<Task>();
		protected readonly List<System.Action> actionsToExecute = new List<System.Action>();

		protected List<Service> servicesToRemove = new List<Service>();
		protected List<Service> servicesForWhichToCreateUserTasks = new List<Service>();
		protected bool orderGotNewOrUpdatedServiceDefinition;
		protected bool orderHasNewEvent;
		protected bool isSuccessful = true;
		protected bool preliminaryOrderBooked;

		protected OrderUpdateHandler(Helpers helpers, Order order, OrderUpdateHandlerInput input)
		{
			Helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			if (input is null) throw new ArgumentNullException(nameof(input));

			isNewOrder = order.Id == Guid.Empty;
			this.isHighPriority = input.IsHighPriority;
			this.processChronologically = input.ProcessChronologically;
			this.options = input.Options;
			this.existingOrder = input.ExistingOrder;
		}

		public Helpers Helpers { get; }

		public List<Exception> Exceptions { get; } = new List<Exception>();

		public UpdateResult Execute()
		{
			LogMethodStart(nameof(Execute), out var stopwatch);

			Log(nameof(Execute), $"Options: {string.Join(", ", options.GetFlags())}");

			CollectActionsToExecute();

			foreach (var action in actionsToExecute.TakeWhile(a => isSuccessful))
			{
				LogMethodStart(action.Method.Name, out var stopwatch2);

				try
				{
					action();
				}
				catch (Exception e)
				{
					Log(nameof(Execute), $"Exception occurred outside of a task: {e}");
					Exceptions.Add(e);
					isSuccessful = false;
				}

				LogMethodCompleted(action.Method.Name, stopwatch2);
			}

			var duration = stopwatch.Elapsed;
			LogMethodCompleted(nameof(Execute), stopwatch);

			Exceptions.AddRange(tasks.Select(t => t.Exception).Where(e => e != null).ToList());

			return new UpdateResult
			{
				UpdateWasSuccessful = !(tasks.Any(t => t.Status != Tasks.Status.Ok) || Exceptions.Any() || !isSuccessful),
				Tasks = tasks.ToList(),
				Exceptions = Exceptions,
				Duration = duration,
			};
		}

		protected void GetExistingOrderAndEvent()
		{
			if (isNewOrder)
			{
				existingOrder = null;

				// Use cases:
				// Manual add Order without existing Event: Order.Event property is null => existing event is null
				// Manual add Order under existing Event: Order.Event property is existing booked Event => existing event is a copy
				// Integration add Order: Order.Event property is non-booked new Event => existing event is null

				existingEvent = order.IntegrationType != IntegrationType.None ? null : (Event)order.Event?.Clone();

				return;
			}

			LogMethodStart(nameof(GetExistingOrderAndEvent), out var stopwatch);
			Report("Getting changes ...");

			try
			{
				order.Reservation = Helpers.ReservationManager.GetReservation(order.Id);
				this.existingOrder = existingOrder ?? Helpers.OrderManager.GetOrder(order.Reservation, true, !options.HasFlag(OptionFlags.ForceGetExistingEvent));

				if (options.HasFlag(OptionFlags.ForceGetExistingEvent))
				{
					// Use cases:
					// Integration edit Order: order.Event property has already been changed by script code => retrieve existing event.

					this.existingEvent = existingOrder.Event;
				}
				else
				{
					// Use cases:
					// Manual edit order: order.Event property is an existing booked event => existingEvent is a copy.

					existingOrder.Event = (Event)order.Event.Clone();
					this.existingEvent = existingOrder.Event;
				}

				Log(nameof(GetExistingOrderAndEvent), $"Set existing event to {existingEvent.ToString()}");

				this.preliminaryOrderBooked = (order.Status == YLE.Order.Status.Confirmed || order.Status == YLE.Order.Status.Planned) && existingOrder.IsSaved;
			}
			catch (Exception e)
			{
				Log(nameof(GetExistingOrderAndEvent), "Something went wrong: " + e);
				Exceptions.Add(e);
				isSuccessful = false;
			}

			LogMethodCompleted(nameof(GetExistingOrderAndEvent), stopwatch);
			Report("Getting changes succeeded");
		}

		/// <summary>
		/// This method will remove the old order in a specific use case.
		/// When an order has status preliminary and a user enables StartNow when the order reservation is already ongoing.
		/// In this case we cannot book the order as we cannot change the Service Definition of an ongoing order or adjust the start time.
		/// </summary>
		protected void RemoveRunningPreliminaryStartNowOrder()
		{
			if (isNewOrder) return;

			bool orderIsNotRunning = existingOrder.StartWithPreRoll > DateTime.Now;
			if (!preliminaryOrderBooked || !order.StartNow || orderIsNotRunning) return;

			Helpers.Log(nameof(OrderUpdateHandler), nameof(RemoveRunningPreliminaryStartNowOrder), $"Removing running preliminary order: {existingOrder.Name}...");

			// Remove existing order
			Helpers.OrderManager.DeleteOrder(existingOrder, existingOrder.AllServices.Where(x => x.IsSharedSource || !x.IsBooked).Select(x => x.Id).ToList());

			isNewOrder = true;
			order.Id = Guid.Empty;
			order.Definition = new ServiceDefinition
			{
				BookingManagerElementName = SrmConfiguration.OrderBookingManagerElementName
			};
		}

		/// <summary>
		/// This step is required in case a major time slot change is performed (= moving order to a new, non-overlapping timeslot).
		/// Due to the recent routing chain generation changes, the timings of the routing services is not being updated in the AssignResourcesToServices step.
		/// If the timings of these services is not updated, only the services that are displayed in the foreground will be moved to the correct timeslot (these timings are update from the foreground)
		/// If the auto generated services are not updated, they will keep their original timing, will not move to the new timeslot and will cause the order timing to be incorrect.
		/// </summary>
		protected void UpdateAutoGeneratedServiceTimings()
		{
			if (isNewOrder) return;

			var serviceEndPoints = order.AllServices.Where(x => !x.Children.Any());
			foreach (var serviceEndPoint in serviceEndPoints)
			{
				order.UpdateAutoGeneratedServiceTimings(Helpers, serviceEndPoint);
			}
		}

		protected abstract void CollectActionsToExecute();

		protected void ClearOrderResources()
		{
			try
			{
				order.Reservation.ResourcesInReservationInstance.Clear();
				DataMinerInterface.ResourceManager.AddOrUpdateReservationInstances(Helpers, order.Reservation);
			}
			catch (Exception e)
			{
				Log(nameof(ClearOrderResources), $"Exception removing resource references from order reservation: {e}");
			}
		}

		protected void ReleaseOrderResources()
		{
			ReleaseContributingServiceFromOrderTask releaseContributingServiceFromOrderTask = new ReleaseContributingServiceFromOrderTask(Helpers, existingOrder, servicesToRemove);
			tasks.Add(releaseContributingServiceFromOrderTask);
			releaseContributingServiceFromOrderTask.Execute();
		}

		protected void CollectAllPossibleServicesToRemove()
		{
			foreach (var service in order.AllServices)
			{
				Log(nameof(CollectAllPossibleServicesToRemove), $"Checking if service {service.Name} can be removed");

				if (!service.IsBooked)
				{
					Log(nameof(CollectAllPossibleServicesToRemove), $"Service {service.Name} is not booked and does not need to be removed");
					continue;
				}

				if (service.IsSharedSource)
				{
					Log(nameof(CollectAllPossibleServicesToRemove), $"Service {service.Name} is a Shared Source");
					service.OrderReferences.Remove(order.Id);

					// if this order was the only one still using the event level reception then it can safely be removed
					if (service.OrderReferences.Count > 0)
					{
						Log(nameof(CollectAllPossibleServicesToRemove), $"Event level reception {service.Name} is used by other order(s) and can not be removed (updating order references property)");

						// in this case this event level reception is still used in another order and should not yet be removed
						// we do need to update the property to make sure this order is not referenced anymore
						service.UpdateOrderReferencesProperty(Helpers);

						continue;
					}
					else
					{
						Log(nameof(CollectAllPossibleServicesToRemove), $"Event level reception {service.Name} is not used by any other order and can be removed");
					}
				}

				Log(nameof(CollectAllPossibleServicesToRemove), $"Service {service.Name} will be removed");
				servicesToRemove.Add(service);
			}
		}

		protected void ReleaseRoutingResourcesInBackground()
		{
			// This method is used to allow sharing of routing services between multiple services.
			// Releasing the routing resources will make sure they are present in the return value when querying the available routing resources, allowing them to be selected by multiple routing services and enabling our routing sharing logic.
			// Also required to enable correct news destination resource selection.

			// IMPORTANT: Resources should only be released if we know they will be reassigned shortly after.
			// This is the case when going through the Book Services flow, as we update services (and assign resources to reservations) shortly after routing generation.
			// However during AddOrUpdate this is not the case, as there is no service update step in that flow to assign resources to reservations. 

			LogMethodStart(nameof(ReleaseRoutingResourcesInBackground), out var stopwatch);

			foreach (var service in order.AllServices)
			{
				if (!service.IsBooked || service.Definition.VirtualPlatform != VirtualPlatform.Routing) continue;

				var reservation = (ServiceReservationInstance)DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, service.Id);
				if (!reservation.ResourcesInReservationInstance.Any())
				{
					Log(nameof(ReleaseRoutingResourcesInBackground), $"No resources assigned to routing service reservation {service.Name}");
					continue;
				}

				var requests = new List<AssignResourceRequest>();
				foreach (var function in service.Functions)
				{
					requests.Add(new AssignResourceRequest
					{
						NewResourceId = Guid.Empty,
						TargetNodeLabel = function.Definition.Label,
					});
				}

				DataMinerInterface.ReservationInstance.AssignResources(Helpers, reservation, requests.ToArray());

				Log(nameof(ReleaseRoutingResourcesInBackground), $"Released resources {string.Join(", ", service.Functions.Select(r => r.ResourceName))} in background for routing service reservation {service.Name}");
			}

			LogMethodCompleted(nameof(ReleaseRoutingResourcesInBackground), stopwatch);
		}

		protected void AddOrUpdateOrderReservation()
		{
			if (order.Event is null)
			{
				// Because of syncing issues it can happen that we were unable to retrieve the Event at the beginning of the book services flow, in that case we retry to get the Event here as we will need it starting from this step.
				// This property should never be null in Add or Update flow.

				var eventId = OrderManager.GetOrderEventGuid(order.Reservation);

				Log(nameof(AddOrUpdateOrderReservation), $"Event is null, retrying to get Event '{eventId}'");

				order.Event = Helpers.EventManager.GetEvent(eventId) ?? throw new EventNotFoundException(eventId);

				existingOrder.Event = (Event)order.Event?.Clone();
				this.existingEvent = existingOrder.Event;
			}

			if (FullOrderAddOrUpdateRequired() && FullOrderAddOrUpdateAllowed())
			{
				FullOrderAddOrUpdate();
			}
			else
			{
				PartialOrderAddOrUpdate();
			}
		}

		private bool FullOrderAddOrUpdateAllowed()
		{
			if (isNewOrder) return true;

			Log(nameof(FullOrderAddOrUpdateAllowed), $"BookingManager.EditBooking call is{(order.ShouldBeRunning ? " not" : string.Empty)} allowed{(order.ShouldBeRunning ? $", because order is or should be running" : string.Empty)}, order start with preroll: {order.StartWithPreRoll}, utc now: {DateTime.Now}. (Order Start Now property = {order.StartNow})");

			return !order.ShouldBeRunning;
		}

		protected void ReleaseLocks()
		{
			Helpers.LockManager.ReleaseLocks();
		}

		protected void UpdateOrderManagerAndContractManagerElement()
		{
			var bookedServicesToRemoveIds = servicesToRemove.Where(s => s.IsBooked).Select(s => s.Id.ToString()).Distinct().ToList();

			// add the order reference to the order manager element
			if (options.HasFlag(OptionFlags.SkipUpdatingOrderManager))
			{
				Log(nameof(UpdateOrderManagerAndContractManagerElement), "Skipping updating Order Manager element because of option flag");
			}
			else if (!Helpers.OrderManagerElement.UpdateOrderManagerReference(order, bookedServicesToRemoveIds, isHighPriority, processChronologically))
			{
				Log(nameof(UpdateOrderManagerAndContractManagerElement), "Order reference could not be added or updated in Order Manager element");
			}
			else
			{
				//Nothing
			}

			if (order.ChangeTrackingStarted)
			{
				var orderChangeToAddToHistory = isNewOrder ? ((OrderChange)order.Change).GetChangeForCreationHistory() : order.Change.GetActualChanges() as OrderChange;

				var orderHistoryChapter = new OrderHistoryChapter(orderChangeToAddToHistory, Helpers.Engine.UserDisplayName, DateTime.Now, Helpers.Context.Script.GetDescription());

				Log(nameof(UpdateOrderManagerAndContractManagerElement), $"Sending order history chapter: '{orderHistoryChapter.ToString()}'");

				Helpers.OrderManagerElement.AddOrUpdateOrderHistory(order.Id, orderHistoryChapter);
			}

			if (order.RecurringSequenceInfo.Recurrence.IsConfigured && order.Status != YLE.Order.Status.Preliminary)
			{
				bool recurringOrderIsRegisteredInOrderManager = order.RecurringSequenceInfo.Id != Guid.Empty;

				if (!recurringOrderIsRegisteredInOrderManager)
				{
					RegisterRecurringOrder();
				}
				else if (order.RecurringSequenceInfo.RecurrenceAction == RecurrenceAction.AllOrdersInSequence)
				{
					UpdateRecurringOrders();
				}
				else
				{
					// nothing to do
				}
			}
		}

		private void UpdateRecurringOrders()
		{
			if (!Helpers.ContractManager.TryGetOrderTemplate(order.RecurringSequenceInfo.Id, out var existingTemplate))
			{
				Log(nameof(UpdateRecurringOrders), "Recurring order template could not be retrieved from Contract Manager element");
			}

			var newTemplate = OrderTemplate.FromOrder(Helpers, order, existingTemplate.Name, true);
			newTemplate.Id = existingTemplate.Id;

			bool templateHasChanged = !OrderTemplate.OrderTemplatesAreEqual(Helpers, existingTemplate, newTemplate);

			Log(nameof(UpdateRecurringOrders), $"Recurring order template has {(templateHasChanged ? string.Empty : "not ")}changed");

			if (templateHasChanged && !Helpers.ContractManager.TryEditOrderTemplate(newTemplate, new string[0]))
			{
				Log(nameof(UpdateRecurringOrders), $"Recurring order template {existingTemplate.Name} could not be edited in Contract Manager element");
				throw new InvalidOperationException($"Recurring order template {existingTemplate.Name} could not be edited in Contract Manager element");
			}

			Helpers.OrderManagerElement.UpdateRecurringOrder(order, existingTemplate.Id, templateHasChanged);
		}

		private void RegisterRecurringOrder()
		{
			if (!Helpers.ContractManager.TryAddOrderTemplate(order.RecurringSequenceInfo.Name, new string[0], order, out var templateId, true))
			{
				Log(nameof(RegisterRecurringOrder), "Recurring order template could not be added or updated in Contract Manager element");
			}

			order.RecurringSequenceInfo.TemplateId = templateId;

			if (!Helpers.OrderManagerElement.AddRecurringOrder(order, templateId))
			{
				Log(nameof(RegisterRecurringOrder), "Recurring order reference could not be added or updated in Order Manager element");
			}

			order.TryUpdateCustomProperties(Helpers, new Dictionary<string, object>
			{
				{LiteOrder.PropertyNameRecurrence, order.RecurringSequenceInfo.ToString() },
				{LiteOrder.PropertyNameFromTemplate, templateId.ToString() }
			});

			Log(nameof(RegisterRecurringOrder), $"Updated order properties {LiteOrder.PropertyNameFromTemplate}={templateId.ToString()} and {LiteOrder.PropertyNameRecurrence}={order.RecurringSequenceInfo.ToString()}");
		}

		protected void LinkOrderToEvent()
		{
			if (!isNewOrder)
			{
				Log(nameof(LinkOrderToEvent), $"Order is not new, therefore its ID should not have changed and the Event should not be updated.");
				return;
			}

			Log(nameof(LinkOrderToEvent), $"Order is new, therefore its ID should be linked to the Event.");

			order.Event.AddOrUpdateOrder(order, Helpers);
			order.Event.TryUpdateJobToJobDomain();
		}

		protected void PartialOrderAddOrUpdate()
		{
			UpdateCustomProperties();
			if (!isSuccessful) return;

			if (NameUpdateRequired())
			{
				UpdateOrderName();
				if (!isSuccessful) return;
			}

			if (TimingUpdateRequired())
			{
				UpdateOrderTiming();
				if (!isSuccessful) return;
			}

			if (ContributingServiceAssignmentRequired())
			{
				AssignContributingResources();
				if (!isSuccessful) return;
			}

			// Update Security View Ids
			order.UpdateSecurityViewIds(Helpers, new HashSet<int>(order.SecurityViewIds));

			foreach (var service in order.AllServices)
			{
				service.UpdateSecurityViewIds(Helpers, order.SecurityViewIds);
			}

			Helpers.OrderManager.UpdateExistingServicesSecurityViewIds(order, existingOrder);
		}

		private bool NameUpdateRequired()
		{
			bool updateRequired = existingOrder?.Name != order.Name;

			Log(nameof(NameUpdateRequired), $"Order name changed from '{existingOrder?.Name}' to '{order.Name}', {(updateRequired ? string.Empty : "no ")}update required.");

			return updateRequired;
		}

		protected void UpdateOrderName()
		{
			Log(nameof(UpdateOrderName), "Update Order name task");
			var updateNameTask = new ChangeOrderNameTask(Helpers, order, existingOrder.Name);
			tasks.Add(updateNameTask);
			if (!updateNameTask.Execute())
			{
				Log(nameof(UpdateOrderName), $"Update Order name task failed: {updateNameTask.Exception}");
				isSuccessful = false;
				return;
			}

			order.Reservation = DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, order.Id);
		}

		protected void UpdateSatelliteRxSynopsisAttachments()
		{
			if (!order.AllServices.Exists(x => x?.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite)) return;

			Log(nameof(UpdateSatelliteRxSynopsisAttachments), "Updating Satellite Rx Synopsis Attachments");
			var syncSynopsisAttachmentsTask = new UpdateSatelliteRxSynopsisAttachmentsTask(Helpers, order);
			tasks.Add(syncSynopsisAttachmentsTask);
			if (!syncSynopsisAttachmentsTask.Execute())
			{
				Log(nameof(UpdateSatelliteRxSynopsisAttachments), $"Updating Satellite Rx Synopsis Attachments failed: {syncSynopsisAttachmentsTask.Exception}");
				isSuccessful = false;
				return;
			}

			order.Reservation = DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, order.Id);
		}

		private void AssignContributingResources()
		{
			Log(nameof(PartialOrderAddOrUpdate), "Assigning contributing services task");
			var assignContributingServicesTask = new AssignContributingServicesToOrderTask(Helpers, order, (ServiceReservationInstance)order.Reservation);
			tasks.Add(assignContributingServicesTask);
			if (!assignContributingServicesTask.Execute())
			{
				Log(nameof(PartialOrderAddOrUpdate), "Assigning contributing services task failed");
				isSuccessful = false;
				return;
			}

			order.Reservation = DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, order.Id);
		}

		private void UpdateOrderTiming()
		{
			Log(nameof(UpdateOrderTiming), "Update Order timing task");
			var updateTimingTask = new ChangeOrderTimeTask(Helpers, order, existingOrder);
			tasks.Add(updateTimingTask);
			if (!updateTimingTask.Execute())
			{
				Log(nameof(UpdateOrderTiming), "Update Order timing task failed");
				isSuccessful = false;
				return;
			}

			order.Reservation = DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, order.Id);
		}

		private bool ContributingServiceAssignmentRequired()
		{
			if (options.HasFlag(OptionFlags.SkipContributingResourceAssignment))
			{
				Log(nameof(ContributingServiceAssignmentRequired), "Skipping Contributing resource assignment due to presence of flag");
				return false;
			}

			order.Reservation = DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, order.Id);

			var emptyResourceUsageDefinitions = order.Reservation.EmptyResourcesInReservation.OfType<ServiceResourceUsageDefinition>().ToList();

			if (emptyResourceUsageDefinitions.Any())
			{
				Log(nameof(ContributingServiceAssignmentRequired), $"Nodes {string.Join(", ", emptyResourceUsageDefinitions.Select(x => x.ServiceDefinitionNodeID))} have no resource assigned to it. Contributing resource assignment required");

				return true;
			}

			var orderServices = order.AllServices;
			var resourceUsageDefinitions = order.Reservation.ResourcesInReservationInstance.OfType<ServiceResourceUsageDefinition>().ToList();

			foreach (var node in order.Definition.Diagram.Nodes)
			{
				var resourceUsageDefinition = resourceUsageDefinitions.SingleOrDefault(x => x.ServiceDefinitionNodeID == node.ID) ?? throw new ResourceUsageDefinitionNotFoundException($"Reservation {order.Id} has no (empty or non-empty) resource usage definition for node {node.Label} ({node.ID})");

				var nodeCurrentResourceId = resourceUsageDefinition.GUID;

				var nodeService = orderServices.SingleOrDefault(s => s.NodeId == node.ID);
				if (nodeService == null)
				{
					Log(nameof(ContributingServiceAssignmentRequired), $"Unable to find service with node ID {node.ID}, releasing contributing resource");
					return true;
				}

				var nodeNewResourceId = nodeService.ContributingResource?.ID ?? Guid.Empty;

				if (nodeCurrentResourceId != nodeNewResourceId)
				{
					Log(nameof(ContributingServiceAssignmentRequired), $"Node {node.Label} has resource {nodeCurrentResourceId} but needs resource {nodeService.ContributingResource?.Name} ({nodeService.ContributingResource?.ID}). Contributing resource assignment required");
					return true;
				}
			}

			Log(nameof(ContributingServiceAssignmentRequired), $"No nodes need new resources. Contributing resource assignment not required");

			return false;
		}

		protected void UpdateCustomProperties()
		{
			Log(nameof(UpdateCustomProperties), "Update Custom properties for Order task");

			var updateCustomPropertiesTask = new Tasks.OrderTasks.UpdateCustomPropertiesTask(Helpers, order, existingOrder, options.HasFlag(OptionFlags.ForceAllOrderCustomPropertiesUpdate));
			tasks.Add(updateCustomPropertiesTask);
			if (!updateCustomPropertiesTask.Execute())
			{
				Log(nameof(UpdateCustomProperties), "Update Custom properties for Order task failed");
				isSuccessful = false;
				return;
			}

			order.Reservation = DataMinerInterface.ResourceManager.GetReservationInstance(Helpers, order.Id);
		}

		private bool TimingUpdateRequired()
		{
			bool startTimeChanged = !existingOrder.Start.Matches(order.Start);
			bool endTimeChanged = !existingOrder.End.Matches(order.End);
			bool prerollChanged = existingOrder.PreRoll != order.PreRoll;
			bool postrollChanged = existingOrder.PostRoll != order.PostRoll;

			bool startTimesChanged = (startTimeChanged || prerollChanged) && !existingOrder.ShouldBeRunning && order.Start > DateTime.Now;
			bool endTimesChanged = endTimeChanged || postrollChanged;
			bool timingChanged = startTimesChanged || endTimesChanged;

			Log(nameof(TimingUpdateRequired), $"order {(existingOrder.ShouldBeRunning ? "should be running and end" : "should not be running and")} timing has{(timingChanged ? string.Empty : " not")} changed. Existing order timing: {existingOrder.TimingInfoToString()}. Current order timing: {order.TimingInfoToString()}");

			return timingChanged;
		}

		private void FullOrderAddOrUpdate()
		{
			Log(nameof(FullOrderAddOrUpdate), "Add or update order task");
			var addOrUpdateOrderTask = new AddOrUpdateOrderTask(Helpers, order, existingOrder);
			tasks.Add(addOrUpdateOrderTask);
			if (!addOrUpdateOrderTask.Execute())
			{
				Log(nameof(FullOrderAddOrUpdate), "Add or update order task failed");
				isSuccessful = false;
				return;
			}

			order.Reservation = addOrUpdateOrderTask.OrderReservationInstance;
		}

		private bool FullOrderAddOrUpdateRequired()
		{
			string existingOrderName = Helpers.ReservationManager.GetReservation(order.Id)?.Name;
			bool orderNameChanged = order.Name != existingOrderName;
			if (orderNameChanged)
			{
				Log(nameof(FullOrderAddOrUpdateRequired), $"Order name changed from {existingOrderName} to {order.Name}");

				bool orderNameAlreadyExists = Helpers.ReservationManager.GetReservation(order.Name) != null;
				if (orderNameAlreadyExists || string.IsNullOrWhiteSpace(order.Name))
				{
					string defaultOrderName = isNewOrder ? $"{order.Name} [{Guid.NewGuid()}]" : existingOrderName;

					Log(nameof(FullOrderAddOrUpdateRequired), $"New Order name {order.Name} is already in use, setting name to {defaultOrderName}");

					order.ManualName = defaultOrderName;

					orderNameChanged = order.Name != existingOrderName;
				}
			}

			bool fullOrderAddOrUpdateRequired = orderNameChanged || isNewOrder || orderGotNewOrUpdatedServiceDefinition;

			Log(nameof(FullOrderAddOrUpdateRequired), $"Full Order add or update is{(fullOrderAddOrUpdateRequired ? string.Empty : " not")} required");

			return fullOrderAddOrUpdateRequired;
		}

		protected void CollectServicesToRemoveComparedToExistingOrder()
		{
			if (isNewOrder) return;

			var newOrderServices = order.AllServices;

			// An existing Service should be removed when no service with that GUID is present in the current Order
			foreach (var existingService in existingOrder.AllServices)
			{
				if (!newOrderServices.Any(s => s.Name == existingService.Name)) servicesToRemove.Add(existingService);
			}

			Log(nameof(CollectServicesToRemoveComparedToExistingOrder), $"Services to remove: {string.Join(";", servicesToRemove.Select(s => s.Name))}");
		}

		protected void GenerateRoutingServices()
		{
			LogMethodStart(nameof(GenerateRoutingServices), out var stopWatch);

			var generateRoutingServicesTask = new GenerateRoutingServicesTask(Helpers, order);
			tasks.Add(generateRoutingServicesTask);

			if (!generateRoutingServicesTask.Execute(false))
			{
				Log(nameof(GenerateRoutingServices), "Generate routing services task failed");
				isSuccessful = false;
			}

			servicesToRemove.AddRange(generateRoutingServicesTask.RemovedRoutingServices);

			LogMethodCompleted(nameof(GenerateRoutingServices), stopWatch);
		}

		protected void GenerateVizremConverterServices()
		{
			if (order.Subtype == OrderSubType.Normal || order.Sources.Exists(s => s.Definition.Description == "NDI Router")) return;

			LogMethodStart(nameof(GenerateVizremConverterServices), out var stopWatch);

			var generateVizremConverterServicesTask = new GenerateVizremConverterServicesTask(Helpers, order);
			tasks.Add(generateVizremConverterServicesTask);

			if (!generateVizremConverterServicesTask.Execute(false))
			{
				Log(nameof(GenerateVizremConverterServices), "Generate vizrem converter services task failed");
				isSuccessful = false;
			}

			LogMethodCompleted(nameof(GenerateVizremConverterServices), stopWatch);
		}

		protected void AddOrUpdateRoutingServices()
		{
			foreach (var service in order.AllServices)
			{
				if (service.Definition.VirtualPlatform != VirtualPlatform.Routing) continue;

				AddOrUpdateService(service);
			}
		}

		protected void ClearMajorTimeslotFlag()
		{
			foreach (var service in order.AllServices)
			{
				if (!service.MajorTimeslotChange) continue;

				service.MajorTimeslotChange = false;
			}
		}

		protected void AddOrUpdateVizremConverterServices()
		{
			if (order.Subtype != OrderSubType.Vizrem) return;

			foreach (var service in order.AllServices)
			{
				if (service.Definition.VirtualPlatform != VirtualPlatform.VizremNC2Converter) continue;

				AddOrUpdateService(service);
			}
		}

		protected void AddOrUpdateNonRoutingNonConverterServices()
		{
			foreach (var service in order.AllServices)
			{
				// routing services are only updated/booked after reprocessing them
				if (service.Definition.VirtualPlatform == VirtualPlatform.Routing || service.Definition.VirtualPlatform == VirtualPlatform.VizremNC2Converter)
				{
					Log(nameof(AddOrUpdateNonRoutingNonConverterServices), $"Skipping {service.Definition.VirtualPlatform.GetDescription()} service");
					continue;
				}

				bool serviceWasAlreadyBookedBeforeAddOrUpdate = service.IsBooked;

				AddOrUpdateService(service);

				if (service.IsSharedSource && !serviceWasAlreadyBookedBeforeAddOrUpdate)
				{
					// ID of the ELR that just got booked needs to be updated in the service configs of its other orders
					// Only applicable when changing the service def of an already existing ELR that is used in multiple orders
					UpdateOtherOrderServiceConfigAfterSharedSourceIsBooked(service);
				}
			}
		}

		protected void CheckMessiChangeHighlightConditionsOnRecordings()
		{
			foreach (var service in order.AllServices.Where(s => s.Definition.VirtualPlatform == VirtualPlatform.Recording))
			{
				bool recordingHighlightingRequired = MessiHighlightingRequiredForRecordingService(service);

				if (recordingHighlightingRequired)
				{
					service.LateChange = true;

					Log(nameof(CheckMessiChangeHighlightConditionsOnRecordings), $"Set service {service.Name} LateChange property to true");
				}
			}
		}

		protected void CheckMessiChangeHighlightConditionsOnRoutings()
		{
			foreach (var service in order.AllServices.Where(s => s.Definition.VirtualPlatform == VirtualPlatform.Routing))
			{
				var liveVideoOrder = new LiveVideoOrder(Helpers, order);

				var chainsForThisRouting = liveVideoOrder.GetRoutingServiceChainsForService(service.Id);

				var recordingChild = chainsForThisRouting.Select(rsc => rsc.OutputService).FirstOrDefault(s => s.Service.Definition.VirtualPlatform == VirtualPlatform.Recording && s.Service.Definition.Id == ServiceDefinitionGuids.RecordingMessiNews)?.Service;

				if (recordingChild is null) continue;

				bool recordingHighlightingRequired = MessiHighlightingRequiredForRoutingService(service);
				if (recordingHighlightingRequired)
				{
					recordingChild.LateChange = true;

					Log(nameof(CheckMessiChangeHighlightConditionsOnRoutings), $"Set service {recordingChild.Name} LateChange property to true");
				}
			}
		}

		private bool MessiHighlightingRequiredForRoutingService(Service routingService)
		{
			bool lessThanTwelveHoursUntilServiceStart = routingService.Start < DateTime.Now + TimeSpan.FromHours(12);
			if (!lessThanTwelveHoursUntilServiceStart) return false;

			Helpers.ServiceManager.TryGetService(routingService.Id, out var oldRoutingService);
			var routingServiceChange = routingService.GetChangeComparedTo(null, oldRoutingService) as ServiceChange;
			var routingServiceChangeSummary = routingServiceChange.Summary as ServiceChangeSummary;

			bool matrixInputSdiChanged = routingServiceChangeSummary.FunctionChangeSummary.ResourceChangeSummary.ResourceAtBeginningOfServiceDefinitionChanged;
			if (matrixInputSdiChanged)
			{
				return routingService.IsNewsRouting;
			}

			bool matrixOutputSdiChanged = routingServiceChangeSummary.FunctionChangeSummary.ResourceChangeSummary.ResourceAtEndOfServiceDefinitionChanged;
			if (matrixOutputSdiChanged)
			{
				return routingService.IsHmxRouting;
			}

			return false;
		}

		private bool MessiHighlightingRequiredForRecordingService(Service recordingService)
		{
			Helpers.ServiceManager.TryGetService(recordingService.Id, out var oldService);
			var serviceChange = recordingService.GetChangeComparedTo(null, oldService) as ServiceChange;
			var serviceChangeSummary = serviceChange.Summary as ServiceChangeSummary;

			bool lessThanTwelveHoursUntilServiceStart = recordingService.Start < DateTime.Now + TimeSpan.FromHours(12);
			if (!lessThanTwelveHoursUntilServiceStart)
			{
				Log(nameof(CheckMessiChangeHighlightConditionsOnRecordings), $"Service {recordingService.Name} does not start within 12 hours, no Messi change highlighting required");
				return false;
			}

			try
			{
				if (serviceChangeSummary.IsNew)
				{
					Log(nameof(CheckMessiChangeHighlightConditionsOnRecordings), $"Service {recordingService.Name} is new, Messi change highlighting required");
					return true;
				}

				bool recordingConfigChange = serviceChangeSummary.PropertyChangeSummary.RecordingConfigurationChanged;
				if (recordingConfigChange)
				{
					Log(nameof(CheckMessiChangeHighlightConditionsOnRecordings), $"Service {recordingService.Name} recording configuration has changed, Messi change highlighting required");
					return true;
				}

				bool commentsChange = serviceChange.PropertyHasChange(nameof(Service.Comments));
				if (commentsChange)
				{
					Log(nameof(CheckMessiChangeHighlightConditionsOnRecordings), $"Service {recordingService.Name} comments have changed, Messi change highlighting required");
					return true;
				}

				bool timeSlotChange = serviceChangeSummary.TimingChangeSummary.IsChanged;
				if (timeSlotChange)
				{
					Log(nameof(CheckMessiChangeHighlightConditionsOnRecordings), $"Service {recordingService.Name} timing has changed, Messi change highlighting required");
					return true;
				}

				if (recordingService.Definition.Id == ServiceDefinitionGuids.RecordingMessiLive)
				{
					bool resourceChanged = serviceChangeSummary.FunctionChangeSummary.ResourceChangeSummary.IsChanged;
					if (resourceChanged)
					{
						Log(nameof(CheckMessiChangeHighlightConditionsOnRecordings), $"Service {recordingService.Name} resource has changed, Messi change highlighting required");
						return true;
					}
				}

				Log(nameof(CheckMessiChangeHighlightConditionsOnRecordings), $"No relevant changes for {recordingService.Name}, no Messi change highlighting required");

				return false;
			}
			catch (Exception e)
			{
				Log(nameof(CheckMessiChangeHighlightConditionsOnRecordings), $"Service {recordingService.Name} unable to check messi changes: {e}");
				return false;
			}
		}

		protected void AddOrUpdateEventLevelReceptions()
		{
			foreach (var eventLevelReception in order.Sources.Where(s => s.IsSharedSource))
			{
				bool elrWasAlreadyBookedBeforeAddOrUpdate = eventLevelReception.IsBooked;

				AddOrUpdateService(eventLevelReception);

				if (!elrWasAlreadyBookedBeforeAddOrUpdate)
				{
					// ID of the ELR that just got booked needs to be updated in the service configs of its other orders
					// Only applicable when changing the service def of an already existing ELR that is used in multiple orders
					UpdateOtherOrderServiceConfigAfterSharedSourceIsBooked(eventLevelReception);
				}
			}
		}

		protected void PromoteReceptionToSharedSource()
		{
			var source = order.Sources.Single(x => x.BackupType == BackupType.None);
			if (!source.IsSharedSource)
			{
				Log(nameof(PromoteReceptionToSharedSource), "Source service is not a Shared Source");
				return;
			}

			if (source.IsSharedSource)
			{
				Log(nameof(PromoteReceptionToSharedSource), "Promoting source service to Shared Source");

				var linkEventToReceptionTask = new PromoteToSharedSourceTask(Helpers, source);
				tasks.Add(linkEventToReceptionTask);

				linkEventToReceptionTask.Execute();
			}
			else
			{
				Log(nameof(PromoteReceptionToSharedSource), "Source service is an already promoted event level reception");
			}
		}

		protected void AddOrUpdateUserTasks()
		{
			foreach (var service in servicesForWhichToCreateUserTasks)
			{
				// User Task generation needs to happen last in the flow
				Log(nameof(AddOrUpdateUserTasks), "Add or update user task task");

				var addOrUpdateUserTasksTask = new AddOrUpdateServiceUserTasksTask(Helpers, service, order);
				tasks.Add(addOrUpdateUserTasksTask);

				if (!addOrUpdateUserTasksTask.Execute())
				{
					Log(nameof(AddOrUpdateUserTasks), "Add or update user task task failed");
					isSuccessful = false;
					return;
				}
			}
		}

		protected void UpdateOrderUiProperties()
		{
			order.UpdateUiProperties(Helpers);
		}

		protected void DeleteServices()
		{
			foreach (var service in servicesToRemove)
			{
				Log(nameof(DeleteServices), $"Service: {service.Id}");

				if (DeleteService(service)) continue;

				foreach (var task in tasks)
				{
					if (task.Status == Tasks.Status.Ok) continue;

					Log(nameof(DeleteServices), $"Task {task.Description} failed: {task.Exception}");
				}

				isSuccessful = false;
				return;
			}
		}

		protected void UpdateServiceUiProperties()
		{
			var bookedServices = order.AllServices.Where(s => s.IsBooked).ToList();
			if (!bookedServices.Any()) return;

			foreach (var service in bookedServices)
			{
				service.UserTasks = Helpers.UserTaskManager.GetUserTasks(service).ToList(); // Get UserTasks again to make sure we have the latest user task status
			}

			// only do this for services that are booked
			// this is checked because it's also called when initially creating the order as there might be event level receptions and only the event level receptions need to be updated in that case
			foreach (var service in bookedServices)
			{
				var valuesToUpdate = new Dictionary<string, object>
				{
					{ ServicePropertyNames.Status, service.GenerateStatus(Helpers, order).GetDescription() },
					{ ServicePropertyNames.MCRStatus, service.DetermineMcrStatus(Helpers, order).GetDescription() },
					{ ServicePropertyNames.ShortDescription, service.GetShortDescription(order).Clean(true) },
					{ ServicePropertyNames.AllUserTasksCompleted, service.AllUserTasksCompleted.ToString().ToLower() /* VSC: ToLower() required */ },
				};

				if (service.LateChange)
				{
					valuesToUpdate.Add(ServicePropertyNames.LateChange, true);
				}

				service.TryUpdateCustomProperties(Helpers, valuesToUpdate);
			}
		}

		protected void TryStartOrderNowIfApplicable()
		{
			Helpers.OrderManager.TryStartOrderNow(Helpers, order);
		}

		protected void TryStopPreviousRunningOrderNowIfApplicable()
		{
			if (!order.ConvertedFromRunningToStartNow)
			{
				Log(nameof(TryStopPreviousRunningOrderNowIfApplicable), $"No need to stop previous running order");
				return;
			}

			try
			{
				var previousRunningOrder = Helpers.OrderManager.GetOrder(order.PreviousRunningOrderId);
				if (previousRunningOrder == null)
				{
					Log(nameof(TryStopPreviousRunningOrderNowIfApplicable), $"Previous running order with id {order.PreviousRunningOrderId} couldn't be retrieved");
					isSuccessful = false;
					return;
				}

				foreach (var previousRunningService in previousRunningOrder.AllServices)
				{
					previousRunningService.End = DateTime.Now; // Update service configuration end time is needed to make sure the correct order status update is guaranteed after stopping it.
				}

				previousRunningOrder.StopNow = true;
				previousRunningOrder.StopOrderNow(Helpers);
				previousRunningOrder.UpdateServiceConfigurationProperty(Helpers);

				order.ConvertedFromRunningToStartNow = false;
				order.UpdateConvertRunningToStartNowProperty(Helpers);
			}
			catch (Exception e)
			{
				Log(nameof(TryStopPreviousRunningOrderNowIfApplicable), $"Exception while trying to stop previous running order with id {order.PreviousRunningOrderId}: {e}");
			}
		}

		protected void TryStopOrderNowIfApplicable()
		{
			Log(nameof(TryStopOrderNowIfApplicable), "Stop order now task");

			var stopOrderTask = new Tasks.OrderTasks.StopOrderTask(Helpers, order);
			tasks.Add(stopOrderTask);
			if (!stopOrderTask.Execute())
			{
				Log(nameof(TryStopOrderNowIfApplicable), "Stop order now task failed");
				isSuccessful = false;
			}
		}

		private void AddOrUpdateService(Service service)
		{
			Helpers.ServiceManager.TryGetService(service.Id, out var existingService);

			var serviceUpdateHandler = new ServiceAddOrUpdateHandler(Helpers, order, service, existingService);

			bool serviceSuccessfullyUpdated = serviceUpdateHandler.Execute(out var serviceTasks, out bool createUserTaskForThisService);
			tasks.AddRange(serviceTasks);

			if (!serviceSuccessfullyUpdated)
			{
				isSuccessful = false;
				return;
			}

			if (createUserTaskForThisService) servicesForWhichToCreateUserTasks.Add(service);
		}

		private void UpdateOtherOrderServiceConfigAfterSharedSourceIsBooked(Service eventLevelReception)
		{
			foreach (var orderReference in eventLevelReception.OrderReferences)
			{
				if (orderReference == order.Id) continue;

				var otherReferencedOrder = Helpers.OrderManager.GetOrder(orderReference);

				var sameSharedSource = otherReferencedOrder.Sources.Single(x => x.IsSharedSource);

				sameSharedSource.Id = eventLevelReception.Id;

				otherReferencedOrder.UpdateServiceConfigurationProperty(Helpers);
			}
		}

		protected void AssignResourcesToServices()
		{
			LogMethodStart(nameof(AssignResourcesToServices), out var stopWatch);

			if (options.HasFlag(OptionFlags.SkipServiceResourceAssignment))
			{
				Log(nameof(AssignResourcesToServices), $"Skipping service resource assignment because of {OptionFlags.SkipServiceResourceAssignment} option");
				LogMethodCompleted(nameof(AssignResourcesToServices), stopWatch);
				return;
			}

			foreach (var service in order.AllServices)
			{
				var assignResourcesToFunctionsTask = new AssignResourcesToFunctionsTask(Helpers, service, order);
				tasks.Add(assignResourcesToFunctionsTask);
				assignResourcesToFunctionsTask.Execute(false);
			}

			LogMethodCompleted(nameof(AssignResourcesToServices), stopWatch);
		}

		/// <summary>
		/// Check to adjust service preroll and/or starttime for new orders and preliminary orders in case they should start within their preroll
		/// or if a new or edited order should start as soon as possible
		/// </summary>
		protected void CheckToAdjustStartTimes()
		{
			if (existingOrder != null && existingOrder.ShouldBeRunning)
			{
				Log(nameof(CheckToAdjustStartTimes), $"Already booked Order should be running, start now is not allowed.");
				return;
			}

			// Update Service preroll and start times
			var now = DateTime.Now.RoundToMinutes();
			foreach (var service in order.AllServices)
			{
				if ((service.IsBooked || !service.ShouldStartDirectly) && order.PreviousRunningOrderId != Guid.Empty) continue;
				service.CheckToAdjustStartTimes(Helpers, now, order);
			}

			// Preroll on order is defined by Service preroll
			if (order.StartNow)
			{
				AdjustStartTimeOfStartNowOrder(now);
			}
			else
			{
				TimeSpan delay = TimeSpan.FromMinutes(Order.StartInTheFutureDelayInMinutes);

				var nowWithDelay = now.Add(delay).RoundToMinutes();
				bool adjustStartTime = order.Start <= nowWithDelay && order.IntegrationType == IntegrationType.None; // startTime should match the nowWithDelay. Prevent start timing change for (integration) orders in the near future

				if (adjustStartTime)
				{
					order.Start = nowWithDelay;
					Log(nameof(CheckToAdjustStartTimes), $"Order start is earlier than now with delay, set order start to {order.Start}");
				}
			}
		}

		private void AdjustStartTimeOfStartNowOrder(DateTime now)
		{
			order.PreRoll = TimeSpan.Zero;

			TimeSpan delay;
			bool shouldUseAddServicesToRunningOrderStartNowDelay = order.ConvertedFromRunningToStartNow;
			bool shouldUseFeenixStartNowDelay = order.IntegrationType == IntegrationType.Feenix && !shouldUseAddServicesToRunningOrderStartNowDelay;
			if (shouldUseAddServicesToRunningOrderStartNowDelay)
			{
				delay = TimeSpan.FromMinutes(Order.StartNowDelayInMinutesWhenAddingServicesToRunningOrder);
			}
			else if (shouldUseFeenixStartNowDelay)
			{
				delay = TimeSpan.FromMinutes(Order.StartNowDelayInMinutesForFeenix);
			}
			else
			{
				delay = TimeSpan.FromMinutes(Order.StartNowDelayInMinutes);
			}

			var nowWithDelay = now.Add(delay).RoundToMinutes();
			order.Start = nowWithDelay;

			Log(nameof(CheckToAdjustStartTimes), $"Order should start now, set order start to {order.Start} and order start with preroll to {order.StartWithPreRoll}");
		}

		protected void AddOrUpdateEvent()
		{
			if (options.HasFlag(OptionFlags.SkipAddOrUpdateEvent))
			{
				Log(nameof(AddOrUpdateEvent), $"Skipping because of option flag");
				return;
			}

			CheckIfOrderHasNewEvent();

			DeleteExistingEventConditional();

			if (!EventUpdateRequired()) return;

			if (order.Event == null) return;

			isSuccessful &= TryGetEventLockInfo(order.Event, out var eventLockInfo);

			if (!isSuccessful) return;

			if (order.Start < order.Event.Start)
			{
				order.Event.Start = order.Start;
				Log(nameof(AddOrUpdateEvent), $"Set Event start time to {order.Event.Start}");
			}

			if (order.End > order.Event.End)
			{
				order.Event.End = order.End;
				Log(nameof(AddOrUpdateEvent), $"Set Event end time to {order.Event.End}");
			}

			var eventsToAddOrUpdate = new List<Tuple<Event, Event>> { new Tuple<Event, Event>(existingEvent, order.Event) }; // List of tuples, item 1 is the existing event (or null), item 2 is the event to add or update

			foreach (var eventToAddOrUpdate in eventsToAddOrUpdate)
			{
				var addOrUpdateEventTask = new AddOrUpdateEventTask(Helpers, eventToAddOrUpdate.Item2, eventToAddOrUpdate.Item1, eventLockInfo);
				tasks.Add(addOrUpdateEventTask);

				if (!addOrUpdateEventTask.Execute())
				{
					Log(nameof(AddOrUpdateEvent), "Add or update event task failed");
					isSuccessful = false;
				}
			}

			if (order.RecurringSequenceInfo.Recurrence.IsConfigured)
			{
				order.RecurringSequenceInfo.EventId = order.Event.Id;
			}
		}

		private void CheckIfOrderHasNewEvent()
		{
			if (order.Event == null)
			{
				// Use case: Manual add order without existing event via LOF
				order.Event = new Event(Helpers, order);
				orderHasNewEvent = true;
			}
			else
			{
				// Use Cases:
				// Manual add order under existing event via LOF
				// Manual edit order via LOF, UpdateService or UpdateELRs
				// Add order via HIU (Event object has Guid.Empty and still needs to be booked)
				// Edit order via HIU
				// Add services to a running order.

				Helpers.EventManager.AddOrUpdateOrderToEvent(order.Event, order);

				bool eventIsNotYetBooked = order.Event.Id == Guid.Empty;
				bool orderMovedFromOneEventToAnother = existingOrder?.Event?.Id != order.Event.Id;
				bool orderHasNewId = existingOrder?.Id != order.Id;

				Log(nameof(AddOrUpdateEvent), $"Event is {(eventIsNotYetBooked ? "not yet" : "already")} booked. Order {(orderMovedFromOneEventToAnother ? "moved" : "did not move")} from one Event to another. Order {(orderHasNewId ? "has" : "does not have")} a new ID.");

				orderHasNewEvent = eventIsNotYetBooked || orderMovedFromOneEventToAnother || orderHasNewId;
			}
		}

		private bool TryGetEventLockInfo(Event @event, out LockInfo eventLockInfo)
		{
			bool eventIsNotYetBooked = @event.Id == Guid.Empty;
			if (eventIsNotYetBooked)
			{
				eventLockInfo = new LockInfo(true, Helpers.Engine.UserLoginName, @event.Id.ToString(), TimeSpan.FromMinutes(1));
				return true;
			}
			else
			{
				var getEventLockTask = new GetEventLockTask(Helpers, @event);
				tasks.Add(getEventLockTask);

				bool success = getEventLockTask.Execute();

				eventLockInfo = getEventLockTask.LockInfo;

				return success;
			}
		}

		private void DeleteExistingEventConditional()
		{
			if (existingEvent == null)
			{
				Log(nameof(DeleteExistingEventConditional), "Existing event is null");
				return;
			}

			if (existingEvent.Id != order.Event.Id)
			{
				Log(nameof(DeleteExistingEventConditional), $"existing event {existingEvent.Id} is different than current event {order.Event.Id}");

				bool oldEventHasRemainingOrders = existingEvent.OrderIds.Any(x => x != order.Id);

				Log(nameof(DeleteExistingEventConditional), $"existing event {existingEvent.Id} has {(oldEventHasRemainingOrders ? string.Empty : "no ")}remaining orders");

				if (!oldEventHasRemainingOrders && (existingEvent.IntegrationType == IntegrationType.Feenix || existingEvent.IntegrationType == IntegrationType.None))
				{
					// Remove event
					Log(nameof(DeleteExistingEventConditional), $"Removing existing event {existingEvent.Id}");
					Helpers.EventManager.DeleteEvent(existingEvent.Id);
				}
				else if (order.IntegrationType == IntegrationType.Feenix)
				{
					// Remove order id
					Log(nameof(DeleteExistingEventConditional), $"Moving Feenix Order from Event {existingEvent.Id} to {order.Event.Id}");
					Helpers.EventManager.DeleteOrderFromEvent(existingEvent, order.Id);
				}
				else
				{
					// nothing to do
				}
			}
		}

		private bool DeleteService(Service service)
		{
			// shouldn't remove services that are not booked yet
			if (!service.IsBooked) return true;

			Log(nameof(DeleteService), "Service to remove: " + service.Name);

			var reservation = Helpers.ResourceManager.GetReservationInstance(service.Id);
			if (reservation == null)
			{
				Log(nameof(DeleteService), $"Reservation instance cannot be found {service.Id}");
			}

			Log(nameof(DeleteService), "Delete service user tasks task");
			var deleteServiceUserTask = new DeleteServiceUserTasksTask(Helpers, service, order);
			tasks.Add(deleteServiceUserTask);
			if (!deleteServiceUserTask.Execute())
			{
				Log(nameof(DeleteService), "Delete service user tasks task failed");
				return false;
			}

			bool skipCheckingIfServiceIsUsedByOtherOrders = false;

			if (service.IsSharedSource)
			{
				var delayTime = Helpers.OrderManagerElement.GetServiceDeletionDelayTime();
				var currentDateTime = DateTime.Now;

				bool isUsedInOrdersWithNotExpiredEndTime = service.OrderReferences.Select(x => Helpers.OrderManager.GetLiteOrder(x)).Any(x => x.End.Add(delayTime) > currentDateTime);

				skipCheckingIfServiceIsUsedByOtherOrders = !isUsedInOrdersWithNotExpiredEndTime;

				Log(nameof(DeleteService), $"Shared source {service.Name} {(isUsedInOrdersWithNotExpiredEndTime ? "is" : "is not")} used in other not expired orders.");
			}

			Log(nameof(DeleteService), "Delete EVS Recording Session task");
			var deleteFromEvsTask = new DeleteFromEvsTask(Helpers, service);
			tasks.Add(deleteFromEvsTask);
			if (!deleteFromEvsTask.Execute())
			{
				Log(nameof(DeleteService), "Delete EVS Recording Session Task failed");
				return false;
			}

			Log(nameof(DeleteService), "Delete service task");
			var deleteServiceTask = new DeleteServiceTask(Helpers, service, order, reservation, skipCheckingIfServiceIsUsedByOtherOrders);
			tasks.Add(deleteServiceTask);

			if (!deleteServiceTask.Execute())
			{
				Log(nameof(DeleteService), "Delete service task failed");
				return false;
			}

			return true;
		}

		private bool EventUpdateRequired()
		{
			Log(nameof(EventUpdateRequired), $"Event object used as Existing Event: {existingEvent?.ToString()}");

			bool doesOrderTimeExceedsEventTime = order.End > order.Event.End || order.Start < order.Event.Start;
			bool isEventStatusPlannedOrPreliminary = order.Event.Status == YLE.Event.Status.Planned || order.Event.Status == YLE.Event.Status.Preliminary;

			Log(nameof(EventUpdateRequired), $"Order {(orderHasNewEvent ? "has" : "does not have")} a new event. Order time {(doesOrderTimeExceedsEventTime ? "exceeds" : "does not exceed")} Event time.");

			bool eventUpdateRequired = false;
			eventUpdateRequired |= orderHasNewEvent;
			eventUpdateRequired |= order.Event.IsUpdated(existingOrder?.Event);
			eventUpdateRequired |= doesOrderTimeExceedsEventTime;
			eventUpdateRequired |= isEventStatusPlannedOrPreliminary;
			return eventUpdateRequired;
		}

		protected void AddOrUpdateOrderServiceDefinition()
		{
			if (options.HasFlag(OptionFlags.SkipAddOrUpdateOrderDefinition))
			{
				Log(nameof(AddOrUpdateOrderServiceDefinition), $"Skipping because of option flag");
				return;
			}

			orderGotNewOrUpdatedServiceDefinition = false;

			var getServiceDefinitionFromOrderTask = new GetServiceDefinitionFromOrderTask(Helpers, order, servicesToRemove.ToList());
			tasks.Add(getServiceDefinitionFromOrderTask);
			if (!getServiceDefinitionFromOrderTask.Execute(false))
			{
				Log(nameof(AddOrUpdateOrderServiceDefinition), "Get service definition from order task failed");
				isSuccessful = false;
				return;
			}

			Report("Building service chain succeeded");

			var newOrderDefinition = getServiceDefinitionFromOrderTask.Result;

			Log(nameof(AddOrUpdateOrderServiceDefinition), $"Service definition built from order: {newOrderDefinition.Diagram.DiagramToString()}");
			Log(nameof(AddOrUpdateOrderServiceDefinition), $"Summary: services per node in definition: {order.ServicesAndServiceDefinitionToString()}");

			if (!ServiceDefinitionSrmUpdateRequired(newOrderDefinition)) return;

			if (!isNewOrder && existingOrder.ShouldBeRunning)
			{
				Log(nameof(AddOrUpdateOrderServiceDefinition), "Order should be running, removing services and nodes from order");

				foreach (var serviceToRemove in servicesToRemove)
				{
					// Remove node and contributing resource from order
					var removeServiceFromRunningOrderTask = new RemoveServiceFromRunningOrderTask(Helpers, order, serviceToRemove, servicesToRemove);
					tasks.Add(removeServiceFromRunningOrderTask);
					if (!removeServiceFromRunningOrderTask.Execute())
					{
						Log(nameof(AddOrUpdateOrderServiceDefinition), "Removing service from running order failed");
						isSuccessful = false;
						return;
					}
				}
			}
			else
			{
				Log(nameof(AddOrUpdateOrderServiceDefinition), $"Order is not running, updating service definition {newOrderDefinition.ID}");

				var addOrUpdateServiceDefinitionTask = new AddOrUpdateServiceDefinitionTask(Helpers, newOrderDefinition);
				tasks.Add(addOrUpdateServiceDefinitionTask);
				if (!addOrUpdateServiceDefinitionTask.Execute())
				{
					Log(nameof(AddOrUpdateOrderServiceDefinition), "Add or update service definition task failed");
					isSuccessful = false;
					return;
				}

				order.Definition = order.Definition ?? new ServiceDefinition
				{
					BookingManagerElementName = SrmConfiguration.OrderBookingManagerElementName
				};

				order.Definition.Diagram = newOrderDefinition.Diagram;
				order.Definition.Id = addOrUpdateServiceDefinitionTask.Result;
			}

			Log(nameof(AddOrUpdateOrderServiceDefinition), $"Final result: Order has definition {order.Definition.Id} consisting of {order.Definition.Diagram.DiagramToString()}");

			orderGotNewOrUpdatedServiceDefinition = true;
		}

		private bool ServiceDefinitionSrmUpdateRequired(Net.ServiceManager.Objects.ServiceDefinition newServiceDefinition)
		{
			LogMethodStart(nameof(ServiceDefinitionSrmUpdateRequired), out var stopwatch);

			bool currentOrderDefinitionIdIsDifferentFromNewServiceDefinitionId = order.Definition?.Id != newServiceDefinition.ID;
			bool serviceDefinitionSrmUpdateRequired = currentOrderDefinitionIdIsDifferentFromNewServiceDefinitionId || newServiceDefinition.IsNewOrHasChangedComparedToExistingServiceDefinition(Helpers);

			Log(nameof(ServiceDefinitionSrmUpdateRequired), $"Order service definition has {(serviceDefinitionSrmUpdateRequired ? string.Empty : "not ")}changed, service definition SRM update {(serviceDefinitionSrmUpdateRequired ? string.Empty : "not ")}required.");

			LogMethodCompleted(nameof(ServiceDefinitionSrmUpdateRequired), stopwatch);

			return serviceDefinitionSrmUpdateRequired;
		}

		protected void GenerateProcessingServices()
		{
			if (options.HasFlag(OptionFlags.SkipGenerateProcessing))
			{
				Log(nameof(GenerateProcessingServices), "Skipped because of options flag.");
				return;
			}

			var generateProcessingServicesTask = new GenerateProcessingServicesTask(Helpers, order);
			tasks.Add(generateProcessingServicesTask);
			if (!generateProcessingServicesTask.Execute(false))
			{
				Log(nameof(GenerateProcessingServices), "Generate processing services task failed");
				isSuccessful = false;
			}
		}

		protected void CancelServicesToRemove()
		{
			foreach (var service in servicesToRemove)
			{
				if (!service.IsBooked) return;

				Log(nameof(CancelServicesToRemove), "Service to cancel: " + service.Name);

				Log(nameof(CancelServicesToRemove), "Cancel service task");
				var cancelServiceTask = new CancelServiceTask(Helpers, service, order);
				tasks.Add(cancelServiceTask);

				if (!cancelServiceTask.Execute()) Log(nameof(CancelServicesToRemove), "Cancel service task failed");
			}
		}

		protected void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch)
		{
			Helpers.LogMethodStart(this.GetType().Name, nameOfMethod, out stopwatch);
		}

		protected void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch = null)
		{
			Helpers.LogMethodCompleted(this.GetType().Name, nameOfMethod, null, stopwatch);
		}

		protected void Log(string nameOfMethod, string message)
		{
			Helpers.Log(this.GetType().Name, nameOfMethod, message, order.Name);
		}

		protected void Report(string message)
		{
			Helpers.ReportProgress(message);
		}
	}
}
