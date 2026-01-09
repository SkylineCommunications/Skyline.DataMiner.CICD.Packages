namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Integrations
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.ServiceConfigurations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public class RetriggerIntegrationUpdatesDialog : Dialog
	{
		private enum TypeOfId
		{
			[Description("Key")] Key,
			[Description("ID")] Id,
			[Description("Order ID")] OrderId
		}

		private enum DateTimeFilterOptions
		{
			[Description("Less Than")] LessThan,
			[Description("Less Than or Equal")] LessThanOrEqual,
			[Description("Greater Than")] GreaterThan,
			[Description("Greater Than or Equal")] GreaterThanOrEqual,
			[Description("Within Range")] WithinRange
		}

		private readonly Helpers helpers;

		private readonly YleCollapseButton basedOnIdCollapseButton = new YleCollapseButton(true);
		private readonly Label basedOnIdLabel = new Label("Based on ID"){Style = TextStyle.Heading};
		
		private readonly Label typeOfIdLabel = new Label("Type of ID");
		private readonly RadioButtonList typeOfIdRadioButtonList = new RadioButtonList(EnumExtensions.GetEnumDescriptions<TypeOfId>(), TypeOfId.Key.GetDescription());
		private readonly Label idLabel = new Label("Key");
		private readonly TextBox idTextBox = new TextBox(string.Empty);
		private readonly Button enterCurrentOrderIdButtton = new Button("Enter Current Order ID"){Width = 200, IsVisible = false};

		private readonly Button retriggerBasedIdButton = new Button("Retrigger Based on ID"){Width = 200};

		private readonly YleCollapseButton basedOnFiltersCollapseButton = new YleCollapseButton(true);
		private readonly Label basedOnFiltersLabel = new Label("Based on Filters") {Style = TextStyle.Heading};

		private readonly CheckBox integrationTypesCheckBox = new CheckBox("Integration Types");
		private readonly CheckBoxList integrationTypesCheckBoxList = new CheckBoxList(EnumExtensions.GetEnumDescriptions<IntegrationType>().Except(new []{IntegrationType.None.GetDescription()}));

		private readonly CheckBox orderStartCheckBox = new CheckBox("Order Start");
		private readonly DropDown orderStartFilterDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<DateTimeFilterOptions>());
		private readonly DateTimePicker orderStartDateTimePicker = new DateTimePicker(DateTime.Now);
		private readonly Label orderStartUntilLabel = new Label("until") { IsVisible = false };
		private readonly DateTimePicker orderStartSecondDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(1)) {IsVisible = false};

		private readonly CheckBox orderEndCheckBox = new CheckBox("Order End");
		private readonly DropDown orderEndFilterDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<DateTimeFilterOptions>());
		private readonly DateTimePicker orderEndDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(1));
		private readonly Label orderEndUntilLabel = new Label("until"){IsVisible = false};
		private readonly DateTimePicker orderEndSecondDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(2)) {IsVisible = false};

		private readonly CheckBox orderExistsCheckBox = new CheckBox("Order Exists");
		private readonly DropDown orderExistsDropDown = new DropDown(new[] {"Yes", "No"});

		private readonly CheckBox processedAtCheckBox = new CheckBox("Processed At");
		private readonly DropDown processedAtFilterDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<DateTimeFilterOptions>());
		private readonly DateTimePicker processedAtDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(1));
		private readonly Label processedAtUntilLabel = new Label("until") { IsVisible = false };
		private readonly DateTimePicker processedAtSecondDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(2)) {IsVisible = false};

		private readonly Label limitAmountOfRetriggersLabel = new Label("Limit Amount of sets to");
		private readonly Numeric limitAmountOfRetriggersNumeric = new Numeric(20){Minimum = 0, StepSize = 1, Decimals = 0};

		private readonly Button retriggerBasedOnFiltersButton = new Button("Retrigger Based on Filters") {Width = 200};

		private readonly List<RequestResultSection> resultSections = new List<RequestResultSection>();

		public RetriggerIntegrationUpdatesDialog(Helpers helpers) : base(helpers.Engine)
		{
			this.helpers = helpers;

			Title = "Retrigger Integration Updates";

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back..."){Width = 150};

		private void Initialize()
		{
			basedOnIdCollapseButton.Pressed += BasedOnIdCollapseButton_Pressed;
			BasedOnIdCollapseButton_Pressed(null, EventArgs.Empty);

			typeOfIdRadioButtonList.Changed += (sender, args) =>
			{
				enterCurrentOrderIdButtton.IsVisible = args.SelectedValue == TypeOfId.OrderId.GetDescription();
				idLabel.Text = TypeOfId.OrderId.GetDescription();
			};

			enterCurrentOrderIdButtton.Pressed += (sender, args) => idTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;
			retriggerBasedIdButton.Pressed += RetriggerBasedIdButton_Pressed;

			basedOnFiltersCollapseButton.Pressed += BasedOnFiltersCollapseButton_Pressed;
			BasedOnFiltersCollapseButton_Pressed(null, EventArgs.Empty);

			orderStartFilterDropDown.Changed += (sender, args) =>
			{
				orderStartSecondDateTimePicker.IsVisible = args.Selected == DateTimeFilterOptions.WithinRange.GetDescription();
				orderStartUntilLabel.IsVisible = orderStartSecondDateTimePicker.IsVisible;
			};

			orderEndFilterDropDown.Changed += (sender, args) =>
			{
				orderEndSecondDateTimePicker.IsVisible = args.Selected == DateTimeFilterOptions.WithinRange.GetDescription();
				orderEndUntilLabel.IsVisible = orderEndSecondDateTimePicker.IsVisible;
			};

			processedAtFilterDropDown.Changed += (sender, args) =>
			{
				processedAtSecondDateTimePicker.IsVisible = args.Selected == DateTimeFilterOptions.WithinRange.GetDescription();
				processedAtUntilLabel.IsVisible = processedAtSecondDateTimePicker.IsVisible;
			};

			retriggerBasedOnFiltersButton.Pressed += RetriggerBasedOnFiltersButton_Pressed;
		}

		private void BasedOnIdCollapseButton_Pressed(object sender, EventArgs e)
		{
			var basedOnIdWidgets = new Widget[] { typeOfIdLabel, typeOfIdRadioButtonList, idLabel, idTextBox, enterCurrentOrderIdButtton, retriggerBasedIdButton };

			foreach (var widget in basedOnIdWidgets)
			{
				widget.IsVisible = !basedOnIdCollapseButton.IsCollapsed;
			}

			enterCurrentOrderIdButtton.IsVisible = !basedOnIdCollapseButton.IsCollapsed && typeOfIdRadioButtonList.Selected == TypeOfId.OrderId.GetDescription();
		}

		private void BasedOnFiltersCollapseButton_Pressed(object sender, EventArgs e)
		{
			var basedOnFiltersWidgets = new Widget[] { integrationTypesCheckBox, integrationTypesCheckBoxList, orderStartCheckBox, orderStartFilterDropDown, orderStartDateTimePicker, orderStartUntilLabel, orderStartSecondDateTimePicker, orderEndCheckBox, orderEndFilterDropDown, orderEndDateTimePicker, orderEndUntilLabel, orderEndSecondDateTimePicker, processedAtCheckBox, processedAtFilterDropDown, processedAtDateTimePicker, processedAtUntilLabel, processedAtSecondDateTimePicker, orderExistsCheckBox, orderExistsDropDown, retriggerBasedOnFiltersButton, limitAmountOfRetriggersLabel, limitAmountOfRetriggersNumeric };

			foreach (var widget in basedOnFiltersWidgets)
			{
				widget.IsVisible = !basedOnFiltersCollapseButton.IsCollapsed;
			}

			orderStartSecondDateTimePicker.IsVisible = !basedOnFiltersCollapseButton.IsCollapsed && orderStartFilterDropDown.Selected == DateTimeFilterOptions.WithinRange.GetDescription();
			orderStartUntilLabel.IsVisible = orderStartSecondDateTimePicker.IsVisible;

			orderEndSecondDateTimePicker.IsVisible = !basedOnFiltersCollapseButton.IsCollapsed && orderEndFilterDropDown.Selected == DateTimeFilterOptions.WithinRange.GetDescription();
			orderEndUntilLabel.IsVisible = orderEndSecondDateTimePicker.IsVisible;

			processedAtSecondDateTimePicker.IsVisible = !basedOnFiltersCollapseButton.IsCollapsed && processedAtFilterDropDown.Selected == DateTimeFilterOptions.WithinRange.GetDescription();
			processedAtUntilLabel.IsVisible = processedAtSecondDateTimePicker.IsVisible;
		}

		private void RetriggerBasedIdButton_Pressed(object sender, EventArgs e)
		{
			var table = GetIntegrationUpdatesTable();

			Func<IntegrationUpdateRow, bool> predicate = row => false;

			switch (typeOfIdRadioButtonList.Selected.GetEnumValue<TypeOfId>())
			{
				case TypeOfId.Key:
					predicate = row => row.Key == idTextBox.Text;
					break;
				case TypeOfId.Id:
					predicate = row => row.ID == idTextBox.Text;
					break;
				case TypeOfId.OrderId:
					predicate = row => row.OrderId == idTextBox.Text;
					break;
				default:
					// no predicate change required
					break;
			}

			var keysToRetrigger = table.Values.Where(predicate).Select(row => row.Key).ToArray();

			helpers.OrderManagerElement.ReprocessIntegrationOrders(keysToRetrigger);

			ShowResult($"Retriggered Integration Updates {DateTime.Now}", $"ID = '{idTextBox.Text}'\nRetriggered Integrations:\n{(keysToRetrigger.Any() ? string.Join("\n", keysToRetrigger) : "None")}");
		}

		private void RetriggerBasedOnFiltersButton_Pressed(object sender, EventArgs e)
		{
			var table = GetIntegrationUpdatesTable();

			var keysToRetrigger = new List<string>();

			foreach (var row in table.Values)
			{
				bool retriggerThisRow = true;

				if (integrationTypesCheckBox.IsChecked)
				{
					retriggerThisRow &= integrationTypesCheckBoxList.Checked.Contains(row.Integration.GetDescription());
				}

				retriggerThisRow = CheckOrderStartCheckBox(row, retriggerThisRow);
				retriggerThisRow = CheckOrderEndCheckBox(row, retriggerThisRow);

				if (orderExistsCheckBox.IsChecked)
				{
					bool validGuid = Guid.TryParse(row.OrderId, out var guid);
					retriggerThisRow &= (validGuid && orderExistsDropDown.Selected.Equals("yes", StringComparison.InvariantCultureIgnoreCase)) || (!validGuid && orderExistsDropDown.Selected.Equals("no", StringComparison.InvariantCultureIgnoreCase));
				}

				retriggerThisRow = CheckProcessedAtTime(row, retriggerThisRow);

				if (retriggerThisRow)
				{
					keysToRetrigger.Add(row.Key);
				}
			}

			int totalMatchingRows = keysToRetrigger.Count;

			keysToRetrigger = keysToRetrigger.Take((int) limitAmountOfRetriggersNumeric.Value).ToList();

			helpers.OrderManagerElement.ReprocessIntegrationOrders(keysToRetrigger.ToArray());

			ShowResult($"Retriggered Integration Updates {DateTime.Now}", $"Filters:\n{FiltersToString()}\nRetriggered Integrations (limited to {(int)limitAmountOfRetriggersNumeric.Value}/{totalMatchingRows}):\n{(keysToRetrigger.Any() ? string.Join("\n", keysToRetrigger) : "None")}");
		}

		private bool CheckOrderStartCheckBox(IntegrationUpdateRow row, bool retriggerThisRow)
		{
			if (!orderStartCheckBox.IsChecked) return retriggerThisRow;

			switch (orderStartFilterDropDown.Selected.GetEnumValue<DateTimeFilterOptions>())
			{
				case DateTimeFilterOptions.GreaterThan:
					retriggerThisRow &= row.Start > orderStartDateTimePicker.DateTime;
					break;
				case DateTimeFilterOptions.GreaterThanOrEqual:
					retriggerThisRow &= row.Start >= orderStartDateTimePicker.DateTime;
					break;
				case DateTimeFilterOptions.LessThan:
					retriggerThisRow &= row.Start < orderStartDateTimePicker.DateTime;
					break;
				case DateTimeFilterOptions.LessThanOrEqual:
					retriggerThisRow &= row.Start <= orderStartDateTimePicker.DateTime;
					break;
				case DateTimeFilterOptions.WithinRange:
					retriggerThisRow &= orderStartDateTimePicker.DateTime <= row.Start && row.Start <= orderStartSecondDateTimePicker.DateTime;
					break;
				default:
					// nothing to check
					break;
			}

			return retriggerThisRow;
		}

		private bool CheckOrderEndCheckBox(IntegrationUpdateRow row, bool retriggerThisRow)
		{
			if (!orderEndCheckBox.IsChecked) return retriggerThisRow;

			switch (orderEndFilterDropDown.Selected.GetEnumValue<DateTimeFilterOptions>())
			{
				case DateTimeFilterOptions.GreaterThan:
					retriggerThisRow &= row.End > orderEndDateTimePicker.DateTime;
					break;
				case DateTimeFilterOptions.GreaterThanOrEqual:
					retriggerThisRow &= row.End >= orderEndDateTimePicker.DateTime;
					break;
				case DateTimeFilterOptions.LessThan:
					retriggerThisRow &= row.End < orderEndDateTimePicker.DateTime;
					break;
				case DateTimeFilterOptions.LessThanOrEqual:
					retriggerThisRow &= row.End <= orderEndDateTimePicker.DateTime;
					break;
				case DateTimeFilterOptions.WithinRange:
					retriggerThisRow &= orderEndDateTimePicker.DateTime <= row.End && row.End <= orderEndSecondDateTimePicker.DateTime;
					break;
				default:
					// nothing to check
					break;
			}

			return retriggerThisRow;
		}

		private bool CheckProcessedAtTime(IntegrationUpdateRow row, bool retriggerThisRow)
		{
			if (!processedAtCheckBox.IsChecked) return retriggerThisRow;

			if (row.ProcessedAt.HasValue)
			{
				switch (processedAtFilterDropDown.Selected.GetEnumValue<DateTimeFilterOptions>())
				{
					case DateTimeFilterOptions.GreaterThan:
						retriggerThisRow &= row.ProcessedAt > processedAtDateTimePicker.DateTime;
						break;
					case DateTimeFilterOptions.GreaterThanOrEqual:
						retriggerThisRow &= row.ProcessedAt >= processedAtDateTimePicker.DateTime;
						break;
					case DateTimeFilterOptions.LessThan:
						retriggerThisRow &= row.ProcessedAt < processedAtDateTimePicker.DateTime;
						break;
					case DateTimeFilterOptions.LessThanOrEqual:
						retriggerThisRow &= row.ProcessedAt <= processedAtDateTimePicker.DateTime;
						break;
					case DateTimeFilterOptions.WithinRange:
						retriggerThisRow &= processedAtDateTimePicker.DateTime <= row.ProcessedAt && row.ProcessedAt <= processedAtSecondDateTimePicker.DateTime;
						break;
					default:
						// nothing to check
						break;
				}
			}
			else
			{
				retriggerThisRow = false;
			}

			return retriggerThisRow;
		}

		private string FiltersToString()
		{
			var sb = new StringBuilder();

			if (integrationTypesCheckBox.IsChecked)
			{
				sb.Append($"Integrations = {string.Join(",", integrationTypesCheckBoxList.Checked)}\n");
			}

			if (orderStartCheckBox.IsChecked)
			{
				sb.Append($"Order Start {orderStartFilterDropDown.Selected} {orderStartDateTimePicker.DateTime}");
				if (orderStartFilterDropDown.Selected == DateTimeFilterOptions.WithinRange.GetDescription())
				{
					sb.Append($" until {orderStartSecondDateTimePicker.DateTime}");
				}

				sb.Append("\n");
			}

			if (orderEndCheckBox.IsChecked)
			{
				sb.Append($"Order End {orderEndFilterDropDown.Selected} {orderEndDateTimePicker.DateTime}");
				if (orderEndFilterDropDown.Selected == DateTimeFilterOptions.WithinRange.GetDescription())
				{
					sb.Append($" until {orderEndSecondDateTimePicker.DateTime}");
				}

				sb.Append("\n");
			}

			if (orderExistsCheckBox.IsChecked)
			{
				sb.Append($"Order Exists = {orderExistsDropDown.Selected}\n");
			}

			if (processedAtCheckBox.IsChecked)
			{
				sb.Append($"Processed At {processedAtFilterDropDown.Selected} {processedAtDateTimePicker.DateTime}");
				if (processedAtFilterDropDown.Selected == DateTimeFilterOptions.WithinRange.GetDescription())
				{
					sb.Append($" until {processedAtSecondDateTimePicker.DateTime}");
				}

				sb.Append("\n");
			}

			return sb.ToString();
		}

		private void ShowResult(string header, params string[] results)
		{
			resultSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 5);

			AddWidget(basedOnIdCollapseButton, ++row, 0);
			AddWidget(basedOnIdLabel, row, 1, 1, 5);

			AddWidget(typeOfIdLabel, ++row, 0);
			AddWidget(typeOfIdRadioButtonList, row, 1, 1, 4);

			AddWidget(idLabel, ++row, 0);
			AddWidget(idTextBox, row, 1, 1, 3);
			AddWidget(enterCurrentOrderIdButtton, row, 4);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(retriggerBasedIdButton, ++row, 0, 1, 5);
			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(basedOnFiltersCollapseButton, ++row, 0);
			AddWidget(basedOnFiltersLabel, row, 1, 1, 5);

			AddWidget(integrationTypesCheckBox, ++row, 0);
			AddWidget(integrationTypesCheckBoxList, row, 1, 1, 3);

			AddWidget(orderStartCheckBox, ++row, 0);
			AddWidget(orderStartFilterDropDown, row, 1);
			AddWidget(orderStartDateTimePicker, row, 2);
			AddWidget(orderStartUntilLabel, row, 3);
			AddWidget(orderStartSecondDateTimePicker, row, 4);

			AddWidget(orderEndCheckBox, ++row, 0);
			AddWidget(orderEndFilterDropDown, row, 1);
			AddWidget(orderEndDateTimePicker, row, 2);
			AddWidget(orderEndUntilLabel, row, 3);
			AddWidget(orderEndSecondDateTimePicker, row, 4);

			AddWidget(orderExistsCheckBox, ++row, 0);
			AddWidget(orderExistsDropDown, row, 1, 1, 3);

			AddWidget(processedAtCheckBox, ++row, 0);
			AddWidget(processedAtFilterDropDown, row, 1);
			AddWidget(processedAtDateTimePicker, row, 2);
			AddWidget(processedAtUntilLabel, row, 3);
			AddWidget(processedAtSecondDateTimePicker, row, 4);

			AddWidget(limitAmountOfRetriggersLabel, ++row, 0);
			AddWidget(limitAmountOfRetriggersNumeric, row, 1, 1, 4);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(retriggerBasedOnFiltersButton, ++row, 0, 1, 5);
			AddWidget(new WhiteSpace(), ++row, 0);

			foreach (var resultSection in resultSections)
			{
				AddSection(resultSection, ++row, 0);
				row += resultSection.RowCount;
			}
		}

		private Dictionary<string, IntegrationUpdateRow> GetIntegrationUpdatesTable()
		{
			helpers.LogMethodStart(nameof(RetriggerIntegrationUpdatesDialog), nameof(GetIntegrationUpdatesTable), out var stopwatch);

			var objectTable = helpers.OrderManagerElement.GetIntegrationOrdersTable();

			var table = new Dictionary<string, IntegrationUpdateRow>();

			foreach (var pair in objectTable)
			{
				table.Add(pair.Key, new IntegrationUpdateRow(pair.Value));
			}

			helpers.Log(nameof(RetriggerIntegrationUpdatesDialog), nameof(GetIntegrationUpdatesTable), $"Found {table.Count} rows");

			helpers.LogMethodCompleted(nameof(RetriggerIntegrationUpdatesDialog), nameof(GetIntegrationUpdatesTable),null, stopwatch);

			return table;
		}

		private sealed class IntegrationUpdateRow
		{
			public IntegrationUpdateRow(object[] row)
			{
				if (row == null) throw new ArgumentNullException(nameof(row));

				Key = Convert.ToString(row[0]);
				ID = Convert.ToString(row[1]);
				Integration = (IntegrationType)Convert.ToInt32(row[2]);
				Start = DateTime.FromOADate(Convert.ToDouble(row[4]));
				End = DateTime.FromOADate(Convert.ToDouble(row[5]));
				OrderId = Convert.ToString(row[8]);

				var processedAtValue = Convert.ToDouble(row[10]);
				ProcessedAt = processedAtValue > 0 ? DateTime.FromOADate(processedAtValue) : (DateTime?) null;
			}

			public string Key { get; set; }

			public string ID { get; set; }

			public IntegrationType Integration { get; set; }

			public DateTime Start { get; set; }

			public DateTime End { get; set; }

			public string OrderId { get; set; }

			public DateTime? ProcessedAt { get; set; }
		}
	}
}