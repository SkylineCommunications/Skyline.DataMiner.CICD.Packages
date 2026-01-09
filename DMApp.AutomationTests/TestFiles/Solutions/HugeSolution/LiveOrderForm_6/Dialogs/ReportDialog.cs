namespace LiveOrderForm_6.Dialogs
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using TaskStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Status;

	public abstract class ReportDialog : Dialog
	{
		private readonly object lockObject = new object();
		private readonly StringBuilder progress = new StringBuilder();

		protected ReportDialog(Helpers helpers) : base((Engine)helpers.Engine)
		{
			Title = "Report";
			OkButton = new Button("OK") { IsEnabled = true, Width = 100 };
			RollBackButton = new Button("Roll Back") { IsEnabled = true, Width = 100, Style = ButtonStyle.CallToAction };

			//IsRollbackReport = false;
			Helpers = helpers;
			helpers.ProgressReported += ProgressReporter_ProgressReported;

			AddWidget(new WhiteSpace(), 0, 0);
			SetColumnWidth(0, 1000);
		}

		private void ProgressReporter_ProgressReported(object sender, ProgressReportedEventArgs e)
		{
			lock (lockObject)
			{
				progress.AppendLine(e.Progress);
				Engine.ShowProgress(progress.ToString());
			}
		}

		public void Finish(IEnumerable<Task> tasks)
		{
			this.Tasks = tasks;
			GenerateUI();
		}

		internal abstract void GenerateUI();

		internal void ClearUi()
		{
			var widgets = new List<Widget>(Widgets);
			foreach (var widget in widgets)
			{
				RemoveWidget(widget);
			}
		}

		public Button OkButton { get; private set; }

		public Button RollBackButton { get; private set; }

		public Helpers Helpers { get; private set; }

		public IEnumerable<Task> Tasks { get; private set; } = new Task[0];

		/// <summary>
		/// Default value: false
		/// </summary>
		//public bool IsRollbackReport { get; internal set; }

		/// <summary>
		/// Indicates if all tasks were successful
		/// </summary>
		public bool IsSuccessful
		{
			get
			{
				return Tasks.All(t => t.Status == TaskStatus.Ok);
			}
		}

		/// <summary>
		/// Indicates if any blocking tasks failed
		/// </summary>
		public bool ShouldRollback
		{
			get
			{
				return Tasks.Any(t => t.IsBlocking && t.Status == TaskStatus.Fail);
			}
		}
	}
}