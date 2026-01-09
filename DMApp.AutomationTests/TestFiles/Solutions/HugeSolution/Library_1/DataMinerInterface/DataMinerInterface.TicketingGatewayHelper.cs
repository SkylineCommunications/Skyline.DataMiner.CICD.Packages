namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using System.Collections.Generic;
	using System.Reflection;
	using Net.Messages.SLDataGateway;
	using Net.Ticketing;
	using Utilities;

	public static partial class DataMinerInterface
	{
		public static class TicketingGatewayHelper
		{
			[WrappedMethod("TicketingGatewayHelper", "GetTickets")]
			public static IEnumerable<Ticket> GetTickets(Helpers helpers, Net.Ticketing.TicketingGatewayHelper ticketingHelper, FilterElement<Ticket> filter)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var tickets = ticketingHelper.GetTickets(filter: filter);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return tickets;
			}

			[WrappedMethod("TicketingGatewayHelper", "SetTicket")]
			public static bool SetTicket(Helpers helpers, Net.Ticketing.TicketingGatewayHelper ticketingHelper, out string error, ref Ticket ticket)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				bool successful = ticketingHelper.SetTicket(out error, ref ticket);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return successful;
			}

			[WrappedMethod("TicketingGatewayHelper", "RemoveTickets")]
			public static bool RemoveTickets(Helpers helpers, Net.Ticketing.TicketingGatewayHelper ticketingHelper, out string error, params Ticket[] tickets)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				bool successful = ticketingHelper.RemoveTickets(out error, tickets);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return successful;
			}
		}
	}
}
