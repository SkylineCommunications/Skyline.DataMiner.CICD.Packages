namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes
{
	using System;

	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;

	public class Note
	{
        public static readonly string TicketDomainName = "Notes";

        public static readonly string TitleTicketField = "Title";

        public static readonly string StartTicketfield = "Start";

        public static readonly string DueTicketField = "Due";

        public static readonly string DescriptionTicketField = "Description";

        public static readonly string PageTicketField = "Page";

        public static readonly string StateTicketField = "State";

        public static readonly string EurovisionIdTicketField = "EurovisionId";

		public Note()
		{
		}

		public Note(Ticket ticket)
		{
			DataMinerId = ticket.ID.DataMinerID;
			TicketId = ticket.ID.TID;
			Title = Convert.ToString(ticket.CustomTicketFields[TitleTicketField]);
			DueDate = Convert.ToDateTime(ticket.CustomTicketFields[DueTicketField]);
			Description = Convert.ToString(ticket.CustomTicketFields[DescriptionTicketField]);
			Page = EnumExtensions.GetEnumValueFromDescription<Page>(Convert.ToString(ticket.CustomTicketFields[PageTicketField]));

            // these fields are only set for Eurovision notes
            TryUpdateStartDateFromTicket(ticket);
            if (ticket.CustomTicketFields.TryGetValue(EurovisionIdTicketField, out var eurovisionId) && eurovisionId != null) EurovisionId = Convert.ToString(eurovisionId);

            var statusTicketField = ticket.GetTicketField(StateTicketField) as Net.Ticketing.Validators.GenericEnumEntry<int>;
			if (statusTicketField != null) Status = (Status)statusTicketField.Value;
		}

		public int? DataMinerId { get; set; }

		public int? TicketId { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime DueDate { get; set; }

		public Page Page { get; set; }

		public Status Status { get; set; }

        public string EurovisionId { get; set; }

        private void TryUpdateStartDateFromTicket(Ticket ticket)
        {
            try
            {
                StartDate = Convert.ToDateTime(ticket.CustomTicketFields[StartTicketfield]);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}