namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System.Linq;
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Service.Service;

	public class GenerateRoutingServicesTask : Task
	{
		private readonly Order order;

		public GenerateRoutingServicesTask(Helpers helpers, Order order)
			: base(helpers)
		{
			IsBlocking = true;
			this.order = order;
		}

		public List<Service> RemovedRoutingServices { get; private set; } = new List<Service>();

		public override string Description => "Generate Routing Services";

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			var liveVideoOrder = new LiveVideoOrder(helpers, order);
			liveVideoOrder.AddOrUpdateRoutingConfiguration(out var removedRoutingServices);
			RemovedRoutingServices = removedRoutingServices;
		}
	}
}