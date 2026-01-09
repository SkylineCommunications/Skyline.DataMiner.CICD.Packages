namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static partial class DataMinerInterface
	{
		public static class Engine
		{
			public static Automation.Element FindElement(Helpers helpers, Automation.IEngine engine, int dataminerId, int elementId)
			{
				using (MetricLogger.StartNew(helpers, nameof(Engine)))
				{
					return engine.FindElement(dataminerId, elementId);
				}
			}
		}
	}
}
