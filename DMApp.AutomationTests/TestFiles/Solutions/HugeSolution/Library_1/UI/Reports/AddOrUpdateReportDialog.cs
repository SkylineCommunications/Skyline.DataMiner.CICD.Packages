using System;
using System.Collections.Generic;
using System.Linq;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class AddOrUpdateReportDialog : ReportDialog
	{
		private readonly bool userIsMcr;

		public AddOrUpdateReportDialog(Helpers helpers, bool userIsMcr = false)
			: base(helpers)
		{
			this.userIsMcr = userIsMcr;
			Title = "Add Or Update";

			SetColumnWidth(0, 2000);
			AddWidget(new WhiteSpace(), new WidgetLayout(0, 0));
		}

		internal override void GenerateUI()
		{
			Clear();

			int row = -1;

			AddWidget(new Label("Task") { Style = TextStyle.Heading }, ++row, 0);
			AddWidget(new Label("Status") { Style = TextStyle.Heading }, row, 1);
			if(userIsMcr)AddWidget(new Label("Duration") { Style = TextStyle.Heading }, row, 2);

			var tasks = UpdateResults.SelectMany(ur => ur.Tasks).ToList();

			foreach (var task in tasks)
			{
				AddWidget(new Label(task.Description), ++row, 0);
				AddWidget(new Label(EnumExtensions.GetDescriptionFromEnumValue(task.Status)), row, 1);
				if(userIsMcr)AddWidget(new Label($"{Math.Truncate(task.Duration.TotalSeconds * 100) / 100} sec"), row, 2);
			}

			if (userIsMcr)
			{
				double totalSeconds = totalDuration?.TotalSeconds ?? tasks.Select(t => t.Duration.TotalSeconds).Sum();

				AddWidget(new Label($"Total") { Style = TextStyle.Heading }, ++row, 2);
				AddWidget(new Label($"{Math.Truncate(totalSeconds * 100) / 100} sec"), ++row, 2);
			}

			AddWidget(new WhiteSpace(), ++row, 0, 1, 2);

			var allExceptions = UpdateResults.SelectMany(ur => ur.Exceptions).ToList();

			// If some blocking tasks failed, then pressing the OK button will cause all tasks to be rolled back
			if (ShouldRollback)
			{
				AddWidget(new Label("Some critical tasks failed, pressing OK will cause these changes to be rolled back."), ++row, 0, 1, 3);
				AddWidget(new Label("WARNING: aborting the script will leave the invalid configuration as is."), ++row, 0, 1, 3);
			}
			else if (!TasksWereSuccessful)
			{
				AddWidget(new Label("Some minor tasks failed, pressing OK will leave configuration. These minor issues can be fixed manually."), ++row, 0, 1, 3);
				AddWidget(new Label("Pressing Roll Back will cause the changes to be rolled back."), ++row, 0, 1, 3);
			}
			else if (allExceptions.Any())
			{
				AddWidget(new Label("An unexpected error happened. More information can be found below."), ++row, 0, 1, 3);

				var exceptionLabels = new List<Label>();
				
				foreach (var exception in allExceptions)
				{
					exceptionLabels.Add(new Label(exception.ToString()));
				}

				var collapseButton = new CollapseButton(exceptionLabels,true){ExpandText = "Show Errors", CollapseText = "Hide Errors", Width = 100};

				AddWidget(collapseButton, ++row, 0);

				foreach (var exceptionLabel in exceptionLabels)
				{
					AddWidget(exceptionLabel, ++row, 0, 1, 3);
				}
			}

			AddWidget(OkButton, ++row, 0);

			// If some non-blocking tasks failed, then the user can decide whether he wants to roll back or not
			if (!TasksWereSuccessful && !ShouldRollback)
			{
				AddWidget(RollBackButton, row, 1, HorizontalAlignment.Right);
			}

			SetColumnWidth(0, 800);
		}
	}
}