using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class DeleteServiceTask : Task
	{
		private readonly Service service;

		private readonly Order order;

		private readonly ReservationInstance serviceReservationInstance;
		private readonly bool skipCheckingIfServiceIsUsedByOtherOrders;

		public DeleteServiceTask(Helpers helpers, Service service, Order order, ReservationInstance serviceReservationInstance = null, bool skipCheckingIfServiceIsUsedByOtherOrders = false)
			: base(helpers)
		{
			this.service = service;
			this.order = order;
			this.serviceReservationInstance = serviceReservationInstance;
			this.skipCheckingIfServiceIsUsedByOtherOrders = skipCheckingIfServiceIsUsedByOtherOrders;
			IsBlocking = false;
		}

		public DeleteServiceTask(Helpers helpers, Guid serviceId, Order order, ReservationInstance serviceReservationInstance = null, bool skipCheckingIfServiceIsUsedByOtherOrders = false)
			: base(helpers)
		{
			this.service = base.helpers.ServiceManager.GetService(serviceId);
			this.order = order;
			this.serviceReservationInstance = serviceReservationInstance;
			this.skipCheckingIfServiceIsUsedByOtherOrders = skipCheckingIfServiceIsUsedByOtherOrders;
			IsBlocking = false;
		}

		public override string Description => "Deleting Service " + service.Name;

		public override Task CreateRollbackTask()
		{
			return new AddOrUpdateServiceTask(helpers, service, service, order);
		}

		protected override void InternalExecute()
		{

			helpers.ServiceManager.DeleteService(service.Id, order.Id, serviceReservationInstance, skipCheckingIfServiceIsUsedByOtherOrders);
			service.WasRemoved();
		}
	}
}