using System;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class GetServiceTask : Task
	{
		private readonly Guid serviceId;

		public GetServiceTask(Helpers helpers, string serviceId) : base(helpers)
		{
			if (!Guid.TryParse(serviceId, out this.serviceId)) throw new ArgumentException("Parameter does not have a Guid format", nameof(serviceId));
		}

		public Service.Service Service { get; private set; }

		protected override void InternalExecute()
		{
			Service = helpers.ServiceManager.GetService(serviceId);
		}
		public override Task CreateRollbackTask()
		{
			return null;
		}


		public override string Description => "Getting Service " + Service?.Name;
	}
}
