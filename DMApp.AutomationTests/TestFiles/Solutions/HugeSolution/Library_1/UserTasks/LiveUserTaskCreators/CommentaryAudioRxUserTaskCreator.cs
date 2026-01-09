namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class CommentaryAudioRxUserTaskCreator : LiveUserTaskCreator
	{
		public CommentaryAudioRxUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.CommentaryAudioReception.ConfigureAudioCodec, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.CommentaryAudioReception.ConfigureAudioCodec, orderType == Order.OrderType.Video ? UserGroup.McrOperator : UserGroup.AudioMcrOperator) } };

			userTaskConditions = new Dictionary<string, bool>
			{
				{ Descriptions.CommentaryAudioReception.ConfigureAudioCodec, true }
			};
		}
	}
}