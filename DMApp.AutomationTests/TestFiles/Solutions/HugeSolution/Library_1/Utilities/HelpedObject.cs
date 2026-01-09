namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Diagnostics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;

	public abstract class HelpedObject
	{
		protected readonly Helpers helpers;

		protected HelpedObject(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
		}

		protected MetricLogger StartPerformanceLogging()
		{
			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			return StartPerformanceLogging(nameOfMethod);
		}

		protected MetricLogger StartPerformanceLogging(string nameOfMethod)
		{
			return MetricLogger.StartNew(helpers, this.GetType().Name, nameOfMethod);
		}

		protected void Log(string message)
		{
			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			helpers.Log(this.GetType().Name, nameOfMethod, message);
		}

		protected void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers.Log(this.GetType().Name, nameOfMethod, message, nameOfObject);
		}
	}
}
