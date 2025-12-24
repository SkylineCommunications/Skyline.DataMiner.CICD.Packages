namespace Debug_2.Debug.ServiceDefinitions
{
	using System;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ServiceManager.Objects;

	public class GetServiceDefinitionDialog : DebugDialog
	{
		private GetServiceDefinitionsSection getServiceDefinitionsSection;

		private readonly Button showJsonButton = new Button("Show Service Definitions JSON");
		private readonly Button setDiagramHashCodePropertyButton = new Button("Set Diagram Hash Code Property");
		private readonly Button getDiagramHashCodeButton = new Button("Calculate Diagram Hash Code");

		public GetServiceDefinitionDialog(Helpers helpers) : base(helpers)
		{
			Title = "Get Service Definition";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			getServiceDefinitionsSection = new GetServiceDefinitionsSection(helpers);

			getServiceDefinitionsSection.RegenerateUi += GetServiceDefinitionsSection_RegenerateUi;

			showJsonButton.Pressed += ShowJsonButton_Pressed;
			setDiagramHashCodePropertyButton.Pressed += SetDiagramHashCodePropertyButton_Pressed;
			getDiagramHashCodeButton.Pressed += CalculateDiagramHashCodeButton_Pressed;
		}

		private void CalculateDiagramHashCodeButton_Pressed(object sender, EventArgs e)
		{
			ShowRequestResult("Diagram Hash Codes", string.Join(", ", getServiceDefinitionsSection.SelectedServiceDefinitions.Select(sd => $"{sd.Name} => {sd.Diagram.GetHashCodeForYleProject()}")));
			GenerateUi();
		}

		private void SetDiagramHashCodePropertyButton_Pressed(object sender, EventArgs e)
		{
			foreach (var serviceDefinition in getServiceDefinitionsSection.SelectedServiceDefinitions)
			{
				var diagramHashCodeProperty = serviceDefinition.Properties.SingleOrDefault(p => p.Name == ServiceDefinitionPropertyNames.DiagramHashCode);

				if (diagramHashCodeProperty is null)
				{
					diagramHashCodeProperty = new Property(ServiceDefinitionPropertyNames.DiagramHashCode, serviceDefinition.Diagram.GetHashCodeForYleProject().ToString());
					serviceDefinition.Properties.Add(diagramHashCodeProperty);
				}
				else
				{
					diagramHashCodeProperty.Value = serviceDefinition.Diagram.GetHashCodeForYleProject().ToString();
				}

				DataMinerInterface.ServiceManager.AddOrUpdateServiceDefinition(helpers, serviceDefinition, true);
			}

			ShowRequestResult($"Updated hash code property on Service definitions", string.Join("\n", getServiceDefinitionsSection.SelectedServiceDefinitions.Select(sd => sd.Name)), string.Join("\n", getServiceDefinitionsSection.SelectedServiceDefinitions.Select(sd => sd.GetPropertiesAsDictionary().Dictionary[ServiceDefinitionPropertyNames.DiagramHashCode])));
			GenerateUi();
		}

		private void GetServiceDefinitionsSection_RegenerateUi(object sender, EventArgs e)
		{
			getServiceDefinitionsSection.GenerateUi();
			GenerateUi();
		}

		private void ShowJsonButton_Pressed(object sender, EventArgs e)
		{
			ShowRequestResult("Serialized Service Definitions", string.Join("\n", getServiceDefinitionsSection.SelectedServiceDefinitions.Select(r => JsonConvert.SerializeObject(r))));
			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddSection(getServiceDefinitionsSection, new SectionLayout(++row, 0));
			row += getServiceDefinitionsSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(showJsonButton, ++row, 0);
			AddWidget(setDiagramHashCodePropertyButton, ++row, 0);
			AddWidget(getDiagramHashCodeButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 1);

			row++;
			foreach (var responseSection in responseSections)
			{
				responseSection.Collapse();
				AddSection(responseSection, row, 0);
				row += responseSection.RowCount;
			}
		}	
	}
}
