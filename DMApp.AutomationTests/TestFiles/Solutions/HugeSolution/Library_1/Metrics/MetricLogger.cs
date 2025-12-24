namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics
{
	using System;
	using System.Diagnostics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class MetricLogger : IDisposable
	{
		private bool disposedValue;

		private readonly Helpers helpers;
		private readonly string nameOfClass;
		private readonly string nameOfMethod;
		private readonly string nameOfObject;
		private readonly Stopwatch stopwatch;

		private MetricLogger(Helpers helpers, string nameOfClass, string nameOfMethod, string nameOfObject = null)
		{
			this.helpers = helpers;
			this.nameOfClass = nameOfClass;
			this.nameOfMethod = nameOfMethod;
			this.nameOfObject = nameOfObject;

			helpers.LogMethodStart(nameOfClass, nameOfMethod, out stopwatch, nameOfObject);
		}

		public TimeSpan Elapsed => stopwatch.Elapsed;

		public static MetricLogger StartNew(Helpers helpers, string nameOfClass)
		{
			string methodName = new StackTrace().GetFrame(1).GetMethod().Name;

			return StartNew(helpers, nameOfClass, methodName);
		}

		public static MetricLogger StartNew(Helpers helpers, string nameOfClass, string nameOfMethod, string nameOfObject = null)
		{
			return new MetricLogger(helpers, nameOfClass, nameOfMethod, nameOfObject);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue && disposing)
			{
				helpers.LogMethodCompleted(nameOfClass, nameOfMethod, nameOfObject, stopwatch);
			}

			disposedValue = true;
		}

		~MetricLogger()
		{
			Dispose(false);
		}
	}
}
