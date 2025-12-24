namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
    using System;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

    public class StartOrderTask : Task
    {
        private readonly Order order;
        private readonly Order oldOrder;

        public StartOrderTask(Helpers helpers, Order order) : base(helpers)
        {
            this.order = order ?? throw new ArgumentNullException(nameof(order));
            oldOrder = helpers.OrderManager.GetOrder(order.Id);
            IsBlocking = true;
        }

        public override string Description => $"Starting order {order.Name} now";

        public override Task CreateRollbackTask()
        {
            return new ChangeOrderTimeTask(helpers, oldOrder, oldOrder);
        }

        protected override void InternalExecute()
        {
            foreach(var service in order.AllServices)
            {
                if (service.IsOrShouldBeRunning) continue;

                helpers.Log(nameof(StartOrderTask), nameof(InternalExecute), $"Starting Service {service.Name}");
                service.StartNow(helpers);
            }

            helpers.Log(nameof(StartOrderTask), nameof(InternalExecute), $"Starting Order {order.Name}");
            helpers.OrderManager.StartOrderNow(helpers, order, false);
        }
    }
}
