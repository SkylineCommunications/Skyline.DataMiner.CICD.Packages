namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	/// <summary>
	/// Creates and/or updates User Tasks linked to one specific service type. 
	/// </summary>
	public abstract class UserTaskCreator
	{
		protected readonly Helpers helpers;

		protected Dictionary<string, UserTask> userTaskConstructors;

		protected Dictionary<string, bool> userTaskConditions;

		protected UserTaskCreator(Helpers helpers)
		{
			this.helpers = helpers;
		}

		/// <summary>
		/// Creates the applicable user tasks.
		/// </summary>
		public IEnumerable<UserTask> CreateUserTasks()
		{
			var createdUserTasks = new List<UserTask>();
			var notRequiredUserTasks = new List<string>(); // for logging purposes
			
			foreach (string userTaskDescription in userTaskConstructors.Keys)
			{
				if (userTaskConditions[userTaskDescription])
				{
					createdUserTasks.Add(userTaskConstructors[userTaskDescription]);
				}
				else
				{
					notRequiredUserTasks.Add(userTaskDescription);
				}
			}

			Log(nameof(CreateUserTasks), $"Not required user tasks: {string.Join(";", notRequiredUserTasks)}");
			Log(nameof(CreateUserTasks), $"Created required user task objects: {string.Join(";", createdUserTasks.Select(u => u.Name))}");
			
			return createdUserTasks;
		}

		/// <summary>
		/// Updates the values of the given existing user tasks.
		/// </summary>
		public IEnumerable<UserTask> UpdateUserTasks(IEnumerable<UserTask> existingUserTasks, out IEnumerable<UserTask> userTasksToDelete)
		{
			var userTasksToAddOrUpdate = new List<UserTask>();
			var auxUserTasksToDelete = new List<UserTask>();

			// Collect existing User Tasks
			foreach (var userTask in existingUserTasks)
			{
				if (userTaskConditions.TryGetValue(userTask.Description, out bool isRequired) && isRequired)
				{
					// Change the usertask name in case the name of the service/transfer has changed
					string description = userTask.Name.Substring(userTask.Name.LastIndexOf(':') + 1);
					UpdateUserTaskName(userTask, description);

					userTasksToAddOrUpdate.Add(userTask);

					Log(nameof(UpdateUserTasks), $"Existing user task {userTask.Description} is still required");
				}
				else
				{
					auxUserTasksToDelete.Add(userTask);

					Log(nameof(UpdateUserTasks), $"Existing user task {userTask.Description} is no longer required");

				}
			}

			var existingUserTaskDescriptions = existingUserTasks.Select(u => u.Description).ToList();

			// Create and Add missing User Tasks

			var missingUserTaskDescriptions = userTaskConstructors.Keys.Except(existingUserTaskDescriptions);
			foreach (string userTaskDescription in missingUserTaskDescriptions)
			{
				Log(nameof(UpdateUserTasks), $"New user task {userTaskDescription} is {(userTaskConditions[userTaskDescription] ? string.Empty : "not ")}required");

				if (userTaskConditions[userTaskDescription])
				{
					userTasksToAddOrUpdate.Add(userTaskConstructors[userTaskDescription]);
				}
			}

			userTasksToDelete = auxUserTasksToDelete;
			return userTasksToAddOrUpdate;
		}

		protected abstract void UpdateUserTaskName(UserTask userTask, string additionalDescription);

		protected void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers.Log(GetType().Name, nameOfMethod, message, nameOfObject);
		}
	}
}