namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTaskCreators
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ProjectUserTaskCreator : NonLiveUserTaskCreator
	{
		public ProjectUserTaskCreator(Helpers helpers, Project projectOrder, Guid ticketFieldResolverId)
			: base(helpers, projectOrder)
		{
			userTaskConstructors = new Dictionary<string, UserTask>();
			userTaskConditions = new Dictionary<string, bool>();

			if (projectOrder.BackupDeletionDate != default)
            {
				userTaskConstructors.Add(Descriptions.NonLiveProject.IsilonBackupDeletion, new NonIplayProjectUserTask(helpers, ticketFieldResolverId, projectOrder, Descriptions.NonLiveProject.IsilonBackupDeletion));

				userTaskConditions.Add(Descriptions.NonLiveProject.IsilonBackupDeletion, true);
            }
		}
	}
}
