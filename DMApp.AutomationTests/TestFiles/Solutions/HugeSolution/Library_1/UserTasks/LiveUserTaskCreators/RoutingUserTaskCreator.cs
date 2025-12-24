namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class RoutingUserTaskCreator : LiveUserTaskCreator
	{
		public RoutingUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.Routing.MakeConnection, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Routing.MakeConnection, DetermineUserTaskUserGroup() ) } };

			userTaskConditions = new Dictionary<string, bool> { { Descriptions.Routing.MakeConnection, true } };
		}

		private UserGroup DetermineUserTaskUserGroup()
        {
			var userGroup = orderType == OrderType.Video ? UserGroup.McrOperator : UserGroup.AudioMcrOperator;

			var firstResource = service.Functions[0].Resource;
			string firstResourceName = firstResource?.Name;

			var lastResource = service.Functions.Last().Resource;
			string lastResourceName = lastResource?.Name;

			bool isNewsRouting = (firstResourceName != null && firstResourceName.IndexOf("NMX", StringComparison.InvariantCultureIgnoreCase) != -1) || (lastResourceName != null && lastResourceName.IndexOf("NMX", StringComparison.InvariantCultureIgnoreCase) != -1);

			if (isNewsRouting)
            {
				userGroup = UserGroup.MediaOperator;
            }

			return userGroup;
        }
	}
}