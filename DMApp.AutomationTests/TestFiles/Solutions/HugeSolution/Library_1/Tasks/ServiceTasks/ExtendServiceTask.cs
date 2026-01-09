using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;
	using Service;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;

	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class ExtendServiceTask : Task
	{
		private readonly Service service;

		private readonly Service oldService;

		public ExtendServiceTask(Helpers helpers, Service service)
			: base(helpers)
		{
			this.service = service;
			this.oldService = base.helpers.ServiceManager.GetService(service.Id);
			IsBlocking = true;
		}

		public override string Description => "Extending Service " + service.Name;

		public override Task CreateRollbackTask()
		{
			return new ChangeServiceTimeTask(helpers, oldService);
		}

		protected override void InternalExecute()
		{
			var timeToAdd = service.End - oldService.End;
			if(timeToAdd <= TimeSpan.Zero)
			{
				Log( nameof(InternalExecute), "Unable to extend service with negative or zero timeToAdd: " + timeToAdd);
				return;
			}

			if (!helpers.ServiceManager.TryExtendService(service, timeToAdd)) throw new ChangeTimingFailedException(service.Name);
		}
	}
}
