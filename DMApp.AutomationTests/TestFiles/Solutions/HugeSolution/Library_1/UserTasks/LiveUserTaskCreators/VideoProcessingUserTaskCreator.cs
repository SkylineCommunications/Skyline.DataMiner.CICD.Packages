namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class VideoProcessingUserTaskCreator : LiveUserTaskCreator
	{
		public VideoProcessingUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.VideoProcessing.EquipmentConfiguration, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.VideoProcessing.EquipmentConfiguration, UserGroup.McrOperator) } };

			userTaskConditions = new Dictionary<string, bool> { { Descriptions.VideoProcessing.EquipmentConfiguration, true } };
		}
	}
}