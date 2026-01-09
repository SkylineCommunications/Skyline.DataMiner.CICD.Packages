namespace LiveOrderForm_6.Sections
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.Net.Messages;

	public class ExtendedServiceWithOccupiedResource
	{
		public Service ExtendedService { get; set; }

		public Resource Resource { get; set; }

		public Function Function { get; set; }

		public Service OccupyingService { get; set; }

		public string OccupyingOrderName { get; set; }
	}
}