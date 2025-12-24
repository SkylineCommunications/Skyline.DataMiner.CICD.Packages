namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Scripts
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.MethodCalls;
	
	public class ScriptExecutionMetricSummary : ScriptExecutionIdentifier
	{
		public ScriptExecutionMetricSummary()
		{

		}

		public ScriptExecutionMetricSummary(ScriptExecutionIdentifier identifier)
		{
			SetScriptExecutionIdentifier(identifier);
		}
		
		public bool ScriptMetricsAreValid { get; set; } = true;

		public List<MethodCallSummary> MethodCallSummaries { get; set; } = new List<MethodCallSummary>();

		public TimeSpan ExecutionTime { get; set; }

		public static Dictionary<ScriptExecutionIdentifier, List<ScriptExecutionMetricSummary>> Combine(List<ScriptExecutionMetricSummary> scriptExecutionMetricSummariesToCombine)
		{
			var combinedSummaries = new Dictionary<ScriptExecutionIdentifier, List<ScriptExecutionMetricSummary>>();

			foreach (var scriptExecutionMetricSummary in scriptExecutionMetricSummariesToCombine)
			{
				var key = combinedSummaries.Keys.SingleOrDefault(id => id.IsSameScriptAndHasRunOnSameDmaAs(scriptExecutionMetricSummary));
				if (key == null)
				{
					key = scriptExecutionMetricSummary.GetScriptExectionIdentifier();
					combinedSummaries.Add(key, new List<ScriptExecutionMetricSummary> ());
				}

				combinedSummaries[key].Add(scriptExecutionMetricSummary);
			}

			return combinedSummaries;
		}
	}
}
