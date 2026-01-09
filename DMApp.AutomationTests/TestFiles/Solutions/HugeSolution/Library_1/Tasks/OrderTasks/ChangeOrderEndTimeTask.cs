using System;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ChangeOrderEndTimeTask : Task
	{
		private readonly Order order;

		private readonly Order oldOrder;

		public ChangeOrderEndTimeTask(Helpers helpers, Order order, Order oldOrder = null)
			: base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.oldOrder = oldOrder ?? base.helpers.OrderManager.GetOrder(order.Id);
			IsBlocking = true;
		}

		public override string Description => $"Changing end timing for Order {order.Name} from {oldOrder.End.ToFullDetailString()} to {order.End.ToFullDetailString()}";

		public override Task CreateRollbackTask()
		{
			return new ChangeOrderEndTimeTask(helpers, oldOrder, oldOrder);
		}

		protected override void InternalExecute()
		{
			if (!order.TryChangeOrderEndTime(helpers)) throw new ChangeTimingFailedException(order.Name);
        }
	}
}