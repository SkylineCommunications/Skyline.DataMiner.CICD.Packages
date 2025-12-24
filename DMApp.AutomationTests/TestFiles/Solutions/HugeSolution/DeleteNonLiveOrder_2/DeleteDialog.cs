namespace DeleteNonLiveOrder_2
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;

	public class DeleteDialog : Dialog
	{
		private readonly Label infoLabel = new Label();

		public DeleteDialog(Engine engine, NonLiveOrder nonLiveOrder) : base(engine)
		{
			Title = "Delete Export";
			infoLabel.Text = "Are you sure you want to delete this Non-Live Order?";

			YesButton = new Button("Yes") { Width = 150, Style = ButtonStyle.CallToAction };
			NoButton = new Button("No") { Width = 150 };

			GenerateUI();
		}

		public Button YesButton
		{
			get; private set;
		}

		public Button NoButton
		{
			get; private set;
		}

		private void GenerateUI()
		{
			MinWidth = 800;
			int row = 0;

			AddWidget(infoLabel, ++row, 0, 1, 2);

			AddWidget(YesButton, ++row, 0);
			AddWidget(NoButton, row, 1);
		}
	}
}