namespace LiveOrderForm_6.LoadLiveOrderFormTasks
{
	using System;
	using System.Collections.Generic;
	using Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class MergeOrdersTask : Task
	{
		private readonly OrderMergingDialog orderMergingDialog;

		public MergeOrdersTask(Helpers helpers, OrderMergingDialog orderMergingDialog) : base(helpers)
		{
			this.orderMergingDialog = orderMergingDialog ?? throw new ArgumentNullException(nameof(orderMergingDialog));
		}

		public Order MergedOrder { get; private set; }

		public List<Order> NonPrimaryMergingOrders { get; private set; }

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			MergedOrder = orderMergingDialog.GetMergedOrder();

			NonPrimaryMergingOrders = orderMergingDialog.GetNonPrimaryMergingOrders();
		}

		public override string Description => "Merging Orders";
	}
}