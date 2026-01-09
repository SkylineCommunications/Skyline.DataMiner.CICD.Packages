namespace DeleteIntegrationOrders_1
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public class IntegrationEvent : ISrmObject
	{
		public string Name { get; set; }

		public Guid ID { get; set; }

		public IntegrationType Type { get; set; }

		public Job Job { get; set; }
	}
}