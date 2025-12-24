namespace UpdateService_4
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ConstructUseSharedSourceDialogTask : Task
	{
		private readonly Guid serviceToUpdateId;
		private readonly Order order;
		private readonly LockInfo lockInfo;
		private readonly UserInfo userInfo;
		private readonly EditOrderFlows flow;

		public ConstructUseSharedSourceDialogTask(Helpers helpers, Guid serviceToUpdateId, Order order, LockInfo lockInfo, UserInfo userInfo, EditOrderFlows flow) : base(helpers)
		{
			this.serviceToUpdateId = serviceToUpdateId;
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.lockInfo = lockInfo;
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.flow = flow;
			IsBlocking = true;
		}

		public UseSharedSourceDialog UseSharedSourceDialog { get; private set; }

		public override string Description => "Building UI";

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			order.AcceptChanges();
			UseSharedSourceDialog = new UseSharedSourceDialog(helpers, order, order.Event, userInfo, order.AllServices.Single(s => s.Id == serviceToUpdateId), lockInfo, flow);
		}
	}
}
