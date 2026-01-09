using System;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public abstract class ReportDialog : ProgressDialog
	{
		protected Helpers helpers;

		protected TimeSpan? totalDuration;

		protected ReportDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Report";
			RollBackButton = new Button("Roll Back") { IsEnabled = true, Width = 100 };

			// IsRollbackReport = false;
			this.helpers = helpers;
			helpers.ProgressReported += ProgressReporter_ProgressReported;
		}

		private void ProgressReporter_ProgressReported(object sender, ProgressReportedEventArgs e)
		{
			AddProgressLine(e.Progress.Replace('=', ' ')); // '=' is breaking Engine.ShowProgress
		}

		public void Finish(IEnumerable<Task> tasks, TimeSpan? duration = null)
		{
			this.totalDuration = duration;

			var updateResult = new UpdateResult
			{
				Tasks = tasks.ToList(),
				Exceptions = tasks.Select(t => t.Exception).Where(e => e != null).ToList()
			};

			updateResult.UpdateWasSuccessful = !(updateResult.Tasks.Any(t => t.Status != Status.Ok) || updateResult.Exceptions.Any());

			UpdateResults = new[] { updateResult };

			GenerateUI();
		}

		public void Finish(UpdateResult updateResult)
		{
			if (updateResult == null) throw new ArgumentNullException(nameof(updateResult));

			UpdateResults = updateResult.Yield();

			this.totalDuration = updateResult.Duration;

			GenerateUI();
		}

		internal abstract void GenerateUI();

		public Button RollBackButton { get; private set; }

		public IEnumerable<UpdateResult> UpdateResults { get; private set; } = new List<UpdateResult>();

		/// <summary>
		/// Default value: false
		/// </summary>
		// public bool IsRollbackReport { get; internal set; }

		/// <summary>
		/// Indicates if all tasks were successful
		/// </summary>
		public bool TasksWereSuccessful
		{
			get
			{
				return UpdateResults.SelectMany(ur => ur.Tasks).All(t => t.Status == Status.Ok);
			}
		}

		/// <summary>
		/// Indicates if any blocking tasks failed
		/// </summary>
		public bool ShouldRollback
		{
			get
			{
				return UpdateResults.SelectMany(ur => ur.Tasks).Any(t => t.IsBlocking && t.Status == Status.Fail);
			}
		}
	}
}