namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Reservations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Debug_2.Debug.Reservations;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class StopOrderNowDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label findReservationsLabel = new Label("Stop Order Now") { Style = TextStyle.Heading };
		private readonly Label orderNameLabel = new Label("Name");
		private readonly Label orderIdLabel = new Label("GUID");
		private readonly Button enterCurrentOrderIdButton = new Button("Enter Current ID") { Width = 200 };
		private readonly List<ResponseSection> responseSections = new List<ResponseSection>();

		public StopOrderNowDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Stop Order Now";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private TextBox OrderNameTextBox { get; set; }

		private TextBox OrderIdTextBox { get; set; }

		private Button FindByOrderNameButton { get; set; }

		private Button FindByOrderIdButton { get; set; }

		private void Initialize()
		{
			OrderNameTextBox = new TextBox { PlaceHolder = "Name", Width = 400 };
			OrderIdTextBox = new TextBox { PlaceHolder = "GUID", ValidationText = "Invalid GUID", Width = 400 };

			FindByOrderNameButton = new Button("Stop By Name") { Width = 150 };
			FindByOrderNameButton.Pressed += StopOrderByNameButton_Pressed;

			FindByOrderIdButton = new Button("Stop By GUID") { Width = 150 };
			FindByOrderIdButton.Pressed += StopOrderByIdButton_Pressed;

			enterCurrentOrderIdButton.Pressed += (sender, args) => OrderIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(findReservationsLabel, ++row, 0, 1, 5);

			AddWidget(orderNameLabel, ++row, 0, 1, 2);
			AddWidget(OrderNameTextBox, row, 2);
			AddWidget(FindByOrderNameButton, row, 3);

			AddWidget(orderIdLabel, ++row, 0, 1, 2);
			AddWidget(OrderIdTextBox, row, 2);
			AddWidget(FindByOrderIdButton, row, 3);
			AddWidget(enterCurrentOrderIdButton, ++row, 2);

			AddWidget(new WhiteSpace(), ++row, 1);

			row++;
			foreach (var responseSection in responseSections)
			{
				responseSection.Collapse();
				AddSection(responseSection, row, 0);
				row += responseSection.RowCount;
			}
		}

		private void ShowRequestResult(string header, params string[] results)
		{
			responseSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}

		private void StopOrderByNameButton_Pressed(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(OrderNameTextBox.Text))
			{
				OrderIdTextBox.Text = String.Empty;
				OrderNameTextBox.ValidationText = "Fill out the Name of a reservation";
				OrderNameTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			var reservationInstances = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(ReservationInstanceExposers.Name.Contains(OrderNameTextBox.Text)));
			if (!reservationInstances.Any())
			{
				OrderIdTextBox.Text = String.Empty;
				OrderNameTextBox.ValidationText = "Unable to find a reservation with the specified Name";
				OrderNameTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			var reservationInstance = reservationInstances.First();

			OrderNameTextBox.Text = reservationInstance.Name;
			OrderIdTextBox.Text = reservationInstance.ID.ToString();
			OrderNameTextBox.ValidationState = UIValidationState.Valid;

			var order = helpers.OrderManager.GetOrder(reservationInstance, false, true);

			order.StopNow = true;
			var tasks = order.StopOrderAndLinkedServices(helpers).Tasks;

			ShowRequestResult($"Stopping order {order.Name}", string.Join("\n", tasks.Select(t => $"{t.Description} {t.Status}")));
		}

		private void StopOrderByIdButton_Pressed(object sender, EventArgs e)
		{
			if (!Guid.TryParse(OrderIdTextBox.Text, out var guid))
			{
				OrderNameTextBox.Text = String.Empty;
				OrderNameTextBox.ValidationText = "Unable to parse the GUID";
				OrderIdTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			var reservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, guid);
			if (reservationInstance == null)
			{
				OrderNameTextBox.Text = String.Empty;
				OrderNameTextBox.ValidationText = "No reservations found with the given ID";
				OrderIdTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			OrderNameTextBox.Text = reservationInstance.Name;
			OrderIdTextBox.Text = reservationInstance.ID.ToString();
			OrderIdTextBox.ValidationState = UIValidationState.Valid;

			var order = helpers.OrderManager.GetOrder(reservationInstance, false, true);

			order.StopNow = true;
			var tasks = order.StopOrderAndLinkedServices(helpers).Tasks;

			ShowRequestResult($"Stopping order {order.Name}", string.Join("\n", tasks.Select(t => $"{t.Description} {t.Status}")));
		}
	}
}