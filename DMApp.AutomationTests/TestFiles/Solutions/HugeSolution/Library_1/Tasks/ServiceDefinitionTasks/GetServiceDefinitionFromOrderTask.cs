using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceDefinitionTasks
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.Net.ServiceManager.Objects;

	using Service = Service.Service;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class GetServiceDefinitionFromOrderTask : Task
	{
		private readonly Order order;

		private readonly List<Service> servicesToRemove;

		public GetServiceDefinitionFromOrderTask(Helpers helpers, Order order, List<Service> servicesToRemove)
			: base(helpers)
		{
			this.order = order;
			this.servicesToRemove = servicesToRemove;
			IsBlocking = true;
		}

		public override string Description => "Building service chain for Order " + order.Name;

		public ServiceDefinition Result { get; private set; }

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			Result = helpers.ServiceDefinitionManager.GetServiceDefinitionFromOrder(order, servicesToRemove);
		}
	}
}