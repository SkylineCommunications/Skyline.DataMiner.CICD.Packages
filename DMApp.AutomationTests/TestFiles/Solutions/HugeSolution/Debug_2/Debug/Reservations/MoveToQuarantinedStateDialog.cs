namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Reservations
{ 
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Library.Solutions.SRM;

	public class MoveToQuarantinedStateDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label findReservationsLabel = new Label("Find Reservation") { Style = TextStyle.Heading };
		private readonly Label reservationNameLabel = new Label("Name");
		private readonly Label reservationIdLabel = new Label("GUID");
		private readonly Label srmStateLabel = new Label("SRM State");
		private readonly Button enterCurrentOrderIdButton = new Button("Enter Current ID") { Width = 200 };

		private ReservationInstance reservationInstance;

		public MoveToQuarantinedStateDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Move Reservation To Quarantined State";

			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		private TextBox ReservationNameTextBox { get; set; }

		private TextBox ReservationIdTextBox { get; set; }

		private Button FindByReservationNameButton { get; set; }

		private Button FindByReservationIdButton { get; set; }

		public Button MoveToQuarantinedStateButton { get; } = new Button("Move to Quarantined State") { Width = 200, IsEnabled = false };

		public TextBox ExceptionTextBox { get; } = new TextBox { IsMultiline = true, Height = 300 };

		private void Initialize()
		{
			ReservationNameTextBox = new TextBox { PlaceHolder = "Name", Width = 400 };
			ReservationIdTextBox = new TextBox { PlaceHolder = "GUID", ValidationText = "Invalid GUID", Width = 400 };

			FindByReservationNameButton = new Button("Find By Name") { Width = 150 };
			FindByReservationNameButton.Pressed += FindByReservationNameButton_Pressed;

			FindByReservationIdButton = new Button("Find By GUID") { Width = 150 };
			FindByReservationIdButton.Pressed += FindByReservationIdButton_Pressed;

			enterCurrentOrderIdButton.Pressed += (sender, args) => ReservationIdTextBox.Text = helpers.Engine.GetScriptParam(1)?.Value;

			MoveToQuarantinedStateButton.Pressed += MoveToQuarantinedStateButton_Pressed;
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

			if (reservationInstance != null)
			{
				AddWidget(srmStateLabel, ++row, 0, 1, 2);
				AddWidget(new Label(reservationInstance.Status.GetDescription()), row, 3);
			}

			AddWidget(new WhiteSpace(), ++row, 1);

			AddWidget(MoveToQuarantinedStateButton, ++row, 0, 1, 5);

			if (!String.IsNullOrWhiteSpace(ExceptionTextBox.Text))
			{
				AddWidget(ExceptionTextBox, row + 1, 0, 1, 5);
			}
		}

		private void FindByReservationNameButton_Pressed(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ReservationNameTextBox.Text))
			{
				ReservationIdTextBox.Text = String.Empty;
				ReservationNameTextBox.ValidationText = "Fill out the Name of a reservation";
				ReservationNameTextBox.ValidationState = UIValidationState.Invalid;
				MoveToQuarantinedStateButton.IsEnabled = false;
				reservationInstance = null;
				GenerateUi();
				return;
			}

			var reservationInstances = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(ReservationInstanceExposers.Name.Contains(ReservationNameTextBox.Text)));
			if (!reservationInstances.Any())
			{
				ReservationIdTextBox.Text = String.Empty;
				ReservationNameTextBox.ValidationText = "Unable to find a reservation with the specified Name";
				ReservationNameTextBox.ValidationState = UIValidationState.Invalid;
				MoveToQuarantinedStateButton.IsEnabled = false;
				reservationInstance = null;
				GenerateUi();
				return;
			}

			reservationInstance = reservationInstances.First();

			ReservationNameTextBox.Text = reservationInstance.Name;
			ReservationIdTextBox.Text = reservationInstance.ID.ToString();
			ReservationNameTextBox.ValidationState = UIValidationState.Valid;

			MoveToQuarantinedStateButton.IsEnabled = true;

			GenerateUi();
		}

		private void FindByReservationIdButton_Pressed(object sender, EventArgs e)
		{
			if (!Guid.TryParse(ReservationIdTextBox.Text, out var guid))
			{
				ReservationNameTextBox.Text = String.Empty;
				ReservationNameTextBox.ValidationText = "Unable to parse the GUID";
				ReservationIdTextBox.ValidationState = UIValidationState.Invalid;
				MoveToQuarantinedStateButton.IsEnabled = false;
				reservationInstance = null;
				GenerateUi();
				return;
			}

			reservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, guid);
			if (reservationInstance == null)
			{
				ReservationNameTextBox.Text = String.Empty;
				ReservationNameTextBox.ValidationText = "No reservations found with the given ID";
				ReservationIdTextBox.ValidationState = UIValidationState.Invalid;
				MoveToQuarantinedStateButton.IsEnabled = false;
				reservationInstance = null;
				GenerateUi();
				return;
			}

			ReservationNameTextBox.Text = reservationInstance.Name;
			ReservationIdTextBox.Text = reservationInstance.ID.ToString();
			ReservationIdTextBox.ValidationState = UIValidationState.Valid;

			MoveToQuarantinedStateButton.IsEnabled = true;

			GenerateUi();
		}

		private void MoveToQuarantinedStateButton_Pressed(object sender, EventArgs e)
		{
			if (reservationInstance == null) return;

			try
			{
				reservationInstance.IsQuarantined = true;
				reservationInstance = SrmManagers.ResourceManager.AddOrUpdateReservationInstances(true, reservationInstance).FirstOrDefault();
				ExceptionTextBox.Text = String.Empty;
			}
			catch(Exception exception)
			{
				ExceptionTextBox.Text = exception.ToString();
				helpers.Log(nameof(MoveToQuarantinedStateDialog), nameof(MoveToQuarantinedStateButton_Pressed), "Something went wrong: " + exception);
			}

			GenerateUi();
		}
	}
}
