using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Status = Order.Status;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class UpdateOrderStatusTask : Task
	{
		private readonly Order order;
		private readonly Status previousStatus;
		private readonly Status status;

		public UpdateOrderStatusTask(Helpers helpers, Order order, Status status) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.status = status;
			this.previousStatus = order.Status;

			IsBlocking = false;
		}

		public override Task CreateRollbackTask()
		{
			return new UpdateOrderStatusTask(helpers, order, previousStatus);
		}

		protected override void InternalExecute()
		{
			order.UpdateStatus(helpers, status);
		}

		public override string Description => $"Setting Order {order.Name} status to {status.GetDescription()}";
	}
}
