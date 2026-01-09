namespace Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets
{
	using System;
	using System.Diagnostics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UiDisabling;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class YleSection : Section, IDisableableUi
	{
		protected readonly Helpers helpers;

		protected YleSection(Helpers helpers)
		{
			this.helpers = helpers;
		}

		public new bool IsVisible
		{
			get => base.IsVisible;
			set
			{
				base.IsVisible = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public new bool IsEnabled
		{
			get => base.IsEnabled;
			set
			{
				base.IsEnabled = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public event EventHandler<EnabledStateEventArgs> UiEnabledStateChangeRequired;

		public event EventHandler RegenerateUiRequired;

		protected string Identifier { get; set; }

		public abstract void RegenerateUi();

		protected virtual void GenerateUi(out int row)
		{
			Clear();

			row = 0;
		}

		protected abstract void HandleVisibilityAndEnabledUpdate();

		protected void HandleUiEnabledStateChangeRequired(object sender,  EnabledStateEventArgs e)
		{
			UiEnabledStateChangeRequired?.Invoke(sender, e);
		}

		protected MetricLogger StartPerformanceLogging()
		{
			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			return MetricLogger.StartNew(helpers, this.GetType().Name, nameOfMethod, Identifier);
		}

		protected void HandleRegenerateUiRequired(object sender, EventArgs e)
		{
			RegenerateUiRequired?.Invoke(sender, e);
		}

		public void DisableUi()
		{
			UiEnabledStateChangeRequired?.Invoke(this, new EnabledStateEventArgs(EnabledState.Disabled));
		}

		public void EnableUi()
		{
			UiEnabledStateChangeRequired?.Invoke(this, new EnabledStateEventArgs(EnabledState.Enabled));
		}

		protected void InvokeRegenerateUi()
		{
			RegenerateUiRequired?.Invoke(this, EventArgs.Empty);
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
