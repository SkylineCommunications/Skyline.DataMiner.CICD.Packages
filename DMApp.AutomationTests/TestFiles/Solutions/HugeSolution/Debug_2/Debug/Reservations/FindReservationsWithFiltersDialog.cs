namespace Debug_2.Debug.Reservations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class FindReservationsWithFiltersDialog : DebugDialog
	{
		private readonly GetReservationsSection getReservationsSection;

		private readonly Label actionsLabel = new Label("Actions") { Style = TextStyle.Heading };

		private readonly Button showJsonButton = new Button("Show Reservations JSON");
		private readonly Button deleteButton = new Button("Delete Reservations");
		private readonly CheckBox deleteSafetyCheckbox = new CheckBox("Confirm Delete");
		private readonly Button getIdsButton = new Button("Get IDs");
		private readonly Button considerAsServiceAndCancelButton = new Button("Consider as Service and Cancel");
		private readonly CheckBox cancelSafetyCheckbox = new CheckBox("Confirm Cancel");
		private readonly Button deleteIfContributingIsNowhereAssignedButton = new Button("Delete if contributing resource is not assigned anywhere");
		private readonly CheckBox deleteIfContributingIsNowhereAssignedButtonConfirmCheckbox = new CheckBox("Confirm Delete");

		public FindReservationsWithFiltersDialog(Helpers helpers) : base(helpers)
		{
			Title = "Find Reservations with Filters";

			getReservationsSection = new GetReservationsSection(helpers);

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			getReservationsSection.RegenerateUiRequired += GetReservationsSection_RegenerateUi;

			showJsonButton.Pressed += ShowJsonButton_Pressed;

			deleteButton.Pressed += DeleteButton_Pressed;

			getIdsButton.Pressed += GetIdsButton_Pressed;

			considerAsServiceAndCancelButton.Pressed += ConsiderAsServiceAndCancelButton_Pressed;

			deleteIfContributingIsNowhereAssignedButton.Pressed += DeleteIfContributingIsNowhereAssignedButton_Pressed;
		}

		private void DeleteIfContributingIsNowhereAssignedButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (!deleteIfContributingIsNowhereAssignedButtonConfirmCheckbox.IsChecked) return;

				var contributingReservationsToRemove = new List<ReservationInstance>();

				foreach (var reservation in getReservationsSection.SelectedReservations)
				{
					var orderReservations = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(reservation.ID));

					if (!orderReservations.Any())
					{
						contributingReservationsToRemove.Add(reservation);
					}
				}

				DataMinerInterface.ResourceManager.RemoveReservationInstances(helpers, contributingReservationsToRemove.ToArray());

				ShowRequestResult("Removed contributing reservations", string.Join("\n", contributingReservationsToRemove.Select(r => r.Name)));
				GenerateUi();
			}
		}

		private void ConsiderAsServiceAndCancelButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (!cancelSafetyCheckbox.IsChecked) return;

				var messages = new List<string>();

				foreach (var reservation in getReservationsSection.SelectedReservations)
				{
					var service = Service.FromReservationInstance(helpers, reservation);

					var order = helpers.OrderManager.GetOrder(service.OrderReferences.First());

					service.Cancel(helpers, order);

					messages.Add($"{service.Name} part of order {order.Name}");
				}

				ShowRequestResult("Cancelled Services", string.Join("\n", messages));
				GenerateUi();
			}	
		}

		private void GetIdsButton_Pressed(object sender, EventArgs e)
		{
			ShowRequestResult("Reservation IDs", string.Join("\n", getReservationsSection.SelectedReservations.Select(r => r.ID)));
			GenerateUi();
		}

		private void DeleteButton_Pressed(object sender, EventArgs e)
		{
			if (!deleteSafetyCheckbox.IsChecked) return;

			DataMinerInterface.ResourceManager.RemoveReservationInstances(helpers, getReservationsSection.SelectedReservations.ToArray());

			ShowRequestResult("Removed Reservations", string.Join("\n", getReservationsSection.SelectedReservations.Select(r => r.Name)));
			GenerateUi();
		}

		private void GetReservationsSection_RegenerateUi(object sender, EventArgs e)
		{
			getReservationsSection.RegenerateUi();
			GenerateUi();
		}

		private void ShowJsonButton_Pressed(object sender, EventArgs e)
		{
			ShowRequestResult("Serialized Reservations", string.Join("\n", getReservationsSection.SelectedReservations.Select(r => JsonConvert.SerializeObject(r))));
			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddSection(getReservationsSection, new SectionLayout(++row, 0));
			row += getReservationsSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(actionsLabel, ++row, 0);
			AddWidget(showJsonButton, ++row, 0);
			AddWidget(deleteButton, ++row, 0);
			AddWidget(deleteSafetyCheckbox, row, 1);
			AddWidget(getIdsButton, ++row, 0);
			AddWidget(considerAsServiceAndCancelButton, ++row, 0);
			AddWidget(cancelSafetyCheckbox, row, 1);
			AddWidget(deleteIfContributingIsNowhereAssignedButton, ++row, 0);
			AddWidget(deleteIfContributingIsNowhereAssignedButtonConfirmCheckbox, row, 1);
			AddWidget(new WhiteSpace(), ++row, 0);

			AddResponseSections(row);
		}

		protected override void HandleEnabledUpdate()
		{
			base.HandleEnabledUpdate();

			getReservationsSection.IsEnabled = IsEnabled;
		}
	}
}
