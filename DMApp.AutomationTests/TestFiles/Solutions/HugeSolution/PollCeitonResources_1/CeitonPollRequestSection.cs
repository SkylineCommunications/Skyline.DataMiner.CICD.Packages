namespace PollCeitonResources_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Ceiton;
	using Skyline.DataMiner.Utils.YLE.Integrations.Ceiton;

	public class CeitonPollRequestSection : Section
	{
		private readonly Label title = new Label();

		public CeitonPollRequestSection(ExternalRequest request)
		{
			title.Text = $"{request.ResourceType} {request.ResourceId}";
			TextBox.Text = JsonConvert.SerializeObject(request, Formatting.Indented);

			CollapseButton.LinkedWidgets.Add(TextBox);
			CollapseButton.IsCollapsed = true;

			int row = -1;

			AddWidget(CollapseButton, ++row, 0);
			AddWidget(title, row, 1, 1, 2);

			AddWidget(TextBox, ++row, 1, 1, 2);
		}

		public CollapseButton CollapseButton { get; private set; } = new CollapseButton { CollapseText = "-", ExpandText = "+", Width = 34 };

		public TextBox TextBox { get; private set; } = new TextBox { IsMultiline = true, Height = 150 };
	}
}
