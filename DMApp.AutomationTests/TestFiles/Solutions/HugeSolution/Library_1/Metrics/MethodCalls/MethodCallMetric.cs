namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.MethodCalls
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Filters;

	public class MethodCallMetric : MethodCallIdentifier
	{
		public DateTime TimeStamp { get; set; }

		public TimeSpan ExecutionTime { get; set; }

		public List<MethodCallMetric> SubMethodCallMetrics { get; set; } = new List<MethodCallMetric>();

		public bool MatchesFilter(MethodCallMetricFilter metricFilter)
		{
			bool matchesFilter = true;

			matchesFilter &= base.MatchesFilter(metricFilter);

			matchesFilter &= metricFilter.TimeRange == null || (metricFilter.TimeRange.Start <= TimeStamp && TimeStamp <= metricFilter.TimeRange.Stop);

			matchesFilter &= metricFilter.MinimumExecutionTime == null || metricFilter.MinimumExecutionTime <= ExecutionTime;

			return matchesFilter;
		}

		public MethodCallMetric FindLongestSubMethodCallMetric()
		{
			return SubMethodCallMetrics.OrderByDescending(m => m.ExecutionTime).FirstOrDefault();
		}

		public static List<MethodCallSummary> Summarize(List<MethodCallMetric> methodCallMetricsToSummarize)
		{
			var methodCallSummaries = new List<MethodCallSummary>();

			foreach (var methodCallMetric in methodCallMetricsToSummarize)
			{
				var identifier = methodCallMetric.GetMethodCallIdentifier();

				var summary = methodCallSummaries.SingleOrDefault(sum => sum.IsSameMethodAs(identifier));

				if (summary == null)
				{
					summary = new MethodCallSummary(identifier);
					methodCallSummaries.Add(summary);
				}

				summary.IncludeMetric(methodCallMetric);
			}

			return methodCallSummaries;
		}
	}
}
