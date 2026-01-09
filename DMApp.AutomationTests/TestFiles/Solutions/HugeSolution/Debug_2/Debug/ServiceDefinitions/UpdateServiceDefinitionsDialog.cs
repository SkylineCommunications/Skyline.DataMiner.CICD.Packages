namespace Debug_2.Debug.ServiceDefinitions
{
	using System;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ServiceManager.Objects;

	public class UpdateServiceDefinitionsDialog : DebugDialog
	{
		private readonly Label updateServiceDefinitionLabel = new Label("Add or Update Service Definition") { Style = TextStyle.Heading };

		private readonly Label serviceDefinitionLabel = new Label("Service Definition");
		private readonly TextBox serviceDefinitionTextBox = new TextBox { IsMultiline = true };

		private readonly Button addOrUpdateButton = new Button("Add or Update");

		public UpdateServiceDefinitionsDialog(Helpers helpers) : base(helpers)
		{
			Title = "Add or Update Service Definition";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			addOrUpdateButton.Pressed += AddOrUpdateButton_Pressed;
		}

		private void AddOrUpdateButton_Pressed(object sender, EventArgs e)
		{
			var serviceDefinition = JsonConvert.DeserializeObject<ServiceDefinition>(serviceDefinitionTextBox.Text);

			bool successful;
			if (serviceDefinition is null)
			{
				successful = false;
			}
			else
			{
				DataMinerInterface.ServiceManager.AddOrUpdateServiceDefinition(helpers, serviceDefinition, true);
				successful = true;
			}

			ShowRequestResult($"Update {(successful ? "succeeded" : "failed")}", $"Update {(successful ? "succeeded" : "failed")}");
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 8);

			AddWidget(updateServiceDefinitionLabel, ++row, 0, 1, 8);

			AddWidget(serviceDefinitionLabel, ++row, 0);
			AddWidget(serviceDefinitionTextBox, row, 1, 1, 8);

			AddWidget(addOrUpdateButton, ++row, 0);

			AddResponseSections(row);
		}
	}
}
