namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.ServiceConfigurations
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

	public class AddOrUpdateServiceConfigurationsDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label addOrUpdateServiceConfigurationsLabel = new Label("Add or Update Service Configuration") { Style = TextStyle.Heading };

		private readonly Label orderIdLabel = new Label("Order ID");
		private readonly TextBox orderIdTextBox = new TextBox(string.Empty);
		private readonly Button enterCurrentIdButton = new Button("Enter Current ID") { Width = 200 };

		private readonly Label orderEndLabel = new Label("Order End");
		private readonly DateTimePicker orderEndDateTimePicker = new DateTimePicker(DateTime.Now);
		private readonly Label timeZoneLabel = new Label($"Times are displayed here in the time zone of the client.");

		private readonly Label serviceConfigurationsLabel = new Label("Service Configurations");
		private readonly TextBox serviceConfigurationsTextBox = new TextBox { IsMultiline = true };

		private readonly Button addOrUpdateButton = new Button("Add or Update");

		private readonly List<RequestResultSection> responseSections = new List<RequestResultSection>();

		public AddOrUpdateServiceConfigurationsDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Service Configurations";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private void Initialize()
		{
			enterCurrentIdButton.Pressed += (sender, args) => orderIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;

			addOrUpdateButton.Pressed += AddOrUpdateButton_Pressed;
		}

		private void AddOrUpdateButton_Pressed(object sender, EventArgs e)
		{
			if (!Guid.TryParse(orderIdTextBox.Text, out Guid orderId))
			{
				ShowRequestResult("Invalid Guid", "Invalid Guid");
			}

			bool successful = helpers.OrderManagerElement.AddOrUpdateServiceConfigurations(orderId, orderEndDateTimePicker.DateTime, serviceConfigurationsTextBox.Text);

			ShowRequestResult($"Update {(successful ? "succeeded" : "failed")} for ID {orderId}", $"Update {(successful ? "succeeded" : "failed")} for ID {orderId}");
		}

		private void ShowRequestResult(string header, params string[] results)
		{
			responseSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(addOrUpdateServiceConfigurationsLabel, ++row, 0, 1, 5);

			AddWidget(orderIdLabel, ++row, 0, 1, 2);
			AddWidget(orderIdTextBox, row, 2, 1, 2);
			AddWidget(enterCurrentIdButton, ++row, 2, 1, 2);

			AddWidget(orderEndLabel, ++row, 0, 1, 2);
			AddWidget(orderEndDateTimePicker, row, 2, 1, 2);
			AddWidget(timeZoneLabel, ++row, 2, 1, 2);

			AddWidget(serviceConfigurationsLabel, ++row, 0, 1, 2);
			AddWidget(serviceConfigurationsTextBox, row, 2, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(addOrUpdateButton, ++row, 0, 1, 2);

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
