using System.Linq;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class RollBackReportDialog : ReportDialog
	{
		public RollBackReportDialog(Helpers helpers)
			: base(helpers)
		{
			Title = "Roll Back";
		}

		internal override void GenerateUI()
		{
			Clear();

			int row = -1;

			AddWidget(new Label("Task") { Style = TextStyle.Heading }, ++row, 0);
			AddWidget(new Label("Status") { Style = TextStyle.Heading }, row, 1);

			var tasks = UpdateResults.SelectMany(ur => ur.Tasks).ToList();

			foreach (var task in tasks)
			{
				AddWidget(new Label(task.Description), ++row, 0);
				AddWidget(new Label(EnumExtensions.GetDescriptionFromEnumValue(task.Status)), row, 1);
			}

			AddWidget(new WhiteSpace(), ++row, 0, 1, 2);

			if (!TasksWereSuccessful)
			{
				AddWidget(new Label("The roll back failed."), ++row, 0, 1, 2);
				AddWidget(new Label("Please contact your local DataMiner operator to fix this manually."), ++row, 0, 1, 2);
			}

			AddWidget(OkButton, ++row, 0);

			SetColumnWidth(0, 400);
		}
	}
}