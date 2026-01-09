using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	public static class EngineExtensions
	{
		/// <summary>
		/// Logs to SLAutomation using the following template: <paramref name="nameOfClass"/>|<paramref name="nameOfMethod"/>|<paramref name="nameOfOrderOrService"/>|<paramref name="message"/>
		/// </summary>
		public static void Log(this IEngine engine, string nameOfClass, string nameOfMethod, string message, string nameOfOrderOrService = null)
		{
			var sb = new StringBuilder();

			sb.Append(nameOfClass);
			sb.Append("|");
			sb.Append(nameOfMethod);
			sb.Append("|");

			if (!string.IsNullOrEmpty(nameOfOrderOrService))
			{
				sb.Append(nameOfOrderOrService);
				sb.Append("|");
			}

			sb.Append(message);

			engine.Log(sb.ToString());
		}

		public static void LogMethodStart(this IEngine engine, string nameOfClass, string nameOfMethod, string nameOfOrderOrService = null)
		{
			Log(engine, nameOfClass, nameOfMethod, "Start", nameOfOrderOrService);
		}

		public static void LogMethodCompleted(this IEngine engine, string nameOfClass, string nameOfMethod, string nameOfOrderOrService = null)
		{
			Log(engine, nameOfClass, nameOfMethod, "Completed", nameOfOrderOrService);
		}

		public static void LogException(this IEngine engine, string nameOfClass, string nameOfMethod, Exception exception)
		{
			Log(engine, nameOfClass, nameOfMethod, "Something went wrong: "+exception.ToString());
		}
	}
}
