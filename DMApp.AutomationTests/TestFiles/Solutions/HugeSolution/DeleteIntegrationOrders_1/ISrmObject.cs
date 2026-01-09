namespace DeleteIntegrationOrders_1
{
	using System;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public interface ISrmObject
	{
		string Name { get; set; }

		Guid ID { get; set; }

		IntegrationType Type { get; set; }
	}
}