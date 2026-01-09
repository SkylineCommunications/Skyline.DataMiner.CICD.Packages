namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks
{
	using System;
	using Library_1.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;

	public class IplayFolderCreationUserTask : NonLiveUserTask
    {
        private const string TicketFieldProgramName = "Program Name";
        private const string TicketFieldProducerEmail = "Producer Email";
        private const string TicketFieldMediaManagerEmail = "Media Manager Email";
        private const string TicketFieldEpisodeNumberOrName = "Episode Number or Name";
        private const string TicketFieldProductOrProductionNumber = "Product or Production Number";
        private const string TicketFieldDestination = "Destination";

        public IplayFolderCreationUserTask(Helpers helpers, Ticket ticket) : base(helpers, ticket)
        {
            Destination = ticket.CustomTicketFields.TryGetValue(TicketFieldDestination, out var destination) ? Convert.ToString(destination) : String.Empty;
            ProducerEmail = ticket.CustomTicketFields.TryGetValue(TicketFieldProducerEmail, out var producerEmail) ? Convert.ToString(producerEmail) : String.Empty;
            MediaManagerEmail = ticket.CustomTicketFields.TryGetValue(TicketFieldMediaManagerEmail, out var mediaManagerEmail) ? Convert.ToString(mediaManagerEmail) : String.Empty;

            if (Description.Contains(Descriptions.NonLiveFolderCreation.FolderDeletionEpisode))
            {
                ProductOrProductionNumber = ticket.CustomTicketFields.TryGetValue(TicketFieldProductOrProductionNumber, out var productOrProductionNumber) ? Convert.ToString(productOrProductionNumber) : String.Empty;
                EpisodeNumberOrName = ticket.CustomTicketFields.TryGetValue(TicketFieldEpisodeNumberOrName, out var episodeNumberOrName) ? Convert.ToString(episodeNumberOrName) : String.Empty;
            }
            else
            {
                ProgramName = ticket.CustomTicketFields.TryGetValue(TicketFieldProgramName, out var programName) ? Convert.ToString(programName) : String.Empty;
            }

            OriginalDeleteDate = ticket.CustomTicketFields.TryGetValue(TicketFieldOriginalDeleteDate, out var deleteDate) ? (DateTime)deleteDate : DeleteDate;
        }

        public IplayFolderCreationUserTask(Helpers helpers, Guid ticketFieldResolverId, FolderCreation folderCreation, string description, NewEpisodeFolderRequestDetails linkedEpisodeFolderRequestDetails = null) : base(helpers, ticketFieldResolverId, folderCreation, description)
        {
            SetValuesBasedOnNonLiveOrder(folderCreation);

            if (linkedEpisodeFolderRequestDetails != null)
            {
                EpisodeNumberOrName = linkedEpisodeFolderRequestDetails.EpisodeNumberOrName;
                ProducerEmail = linkedEpisodeFolderRequestDetails.ProducerEmail;
                MediaManagerEmail = linkedEpisodeFolderRequestDetails.MediaManagerEmail;
                ProductOrProductionNumber = linkedEpisodeFolderRequestDetails.ProductOrProductionName;
                DeleteDate = linkedEpisodeFolderRequestDetails.DeleteDate;
            }

            OriginalDeleteDate = folderCreation.OriginalDeleteDate;

            Status = UserTaskStatus.Pending;
        }

		private IplayFolderCreationUserTask(IplayFolderCreationUserTask other)
		{
			CloneHelper.CloneProperties(other, this);
		}

        public string Destination { get; set; }

        public string EpisodeNumberOrName { get; set; }

        public string ProgramName { get; set; }

        public string ProducerEmail { get;  set; }

        public string MediaManagerEmail { get; set; }

        public string ProductOrProductionNumber { get; set; }

		public override IngestExport.Type LinkedOrderType => IngestExport.Type.IplayFolderCreation;

		public override object Clone()
		{
			return new IplayFolderCreationUserTask(this);
		}

		public override void SetValuesBasedOnNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
			if (!(nonLiveOrder is FolderCreation folderCreation))
			{
                throw new ArgumentException($"Argument is not of type {nameof(FolderCreation)}", nameof(nonLiveOrder));
			}

			base.SetValuesBasedOnNonLiveOrder(nonLiveOrder);

            Destination = folderCreation.Destination;
            OrderName = folderCreation.OrderDescription;
            FolderPath = folderCreation.ParentFolder;

            if (Description.Contains(Descriptions.NonLiveFolderCreation.FolderDeletionProgram))
            {
                ProgramName = folderCreation.NewProgramFolderRequestDetails.ProgramName;
                ProducerEmail = folderCreation.NewProgramFolderRequestDetails.ProducerEmail;
                MediaManagerEmail = folderCreation.NewProgramFolderRequestDetails.MediaManagerEmail;
                DeleteDate = folderCreation.NewProgramFolderRequestDetails.DeleteDate;
            }
			else
			{
				DeleteDate = folderCreation.OriginalDeleteDate;
			}
        }

		public override void AddOrUpdate(Helpers helpers)
        {
            helpers.LogMethodStart(nameof(IplayFolderCreationUserTask), nameof(AddOrUpdate), out var stopwatch);

            SetTicketFields(helpers);

            ticket.CustomTicketFields[TicketFieldProgramName] = ProgramName;
            ticket.CustomTicketFields[TicketFieldEpisodeNumberOrName] = EpisodeNumberOrName;
            ticket.CustomTicketFields[TicketFieldProducerEmail] = ProducerEmail;
            ticket.CustomTicketFields[TicketFieldMediaManagerEmail] = MediaManagerEmail;
            ticket.CustomTicketFields[TicketFieldDestination] = Destination;
            ticket.CustomTicketFields[TicketFieldProductOrProductionNumber] = ProductOrProductionNumber;
            ticket.CustomTicketFields[TicketFieldOriginalDeleteDate] = OriginalDeleteDate;

            helpers.NonLiveUserTaskManager.TicketingManager.AddOrUpdateTicket(ticket, out var ticketId);

            ID = ticketId;

            helpers.LogMethodCompleted(nameof(IplayFolderCreationUserTask), nameof(AddOrUpdate), null, stopwatch);
        }

        public override void SendReminderMail()
        {
            NotificationManager.SendNonLiveUsertaskIplayFolderDeletionMail(helpers, this);
        }
    }
}
