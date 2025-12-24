namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class LiveuUserTaskCreator : LiveUserTaskCreator
	{
		public LiveuUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.LiveU.Connect, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.LiveU.Connect, UserGroup.McrOperator) }, };

			userTaskConditions = new Dictionary<string, bool>
			{
				{ Descriptions.LiveU.Connect, true },
			};
		}
	}
}