namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Scripts
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.MethodCalls;

	public class ScriptExecutionMetric : ScriptExecutionIdentifier
	{
        public List<MethodCallMetric> MethodCallMetrics { get; set; } = new List<MethodCallMetric>();
	}
}
