namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public abstract class Task : HelpedObject
	{
		protected Task(Helpers helpers) : base(helpers)
		{
			IsBlocking = false;
			Status = Status.NotStarted;
		}

		public TimeSpan Duration { get; private set; }

		public abstract string Description { get; }

		public Status Status { get; private set; }

		public Exception Exception { get; protected set; }

		public bool IsBlocking { get; protected set; }

		public static GenericTask<TResult> CreateNew<TResult>(Helpers helpers, Func<TResult> taskAction, string taskDescription, Func<TResult, string> additionalDescription = null)
		{
			return new GenericTask<TResult>(helpers, taskAction, taskDescription, additionalDescription);
		}

		/// <summary>
		/// This method will execute the Task.
		/// </summary>
		/// <param name="showDescription">An optional boolean indicating if you want to print the description of the task to the loading screen.</param>
		/// <returns>Status indicating if we can execute the next task.</returns>
		public bool Execute(bool showDescription = true)
		{
			using (var metriclogger = StartPerformanceLogging(Description))
			{
				try
				{
					if (showDescription) Report($"{Description}...");

					InternalExecute();

					if (showDescription) Report($"{Description} succeeded");

					Duration = metriclogger.Elapsed;
					Status = Status.Ok;

					return true;
				}
				catch (Exception e)
				{
					if (showDescription) Report($"{Description} failed");
					Log(nameof(Execute), $"Task failed: {e}");

					Exception = e;
					Status = Status.Fail;

					return !IsBlocking;
				}
			}
		}

		public abstract Task CreateRollbackTask();

		protected abstract void InternalExecute();

		protected void Report(string message)
		{
			helpers.ReportProgress(message);
		}
	}
}