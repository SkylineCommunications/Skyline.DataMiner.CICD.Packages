namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks
{
	using System;
	using Library_1.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;

	public class TransferUserTask : NonLiveUserTask
    {
        private readonly Transfer transferOrder;

        public TransferUserTask(Helpers helpers, Ticket ticket) : base(helpers, ticket)
        {
        }

        public TransferUserTask(Helpers helpers, Guid ticketFieldResolverId, Transfer transferOrder, string description) : base(helpers, ticketFieldResolverId, transferOrder, description)
        {
            this.transferOrder = transferOrder;
            OrdererName = transferOrder.CreatedBy;
            UserGroup = DetermineUserGroup();
        }

		private TransferUserTask(TransferUserTask other)
		{
			CloneHelper.CloneProperties(other, this);
		}

		public override IngestExport.Type LinkedOrderType => IngestExport.Type.IplayWgTransfer;

		public override object Clone()
		{
			return new TransferUserTask(this);
		}

		public override void AddOrUpdate(Helpers helpers)
        {
            helpers.LogMethodStart(nameof(TransferUserTask), nameof(AddOrUpdate), out var stopwatch);

            SetTicketFields(helpers);

			helpers.NonLiveUserTaskManager.TicketingManager.AddOrUpdateTicket(ticket, out var ticketId);

			ID = ticketId;

            helpers.LogMethodCompleted(nameof(TransferUserTask), nameof(AddOrUpdate), null, stopwatch);
        }

        public override void SendReminderMail()
        {
            // Currently no mail needed for transfer user tasks.
        }

        private UserGroup DetermineUserGroup()
        {
            UserGroup userGroupTransfer = UserGroup.MessiSpecific;
            if (transferOrder.Source.Contains("Helsinki")) userGroupTransfer = UserGroup.MessiSpecific;
            else if (transferOrder.Source.Contains("Tampere")) userGroupTransfer = UserGroup.MediaputiikkiSpecific;
            else if (transferOrder.Source.Contains("Vaasa")) userGroupTransfer = UserGroup.MediamyllySpecific;
            else if (transferOrder.Source.Contains("UA")) userGroupTransfer = UserGroup.UaSpecific;

            UserGroup userGroupReception = UserGroup.MessiSpecific;
            if (transferOrder.Destination.Contains("Helsinki")) userGroupReception = UserGroup.MessiSpecific;
            else if (transferOrder.Destination.Contains("Tampere")) userGroupReception = UserGroup.MediaputiikkiSpecific;
            else if (transferOrder.Destination.Contains("Vaasa")) userGroupReception = UserGroup.MediamyllySpecific;
            else if (transferOrder.Destination.Contains("UA")) userGroupReception = UserGroup.UaSpecific;

            return Description == Descriptions.NonLiveTransfer.Transfer ? userGroupTransfer : userGroupReception;
        }
    }
}
