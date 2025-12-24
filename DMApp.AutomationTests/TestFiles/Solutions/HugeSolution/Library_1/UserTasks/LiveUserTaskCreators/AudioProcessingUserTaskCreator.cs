namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class AudioProcessingUserTaskCreator : LiveUserTaskCreator
	{
		public AudioProcessingUserTaskCreator(Helpers helpers, Service.Service service, Guid ticketFieldResolverId, Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.AudioProcessing.ConfigureEquipment, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.AudioProcessing.ConfigureEquipment, orderType == OrderType.Video ? UserGroup.McrOperator : UserGroup.AudioMcrOperator) } };

			userTaskConditions = new Dictionary<string, bool>
			{
				{ Descriptions.AudioProcessing.ConfigureEquipment, true }
			};
		}
	}
}