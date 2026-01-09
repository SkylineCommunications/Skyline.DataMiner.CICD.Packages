namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class FilePlayoutUserTaskCreator : LiveUserTaskCreator
	{
		public FilePlayoutUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.FilePlayout.Configure, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.FilePlayout.Configure, UserGroup.MediaOperator) } };

			userTaskConditions = new Dictionary<string, bool>
			{
				{ Descriptions.FilePlayout.Configure, true }
			};
		}
	}
}