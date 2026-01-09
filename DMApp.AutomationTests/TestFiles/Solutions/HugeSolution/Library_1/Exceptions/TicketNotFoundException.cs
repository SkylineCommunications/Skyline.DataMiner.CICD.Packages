namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class TicketNotFoundException : MediaServicesException
	{
		public TicketNotFoundException()
		{
		}

		public TicketNotFoundException(int dataminerId, int ticketId)
			: base($"Unable to find Ticket with ID {dataminerId}/{ticketId}")
		{
		}

		public TicketNotFoundException(string fullTicketId)
			: base($"Unable to find Ticket with ID {fullTicketId}")
		{
		}

        public TicketNotFoundException(Guid uniqueId)
            : base($"Unable to find Ticket with unique ID {uniqueId}")
        {
        }

        public TicketNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}