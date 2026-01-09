using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using Service;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;

	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ChangeServiceEndTimeTask : Task
	{
		private readonly Service service;

		private readonly Service oldService;

		public ChangeServiceEndTimeTask(Helpers helpers, Service service)
			: base(helpers)
		{
			this.service = service;
			this.oldService = base.helpers.ServiceManager.GetService(service.Id);
			IsBlocking = true;
		}

		public override string Description => "Changing end time for Service " + service.Name;

		public override Task CreateRollbackTask()
		{
			return new ChangeServiceEndTimeTask(helpers, oldService);
		}

		protected override void InternalExecute()
		{
			if (!service.TryChangeServiceEndTime(helpers)) throw new ChangeTimingFailedException(service.Name);
		}
	}
}