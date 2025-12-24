using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class UpdateCustomPropertiesTask : Task
	{
		private readonly Service oldService;

		private readonly Service service;

		private readonly Order order;

		public UpdateCustomPropertiesTask(Helpers helpers, Service service, Service oldService, Order order)
			: base(helpers)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.oldService = oldService ?? throw new ArgumentNullException(nameof(oldService));
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			IsBlocking = false;
		}

		public override string Description => "Updating custom properties for service " + service.Name;

		public override Task CreateRollbackTask()
		{
			return new UpdateCustomPropertiesTask(helpers, oldService, oldService, order);
		}

		protected override void InternalExecute()
		{
			if (!helpers.ServiceManager.TryUpdateAllCustomProperties(service, order))
				throw new CustomPropertyUpdateFailedException(service.Name);
		}
	}
}