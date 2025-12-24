namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class UpdateShortDescriptionTask : Task
	{
		private readonly Service service;

		private readonly Order order;

		public UpdateShortDescriptionTask(Helpers helpers, Service service, Order order)
			: base(helpers)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			IsBlocking = false;
		}

		public override string Description => "Update Short Description for " + service.Name;

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			helpers.ServiceManager.UpdateShortDescription(service, order);
		}
	}
}