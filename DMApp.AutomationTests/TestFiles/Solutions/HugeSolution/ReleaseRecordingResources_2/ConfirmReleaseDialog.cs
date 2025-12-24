namespace ReleaseRecordingResources_2
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ConfirmReleaseDialog : Dialog
	{
		private readonly Label messageLabel = new Label("Are you sure that you want to release the resource of this service reservation?");

		public ConfirmReleaseDialog(Engine engine) : base(engine)
		{
			this.Title = "Confirm Release";

			YesButton = new Button("Yes") { Width = 200 };
			NoButton = new Button("No") { Width = 200 };

			GenerateUi();
		}

		public Button YesButton { get; private set; }

		public Button NoButton { get; private set; }

		private void GenerateUi()
		{
			int row = -1;

			AddWidget(messageLabel, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(YesButton, ++row, 0, 1, 1);
			AddWidget(NoButton, row, 1, 1, 1);
		}
	}
}