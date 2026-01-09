namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	/// <summary>
	/// Represents a service of type Reception.
	/// </summary>
	public class SourceService : ProcessingRelatedService
	{
		public SourceService(Helpers helpers, Service service, LiveVideoOrder order) : base(helpers, service, order)
		{
		}
	}
}