namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class GetOrderTask : Task
	{
		private readonly Guid orderGuid;

		public GetOrderTask(Helpers helpers, string orderGuid) : base(helpers)
		{
			if (!Guid.TryParse(orderGuid, out this.orderGuid)) throw new ArgumentException("Parameter does not have a Guid format", nameof(orderGuid));

			IsBlocking = true;
		}

        public GetOrderTask(Helpers helpers, Guid orderGuid) : this(helpers, orderGuid.ToString())
		{
		}

		public Order.Order Order { get; private set; }

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			Order = helpers.OrderManager.GetOrder(orderGuid, false);
		}

		public override string Description => $"Getting Order {Order?.Name}";
	}
}
