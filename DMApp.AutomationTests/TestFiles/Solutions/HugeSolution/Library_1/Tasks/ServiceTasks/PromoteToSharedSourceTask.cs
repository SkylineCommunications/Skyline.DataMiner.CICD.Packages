namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using Service = Service.Service;
	using Helpers = Utilities.Helpers;

	public class PromoteToSharedSourceTask : Task
	{
		private readonly Service service;

		public PromoteToSharedSourceTask(Helpers helpers, Service service) : base(helpers)
		{
			this.service = service;
			IsBlocking = false;
		}

		public override string Description => $"Promoting service {service.Name} to Shared Source";

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
            if (service.IsSharedSource) helpers.ServiceManager.PromoteToSharedSource(service);
		}
	}
}