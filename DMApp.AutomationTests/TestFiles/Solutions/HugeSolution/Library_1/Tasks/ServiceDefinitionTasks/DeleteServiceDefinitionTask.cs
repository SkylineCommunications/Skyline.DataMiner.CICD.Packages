using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.Net.InterDataMiner;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceDefinitionTasks
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.ServiceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class DeleteServiceDefinitionTask : Task
	{
		private readonly ServiceDefinition serviceDefinition;

		public DeleteServiceDefinitionTask(Helpers helpers, ServiceDefinition serviceDefinition)
			: base(helpers)
		{
			IsBlocking = false;
			this.serviceDefinition = serviceDefinition;
		}

		public DeleteServiceDefinitionTask(Helpers helpers, Guid serviceDefinitionId)
			: base(helpers)
		{
			IsBlocking = false;
			this.serviceDefinition = base.helpers.ServiceDefinitionManager.GetRawServiceDefinition(serviceDefinitionId);
		}

		public override string Description => "Deleting Service Definition " + serviceDefinition.Name;

		public override Task CreateRollbackTask()
		{
			// TODO: Does it make sense to add the Service Definition again after it has been deleted, while the user wants it removed?
			// return new AddOrUpdateServiceDefinitionTask(engine, progressReporter, serviceDefinition);
			return null;
		}

		protected override void InternalExecute()
		{
			helpers.ServiceDefinitionManager.DeleteServiceDefinition(serviceDefinition.ID);
		}
	}
}