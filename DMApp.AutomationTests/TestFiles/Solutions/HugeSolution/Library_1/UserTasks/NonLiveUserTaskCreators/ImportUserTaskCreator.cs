namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTaskCreators
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ImportUserTaskCreator : NonLiveUserTaskCreator
	{
		public ImportUserTaskCreator(Helpers helpers, Ingest ingestOrder, Guid ticketFieldResolverId)
			: base(helpers, ingestOrder)
		{
			userTaskConstructors = new Dictionary<string, UserTask>();
			userTaskConditions = new Dictionary<string, bool>();

            if (ingestOrder.BackupDeletionDate != default)
            {
				userTaskConstructors.Add(Descriptions.NonLiveImport.IsilonBackupDeletion, new ImportUserTask(helpers, ticketFieldResolverId, ingestOrder, Descriptions.NonLiveImport.IsilonBackupDeletion));

				userTaskConditions.Add(Descriptions.NonLiveImport.IsilonBackupDeletion, true);
            }
		}
	}
}
