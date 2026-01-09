namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class DebugDialog : YleDialog
	{
		protected readonly List<RequestResultSection> responseSections = new List<RequestResultSection>();

		protected DebugDialog(Helpers helpers) : base(helpers)
		{

		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		protected void ShowRequestResult(string header, params string[] results)
		{
			responseSections.Add(new RequestResultSection(header, results));
		}

		protected void AddResponseSections(int row)
		{
			row++;
			foreach (var responseSection in responseSections)
			{
				if (Widgets.Contains(responseSection.Widgets.First())) continue; // skip already added sections

				responseSection.Collapse();

				AddSection(responseSection, ++row, 0);
				row += responseSection.RowCount;
			}
		}

		protected override void HandleEnabledUpdate()
		{
			foreach (var interactiveWidget in Widgets.OfType<InteractiveWidget>())
			{
				interactiveWidget.IsEnabled = IsEnabled;
			}
		}
	}
}
