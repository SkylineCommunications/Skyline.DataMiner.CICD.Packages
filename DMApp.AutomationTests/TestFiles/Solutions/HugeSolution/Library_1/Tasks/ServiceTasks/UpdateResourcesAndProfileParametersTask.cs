using System.Runtime.InteropServices;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class UpdateResourcesAndProfileParametersTask : Task
	{
		private readonly Service service;
		private readonly List<Function> functionsToUpdate;

		public UpdateResourcesAndProfileParametersTask(Helpers helpers, Service service, List<Function> functionsToUpdate) : base(helpers)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.functionsToUpdate = functionsToUpdate ?? throw new ArgumentNullException(nameof(functionsToUpdate));
		}

		public override Task CreateRollbackTask()
		{
			throw new NotImplementedException();
		}

		protected override void InternalExecute()
		{
			if(!helpers.ServiceManager.TryUpdateResourcesAndProfileParameters(service, functionsToUpdate)) throw new ProfileParametersAndResourcesNotUpdatedException(functionsToUpdate);
		}

		public override string Description => "Updating profile parameters and resources";
	}
}
