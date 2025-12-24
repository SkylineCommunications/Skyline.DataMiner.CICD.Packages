using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	/// <summary>
	/// Task used to remove the Order reservation from the Order Booking manager
	/// </summary>
	public class CancelOrderTask : Task
	{
		private readonly Order order;

		public CancelOrderTask(Helpers helpers, Order order)
			: base(helpers)
		{
			this.order = order;
			IsBlocking = true;
		}

		public override string Description => "Canceling Order " + order.Name;

		/// <summary>
		/// This method should return multiple Tasks as the Delete Order task will remove the order, but also the underlying services.
		/// In order to roll this back, we should first recreate the Services and then add the order.
		/// </summary>
		/// <returns></returns>
		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			order.UpdateStatus(helpers, YLE.Order.Status.Cancelled);
		}
	}
}