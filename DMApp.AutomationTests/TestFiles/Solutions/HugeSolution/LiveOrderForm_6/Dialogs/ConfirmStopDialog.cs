namespace LiveOrderForm_6.Dialogs
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ConfirmStopDialog : Dialog
	{
		private readonly Label messageLabel = new Label("This will execute a stop command for the order. Are you sure you want to continue?");

		public ConfirmStopDialog(IEngine engine) : base(engine)
		{
			this.Title = "Confirm Stop";

			YesButton = new Button("Yes") { Style = ButtonStyle.CallToAction };
			NoButton = new Button("No");

			int row = -1;

			AddWidget(messageLabel, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(YesButton, ++row, 0, HorizontalAlignment.Left);
			AddWidget(NoButton, row, 1, HorizontalAlignment.Right);
		}

		public Button YesButton { get; private set; }

		public Button NoButton { get; private set; }
	}
}