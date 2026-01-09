namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Templates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Debug_2.Debug;

	public class DeleteTemplatesDialog : DebugDialog
	{
		private readonly Label addOrUpdateServiceConfigurationsLabel = new Label("Order and Event Templates") { Style = TextStyle.Heading };

		private readonly Label idLabel = new Label("ID");
		private readonly YleTextBox idTextBox = new YleTextBox(string.Empty) { ValidationPredicate = value => Guid.TryParse(value, out var result), ValidationText = "Invalid GUID" };
		private readonly Button enterCurrentIdButton = new Button("Enter Current ID") { Width = 200 };

		private readonly Button deleteOrderTemplateButton = new Button("Delete Order Template");

		public DeleteTemplatesDialog(Helpers helpers) : base(helpers)
		{
			Title = "Templates";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			enterCurrentIdButton.Pressed += (sender, args) => idTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;

			deleteOrderTemplateButton.Pressed += DeleteTemplateButton_Pressed;
		}

		private void DeleteTemplateButton_Pressed(object sender, EventArgs e)
		{
			if (!idTextBox.IsValid) return;

			if (helpers.ContractManager.TryDeleteOrderTemplate(Guid.Parse(idTextBox.Text)))
			{
				ShowRequestResult($"Successfully Removed Order Template {idTextBox.Text}", $"Removed Order Template {idTextBox.Text}");
			}
			else
			{
				ShowRequestResult($"Failed to remove Order Template {idTextBox.Text}", $"Failed to remove Order Template {idTextBox.Text}");
			}
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(addOrUpdateServiceConfigurationsLabel, ++row, 0, 1, 5);

			AddWidget(idLabel, ++row, 0, 1, 2);
			AddWidget(idTextBox, row, 2, 1, 2);
			AddWidget(enterCurrentIdButton, ++row, 2, 1, 2);

			AddWidget(deleteOrderTemplateButton, ++row, 0, 1, 5);

			AddResponseSections(row);
		}
	}
}
