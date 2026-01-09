namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Task = Tasks.Task;

	public abstract class LoadingDialog : ProgressDialog
	{
		private readonly Button sendReportButton = new Button("Send Report") { Width = 150 };
		private readonly Button closeButton = new Button("Close") { Width = 150, Style = ButtonStyle.CallToAction };
		private readonly CollapseButton collapseButton = new CollapseButton() { IsVisible = false, IsCollapsed = true, CollapseText = "Hide Exception", ExpandText = "Show Exception", Width = 150 };
		private bool showInformationMessageLabel = true;
		private bool showSendReportButton = true;

		protected readonly Label informationMessageLabel = new Label(string.Empty) { IsVisible = false };
		protected readonly Label reportSuccessfullySentLabel = new Label("Report successfully sent") { IsVisible = false };
		protected List<Action> methodsToExecute = new List<Action>();

		protected LoadingDialog(Helpers helpers) : base((Engine)helpers?.Engine)
		{
			Helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));

			Title = "Loading UI";

			IsSuccessful = true;
			Tasks = new List<Task>();
			LockInfos = new List<LockInfo>();

			Helpers.ProgressReported += ProgressReported;

			closeButton.Pressed += (o, e) => Engine.ExitSuccess(string.Empty);
			sendReportButton.Pressed += SendReportButton_Pressed;
		}

		public List<Task> Tasks { get; protected set; }

		public bool IsSuccessful { get; protected set; }

		public List<LockInfo> LockInfos { get; }

		public UserInfo UserInfo { get; protected set; }

		public Helpers Helpers { get; }

		/// <summary>
		/// Executes the actions in <see cref="methodsToExecute"/> as long as they succeed.
		/// </summary>
		/// <returns>A boolean indicating if everything was successful.</returns>
		/// <exception cref="ScriptAbortException">Thrown when the user clicks the Close button.</exception>
		public bool Execute()
		{
			Helpers.LogMethodStart(nameof(LoadingDialog), nameof(Execute), out var stopwatch);

			try
			{
				GetScriptInput();

				CollectActions();

				foreach (var action in methodsToExecute.TakeWhile(action => IsSuccessful))
				{
					LogActionStart(action.Method.Name, out var actionStopwatch);
					action();
					LogActionCompleted(action.Method.Name, actionStopwatch);
				}
			}
			catch (Exception e)
			{
				Engine.Log("Execute|" + e);
				Log(nameof(Execute), $"Exception occurred: {e}");
				PrepareUiForExceptionErrorMessage(e);
			}

			if (!IsSuccessful)
			{
				var failedTask = Tasks.FirstOrDefault(task => task.Status == Status.Fail);
				if (failedTask != null) PrepareUiForExceptionErrorMessage(failedTask.Exception);
			}

			UpdateWidgets();
			GenerateUi();

			Helpers.LogMethodCompleted(nameof(LoadingDialog), nameof(Execute), null, stopwatch);

			return IsSuccessful;
		}

		protected abstract void GetScriptInput();

		protected abstract void CollectActions();

		protected abstract void SendReportButton_Pressed(object sender, EventArgs e);

		protected void PrepareUiForManualErrorMessage(string errorMessage, bool showExceptionWidgets = true)
		{
			IsSuccessful = false;

			PrepareUiForManualMessage(errorMessage, showSendReportButton: false, showExceptionWidgets);
		}

		protected void PrepareUiForExceptionErrorMessage(Exception e)
		{
			this.Title = "Exception Occurred";
			IsSuccessful = false;
			PrepareUiForManualMessage(e.ToString(), showSendReportButton: true);
		}

		protected void PrepareUiForManualMessage(string message, bool showSendReportButton = false, bool showExceptionWidgets = true)
		{
			showInformationMessageLabel = showExceptionWidgets;

			collapseButton.Pressed += (o, e) => UpdateWidgets();

			collapseButton.IsVisible = showExceptionWidgets;

			this.showSendReportButton = showSendReportButton;

			informationMessageLabel.Text = message;
			informationMessageLabel.IsVisible = !showExceptionWidgets || !collapseButton.IsCollapsed;
		}

		private void UpdateWidgets()
		{
			sendReportButton.IsVisible = !IsSuccessful && showSendReportButton;
			informationMessageLabel.IsVisible = !showInformationMessageLabel || !collapseButton.IsCollapsed;
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			foreach (var task in Tasks)
			{
				AddWidget(new Label(task.Description), ++row, 0);
				AddWidget(new Label(EnumExtensions.GetDescriptionFromEnumValue(task.Status)), row, 1);
			}

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(collapseButton, new WidgetLayout(++row, 0));
			AddWidget(informationMessageLabel, new WidgetLayout(++row, 0, 1, 2));

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(closeButton, new WidgetLayout(++row, 0));
			AddWidget(sendReportButton, new WidgetLayout(row, 1));

			AddWidget(reportSuccessfullySentLabel, new WidgetLayout(++row, 0));
		}

		protected void ProgressReported(object sender, ProgressReportedEventArgs e)
		{
			AddProgressLine(e.Progress.Replace('=', ' ')); // '=' is breaking Engine.ShowProgress
		}

		protected void LogActionStart(string nameOfMethod, out Stopwatch stopwatch)
		{
			Helpers.LogMethodStart(GetType().Name, nameOfMethod, out stopwatch);
		}

		protected void LogActionCompleted(string nameOfMethod, Stopwatch stopwatch)
		{
			Helpers.LogMethodCompleted(GetType().Name, nameOfMethod, null, stopwatch);
		}

		protected void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			Helpers.Log(GetType().Name, nameOfMethod, message, nameOfObject);
		}
	}
}
