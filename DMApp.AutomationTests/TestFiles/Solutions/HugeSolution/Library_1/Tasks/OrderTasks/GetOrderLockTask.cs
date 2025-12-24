namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class GetOrderLockTask : Task
	{
		private readonly Guid orderId;
		private readonly string orderName;

		public GetOrderLockTask(Helpers helpers, Guid orderId, string orderName = null) : base(helpers)
		{
			this.orderId = orderId;
			this.orderName = orderName;
		}

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			LockInfo = helpers.LockManager.RequestOrderLock(orderId);
		}

		public override string Description => $"Getting lock for Order {orderName}";

		public LockInfo LockInfo { get; private set; }
	}
}
