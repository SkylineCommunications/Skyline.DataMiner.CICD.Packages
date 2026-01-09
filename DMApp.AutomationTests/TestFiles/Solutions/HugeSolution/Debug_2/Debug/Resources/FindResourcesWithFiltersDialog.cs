using System;
using System.Linq;
using Newtonsoft.Json;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Debug_2.Debug.Resources
{
	public class FindResourcesWithFiltersDialog : DebugDialog
	{
		private readonly GetResourcesSection getResourcesSection = new GetResourcesSection();

		private readonly Label actionsHeader = new Label("Actions") { Style = TextStyle.Heading };
		private readonly Button showJsonButton = new Button("Show Resources JSON");

		public FindResourcesWithFiltersDialog(Helpers helpers) : base(helpers)
		{
			Title = "Find Resources with Filters";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			getResourcesSection.RegenerateUi += (o, e) =>
			{
				getResourcesSection.GenerateUi();
				GenerateUi();
			};

			showJsonButton.Pressed += ShowJsonButton_Pressed;
		}

		private void ShowJsonButton_Pressed(object sender, EventArgs e)
		{
			ShowRequestResult("Serialized Resources", string.Join("\n", getResourcesSection.SelectedResources.Select(r => JsonConvert.SerializeObject(r))));
			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddSection(getResourcesSection, new SectionLayout(++row, 0));
			for (int i = row; i < getResourcesSection.RowCount; i++)
			{
				SetRowHeight(i, 30);
			}

			row += getResourcesSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(actionsHeader, ++row, 0, 1, 2);

			AddWidget(showJsonButton, ++row, 0, verticalAlignment: VerticalAlignment.Top);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddResponseSections(row);
		}
	}
}
