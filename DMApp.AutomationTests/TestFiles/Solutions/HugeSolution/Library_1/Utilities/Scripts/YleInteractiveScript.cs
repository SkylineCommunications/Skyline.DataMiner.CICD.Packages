namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class YleInteractiveScript : YleScript
	{
		protected InteractiveController app;

		protected override void Initialize(Engine engine)
		{
			base.Initialize(engine);

			app = new InteractiveController(engine);
		}

		protected abstract void EngineShowUiInComments(); // Write engine.ShowUI() in comment to let DataMiner recognize this script as interactive

		protected void ShowExceptionDialog(Exception exception)
		{
			var dialog = new ExceptionDialog((Engine)helpers.Engine, exception);
			dialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess("Something went wrong.");

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		protected void ShowMessageDialog(string message, string title)
		{
			var dialog = new MessageDialog((Engine)helpers.Engine, message) { Title = title };
			dialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess(message);

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}
	}	
}
