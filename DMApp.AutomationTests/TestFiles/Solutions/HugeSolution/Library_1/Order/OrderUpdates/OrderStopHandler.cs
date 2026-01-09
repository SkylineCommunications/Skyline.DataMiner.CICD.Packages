namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;

    public class OrderStopHandler : OrderUpdateHandler
    {
        public OrderStopHandler(Helpers helpers, Order order) : base(helpers, order, new OrderUpdateHandlerInput())
        {
        }

        protected override void CollectActionsToExecute()
        {
            actionsToExecute.Add(TryStopOrderNowIfApplicable);
        }
    }
}
