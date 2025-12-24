namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Filters
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Net.Time;

	public class MethodCallMetricFilter : MethodCallFilter
	{
		public TimeSpan? MinimumExecutionTime { get; set; }
	}
}
