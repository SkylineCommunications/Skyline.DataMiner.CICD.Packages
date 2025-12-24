namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks
{
	using System;
	using Library_1.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;

	public class NonIplayProjectUserTask : NonLiveUserTask
    {
        private const string TicketFieldProductionDepartmentName = "Production Department Name";
        private const string TicketFieldImportDepartment = "Import Department";
        private const string TicketFieldProductionNumber = "Production Number";
        private const string TicketFieldProjectName = "Project Name";
		private const string TicketFieldIsilonBackupFileLocation = "Isilon Backup File Location";

        public NonIplayProjectUserTask(Helpers helpers, Ticket ticket) : base(helpers, ticket)
        {
            DeleteComment = ticket.CustomTicketFields.TryGetValue(TicketFieldDeleteComment, out var deleteCommentValue) ? Convert.ToString(deleteCommentValue) : String.Empty;
            
            ProductionDepartmentName = ticket.CustomTicketFields.TryGetValue(TicketFieldProductionDepartmentName, out var productionDepartmentNameValue) ? Convert.ToString(productionDepartmentNameValue) : String.Empty;

            ProductionNumber = ticket.CustomTicketFields.TryGetValue(TicketFieldProductionNumber, out var productionNumberValue) ? Convert.ToString(productionNumberValue) : String.Empty;

            ImportDepartment = ticket.CustomTicketFields.TryGetValue(TicketFieldImportDepartment, out var importDepartmentValue) ? Convert.ToString(importDepartmentValue) : String.Empty;

            ProjectName = ticket.CustomTicketFields.TryGetValue(TicketFieldProjectName, out var projectNameValue) ? Convert.ToString(projectNameValue) : String.Empty;

            OriginalDeleteDate = ticket.CustomTicketFields.TryGetValue(TicketFieldOriginalDeleteDate, out var deleteDate) ? (DateTime)deleteDate : DeleteDate;

			IsilonBackupFileLocation = ticket.CustomTicketFields.TryGetValue(TicketFieldIsilonBackupFileLocation, out var  isilonBackupFileLocation) ? Convert.ToString(isilonBackupFileLocation) : String.Empty;
        }

        public NonIplayProjectUserTask(Helpers helpers, Guid ticketFieldResolverId, Project projectOrder, string description) : base(helpers, ticketFieldResolverId, projectOrder, description)
        {
            SetValuesBasedOnNonLiveOrder(projectOrder);
            OriginalDeleteDate = projectOrder.OriginalDeleteDate; //Should only be set on ticket creation and never be updated.

            Status = UserTaskStatus.Pending;
        }

		private NonIplayProjectUserTask(NonIplayProjectUserTask other)
		{
			CloneHelper.CloneProperties(other, this);
		}

        public string ProductionDepartmentName { get; set; }

        public string ImportDepartment { get; set; }

        public string ProductionNumber { get; set; }

        public string ProjectName { get; set; }

		public string IsilonBackupFileLocation { get; set; }

		public override IngestExport.Type LinkedOrderType => IngestExport.Type.NonInterplayProject;

		public override object Clone()
		{
			return new NonIplayProjectUserTask(this);
		}

		public override void SetValuesBasedOnNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
			if (!(nonLiveOrder is Project projectOrder))
			{
                throw new ArgumentException($"Argument is not of type {nameof(Project)}", nameof(nonLiveOrder));
			}

			base.SetValuesBasedOnNonLiveOrder(nonLiveOrder);

            ProductionDepartmentName = projectOrder.ProductionDepartmentName;
            ImportDepartment = projectOrder.ImportDepartment;
            ProductionNumber = projectOrder.ProductionNumber;

            OrderName = projectOrder.OrderDescription;
            DeleteDate = projectOrder.IsLongerStoredBackUpChecked ? projectOrder.BackupDeletionDate : projectOrder.OriginalDeleteDate;
            DeleteComment = projectOrder.WhyMustBackUpBeStoredLonger;
            ProjectName = projectOrder.ProjectName;

			DeliveryDate = projectOrder.MaterialDeliveryTime;
			DateOfCompletion = projectOrder.State == State.Completed && DateOfCompletion == default ? DateTime.Now : default;

			IsilonBackupFileLocation = projectOrder.IsilonBackupFileLocation;
        }

        public override void AddOrUpdate(Helpers helpers)
        {
            helpers.LogMethodStart(nameof(NonIplayProjectUserTask), nameof(AddOrUpdate), out var stopwatch);

            SetTicketFields(helpers);

            ticket.CustomTicketFields[TicketFieldProductionDepartmentName] = ProductionDepartmentName;
            ticket.CustomTicketFields[TicketFieldProductionNumber] = ProductionNumber;
            ticket.CustomTicketFields[TicketFieldImportDepartment] = ImportDepartment;
            ticket.CustomTicketFields[TicketFieldProjectName] = ProjectName;
            ticket.CustomTicketFields[TicketFieldOriginalDeleteDate] = OriginalDeleteDate;
			ticket.CustomTicketFields[TicketFieldIsilonBackupFileLocation] = IsilonBackupFileLocation;

			helpers.NonLiveUserTaskManager.TicketingManager.AddOrUpdateTicket(ticket, out var ticketId);

			ID = ticketId;

            helpers.LogMethodCompleted(nameof(NonIplayProjectUserTask), nameof(AddOrUpdate), null, stopwatch);
        }

        public override void SendReminderMail()
        {
            // TODO: Must be commented out temporarily related to task DCP: 208438
            //NotificationManager.SendNonLiveProjectUserTaskIsilonBackupDeletionMail(helpers, this);
        }
    }
}
