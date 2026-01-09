/*
****************************************************************************
*  Copyright (c) 2022,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

dd/mm/2022	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace AggregateMetrics_1
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Files;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Automation;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private const string MetricLoggingDirectory = @"C:\Skyline_Data\MetricLogging";

		private Helpers helpers;
		private MetricAggregator aggregator;
		private bool pushToQaPortal;
		private bool disposedValue;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			try
			{
				Initialize(engine);

				var summary = aggregator.GetMetricsSummary(MetricLoggingDirectory, out var filesToRemove, out var amountOfProcessedFiles, DateTime.Now.AddDays(-1));
				RemoveProcessedFiles(filesToRemove);

				if (pushToQaPortal)
				{
					MergeMetricsSummariesFromOtherAgents(summary);
					PushMetricsToQaPortal(summary);
				}
				else
				{
					AddSummaryToScriptOutput(engine, summary);
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Exception occurred: {e}");
				throw;
			}
			finally
			{
				Dispose();
			}
		}

		private static void RemoveProcessedFiles(List<string> files)
		{
			foreach (var processedFile in files)
			{
				File.Delete(processedFile);
			}
		}

		private static void AddSummaryToScriptOutput(Engine engine, MetricsSummary summary)
		{
			//var settings = new JsonSerializerSettings();
			//settings.TypeNameHandling = TypeNameHandling.All;
			//var serializedSummary = JsonConvert.SerializeObject(summary, Formatting.None, settings);

			var serializedSummary = JsonConvert.SerializeObject(summary, Formatting.None);

			engine.AddOrUpdateScriptOutput("summary", serializedSummary);
		}

		private void Initialize(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(10);
			engine.SetFlag(RunTimeFlags.NoInformationEvents);

			pushToQaPortal = Convert.ToBoolean(engine.GetScriptParam("pushToQaPortal").Value);
			int daysToRemainMetricFiles = Convert.ToInt32(engine.GetScriptParam("DaysToRemainMetricFiles").Value);

			helpers = new Helpers(engine, Scripts.AggregateMetrics);
			aggregator = new MetricAggregator(helpers, daysToRemainMetricFiles);
		}

		private void MergeMetricsSummariesFromOtherAgents(MetricsSummary summary)
		{
			var thisAgentId = Engine.SLNetRaw.ServerDetails.AgentID;

			var dms = Engine.SLNetRaw.GetDms();
			foreach (var agentId in dms.GetAgents().Where(a => a.State == Skyline.DataMiner.Core.DataMinerSystem.Common.AgentState.Running).Select(a => a.Id))
			{
				if (agentId == thisAgentId) continue;

				var summaryForAgent = GetMetricsSummaryFromAgent(agentId);
				if (summaryForAgent != null)
				{
					summary.Merge(summaryForAgent);
				}
			}
		}

		private MetricsSummary GetMetricsSummaryFromAgent(int agentId)
		{
			var executeScriptMessage = new ExecuteScriptMessage
			{
				ScriptName = "AggregateMetrics",
				DataMinerID = agentId,
				HostingDataMinerID = agentId,
				Options = new SA(new[]
				{
					"DEFER:FALSE",
					"PARAMETERBYNAME:pushToQaPortal:False",
					$"PARAMETERBYNAME:DaysToRemainMetricFiles:{aggregator.DaysToRemainMetricFiles}"
				})
			};

			try
			{
				var response = (ExecuteScriptResponseMessage)helpers.Engine.SendSLNetSingleResponseMessage(executeScriptMessage);

				var output = response.ScriptOutput;
				return GetSummaryFromScriptOutput(output, agentId);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(GetMetricsSummaryFromAgent), $"Exception retrieving metrics summary from agent {agentId}: {e}");
				return null;
			}
		}

		private MetricsSummary GetSummaryFromScriptOutput(Dictionary<string, string> output, int agentId)
		{
			if (output == null || !output.TryGetValue("summary", out var summary))
			{
				helpers.Log(nameof(Script), nameof(GetSummaryFromScriptOutput), $"No metrics summary results received from agent {agentId}");
				return null;
			}

			if (String.IsNullOrEmpty(summary))
			{
				helpers.Log(nameof(Script), nameof(GetSummaryFromScriptOutput), $"Empty metrics summary results received from agent {agentId}");
				return null;
			}

			helpers.Log(nameof(Script), nameof(GetSummaryFromScriptOutput), $"Summary from {agentId}: {summary}");

			//var settings = new JsonSerializerSettings();
			//settings.TypeNameHandling = TypeNameHandling.All;
			//return JsonConvert.DeserializeObject<MetricsSummary>(summary, settings);

			return JsonConvert.DeserializeObject<MetricsSummary>(summary);
		}

		private void PushMetricsToQaPortal(MetricsSummary summary)
		{
			//var allStartTimes = summary.ScriptExecutionMetricSummaries.Values.SelectMany(s => s).Select(x => x.StartTime).Where(s => s != null).ToList();

			var allStartTimes = summary.ScriptMetricSummaries.SelectMany(s => s.GetStartTimes()).Where(s => s != null).ToList();

			var earliestStartTime = allStartTimes.Min();
			var latestEndTime = allStartTimes.Max();

			// Temporary method additions, goal is to make something generic to gather and push method results.
			var fixedMethodNameList = new[] { "GetEvent", "Event", "GetEventByName", "AddOrUpdateOrderReservation", "AddOrUpdateServiceReservation" };
			var methodSummeriesToPush = summary.MethodCallSummaries.Where(mcs => fixedMethodNameList.Contains(mcs.MethodName)).ToList();

			foreach (var methodSummary in methodSummeriesToPush)
			{
				if (methodSummary is null) continue;

				string extraInfo = $"Value aggregated from {methodSummary.AmountOfExecutions} executions from {earliestStartTime} until {latestEndTime}";

				helpers.QAPortalHelper.PublishPerformanceTestResult(methodSummary.QaPortalTestNameForAverage, methodSummary.AverageExecutionTime, extraInfo);
				helpers.QAPortalHelper.PublishPerformanceTestResult(methodSummary.QaPortalTestNameForHighest, methodSummary.HighestExecutionTime, extraInfo);
			}

			foreach (var scriptSummary in summary.ScriptMetricSummaries)
			{
				if (String.IsNullOrWhiteSpace(scriptSummary.ScriptName)) continue;
				if (!scriptSummary.ScriptExecutionMetricSummaries.Any()) continue;

				string extraInfo = $"Value aggregated from {scriptSummary.AmountOfExecutions} executions from {earliestStartTime} until {latestEndTime}";

				helpers.QAPortalHelper.PublishPerformanceTestResult(scriptSummary.QaPortalTestNameForAverage, scriptSummary.AverageExecutionTime, extraInfo);
				helpers.QAPortalHelper.PublishPerformanceTestResult(scriptSummary.QaPortalTestNameForHighest, scriptSummary.HighestExecutionTime, extraInfo);
			}

			foreach (var dataMinerInterfaceMethodSummary in summary.DataMinerInterfaceMethodCallSummaries)
			{
				if (String.IsNullOrWhiteSpace(dataMinerInterfaceMethodSummary.ClassName) ||
					String.IsNullOrWhiteSpace(dataMinerInterfaceMethodSummary.MethodName)) continue;
				if (!dataMinerInterfaceMethodSummary.ExecutionTimes.Any()) continue;

				string extraInfo = $"Value aggregated from {dataMinerInterfaceMethodSummary.AmountOfExecutions} executions from {earliestStartTime} until {latestEndTime}";

				helpers.QAPortalHelper.PublishPerformanceTestResult(dataMinerInterfaceMethodSummary.QaPortalTestNameForAverage, dataMinerInterfaceMethodSummary.AverageExecutionTime, extraInfo);
				helpers.QAPortalHelper.PublishPerformanceTestResult(dataMinerInterfaceMethodSummary.QaPortalTestNameForHighest, dataMinerInterfaceMethodSummary.HighestExecutionTime, extraInfo);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					helpers.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}