namespace LiveOrderForm_6.Dialogs
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class DeleteOrderReportDialog : ReportDialog
	{
		public DeleteOrderReportDialog(Helpers helpers) : base(helpers)
		{
			Title = "Delete Order";
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

			// If some blocking tasks failed, then pressing the OK button will cause all tasks to be rolled back
			if (!IsSuccessful)
			{
				AddWidget(new Label("Some items failed to delete."), ++row, 0, 1, 2);
				AddWidget(new Label("Please contact your local DataMiner operator to remove these manually."), ++row, 0, 1, 2);
			}

			AddWidget(OkButton, ++row, 0);

			SetColumnWidth(0, 400);
		}
	}
}