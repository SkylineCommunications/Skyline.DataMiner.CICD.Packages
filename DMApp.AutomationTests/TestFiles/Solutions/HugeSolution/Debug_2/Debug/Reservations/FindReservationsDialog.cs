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
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class FindReservationsDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label findReservationsLabel = new Label("Find Reservation"){Style = TextStyle.Heading};
		private readonly Label reservationNameLabel = new Label("Name");
		private readonly Label reservationIdLabel = new Label("GUID");
		private readonly Button enterCurrentOrderIdButton = new Button("Enter Current ID") { Width = 200 };
		private readonly List<ReservationDetailsSection> responseSections = new List<ReservationDetailsSection>();

		public FindReservationsDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Find Reservation";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private TextBox ReservationNameTextBox { get; set; }

		private TextBox ReservationIdTextBox { get; set; }

		private Button FindByReservationNameButton { get; set; }

		private Button FindByReservationIdButton { get; set; }

		private void Initialize()
		{
			ReservationNameTextBox = new TextBox { PlaceHolder = "Name", Width = 400 };
			ReservationIdTextBox = new TextBox { PlaceHolder = "GUID", ValidationText = "Invalid GUID", Width = 400 };

			FindByReservationNameButton = new Button("Find By Name") { Width = 150 };
			FindByReservationNameButton.Pressed += FindByReservationNameButton_Pressed;

			FindByReservationIdButton = new Button("Find By GUID") { Width = 150 };
			FindByReservationIdButton.Pressed += FindByReservationIdButton_Pressed;

			enterCurrentOrderIdButton.Pressed += (sender, args) => ReservationIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(findReservationsLabel, ++row, 0, 1, 5);

			AddWidget(reservationNameLabel, ++row, 0, 1, 2);
			AddWidget(ReservationNameTextBox, row, 2);
			AddWidget(FindByReservationNameButton, row, 3);

			AddWidget(reservationIdLabel, ++row, 0, 1, 2);
			AddWidget(ReservationIdTextBox, row, 2);
			AddWidget(FindByReservationIdButton, row, 3);
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

		private void ShowOrderDetails(ReservationInstance reservationInstance, string serviceConfiguration)
		{
			var reservationDetailsSection = new ReservationDetailsSection(helpers, reservationInstance, serviceConfiguration);
			reservationDetailsSection.RegenerateUi += (o, e) => GenerateUi();

			responseSections.Add(reservationDetailsSection);

			GenerateUi();
		}

		private void FindByReservationNameButton_Pressed(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ReservationNameTextBox.Text))
			{
				ReservationIdTextBox.Text = String.Empty;
				ShowOrderDetails(null, null);
				ReservationNameTextBox.ValidationText = "Fill out the Name of a reservation";
				ReservationNameTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			var reservationInstances = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(ReservationInstanceExposers.Name.Contains(ReservationNameTextBox.Text)));
			if (!reservationInstances.Any())
			{
				ReservationIdTextBox.Text = String.Empty;
				ShowOrderDetails(null, null);
				ReservationNameTextBox.ValidationText = "Unable to find a reservation with the specified Name";
				ReservationNameTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			var reservationInstance = reservationInstances.First();

			ReservationNameTextBox.Text = reservationInstance.Name;
			ReservationIdTextBox.Text = reservationInstance.ID.ToString();
			ReservationNameTextBox.ValidationState = UIValidationState.Valid;

			helpers.OrderManagerElement.TryGetServiceConfigurations(reservationInstance.ID, out var serviceConfigurations);
			string serviceConfiguration = JsonConvert.SerializeObject(serviceConfigurations);

			ShowOrderDetails(reservationInstance, serviceConfiguration);
		}

		private void FindByReservationIdButton_Pressed(object sender, EventArgs e)
		{
			if (!Guid.TryParse(ReservationIdTextBox.Text, out var guid))
			{
				ReservationNameTextBox.Text = String.Empty;
				ShowOrderDetails(null, null);
				ReservationNameTextBox.ValidationText = "Unable to parse the GUID";
				ReservationIdTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			var reservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, guid);
			if (reservationInstance == null)
			{
				ReservationNameTextBox.Text = String.Empty;
				ShowOrderDetails(null, null);
				ReservationNameTextBox.ValidationText = "No reservations found with the given ID";
				ReservationIdTextBox.ValidationState = UIValidationState.Invalid;
				return;
			}

			ReservationNameTextBox.Text = reservationInstance.Name;
			ReservationIdTextBox.Text = reservationInstance.ID.ToString();
			ReservationIdTextBox.ValidationState = UIValidationState.Valid;

			helpers.OrderManagerElement.TryGetServiceConfigurations(reservationInstance.ID, out var serviceConfigurations);
			string serviceConfiguration = JsonConvert.SerializeObject(serviceConfigurations);

			ShowOrderDetails(reservationInstance, serviceConfiguration);
		}
	}
}