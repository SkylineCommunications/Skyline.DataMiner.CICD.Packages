using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;

	using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.ResourceManager.Objects;
    using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class DeleteServiceUserTasksTask : Task
	{
		private readonly Service service;
		private readonly Order.Order order;

        public DeleteServiceUserTasksTask(Helpers helpers, Service service, Order.Order order)
			: base(helpers)
		{
			this.service = service;
			this.order = order;
			IsBlocking = false;
		}

        public DeleteServiceUserTasksTask(Helpers helpers, Guid serviceId, Order.Order order)
           : base(helpers)
        {
            this.service = base.helpers.ServiceManager.GetService(serviceId);
            this.order = order;
            IsBlocking = false;
        }

        public override string Description => $"Deleting User Tasks for Service {service.Name}";

		public override Task CreateRollbackTask()
		{
			return new AddOrUpdateServiceUserTasksTask(helpers, service, order);
		}

		protected override void InternalExecute()
		{
			helpers.UserTaskManager.DeleteUserTasks(service, order.Id);
		}
	}
}