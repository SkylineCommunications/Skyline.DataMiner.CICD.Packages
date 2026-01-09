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
	public class DeleteOrderTask : Task
	{
		private readonly Guid orderId;

		private readonly LiteOrder oldOrder;

		public DeleteOrderTask(Helpers helpers, LiteOrder order)
			: base(helpers)
		{
			this.orderId = order.Id;
			oldOrder = order;
			IsBlocking = true;
		}

		public override string Description => "Deleting Order " + oldOrder.Name;

		/// <summary>
		/// This method should return multiple Tasks as the Delete Order task will remove the order, but also the underlying services.
		/// In order to roll this back, we should first recreate the Services and then add the order.
		/// </summary>
		/// <returns></returns>
		public override Task CreateRollbackTask()
		{
			// return new AddOrUpdateOrderTask(engine, progressReporter, oldOrder);
			return null;
		}

		protected override void InternalExecute()
		{
			helpers.OrderManager.DeleteOrderReservation(orderId);
		}
	}
}