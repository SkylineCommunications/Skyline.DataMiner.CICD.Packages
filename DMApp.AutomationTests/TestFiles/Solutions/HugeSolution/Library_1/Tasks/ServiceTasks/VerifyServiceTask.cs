using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class VerifyServiceTask : Task
	{
		private readonly Service service;

		private readonly Order orderContainingService;

		public VerifyServiceTask(Helpers helpers, Service service, Order orderContainingService)
			: base(helpers)
		{
			this.service = service;
			this.orderContainingService = orderContainingService;
			IsBlocking = false;
		}

		public override string Description => $"Verifying Service {service.Name}";

		public override Task CreateRollbackTask()
		{
			// rollback tasks are not needed as the rollback tasks from the AddOrUpdateServiceTask will be used to updated the service correctly in case of a rollback
			return null;
		}

		protected override void InternalExecute()
		{
			service.Verify(helpers, orderContainingService);
		}
	}
}