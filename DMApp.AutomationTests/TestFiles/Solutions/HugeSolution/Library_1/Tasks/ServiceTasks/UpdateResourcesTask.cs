namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Service = Service.Service;

	public class UpdateResourcesTask : Task
	{
		private readonly Service service;
		private readonly List<Function> functionsToUpdate;

		public UpdateResourcesTask(Helpers helpers, Service service, List<Function> functionsToUpdate)
			: base(helpers)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.functionsToUpdate = functionsToUpdate ?? throw new ArgumentNullException(nameof(functionsToUpdate));
			IsBlocking = false;
		}

		public override string Description => $"Changing resource(s) for service {service.Name} to {string.Join(", ", functionsToUpdate.Select(f => $"{f.Name}={f.ResourceName}"))}  ";

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			try
			{
				helpers.ServiceManager.TryUpdateResources(service, functionsToUpdate);
			}
			catch (Exception)
			{
				throw new SetOrSwapResourceFailedException();
			}
		}
	}
}