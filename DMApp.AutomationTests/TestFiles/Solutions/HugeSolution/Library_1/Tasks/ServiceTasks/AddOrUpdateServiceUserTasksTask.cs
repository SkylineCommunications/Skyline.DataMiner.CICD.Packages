using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class AddOrUpdateServiceUserTasksTask : Task
	{
		private readonly Service service;
		private readonly Order.Order order;

		private readonly RequiredUpdateType requiredUpdateType;

		public AddOrUpdateServiceUserTasksTask(Helpers helpers, Service service, Order.Order order, RequiredUpdateType requiredUpdateType = RequiredUpdateType.None)
			: base(helpers)
		{
			this.service = service;
			this.order = order;
			this.requiredUpdateType = requiredUpdateType;
			IsBlocking = false;
		}

        public override string Description => $"Generating User Tasks for Service {service.Name}";

		public override Task CreateRollbackTask()
		{
			return new DeleteServiceUserTasksTask(helpers, service, order);
		}

		protected override void InternalExecute()
		{
			helpers.UserTaskManager.AddOrUpdateUserTasks(service, order, requiredUpdateType != RequiredUpdateType.None);
		}
	}
}