namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class VizremFarmUserTaskCreator : LiveUserTaskCreator
	{
		public VizremFarmUserTaskCreator(Helpers helpers, Service.Service service, Guid ticketFieldResolverId, Order.Order order) : base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.VizremFarm.ConfigureRemoteEngine, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.VizremFarm.ConfigureRemoteEngine, UserGroup.Tom) } };

			userTaskConditions = new Dictionary<string, bool> { { Descriptions.VizremFarm.ConfigureRemoteEngine, true } };
		}
	}
}
