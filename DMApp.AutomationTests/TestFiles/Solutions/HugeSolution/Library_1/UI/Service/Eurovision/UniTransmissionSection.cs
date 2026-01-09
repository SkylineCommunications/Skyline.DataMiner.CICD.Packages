namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service.Eurovision
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class UniTransmissionSection : Section
	{
		private readonly EurovisionBookingDetails details;
		private readonly UniTransmissionSectionConfiguration configuration;
		private readonly VirtualPlatformType virtualPlatformType;
		private readonly Helpers helpers;

		private readonly Label originOrganizationLabel = new Label("Origin Organization");
		private readonly DropDown originOrganizationDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label originCityLabel = new Label("Origin City");
		private readonly DropDown originCityDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

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

		public UniTransmissionSection(EurovisionBookingDetails details, VirtualPlatformType virtualPlatformType, UniTransmissionSectionConfiguration configuration, Helpers helpers = null)
		{
			this.details = details ?? throw new ArgumentNullException(nameof(details));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.virtualPlatformType = virtualPlatformType;
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

		public event EventHandler<Organization> OriginOrganizationChanged;

		public event EventHandler<BroadcastCenter> OriginCityChanged;

		public event EventHandler<Organization> DestinationOrganizationChanged;

		public event EventHandler<City> DestinationCityChanged;

		public event EventHandler<string> ContactChanged;

		public event EventHandler<Contract> ContractChanged;

		public event EventHandler<string> NoteChanged;

		public VideoSection VideoSection { get; private set; }

		public AudioSection AudioSection { get; private set; }

		private Organization OriginOrganization => details.GetUniTransmissionOrigins(virtualPlatformType).FirstOrDefault(o => o.DisplayName == originOrganizationDropDown.Selected);

		private BroadcastCenter OriginFeedpoint => details.OriginFeedpoints.FirstOrDefault(b => !String.IsNullOrWhiteSpace(b?.City?.Name) && b.City.Name == originCityDropDown.Selected);

		private Organization DestinationOrganization => details.GetUniTransmissionDestinations(virtualPlatformType).FirstOrDefault(o => o.DisplayName == destinationOrganizationDropDown.Selected);

		private City DestinationCity => details.DestinationFeedpoints.FirstOrDefault(b => !String.IsNullOrWhiteSpace(b?.City?.Name) && b.City.Name == destinationCityDropDown.Selected)?.City;

		private string Contact => contactTextBox.Text;

		private string Note => noteTextBox.Text;

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
			UpdateOriginOrganizationOptions();
			SetSelectedOriginOrganization();

			UpdateOriginCityOptions();
			SetSelectedOriginCity();

			UpdateDestinationOrganizationOptions();
			SetSelectedDestinationOrganization();

			UpdateDestinationCityOptions();
			SetSelectedDestinationCity();

			UpdateContractOptions();
			SetSelectedContract();

			contactTextBox.Text = details.Contact ?? String.Empty;
			noteTextBox.Text = details.Note ?? String.Empty;
		}

		private void UpdateOriginOrganizationOptions()
		{
			var organizations = details.GetUniTransmissionOrigins(virtualPlatformType);
			var originOrganizationOptions = organizations.Any() ? organizations.OrderBy(o => o.DisplayName).Select(o => o.DisplayName).ToList() : new List<string> { "None" };
			originOrganizationDropDown.SetOptions(originOrganizationOptions);
		}

		private void SetSelectedOriginOrganization()
		{
			var selectedOrganization = details.GetUniTransmissionOrigins(virtualPlatformType).FirstOrDefault(x => x.Code.Equals(details.OriginOrganizationCode));
			originCityDropDown.Selected = selectedOrganization != null ? selectedOrganization.DisplayName : null;
		}

		private void UpdateOriginCityOptions()
		{
			var originCityOptions = details.OriginFeedpoints.Any() ? details.OriginFeedpoints.Where(x => !String.IsNullOrWhiteSpace(x?.City?.Name)).OrderBy(b => b.City.Name).Select(b => b.City.Name).ToList() : new List<string> { "None" };
			originCityDropDown.SetOptions(originCityOptions);
		}

		private void SetSelectedOriginCity()
		{
			var selectedOriginCity = details.OriginFeedpoints.FirstOrDefault(x => x.Code.Equals(details.OriginCityCode) && !String.IsNullOrWhiteSpace(x.City?.Name));
			originCityDropDown.Selected = selectedOriginCity != null ? selectedOriginCity.City.Name : "None";
		}

		private void UpdateDestinationOrganizationOptions()
		{
			var destinations = details.GetUniTransmissionDestinations(virtualPlatformType);
			var destinationOrganizationOptions = destinations.Any() ? destinations.OrderBy(o => o.DisplayName).Select(o => o.DisplayName).ToList() : new List<string> { "None" };
			destinationOrganizationDropDown.SetOptions(destinationOrganizationOptions);
		}

		private void SetSelectedDestinationOrganization()
		{
			var organization = details.GetUniTransmissionDestinations(virtualPlatformType).FirstOrDefault(x => x.Code.Equals(details.DestinationOrganizationCode));
			destinationOrganizationDropDown.Selected = organization != null ? organization.DisplayName : "None";
		}

		private void UpdateDestinationCityOptions()
		{
			var destinationCityOptions = details.DestinationFeedpoints.Any() ? details.DestinationFeedpoints.Where(x => !String.IsNullOrWhiteSpace(x?.City?.DisplayName)).OrderBy(b => b.City.DisplayName).Select(b => b.City.DisplayName) : new List<string> { "None" };
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
			var contract = details.Contracts.FirstOrDefault(c => c.Name == contractDropDown.Selected);
			contractDropDown.Selected = contract != null ? contract.Name : "None";
		}

		private void InitializeSections()
		{
			VideoSection = new VideoSection(details, configuration.VideoSectionConfiguration);
			AudioSection = new AudioSection(details, configuration.AudioSectionConfiguration);
		}

		private void SubscribeToWidgets()
		{
			originOrganizationDropDown.Changed += (s, e) => OriginOrganizationChanged?.Invoke(this, OriginOrganization);
			originCityDropDown.Changed += (s, e) => OriginCityChanged?.Invoke(this, OriginFeedpoint);
			destinationOrganizationDropDown.Changed += (s, e) => DestinationOrganizationChanged?.Invoke(this, DestinationOrganization);
			destinationCityDropDown.Changed += (s, e) => DestinationCityChanged?.Invoke(this, DestinationCity);
			contactTextBox.Changed += (s, e) => ContactChanged?.Invoke(this, Contact);
			contractDropDown.Changed += (s, e) => ContractChanged?.Invoke(this, Contract);
			noteTextBox.Changed += (s, e) => NoteChanged?.Invoke(this, Note);
		}

		private void SubscribeToBookingDetails()
		{
			details.OrganizationsChanged += (s, e) =>
			{
				UpdateOriginOrganizationOptions();
				UpdateDestinationOrganizationOptions();

				SetSelectedOriginOrganization();
				SetSelectedDestinationOrganization();
			};

			details.OriginOrganizationCodeChanged += (s, e) => SetSelectedOriginOrganization();

			details.DestinationOrganizationCodeChanged += (s, e) => SetSelectedDestinationOrganization();

			details.OriginFeedpointsChanged += (s, e) =>
			{
				UpdateOriginCityOptions();
				SetSelectedOriginCity();
			};

			details.OriginCityCodeChanged += (s, e) => SetSelectedOriginCity();

			details.DestinationFeedpointsChanged += (s, e) =>
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

			AddWidget(originOrganizationLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(originOrganizationDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(originCityLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(originCityDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

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
