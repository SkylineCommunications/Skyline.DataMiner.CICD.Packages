using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ExtendOrderTask : Task
	{
		private readonly Order order;

		private readonly Order oldOrder;

		public ExtendOrderTask(Helpers helpers, Order order)
			: base(helpers)
		{
			this.order = order;
			this.oldOrder = base.helpers.OrderManager.GetOrder(order.Id);
			IsBlocking = true;
		}

		public override string Description => "Extending Order " + order.Name;

		public override Task CreateRollbackTask()
		{
			return new ChangeOrderTimeTask(helpers, oldOrder);
		}

		protected override void InternalExecute()
		{
			TimeSpan timeToAdd = order.End - oldOrder.End;
			if(timeToAdd <= TimeSpan.Zero)
			{
				Log(nameof(InternalExecute), "Unable to extend order with negative or zero timeToAdd: " + timeToAdd);
				return;
			}

			if (!helpers.OrderManager.TryExtendOrder(order, timeToAdd)) throw new ChangeTimingFailedException(order.Name);
		}
	}
}
