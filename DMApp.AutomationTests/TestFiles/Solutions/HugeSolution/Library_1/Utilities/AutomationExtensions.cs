namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;

	public static class AutomationExtensions
	{
		public static bool TryStartScript(Helpers helpers, string scriptName, Dictionary<int, string> parameters, bool waitForCompletion = false, bool checkSets = false)
		{
			var options = new List<string>();
			foreach (var parameter in parameters) options.Add(String.Format("PARAMETER:{0}:{1}", parameter.Key, parameter.Value));

			options.Add("OPTIONS:0");
			options.Add(String.Format("CHECKSETS:{0}", checkSets ? "TRUE" : "FALSE"));
			options.Add("EXTENDED_ERROR_INFO");
			options.Add(String.Format("DEFER:{0}", waitForCompletion ? "FALSE" : "TRUE"));

			return TryStartScript(helpers, scriptName, options.ToArray());
		}

		public static bool TryStartScript(Helpers helpers, string scriptName, Dictionary<string, string> parameters, bool waitForCompletion = false, bool checkSets = false)
		{
			var options = new List<string>();
			foreach (var parameter in parameters) options.Add(String.Format("PARAMETERBYNAME:{0}:{1}", parameter.Key, parameter.Value));

			options.Add("OPTIONS:0");
			options.Add(String.Format("CHECKSETS:{0}", checkSets ? "TRUE" : "FALSE"));
			options.Add("EXTENDED_ERROR_INFO");
			options.Add(String.Format("DEFER:{0}", waitForCompletion ? "FALSE" : "TRUE"));

			return TryStartScript(helpers, scriptName, options.ToArray());
		}

		public static bool IsAutomationScriptRunning(Helpers helpers, string scriptName)
		{
			return MaxAmountOfConcurrentExecutionsReached(helpers, scriptName, 1);
		}

		public static bool MaxAmountOfConcurrentExecutionsReached(Helpers helpers, string scriptName, int maxAmountOfConcurrentExecutions)
		{
			int amountOfCurrentlyRunningScripts = 0;

			try
			{
				var setAutomationInfoResponse = helpers.Engine.SendSLNetSingleResponseMessage(new SetAutomationInfoMessage(AutomationInfoType.Maintenance) { Sa = new SA(new[] { "LIST_SCRIPTIDS" }) }) as SetAutomationInfoResponseMessage;
				if (setAutomationInfoResponse?.saRet?.Sa.Length == null)
				{
					helpers.Log(nameof(AutomationExtensions), nameof(MaxAmountOfConcurrentExecutionsReached), "Received empty SetAutomationInfoResponse");
					return false;
				}

				foreach (var info in setAutomationInfoResponse.saRet.Sa)
				{
					try
					{
						var automationInfo = new AutomationInfo(info);
						if (automationInfo.ScriptName == scriptName) amountOfCurrentlyRunningScripts++;
					}
					catch (Exception)
					{
						// do not log parsing errors
					}
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(AutomationExtensions), nameof(MaxAmountOfConcurrentExecutionsReached), $"Exception: {e}");
			}

			return amountOfCurrentlyRunningScripts >= maxAmountOfConcurrentExecutions;
		}

		private static bool TryStartScript(Helpers helpers, string scriptName, string[] options)
		{
			try
			{
				var response = helpers.Engine.SendSLNetSingleResponseMessage(new ExecuteScriptMessage(scriptName) { Options = new SA(options) }) as ExecuteScriptResponseMessage;
				return response != null && !response.HadError;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(AutomationExtensions), nameof(TryStartScript), $"Exception starting script: {e}");
				return false;
			}
		}

		private struct AutomationInfo
		{
			public string ScriptName;

			/*
			public int Id;
			public DateTime StartTime;
			public string UserName;
			public bool IsRunning;

			private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
			*/

			public AutomationInfo(string info)
			{
				var infoDetails = info.Split(';');
				if (infoDetails.Length != 5) throw new ArgumentException("Invalid info", info);

				ScriptName = infoDetails[2];

				/*
				Id = Convert.ToInt32(infoDetails[0]);
				StartTime = DateTime.ParseExact(infoDetails[1], DateTimeFormat, CultureInfo.InvariantCulture);
				UserName = infoDetails[3];
				IsRunning = infoDetails[4].Equals("Running", StringComparison.OrdinalIgnoreCase);
				*/
			}
		}
	}
}
