using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
    using System;
    using System.Threading;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.Net.ResourceManager.Objects;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

    /// <summary>
    /// This task is used to add or update the reservation for the provided Order.
    /// </summary>
    public class AddOrUpdateOrderTask : Task
    {
        private readonly Order order;

        private readonly Order oldOrder;

        public AddOrUpdateOrderTask(Helpers helpers, Order order, Order oldOrder = null)
            : base(helpers)
        {
            this.order = order;

            this.oldOrder = oldOrder ?? (order.Id == Guid.Empty ? null : base.helpers.OrderManager.GetOrder(order.Id));

            this.IsBlocking = true;
        }

        public ReservationInstance OrderReservationInstance { get; private set; }

        public override string Description => "Booking Order " + order.Name;

        public override Task CreateRollbackTask()
        {
            if (oldOrder == null)
            {
                // Delete Order reservation
                return new DeleteOrderTask(helpers, order);
            }
            else
            {
                // Set back old values
                return new AddOrUpdateOrderTask(helpers, oldOrder);
            }
        }

        protected override void InternalExecute()
        {
            // Retry mechanism to work around possible syncing issues
            int count = 0;
            Exception exception;
            do
            {
                try
                {
                    OrderReservationInstance = helpers.OrderManager.AddOrUpdateOrderReservation(order);
                    exception = null;
                }
                catch (Exception e)
                {
                    helpers.Log(nameof(AddOrUpdateOrderTask), nameof(InternalExecute), $"Failed to Add or Update Order Reservation|Count: {count}|Exception: {e}");
                    exception = e;
                    Thread.Sleep(500);
                }

                count++;
            }
            while (count < 3 && exception != null);

            if (exception != null) throw exception;

			helpers.OrderManager.UpdateExistingServicesSecurityViewIds(order, oldOrder);
            order.Id = OrderReservationInstance.ID;

			if (order.ShouldBeRunning) helpers.OrderManager.UpdateServiceOrderIds(order); // Running edit: Still needed as some of the already running services are nearly untouched during book services flow.
		}
	}
}