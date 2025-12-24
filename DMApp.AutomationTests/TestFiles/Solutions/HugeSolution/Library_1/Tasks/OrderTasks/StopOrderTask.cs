namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks
{
    using System;
    using System.Linq;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Service = Service.Service;
    using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
    using DateTimeExtensions = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.DateTimeExtensions;

    public class StopOrderTask : Task
    {
        private readonly Order order;

        public StopOrderTask(Helpers helpers, Order order)
            : base(helpers)
        {
            this.order = order ?? throw new ArgumentNullException(nameof(order));
            IsBlocking = true;
        }

        public override string Description => "Stopping order " + order.Name;

        public override Task CreateRollbackTask()
        {
            return null;
        }

        protected override void InternalExecute()
        {
			order.StopNow = true;
            order.StopOrderNow(helpers);

            var nonSharedSourceServices = order.AllServices.Where(s => !s.IsSharedSource || (s.OrderReferences.Count == 1 && s.OrderReferences.First() == order.Id));
			foreach (var service in nonSharedSourceServices)
            {
				if (service.IsBooked) 
				{
					StopBookedService(service);
				} 
				else
				{ 
					StopSavedService(service);
				}
			}

            // Updating the service configuration of the order to have service times in sync again.
            order.UpdateServiceConfigurationProperty(helpers);
        }

		private void StopBookedService(Service service)
		{
			service.StopNow = true;
			service.TryStopServiceNow(helpers);
			service.TryUpdateStatus(helpers, order, handleServiceAction: null, updateOrderStatus: false); // Service goes to post roll, completed or file processing.
		}

		private void StopSavedService(Service service)
		{
			service.StopNow = true;
			service.End = DateTimeExtensions.RoundToMinutes(DateTime.Now);
		}
    }
}
