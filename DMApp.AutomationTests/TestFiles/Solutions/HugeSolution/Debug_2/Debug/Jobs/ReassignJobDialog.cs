using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.Library.Solutions.SRM;
using Skyline.DataMiner.Net.Messages.SLDataGateway;
using Skyline.DataMiner.Net.ResourceManager.Objects;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Jobs
{
	public class ReassignJobDialog : DebugDialog
	{
		private readonly Label titleLabel = new Label("Reassign Job") { Style = TextStyle.Heading };

		private TextBox originalJobIdTextBox = new TextBox() { PlaceHolder = "old job id", Width = 300 };
		private TextBox newJobIdTextBox = new TextBox() { PlaceHolder = "new job id", Width = 300 };
		private Button retrieveReservationsButton = new Button("Retrieve Reservations with Old Job ID");
		private Button updateReservationsButton = new Button("Reassign Reservations");
		private Label foundReservationsLabel = new Label(String.Empty);
		private TextBox reservationsTextBox = new TextBox { IsMultiline = true, Height = 400 };
		private TextBox statusTextBox = new TextBox { IsMultiline = true, Height = 400 };

		private List<ReservationInstance> reservations = new List<ReservationInstance>();

		public ReassignJobDialog(Helpers helpers) : base(helpers)
		{
			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			retrieveReservationsButton.Pressed += (s, e) =>
			{
				reservations.Clear();
				reservationsTextBox.Text = String.Empty;

				if (!Guid.TryParse(originalJobIdTextBox.Text, out Guid originalJobId))
				{
					foundReservationsLabel.Text = "Invalid GUID";
					return;
				}

				reservations = SrmManagers.ResourceManager.GetReservationInstances(ReservationInstanceExposers.Properties.DictStringField(LiteOrder.PropertyNameEventId).Equal(originalJobIdTextBox.Text)).ToList();

				foundReservationsLabel.Text = $"Found {reservations.Count} reservations";
				reservationsTextBox.Text = String.Join(Environment.NewLine, reservations.Select(x => $"{x.Name} [{x.ID}]"));
			};

			updateReservationsButton.Pressed += (s, e) =>
			{
				if (!reservations.Any()) return;
				if (!Guid.TryParse(newJobIdTextBox.Text, out Guid newJobId))
				{
					statusTextBox.Text = "Invalid GUID";
					return;
				}

				var newEvent = helpers.EventManager.GetEvent(newJobId);
				if (newEvent == null)
				{
					statusTextBox.Text = $"Event with ID {newJobId} does not exist";
					return;
				}

				statusTextBox.Text = $"Updating {reservations.Count} orders...";
				foreach (var reservation in reservations)
				{
					try
					{
						statusTextBox.Text += $"{Environment.NewLine}Moving order {reservation.Name}...";

						var liteOrder = helpers.OrderManager.GetLiteOrder(reservation, true);
						newEvent.AddOrUpdateOrder(liteOrder, helpers, true);

						statusTextBox.Text += $"{Environment.NewLine}Moving order succeeded";
					}
					catch(Exception exception)
					{
						statusTextBox.Text += $"{Environment.NewLine}Moving order failed due to {exception}";
					}
				}

				reservations.Clear();
				reservationsTextBox.Text = String.Empty;
			};
		}

		private void GenerateUi()
		{
			int row = -1;

			AddWidget(BackButton, ++row, 0);

			AddWidget(titleLabel, ++row, 0);

			AddWidget(new Label("Old Job Id"), ++row, 0);
			AddWidget(originalJobIdTextBox, row, 1);

			AddWidget(retrieveReservationsButton, ++row, 0, 1, 2);

			AddWidget(foundReservationsLabel, ++row, 0, 1, 2);
			AddWidget(reservationsTextBox, ++row, 0, 1, 2);

			AddWidget(new Label("New Job Id"), ++row, 0);
			AddWidget(newJobIdTextBox, row, 1);

			AddWidget(updateReservationsButton, ++row, 0, 1, 2);

			AddWidget(statusTextBox, ++row, 0, 1, 2);

			AddResponseSections(++row);
		}
	}
}
