namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net.Messages;

	public class LogCollectorHelper
	{
		[Flags]
		public enum Processes
		{
			None = 0,
			SLAutomation = 1,
			SLNet = 2,
		}

		private const string directory = @"C:\Skyline_Data\SLLogCollector\";
		private const string scriptName = "RunLogCollector";
		private TimeSpan minimumTimeBetweenLogCollections = TimeSpan.FromMinutes(20);

		private readonly Helpers helpers;
		private readonly ICollection<IDma> agents;

		public LogCollectorHelper(Helpers helpers)
		{
			this.helpers = helpers;

			this.agents = Engine.SLNetRaw.GetDms().GetAgents();
		}

		public bool LogCollectionRequired(DateTime scriptStartTime)
		{
			var mostRecentLogCollectionTimeStamp = GetMostRecentLogCollectionTimeStamp();

			bool required = mostRecentLogCollectionTimeStamp < scriptStartTime && mostRecentLogCollectionTimeStamp + minimumTimeBetweenLogCollections < DateTime.Now && !AutomationExtensions.IsAutomationScriptRunning(helpers, scriptName);

			helpers.Log(nameof(LogCollectorHelper), nameof(LogCollectionRequired), $"LogCollection {(required ? string.Empty : "not ")}required.");

			return required;
		}

		public DateTime GetMostRecentLogCollectionTimeStamp()
		{
			if (!Directory.Exists(directory)) return default(DateTime);

			var mostRecentLogCollectionTimestamp = default(DateTime);

			var files = Directory.GetFiles(directory);

			foreach (var file in files)
			{
				var fileName = Path.GetFileName(file);

				// expected format "2022_01_10 11_31_19_474 [agent name] [agent ID] NOW.zip"

				var fileNameParts = fileName.Split(' ');

				if (fileNameParts.Length != 5)
				{
					helpers.Log(nameof(LogCollectorHelper), nameof(LogCollectionRequired), $"File {fileName} does not have the expected format");
					continue;
				}

				var datePart = fileNameParts[0];

				if (!DateTime.TryParseExact(datePart, "yyyy_MM_dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
				{
					helpers.Log(nameof(LogCollectorHelper), nameof(LogCollectionRequired), $"Could not parse date from {datePart} from filename {fileName}");
					continue;
				}

				var timePart = fileNameParts[1];

				if (!TimeSpan.TryParseExact(timePart, "hh'_'mm'_'ss'_'fff", null, out var time))
				{
					helpers.Log(nameof(LogCollectorHelper), nameof(LogCollectionRequired), $"Could not parse time from {timePart} from filename {fileName}");
					continue;
				}

				var existingLogCollectionTimeStamp = date.Add(time);

				if (mostRecentLogCollectionTimestamp < existingLogCollectionTimeStamp)
				{
					mostRecentLogCollectionTimestamp = existingLogCollectionTimeStamp;
				}
			}

			return mostRecentLogCollectionTimestamp;
		}

		public void RunLogCollectorAsync()
		{
			helpers.LogMethodStart(nameof(LogCollectorHelper), nameof(RunLogCollectorAsync), out var stopwatch);

			PrepareForLogCollection();

			var currentDmaId = Engine.SLNetRaw.ServerDetails.AgentID;
			RunLogCollectorAsyncOnDma(currentDmaId);

			helpers.Log(nameof(LogCollectorHelper), nameof(RunLogCollectorAsync), $"Triggered LogCollector async on current DMA {currentDmaId}");

			var masterDmaId = agents.Select(agent => agent.Id).Min();

			if (currentDmaId != masterDmaId)
			{
				RunLogCollectorAsyncOnDma(masterDmaId);

				helpers.Log(nameof(LogCollectorHelper), nameof(RunLogCollectorAsync), $"Triggered LogCollector async on master DMA {masterDmaId}");
			}

			helpers.LogMethodCompleted(nameof(LogCollectorHelper), nameof(RunLogCollectorAsync), null, stopwatch);
		}

		public void RunLogCollectorOnThisDma(Processes processesToDump = Processes.None)
		{
			helpers.LogMethodStart(nameof(LogCollectorHelper), nameof(RunLogCollectorOnThisDma), out var stopwatch);

			if (AutomationExtensions.MaxAmountOfConcurrentExecutionsReached(helpers, scriptName, 2))
			{
				helpers.Log(nameof(LogCollectorHelper), nameof(RunLogCollectorOnThisDma), $"Script is already running.");
				helpers.LogMethodCompleted(nameof(LogCollectorHelper), nameof(RunLogCollectorOnThisDma), null, stopwatch);
			}

			PrepareForLogCollection();

			LogOpenConnectionsDiagnose();

			try
			{
				Process proc = new Process();
				proc.StartInfo.FileName = @"C:\skyline dataminer\tools\sllogcollector\SL_LogCollector.exe";
				proc.StartInfo.Arguments = $"-c -f={directory}";
				if (processesToDump != Processes.None)
				{
					string processesToDumpArgument = $" -d={processesToDump.ToString().Replace(" ", string.Empty)}";

					helpers.Log(nameof(LogCollectorHelper), nameof(RunLogCollectorOnThisDma), $"Added processes to dump argument: '{processesToDumpArgument}'");

					proc.StartInfo.Arguments += processesToDumpArgument;
				}
				proc.StartInfo.UseShellExecute = true;
				proc.StartInfo.Verb = "runas";
				proc.Start();
				proc.WaitForExit();
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(LogCollectorHelper), nameof(RunLogCollectorOnThisDma), $"Exception ocurred: {ex}");
			}
			finally
			{
				helpers.LogMethodCompleted(nameof(LogCollectorHelper), nameof(RunLogCollectorOnThisDma), null, stopwatch);
			}
		}

		private void LogOpenConnectionsDiagnose()
		{
			try
			{
				var openConnectionsDiagnose = ((TextMessage)helpers.Engine.SendSLNetSingleResponseMessage(new DiagnoseMessage(DiagnoseMessageType.OpenConnections)));

				helpers.Engine.Log($"OPEN CONNECTIONS DIAGNOSE:\n{openConnectionsDiagnose.Text}"); // log with engine so the LogCollector will collect it 
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(LogCollectorHelper), nameof(LogOpenConnectionsDiagnose), $"Exception ocurred: {ex}");
			}
		}

		private void PrepareForLogCollection()
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
		}

		private void RunLogCollectorAsyncOnDma(int dmaId)
		{
			helpers.Engine.SendSLNetSingleResponseMessage(new ExecuteScriptMessage(dmaId, scriptName)
			{
				Options = new SA(new[]
					{
						"USER:cookie",	
						"OPTIONS:0",
						"CHECKSETS:FALSE",
						"EXTENDED_ERROR_INFO",
						"DEFER:TRUE" // async execution
		            })
			});
		}
	}
}
