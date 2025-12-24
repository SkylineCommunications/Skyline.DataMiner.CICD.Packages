namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;

	using Service = Service.Service;
	using Helpers = Utilities.Helpers;

	public class ReleaseResourcesTask : Task
	{
		private readonly Service service;
		private readonly List<Function> existingFunctions;

		public ReleaseResourcesTask(Helpers helpers, Service service, List<Function> functionsToRelease)
			: base(helpers)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			existingFunctions = functionsToRelease ?? throw new ArgumentNullException(nameof(functionsToRelease));
			IsBlocking = false;
		}

		public override string Description => $"Releasing resource(s) in service {service.Name} from functions {String.Join(", ", existingFunctions.Select(f => f.Name))}";

		public override Task CreateRollbackTask()
		{
			return new UpdateResourcesTask(helpers, service, existingFunctions);
		}

		protected override void InternalExecute()
		{
			try
			{
				var functionsToRelease = new List<Function>();
				foreach (var function in existingFunctions)
				{
					var functionToRelease = function.Clone() as Function;
					functionToRelease.Resource = null;

					functionsToRelease.Add(functionToRelease);
				}

				helpers.ServiceManager.TryReleaseResources(service, functionsToRelease);
			}
			catch (Exception)
			{
				throw new SetOrSwapResourceFailedException();
			}
		}
	}
}
