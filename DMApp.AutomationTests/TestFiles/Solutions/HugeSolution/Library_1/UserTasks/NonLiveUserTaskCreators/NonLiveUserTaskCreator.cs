namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTaskCreators
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public abstract class NonLiveUserTaskCreator : UserTaskCreator
	{
		protected readonly NonLiveOrder nonLiveOrder;

		protected NonLiveUserTaskCreator(Helpers helpers, NonLiveOrder nonLiveOrder) : base(helpers)
		{
			this.nonLiveOrder = nonLiveOrder ?? throw new ArgumentNullException(nameof(nonLiveOrder));
		}

		protected override void UpdateUserTaskName(UserTask userTask, string additionalDescription)
		{
			userTask.Name = $"{nonLiveOrder.OrderDescription}: {additionalDescription}";
		}
	}
}
