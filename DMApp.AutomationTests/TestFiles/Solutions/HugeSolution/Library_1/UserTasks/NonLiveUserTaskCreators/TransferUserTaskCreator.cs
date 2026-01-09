namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using TransferUserTask = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks.TransferUserTask;

	public class TransferUserTaskCreator : NonLiveUserTaskCreator
	{
		public TransferUserTaskCreator(Helpers helpers, Transfer transfer, Guid ticketFieldResolverId)
			: base(helpers, transfer)
		{
			userTaskConstructors = new Dictionary<string, UserTask> 
			{
				{ Descriptions.NonLiveTransfer.Transfer, new TransferUserTask(helpers, ticketFieldResolverId, transfer, Descriptions.NonLiveTransfer.Transfer) },
				{ Descriptions.NonLiveTransfer.Reception, new TransferUserTask(helpers, ticketFieldResolverId, transfer, Descriptions.NonLiveTransfer.Reception) }
			};

			userTaskConditions = new Dictionary<string, bool> { { Descriptions.NonLiveTransfer.Transfer, true }, { Descriptions.NonLiveTransfer.Reception, true } };
		}
	}
}