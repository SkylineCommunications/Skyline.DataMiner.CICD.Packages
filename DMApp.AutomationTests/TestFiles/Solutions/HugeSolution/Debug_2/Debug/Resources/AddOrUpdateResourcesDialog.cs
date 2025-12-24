namespace Debug_2.Debug.Resources
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class AddOrUpdateResourcesDialog : DebugDialog
	{
		private readonly GetResourcesSection getResourcesSection = new GetResourcesSection();

		private readonly CheckBox addOrUpdateCustomPropertyCheckBox = new CheckBox("Add or Update Custom Property"); 
		private readonly Label propertyNameLabel = new Label("Property Name to Add or Update");
		private readonly TextBox propertyNameTextBox = new TextBox();
		private readonly Label propertyValueLabel = new Label("Property Value to Add or Update");
		private readonly TextBox propertyValueTextBox = new TextBox();

		private readonly CheckBox updateFunctionGuidCheckBox = new CheckBox("Update Function GUID");
		private readonly Label functionGuidLabel = new Label("Function GUID");
		private readonly TextBox functionGuidTextBox = new TextBox();

		private readonly Button addOrUpdateResourcesButton = new Button("Add or Update Resources");

		public AddOrUpdateResourcesDialog(Helpers helpers) : base(helpers)
		{
			Title = "Add or Update Resources";

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

			addOrUpdateResourcesButton.Pressed += AddOrUpdateResourcesButton_Pressed;
		}

		private void AddOrUpdateResourcesButton_Pressed(object sender, EventArgs e)
		{
			var selectedResources = getResourcesSection.SelectedResources;

			string propertyName = propertyNameTextBox.Text;
			string propertyValue = propertyValueTextBox.Text;

			bool validGuid = Guid.TryParse(functionGuidTextBox.Text, out var newFunctionGuid);

			foreach (var resource in selectedResources)
			{
				if (addOrUpdateCustomPropertyCheckBox.IsChecked)
				{
					var existingProperty = resource.Properties.SingleOrDefault(p => p.Name == propertyName);

					if (existingProperty is null)
					{
						resource.Properties.Add(new Skyline.DataMiner.Net.Messages.ResourceManagerProperty
						{
							Name = propertyName,
							Value = propertyValue,
						});
					}
					else
					{
						existingProperty.Value = propertyValue;
					}
				}

				if (updateFunctionGuidCheckBox.IsChecked && validGuid)
				{
					resource.FunctionGUID = newFunctionGuid;
				}
			}

			DataMinerInterface.ResourceManager.AddOrUpdateResources(helpers, selectedResources);		
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
			AddWidget(addOrUpdateCustomPropertyCheckBox, ++row, 0);
			AddWidget(propertyNameLabel, ++row, 0);
			AddWidget(propertyNameTextBox, row, 1);
			AddWidget(propertyValueLabel, ++row, 0);
			AddWidget(propertyValueTextBox, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(updateFunctionGuidCheckBox, ++row, 0);
			AddWidget(functionGuidLabel, ++row, 0);
			AddWidget(functionGuidTextBox, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(addOrUpdateResourcesButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddResponseSections(row);
		}
	}
}
