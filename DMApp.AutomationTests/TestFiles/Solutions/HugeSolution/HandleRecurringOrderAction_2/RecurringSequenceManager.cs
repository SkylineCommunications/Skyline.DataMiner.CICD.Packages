namespace HandleRecurringOrderAction_2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status;

	public class RecurringSequenceManager : IDisposable
	{
		private readonly TimeSpan slidingWindowSize;
		private readonly int maxBookingAmount;
		private readonly OrderTemplate template;
		private readonly Helpers helpers;
		private readonly RecurringSequenceInfo recurringSequenceInfo;
		private readonly List<DateTime> allFutureOccurrences;
		private readonly List<ReservationInstance> allExistingOrders;
		private readonly UserInfo userInfo;
		private readonly Event _event;

		// DCP200430
		private readonly Dictionary<Order, List<FunctionResource>> vizremOrdersUnableToBook = new Dictionary<Order, List<FunctionResource>>();

		public RecurringSequenceManager(Helpers helpers, RecurringSequenceInfo recurringOrderInfo)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.slidingWindowSize = helpers.OrderManagerElement.GetRecurringOrdersSlidingWindowSize();
			this.maxBookingAmount = helpers.OrderManagerElement.GetRecurringOrdersMaxBookingAmount();
			this.recurringSequenceInfo = recurringOrderInfo ?? throw new ArgumentNullException(nameof(recurringOrderInfo));

			if (!helpers.ContractManager.TryGetOrderTemplate(recurringOrderInfo.TemplateId, out this.template))
			{
				throw new ArgumentException($"Unable to retrieve order template {recurringOrderInfo.TemplateId}", nameof(recurringOrderInfo));
			}

			this._event = helpers.EventManager.GetEvent(recurringOrderInfo.EventId);

			this.userInfo = string.IsNullOrWhiteSpace(template.CreatedByUserName) ? null : helpers.ContractManager.GetUserInfo(template.CreatedByUserName, _event);

			allFutureOccurrences = recurringOrderInfo.GetAllFutureOccurrences();

			Log(nameof(RecurringSequenceManager), $"All occurrences for {recurringSequenceInfo.Name}: {string.Join(";", allFutureOccurrences.Select(o => o.ToString("o")))}");

			this.allExistingOrders = helpers.OrderManager.GetAllReservationInstancesFromTemplate(recurringOrderInfo.TemplateId).ToList();

			Log(nameof(RecurringSequenceManager), $"All existing orders for {recurringSequenceInfo.Name}: '{string.Join(", ", allExistingOrders.Select(o => $"{o.Name}({o.ID})"))}'");

			Log(nameof(RecurringSequenceManager), $"Template has{(recurringSequenceInfo.TemplateIsUpdated ? string.Empty : " not")} been updated.");
			// Boolean is true when
			// - Reprocess button was pressed in Recurring Orders table in Order Manager element
			// - Recurring Order Sequence was edited using LOF
			// Boolean is false when daily evaluation is triggered by Order Manager timer
		}

		public void ProcessRecurringOrderInfo()
		{
			try
			{
				if (recurringSequenceInfo.TemplateId == Guid.Empty)
				{
					Log(nameof(ProcessRecurringOrderInfo), $"Recurring order {recurringSequenceInfo.Name} has empty template ID.");
					return;
				}

				if (recurringSequenceInfo.TemplateIsUpdated || recurringSequenceInfo.Recurrence.EffectiveEndDate < DateTime.Now)
				{
					DeleteAllFutureRecurringOrders();
				}

				BookNewRecurringOrders();

				HandleRecurrenceEnding();

				ReportResults();
			}
			catch (Exception e)
			{
				Log(nameof(ProcessRecurringOrderInfo), $"Something went wrong: {e}");
			}
		}

		private void ReportResults()
		{
			foreach (var failedOrder in vizremOrdersUnableToBook)
			{
				NotificationManager.SendUnableToBookVizremOrderMail(helpers, failedOrder.Key, failedOrder.Value);
			}
		}

		private void BookNewRecurringOrders()
		{
			var now = DateTime.Now;

			var existingOrderOccurrences = allExistingOrders.Select(x => x.Start.FromReservation().Add(x.GetPreRoll())).ToList();
			Log(nameof(BookNewRecurringOrders), $"All existing reservations for {recurringSequenceInfo.Name}: {string.Join(";", existingOrderOccurrences.Select(o => o.ToString("o")))}");

			int bookingCounter = 0;
			foreach (var occurrence in allFutureOccurrences)
			{
				var existingOrderForThisDay = allExistingOrders.SingleOrDefault(o => occurrence.Date <= o.Start && o.Start <= occurrence.Date.AddDays(1));
				if (existingOrderForThisDay != null)
				{
					Log(nameof(BookNewRecurringOrders), $"Order {existingOrderForThisDay.Name}({existingOrderForThisDay.ID}) already exists for occurrence {occurrence}, no need to create new one");
					continue;
				}

				bool occurrenceFallsWithinSlidingWindow = recurringSequenceInfo.Recurrence.StartTime <= occurrence && occurrence <= now + slidingWindowSize;
				if (!occurrenceFallsWithinSlidingWindow) continue;

				bool occurrenceAlreadyExists = existingOrderOccurrences.Any(o => o == occurrence);
				if (occurrenceAlreadyExists)
				{
					Log(nameof(BookNewRecurringOrders), $"Recurring order {recurringSequenceInfo.Name} occurrence {occurrence.ToString("o")} already exists");
					continue;
				}

				BookRecurringOrder(occurrence);

				if (++bookingCounter >= maxBookingAmount)
				{
					Log(nameof(BookNewRecurringOrders), $"Booked the max of {maxBookingAmount} new orders for {recurringSequenceInfo.Name}, aborting script.");
					break;
				}
			}
		}

		private void HandleRecurrenceEnding()
		{
			bool recurrenceHasEnded = !allFutureOccurrences.Any() && !allExistingOrders.Any(o => DateTime.Now <= o.Start.FromReservation().Add(o.GetPreRoll()));

			if (recurrenceHasEnded)
			{
				Log(nameof(HandleRecurrenceEnding), $"Recurrence {recurringSequenceInfo.Name} has ended, deleting order template, event (if empty) and recurring order row...");

				// Delete Template
				if (!helpers.ContractManager.TryDeleteOrderTemplate(recurringSequenceInfo.TemplateId))
				{
					Log(nameof(DeleteRecurringOrder), $"Unable to delete order template {recurringSequenceInfo.TemplateId}");
				}

				// Delete Event if empty
				if (_event != null && !helpers.EventManager.GetLiteOrdersInEvent(_event.Id).Any())
				{
					helpers.EventManager.DeleteEvent(_event.Id);
				}

				// Delete Recurring Orders table row
				helpers.OrderManagerElement.DeleteRow(2400, recurringSequenceInfo.TemplateId.ToString());
			}
		}

		private bool OrderTimingMatchesRecurrenceSequence(ReservationInstance existingOrder)
		{
			var orderStart = existingOrder.Start.FromReservation().Add(existingOrder.GetPreRoll());
			var orderEnd = existingOrder.End.FromReservation().Add(-existingOrder.GetPostRoll());

			bool orderStartsBeforeRecurrenceEnd = orderStart < recurringSequenceInfo.Recurrence.EffectiveEndDate;

			Log(nameof(OrderTimingMatchesRecurrenceSequence), $"Existing order {existingOrder.Name} start {orderStart.ToString("o")} falls {(orderStartsBeforeRecurrenceEnd ? "inside" : "outside")} recurrence end {recurringSequenceInfo.Recurrence.EffectiveEndDate.ToString("o")}");

			bool orderStartTimeMatchesSequence = allFutureOccurrences.Any(occurrence => occurrence == orderStart);

			Log(nameof(OrderTimingMatchesRecurrenceSequence), $"Existing order {existingOrder.Name} start {orderStart.ToString("o")} {(orderStartTimeMatchesSequence ? "matches a" : "does not match any")} value from {string.Join(" ; ", allFutureOccurrences.Select(o => o.ToString("o")))}");

			var allOccurrenceEndTimes = allFutureOccurrences.Select(o => o.Add(template.Duration)).ToList();
			bool orderEndTimeMatchesSequence = allOccurrenceEndTimes.Any(occurrenceEnd => occurrenceEnd == orderEnd);

			Log(nameof(OrderTimingMatchesRecurrenceSequence), $"Existing order {existingOrder.Name} end {orderEnd.ToString("o")} {(orderEndTimeMatchesSequence ? "matches a" : "does not match any")} value from {string.Join(" ; ", allOccurrenceEndTimes.Select(o => o.ToString("o")))}");

			return orderStartsBeforeRecurrenceEnd && orderStartTimeMatchesSequence && orderEndTimeMatchesSequence;
		}

		private Status DetermineOrderStatus(Order order)
		{
			if (order.AllServices.Exists(s => s.Definition.Name.Contains("Unknown")))
			{
				return Status.PlannedUnknownSource;
			}
			else
			{
				return userInfo != null && userInfo.IsMcrUser ? Status.Confirmed : Status.Planned;
			}
		}

		private void BookRecurringOrder(DateTime occurrence)
		{
			var order = Order.FromTemplate(helpers, template, $"{recurringSequenceInfo.Name} [{occurrence.ToFinnishDateString()}]", occurrence);

			order.Status = DetermineOrderStatus(order);

			order.CreatedByUserName = string.IsNullOrWhiteSpace(template.CreatedByUserName) ? "DataMiner Agent" : template.CreatedByUserName;
			order.LastUpdatedBy = "DataMiner Agent";
			order.Event = _event;

			if (order.Subtype == OrderSubType.Vizrem)
			{
				order.IsInternal = true;

				foreach (var service in order.AllServices)
				{
					if (!service.AllCurrentlyAssignedResourcesAreAvailable(helpers, out var unavailableResources))
					{
						Log(nameof(BookRecurringOrder), $"Resources {string.Join(", ", unavailableResources.Select(r => r.Name))} used in VIZREM order {order.Name} are not available, order will not be created");

						vizremOrdersUnableToBook.Add(order, unavailableResources);
						return;
					}
				}
			}

			var tasks = order.AddOrUpdate(helpers, false).Tasks;

			var failedBlockingTask = tasks.FirstOrDefault(t => t.IsBlocking && t.Status == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Status.Fail);
			bool orderAddOrUpdateFailed = failedBlockingTask != null;
			if (orderAddOrUpdateFailed)
			{
				Log(nameof(BookRecurringOrder), $"Failed to book recurred order {order.Name} (start {occurrence.ToString("o")}): {failedBlockingTask.Exception}");
			}
			else
			{
				Log(nameof(BookRecurringOrder), $"Successfully booked recurred order {order.Name} (start {occurrence.ToString("o")})");
			}
		}

		private void DeleteRecurringOrder(ReservationInstance orderReservation)
		{
			Log(nameof(DeleteRecurringOrder), $"Deleting existing recurring order {orderReservation.Name}");

			ManageBookServicesFlow(orderReservation.ID);

			helpers.OrderManager.DeleteOrder(orderReservation.ID);
		}

		private void ManageBookServicesFlow(Guid orderId)
		{
			bool orderHasOngoingBookServicesProcess = helpers.OrderManagerElement.TryGetBookServicesStatus(orderId, out BookServicesStatus bookServicesStatus) && bookServicesStatus == BookServicesStatus.Ongoing;
			var timeoutTimer = TimeSpan.Zero;

			while (orderHasOngoingBookServicesProcess && timeoutTimer < TimeSpan.FromMinutes(2))
			{
				Log(nameof(ManageBookServicesFlow), $"Entry {orderId} has status {bookServicesStatus.ToString()} in Orders table in Order Manager element, waiting 10 seconds...");

				var fiveSeconds = TimeSpan.FromSeconds(5);
				Thread.Sleep(fiveSeconds);
				timeoutTimer += fiveSeconds;

				orderHasOngoingBookServicesProcess = helpers.OrderManagerElement.TryGetBookServicesStatus(orderId, out bookServicesStatus) && bookServicesStatus == BookServicesStatus.Ongoing;
			}
		}

		private void VerifyAndDeleteRecurringOrders()
		{
			// Delete order in case of
			// - name not matching with recurrence sequence
			// - timing not matching with recurrence sequence
			// - updated template

			Log(nameof(VerifyAndDeleteRecurringOrders), $"Template has{(recurringSequenceInfo.TemplateIsUpdated ? string.Empty : " not")} been updated.");

			if (recurringSequenceInfo.TemplateIsUpdated || recurringSequenceInfo.Recurrence.EffectiveEndDate < DateTime.Now)
			{
				DeleteAllFutureRecurringOrders();
			}
			else if(recurringSequenceInfo.Recurrence.EffectiveEndDate < DateTime.Now)
			{
				var deletedOrderIds = new List<Guid>();

				foreach (var existingOrder in allExistingOrders)
				{
					bool orderTimingMatchesRecurrenceSequence = OrderTimingMatchesRecurrenceSequence(existingOrder);

					bool orderNameMatchesSequence = OrderNameMatchesSequence(existingOrder);

					if ((!orderTimingMatchesRecurrenceSequence || !orderNameMatchesSequence) && TryDeleteOrder(existingOrder))
					{
						deletedOrderIds.Add(existingOrder.ID);
					}
				}

				allExistingOrders.RemoveAll(o => deletedOrderIds.Contains(o.ID));
			}
			else
			{
				// nothing
			}
		}

		private bool OrderNameMatchesSequence(ReservationInstance existingOrder)
		{
			var orderName = existingOrder.Name.Contains(" [") ? existingOrder.Name.Substring(0, existingOrder.Name.LastIndexOf(" [", StringComparison.InvariantCulture)) : existingOrder.Name;

			bool orderNameMatchesSequence = orderName == recurringSequenceInfo.Name;

			Log(nameof(OrderNameMatchesSequence), $"Existing order name {orderName} {(orderNameMatchesSequence ? "matches" : "does not match")} recurring sequence name {recurringSequenceInfo.Name}");

			return orderNameMatchesSequence;
		}

		private bool TryDeleteOrder(ReservationInstance existingOrder)
		{
			var orderLock = helpers.LockManager.RequestOrderLock(existingOrder.ID);

			if (orderLock.IsLockGranted)
			{
				DeleteRecurringOrder(existingOrder);
				return true;
			}
			else
			{
				Log(nameof(VerifyAndDeleteRecurringOrders), $"Unable to delete order {existingOrder.Name} because lock was not granted");
				return false;
			}
		}

		private void DeleteAllFutureRecurringOrders()
		{
			var deletedOrderIds = new List<Guid>();

			foreach (var order in allExistingOrders.Where(o => DateTime.Now <= o.Start))
			{
				if (TryDeleteOrder(order))
				{
					deletedOrderIds.Add(order.ID);
				}
			}

			allExistingOrders.RemoveAll(o => deletedOrderIds.Contains(o.ID));
		}

		private void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers.Log(nameof(RecurringSequenceManager), nameOfMethod, message, nameOfObject);
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					helpers.Dispose();
				}

				disposedValue = true;
			}
		}

		~RecurringSequenceManager()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}