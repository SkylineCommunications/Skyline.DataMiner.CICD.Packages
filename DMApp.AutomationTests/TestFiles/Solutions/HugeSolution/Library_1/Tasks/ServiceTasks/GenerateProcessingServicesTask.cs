namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks
{
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class GenerateProcessingServicesTask : Task
	{
		private readonly Order order;

		public GenerateProcessingServicesTask(Helpers helpers, Order order)
			: base(helpers)
		{
			IsBlocking = true;
			this.order = order;
		}

		public override string Description => "Generate Processing Services";

		public List<Service> RemovedServices { get; } = new List<Service>();

		public override Task CreateRollbackTask()
		{
			return null;
		}

		protected override void InternalExecute()
		{
			var liveVideoOrder = new LiveVideoOrder(helpers, order);
			liveVideoOrder.AddOrUpdateProcessingServices();
		}
	}
}