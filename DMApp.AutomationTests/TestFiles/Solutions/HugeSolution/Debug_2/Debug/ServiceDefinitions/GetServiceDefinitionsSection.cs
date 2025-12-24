namespace Debug_2.Debug.ServiceDefinitions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library.UI.Filters;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ServiceManager.Objects;

	public class GetServiceDefinitionsSection : Section
	{
		private readonly Label header = new Label("Get Service Definitions with Filters") { Style = TextStyle.Heading };

		private readonly StringFilterSection<ServiceDefinition> nameFilterSection = new StringFilterSection<ServiceDefinition>("Name", name => ServiceDefinitionExposers.Name.Equal((string)name));

		private readonly StringFilterSection<ServiceDefinition> idFilterSection = new GuidFilterSection<ServiceDefinition>("ID", id => ServiceDefinitionExposers.ID.Equal((Guid)id));

		private readonly List<FilterSection<ServiceDefinition>> nodeFunctionIdFilterSections = new List<FilterSection<ServiceDefinition>>();

		private readonly Button addNodeFunctionIdFilterButton = new Button("Add Node Function ID Filter");

		private readonly List<FilterSection<ServiceDefinition>> propertyFilterSections = new List<FilterSection<ServiceDefinition>>();

		private readonly Button addPropertyFilterButton = new Button("Add Property Filter");

		private readonly Button getSelectedServiceDefinitionsButton = new Button("Get Selected Service Definitions") { Style = ButtonStyle.CallToAction };
		private readonly CollapseButton showSelectedServiceDefinitionsButton;
		private readonly TextBox selectedServiceDefinitionsTextBox = new TextBox() { IsMultiline = true, MinWidth = 500 };
		private readonly Helpers helpers;

		public GetServiceDefinitionsSection(Helpers helpers)
		{
			this.helpers = helpers;

			addNodeFunctionIdFilterButton.Pressed += AddNodeFunctionIdFilterButton_Pressed;
			addPropertyFilterButton.Pressed += AddPropertyFilterButton_Pressed;

			showSelectedServiceDefinitionsButton = new CollapseButton(selectedServiceDefinitionsTextBox.Yield(), true) { CollapseText = "Hide Selected Service Definitions", ExpandText = "Show Selected Service Definitions" };

			getSelectedServiceDefinitionsButton.Pressed += (o, e) => SelectedServiceDefinitions = GetSelectedServiceDefinitions();

			GenerateUi();
		}

		public IEnumerable<ServiceDefinition> SelectedServiceDefinitions { get; private set; } = new List<ServiceDefinition>();

		public event EventHandler RegenerateUi;

		private void AddNodeFunctionIdFilterButton_Pressed(object sender, EventArgs e)
		{
			var nodeFunctionIdFilterSection = new GuidFilterSection<ServiceDefinition>("Uses Function ID", guid => ServiceDefinitionExposers.NodeFunctionIDs.Contains((Guid)guid));

			nodeFunctionIdFilterSections.Add(nodeFunctionIdFilterSection);

			RegenerateUi?.Invoke(this, EventArgs.Empty);
		}

		private void AddPropertyFilterButton_Pressed(object sender, EventArgs e)
		{
			var propertyFilterSection = new StringPropertyFilterSection<ServiceDefinition>("Property", (propertyName, propertyValue) => ServiceDefinitionExposers.Properties.DictStringField((string)propertyName).Equal((string)propertyValue));

			propertyFilterSections.Add(propertyFilterSection);

			RegenerateUi?.Invoke(this, EventArgs.Empty);
		}

		private IEnumerable<ServiceDefinition> GetSelectedServiceDefinitions()
		{
			selectedServiceDefinitionsTextBox.Text = String.Empty;
			if (!this.ActiveFiltersAreValid<ServiceDefinition>()) return new List<ServiceDefinition>();

			if (!this.TryGetCombinedFilterElement<ServiceDefinition>(out var combinedFilter))
			{
				combinedFilter = new ANDFilterElement<ServiceDefinition>(ServiceDefinitionExposers.Name.NotEqual(string.Empty));
			}

			var selectedServiceDefinitions = DataMinerInterface.ServiceManager.GetServiceDefinitions(helpers, combinedFilter);

			selectedServiceDefinitionsTextBox.Text = String.Join("\n", selectedServiceDefinitions.Select(r => r.Name).OrderBy(name => name));

			return selectedServiceDefinitions;
		}

		public void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(header, ++row, 0, 1, 5);

			AddSection(nameFilterSection, new SectionLayout(++row, 0));

			AddSection(idFilterSection, new SectionLayout(++row, 0));

			foreach (var nodeFunctionIdFilterSection in nodeFunctionIdFilterSections)
			{
				AddSection(nodeFunctionIdFilterSection, new SectionLayout(++row, 0));
			}

			AddWidget(addNodeFunctionIdFilterButton, ++row, 0);

			foreach (var propertyFilterSection in propertyFilterSections)
			{
				AddSection(propertyFilterSection, new SectionLayout(++row, 0));
			}

			AddWidget(addPropertyFilterButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(getSelectedServiceDefinitionsButton, ++row, 0);
			AddWidget(showSelectedServiceDefinitionsButton, row, 1);
			AddWidget(selectedServiceDefinitionsTextBox, ++row, 1);
		}
	}
}
