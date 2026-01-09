namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;

	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ConstructNonLiveUserTaskDialogTask : Task
	{
		private readonly NonLiveUserTask nonLiveUserTask;

		public ConstructNonLiveUserTaskDialogTask(Helpers helpers, NonLiveUserTask nonLiveUserTask) : base(helpers)
		{
			this.nonLiveUserTask = nonLiveUserTask;

			IsBlocking = true;
		}

		public NonLiveUserTaskDialog NonLiveUserTaskDialog { get; private set; }

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			NonLiveUserTaskDialog = new NonLiveUserTaskDialog(helpers, nonLiveUserTask);
		}

		public override string Description => "Building UI";
	}
}
