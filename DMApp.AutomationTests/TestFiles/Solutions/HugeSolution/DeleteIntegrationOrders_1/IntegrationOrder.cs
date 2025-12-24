namespace DeleteIntegrationOrders_1
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public class IntegrationOrder : ISrmObject
	{
		public string Name { get; set; }

		public Guid ID { get; set; }

		public IntegrationType Type { get; set; }

		public ServiceReservationInstance Reservation { get; set; }
	}
}