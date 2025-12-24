namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Files
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.MethodCalls;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Scripts;
    using System.Collections.Generic;

    public class MetricFileSummary
	{
		public string FileName { get; set; }

		public List<ScriptExecutionMetric> ScriptExecutionMetrics { get; set; } = new List<ScriptExecutionMetric>();	

		public List<ScriptExecutionMetricSummary> ScriptExecutionMetricSummaries { get; set; } = new List<ScriptExecutionMetricSummary>();

		public List<ScriptMetricSummary> ScriptMetricSummaries { get; set; } = new List<ScriptMetricSummary>();

		public List<MethodCallSummary> DataMinerInterfaceMethodCallSummaries { get; set; } = new List<MethodCallSummary>();
	}
}