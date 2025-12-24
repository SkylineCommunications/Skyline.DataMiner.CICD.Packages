namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Filters;
	using MethodCalls;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Files;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Scripts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class MetricAggregator
	{
		private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Include,
			ReferenceLoopHandling = ReferenceLoopHandling.Serialize
		};

		private readonly Helpers helpers;

		public MetricAggregator(Helpers helpers, int daysToRemainMetricFiles = 7)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			DaysToRemainMetricFiles = daysToRemainMetricFiles;
		}

		public int DaysToRemainMetricFiles { get; }

		public static MethodCallFilter DataMinerInterfaceFilter { get; } = new MethodCallSummaryFilter{ MethodNames = DataMinerInterface.GetWrappedMethods().Select(w => w.ToString()).ToList() };

		public HashSet<DateTime> GetPerformanceDropTimeStamps(string directory, out List<string> processedFileNames, int maxAmountOfFilesToProcess = Int32.MaxValue)
		{
			processedFileNames = Directory.GetFiles(directory).Take(maxAmountOfFilesToProcess).ToList();

			var wrappedMethods = DataMinerInterface.GetWrappedMethods();

			var timestamps = new HashSet<DateTime>();

			foreach (var file in processedFileNames)
			{
				var scriptExecutionMetrics = ReadMetricFile(file);

				foreach (var scriptExecutionMetric in scriptExecutionMetrics)
				{
					var methodCallMetrics = MetricCreator.FlattenMethodCallMetrics(scriptExecutionMetric.MethodCallMetrics);

					foreach (var methodCallMetric in methodCallMetrics)
					{
						var wrappedMethod = wrappedMethods.FirstOrDefault(wm => wm.ToString() == methodCallMetric.MethodName);

						bool performanceDropDetected = wrappedMethod != null && wrappedMethod.PerformanceDropDetectionThreshold < methodCallMetric.ExecutionTime;
						if (!performanceDropDetected) continue;

						timestamps.Add(methodCallMetric.TimeStamp.Truncate(TimeSpan.FromMinutes(1)));
					}
				}
			}

			return timestamps;
		}

		public MetricsSummary GetMetricsSummary(string directory, out List<string> filesToRemove, out int amountOfProcessedFiles , DateTime? startFilter = null, DateTime? endFilter = null, int maxAmountOfFilesToProcess = Int32.MaxValue)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directory);
			var filesToProcess = directoryInfo.GetFiles();
			
			filesToRemove = new List<string>();
			amountOfProcessedFiles = filesToProcess.Length;

			var metricFileSummaries = new List<MetricFileSummary>();
			foreach (var file in filesToProcess)
			{
				if (filesToProcess.Length >= maxAmountOfFilesToProcess) break;

				var fileSummary = ProcessMetricFile(file.FullName, startFilter, endFilter);
				if (fileSummary != null) metricFileSummaries.Add(fileSummary);

				if ((DateTime.Now - file.LastWriteTimeUtc).Days > DaysToRemainMetricFiles) filesToRemove.Add(file.FullName);
			}

			var allScriptExecutionMetricSummaries = metricFileSummaries.SelectMany(smc => smc.ScriptExecutionMetricSummaries).ToList();
			var allScriptMetricSummaries = metricFileSummaries.SelectMany(smc => smc.ScriptMetricSummaries).ToList();

			var metricsSummary = new MetricsSummary
			{
				//ScriptExecutionMetricSummaries = CombineScriptExecutionMetricSummaries(allScriptExecutionMetricSummaries),
				ScriptMetricSummaries = CombineScriptMetricSummaries(allScriptMetricSummaries),
				MethodCallSummaries = GetMethodCallSummaries(allScriptExecutionMetricSummaries)
			};

			return metricsSummary;
		}

		public MethodCallSummary GetDataMinerInterfaceMethodCallSummary(string directory, string methodName, out List<string> processedFileNames, int maxAmountOfFilesToProcess = Int32.MaxValue)
		{
			var result = new MethodCallSummary
			{
				ClassName = nameof(DataMinerInterface),
				MethodName = methodName
			};

			processedFileNames = Directory.GetFiles(directory).Take(maxAmountOfFilesToProcess).ToList();

			foreach (var file in processedFileNames)
			{
				var processedFile = ProcessMetricFile(file);

				var matchingMethodCallSummary = processedFile.DataMinerInterfaceMethodCallSummaries.SingleOrDefault(s => s.IsSameMethodAs(result));
				if (matchingMethodCallSummary != null)
				{
					result.Add(matchingMethodCallSummary);
				}
			}

			return result;
		}

		public MetricFileSummary ProcessMetricFile(string fullFilePath, DateTime? startFilter = null, DateTime? endFilter = null)
		{
			var scriptExecutionMetrics = ReadMetricFile(fullFilePath, startFilter, endFilter);
			if (!scriptExecutionMetrics.Any()) return null;

			var metricFileSummary = new MetricFileSummary
			{
				FileName = fullFilePath,
				ScriptExecutionMetrics = scriptExecutionMetrics
			};

			metricFileSummary.ScriptExecutionMetricSummaries = GetScriptExecutionMetricSummaries(metricFileSummary.ScriptExecutionMetrics.ToArray());
			metricFileSummary.ScriptMetricSummaries = GetScriptMetricSummaries(metricFileSummary.ScriptExecutionMetricSummaries);
			metricFileSummary.DataMinerInterfaceMethodCallSummaries = GetDataMinerInterfaceMetricSummaries(metricFileSummary.ScriptExecutionMetricSummaries.ToArray());

			return metricFileSummary;
		}

		public List<ScriptExecutionMetric> ReadMetricFiles(string directory, DateTime startFilter, DateTime endFilter, int maxAmountOfFilesToProcess = Int32.MaxValue)
		{
			var processedFileNames = Directory.GetFiles(directory).Take(maxAmountOfFilesToProcess).ToList();

			var scriptExecutionMetrics = new List<ScriptExecutionMetric>();

			foreach (var file in processedFileNames)
			{
				scriptExecutionMetrics.AddRange(ReadMetricFile(file, startFilter, endFilter));
			}

			return scriptExecutionMetrics;
		}

		public List<ScriptExecutionMetric> ReadMetricFile(string fullFilePath, DateTime? startFilter = null, DateTime? endFilter = null)
		{
			var scriptMetrics = new List<ScriptExecutionMetric>();

			startFilter = startFilter ?? DateTime.MinValue;
			endFilter = endFilter ?? DateTime.MaxValue;

			try
			{
				var fileContent = File.ReadAllLines(fullFilePath);
				foreach (var serializedScriptMetrics in fileContent)
				{
					var scriptExecutionMetric = JsonConvert.DeserializeObject<ScriptExecutionMetric>(serializedScriptMetrics, JsonSerializerSettings);
					if (scriptExecutionMetric == null || !scriptExecutionMetric.MethodCallMetrics.Any()) continue;

					var lastMethodCall = scriptExecutionMetric.MethodCallMetrics.Last();
					var endTime = lastMethodCall.TimeStamp.Add(lastMethodCall.ExecutionTime);
					if (startFilter <= scriptExecutionMetric.StartTime && endTime <= endFilter)
					{
						scriptMetrics.Add(scriptExecutionMetric);
					}
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(MetricAggregator), nameof(ReadMetricFile), $"Exception occurred: {e}");
			}

			return scriptMetrics;
		}

		public static List<MethodCallMetric> FindMethodCallMetrics(List<ScriptExecutionMetric> scriptExecutionMetrics, MethodCallMetricFilter metricFilter)
		{
			if (scriptExecutionMetrics == null) throw new ArgumentNullException(nameof(scriptExecutionMetrics));

			var matchingMethodCallMetrics = new List<MethodCallMetric>();

			foreach (var scriptExecutionMetric in scriptExecutionMetrics)
			{
				var allMethodCallMetrics = MetricCreator.FlattenMethodCallMetrics(scriptExecutionMetric.MethodCallMetrics);

				matchingMethodCallMetrics.AddRange(FilterMethodCallMetrics(allMethodCallMetrics, metricFilter));
			}

			return matchingMethodCallMetrics;
		}

		public static List<MethodCallMetric> FilterMethodCallMetrics(List<MethodCallMetric> metricsToFilter, MethodCallMetricFilter metricFilter)
		{
			if (metricsToFilter == null) throw new ArgumentNullException(nameof(metricsToFilter));

			return metricsToFilter.Where(m => m.MatchesFilter(metricFilter)).ToList();
		}

		public List<MethodCallSummary> GetDataMinerInterfaceMetricSummaries(params ScriptExecutionMetric[] scriptExecutionMetrics)
		{
			var scriptExecutionSummary = GetScriptExecutionMetricSummaries(scriptExecutionMetrics);
			
			return GetDataMinerInterfaceMetricSummaries(scriptExecutionSummary.ToArray());
		}

		public static List<MethodCallSummary> GetDataMinerInterfaceMetricSummaries(params ScriptExecutionMetricSummary[] scriptExecutionMetricSummaries)
		{
			if (scriptExecutionMetricSummaries == null) throw new ArgumentNullException(nameof(scriptExecutionMetricSummaries));

			var combinedMatchingMethodCallSummaries = new List<MethodCallSummary>();

			foreach (var scriptExecutionMetricSummary in scriptExecutionMetricSummaries)
			{
				var matchingMethodCallSummaries = scriptExecutionMetricSummary.MethodCallSummaries.Where(s => s.MatchesFilter(DataMinerInterfaceFilter));

				foreach (var matchingMethodCallSummary in matchingMethodCallSummaries)
				{
					var existingSummary = combinedMatchingMethodCallSummaries.SingleOrDefault(mcs => mcs.IsSameMethodAs(matchingMethodCallSummary));
					if (existingSummary != null)
					{
						existingSummary.Add(matchingMethodCallSummary);
					}
					else
					{
						combinedMatchingMethodCallSummaries.Add(matchingMethodCallSummary);
					}
				}
			}

			return combinedMatchingMethodCallSummaries;
		}

		public static List<MethodCallSummary> GetMethodCallSummaries(List<ScriptExecutionMetricSummary> scriptExecutionMetricSummaries)
		{
			if (scriptExecutionMetricSummaries == null) throw new ArgumentNullException(nameof(scriptExecutionMetricSummaries));

			var combinedMatchingMethodCallSummaries = new List<MethodCallSummary>();

			foreach (var scriptExecutionMetricSummary in scriptExecutionMetricSummaries)
			{
				foreach (var methodCallSummary in scriptExecutionMetricSummary.MethodCallSummaries)
				{
					var existingSummary = combinedMatchingMethodCallSummaries.SingleOrDefault(mcs => mcs.IsSameMethodAs(methodCallSummary));
					if (existingSummary != null)
					{
						existingSummary.Add(methodCallSummary);
					}
					else
					{
						combinedMatchingMethodCallSummaries.Add(methodCallSummary);
					}
				}
			}

			return combinedMatchingMethodCallSummaries;
		}

		public List<ScriptExecutionMetricSummary> GetScriptExecutionMetricSummaries(params ScriptExecutionMetric[] scriptExecutionMetrics)
		{
			var scriptExecutionMetricSummaries = new List<ScriptExecutionMetricSummary>();

			foreach (var scriptMetric in scriptExecutionMetrics)
			{
				var identifier = scriptMetric.GetScriptExectionIdentifier();

				var scriptExecutionSummary = new ScriptExecutionMetricSummary(identifier)
				{
					UserLoginName = scriptMetric.UserLoginName,
					UserDisplayName = scriptMetric.UserDisplayName,
					ExecutionTime = scriptMetric.MethodCallMetrics.Any() ? 
						TimeSpan.FromSeconds(scriptMetric.MethodCallMetrics.Where(m => m?.ExecutionTime != null).Select(m => m.ExecutionTime.TotalSeconds).Sum()) : 
						TimeSpan.Zero,
				};

				var metricsToSummarize = MetricCreator.FlattenMethodCallMetrics(scriptMetric.MethodCallMetrics);
				
				if (metricsToSummarize.Any(metric => metric.ExecutionTime == TimeSpan.Zero)) scriptExecutionSummary.ScriptMetricsAreValid = false;

				var summarizedMethodCallMetrics = MethodCallMetric.Summarize(metricsToSummarize);

				scriptExecutionSummary.MethodCallSummaries.AddRange(summarizedMethodCallMetrics);

				scriptExecutionSummary.StartTime = metricsToSummarize.Any() ? metricsToSummarize.Select(m => m.TimeStamp).Min() : (DateTime?)null;

				scriptExecutionMetricSummaries.Add(scriptExecutionSummary);
			}

			return scriptExecutionMetricSummaries;
		}

		public List<ScriptMetricSummary> GetScriptMetricSummaries(List<ScriptExecutionMetricSummary> scriptExecutionMetricSummaries)
		{
			if (scriptExecutionMetricSummaries == null) throw new ArgumentNullException(nameof(scriptExecutionMetricSummaries));

			var scriptMetricSummaries = new List<ScriptMetricSummary>();

			foreach (var scriptExecutionMetricSummary in scriptExecutionMetricSummaries)
			{
				var summary = scriptMetricSummaries.SingleOrDefault(s => s.IsSameScriptAs(scriptExecutionMetricSummary));

				if (summary == null)
				{
					summary = new ScriptMetricSummary
					{
						ScriptName = scriptExecutionMetricSummary.ScriptName
					};

					scriptMetricSummaries.Add(summary);
				}

				summary.ScriptExecutionMetricSummaries.Add(scriptExecutionMetricSummary);
			}

			return scriptMetricSummaries;
		}

		public static List<MethodCallSummary> CombinedMethodCallSummaries(List<MethodCallSummary> methodCallSummariesToCombine)
		{
			return MethodCallSummary.Combine(methodCallSummariesToCombine);
		}

		public static List<ScriptMetricSummary> CombineScriptMetricSummaries(List<ScriptMetricSummary> scriptMetricSummariesToCombine)
		{
			return ScriptMetricSummary.Combine(scriptMetricSummariesToCombine);
		}

		public static Dictionary<ScriptExecutionIdentifier, List<ScriptExecutionMetricSummary>> CombineScriptExecutionMetricSummaries(List<ScriptExecutionMetricSummary> scriptExecutionMetricSummariesToCombine)
		{
			return ScriptExecutionMetricSummary.Combine(scriptExecutionMetricSummariesToCombine);
		}
	}
}