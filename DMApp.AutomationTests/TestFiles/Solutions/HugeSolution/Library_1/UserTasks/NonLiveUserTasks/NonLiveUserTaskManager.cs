namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTaskCreators;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.Net.Ticketing;
    using Type = IngestExport.Type;

    public class NonLiveUserTaskManager : UserTaskManager
    {
        public NonLiveUserTaskManager(Helpers helpers) : base(helpers)
        {
        }

        public bool TryGetAllPendingUserTasks(out List<NonLiveUserTask> pendingUserTasks)
        {
            try
            {
                var allPendingTickets = TicketingManager.GetTicketsBasedOnCustomState(UserTaskStatus.Pending).ToList();

                pendingUserTasks = CreateNonLiveUserTasks(allPendingTickets);

                return true;
            }
            catch (Exception e)
            {
                helpers.Log(nameof(NonLiveUserTaskManager), nameof(TryGetAllPendingUserTasks), "An exception occurred: " + e);
                pendingUserTasks = null;
                return false;
            }
        }

        public bool TryGetAllFutureUserTasks(DateTime referenceDateTime, DateTime maxLookupTimeFrame, out List<NonLiveUserTask> futureTickets)
        {
            try
            {
                var allFutureTickets = TicketingManager.GetFutureTicketsBasedOnDeleteDate(referenceDateTime, maxLookupTimeFrame).ToList();

                futureTickets = CreateNonLiveUserTasks(allFutureTickets);

                return true;
            }
            catch (Exception e)
            {
                helpers.Log(nameof(NonLiveUserTaskManager), nameof(TryGetAllFutureUserTasks), "An exception occurred: " + e);
                futureTickets = null;
                return false;
            }
        }


        public void AddOrUpdateUserTasks(NonLiveOrder nonLiveOrder)
        {
            UserTaskCreator userTaskCreator;

            switch (nonLiveOrder.OrderType)
            {
                case Type.Import:
                    userTaskCreator = new ImportUserTaskCreator(helpers, (Ingest)nonLiveOrder, TicketingManager.TicketFieldResolver.ID);
                    break;
                case Type.IplayFolderCreation:
                    userTaskCreator = new FolderCreationUserTaskCreator(helpers, (FolderCreation)nonLiveOrder, TicketingManager.TicketFieldResolver.ID);
                    break;
                case Type.IplayWgTransfer:
                    userTaskCreator = new TransferUserTaskCreator(helpers, (Transfer)nonLiveOrder, TicketingManager.TicketFieldResolver.ID);
                    break;
                case Type.NonInterplayProject:
                    userTaskCreator = new ProjectUserTaskCreator(helpers, (Project)nonLiveOrder, TicketingManager.TicketFieldResolver.ID);
                    break;
                default:
                    return;
            }

            var existingUserTasks = GetNonLiveUserTasks(nonLiveOrder);

			foreach (var existingUserTask in existingUserTasks)
			{
                existingUserTask.SetValuesBasedOnNonLiveOrder(nonLiveOrder);
			}

            AddOrUpdateUserTasks(userTaskCreator, existingUserTasks);
        }

        public bool TryGetNonLiveUserTask(string fullTicketId, out NonLiveUserTask nonLiveUserTask)
        {
            try
            {
                nonLiveUserTask = GetNonLiveUserTask(fullTicketId);
                return true;
            }
            catch (Exception e)
            {
                helpers.Log(nameof(NonLiveUserTaskManager), nameof(TryGetNonLiveUserTask), "An exception occurred: " + e);
                nonLiveUserTask = null;
                return false;
            }
        }

        public NonLiveUserTask GetNonLiveUserTask(string fullTicketId)
        {
            string[] splitFullTicketId = fullTicketId.Split('/');
            if (splitFullTicketId.Length != 2)
                throw new ArgumentException("Unable to split in DataMiner ID and Ticket ID", "fullTicketId");
            if (!Int32.TryParse(splitFullTicketId[0], out int dataminerId))
                throw new ArgumentException("Unable to parse DataMiner ID", "fullTicketId");
            if (!Int32.TryParse(splitFullTicketId[1], out int ticketId))
                throw new ArgumentException("Unable to parse Ticket ID", "fullTicketId");

            return GetNonLiveUserTask(dataminerId, ticketId);
        }

		public NonLiveUserTask GetNonLiveUserTask(int dataminerId, int ticketId)
		{
			var ticket = TicketingManager.GetTicket(dataminerId, ticketId) ?? throw new TicketNotFoundException(dataminerId, ticketId);

			return NonLiveUserTask.Factory(helpers, ticket);
		}

        /// <summary>
        /// Get user tasks that are applicable for the given non live order.
        /// </summary>
        /// <returns>Returns all linked user tasks of given non live order.</returns>
        public List<NonLiveUserTask> GetNonLiveUserTasks(NonLiveOrder nonLiveOrder)
        {
            string ingestExportForeignKey = nonLiveOrder.DataMinerId + "/" + nonLiveOrder.TicketId;

            var tickets = TicketingManager.GetUserTaskTicketsForNonLive(ingestExportForeignKey).ToList();

            return CreateNonLiveUserTasks(tickets);
        }

        /// <summary>
        /// Will convert each ticket into an user task based on non live type.
        /// </summary>
        /// <returns>Returns filtered user tasks based on non live type</returns>
        public List<NonLiveUserTask> CreateNonLiveUserTasks(List<Ticket> tickets)
        {
            if (tickets is null) throw new ArgumentNullException(nameof(tickets));

            var userTasks = new List<NonLiveUserTask>();
            foreach (var ticket in tickets)
            {
                var createNonLiveUserTask = NonLiveUserTask.Factory(helpers, ticket);
                if (createNonLiveUserTask != null)
                {
                    userTasks.Add(createNonLiveUserTask);
                }
            }

            return userTasks;
        }
    }
}
