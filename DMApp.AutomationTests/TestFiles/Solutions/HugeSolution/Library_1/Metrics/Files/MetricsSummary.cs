namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Files
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Scripts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.MethodCalls;
    using Skyline.DataMiner.Library;
    using Newtonsoft.Json;

    [Serializable]
    public class MetricsSummary
	{
        //public Dictionary<ScriptExecutionIdentifier, List<ScriptExecutionMetricSummary>> ScriptExecutionMetricSummaries { get; set; }

        public List<ScriptMetricSummary> ScriptMetricSummaries { get; set; }

        public List<MethodCallSummary> MethodCallSummaries { get; set; }

        [JsonIgnore]
        public IEnumerable<MethodCallSummary> DataMinerInterfaceMethodCallSummaries => MethodCallSummaries.Where(m => m.MatchesFilter(MetricAggregator.DataMinerInterfaceFilter));

        public void Merge(MetricsSummary other)
        {
            //var scriptExecutionMetricSummaries = new List<ScriptExecutionMetricSummary>();
            //ScriptExecutionMetricSummaries.Values.ForEach(s => scriptExecutionMetricSummaries.AddRange(s));
            //other.ScriptExecutionMetricSummaries.Values.ForEach(s => scriptExecutionMetricSummaries.AddRange(s));
            //ScriptExecutionMetricSummaries = ScriptExecutionMetricSummary.Combine(scriptExecutionMetricSummaries);

            var scriptMetricSummaries = ScriptMetricSummaries.ToList();
            scriptMetricSummaries.AddRange(other.ScriptMetricSummaries);
            ScriptMetricSummaries = ScriptMetricSummary.Combine(scriptMetricSummaries);

            var methodCallSummaries = MethodCallSummaries.ToList();
            methodCallSummaries.AddRange(other.MethodCallSummaries);
            MethodCallSummaries = MethodCallSummary.Combine(methodCallSummaries);
        }
    }
}