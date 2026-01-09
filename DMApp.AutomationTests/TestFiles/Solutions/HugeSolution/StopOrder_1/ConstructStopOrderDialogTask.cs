namespace StopOrder
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using StopOrder.Dialogs;

	public class ConstructStopOrderDialogTask : Task
	{
		public ConstructStopOrderDialogTask(Helpers helpers) : base(helpers)
		{
		}

		public ConfirmStopDialog ConfirmStopDialog { get; private set; }

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			ConfirmStopDialog = new ConfirmStopDialog(helpers.Engine);
		}

		public override string Description => "Building UI";
	}
}