namespace UpdateService_4
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ConstructUpdateServiceDialogTask : Task
	{
		private readonly Guid serviceToUpdateId;
		private readonly Order order;
		private readonly LockInfo lockInfo;
		private readonly UserInfo userInfo;
		private readonly EditOrderFlows flow;

		public ConstructUpdateServiceDialogTask(Helpers helpers, Guid serviceToUpdateId, Order order, LockInfo lockInfo, UserInfo userInfo, EditOrderFlows flow) : base(helpers)
		{
			this.serviceToUpdateId = serviceToUpdateId;
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.lockInfo = lockInfo;
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.flow = flow;
			IsBlocking = true;
		}

		public UpdateServiceDialog UpdateServiceDialog { get; private set; }

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			order.AcceptChanges();
			UpdateServiceDialog = new UpdateServiceDialog(helpers, order, order.Event, userInfo, order.AllServices.Single(s => s.Id == serviceToUpdateId), lockInfo, flow);
		}

		public override string Description => "Building UI";
	}
}