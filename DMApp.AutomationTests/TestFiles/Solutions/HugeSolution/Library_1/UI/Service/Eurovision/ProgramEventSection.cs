namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service.Eurovision
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class ProgramEventSection : Section
	{
		private readonly EurovisionBookingDetails details;
		private readonly ProgramEventSectionConfiguration configuration;
		private readonly Helpers helpers;

		private readonly Label eventLabel = new Label("Event");
		private readonly DropDown eventDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label participationLabel = new Label("Participation");
		private readonly DropDown participationDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };
		private readonly CheckBox includeBookedParticipationsCheckBox = new CheckBox("Include booked participations");

		private readonly Label destinationOrganizationLabel = new Label("Destination Organization");
		private readonly DropDown destinationOrganizationDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label destinationCityLabel = new Label("Destination City");
		private readonly DropDown destinationCityDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label contactLabel = new Label("Contact");
		private readonly YleTextBox contactTextBox = new YleTextBox();

		private readonly Label contractLabel = new Label("Contract");
		private readonly DropDown contractDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label noteLabel = new Label("Note");
		private readonly YleTextBox noteTextBox = new YleTextBox();

		public ProgramEventSection(EurovisionBookingDetails details, ProgramEventSectionConfiguration configuration, Helpers helpers = null)
		{
			this.details = details ?? throw new ArgumentNullException(nameof(details));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.helpers = helpers;

			Initialize();
			GenerateUI();
			HandleVisibilityAndEnabledUpdate();
		}

		public new bool IsVisible
		{
			get => base.IsVisible;

			set
			{
				base.IsVisible = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public new bool IsEnabled
		{
			get => base.IsEnabled;

			set
			{
				base.IsEnabled = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public event EventHandler<Event> EventChanged;

		public event EventHandler<MultilateralTransmission> ParticipationChanged;

		public event EventHandler<Organization> DestinationOrganizationChanged;

		public event EventHandler<City> DestinationCityChanged;

		public event EventHandler<string> ContactChanged;

		public event EventHandler<string> NoteChanged;

		public event EventHandler<Contract> ContractChanged;

		private Event Event => details.GetProgramEvents().FirstOrDefault(x => x.DisplayValue.Equals(eventDropDown.Selected));

		private MultilateralTransmission Participation => details.Participations.FirstOrDefault(x => x.DisplayValue.Equals(participationDropDown.Selected));

		private Organization DestinationOrganization => details.Organizations.FirstOrDefault(o => o.DisplayName == destinationOrganizationDropDown.Selected);

		private City DestinationCity => details.Cities.FirstOrDefault(c => c.DisplayName == destinationCityDropDown.Selected);

		private string Contact => contactTextBox.Text;

		private string Note => noteTextBox.Text;

		private Contract Contract
		{
			get
			{
				return details.Contracts.FirstOrDefault(c => c.Name == contractDropDown.Selected);
			}
		}

		public void RegenerateUI()
		{
			Clear();
			GenerateUI();
			HandleVisibilityAndEnabledUpdate();
		}

		private void Initialize()
		{
			IntializeWidgets();
			SubscribeToWidgets();
			SubscribeToBookingDetails();
		}

		private void IntializeWidgets()
		{
			UpdateEventOptions();
			SetSelectedEvent();

			UpdateParticipationOptions();
			SetSelectedParticipation();

			UpdateDestinationOrganizationOptions();
			SetSelectedDestinationOrganization();

			UpdateDestinationCityOptions();
			SetSelectedDestinationCity();

			UpdateContractOptions();
			SetSelectedContract();

			contactTextBox.Text = details.Contact ?? String.Empty;
			noteTextBox.Text = details.Note ?? String.Empty;
		}

		private void UpdateEventOptions()
		{
			var eventOptions = details.GetProgramEvents().Any() ? details.GetProgramEvents().OrderBy(e => e.StartDate).Select(e => e.DisplayValue).ToList() : new List<string> { "None" };
			eventDropDown.SetOptions(eventOptions);
		}

		private void SetSelectedEvent()
		{
			var selectedEvent = details.GetProgramEvents().FirstOrDefault(x => x.EventId.Equals(details.EventId));
			eventDropDown.Selected = selectedEvent != null ? selectedEvent.DisplayValue : "None";
		}

		private void UpdateParticipationOptions()
		{
			var participationOptions = details.Participations.Any() ? details.Participations.Where(m => !m.IsAlreadyBooked || includeBookedParticipationsCheckBox.IsChecked).Select(m => m.DisplayValue).OrderBy(m => m).ToList() : new List<string> { "None" };
			participationDropDown.SetOptions(participationOptions);
		}

		private void SetSelectedParticipation()
		{
			var participation = details.Participations.FirstOrDefault(x => x.Id.Equals(details.MultilateralTransmissionId));
			participationDropDown.Selected = participation != null ? participation.DisplayValue : "None";
		}

		private void UpdateDestinationOrganizationOptions()
		{
			var destinationOrganizationOptions = details.GetFinnishOrganizations().Any() ? details.GetFinnishOrganizations().OrderBy(o => o.DisplayName).Select(o => o.DisplayName).ToList() : new List<string> { "None" };
			destinationOrganizationDropDown.SetOptions(destinationOrganizationOptions);
		}

		private void SetSelectedDestinationOrganization()
		{
			var organization = details.GetFinnishOrganizations().FirstOrDefault(x => x.Code.Equals(details.DestinationOrganizationCode));
			destinationOrganizationDropDown.Selected = organization != null ? organization.DisplayName : "None";
		}

		private void UpdateDestinationCityOptions()
		{
			var destinationCityOptions = details.Cities.Any() ? details.Cities.OrderBy(c => c.DisplayName).Select(c => c.DisplayName).ToList() : new List<string> { "None" };
			destinationCityDropDown.SetOptions(destinationCityOptions);
		}

		private void SetSelectedDestinationCity()
		{
			var city = details.Cities.FirstOrDefault(x => x.Code.Equals(details.DestinationCityCode));
			destinationCityDropDown.Selected = city != null ? city.DisplayName : "None";
		}

		private void UpdateContractOptions()
		{
			var contractOptions = details.Contracts.Any() ? details.Contracts.Where(c => c != null).OrderBy(c => c.Name).Select(c => c.Name).ToList() : new List<string> { "None" };
			contractDropDown.SetOptions(contractOptions);
		}

		private void SetSelectedContract()
		{
			var contract = details.Contracts.FirstOrDefault(c => c.Name == contractDropDown.Selected);
			contractDropDown.Selected = contract != null ? contract.Name : "None";
		}

		private void SubscribeToWidgets()
		{
			eventDropDown.Changed += (s, e) => EventChanged?.Invoke(this, Event);
			participationDropDown.Changed += (s, e) => ParticipationChanged?.Invoke(this, Participation);
			includeBookedParticipationsCheckBox.Changed += (s, e) => UpdateParticipationOptions();
			destinationOrganizationDropDown.Changed += (s, e) => DestinationOrganizationChanged?.Invoke(this, DestinationOrganization);
			destinationCityDropDown.Changed += (s, e) => DestinationCityChanged?.Invoke(this, DestinationCity);
			contactTextBox.Changed += (s, e) => ContactChanged?.Invoke(this, Contact);
			contractDropDown.Changed += (s, e) => ContractChanged?.Invoke(this, Contract);
			noteTextBox.Changed += (s, e) => NoteChanged?.Invoke(this, Note);
		}

		private void SubscribeToBookingDetails()
		{
			details.EventsChanged += (s, e) =>
			{
				UpdateEventOptions();
				SetSelectedEvent();
			};

			details.EventIdChanged += (s, e) => SetSelectedEvent();

			details.ParticipationsChanged += (s, e) =>
			{
				UpdateParticipationOptions();
				SetSelectedParticipation();
			};

			details.MultilateralTransmissionIdChanged += (s, e) => SetSelectedParticipation();

			details.OrganizationsChanged += (s, e) =>
			{
				UpdateDestinationOrganizationOptions();
				SetSelectedDestinationOrganization();
			};

			details.DestinationOrganizationCodeChanged += (s, e) => SetSelectedDestinationOrganization();

			details.CitiesChanged += (s, e) =>
			{
				UpdateDestinationCityOptions();
				SetSelectedDestinationCity();
			};

			details.DestinationCityCodeChanged += (s, e) => SetSelectedDestinationCity();

			details.ContractsChanged += (s, e) =>
			{
				UpdateContractOptions();
				SetSelectedContract();
			};

			details.ContractsChanged += (s, e) => SetSelectedContract();
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(eventLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(eventDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan + 1); // spanned over 2 columns because of long dropdown options

			AddWidget(participationLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(participationDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan + 1); // spanned over 2 columns because of long dropdown options
			AddWidget(includeBookedParticipationsCheckBox, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan + 1);

			AddWidget(destinationOrganizationLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(destinationOrganizationDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(destinationCityLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(destinationCityDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(contactLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(contactTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(contractLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(contractDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(noteLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(noteTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		private void HandleVisibilityAndEnabledUpdate()
		{
			// Either the entire section is visible/enabled or not at all.

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}
