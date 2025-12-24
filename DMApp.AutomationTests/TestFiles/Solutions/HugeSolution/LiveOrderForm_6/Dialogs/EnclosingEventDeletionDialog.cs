namespace LiveOrderForm_6.Dialogs
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class EnclosingEventDeletionDialog : Dialog
	{
		private readonly Label informationLabel = new Label("The enclosing Event has no Orders left. Would you like to delete this Event as well?");

		public EnclosingEventDeletionDialog(IEngine engine, Guid jobId) : base(engine)
		{
			Title = "Delete Enclosing Event";

			JobId = jobId;

			YesButton = new Button("Yes") { Width = 150, Style = ButtonStyle.CallToAction };
			NoButton = new Button("No") { Width = 150 };

			GenerateUI();
		}

		public Button YesButton { get; private set; }

		public Button NoButton { get; private set; }

		public Guid JobId { get; private set; }

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(informationLabel, ++row, 0, 1, 2);
			row += 1;

			AddWidget(YesButton, ++row, 0);
			AddWidget(NoButton, row, 1, HorizontalAlignment.Right);
		}
	}
}