using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class AddOrUpdateServiceTask : Task
	{
		private readonly Service service;

		private readonly Order order;

		private readonly Service oldService;

		public AddOrUpdateServiceTask(Helpers helpers, Service service, Service oldService, Order order)
			: base(helpers)
		{
			this.service = service;
			this.oldService = oldService;
			this.order = order;
			IsBlocking = false;
		}

		public override string Description => "Add Or Update Service " + service.Name;

		public override Task CreateRollbackTask()
		{
			if (oldService == null)
			{
				// Delete Service
				return new DeleteServiceTask(helpers, service, order);
			}
			else
			{
				// Set back previous values
				return new AddOrUpdateServiceTask(helpers, oldService, oldService, order);
			}
		}

		protected override void InternalExecute()
		{
			helpers.ServiceManager.AddOrUpdateServiceReservation(service, order);
		}
	}
}