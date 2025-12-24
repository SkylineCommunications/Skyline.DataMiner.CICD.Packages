namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Ticketing;
	using Skyline.DataMiner.Net.Ticketing.Helpers;
    using Skyline.DataMiner.Net.Ticketing.Validators;
    using Utilities;

	public class TicketingManager
	{
		private readonly Helpers helpers;

        public static readonly string DeadLineTicketFieldName = "Deadline";

        public static readonly string DeleteDateTicketFieldName = "Delete Date";

		private readonly TicketingGatewayHelper ticketingHelper;

		private readonly TicketFieldResolver ticketFieldResolver;

		public TicketingManager(Helpers helpers, string domain)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));

			ticketingHelper = new TicketingGatewayHelper { HandleEventsAsync = false };
			ticketingHelper.RequestResponseEvent += (sender, args) => args.responseMessage = Engine.SLNet.SendSingleResponseMessage(args.requestMessage);

			ticketFieldResolver = ticketingHelper.GetTicketFieldResolvers(TicketFieldResolver.Factory.CreateEmptyResolver(domain)).FirstOrDefault();
		}

		public TicketFieldResolver TicketFieldResolver => ticketFieldResolver;

		public IEnumerable<Ticket> AllTicketsOlderThan(TimeSpan time)
		{
            DateTime twoYearsAgo = DateTime.Now.Add(-time);
            return DataMinerInterface.TicketingGatewayHelper.GetTickets(helpers, ticketingHelper, TicketingExposers.CreationDate.LessThan(twoYearsAgo));
		}

        public static List<TicketID> ParseTicketIdsFromScriptInput(string scriptParameterValue)
		{
			bool isScriptParamaterInArrayFormat = scriptParameterValue.Contains("[");
			var fullTicketIds = isScriptParamaterInArrayFormat ? JsonConvert.DeserializeObject<string[]>(scriptParameterValue) : new[] { scriptParameterValue };

			var ticketIds = new List<TicketID>();

			foreach (var fullTicketId in fullTicketIds)
			{
				var splitTicketId = fullTicketId.Split('/');

				if (splitTicketId.Length != 2) throw new ArgumentException($"Unable to split '{fullTicketId}' into 2 parts.", nameof(scriptParameterValue));

				var convertedSplitTicketId = Array.ConvertAll(splitTicketId, Convert.ToInt32);

				ticketIds.Add(new TicketID(convertedSplitTicketId[0], convertedSplitTicketId[1]));
			}

			return ticketIds;
		}

		public Ticket GetTicket(int dataMinerId, int ticketId)
		{
			return DataMinerInterface.TicketingGatewayHelper.GetTickets(helpers, ticketingHelper, new ANDFilterElement<Ticket>(TicketingExposers.DataMinerID.Equal(dataMinerId), TicketingExposers.TicketID.Equal(ticketId))).FirstOrDefault();
		}

        public Ticket GetTicket(Guid uniqueId)
        {
            return DataMinerInterface.TicketingGatewayHelper.GetTickets(helpers, ticketingHelper, new ANDFilterElement<Ticket>(TicketingExposers.UniqueID.Equal(uniqueId))).FirstOrDefault();
        }

        public Ticket GetTicketWithFieldValue(string fieldName, string fieldValue)
        {
            return DataMinerInterface.TicketingGatewayHelper.GetTickets(helpers, ticketingHelper, TicketingExposers.CustomTicketFields.DictStringField(fieldName).Equal(fieldValue)).FirstOrDefault();
        }

        public IEnumerable<Ticket> GetTicketsForService(Guid serviceId)
		{
			// This is the preferred way of retrieving the ticket, but didn't work because of a bug in Software
			// return ticketingHelper.GetTickets(new[] { TicketLink.Create(new ReservationInstanceID(serviceId)) });
			return DataMinerInterface.TicketingGatewayHelper.GetTickets(helpers, ticketingHelper, TicketingExposers.CustomTicketFields.DictStringField(LiveUserTask.TicketFieldServiceId).Equal(serviceId.ToString()));
		}

		public IEnumerable<Ticket> GetUserTaskTicketsForNonLive(string ingestExportForeignKey)
		{
			return DataMinerInterface.TicketingGatewayHelper.GetTickets(helpers, ticketingHelper, TicketingExposers.CustomTicketFields.DictStringField("Ingest Export FK").Equal(ingestExportForeignKey));
		}

        public IEnumerable<Ticket> GetTicketsWithinTimeFrame(DateTime endTime)
        {
            return DataMinerInterface.TicketingGatewayHelper.GetTickets(helpers, ticketingHelper, TicketingExposers.CustomTicketFields.DictField(DeadLineTicketFieldName).UncheckedLessThanOrEqual(endTime));
        }

		public IEnumerable<Ticket> GetFutureTicketsBasedOnDeleteDate(DateTime referenceDateTime, DateTime maxLookup)
		{
			return DataMinerInterface.TicketingGatewayHelper.GetTickets(helpers, ticketingHelper, TicketingExposers.CustomTicketFields.DictField(DeleteDateTicketFieldName).UncheckedGreaterThanOrEqual(referenceDateTime).AND(TicketingExposers.CustomTicketFields.DictField(DeleteDateTicketFieldName).UncheckedLessThanOrEqual(maxLookup)));
		}

		public IEnumerable<Ticket> GetTicketsBasedOnCustomState(State state)
		{
            return DataMinerInterface.TicketingGatewayHelper.GetTickets(helpers, ticketingHelper, TicketingExposers.CustomTicketFields.DictField("State")
			.Equal(new GenericEnumEntry<int> { Name = state.GetDescription(), Value = (int)state }));
        }

		public IEnumerable<Ticket> GetTicketsBasedOnCustomState(UserTaskStatus state)
		{
			return DataMinerInterface.TicketingGatewayHelper.GetTickets(helpers, ticketingHelper, TicketingExposers.CustomTicketFields.DictField("State")
			.Equal(new GenericEnumEntry<int> { Name = state.GetDescription(), Value = (int)state }));
		}

		public bool AddOrUpdateTicket(Ticket ticket, out string ticketId)
		{
            try
			{
				bool success = DataMinerInterface.TicketingGatewayHelper.SetTicket(helpers, ticketingHelper, out string error, ref ticket);

				if (success)
				{
					helpers.Log(nameof(TicketingManager), nameof(AddOrUpdateTicket), $"Successfully created or updated ticket: {ticket.ToJson()}");

					ticketId = ticket.ID.ToString();
				}
				else
				{
					helpers.Log(nameof(TicketingManager), nameof(AddOrUpdateTicket), $"Failed to create or update ticket: {ticket.ToJson()}\n{error}");
					ticketId = null;
				}

				return success;
			}
			catch (Exception e)
			{
				ticketId = null;
                helpers.Log(nameof(TicketingManager), nameof(AddOrUpdateTicket), $"Something went wrong while adding or updating the ticket: " + e);
				return false;
			}
		}

		public bool DeleteTicket(Ticket ticket)
		{
            string error = String.Empty;

            try
			{
				return DataMinerInterface.TicketingGatewayHelper.RemoveTickets(helpers, ticketingHelper, out error, ticket);
			}
			catch (Exception)
			{
                helpers.Log(nameof(TicketingManager), nameof(AddOrUpdateTicket), $"Something went wrong while deleting the ticket: " + error);
                return false;
			}
		}
	}
}