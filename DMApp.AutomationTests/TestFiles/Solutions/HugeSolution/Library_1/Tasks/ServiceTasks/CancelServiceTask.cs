using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class CancelServiceTask : Task
	{
		private readonly Service service;

		private readonly Order order;

		public CancelServiceTask(Helpers helpers, Service service, Order order)
			: base(helpers)
		{
			this.service = service;
			this.order = order;
			IsBlocking = false;
		}

		public CancelServiceTask(Helpers helpers, Guid serviceId, Order order)
			: base(helpers)
		{
			this.service = base.helpers.ServiceManager.GetService(serviceId);
			this.order = order;
			IsBlocking = false;
		}

		public override string Description => "Canceling Service " + service.Name;

		public override Task CreateRollbackTask()
		{
			return new AddOrUpdateServiceTask(helpers, service, service, order);
		}

		protected override void InternalExecute()
		{
			helpers.ServiceManager.CancelService(service.Id, order.Id);
		}
	}
}