namespace ExecuteFeenixStopCommand_2
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ConfirmDialog : Dialog
	{
		private readonly Label messageLabel = new Label("This will cause a stop notification to be sent to Feenix. Are you sure you want to continue?");

		public ConfirmDialog(Engine engine) : base(engine)
		{
			YesButton = new Button("Yes") { Style = ButtonStyle.CallToAction };
			NoButton = new Button("No");

			int row = -1;
			AddWidget(messageLabel, ++row, 0, 1, 2);

			AddWidget(YesButton, ++row, 0, HorizontalAlignment.Left);
			AddWidget(NoButton, row, 1, HorizontalAlignment.Right);
		}

		public Button YesButton { get; private set; }

		public Button NoButton { get; private set; }
	}
}