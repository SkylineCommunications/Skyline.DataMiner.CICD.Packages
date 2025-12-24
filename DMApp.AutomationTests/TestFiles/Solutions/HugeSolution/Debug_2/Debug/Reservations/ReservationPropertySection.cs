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

	public class ReservationPropertySection : Section
	{
		private readonly Helpers helpers;
		private readonly ReservationInstance reservationInstance;

		private readonly Label nameLabel = new Label();
		private readonly TextBox valueTextBox = new TextBox();
		private readonly Button updateButton = new Button("Update") { Width = 150 };

		public ReservationPropertySection(Helpers helpers, ReservationInstance reservationInstance, KeyValuePair<string, object> property)
		{
			this.helpers = helpers;
			this.reservationInstance = reservationInstance;

			nameLabel.Text = property.Key;
			valueTextBox.Text = Convert.ToString(property.Value);

			updateButton.Pressed += UpdateButton_Pressed;

			GenerateUi();
		}

		private void UpdateButton_Pressed(object sender, EventArgs e)
		{
			try
			{
				DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(helpers, reservationInstance, new Dictionary<string, object> { { nameLabel.Text, valueTextBox.Text } });
				valueTextBox.ValidationState = UIValidationState.Valid;
			}
			catch (Exception ex)
			{
				valueTextBox.ValidationState = UIValidationState.Invalid;
				valueTextBox.ValidationText = $"Unable to update property due to {ex}";

				helpers.Log(nameof(ReservationPropertySection), nameof(UpdateButton_Pressed), $"Unable to update property due to: {ex}");
			}
		}

		private void GenerateUi()
		{
			AddWidget(nameLabel, 0, 0, 1, 2);
			AddWidget(valueTextBox, 0, 2);
			AddWidget(updateButton, 0, 3);
		}
	}
}
