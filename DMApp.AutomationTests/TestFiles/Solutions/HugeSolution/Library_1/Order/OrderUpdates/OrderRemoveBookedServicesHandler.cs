namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
    using System;
    using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Service = Service.Service;

    public class OrderRemoveBookedServicesHandler : OrderUpdateHandler
	{
		private readonly bool automaticCollectionOfServicesToRemoveRequired;

		public OrderRemoveBookedServicesHandler(Helpers helpers, Order order, List<YLE.Service.Service> manuallyProvidedServicesToRemove = null) : base(helpers, order, new OrderUpdateHandlerInput())
		{
			if (manuallyProvidedServicesToRemove != null)
			{
                Log(nameof(OrderRemoveBookedServicesHandler), $"Manually provided services to remove: {string.Join(", ", manuallyProvidedServicesToRemove.Select(s => $"{s.Name} [{s.Id}]"))}");

                this.automaticCollectionOfServicesToRemoveRequired = false;
				this.servicesToRemove = manuallyProvidedServicesToRemove;
			}
			else
			{
				automaticCollectionOfServicesToRemoveRequired = true;
			}
		}

		protected override void CollectActionsToExecute()
		{
			actionsToExecute.Add(GetExistingOrderAndEvent);

            if (automaticCollectionOfServicesToRemoveRequired)
            {
                // this is only executed if all order services need to be removed
                actionsToExecute.Add(CollectAllPossibleServicesToRemove);
                actionsToExecute.Add(ClearOrderResources);
            }
			//else if (order.ShouldBeRunning)
			//{			
				// TODO: Check if releasing the order resources is still needed here, when removing services of a running order the contributing resource and node will be removed via the RemoveServiceFromRunningOrderTask during order add or update.
				// During this flow Node label and Node Id aren't available on the services to remove as they are already detached from the order.
			
			//	// If order is running -> release services to remove
			//	actionsToExecute.Add(ReleaseOrderResources);
			//}
			else
			{
				// Delete manually provided services to remove
			}
			
			actionsToExecute.Add(DeleteServices);
		}
    }
}
