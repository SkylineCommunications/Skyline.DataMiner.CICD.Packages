namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Service = Service.Service;

	public class DeleteFromEvsTask : Task
	{
		private readonly Service service;

		public DeleteFromEvsTask(Helpers helpers, Service service) : base(helpers)
		{
			this.service = service;
			IsBlocking = false;
		}

		public override string Description => $"Removing recording session for {service.Name} from EVS";

		public override Task CreateRollbackTask()
		{
			return new AddOrUpdateInEvsTask(helpers, service);
		}

		protected override void InternalExecute()
		{
			service.DeleteEvsRecordingSession(helpers);
		}
	}
}
