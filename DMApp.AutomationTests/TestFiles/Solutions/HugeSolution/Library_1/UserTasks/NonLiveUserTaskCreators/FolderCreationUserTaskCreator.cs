namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTaskCreators
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class FolderCreationUserTaskCreator : NonLiveUserTaskCreator
    {
        public FolderCreationUserTaskCreator(Helpers helpers, FolderCreation folderCreationOrder, Guid ticketFieldResolverId) : base(helpers, folderCreationOrder)
        {
            userTaskConstructors = new Dictionary<string, UserTask>();
            userTaskConditions = new Dictionary<string, bool>();

            if (folderCreationOrder.Destination == InterplayPamElements.Vaasa.GetDescription()) return;

            if (folderCreationOrder.NewProgramFolderRequestDetails != null)
            {
                userTaskConstructors.Add(Descriptions.NonLiveFolderCreation.FolderDeletionProgram, new IplayFolderCreationUserTask(helpers, ticketFieldResolverId, folderCreationOrder, Descriptions.NonLiveFolderCreation.FolderDeletionProgram));
                userTaskConditions.Add(Descriptions.NonLiveFolderCreation.FolderDeletionProgram, true);
            }

            int episodeNumber = 1;
            foreach (var episodeDetail in folderCreationOrder.NewEpisodeFolderRequestDetails)
            {
                string episodeKey = $"{Descriptions.NonLiveFolderCreation.FolderDeletionEpisode} {episodeNumber}";
                userTaskConstructors.Add(episodeKey, new IplayFolderCreationUserTask(helpers, ticketFieldResolverId, folderCreationOrder, episodeKey, episodeDetail));
                userTaskConditions.Add(episodeKey, true);

                episodeNumber++;
            }
        }
    }
}
