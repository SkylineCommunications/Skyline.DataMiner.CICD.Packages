using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceDefinitionTasks
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.ServiceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class AddOrUpdateServiceDefinitionTask : Task
	{
		private readonly ServiceDefinition serviceDefinition;

		private readonly ServiceDefinition oldServiceDefinition;

		public AddOrUpdateServiceDefinitionTask(Helpers helpers, ServiceDefinition serviceDefinition)
			: base(helpers)
		{
			this.serviceDefinition = serviceDefinition;
			oldServiceDefinition = Guid.Empty.Equals(serviceDefinition.ID) ? null : base.helpers.ServiceDefinitionManager.GetRawServiceDefinition(serviceDefinition.ID);
			IsBlocking = true;
		}

		public override string Description => "Updating service chain";

		protected override void InternalExecute()
		{
			ServiceDefinition updatedServiceDefinition = helpers.ServiceDefinitionManager.AddOrUpdateServiceDefinition(serviceDefinition);
			Result = updatedServiceDefinition.ID;
		}

		public Guid Result { get; private set; }

		public override Task CreateRollbackTask()
		{
			if (oldServiceDefinition == null)
			{
				// Delete created Service Definition
				return new DeleteServiceDefinitionTask(helpers, serviceDefinition.ID);
			}
			else
			{
				// Set back old Service Definition
				return new AddOrUpdateServiceDefinitionTask(helpers, oldServiceDefinition);
			}
		}
	}
}