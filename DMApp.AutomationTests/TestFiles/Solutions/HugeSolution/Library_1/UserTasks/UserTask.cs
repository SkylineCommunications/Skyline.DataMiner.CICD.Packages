namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;

	public abstract class UserTask : ICloneable
    {
        protected static readonly string TicketFieldDescription = "Description";
		protected static readonly string TicketFieldState = "State";
		protected static readonly string TicketFieldName = "Name";
		protected static readonly string TicketFieldUserGroup = "User Group";
		protected static readonly string TicketFieldOwner = "Owner";

        protected Helpers helpers;

        protected Guid ticketFieldResolverId;
		protected Ticket ticket;

		protected UserTask()
		{

		}

        protected UserTask(Helpers helpers, Guid ticketFieldResolverId, string description, UserGroup userGroup, UserTaskStatus status = UserTaskStatus.Incomplete)
        {
			this.helpers = helpers;
			this.ticketFieldResolverId = ticketFieldResolverId;

			Status = status;
			Description = description;
			UserGroup = userGroup;
		}

        /// <summary>
        /// Constructor used to create a user task from a ticket.
        /// </summary>
        protected UserTask(Helpers helpers, Ticket ticket)
        {
            this.helpers = helpers;
            this.ticket = ticket;

            ID = ticket.ID.ToString();
            Name = Convert.ToString(ticket.CustomTicketFields[TicketFieldName]);
            Description = Convert.ToString(ticket.CustomTicketFields[TicketFieldDescription]);
            UserGroup = Convert.ToString(ticket.CustomTicketFields[TicketFieldUserGroup]).GetEnumValue<UserGroup>();
            Owner = ticket.CustomTicketFields.TryGetValue(TicketFieldOwner, out var owner) ? Convert.ToString(owner) : string.Empty;

            var statusTicketField = ticket.GetTicketField(TicketFieldState) as Net.Ticketing.Validators.GenericEnumEntry<int>;
            if (statusTicketField != null) Status = (UserTaskStatus)statusTicketField.Value;

            helpers.Log(nameof(UserTask), nameof(UserTask), "statusTicketField: " + statusTicketField?.ToFormattedString());
        }

        public static UserTask Factory(Helpers helpers, Ticket ticket)
        {
			if (ticket.CustomTicketFields.ContainsKey(LiveUserTask.TicketFieldServiceId))
			{
				var serviceId = Guid.Parse(Convert.ToString(ticket.CustomTicketFields[LiveUserTask.TicketFieldServiceId]));
				var service = helpers.ServiceManager.GetService(serviceId);

				return new LiveUserTask(helpers, service, ticket);
			}
			else if (ticket.CustomTicketFields.ContainsKey(NonLiveUserTask.TicketFieldIngestExportFK))
			{
				return NonLiveUserTask.Factory(helpers, ticket);
			}
			else
			{
				throw new InvalidOperationException($"Unknown ticket type {ticket.ID}");
			}
        }

        /// <summary>
        /// The ID of the ticket in the ticketing domain.
        /// Format: [dataminer ID]/[ticket ID] .
        /// </summary>
        public string ID { get; protected set; }

        /// <summary>
        /// Status of the User Task.
        /// </summary>
        public UserTaskStatus Status { get; set; }

        public string Name { get; set; }

        public string Owner { get; set; }

        public string Description { get; set; }

        public UserGroup UserGroup { get; set; }

		public abstract void AddOrUpdate(Helpers helpers);

		/// <summary>
		/// Deletes the user task from the ticketing domain.
		/// </summary>
		public void Delete(TicketingManager ticketingManager)
        {
            if (ticket == null) return;

            ticketingManager.DeleteTicket(ticket);
        }

		protected void SetStatus(Helpers helpers, UserTaskStatus status)
		{
			Status = status;
			AddOrUpdate(helpers);
		}

		public abstract object Clone();
	}
}