namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Scripts
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;

	public class ScriptMetricSummary : ScriptIdentifier
	{
		public List<ScriptExecutionMetricSummary> ScriptExecutionMetricSummaries { get; set; } = new List<ScriptExecutionMetricSummary>();

		public TimeSpan AverageExecutionTime => TimeSpan.FromSeconds(ScriptExecutionMetricSummaries.Select(s => s.ExecutionTime.TotalSeconds).Average());

		public TimeSpan HighestExecutionTime => GetExecutionTimes().Max();

		public TimeSpan LowestExecutionTime => GetExecutionTimes().Min();

		public int AmountOfExecutions => GetExecutionTimes().Count;

		public List<DateTime?> GetStartTimes()
		{
			return ScriptExecutionMetricSummaries.Where(x => x.StartTime != null).Select(x => x.StartTime).OrderBy(x => x).ToList();
		}

		private List<TimeSpan> GetExecutionTimes()
		{
			return ScriptExecutionMetricSummaries.Select(s => s.ExecutionTime).ToList();
		}

		[JsonIgnore]
		public string QaPortalTestNameForAverage => $"RT_YLE_Perf_Script_{ScriptName} Average";

		[JsonIgnore]
		public string QaPortalTestNameForHighest => $"RT_YLE_Perf_Script_{ScriptName} Highest";

		public static List<ScriptMetricSummary> Combine(List<ScriptMetricSummary> scriptMetricSummariesToCombine)
		{
			var combinedScriptMetricSummaries = new List<ScriptMetricSummary>();

			foreach (var scriptMetricSummary in scriptMetricSummariesToCombine)
			{
				var existingScriptMetricSummary = combinedScriptMetricSummaries.SingleOrDefault(sms => sms.IsSameScriptAs(scriptMetricSummary));

				if (existingScriptMetricSummary == null)
				{
					combinedScriptMetricSummaries.Add(scriptMetricSummary);
				}
				else
				{
					existingScriptMetricSummary.Add(scriptMetricSummary);
				}
			}

			return combinedScriptMetricSummaries;
		}

		public void Add(ScriptMetricSummary second)
		{
			if (!second.IsSameScriptAs(this)) return;

			ScriptExecutionMetricSummaries.AddRange(second.ScriptExecutionMetricSummaries);
		}
	}
}
