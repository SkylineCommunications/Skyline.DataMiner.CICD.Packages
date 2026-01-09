namespace StopOrder.Dialogs
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ConfirmStopDialog : Dialog
	{
		public readonly Button NoButton = new Button("No") { Width = 150 };
		public readonly Button YesButton = new Button("Yes") { Width = 150 };

		private readonly Label messageLabel = new Label("Are you sure you want to stop oder?");

		public ConfirmStopDialog(IEngine engine) : base(engine)
		{
			InitializeWidgets();
			GenerateUI();
		}

		private void InitializeWidgets()
		{
			this.Title = "Confirm Stop";
		}

		private void GenerateUI()
		{
			Clear();

			int row = -1;

			AddWidget(messageLabel, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(YesButton, ++row, 0, HorizontalAlignment.Left);
			AddWidget(NoButton, row, 1, HorizontalAlignment.Right);
		}
	}
}