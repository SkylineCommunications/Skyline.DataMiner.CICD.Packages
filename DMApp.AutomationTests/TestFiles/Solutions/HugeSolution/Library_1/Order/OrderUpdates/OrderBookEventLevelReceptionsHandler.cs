namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class OrderBookEventLevelReceptionsHandler : OrderUpdateHandler
	{
		public OrderBookEventLevelReceptionsHandler(Helpers helpers, Order order) : base(helpers, order, new OrderUpdateHandlerInput())
		{
		}

		protected override void CollectActionsToExecute()
		{
			actionsToExecute.Add(GetExistingOrderAndEvent);
			actionsToExecute.Add(AddOrUpdateEventLevelReceptions);
			actionsToExecute.Add(AddOrUpdateOrderReservation);
			actionsToExecute.Add(PromoteReceptionToSharedSource);
			actionsToExecute.Add(AddOrUpdateUserTasks);
			actionsToExecute.Add(UpdateServiceUiProperties);
		}
	}
}
