namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class OrderReportErrorHandler : OrderUpdateHandler
	{
		public OrderReportErrorHandler(Helpers helpers, Order order) : base(helpers, order, new OrderUpdateHandlerInput())
		{

		}

		protected override void CollectActionsToExecute()
		{
			actionsToExecute.Add(GetExistingOrderAndEvent);
			actionsToExecute.Add(UpdateCustomProperties);
		}
	}
}
