namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class FiberTxUserTaskCreator : LiveUserTaskCreator
	{
		public FiberTxUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.FiberTransmission.Configure, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.FiberTransmission.Configure, UserGroup.FiberSpecialist) }, };

			userTaskConditions = new Dictionary<string, bool>
			{
				{ Descriptions.FiberTransmission.Configure, true }
			};
		}
	}
}