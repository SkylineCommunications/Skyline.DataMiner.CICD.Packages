using System;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class GetLiteOrderTask : Task
	{
		private readonly Guid orderId;

		public GetLiteOrderTask(Helpers helpers, Guid orderId) : base(helpers)
		{
			this.orderId = orderId;
		}

		public Order.LiteOrder LiteOrder { get; private set; }

		protected override void InternalExecute()
		{
			LiteOrder = helpers.OrderManager.GetLiteOrder(orderId);
		}
		public override Task CreateRollbackTask()
		{
			return null;
		}

		public override string Description => "Getting Order " + LiteOrder?.Name;
	}
}
