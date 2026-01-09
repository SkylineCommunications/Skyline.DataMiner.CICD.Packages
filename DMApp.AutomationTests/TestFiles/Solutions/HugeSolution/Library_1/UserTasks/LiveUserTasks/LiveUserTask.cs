namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library_1.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Ticketing;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public class LiveUserTask : UserTask
	{
		public const string TicketFieldServiceId = "Service ID";
		private const string TicketFieldServiceStartTime = "Service Start Time";
		private const string TicketLinkKey = "ServiceReference";

		/// <summary>
		/// Constructor used to create a service-based user task from scratch.
		/// </summary>
		public LiveUserTask(Helpers helpers, Guid ticketFieldResolverId, Service service, string description, UserGroup userGroup, UserTaskStatus status = UserTaskStatus.Incomplete) : base(helpers, ticketFieldResolverId, description, userGroup, status)
		{
			ServiceId = service.Id;
			Service = service;
			ServiceStartTime = Service.Start;

			Name = GenerateName(Service, description);
		}

		/// <summary>
		/// Constructor used to create a service-based user task from a ticket.
		/// </summary>
		public LiveUserTask(Helpers helpers, Service service, Ticket ticket) : base(helpers, ticket)
		{
			ServiceId = Guid.Parse(Convert.ToString(ticket.CustomTicketFields[TicketFieldServiceId]));
			Service = service;
			ServiceStartTime = service.Start;

			Service.UserTasks = new[] { this }.Concat(Service.UserTasks.Where(x => !x.Name.Equals(this.Name))).ToList(); // replacing the existing User Task instance with this instance
		}

		private LiveUserTask(LiveUserTask other)
		{
			CloneHelper.CloneProperties(other, this, new List<string> { nameof(Service) });

			Service = other.Service;
		}

		/// <summary>
		/// ID of the Service this user task is linked to.
		/// </summary>
		public Guid ServiceId { get; private set; }

		public DateTime ServiceStartTime { get; private set; }

		/// <summary>
		/// The Service object this User Task is linked to.
		/// </summary>
		public Service Service { get; private set; }

		/// <summary>
		/// Adds or updates the User Task ticket to the ticketing domain.
		/// </summary>
		/// <param name="helpers">The TicketingManager used to add or update the User Task ticket.</param>
		public override void AddOrUpdate(Helpers helpers)
		{
			helpers.LogMethodStart(nameof(UserTask), nameof(AddOrUpdate), out var stopwatch);

			if (ticket == null)
			{
				ticket = new Ticket { CustomFieldResolverID = ticketFieldResolverId };

				ticket.AddTicketLink(TicketLinkKey, TicketLink.Create(new ReservationInstanceID(ServiceId)));		
			}

			ticket.CustomTicketFields[TicketFieldState] = new Net.Ticketing.Validators.GenericEnumEntry<int> { Value = (int)Status, Name = Status.ToString() };
			ticket.CustomTicketFields[TicketFieldName] = Name;
			ticket.CustomTicketFields[TicketFieldDescription] = Description;
			ticket.CustomTicketFields[TicketFieldUserGroup] = UserGroup.GetDescription();
			ticket.CustomTicketFields[TicketFieldOwner] = Owner ?? "None";

			ticket.CustomTicketFields[TicketFieldServiceId] = ServiceId.ToString();
			ticket.CustomTicketFields[TicketFieldServiceStartTime] = ServiceStartTime;

			helpers.UserTaskManager.TicketingManager.AddOrUpdateTicket(ticket, out var ticketId);

			ID = ticketId;

			helpers.LogMethodCompleted(nameof(UserTask), nameof(AddOrUpdate), null, stopwatch);
		}

		public override object Clone()
		{
			return new LiveUserTask(this);
		}

		public bool CanBeAutoCompleted(Helpers helpers, Order order)
		{
			bool isOrderHiddenAutomatically = order.IsMediaOperatorMessiNewsHiddenFilterNeeded(helpers);
			bool manuallyAddedService = Service != null && !Service.IntegrationIsMaster && Service.IntegrationType == IntegrationType.None;

			bool isMessiNewsRecording = !string.IsNullOrEmpty(Service.Definition.Description) && Service.Definition.Description.Equals("Messi News");
			bool isMessiLiveBackupRecording = !string.IsNullOrEmpty(Service.Definition.Description) && Service.Definition.Description.Equals("Messi Live Backup");

			bool isPlasmaMainNewsRecording = isMessiNewsRecording && Service.IntegrationType == IntegrationType.Plasma;
			bool isPlasmaMainLiveBackupRecording = isMessiLiveBackupRecording && Service.IntegrationType == IntegrationType.Plasma;

			bool isParentRoutingOfPlasmaNewsRecording = Service.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.Routing && Service.Children.Any(s => !string.IsNullOrEmpty(s.Definition.Description) && s.Definition.Description.Equals("Messi News") && s.IntegrationType == IntegrationType.Plasma);

			bool canBeAutoCompleted = !manuallyAddedService && (((isPlasmaMainNewsRecording || isParentRoutingOfPlasmaNewsRecording) && isOrderHiddenAutomatically) || isPlasmaMainLiveBackupRecording);

			helpers?.Log(nameof(UserTask), nameof(CanBeAutoCompleted), $"{nameof(isOrderHiddenAutomatically)}={isOrderHiddenAutomatically}, {nameof(manuallyAddedService)}={manuallyAddedService}, {nameof(isPlasmaMainNewsRecording)}={isPlasmaMainNewsRecording}, {nameof(isParentRoutingOfPlasmaNewsRecording)}={isParentRoutingOfPlasmaNewsRecording}, {nameof(isPlasmaMainLiveBackupRecording)}={isPlasmaMainLiveBackupRecording}. Resulting in {nameof(canBeAutoCompleted)}={canBeAutoCompleted}");

			return canBeAutoCompleted;
		}

		/// <summary>
		/// Sets the status of the ticket to Complete.
		/// </summary>
		public void Complete(Helpers helpers, Order order = null)
		{
			SetStatus(helpers, UserTaskStatus.Complete);

			UpdateService(helpers, order);
		}

		/// <summary>
		/// Sets the status of the ticket to Incomplete
		/// </summary>
		public void Incomplete(Helpers helpers, Order order = null)
		{
			SetStatus(helpers, UserTaskStatus.Incomplete);

			UpdateService(helpers, order);
		}

		public void SetToCompleteOrInComplete(Order order = null)
		{
			if (Status == UserTaskStatus.Incomplete)
			{
				Complete(helpers, order);
			}
			else if (Status == UserTaskStatus.Complete)
			{
				Incomplete(helpers, order);
			}
			else
			{
				// Do nothing
			}
		}

		private void UpdateService(Helpers helpers, Order orderContainingService)
		{
			if (Service != null)
			{
				/* Removed by VSC as part of DCP195348 because of performance reasons
                orderContainingService = orderContainingService ?? helpers.OrderManager.GetOrder(Service.OrderReferences.First(), false, true);
                Service = orderContainingService.AllServices.SingleOrDefault(x => x.Id == Service.Id);
                */
				// Status == UserTaskStatus.Complete ---> DCP218659
				if (!Service.IsOrShouldBeRunning && !Service.InPreRollState && Status == UserTaskStatus.Complete) Service.ApplyProfileConfiguration(helpers);
				else if (Service.InPreRollState) Service.ProfileConfigurationFailReason = string.Empty;

				Service.TryUpdateStatus(helpers, orderContainingService);

				Service.TryUpdateCustomProperties(helpers, new Dictionary<string, object> { { ServicePropertyNames.AllUserTasksCompleted, Service.AllUserTasksCompleted.ToString().ToLower() /* VSC: ToLower() required */ } });

				/* Removed by VSC as part of DCP195348 because of refactoring reasons. Moved to UserTaskHandler class in UpdateTicketStatus script
                // Check if completing this user task should cause the Order and Services to start now
                if (startOrderNow) TryStartOrder(helpers, userTaskStatus);
                */
			}
		}

		private void InCompleteUserTaskOfNewsRecordingRoutingParent(Helpers helpers, List<Order> linkedOrders)
		{
			if (linkedOrders == null) return;

			Service routingParent = null;
			var orderOfRecordingService = linkedOrders.Single();

			foreach (var routingService in orderOfRecordingService.AllServices.Where(s => s != null && s.Definition?.VirtualPlatform == ServiceDefinition.VirtualPlatform.Routing))
			{
				if (routingService?.Children != null && routingService.Children.Any(c => c != null && c.Id == Service.Id))
				{
					routingParent = routingService;
					break;
				}
			}

			if (routingParent != null)
			{
				var routingUserTask = routingParent.UserTasks.First();
				routingUserTask.Incomplete(helpers);
			}
		}

		private void UpdateUiPropertiesFromLinkedOrders(Helpers helpers, List<Order> ordersToUpdate)
		{
			if (Service == null) return;

			foreach (var order in ordersToUpdate)
			{
				order.UpdateUiProperties(helpers);
			}
		}

		/// <summary>
		/// Generates the correct name for the service-based user task.
		/// </summary>
		private string GenerateName(Service service, string description)
		{
			if (service.Definition.VirtualPlatformServiceType == ServiceDefinition.VirtualPlatformType.Reception || service.Definition.VirtualPlatformServiceType == ServiceDefinition.VirtualPlatformType.Transmission)
			{
				return String.Format("{0} {1}: {2}", EnumExtensions.GetDescriptionFromEnumValue(service.Definition.VirtualPlatformServiceName), service.Name, description);
			}
			else
			{
				return String.Format("{0}: {1}", service.Name, description);
			}
		}
	}
}
