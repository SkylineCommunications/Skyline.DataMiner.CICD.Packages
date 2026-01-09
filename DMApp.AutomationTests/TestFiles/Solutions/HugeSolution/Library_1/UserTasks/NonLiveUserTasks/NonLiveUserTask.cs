namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;
	using Type = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type;
	using UserGroup = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.UserGroup;

	public abstract class NonLiveUserTask : UserTask
    {
        protected static readonly string TicketFieldOrderName = "Non Live Order Name";
        protected static readonly string TicketFieldDeleteDate = "Delete Date";
        protected static readonly string TicketFieldOriginalDeleteDate = "Original Delete Date";
        protected static readonly string TicketFieldDeleteComment = "Delete Comment";
        protected static readonly string TicketFieldFolderPath = "Folder Path";
        protected static readonly string TicketFieldOrdererEmail = "Orderer Email";
        protected static readonly string TicketFieldOrdererName = "Orderer Name";
        protected static readonly string TicketFieldNonLiveTypes = "Non Live Types";
        protected static readonly string TeamHkiField = "Team_HKI";
        protected static readonly string TeamNewsField = "Team_NEWS";
        protected static readonly string TeamTreField = "Team_TRE";
        protected static readonly string TeamVsaField = "Team_VSA";
        protected static readonly string TeamMgmtField = "Team_MGMT";
		protected static readonly string TicketFieldDeliveryDate = "Delivery Date";
		protected static readonly string TicketFieldDeadlineDate = "Deadline Date";
		protected static readonly string TicketFieldDateOfCompletion = "Date Of Completion";
		public static readonly string TicketFieldIngestExportFK = "Ingest Export FK";

		protected NonLiveUserTask()
		{

		}

		protected NonLiveUserTask(Helpers helpers, Ticket ticket) : base(helpers, ticket)
        {
            Initialize(ticket);
        }

        protected NonLiveUserTask(Helpers helpers, Guid ticketFieldResolverId, NonLiveOrder nonLiveOrder, string description) : base(helpers, ticketFieldResolverId, description, UserGroup.None, UserTaskStatus.Incomplete)
        {
            Name = $"{nonLiveOrder.OrderDescription}: {Description}";

            IngestExportForeignKey = nonLiveOrder.DataMinerId + "/" + nonLiveOrder.TicketId;
		}

        public abstract Type LinkedOrderType { get; }

        public string OrderName { get; set; }

        public DateTime DeleteDate { get; set; }
        
        public DateTime OriginalDeleteDate { get; protected set; }

        public string DeleteComment { get; set; }

        public string FolderPath { get; set; }

        public string OrdererName { get; set; }

        public string OrdererEmail { get; set; }

        public bool TeamHki { get; set; }

        public bool TeamNews { get; set; }

        public bool TeamTre { get; set; }

        public bool TeamVsa { get; set; }

        public bool TeamMgmt { get; set; }

		public DateTime DeliveryDate { get; set; }

		public DateTime DeadlineDate { get; set; }

		public DateTime DateOfCompletion { get; set; }

		/// <summary>
		/// ID of the non-live order ticket this user task is linked to.
		/// </summary>
		public string IngestExportForeignKey { get; set; }

		public static new NonLiveUserTask Factory(Helpers helpers, Ticket ticket)
		{
			var nonLiveTypeTicketField = ticket.GetTicketField(TicketFieldNonLiveTypes) as Net.Ticketing.Validators.GenericEnumEntry<int>;
			if (nonLiveTypeTicketField == null) return null;

			switch ((Type)nonLiveTypeTicketField.Value)
			{
				case Type.Import:
					return new ImportUserTask(helpers, ticket);
				case Type.IplayFolderCreation:
					return new IplayFolderCreationUserTask(helpers, ticket);
				case Type.IplayWgTransfer:
					return new TransferUserTask(helpers, ticket);
				case Type.NonInterplayProject:
					return new NonIplayProjectUserTask(helpers, ticket);
				default:
					// No Action
					return null;
			}
		}

		public abstract void SendReminderMail();

        public virtual void SetValuesBasedOnNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
            OrdererName = nonLiveOrder.CreatedBy;
            OrdererEmail = nonLiveOrder.CreatedByEmail;

            TeamHki = nonLiveOrder.TeamHki;
            TeamNews = nonLiveOrder.TeamNews;
            TeamTre = nonLiveOrder.TeamTre;
            TeamVsa = nonLiveOrder.TeamVsa;
            TeamMgmt = nonLiveOrder.TeamMgmt;

			DeadlineDate = nonLiveOrder.Deadline;
        }

        protected void SetTicketFields(Helpers helpers)
        {
            helpers.LogMethodStart(nameof(NonLiveUserTask), nameof(AddOrUpdate), out var stopwatch);

            ticket = ticket ?? new Ticket { CustomFieldResolverID = ticketFieldResolverId };

            ticket.CustomTicketFields[TicketFieldState] = new Net.Ticketing.Validators.GenericEnumEntry<int> { Value = (int)Status, Name = Status.ToString() };
            ticket.CustomTicketFields[TicketFieldName] = Name ?? String.Empty;
            ticket.CustomTicketFields[TicketFieldDescription] = Description ?? String.Empty;
            ticket.CustomTicketFields[TicketFieldUserGroup] = UserGroup.GetDescription();
            ticket.CustomTicketFields[TicketFieldOwner] = Owner ?? "None";

            ticket.CustomTicketFields[TicketFieldIngestExportFK] = IngestExportForeignKey;

            var nonLiveTypesTicketField = new Net.Ticketing.Validators.GenericEnumEntry<int> { Value = (int)LinkedOrderType, Name = LinkedOrderType.GetDescription() };
            ticket.CustomTicketFields[TicketFieldNonLiveTypes] = nonLiveTypesTicketField;
            
            ticket.CustomTicketFields[TicketFieldOrderName] = OrderName ?? String.Empty;
            
            if (DeleteDate != default) ticket.CustomTicketFields[TicketFieldDeleteDate] = DeleteDate;
            
            ticket.CustomTicketFields[TicketFieldDeleteComment] = DeleteComment ?? String.Empty;
            ticket.CustomTicketFields[TicketFieldFolderPath] = FolderPath ?? String.Empty;
            ticket.CustomTicketFields[TicketFieldOrdererName] = OrdererName ?? String.Empty;
            ticket.CustomTicketFields[TicketFieldOrdererEmail] = OrdererEmail ?? String.Empty;

            ticket.CustomTicketFields[TeamHkiField] = TeamHki.ToString();
            ticket.CustomTicketFields[TeamMgmtField] = TeamMgmt.ToString();
            ticket.CustomTicketFields[TeamNewsField] = TeamNews.ToString();
            ticket.CustomTicketFields[TeamVsaField] = TeamVsa.ToString();
            ticket.CustomTicketFields[TeamTreField] = TeamTre.ToString();

			ticket.CustomTicketFields[TicketFieldDeliveryDate] = DeliveryDate;
			ticket.CustomTicketFields[TicketFieldDeadlineDate] = DeadlineDate;
			ticket.CustomTicketFields[TicketFieldDateOfCompletion] = DateOfCompletion;

			helpers.LogMethodCompleted(nameof(NonLiveUserTask), nameof(AddOrUpdate), null, stopwatch);
        }

        public bool TryUpdateNonLiveOrderStatus(Helpers helpers, User currentUser)
        {
            string[] splitFullTicketId = IngestExportForeignKey.Split('/');
            if (splitFullTicketId.Length != 2)
                throw new InvalidOperationException("Unable to split in DataMiner ID and Ticket ID");
            if (!Int32.TryParse(splitFullTicketId[0], out int dataminerId))
                throw new InvalidOperationException("Unable to parse DataMiner ID");
            if (!Int32.TryParse(splitFullTicketId[1], out int ticketId))
                throw new InvalidOperationException("Unable to parse Ticket ID");

            if (!helpers.NonLiveOrderManager.TryGetNonLiveOrder(Convert.ToInt32(splitFullTicketId[0]), Convert.ToInt32(splitFullTicketId[1]), out var retrievedNonLiveOrder))
            {
                helpers.Log(nameof(NonLiveUserTask), nameof(TryUpdateNonLiveOrderStatus), "Retrieving non live order fails with following ids: " + splitFullTicketId[0] + "/" + splitFullTicketId[1]);
                return false;
            }
            else
            {
                var retrievedUserTaks = helpers.NonLiveUserTaskManager.GetNonLiveUserTasks(retrievedNonLiveOrder);

                if (!retrievedUserTaks.All(u => u.Status == UserTaskStatus.Complete))
                {
                    retrievedNonLiveOrder.State = State.WorkInProgress;
                }

                return helpers.NonLiveOrderManager.AddOrUpdateNonLiveOrder(retrievedNonLiveOrder, currentUser, out var foundTicketId);
            }
        }

		/// <summary>
		/// Sets the status of the ticket to Complete.
		/// </summary>
		public void Complete(Helpers helpers)
		{
			SetStatus(helpers, UserTaskStatus.Complete);

			UpdateNonLiveOrder(helpers);
		}

		/// <summary>
		/// Sets the status of the ticket to Incomplete
		/// </summary>
		public void Incomplete(Helpers helpers)
		{
			SetStatus(helpers, UserTaskStatus.Incomplete);

			UpdateNonLiveOrder(helpers);
		}

		public void SetToCompleteOrInComplete(Helpers helpers)
		{
			if (Status == UserTaskStatus.Incomplete)
			{
				Complete(helpers);
			}
			else if (Status == UserTaskStatus.Complete)
			{
				Incomplete(helpers);
			}
			else
			{
				// Do nothing
			}
		}

		private void UpdateNonLiveOrder(Helpers helpers)
		{
			var currentUser = helpers.ContractManager.GetBaseUserInfo(helpers.Engine.UserLoginName)?.User;

			if (!helpers.NonLiveOrderManager.TryUpdateNonLiveOrderStatusBasedOnUserTask(helpers.NonLiveUserTaskManager, this, currentUser))
			{
				helpers.Log(nameof(NonLiveUserTask), nameof(UpdateNonLiveOrder), "Updating non live order status failed");
			}
		}

		private void Initialize(Ticket ticket)
        {
			IngestExportForeignKey = ticket.CustomTicketFields.TryGetValue(TicketFieldIngestExportFK, out var foreignKey) ? Convert.ToString(foreignKey) : string.Empty;

            OrderName = ticket.CustomTicketFields.TryGetValue(TicketFieldOrderName, out var orderName) ? Convert.ToString(orderName) : String.Empty;
            DeleteDate = ticket.CustomTicketFields.TryGetValue(TicketFieldDeleteDate, out var deleteDate) ? Convert.ToDateTime(deleteDate) : default;
            DeleteComment = ticket.CustomTicketFields.TryGetValue(TicketFieldDeleteComment, out var deleteComment) ? Convert.ToString(deleteComment) : String.Empty;
            FolderPath = ticket.CustomTicketFields.TryGetValue(TicketFieldFolderPath, out var folderPath) ? Convert.ToString(folderPath) : String.Empty;
            OrdererEmail = ticket.CustomTicketFields.TryGetValue(TicketFieldOrdererEmail, out var ordererEmail) ? Convert.ToString(ordererEmail) : String.Empty;
            OrdererName = ticket.CustomTicketFields.TryGetValue(TicketFieldOrdererName, out var ordererName) ? Convert.ToString(ordererName) : String.Empty;

            TeamHki = ticket.CustomTicketFields.TryGetValue(TeamHkiField, out var teamHkiField) && Convert.ToBoolean(teamHkiField);
            TeamNews = ticket.CustomTicketFields.TryGetValue(TeamNewsField, out var teamNewsField) && Convert.ToBoolean(teamNewsField);
            TeamTre = ticket.CustomTicketFields.TryGetValue(TeamTreField, out var teamTreField) && Convert.ToBoolean(teamTreField);
            TeamVsa = ticket.CustomTicketFields.TryGetValue(TeamVsaField, out var teamVsaField) && Convert.ToBoolean(teamVsaField);
            TeamMgmt = ticket.CustomTicketFields.TryGetValue(TeamMgmtField, out var teamMgmtField) && Convert.ToBoolean(teamMgmtField);

			DeliveryDate = ticket.CustomTicketFields.TryGetValue(TicketFieldDeliveryDate, out var deliveryDate) ? Convert.ToDateTime(deliveryDate) : default;
			DeadlineDate = ticket.CustomTicketFields.TryGetValue(TicketFieldDeadlineDate, out var deadlineDate) ? Convert.ToDateTime(deadlineDate) : default;
			DateOfCompletion = ticket.CustomTicketFields.TryGetValue(TicketFieldDateOfCompletion, out var dateOfCompletion) ? Convert.ToDateTime(dateOfCompletion) : default;
        }
    }
}
