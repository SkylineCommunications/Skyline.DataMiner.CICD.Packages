namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class MicrowaveTxUserTaskCreator : LiveUserTaskCreator
	{
		public MicrowaveTxUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.MicrowaveTransmission.Configure, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.MicrowaveTransmission.Configure, UserGroup.MwSpecialist) }, };

			userTaskConditions = new Dictionary<string, bool> { { Descriptions.MicrowaveTransmission.Configure, true } };
		}
	}
}