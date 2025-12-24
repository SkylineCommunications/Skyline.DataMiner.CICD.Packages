namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;

	public class EventManager : HelpedObject, IEventManager
	{
		private readonly JobManager jobManager;

		private SectionDefinition orderSectionDefinition; // saved for performance reasons
		private SectionDefinition customEventSectionDefinition; // saved for performance reasons
		private SectionDefinition staticEventSectionDefinition; // saved for performance reasons

		public EventManager(Helpers helpers) : base(helpers)
		{
			jobManager = new JobManager(Engine.SLNetRaw, this.helpers);
		}

		public SectionDefinition OrderSectionDefinition => orderSectionDefinition ?? (orderSectionDefinition = jobManager.GetOrderSectionDefinition());

		public SectionDefinition CustomEventSectionDefinition => customEventSectionDefinition ?? (customEventSectionDefinition = jobManager.GetCustomEventSectionDefinition());

		public SectionDefinition StaticEventSectionDefinition => staticEventSectionDefinition ?? (staticEventSectionDefinition = jobManager.GetStaticEventSectionDefinition());

		public Event GetEvent(Guid id)
		{
			using (StartPerformanceLogging())
			{
				var job = jobManager.GetJob(id);
				if (job == null)
				{
					Log(nameof(GetEvent), $"No Job found with ID {id}");
					return null;
				}

				var @event = new Event(helpers, job, OrderSectionDefinition, CustomEventSectionDefinition);

				Log(nameof(GetEvent), $"Retrieved Event: {@event}");

				return @event;
			}
		}

		public Event GetEvent(string projectNumber)
		{
			using (StartPerformanceLogging())
			{
				try
				{
					var projectNumberField = jobManager.GetFieldDescriptor(CustomEventSectionDefinition, "Project Number");
					if (projectNumberField == null) return null;

					var filter = JobExposers.FieldValues.JobField(projectNumberField.ID).Equal(projectNumber);

					var job = jobManager.GetJob(filter);
					if (job == null)
					{
						Log(nameof(GetEvent), $"No job found for Project Number '{projectNumber}'");
						return null;
					}

					var @event = new Event(helpers, job, OrderSectionDefinition, CustomEventSectionDefinition);

					Log(nameof(GetEvent), $"Retrieved Event: {@event}");

					return @event;
				}
				catch (Exception e)
				{
					Log(nameof(GetEvent), $"Exception while retrieving event: {e}");
					return null;
				}
			}
		}

		public Event GetEventByName(string name)
		{
			using (StartPerformanceLogging())
			{
				var jobNameField = jobManager.GetFieldDescriptor(CustomEventSectionDefinition, "Name");
				if (jobNameField == null) return null;

				var filter = JobExposers.FieldValues.JobField(jobNameField.ID).Equal(name);

				var job = jobManager.GetJob(filter);
				if (job == null) return null;

				return new Event(helpers, job, OrderSectionDefinition, CustomEventSectionDefinition);
			}
		}

		public bool AddOrUpdateEvent(Event eventToAddOrUpdate, Event existingEvent = null)
		{
			using (StartPerformanceLogging())
			{
				if (existingEvent == null)
				{
					if (eventToAddOrUpdate.Id != Guid.Empty)
					{
						// For updating existing events
						existingEvent = GetEvent(eventToAddOrUpdate.Id);
						Log(nameof(AddOrUpdateEvent), $"Found existing event based on ID {existingEvent.Id}");
					}
					else if (!string.IsNullOrEmpty(eventToAddOrUpdate.ProjectNumber))
					{
						// For linking Plasma orders to Ceiton events
						existingEvent = GetEvent(eventToAddOrUpdate.ProjectNumber);
						if (existingEvent != null) Log(nameof(AddOrUpdateEvent), $"Found existing event {existingEvent.Id} based on project number {eventToAddOrUpdate.ProjectNumber}");
					}
					else
					{
						// nothing
					}
				}

				if (existingEvent == null || eventToAddOrUpdate.Id == Guid.Empty)
				{
					bool addSuccessful = CreateNewEvent(eventToAddOrUpdate);
					return addSuccessful;
				}
				else
				{
					bool updateSuccessful = UpdateExistingEvent(eventToAddOrUpdate, existingEvent);
					return updateSuccessful;
				}
			}
		}

		private bool UpdateExistingEvent(Event eventToAddOrUpdate, Event existingEvent)
		{
			Report("Updating existing event...");
			Log(nameof(UpdateExistingEvent), "Updating existing event");

			bool updateSuccessful = eventToAddOrUpdate.TryUpdateJobToJobDomain();

			Log(nameof(UpdateExistingEvent), $"{(updateSuccessful ? "Succeeded" : "Failed")} to update Job for Event {eventToAddOrUpdate}");
			Report($"Updating existing event {(updateSuccessful ? "succeeded" : "failed")}");

			if (!helpers.OrderManagerElement.UpdateOrderManagerReference(existingEvent)) Log("AddOrUpdateEvent", "Event reference could not be updated in Order Manager element");
			return updateSuccessful;
		}

		private bool CreateNewEvent(Event eventToAddOrUpdate)
		{
			Report("Creating new event...");
			Log(nameof(CreateNewEvent), "Create new event");

			bool addSuccessful = eventToAddOrUpdate.TryAddJobToJobDomain();

			Log(nameof(CreateNewEvent), $"{(addSuccessful ? "Succeeded" : "Failed")} to add Job for Event {eventToAddOrUpdate}");
			Report($"Creating new event {(addSuccessful ? "succeeded" : "failed")}");

			if (!helpers.OrderManagerElement.UpdateOrderManagerReference(eventToAddOrUpdate)) Log(nameof(AddOrUpdateEvent), "Event reference could not be added in Order Manager element");
			return addSuccessful;
		}

		public bool AddOrUpdateOrderToEvent(Guid eventId, Guid orderId, bool orderEventReferenceUpdateRequired = false)
		{
			var liteOrder = helpers.OrderManager.GetLiteOrder(orderId);

			return AddOrUpdateOrderToEvent(eventId, liteOrder, orderEventReferenceUpdateRequired);
		}

		public bool AddOrUpdateOrderToEvent(Guid eventId, LiteOrder order, bool orderEventReferenceUpdateRequired = false)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));

			var @event = GetEvent(eventId);

			return AddOrUpdateOrderToEvent(@event, order, orderEventReferenceUpdateRequired);
		}

		public bool AddOrUpdateOrderToEvent(Event @event, LiteOrder order, bool orderEventReferenceUpdateRequired = false)
		{
			if (order == null) throw new ArgumentNullException(nameof(order));
			if (@event == null) throw new ArgumentNullException(nameof(@event));

			using (StartPerformanceLogging())
			{
				@event.AddOrUpdateOrder(order, helpers, orderEventReferenceUpdateRequired);

				return true;
			}
		}

		/// <summary>
		/// Used to update the status of an event.
		/// </summary>
		/// <param name="eventId">ID of the event.</param>
		/// <param name="status">Status to set.</param>
		/// <returns>Value indicating if the update was successful or not.</returns>
		public bool UpdateEventStatus(Guid eventId, Status status)
		{
			using (StartPerformanceLogging())
			{
				var job = jobManager.GetJob(eventId);
				if (job == null)
				{
					Log(nameof(UpdateEventStatus), $"Unable to retrieve Job with ID {eventId}");
					return false;
				}

				var eventSection = jobManager.GetCustomEventSection(job);
				if (eventSection == null)
				{
					Log(nameof(UpdateEventStatus), $"Unable to retrieve the custom Event section from Job with ID {eventId}");
					return false;
				}

				jobManager.UpdateSectionField(eventSection, CustomEventSectionDefinition, "Status", status.GetDescription());

				return jobManager.TryUpdateJob(job, out var resultingJob);
			}
		}

		public bool DeleteEvent(Guid id)
		{
			var job = jobManager.GetJob(id);
			if (job == null)
			{
				return true;
			}

			var eventInfo = GetEvent(id);
			if (eventInfo == null)
			{
				return true;
			}

			// remove the reference from the Order Manager element
			helpers.OrderManagerElement.DeleteOrderManagerReference(eventInfo);

			return jobManager.DeleteJob(job);
		}

		public IEnumerable<Event> GetAllEvents()
		{
			var @events = new List<Event>();

			var jobs = jobManager.GetJobs(JobExposers.FieldValues.JobStartGreaterThan(default(DateTime)));
			foreach (var job in jobs) @events.Add(new Event(helpers, job, OrderSectionDefinition, CustomEventSectionDefinition));

			return @events;
		}

		public IEnumerable<Event> GetAllEventsEndingInTheFuture()
		{
			var @events = new List<Event>();

			var jobs = jobManager.GetJobs(JobExposers.FieldValues.JobEndGreaterThan(DateTime.Now));
			foreach (var job in jobs) @events.Add(new Event(helpers, job, OrderSectionDefinition, CustomEventSectionDefinition));

			return @events;
		}

		public IEnumerable<Event> GetAllEventsBasedOnCompany(DateTime start, DateTime end, string requestedCompany)
		{
			var events = new List<Event>();

			var jobs = jobManager.GetJobs(JobExposers.FieldValues.JobStartGreaterThanOrEqual(start.ToUniversalTime()).AND(JobExposers.FieldValues.JobEndLessThanOrEqual(end.ToUniversalTime())));
			foreach (var job in jobs) events.Add(new Event(helpers, job, OrderSectionDefinition, CustomEventSectionDefinition));

			return events.Where(x => x.Company != null && x.Company.Equals(requestedCompany, StringComparison.InvariantCultureIgnoreCase) || x.Contract.Contains(requestedCompany));
		}

		public bool DeleteOrderFromEvent(Guid eventId, Guid orderId)
		{
			if (eventId == Guid.Empty) return true;

			var @event = GetEvent(eventId);
			if (@event == null) throw new EventNotFoundException(eventId);

			if (!@event.RemoveOrder(orderId)) return false;

			return @event.TryUpdateJobToJobDomain();
		}

		public bool DeleteOrderFromEvent(Event @event, Guid orderId)
		{
			if (@event == null) throw new ArgumentNullException(nameof(@event));

			if (!@event.RemoveOrder(orderId)) return false;

			return @event.TryUpdateJobToJobDomain();
		}

		public bool HasOrders(Guid eventId)
		{
			var job = jobManager.GetJob(eventId);
			if (job == null) return false;

			var ordersSectionDefinition = jobManager.GetOrderSectionDefinition() as CustomSectionDefinition;
			if (ordersSectionDefinition == null) return false;

			var reservationFieldDescriptor = jobManager.GetOrCreateOrderReservationIdFieldDescriptor(ordersSectionDefinition);
			if (reservationFieldDescriptor == null) return false;

			return jobManager.HasReservations(job, ordersSectionDefinition, reservationFieldDescriptor);
		}

		public bool HasOrder(Guid jobId, Guid orderId)
		{
			var job = jobManager.GetJob(jobId);
			if (job == null) return false;

			var @event = new Event(helpers, job, OrderSectionDefinition, CustomEventSectionDefinition);

			return @event.HasOrder(orderId);
		}

		public List<Order> GetOrdersInEvent(Guid eventGuid)
		{
			var orders = new List<Order>();

			var job = jobManager.GetJob(eventGuid);
			if (job == null) return orders;

			var ordersSectionDefinition = jobManager.GetOrderSectionDefinition() as CustomSectionDefinition;
			if (ordersSectionDefinition == null) return orders;

			var reservationFieldDescriptor = jobManager.GetOrCreateOrderReservationIdFieldDescriptor(ordersSectionDefinition);
			if (reservationFieldDescriptor == null) return orders;

			var orderIds = jobManager.GetReservationIdsLinkedToJob(job, ordersSectionDefinition, reservationFieldDescriptor);
			foreach (var orderId in orderIds)
			{
				Order order = helpers.OrderManager.GetOrder(orderId);
				if (order != null) orders.Add(order);
			}

			return orders;
		}

		public List<LiteOrder> GetLiteOrdersInEvent(Guid eventGuid)
		{
			var orders = new List<LiteOrder>();

			var job = jobManager.GetJob(eventGuid);
			if (job == null) return orders;

			var ordersSectionDefinition = jobManager.GetOrderSectionDefinition() as CustomSectionDefinition;
			if (ordersSectionDefinition == null) return orders;

			var reservationFieldDescriptor = jobManager.GetOrCreateOrderReservationIdFieldDescriptor(ordersSectionDefinition);
			if (reservationFieldDescriptor == null) return orders;

			var orderIds = jobManager.GetReservationIdsLinkedToJob(job, ordersSectionDefinition, reservationFieldDescriptor);
			foreach (var orderId in orderIds)
			{
				try
				{
					var order = helpers.OrderManager.GetLiteOrder(orderId);
					if (order != null) orders.Add(order);
				}
				catch (ReservationNotFoundException)
				{
					continue;
				}
			}

			return orders;
		}

		/// <summary>
		/// Checks if the event with the provided id has attachments.
		/// </summary>
		/// <param name="eventGuid">ID of the event to be checked.</param>
		/// <returns>Returns true if the event contains attachments, else false.</returns>
		public bool HasAttachments(Guid eventGuid)
		{
			return jobManager.GetAttachments(new JobID(eventGuid)).Any();
		}

		/// <summary>
		/// This method is used to copy attachments over from one event to another.
		/// </summary>
		/// <param name="sourceEventGuid">Event from which the attachments will be retrieved.</param>
		/// <param name="destinationEventGuid">Event to which the attachments will be copied to.</param>
		/// <returns>Indicates if the copy was successful.</returns>
		public bool CopyAttachments(Guid sourceEventGuid, Guid destinationEventGuid)
		{
			try
			{
				List<string> sourceFileNames = jobManager.GetAttachments(new JobID(sourceEventGuid));

				byte[] fileContent;
				List<string> destinationFileNames;
				string newFileName;
				foreach (string fileName in sourceFileNames)
				{
					newFileName = fileName;
					fileContent = jobManager.GetAttachment(new JobID(sourceEventGuid), fileName);
					destinationFileNames = jobManager.GetAttachments(new JobID(destinationEventGuid));

					while (destinationFileNames.Contains(newFileName))
					{
						string[] splitFileName = newFileName.Split('.');
						splitFileName[0] += "_copy";
						newFileName = String.Join(".", splitFileName);
					}

					jobManager.AddAttachment(new JobID(destinationEventGuid), newFileName, fileContent);
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// This method is used to move attachments over from one event to another.
		/// This means that the attachments will be removed from the source event.
		/// </summary>
		/// <param name="sourceEventGuid">Event from which the attachments will be retrieved.</param>
		/// <param name="destinationEventGuid">Event to which the attachments will be moved.</param>
		/// <returns>Indicates if the move was successful.</returns>
		public bool MoveAttachments(Guid sourceEventGuid, Guid destinationEventGuid)
		{
			try
			{
				List<string> sourceFileNames = jobManager.GetAttachments(new JobID(sourceEventGuid));

				byte[] fileContent;
				List<string> destinationFileNames;
				string newFileName;
				foreach (string fileName in sourceFileNames)
				{
					newFileName = fileName;
					fileContent = jobManager.GetAttachment(new JobID(sourceEventGuid), fileName);
					destinationFileNames = jobManager.GetAttachments(new JobID(destinationEventGuid));

					while (destinationFileNames.Contains(newFileName))
					{
						string[] splitFileName = newFileName.Split('.');
						splitFileName[0] += "_copy";
						newFileName = String.Join(".", splitFileName);
					}

					jobManager.AddAttachment(new JobID(destinationEventGuid), newFileName, fileContent);
					jobManager.DeleteAttachments(new JobID(sourceEventGuid), fileName);
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		protected void Report(string message)
		{
			helpers.ReportProgress(message);
		}
	}
}