namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Diagnostics;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;

	public abstract class YleScript : IDisposable
	{
		protected Helpers helpers;
		protected int timeOutResult;

		protected abstract Scripts ScriptName { get; }

		protected abstract TimeSpan? TimeOut { get; }

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			Initialize(engine);

			System.Threading.Tasks.Task scriptTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
			{
				try
				{
					InternalRun();
				}
				catch (ScriptAbortException)
				{
					// Nothing to do
				}
				catch (InteractiveUserDetachedException)
				{
					// Nothing to do
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						helpers.Log(nameof(YleScript), nameof(Run), $"Something went wrong: {e}");

						HandleException(e);
					}
				}
				finally
				{
					Dispose();
				}
			});

			timeOutResult = System.Threading.Tasks.Task.WaitAny(new[] { scriptTask }, TimeOut.Value);
		}

		protected abstract void InternalRun();

		protected abstract void HandleException(Exception e);

		protected virtual void Initialize(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(10);

			helpers = new Helpers(engine, ScriptName);
		}

		protected MetricLogger StartPerformanceLogging()
		{
			string methodName = new StackTrace().GetFrame(1).GetMethod().Name;

			return MetricLogger.StartNew(helpers, this.GetType().Name, methodName);
		}

		protected void Log(string message)
		{
			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			helpers?.Log(this.GetType().Name, nameOfMethod, message);
		}

		#region IDisposable Support
		private bool isDisposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed && disposing)
			{
				helpers.Dispose();
			}

			isDisposed = true;
		}

		~YleScript()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
