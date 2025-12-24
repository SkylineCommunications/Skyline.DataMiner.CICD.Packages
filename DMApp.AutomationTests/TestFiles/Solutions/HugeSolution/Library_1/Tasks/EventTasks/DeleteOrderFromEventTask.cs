using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks
{
	using System;

	using Skyline.DataMiner.Automation;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class DeleteOrderFromEventTask : Task
	{
		private readonly Guid orderId;

		private readonly Guid eventId;

		public DeleteOrderFromEventTask(Helpers helpers, Guid eventId, Guid orderId)
			: base(helpers)
		{
			IsBlocking = false;
			this.orderId = orderId;
			this.eventId = eventId;
		}

		public override string Description
		{
			get
			{
				return "Deleting Order from Event";
			}
		}

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			helpers.EventManager.DeleteOrderFromEvent(eventId, orderId);
		}
	}
}