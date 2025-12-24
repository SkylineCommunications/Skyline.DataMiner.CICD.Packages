namespace EditOrderTemplate_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;

	public class EditOrderTemplateDialog : Dialog
	{
		private readonly Helpers helpers;
		private readonly Element contractManagerElement;

		private readonly Dictionary<string, string> templatePrimaryKeysPerTemplateName;

		private readonly Label templateLabel = new Label("Template");
		private readonly DropDown templateDropDown;

		private readonly TextBox templateTextBox = new TextBox(string.Empty) { IsMultiline = true, Height = 500, Width = 1000 };
		private readonly Button confirmButton = new Button("Confirm");
		private readonly Label validationLabel = new Label(string.Empty);

		public EditOrderTemplateDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Edit Order Template";

			this.helpers = helpers;
			this.contractManagerElement = helpers.Engine.FindElementsByProtocol("Finnish Broadcasting Company Contract Manager").FirstOrDefault() ?? throw new ElementNotFoundException("Could not find Contract Manager Element");


			templatePrimaryKeysPerTemplateName = helpers.ContractManager.GetAllOrderTemplateNamesAndIds();

			var orderTemplateNames = new List<string> { Constants.None }.Concat(templatePrimaryKeysPerTemplateName.Keys.OrderBy(name => name));

			templateDropDown = new DropDown(orderTemplateNames, Constants.None) { IsDisplayFilterShown = true };
			templateDropDown.Changed += TemplateDropDown_Changed;

			confirmButton.Pressed += ConfirmButtonOnPressed;

			GenerateUi();
		}

		private void ConfirmButtonOnPressed(object sender, EventArgs e)
		{
			var newTemplate = OrderTemplate.Deserialize(templateTextBox.Text);

			if (!templateTextBox.IsEnabled || newTemplate == null)
			{
				validationLabel.Text = "Provide a valid template";
			}
			else
			{
				contractManagerElement.SetParameterByPrimaryKey(15004, templatePrimaryKeysPerTemplateName[templateDropDown.Selected], templateTextBox.Text);

				validationLabel.Text = "Update complete";
			}
		}

		private void TemplateDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			if (e.Selected == Constants.None)
			{
				templateTextBox.Text = string.Empty;
				templateTextBox.IsEnabled = false;
			}
			else if (helpers.ContractManager.TryGetOrderTemplate(e.Selected, out var template))
			{
				try
				{
					templateTextBox.Text = JsonConvert.SerializeObject(template, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Serialize });
					templateTextBox.IsEnabled = true;

				}
				catch (Exception ex)
				{
					templateTextBox.Text = ex.ToString();
					templateTextBox.IsEnabled = false;
				}
			}
			else
			{
				templateTextBox.Text = "Error while getting template";
				templateTextBox.IsEnabled = false;
			}
		}

		private void GenerateUi()
		{
			int row = -1;

			AddWidget(templateLabel, new WidgetLayout(++row, 0));
			AddWidget(templateDropDown, new WidgetLayout(row, 1));

			AddWidget(templateTextBox, new WidgetLayout(++row, 0, 1, 2));

			AddWidget(confirmButton, new WidgetLayout(++row, 0));
			AddWidget(validationLabel, new WidgetLayout(row + 1, 0));

			SetColumnWidth(0, 500);
			SetColumnWidth(1, 500);
		}
	}
}