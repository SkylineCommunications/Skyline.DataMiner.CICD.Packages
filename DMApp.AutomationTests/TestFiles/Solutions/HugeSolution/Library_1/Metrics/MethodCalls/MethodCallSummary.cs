namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.MethodCalls
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Filters;

	public class MethodCallSummary : MethodCallIdentifier
	{
		public MethodCallSummary()
		{

		}

		public MethodCallSummary(MethodCallIdentifier identifier)
		{
			SetMethodCallIdentifier(identifier);
		}

		public TimeSpan AverageExecutionTime => TimeSpan.FromSeconds(ExecutionTimes.Select(s => s.TotalSeconds).Average());

		public TimeSpan HighestExecutionTime => ExecutionTimes.Max();

		public int AmountOfExecutions => ExecutionTimes.Count;

		public TimeSpan LowestExecutionTime => ExecutionTimes.Min();

		public TimeSpan TotalExecutionTime => TimeSpan.FromSeconds(ExecutionTimes.Select(s => s.TotalSeconds).Sum());

		public List<TimeSpan> ExecutionTimes { get; set; } = new List<TimeSpan>();

		[JsonIgnore]
		public string QaPortalTestNameForAverage => $"RT_YLE_Perf_Method_{ClassName}.{MethodName} Average";

		[JsonIgnore]
		public string QaPortalTestNameForHighest => $"RT_YLE_Perf_Method_{ClassName}.{MethodName} Highest";

		public static List<MethodCallSummary> Combine(List<MethodCallSummary> methodCallSummariesToCombine)
		{
			var combinedMethodCallSummaries = new List<MethodCallSummary>();

			foreach (var methodCallSummary in methodCallSummariesToCombine)
			{
				var existingSummary = combinedMethodCallSummaries.SingleOrDefault(sum => sum.IsSameMethodAs(methodCallSummary));

				if (existingSummary == null)
				{
					combinedMethodCallSummaries.Add(methodCallSummary);
				}
				else
				{
					existingSummary.Add(methodCallSummary);
				}
			}

			return combinedMethodCallSummaries;
		}

		public bool MatchesFilter(MethodCallSummaryFilter filter)
		{
			bool matchesFilter = true;

			matchesFilter &= base.MatchesFilter(filter);

			return matchesFilter;
		}

		public void IncludeMetric(MethodCallMetric metric)
		{
			if (!metric.IsSameMethodAs(this)) return;

			ExecutionTimes.Add(metric.ExecutionTime);

			ExecutionTimes = ExecutionTimes.OrderByDescending(x => x.TotalSeconds).ToList();
		}

		public void Add(MethodCallSummary second)
		{
			if (!second.IsSameMethodAs(this)) return;

			ExecutionTimes.AddRange(second.ExecutionTimes);
				
			ExecutionTimes = ExecutionTimes.OrderByDescending(x => x.TotalSeconds).ToList();
		}
	}
}