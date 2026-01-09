namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.ServiceConfigurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;

	public class OrderHistoryDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label findServiceConfigurationsLabel = new Label("Find Service Configuration") { Style = TextStyle.Heading };

		private readonly Label orderNameLabel = new Label("Order Name");
		private readonly TextBox orderNameTextBox = new TextBox(string.Empty);
		private readonly Button findByNameButton = new Button("Find by Name") { Width = 150 };

		private readonly Label orderIdLabel = new Label("Order ID");
		private readonly TextBox orderIdTextBox = new TextBox(string.Empty);
		private readonly Button findByIdButton = new Button("Find by ID") { Width = 150 };
		private readonly Button enterCurrentIdButton = new Button("Enter Current ID") { Width = 200 };

		private readonly List<RequestResultSection> responseSections = new List<RequestResultSection>();

		public OrderHistoryDialog(Helpers helpers) : base(helpers.Engine)
		{
			this.helpers = helpers;

			Title = "Order History";

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private void Initialize()
		{
			findByNameButton.Pressed += FindByNameButton_Pressed;

			findByIdButton.Pressed += FindByIdButton_Pressed;

			enterCurrentIdButton.Pressed += (sender, args) => orderIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;
		}

		private void FindByIdButton_Pressed(object sender, EventArgs e)
		{
			if (!Guid.TryParse(orderIdTextBox.Text, out var guid)) return;

			string serializedOrderHistory = string.Empty;
			var deserializedOrderHistory = new List<OrderHistoryChapter>();

			try
			{
				serializedOrderHistory = helpers.OrderManagerElement.GetSerializedOrderHistory(guid);

				deserializedOrderHistory = JsonConvert.DeserializeObject<List<OrderHistoryChapter>>(serializedOrderHistory);
			}
			catch(Exception ex)
			{
				serializedOrderHistory = ex.Message;
			}

			ShowRequestResult($"Order {guid} history (serialized)", $"'{serializedOrderHistory}'");

			ShowRequestResult($"Order {guid} history (deserialized)", $"'{JsonConvert.SerializeObject(deserializedOrderHistory)}'");
		}

		private void FindByNameButton_Pressed(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(orderNameTextBox.Text))
			{
				orderNameTextBox.Text = String.Empty;
				orderNameTextBox.ValidationText = "Fill out the Name of a reservation";
				orderNameTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			var reservationInstances = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(ReservationInstanceExposers.Name.Contains(orderNameTextBox.Text)));
			if (!reservationInstances.Any())
			{
				orderNameTextBox.Text = String.Empty;
				orderNameTextBox.ValidationText = "Unable to find a reservation with the specified Name";
				orderNameTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			var reservationInstance = reservationInstances.First();

			orderNameTextBox.Text = reservationInstance.Name;
			orderIdTextBox.Text = reservationInstance.ID.ToString();
			orderNameTextBox.ValidationState = UIValidationState.Valid;

			ShowRequestResult($"Order {reservationInstance.Name} [{reservationInstance.ID}] history", $"'{helpers.OrderManagerElement.GetSerializedOrderHistory(reservationInstance.ID)}'");
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

			AddWidget(findServiceConfigurationsLabel, ++row, 0, 1, 5);

			AddWidget(orderNameLabel, ++row, 0, 1, 2);
			AddWidget(orderNameTextBox, row, 2, 1, 2);
			AddWidget(findByNameButton, row, 4);

			AddWidget(orderIdLabel, ++row, 0, 1, 2);
			AddWidget(orderIdTextBox, row, 2, 1, 2);
			AddWidget(findByIdButton, row, 4);
			AddWidget(enterCurrentIdButton, ++row, 2, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 0);

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
