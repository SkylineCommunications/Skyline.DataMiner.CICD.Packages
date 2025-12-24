namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceUpdates
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class ServiceAddOrUpdateHandler : ServiceUpdateHandler
	{
		public ServiceAddOrUpdateHandler(Helpers helpers, Order orderContainingService, Service service, Service existingService = null)
			: base(helpers, orderContainingService, service, existingService)
		{
			
		}

		protected override void CollectTasks()
		{
			Log(nameof(CollectTasks), service.Name + "|Adding or updating service");

			userTasksRequired = false;

			ServiceStatusActions();

            service.OrderReferences.Add(orderContainingService.Id);

            if (!ServiceAddOrUpdateRequired()) return;

            service.OrderName = orderContainingService.Name;

			tasks.AddRange(service.GetUpdateTasks(Helpers, orderContainingService, existingService));

			userTasksRequired = true;
		}
	}
}
