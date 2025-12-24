namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service.Eurovision
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

	public class NewsEventSection : Section
	{
		private readonly Helpers helpers;

		private readonly EurovisionBookingDetails details;
		private readonly NewsEventSectionConfiguration configuration;

		private readonly Label eventLabel = new Label("Event");
		private readonly DropDown eventDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label feedpointLabel = new Label("Feedpoint");
		private readonly DropDown feedpointDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label facilityLabel = new Label("Facility");
		private readonly DropDown facilityDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label destinationOrganizationLabel = new Label("Destination Organization");
		private readonly DropDown destinationOrganizationDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label destinationCityLabel = new Label("Destination City");
		private readonly DropDown destinationCityDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label descriptionLabel = new Label("Description");
		private readonly YleTextBox descriptionTextBox = new YleTextBox();

		private readonly Label contactLabel = new Label("Contact");
		private readonly YleTextBox contactTextBox = new YleTextBox();

		private readonly Label contractLabel = new Label("Contract");
		private readonly DropDown contractDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label noteLabel = new Label("Note");
		private readonly YleTextBox noteTextBox = new YleTextBox();

		public NewsEventSection(EurovisionBookingDetails details, NewsEventSectionConfiguration configuration, Helpers helpers)
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

		public event EventHandler<Organization> DestinationOrganizationChanged;

		public event EventHandler<City> DestinationCityChanged;

		public event EventHandler<BroadcastCenter> FeedpointChanged;

		public event EventHandler<Facility> FacilityChanged;

		public event EventHandler<string> ContactChanged;

		public event EventHandler<string> DescriptionChanged;

		public event EventHandler<string> NoteChanged;

		public event EventHandler<Contract> ContractChanged;

		public VideoSection VideoSection { get; private set; }

		public AudioSection AudioSection { get; private set; }

		private Event Event => details.GetNewsEvents().FirstOrDefault(x => x.DisplayValue.Equals(eventDropDown.Selected));

		private Organization DestinationOrganization => details.Organizations.FirstOrDefault(o => o.DisplayName == destinationOrganizationDropDown.Selected);

		private City DestinationCity => details.Cities.FirstOrDefault(c => c.DisplayName == destinationCityDropDown.Selected);

		public BroadcastCenter Feedpoint => details.OriginFeedpoints.FirstOrDefault(f => f.Name == feedpointDropDown.Selected);

		private Facility Facility => details.Facilities.FirstOrDefault(f => f.ProductName == facilityDropDown.Selected);

		private Contract Contract => details.Contracts.FirstOrDefault(c => c.Name == contractDropDown.Selected);

		public void RegenerateUI()
		{
			Clear();
			VideoSection.RegenerateUI();
			AudioSection.RegenerateUI();
			GenerateUI();
			HandleVisibilityAndEnabledUpdate();
		}

		private void Initialize()
		{
			IntializeWidgets();
			InitializeSections();
			SubscribeToWidgets();
			SubscribeToBookingDetails();
		}

		private void IntializeWidgets()
		{
			UpdateEventOptions();
			SetSelectedEvent();

			UpdateFeedpointOptions();
			SetSelectedFeedpoint();

			UpdateFacilityOptions();
			SetSelectedFacility();

			UpdateDestinationOrganizationOptions();
			SetSelectedDestinationOrganization();

			UpdateDestinationCityOptions();
			SetSelectedDestinationCity();

			UpdateContractOptions();
			SetSelectedContract();

			descriptionTextBox.Text = details.Description ?? String.Empty;
			noteTextBox.Text = details.Note ?? String.Empty;
			contactTextBox.Text = details.Contact ?? String.Empty;
		}

		private void UpdateEventOptions()
		{
			var eventOptions = details.GetNewsEvents().Any() ? details.GetNewsEvents().OrderBy(e => e.StartDate).Select(e => e.DisplayValue).ToList() : new List<string> { "None" };
			eventDropDown.SetOptions(eventOptions);
		}

		private void SetSelectedEvent()
		{
			var selectedEvent = details.GetNewsEvents().FirstOrDefault(x => x.EventId.Equals(details.EventId));

			helpers?.Log(nameof(NewsEventSection), nameof(SetSelectedEvent), $"Saved event ID: {details.EventId}, Setting event to {selectedEvent?.DisplayValue}");

			eventDropDown.Selected = selectedEvent != null ? selectedEvent.DisplayValue : "None";
		}

		private void UpdateFeedpointOptions()
		{
			var feedpointOptions = details.OriginFeedpoints.Any() ? details.OriginFeedpoints.OrderBy(f => f.Name).Select(f => f.Name).ToList() : new List<string> { "None" };
			feedpointDropDown.SetOptions(feedpointOptions);
		}

		private void SetSelectedFeedpoint()
		{
			var selectedFeedpoint = details.OriginFeedpoints.FirstOrDefault(x => x.Code.Equals(details.FeedpointCode));
			feedpointDropDown.Selected = selectedFeedpoint != null ? selectedFeedpoint.Name : "None";
		}

		private void UpdateFacilityOptions()
		{
			var facilityOptions = details.Facilities.Any() ? details.Facilities.OrderBy(f => f.ProductName).Select(f => f.ProductName).ToList() : new List<string> { "None" };
			facilityDropDown.SetOptions(facilityOptions);
		}

		private void SetSelectedFacility()
		{
			var selectedFacility = details.Facilities.FirstOrDefault(x => x.ProductId.Equals(details.FacilityProductId));
			facilityDropDown.Selected = selectedFacility != null ? selectedFacility.ProductName : "None";
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
			var contractOptions = details.Contracts.Any() ? details.Contracts.OrderBy(c => c.Name).Select(c => c.Name).ToList() : new List<string> { "None" };
			contractDropDown.SetOptions(contractOptions);
		}

		private void SetSelectedContract()
		{
			var contract = details.Contracts.FirstOrDefault(c => c.Name.Equals(details.ContractCode));

			helpers?.Log(nameof(NewsEventSection), nameof(SetSelectedContract), $"Saved event ID: {details.ContractCode}, Setting event to {contract?.Name}");

			contractDropDown.Selected = contract != null ? contract.Name : "None";
		}

		private void InitializeSections()
		{
			VideoSection = new VideoSection(details, configuration.VideoSectionConfiguration);
			AudioSection = new AudioSection(details, configuration.AudioSectionConfiguration, helpers);
		}

		private void SubscribeToWidgets()
		{
			eventDropDown.Changed += (s, e) => EventChanged?.Invoke(this, Event);
			feedpointDropDown.Changed += (s, e) => FeedpointChanged?.Invoke(this, Feedpoint);
			facilityDropDown.Changed += (s, e) => FacilityChanged?.Invoke(this, Facility);
			destinationOrganizationDropDown.Changed += (s, e) => DestinationOrganizationChanged?.Invoke(this, DestinationOrganization);
			destinationCityDropDown.Changed += (s, e) => DestinationCityChanged?.Invoke(this, DestinationCity);
			descriptionTextBox.Changed += (s, e) => DescriptionChanged?.Invoke(this, descriptionTextBox.Text);
			contactTextBox.Changed += (s, e) => ContactChanged?.Invoke(this, contactTextBox.Text);
			contractDropDown.Changed += (s, e) => ContractChanged?.Invoke(this, Contract);
			noteTextBox.Changed += (s, e) => NoteChanged?.Invoke(this, noteTextBox.Text);
		}

		private void SubscribeToBookingDetails()
		{
			details.EventsChanged += (s, e) =>
			{
				UpdateEventOptions();
				SetSelectedEvent();
			};

			details.EventIdChanged += (s, e) => SetSelectedEvent();

			details.FacilitiesChanged += (s, e) =>
			{
				UpdateFacilityOptions();
				SetSelectedFacility();
			};

			details.FacilityProductIdChanged += (s, e) => SetSelectedFacility();

			details.OriginFeedpointsChanged += (s, e) =>
			{
				UpdateFeedpointOptions();
				SetSelectedFeedpoint();
			};

			details.FeedpointCodeChanged += (s, e) => SetSelectedFeedpoint();

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

			details.ContractCodeChanged += (s, e) => SetSelectedContract();
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(eventLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(eventDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan + 1); // spanned over more columns because of long dropdown options

			AddWidget(feedpointLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(feedpointDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(facilityLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(facilityDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(destinationOrganizationLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(destinationOrganizationDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(destinationCityLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(destinationCityDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(descriptionLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(descriptionTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(contactLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(contactTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(contractLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(contractDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(noteLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(noteTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddSection(VideoSection, new SectionLayout(++row, 0));
			row += VideoSection.RowCount;

			AddSection(AudioSection, new SectionLayout(++row, 0));

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		private void HandleVisibilityAndEnabledUpdate()
		{
			VideoSection.IsVisible = IsVisible;
			VideoSection.IsEnabled = IsEnabled;

			AudioSection.IsVisible = IsVisible;
			AudioSection.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}
