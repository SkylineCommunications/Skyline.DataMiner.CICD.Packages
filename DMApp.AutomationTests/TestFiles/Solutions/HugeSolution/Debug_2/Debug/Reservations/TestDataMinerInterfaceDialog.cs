namespace Debug_2.Debug.Reservations
{
	using System;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class TestDataMinerInterfaceDialog : DebugDialog
	{
		private readonly Label header = new Label("Test DataMiner Interface") { Style = TextStyle.Heading };

		private readonly GetReservationsSection getReservationsSection;

		private readonly Label resourceManagerAddOrUpdateReservationInstancesLabel = new Label("ResourceManager.AddOrUpdateReservationInstances") { Style = TextStyle.Bold };
		private readonly Label securityViewIdsLabel = new Label("comma-separated security view IDs");
		private readonly TextBox securityViewIdsTextBox = new TextBox();
		private readonly Button resourceManagerAddOrUpdateReservationInstancesButton = new Button("Execute");


		private readonly Label bookingManagerChangeNameLabel = new Label("BookingManager.ChangeName") { Style = TextStyle.Bold };
		private readonly Label bookingManagerNameLabel = new Label("Booking Manager Name");
		private readonly TextBox bookingManagerTextBox = new TextBox();
		private readonly Label nameToSetLabel = new Label("New Reservation Name");
		private readonly TextBox nameToSetTextBox = new TextBox();
		private readonly Button bookingManagerChangeNameButton = new Button("Execute");

		private readonly Label reservationInstanceUpdateServiceReservationPropertiesLabel = new Label("ReservationInstance.UpdateServiceReservationProperties") { Style = TextStyle.Bold };
		private readonly Label propertyNameLabel = new Label("Property Name");
		private readonly TextBox propertyNameTextBox = new TextBox();
		private readonly Label propertyValueLabel = new Label("New Property Name");
		private readonly TextBox propertyValueTextBox = new TextBox();
		private readonly Button reservationInstanceUpdateServiceReservationPropertiesButton = new Button("Execute");

		public TestDataMinerInterfaceDialog(Helpers helpers) : base(helpers)
		{
			Title = "Test DataMiner Interface";

			getReservationsSection = new GetReservationsSection(helpers);

			Initialize();
			GenerateUi();	
		}

		private void Initialize()
		{
			getReservationsSection.RegenerateUiRequired += GetReservationsSection_RegenerateUi;

			bookingManagerChangeNameButton.Pressed += BookingManagerChangeNameButton_Pressed;

			reservationInstanceUpdateServiceReservationPropertiesButton.Pressed += ReservationInstanceUpdateServiceReservationPropertiesButton_Pressed;

			resourceManagerAddOrUpdateReservationInstancesButton.Pressed += ResourceManagerAddOrUpdateReservationInstancesButton_Pressed;
		}

		private void ResourceManagerAddOrUpdateReservationInstancesButton_Pressed(object sender, EventArgs e)
		{
			var securityViewIdsToSet = Array.ConvertAll(securityViewIdsTextBox.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), Convert.ToInt32).ToList();

			foreach (var reservation in getReservationsSection.SelectedReservations)
			{
				reservation.SecurityViewIDs = securityViewIdsToSet;
				DataMinerInterface.ResourceManager.AddOrUpdateReservationInstances(helpers, reservation);
			}
			
			ShowRequestResult($"Updated reservations with security view IDs {string.Join(",", securityViewIdsToSet)}", string.Join("\n", getReservationsSection.SelectedReservations.Select(r => r.Name)));
			GenerateUi();
		}

		private void ReservationInstanceUpdateServiceReservationPropertiesButton_Pressed(object sender, EventArgs e)
		{
			var reservationsToUpdate = getReservationsSection.SelectedReservations;
			foreach (var reservation in reservationsToUpdate)
			{
				DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(helpers, reservation, new System.Collections.Generic.Dictionary<string, object> { { propertyNameTextBox.Text, propertyValueTextBox.Text } });
			}

			ShowRequestResult($"Updated reservation properties.", $"Set property {propertyNameTextBox.Text} to {propertyValueTextBox.Text} for reservations {String.Join(", ", reservationsToUpdate.Select(x => x.Name))}");

			GenerateUi();
		}

		private void BookingManagerChangeNameButton_Pressed(object sender, EventArgs e)
		{
			var reservationToUpdate = getReservationsSection.SelectedReservations.FirstOrDefault() as Skyline.DataMiner.Net.ResourceManager.Objects.ServiceReservationInstance;
			var updatedReservation = DataMinerInterface.BookingManager.ChangeName(helpers, reservationToUpdate, nameToSetTextBox.Text);

			ShowRequestResult($"Change Name for {reservationToUpdate.Name} ({reservationToUpdate.ID}) to {nameToSetTextBox.Text}", JsonConvert.SerializeObject(updatedReservation));
		}

		private void GetReservationsSection_RegenerateUi(object sender, EventArgs e)
		{
			getReservationsSection.RegenerateUi();
			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0);
			AddWidget(header, ++row, 0, 1, 2);

			AddSection(getReservationsSection, new SectionLayout(++row, 0));
			row += getReservationsSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(bookingManagerChangeNameLabel, ++row, 0);
			AddWidget(bookingManagerNameLabel, ++row, 0);
			AddWidget(bookingManagerTextBox, row, 1);
			AddWidget(nameToSetLabel, ++row, 0);
			AddWidget(nameToSetTextBox, row, 1);
			AddWidget(bookingManagerChangeNameButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(reservationInstanceUpdateServiceReservationPropertiesLabel, ++row, 0);
			AddWidget(propertyNameLabel, ++row, 0);
			AddWidget(propertyNameTextBox, row, 1);
			AddWidget(propertyValueLabel, ++row, 0);
			AddWidget(propertyValueTextBox, row, 1);
			AddWidget(reservationInstanceUpdateServiceReservationPropertiesButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(resourceManagerAddOrUpdateReservationInstancesLabel, ++row, 0);
			AddWidget(securityViewIdsLabel, ++row, 0);
			AddWidget(securityViewIdsTextBox, row, 1);
			AddWidget(resourceManagerAddOrUpdateReservationInstancesButton, ++row, 0);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddResponseSections(row);
		}
	}
}
