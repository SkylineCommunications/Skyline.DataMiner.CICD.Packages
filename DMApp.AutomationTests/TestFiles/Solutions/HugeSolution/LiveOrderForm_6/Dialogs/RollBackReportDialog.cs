namespace LiveOrderForm_6.Dialogs
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class RollBackReportDialog : ReportDialog
	{
		public RollBackReportDialog(Helpers helpers) : base(helpers)
		{
			Title = "Roll Back Order";
		}

		internal override void GenerateUI()
		{
			ClearUi();

			int row = -1;

			AddWidget(new Label("Task") { Style = TextStyle.Heading }, ++row, 0);
			AddWidget(new Label("Status") { Style = TextStyle.Heading }, row, 1);

			foreach (var task in Tasks)
			{
				AddWidget(new Label(task.Description), ++row, 0);
				AddWidget(new Label(EnumExtensions.GetDescriptionFromEnumValue(task.Status)), row, 1);
			}

			AddWidget(new WhiteSpace(), ++row, 0, 1, 2);

			if (!IsSuccessful)
			{
				AddWidget(new Label("The roll back failed."), ++row, 0, 1, 2);
				AddWidget(new Label("Please contact your local DataMiner operator to fix this manually."), ++row, 0, 1, 2);
			}

			AddWidget(OkButton, ++row, 0);

			SetColumnWidth(0, 400);
		}
	}
}