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
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Net.Messages;

	public class GetServiceConfigurationsDialog : Dialog
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

		private readonly Button findReservationsWithMissingOrEmptyServiceConfigurations = new Button("Find Missing Service Configurations");
		private readonly Label fromLabel = new Label("from");
		private readonly DateTimePicker startDateTimePicker = new DateTimePicker(DateTime.UtcNow);
		private readonly Label untilLabel = new Label("until");
		private readonly DateTimePicker endDateTimePicker = new DateTimePicker(DateTime.UtcNow.AddDays(1));

		private readonly Button executeSetsOnOrderManagerElementButton = new Button("Load test for Order Manager Element [DCP208819]");
		private readonly Label amountOfSetsLabel = new Label("Amount of Sets");
		private readonly Numeric amountOfSetsNumeric = new Numeric();

		private readonly List<RequestResultSection> responseSections = new List<RequestResultSection>();

		public GetServiceConfigurationsDialog(Helpers helpers) : base(helpers.Engine)
		{
			this.helpers = helpers;

			Title = "Service Configurations";

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back..."){Width = 150};

		private void Initialize()
		{
			findByNameButton.Pressed += FindByNameButton_Pressed;

			findByIdButton.Pressed += FindByIdButton_Pressed;

			findReservationsWithMissingOrEmptyServiceConfigurations.Pressed += FindReservationsWithMissingOrEmptyServiceConfigurations_Pressed;

			executeSetsOnOrderManagerElementButton.Pressed += ExecuteSetsOnOrderManagerElementButton_Pressed;

			enterCurrentIdButton.Pressed += (sender, args) => orderIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;
		}

		private void ExecuteSetsOnOrderManagerElementButton_Pressed(object sender, EventArgs e)
		{
			var startTime = new DateTime(2020,1,1);
			var endTime = new DateTime(2024,1,1);

			var reservations = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, ReservationInstanceExposers.Start.GreaterThanOrEqual(startTime).AND(ReservationInstanceExposers.End.LessThanOrEqual(endTime)).AND(ReservationInstanceExposers.Properties.DictStringField("Type").Equal("Video"))).Take((int)amountOfSetsNumeric.Value / 7);

			var reservationsToLoopOver = reservations.Concat(reservations).Concat(reservations).Concat(reservations).Concat(reservations).Concat(reservations).Concat(reservations).ToList();

			var element = Engine.FindElement("Order Manager");

			var stopwatch = Stopwatch.StartNew();

			Parallel.ForEach<ReservationInstance>(reservationsToLoopOver, (reservation) => AutomationExtensions.TryStartScript(helpers, "RequestServiceConfigurationFromOrderManager", new Dictionary<int, string> { { 2, reservation.ID.ToString() } }, false, false));

			ShowRequestResult($"Executed {(int)amountOfSetsNumeric.Value} sets on order manager in {stopwatch.Elapsed}");
		}

		private void FindReservationsWithMissingOrEmptyServiceConfigurations_Pressed(object sender, EventArgs e)
		{
			var startTime = startDateTimePicker.DateTime;
			var endTime = endDateTimePicker.DateTime;

			var reservations = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, ReservationInstanceExposers.Start.GreaterThanOrEqual(startTime).AND(ReservationInstanceExposers.End.LessThanOrEqual(endTime)).AND(ReservationInstanceExposers.Properties.DictStringField("Type").Equal("Video")));

			var ordersWithInvalidServiceConfig = new List<ReservationInstance>();

			foreach (var orderReservation in reservations)
			{
				if (!helpers.OrderManagerElement.TryGetServiceConfigurations(orderReservation.ID, out var serviceConfigurations))
				{
					ordersWithInvalidServiceConfig.Add(orderReservation);
				}
			}

			ShowRequestResult($"Orders from {startTime} until {endTime} with invalid service configs", $"Checked {reservations.Count()} orders:\n{string.Join("\n", ordersWithInvalidServiceConfig.Select(o => o.Name))}", string.Join("\n", ordersWithInvalidServiceConfig.Select(o => o.ID)), string.Join("\n", ordersWithInvalidServiceConfig.Select(o => o.CreatedAt)));
		}

		private void FindByIdButton_Pressed(object sender, EventArgs e)
		{
			if (!Guid.TryParse(orderIdTextBox.Text, out var guid)) return;

			ShowRequestResult($"Order {guid} service configuration", $"{helpers.OrderManagerElement.GetSerializedServiceConfigurations(guid)}");
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

			ShowRequestResult($"Order {reservationInstance.Name} service configuration", $"{helpers.OrderManagerElement.GetSerializedServiceConfigurations(reservationInstance.ID)}");
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

			AddWidget(findReservationsWithMissingOrEmptyServiceConfigurations, ++row, 0, 1, 2);
			AddWidget(fromLabel, row, 2);
			AddWidget(startDateTimePicker, row, 3);
			AddWidget(untilLabel, ++row, 2);
			AddWidget(endDateTimePicker, row, 3);

			AddWidget(new WhiteSpace(), ++row, 1);

			AddWidget(executeSetsOnOrderManagerElementButton, ++row, 0, 1, 2);
			AddWidget(amountOfSetsLabel, row, 2);
			AddWidget(amountOfSetsNumeric, row, 3);

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
