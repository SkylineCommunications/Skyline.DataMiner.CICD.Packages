namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Ticketing;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Ticketing;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Service.Service;

	public class UserTaskManager
	{
		protected readonly Helpers helpers;
		public static readonly string TicketingDomain = "User Tasks";

		public UserTaskManager(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			TicketingManager = new TicketingManager(helpers, TicketingDomain);
		}

		public TicketingManager TicketingManager { get; }

        public void AddOrUpdateUserTasks(UserTaskCreator userTaskCreator, IEnumerable<UserTask> existingUserTasks)
        {
            IEnumerable<UserTask> userTasksToAddOrUpdate;
            if (!existingUserTasks.Any())
            {
                userTasksToAddOrUpdate = userTaskCreator.CreateUserTasks();
            }
            else
            {
                userTasksToAddOrUpdate = userTaskCreator.UpdateUserTasks(existingUserTasks, out var userTasksToDelete);
                foreach (var userTask in userTasksToDelete)
                {
                    userTask.Delete(TicketingManager);
                }
            }

            foreach (var userTask in userTasksToAddOrUpdate)
            {
                userTask.AddOrUpdate(helpers);
            }
        }

		/// <summary>
		/// Add or Update user tasks that are applicable for the given live service.
		/// </summary>
		public void AddOrUpdateUserTasks(Service service, Order.Order order, bool reopenExistingTasks = false)
		{
			var existingUserTasks = GetUserTasks(service).ToList();

			Log(nameof(AddOrUpdateUserTasks), $"After getting existing user tasks, service UserTasks property contains: {string.Join(";", service.UserTasks.Select(u => $"{u.Name}({u.ID}) = {u.Status}"))}");

			var userTaskCreator = GetUserTaskCreator(service, order);
			if (userTaskCreator == null) return;

			Log(nameof(AddOrUpdateUserTasks), $"After getting user task creator, service UserTasks property contains: {string.Join(";", service.UserTasks.Select(u => $"{u.Name}({u.ID}) = {u.Status} | Usergroup = {u.UserGroup.GetDescription()}"))}");

			if (service.IsSharedSource && service.IsBooked && existingUserTasks.Any()) return; // User Tasks for Booked Shared Sources don't need changes

			IEnumerable<LiveUserTask> userTasksToAddOrUpdate;
			if (!existingUserTasks.Any())
			{
				userTasksToAddOrUpdate = userTaskCreator.CreateUserTasks().Cast<LiveUserTask>();
			}
			else
			{
				userTasksToAddOrUpdate = userTaskCreator.UpdateUserTasks(existingUserTasks, out var userTasksToDelete).Cast<LiveUserTask>();

				foreach (var userTask in userTasksToDelete)
				{
					userTask.Delete(TicketingManager);
				}

				Log(nameof(AddOrUpdateUserTasks), $"Deleted user tasks '{string.Join(";", userTasksToDelete.Select(u => $"{u.Name}({u.ID})"))}'", service.Name);
			}
			
			service.UserTasks = new List<LiveUserTask>();
			foreach (var userTask in userTasksToAddOrUpdate)
			{
				if (reopenExistingTasks) userTask.Status = UserTaskStatus.Incomplete;
				else if (userTask.CanBeAutoCompleted(helpers, order)) userTask.Status = UserTaskStatus.Complete;
				else
				{
					// nothing
				}

				userTask.AddOrUpdate(helpers);
				service.UserTasks.Add(userTask);
			}

			Log(nameof(AddOrUpdateUserTasks), $"Added or updated user tasks '{string.Join(";", userTasksToAddOrUpdate.Select(u => $"{u.Name}({u.ID})"))}'",service.Name);
		}

		/// <summary>
		/// Delete user tasks that are applicable for the given live service.
		/// </summary>
		public void DeleteUserTasks(Service service, Guid orderId)
		{   
			// Do not remove user tasks if the service is a Shared Source and is used by other orders
			if (service.IsSharedSource && helpers.ServiceManager.ServiceIsUsedByOtherOrders(service.Id, new [] { orderId }))
            {
				Log(nameof(DeleteUserTasks), $"User tasks for service {service.Name} were not removed as it is a Shared Source and used by other orders");
				return;
			}

            IEnumerable<UserTask> existingUserTasks = GetUserTasks(service);

			foreach (UserTask userTask in existingUserTasks)
			{
				userTask.Delete(TicketingManager);
			}

			if (service.UserTasks != null && service.UserTasks.Any())
			{
				service.UserTasks.Clear();
			}
		}

		/// <summary>
		/// Get user tasks that are applicable for the given live service.
		/// </summary>
		public IEnumerable<LiveUserTask> GetUserTasks(Service service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			LogMethodStarted(nameof(GetUserTasks), out var stopwatch, service.Name);

			var tickets = TicketingManager.GetTicketsForService(service.Id);

			var userTasks = tickets.Select(x => new LiveUserTask(helpers, service, x)).ToList();

			Log(nameof(GetUserTasks),$"Retrieved following user tasks from database: {string.Join(";", userTasks.Select(u => $"{u.Name}({u.ID})={u.Status}"))}",service.Name);

			LogMethodCompleted(nameof(GetUserTasks), service.Name, stopwatch);

			return userTasks;
		}

		/// <summary>
		/// Tries to get the user task for the given ID.
		/// </summary>
		public bool TryGetUserTask(string fullTicketId, out UserTask userTask)
		{
			try
			{
				userTask = GetUserTask(fullTicketId);
				return true;
			}
			catch (Exception e)
			{
				Log(nameof(TryGetUserTask), $"Exception occurred: {e}");
				userTask = null;
				return false;
			}
		}

        /// <summary>
		/// Tries to get the user task for the given ID.
		/// </summary>
		public bool TryGetUserTaskBasedOnUniqueId(Guid uniqueId, out UserTask userTask)
        {
            try
            {
                userTask = GetUserTaskBasedOnUniqueId(uniqueId);
                return userTask != null;
            }
            catch (Exception e)
            {
                Log(nameof(TryGetUserTaskBasedOnUniqueId), $"Exception occurred: {e}");
                userTask = null;
                return false;
            }
        }

        public UserTask GetUserTask(string fullTicketId)
		{
			string[] splitFullTicketId = fullTicketId.Split('/');
			if (splitFullTicketId.Length != 2) throw new ArgumentException("Unable to split in DataMiner ID and Ticket ID", nameof(fullTicketId));
			if (!Int32.TryParse(splitFullTicketId[0], out int dataminerId)) throw new ArgumentException("Unable to parse DataMiner ID", nameof(fullTicketId));
			if (!Int32.TryParse(splitFullTicketId[1], out int ticketId)) throw new ArgumentException("Unable to parse Ticket ID", nameof(fullTicketId));

			LogMethodStarted(nameof(GetUserTask), out var stopwatch);

			var ticket = TicketingManager.GetTicket(dataminerId, ticketId) ?? throw new TicketNotFoundException(dataminerId, ticketId);

			var userTask = UserTask.Factory(helpers, ticket);

			LogMethodCompleted(nameof(GetUserTask), null, stopwatch);

			return userTask;
		}

        public UserTask GetUserTaskBasedOnUniqueId(Guid uniqueId)
        {
			LogMethodStarted(nameof(GetUserTaskBasedOnUniqueId), out var stopwatch);

            var ticket = TicketingManager.GetTicket(uniqueId) ?? throw new TicketNotFoundException(uniqueId);
			
			var userTask = UserTask.Factory(helpers, ticket);

			LogMethodCompleted(nameof(GetUserTaskBasedOnUniqueId), null, stopwatch);

			return userTask;
        }

        public UserTaskCreator GetUserTaskCreator(Service service, Order.Order order)
		{
			if (service?.Definition == null) return null;

			helpers.Log(nameof(UserTaskManager), nameof(GetUserTaskCreator),$"Getting usertask creator for virtual platform {service.Definition.VirtualPlatform.GetDescription()}",service.Name);

			switch (service.Definition.VirtualPlatform)
			{
				case VirtualPlatform.ReceptionNone:
				case VirtualPlatform.TransmissionNone:
					return new DummyUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.ReceptionSatellite:
					return new SatelliteRxUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.ReceptionFiber:
					return new FiberRxUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.Routing:
					return new RoutingUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.ReceptionMicrowave:
					return new MicrowaveRxUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.TransmissionIp:
				case VirtualPlatform.ReceptionIp:
					return new IpUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.ReceptionLiveU:
				case VirtualPlatform.TransmissionLiveU:
					return new LiveuUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.TransmissionSatellite:
					return new SatelliteTxUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.VideoProcessing:
					return new VideoProcessingUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.AudioProcessing:
					return new AudioProcessingUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.Recording:
					return new RecordingUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.FilePlayout:
					return new FilePlayoutUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.TransmissionFiber:
					return new FiberTxUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.TransmissionMicrowave:
					return new MicrowaveTxUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.ReceptionCommentaryAudio:
					return new CommentaryAudioRxUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				case VirtualPlatform.VizremFarm:
					return new VizremFarmUserTaskCreator(helpers, service, TicketingManager.TicketFieldResolver.ID, order);

				default:
					return null;
			}
		}

		private void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers.Log(nameof(UserTaskManager), nameOfMethod, message, nameOfObject);
		}

		private void LogMethodStarted(string methodName, out Stopwatch stopwatch, string objectName = null)
		{
			helpers.LogMethodStart(nameof(UserTaskManager), methodName, out stopwatch ,objectName);

		}

		private void LogMethodCompleted(string methodName, string objectName = null, Stopwatch stopwatch = null)
		{
			helpers.LogMethodCompleted(nameof(UserTaskManager), methodName, objectName, stopwatch);
		}
	}
}