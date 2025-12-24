namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Resources
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Debug_2.Debug.Resources;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class FixResourceConfigurationDialog : Dialog
	{
		private const int GenericDveTablePid = 65132;
		private const int GenericDveLinkerTablePid = 65146;

		private readonly Helpers helpers;

		private readonly GetResourcesSection getResourcesSection = new GetResourcesSection();

		private readonly Label fixDveElementIdAndStateHeader = new Label("Fix DVE Element ID and State") { Style = TextStyle.Heading };
		private readonly Label explanationLabel = new Label("Will update the Element ID value of the selected resources to the Element ID found in the [Generic DVE table]\nWill set the DVE State column of the [Generic DVE table] to the correct value based on presence of the resource DVE.");
		private readonly Button fixResourcesButton = new Button("Fix Selected Resources");

		private readonly Button fixResourceElementGenericDveTableButton = new Button("Fix Resource Element [Generic DVE Table]");

		private readonly Label fixResourceElementGenericDveLinkerTableHeader = new Label("Fix [Generic DVE Linker Table]") { Style = TextStyle.Heading };
		private readonly Label fixResourceElementGenericDveLinkerTableLabel = new Label("Created by VSC as part of tasks DCP20070 and DCP199662.\nWill loop over all selected resources and check the function Guid in the [Generic DVE Table] and then add or set the row in the [Generic DVE Linker Table]");
		private readonly Label functionGuidLabel = new Label("Function Guid");
		private readonly TextBox functionGuidTextBox = new TextBox();
		private readonly Label fkDataLabel = new Label("[FK Data] value to set");
		private readonly Numeric fkDataNumeric = new Numeric() { Minimum = -1, Decimals = 0 };
		private readonly Label fkTableLabel = new Label("[FK Table] value to set");
		private readonly Numeric fkTableNumeric = new Numeric() { Minimum = -1, Decimals = 0 };
		private readonly Button getResourceElementGenericDveLinkerTableButton = new Button("Get Resources with invalid Resource Element [Generic DVE Linker Table]");
		private readonly Button fixResourceElementGenericDveLinkerTableButton = new Button("Fix Resource Element [Generic DVE Linker Table]");

		private readonly Label deleteResourcesHeader = new Label("Delete Resources") { Style = TextStyle.Heading };
		private readonly Button deleteResourcesButton = new Button("Delete Resources") { Style = ButtonStyle.CallToAction };

		private readonly List<RequestResultSection> resultSections = new List<RequestResultSection>();

		private IDms dms;

		public FixResourceConfigurationDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Fix Resource Configuration";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private void Initialize()
		{
			fixResourcesButton.Pressed += UpdateResourcesButton_Pressed;

			fixResourceElementGenericDveTableButton.Pressed += FixResourceElementGenericDveTableButton_Pressed;

			getResourceElementGenericDveLinkerTableButton.Pressed += GetResourceElementGenericDveLinkerTableButton_Pressed;
			fixResourceElementGenericDveLinkerTableButton.Pressed += FixResourceElementGenericDveLinkerTableButton_Pressed;

			deleteResourcesButton.Pressed += DeleteResourcesButton_Pressed;
		}

		private void DeleteResourcesButton_Pressed(object sender, EventArgs e)
		{
			try
			{
				SrmManagers.ResourceManager.RemoveResources(GetSelectedResources());

				ShowRequestResult("Successfully deleted resources", string.Join("\n", GetSelectedResources().Select(r => r.Name)));
			}
			catch (Exception ex)
			{
				ShowRequestResult("Failed to delete resources", ex.ToString());
			}
		}

		private void GetResourceElementGenericDveLinkerTableButton_Pressed(object sender, EventArgs e)
		{
			if (!Guid.TryParse(functionGuidTextBox.Text, out var inputFunctionGuid))
			{
				ShowRequestResult("Invalid Input", $"Unable to parse {functionGuidTextBox.Text} to a Guid");
				return;
			}

			string inputFkData = ((int)fkDataNumeric.Value).ToString();
			string inputFkTable = ((int)fkTableNumeric.Value).ToString();

			var resourcesToUpdate = GetSelectedResources();

			dms = dms ?? Skyline.DataMiner.Automation.Engine.SLNetRaw.GetDms();

			var elements = new Dictionary<DmsElementId, IDmsElement>();
			var cachedGenericDveTables = new Dictionary<DmsElementId, IDmsTable>();
			var cachedGenericDveLinkerTables = new Dictionary<DmsElementId, IDmsTable>();

			var resourcesWithFailures = new List<string>();
			var resourcesWithMissingRows = new List<string>();
			var resourcesWithInvalidRows = new List<string>();

			foreach (var resource in resourcesToUpdate.OfType<FunctionResource>())
			{
				int mainDveDmaId = resource.MainDVEDmaID;
				int mainDveElementId = resource.MainDVEElementID;

				var mainDveFullId = new DmsElementId(mainDveDmaId, mainDveElementId);

				if (!elements.TryGetValue(mainDveFullId, out var element))
				{
					element = dms.GetElement(mainDveFullId);
					elements.Add(mainDveFullId, element);
				}

				if (!cachedGenericDveTables.TryGetValue(mainDveFullId, out var genericDveTable))
				{
					genericDveTable = element.GetTable(GenericDveTablePid);
					cachedGenericDveTables.Add(mainDveFullId, genericDveTable);
				}

				var genericDveTableRow = genericDveTable.GetData().Values.SingleOrDefault(r => Convert.ToString(r[1]) == resource.FunctionName);
				if (genericDveTableRow is null)
				{
					resourcesWithFailures.Add($"Unable to find row for resource {resource.Name}");
					continue;
				}

				if (!Guid.TryParse(Convert.ToString(genericDveTableRow[4]), out var functionGuid) || functionGuid != inputFunctionGuid)
				{
					resourcesWithFailures.Add($"Resource {resource.Name} Generic DVE Table DVE function Guid is not {inputFunctionGuid}");
					continue;
				}

				string genericDveTableRowPrimaryKey = Convert.ToString(genericDveTableRow[0]);

				if (!cachedGenericDveLinkerTables.TryGetValue(mainDveFullId, out var genericDveLinkerTable))
				{
					genericDveLinkerTable = element.GetTable(GenericDveLinkerTablePid);
					cachedGenericDveLinkerTables.Add(mainDveFullId, genericDveLinkerTable);
				}

				var genericDveLinkerTableRow = genericDveLinkerTable.GetData().Values.SingleOrDefault(r => Convert.ToString(r[1]) == genericDveTableRowPrimaryKey);

				if (genericDveLinkerTableRow is null || !genericDveLinkerTableRow.Any())
				{
					resourcesWithMissingRows.Add(resource.Name);
				}
				else if (Convert.ToString(genericDveLinkerTableRow[1]) != genericDveTableRowPrimaryKey || Convert.ToString(genericDveLinkerTableRow[2]) != inputFkData || Convert.ToString(genericDveLinkerTableRow[3]) != inputFkTable)
				{
					resourcesWithInvalidRows.Add(resource.Name);
				}
				else
				{
					// nothing
				}
			}

			ShowRequestResult("Resources without row in [Generic DVE Linker Table]", string.Join("\n", resourcesWithMissingRows));
			ShowRequestResult("Resources with invalid row in [Generic DVE Linker Table]", string.Join("\n", resourcesWithInvalidRows));
			ShowRequestResult("Resources without row in [Generic DVE Table]", string.Join("\n", resourcesWithFailures));
		}

		private void FixResourceElementGenericDveTableButton_Pressed(object sender, EventArgs e)
		{
			var resourcesToUpdate = GetSelectedResources();

			dms = dms ?? Skyline.DataMiner.Automation.Engine.SLNetRaw.GetDms();

			var elements = new Dictionary<DmsElementId, IDmsElement>();
			var cachedGenericDveTables = new Dictionary<DmsElementId, IDmsTable>();

			foreach (var resource in resourcesToUpdate.OfType<FunctionResource>())
			{
				int mainDveDmaId = resource.MainDVEDmaID;
				int mainDveElementId = resource.MainDVEElementID;

				var mainDveFullId = new DmsElementId(mainDveDmaId, mainDveElementId);

				if (!elements.TryGetValue(mainDveFullId, out var element))
				{
					element = dms.GetElement(mainDveFullId);
					elements.Add(mainDveFullId, element);
				}

				if (!cachedGenericDveTables.TryGetValue(mainDveFullId, out var genericDveTable))
				{
					genericDveTable = element.GetTable(GenericDveTablePid);
					cachedGenericDveTables.Add(mainDveFullId, genericDveTable);
				}

				var genericDveTableRow = genericDveTable.GetData().Values.SingleOrDefault(r => Convert.ToString(r[1]) == resource.FunctionName);
				if (genericDveTableRow is null)
				{
					var existingDisplayKeys = genericDveTable.GetDisplayKeys().Select(key => Convert.ToInt16(key)).ToList();

					genericDveTableRow = new object[]
					{
						(existingDisplayKeys.Any() ? existingDisplayKeys.Max() + 1 : 1).ToString(),
						resource.FunctionName,
						$"{resource.DmaID}/{resource.ElementID}",
						0.ToString(), // disabled
						resource.FunctionGUID.ToString(),
						resource.ID.ToString(),
					};
				}
			}
		}

		private void FixResourceElementGenericDveLinkerTableButton_Pressed(object sender, EventArgs e)
		{
			if (!Guid.TryParse(functionGuidTextBox.Text, out var inputFunctionGuid))
			{
				ShowRequestResult("Invalid Input", $"Unable to parse {functionGuidTextBox.Text} to a Guid");
				return;
			}

			string inputFkData = ((int)fkDataNumeric.Value).ToString();
			string inputFkTable = ((int)fkTableNumeric.Value).ToString();

			var resourcesToUpdate = GetSelectedResources();

			dms = dms ?? Skyline.DataMiner.Automation.Engine.SLNetRaw.GetDms();

			var failedUpdates = new List<string>();
			var elements = new Dictionary<DmsElementId, IDmsElement>();
			var cachedGenericDveTables = new Dictionary<DmsElementId, IDmsTable>();
			var cachedGenericDveLinkerTables = new Dictionary<DmsElementId, IDmsTable>();

			foreach (var resource in resourcesToUpdate.OfType<FunctionResource>())
			{
				int mainDveDmaId = resource.MainDVEDmaID;
				int mainDveElementId = resource.MainDVEElementID;

				var mainDveFullId = new DmsElementId(mainDveDmaId, mainDveElementId);

				if (!elements.TryGetValue(mainDveFullId, out var element))
				{
					element = dms.GetElement(mainDveFullId);
					elements.Add(mainDveFullId, element);
				}

				if (!cachedGenericDveTables.TryGetValue(mainDveFullId, out var genericDveTable))
				{
					genericDveTable = element.GetTable(GenericDveTablePid);
					cachedGenericDveTables.Add(mainDveFullId, genericDveTable);
				}

				var genericDveTableRow = genericDveTable.GetData().Values.SingleOrDefault(r => Convert.ToString(r[1]) == resource.FunctionName);
				if (genericDveTableRow is null)
				{
					failedUpdates.Add($"Unable to find row for resource {resource.Name}");
					continue;
				}

				if (!Guid.TryParse(Convert.ToString(genericDveTableRow[4]), out var functionGuid) || functionGuid != inputFunctionGuid)
				{
					failedUpdates.Add($"Resource {resource.Name} Generic DVE Table DVE function Guid is not {inputFunctionGuid}");
					continue;
				}

				string genericDveTableRowPrimaryKey = Convert.ToString(genericDveTableRow[0]);

				if (!cachedGenericDveLinkerTables.TryGetValue(mainDveFullId, out var genericDveLinkerTable))
				{
					genericDveLinkerTable = element.GetTable(GenericDveLinkerTablePid);
					cachedGenericDveLinkerTables.Add(mainDveFullId, genericDveLinkerTable);
				}

				var genericDveLinkerTableRow = genericDveLinkerTable.GetData().Values.SingleOrDefault(r => Convert.ToString(r[1]) == genericDveTableRowPrimaryKey);

				if (genericDveLinkerTableRow is null || !genericDveLinkerTableRow.Any())
				{
					var existingDisplayKeys = genericDveLinkerTable.GetDisplayKeys().Select(key => Convert.ToInt16(key)).ToList();

					genericDveLinkerTableRow = new object[]
					{
						(existingDisplayKeys.Any() ? existingDisplayKeys.Max() + 1 : 1).ToString(),
						genericDveTableRowPrimaryKey,
						inputFkData,
						inputFkTable,
					};

					genericDveLinkerTable.AddRow(genericDveLinkerTableRow);
				}
				else
				{
					genericDveLinkerTableRow[1] = genericDveTableRowPrimaryKey;
					genericDveLinkerTableRow[2] = inputFkData;
					genericDveLinkerTableRow[3] = inputFkTable;

					genericDveLinkerTable.SetRow(genericDveLinkerTableRow[0].ToString(), genericDveLinkerTableRow);
				}
			}

			ShowRequestResult("Updated Resource Element Generic DVE Linker Tables", $"Updated Generic DVE Linker Tables on resource elements:\n{string.Join("\n", resourcesToUpdate.Select(r => $"{r.Name} => {r.DmaID}/{r.ElementID}"))}");

			ShowRequestResult("Failed Updates", $"{string.Join("\n", failedUpdates)}");
		}

		private void UpdateResourcesButton_Pressed(object sender, EventArgs e)
		{
			var resourcesToUpdate = GetSelectedResources();

			dms = dms ?? Skyline.DataMiner.Automation.Engine.SLNetRaw.GetDms();

			var failedUpdates = new List<string>();
			var cachedTables = new Dictionary<DmsElementId, IDmsTable>();

			foreach (var resource in resourcesToUpdate.OfType<FunctionResource>())
			{
				int mainDveDmaId = resource.MainDVEDmaID;
				int mainDveElementId = resource.MainDVEElementID;

				var mainDveFullId = new DmsElementId(mainDveDmaId, mainDveElementId);

				if (!cachedTables.TryGetValue(mainDveFullId, out var table))
				{
					table = dms.GetElement(mainDveFullId).GetTable(65132);
					cachedTables.Add(mainDveFullId, table);
				}

				var row = table.GetData().Values.FirstOrDefault(r => Convert.ToString(r[1]) == resource.FunctionName);
				if (row is null)
				{
					failedUpdates.Add($"Unable to find row for resource {resource.Name}");
					continue;
				}

				var resourceDveFullId = Convert.ToString(row[2]);

				resource.DmaID = int.Parse(resourceDveFullId.Split('/')[0]);
				resource.ElementID = int.Parse(resourceDveFullId.Split('/').Last());

				try
				{
					var resourceDve = dms.GetElement(resourceDveFullId);

					if (Convert.ToInt32(row[3]) != 1)
					{
						// set cell to 1
						row.SetValue(1, 3);
						table.SetRow(Convert.ToString(row[0]), row);
					}
				}
				catch (Exception)
				{
					if (Convert.ToInt32(row[3]) != 0)
					{
						// set cell to 0
						row.SetValue(0, 3);
						table.SetRow(Convert.ToString(row[0]), row);
					}
				}
			}

			SrmManagers.ResourceManager.AddOrUpdateResources(resourcesToUpdate);

			ShowRequestResult("Updated Resources", $"Updated Resources Element IDs:\n{string.Join("\n", resourcesToUpdate.Select(r => $"{r.Name} => {r.DmaID}/{r.ElementID}"))}");
			ShowRequestResult("Failed Updates", $"{string.Join("\n", failedUpdates)}");
		}

		private Resource[] GetSelectedResources()
		{
			return getResourcesSection.SelectedResources.ToArray();
		}

		private void ShowRequestResult(string header, params string[] results)
		{
			resultSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 5);
			AddWidget(new WhiteSpace(), ++row, 0);

			AddSection(getResourcesSection, new SectionLayout(++row, 0));
			row += getResourcesSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(fixDveElementIdAndStateHeader, ++row, 0, 1, 5);
			AddWidget(explanationLabel, ++row, 0, 1, 5);
			AddWidget(fixResourcesButton, ++row, 0);

			/*
			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(fixResourceElementGenericDveTableHeader, ++row, 0, 1, 5);
			AddWidget(fixResourceElementGenericDveTableLabel, ++row, 0, 1, 5);
			AddWidget(fixResourceElementGenericDveTableButton, ++row, 0, 1, 5);
			*/

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(fixResourceElementGenericDveLinkerTableHeader, ++row, 0, 1, 5);
			AddWidget(fixResourceElementGenericDveLinkerTableLabel, ++row, 0, 1, 5);
			AddWidget(functionGuidLabel, ++row, 0);
			AddWidget(functionGuidTextBox, row, 1);
			AddWidget(fkDataLabel, ++row, 0);
			AddWidget(fkDataNumeric, row, 1);
			AddWidget(fkTableLabel, ++row, 0);
			AddWidget(fkTableNumeric, row, 1);
			AddWidget(getResourceElementGenericDveLinkerTableButton, ++row, 0);
			AddWidget(fixResourceElementGenericDveLinkerTableButton, ++row, 0);

			AddWidget(deleteResourcesHeader, ++row, 0, 1, 5);
			AddWidget(deleteResourcesButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			foreach (var resultSection in resultSections)
			{
				AddSection(resultSection, ++row, 0);
				row += resultSection.RowCount;
			}
		}
	}
}
