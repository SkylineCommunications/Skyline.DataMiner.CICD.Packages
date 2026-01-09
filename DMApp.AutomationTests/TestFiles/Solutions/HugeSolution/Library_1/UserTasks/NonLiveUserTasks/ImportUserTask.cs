namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks
{
	using System;
	using Library_1.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;

	public class ImportUserTask : NonLiveUserTask
    {
        private const string TicketFieldImportDestination = "Import Destination";
		private const string TicketFieldIsilonBackupFileLocation = "Isilon Backup File Location";


		public ImportUserTask(Helpers helpers, Ticket ticket) : base(helpers, ticket)
        {
            ImportDestination = ticket.CustomTicketFields.TryGetValue(TicketFieldImportDestination, out var importDestinationValue) ? Convert.ToString(importDestinationValue) : String.Empty;
			IsilonBackupFileLocation = ticket.CustomTicketFields.TryGetValue(TicketFieldIsilonBackupFileLocation, out var isilonBackupFileLocation) ? Convert.ToString(isilonBackupFileLocation) : String.Empty;
            OriginalDeleteDate = ticket.CustomTicketFields.TryGetValue(TicketFieldOriginalDeleteDate, out var deleteDate) ? (DateTime)deleteDate : DeleteDate;
        }

        public ImportUserTask(Helpers helpers, Guid ticketFieldResolverId, Ingest ingestOrder, string description) : base(helpers, ticketFieldResolverId, ingestOrder, description)
        {
            Status = UserTaskStatus.Pending;
            OriginalDeleteDate = ingestOrder.OriginalDeleteDate; //Should only be set on ticket creation and never be updated.

            SetValuesBasedOnNonLiveOrder(ingestOrder);
        }

		private ImportUserTask(ImportUserTask other)
		{
			CloneHelper.CloneProperties(other, this);
		}

        public string ImportDestination { get; set; }

		public string IsilonBackupFileLocation { get; set; }

		public override IngestExport.Type LinkedOrderType => IngestExport.Type.Import;

		public override object Clone()
		{
			return new ImportUserTask(this);
		}

		public override void SetValuesBasedOnNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
			if (!(nonLiveOrder is Ingest ingestOrder))
			{
                throw new ArgumentException($"Argument is not of type {nameof(Ingest)}", nameof(nonLiveOrder));
			}

            base.SetValuesBasedOnNonLiveOrder(nonLiveOrder);

            ImportDestination = ingestOrder.IngestDestination.Destination;

            OrderName = ingestOrder.OrderDescription;

			DeleteDate = (bool)ingestOrder.BackUpsLongerStored ? ingestOrder.BackupDeletionDate : ingestOrder.OriginalDeleteDate;
            DeleteComment = ingestOrder.WhyBackUpLongerStored;
            FolderPath = ingestOrder.IngestDestination.InterplayDestinationFolder;

			DeliveryDate = ingestOrder.DeliveryTime;
			DateOfCompletion = ingestOrder.State == State.Completed && DateOfCompletion == default ? DateTime.Now : default;

			IsilonBackupFileLocation = ingestOrder.IsilonBackupFileLocation;
        }

        public override void AddOrUpdate(Helpers helpers)
        {
            helpers.LogMethodStart(nameof(ImportUserTask), nameof(AddOrUpdate), out var stopwatch);

            SetTicketFields(helpers);

            ticket.CustomTicketFields[TicketFieldImportDestination] = ImportDestination;
            ticket.CustomTicketFields[TicketFieldOriginalDeleteDate] = OriginalDeleteDate;
			ticket.CustomTicketFields[TicketFieldIsilonBackupFileLocation] = IsilonBackupFileLocation;
			helpers.NonLiveUserTaskManager.TicketingManager.AddOrUpdateTicket(ticket, out var ticketId);

			ID = ticketId;

            helpers.LogMethodCompleted(nameof(ImportUserTask), nameof(AddOrUpdate), null, stopwatch);
        }

        public override void SendReminderMail()
        {
            // TODO: Must be commented out temporarily related to task DCP: 208438
            //NotificationManager.SendNonLiveImportUserTaskIsilonBackupDeletionMail(helpers, this);
        }
    }
}
