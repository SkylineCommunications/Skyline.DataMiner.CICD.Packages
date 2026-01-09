namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class YleDialog : Dialog, IDisableableUi
	{
		protected readonly Helpers helpers;

		private int unfinishedDisableUiRequests;

		protected YleDialog(Helpers helpers) : base(helpers.Engine)
		{
			this.helpers = helpers;
		}

		public new bool IsEnabled
		{
			get => base.IsEnabled;
			set
			{
				base.IsEnabled = value;
				HandleEnabledUpdate();
			}
		}

		protected abstract void HandleEnabledUpdate();

		public void DisableUi()
		{
			using (StartPerformanceLogging())
			{
				unfinishedDisableUiRequests += 1;

				if (unfinishedDisableUiRequests > 1) return;
				
				if (!Widgets.Any()) return;

				IsEnabled = false;

				LogMethodStart(nameof(Show), out var stopwatch2);

				Show(false);

				LogMethodCompleted(nameof(Show), stopwatch2);

				IsEnabled = true;
			}
		}	

		public void EnableUi()
		{
			unfinishedDisableUiRequests -= 1;

			if (unfinishedDisableUiRequests < 0) unfinishedDisableUiRequests = 0;
		}

		protected void Section_UiEnabledStateChangeRequired(object sender, UiDisabling.EnabledStateEventArgs e)
		{
			if (e.EnabledState == UiDisabling.EnabledState.Enabled)
			{
				EnableUi();
			}
			else if (e.EnabledState == UiDisabling.EnabledState.Disabled)
			{
				DisableUi();
			}
		}

		protected MetricLogger StartPerformanceLogging()
		{
			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			return MetricLogger.StartNew(helpers, this.GetType().Name, nameOfMethod);
		}

		protected void Log(string nameOfMethod, string message)
		{
			helpers.Log(this.GetType().Name, nameOfMethod, message);
		}

		protected void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch)
		{
			helpers.LogMethodStart(this.GetType().Name, nameOfMethod, out stopwatch);
		}

		protected void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch)
		{
			helpers.LogMethodCompleted(this.GetType().Name, nameOfMethod, null, stopwatch);
		}
	}
}
