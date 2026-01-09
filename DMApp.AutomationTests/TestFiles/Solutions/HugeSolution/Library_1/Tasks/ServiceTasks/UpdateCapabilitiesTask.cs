using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class UpdateCapabilitiesTask : Task
	{
		private readonly Service service;

		public UpdateCapabilitiesTask(Helpers helpers, Service service) : base(helpers)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));

			IsBlocking = false;
		}

		public override string Description => "Update capabilities for service " + service.Name;

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			helpers.ServiceManager.UpdateCapabilities(service);
		}
	}
}
