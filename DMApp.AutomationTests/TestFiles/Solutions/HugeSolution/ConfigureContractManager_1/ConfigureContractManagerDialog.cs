namespace ConfigureContractManager_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Automation;

	internal class ConfigureContractManagerDialog : Dialog
	{
		private int tableIdToPoll = ContractManagerProtocol.UsersTableID;
		private Element element;
		private readonly Helpers helpers;
		private readonly Dictionary<string, int> objectKeys = new Dictionary<string, int>();
		private IReadOnlyDictionary<int, string> columnIdxToColumnName = ContractManagerProtocol.NotificationParamsIDX;
		private IDictionary<string, object[]> table;

		private readonly Label infoLabel = new Label("Info:") { Style = TextStyle.Bold };
		private readonly Label tableToConfigureLabel = new Label("Table to Configure");
		private readonly Button updateButton = new Button("Update");
		private readonly Button getSelectedInfoButton = new Button("Get Selected Info");
		private readonly Label rowToConfigureLabel = new Label();
		private DropDown allObjectsDropdown;
		private readonly DropDown typeOfConfigDropdown = new DropDown(EnumExtensions.GetEnumDescriptions<TypeToConfigure>());
		private readonly List<UsersGroupWidgets> allWidgetsToSet = new List<UsersGroupWidgets>();

		public ConfigureContractManagerDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Configure Contract Manager";
			this.helpers = helpers;
			Initialize();
			GenerateUI();
		}

		private void Initialize()
		{
			element = helpers.Engine.FindElementsByProtocol(ContractManagerProtocol.ProtocolName).SingleOrDefault() ?? throw new ElementNotFoundException(ContractManagerProtocol.ProtocolName);
			rowToConfigureLabel.Text = $"{typeOfConfigDropdown.Selected.Trim('s')} to Configure";
			allObjectsDropdown = new DropDown();
			UpdateObjectsDropdown();
			typeOfConfigDropdown.Changed += TypeOfConfig_Changed;
			getSelectedInfoButton.Pressed += GetObjectInfo_Pressed;
			updateButton.Pressed += UpdateButton_Pressed;
		}

		private void TypeOfConfig_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			try
			{
				objectKeys.Clear();
				allWidgetsToSet.Clear();
				var selectedType = typeOfConfigDropdown.Selected.GetEnumValue<TypeToConfigure>();
				tableIdToPoll = (selectedType == TypeToConfigure.User) ? ContractManagerProtocol.UsersTableID : ContractManagerProtocol.UsersGroupTableID;
				columnIdxToColumnName = (selectedType == TypeToConfigure.User) ? ContractManagerProtocol.NotificationParamsIDX : ContractManagerProtocol.UserGroupParamsIDX;
				rowToConfigureLabel.Text = $"{typeOfConfigDropdown.Selected.Trim('s')} to Configure";
				UpdateObjectsDropdown();
				GenerateUI();
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(Script), nameof(GetObjectInfo_Pressed), ex.ToString());
			}
		}

		private void UpdateObjectsDropdown()
		{
			table = element.GetTable(helpers.Engine, tableIdToPoll);
			var objectsFromTable = new List<string>();
			foreach (var value in table.Values)
			{
				objectKeys.Add(Convert.ToString(value[1]), Convert.ToInt32(value[0]));
				objectsFromTable.Add(Convert.ToString(value[1]));
			}
			allObjectsDropdown.SetOptions(objectsFromTable);
		}

		private void GetObjectInfo_Pressed(object sender, EventArgs e)
		{
			try
			{
				allWidgetsToSet.Clear();
				int userKey = objectKeys[allObjectsDropdown.Selected];
				var userRow = table[userKey.ToString()];
				var selectedType = typeOfConfigDropdown.Selected.GetEnumValue<TypeToConfigure>();
				foreach (var idx in columnIdxToColumnName)
				{
					var labelAndCheckbox = new UsersGroupWidgets();
					labelAndCheckbox.Label.Text = idx.Value;
					bool isUserTextParam = ((idx.Key == ContractManagerProtocol.UsersEmailIDX || idx.Key == ContractManagerProtocol.UsersPhoneIDX) && selectedType == TypeToConfigure.User);
					bool isUsersGroupTextParam = ((idx.Key == ContractManagerProtocol.CompanyIdx || idx.Key == ContractManagerProtocol.GroupCustomNameIdx) && selectedType == TypeToConfigure.UserGroup);
					if (isUserTextParam || isUsersGroupTextParam)
					{
						labelAndCheckbox.InteractiveWidget = new TextBox(Convert.ToString(userRow[idx.Key]));
					}
					else
					{
						labelAndCheckbox.InteractiveWidget = new CheckBox { IsChecked = Convert.ToBoolean(userRow[idx.Key]) };
					}
					allWidgetsToSet.Add(labelAndCheckbox);
				}

				GenerateUI();
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(Script), nameof(GetObjectInfo_Pressed), ex.ToString());
			}
		}

		private void UpdateButton_Pressed(object sender, EventArgs e)
		{
			int userKey = objectKeys[allObjectsDropdown.Selected];
			var userRow = table[userKey.ToString()];
			foreach (var idx in columnIdxToColumnName)
			{
				var labelCheckboxPair = allWidgetsToSet.Single(x => x.Label.Text.Equals(idx.Value));
				userRow[idx.Key] = labelCheckboxPair.WidgetValue;
			}
			element.SetRow((Engine)helpers.Engine, tableIdToPoll, userKey.ToString(), userRow.ToArray());
			allWidgetsToSet.Clear();
			Thread.Sleep(1000);
			table = element.GetTable(helpers.Engine, tableIdToPoll);
			GenerateUI();
		}

		private void GenerateUI()
		{
			Clear();

			int row = 0;

			AddWidget(tableToConfigureLabel, ++row, 0);
			AddWidget(typeOfConfigDropdown, row, 1);

			AddWidget(rowToConfigureLabel, ++row, 0);
			AddWidget(allObjectsDropdown, new WidgetLayout(row, 1));

			AddWidget(getSelectedInfoButton, new WidgetLayout(++row, 0));

			if (allWidgetsToSet.Any())
			{
				AddWidget(infoLabel, ++row, 0, 1, 2);
				foreach (var item in allWidgetsToSet)
				{
					AddWidget(item.Label, ++row, 0);
					AddWidget(item.InteractiveWidget, row, 1);
				}
				AddWidget(updateButton, row + 1, 0);
			}
		}
	}
}
