namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.MethodCalls;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Scripts;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class MetricCreator
	{
		private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
		{
			NullValueHandling = NullValueHandling.Include,
			ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			DateFormatHandling = DateFormatHandling.IsoDateFormat
		};

		private readonly Helpers helpers;
		private readonly List<WrappedMethodAttribute> wrappedMethods = DataMinerInterface.GetWrappedMethods();

		private MethodCallMetric currentMethodCallMetric;

		public MetricCreator(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));

            ScriptExecutionMetric.DmaId = Engine.SLNetRaw.ServerDetails.AgentID;
            ScriptExecutionMetric.UserLoginName = helpers.Engine.UserLoginName;
			ScriptExecutionMetric.UserDisplayName = helpers.Engine.UserDisplayName;
			ScriptExecutionMetric.StartTime = DateTime.Now;
		}

		public string ScriptName
		{
			get => ScriptExecutionMetric.ScriptName;
			set => ScriptExecutionMetric.ScriptName = value;
		}

		public ScriptExecutionMetric ScriptExecutionMetric { get; } = new ScriptExecutionMetric();

		public event EventHandler PerformanceDropDetected;

		public static List<MethodCallMetric> FlattenMethodCallMetrics(IEnumerable<MethodCallMetric> methodCallMetrics)
		{
			var flattenedMetrics = new List<MethodCallMetric>();
			foreach (var metric in methodCallMetrics)
			{
				if (metric == null) continue;

				flattenedMetrics.Add(metric);
				flattenedMetrics.AddRange(FlattenMethodCallMetrics(metric.SubMethodCallMetrics));
			}

			return flattenedMetrics;
		}

		public void StartMethodCallMetric(string className, string methodName, string objectName)
		{
			if (string.IsNullOrWhiteSpace(className)) throw new ArgumentNullException(nameof(className));
			if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentNullException(nameof(methodName));

			var newMethodCallMetric = new MethodCallMetric
			{
				ClassName = className,
				MethodName = methodName,
				ObjectName = objectName ?? string.Empty,
				TimeStamp = DateTime.Now
			};

			if (currentMethodCallMetric == null)
			{
				ScriptExecutionMetric.MethodCallMetrics.Add(newMethodCallMetric);
			}
			else
			{
				currentMethodCallMetric.SubMethodCallMetrics.Add(newMethodCallMetric);
			}

			currentMethodCallMetric = newMethodCallMetric;
		}

		public bool TryCompleteMethodCallMetric(string className, string methodName, string objectName, TimeSpan? executionTime)
		{
			try
			{
				CompleteMethodCallMetric(className, methodName, objectName, executionTime);
				return true;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(MetricCreator), nameof(TryCompleteMethodCallMetric), $"Unable to get metrics for {className} {methodName} {objectName}: {e}");
				return false;
			}
		}

		public void CompleteMethodCallMetric(string className, string methodName, string objectName, TimeSpan? executionTime)
		{
			if (string.IsNullOrWhiteSpace(className)) throw new ArgumentNullException(nameof(className));
			if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentNullException(nameof(methodName));

			if (currentMethodCallMetric.ClassName != className || currentMethodCallMetric.MethodName != methodName)
			{
				throw new InvalidOperationException($"Current method is {currentMethodCallMetric.ClassName}.{currentMethodCallMetric.MethodName}, but incoming metric is for {className}.{methodName}");
			}

			currentMethodCallMetric.ExecutionTime = executionTime ?? TimeSpan.Zero;
			
			DetectPerformanceDrop(methodName, currentMethodCallMetric.ExecutionTime);

			currentMethodCallMetric = GetParentMethodCallMetric(currentMethodCallMetric);
		}

		private void DetectPerformanceDrop(string methodName, TimeSpan executionTime)
		{
			try
			{
				var matchingWrappedMethod = wrappedMethods.FirstOrDefault(wm => wm.ToString() == methodName);
				if (matchingWrappedMethod != null && matchingWrappedMethod.PerformanceDropDetectionThreshold < executionTime)
				{
					helpers.Log(nameof(MetricCreator), nameof(DetectPerformanceDrop), $"Performance drop detected: {methodName} took {executionTime} which is more than threshold {matchingWrappedMethod.PerformanceDropDetectionThreshold}");

					PerformanceDropDetected?.Invoke(this, EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(MetricCreator), nameof(DetectPerformanceDrop), $"Exception occurred: {ex}");
			}
		}

		public bool TryGetSerializedMetrics(out string serializedMetrics)
		{
			try
			{
				serializedMetrics = GetSerializedMetrics();
				return true;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(MetricCreator), nameof(TryGetSerializedMetrics), $"Exception occurred: {e}");
				serializedMetrics = string.Empty;
				return false;
			}
		}

		public string GetSerializedMetrics()
		{
			return JsonConvert.SerializeObject(ScriptExecutionMetric, JsonSettings);
		}

		private MethodCallMetric GetParentMethodCallMetric(MethodCallMetric childMethodCallMetric)
		{
			var allMetrics = FlattenMethodCallMetrics(ScriptExecutionMetric.MethodCallMetrics);

			return allMetrics.SingleOrDefault(m => m.SubMethodCallMetrics.Contains(childMethodCallMetric));
		}
	}
}
