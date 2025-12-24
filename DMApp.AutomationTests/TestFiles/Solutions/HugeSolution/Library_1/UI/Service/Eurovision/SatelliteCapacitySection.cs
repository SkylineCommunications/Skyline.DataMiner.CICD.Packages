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
	using Library_1.EventArguments;

	public class SatelliteCapacitySection : Section
	{
		private readonly EurovisionBookingDetails details;
		private readonly SatelliteCapacitySectionConfiguration configuration;
		private readonly VirtualPlatformType virtualPlatformType;
		private readonly Helpers helpers;

		private readonly Label satelliteLabel = new Label("Satellite");
		private readonly DropDown satelliteDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label uplinkLabel = new Label("Uplink");
		private readonly DropDown uplinkDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label originOrganizationLabel = new Label("Origin Organization");
		private readonly DropDown originOrganizationDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label originCityLabel = new Label("Origin City");
		private readonly DropDown originCityDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label contractLabel = new Label("Contract");
		private readonly DropDown contractDropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };

		private readonly Label noteLabel = new Label("Note");
		private readonly YleTextBox noteTextBox = new YleTextBox();

		private readonly Label lineUpLabel = new Label("Line Up");
		private readonly DropDown lineUpDropDown = new DropDown(new[] { "0", "5", "10", "15" }, "10");

		private readonly Label contactTitle = new Label("CONTACT") { Style = TextStyle.Heading };

		private readonly Label contactFirstNameLabel = new Label("First Name");
		private readonly YleTextBox contactFirstNameTextBox = new YleTextBox();

		private readonly Label contactLastNameLabel = new Label("Last Name");
		private readonly YleTextBox contactLastNameTextBox = new YleTextBox();

		private readonly Label uplinkEmailLabel = new Label("Email");
		private readonly YleTextBox uplinkEmailTextBox = new YleTextBox();

		private readonly Label uplinkTelephoneNumberLabel = new Label("Phone Number");
		private readonly YleTextBox uplinkTelephoneNumberTextBox = new YleTextBox();

		public SatelliteCapacitySection(EurovisionBookingDetails details, VirtualPlatformType virtualPlatformType, SatelliteCapacitySectionConfiguration configuration)
		{
			this.details = details ?? throw new ArgumentNullException(nameof(details));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.virtualPlatformType = virtualPlatformType;

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

		public VideoSection VideoSection { get; private set; }

		public AudioSection AudioSection { get; private set; }

		public event EventHandler<Satellite> SatelliteChanged;

		public event EventHandler<Transportable> UplinkChanged;

		public event EventHandler<Organization> OriginChanged;

		public event EventHandler<BroadcastCenter> OriginCityChanged;

		public event EventHandler<StringEventArgs> ContactFirstNameChanged;

		public event EventHandler<StringEventArgs> ContactLastNameChanged;

		public event EventHandler<StringEventArgs> NoteChanged;

		public event EventHandler<StringEventArgs> LineUpChanged;

		public event EventHandler<StringEventArgs> PhoneChanged;

		public event EventHandler<StringEventArgs> EmailChanged;

		public event EventHandler<Contract> ContractChanged;

		private Satellite Satellite
		{
			get
			{
				return details.Satellites.FirstOrDefault(s => s.DisplayName == satelliteDropDown.Selected);
			}
		}

		private Transportable Uplink
		{
			get
			{
				return details.GetUplinks(virtualPlatformType).FirstOrDefault(u => u.DisplayName == uplinkDropDown.Selected);
			}
		}

		private Organization OriginOrganization
		{
			get
			{
				return details.Organizations.FirstOrDefault(o => o.DisplayName == originOrganizationDropDown.Selected);
			}
		}

		public BroadcastCenter OriginCity
		{
			get
			{
				return details.OriginFeedpoints.FirstOrDefault(b => !String.IsNullOrWhiteSpace(b?.City?.Name) && b.City.Name == originCityDropDown.Selected);
			}
		}

		private string ContactFirstName => contactFirstNameTextBox.Text;

		private string ContactLastName => contactLastNameTextBox.Text;

		private string Note => noteTextBox.Text;

		private string LineUp => lineUpDropDown.Selected;

		private string Phone => uplinkTelephoneNumberTextBox.Text;

		private string Email => uplinkEmailTextBox.Text;

		private Contract Contract
		{
			get
			{
				if (details.Contracts == null) return null;
				return details.Contracts.FirstOrDefault(c => c.Name == contractDropDown.Selected);
			}
		}

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
			UpdateSatelliteOptions();
			SetSelectedSatellite();

			UpdateUplinkOptions();
			SetSelectedUplink();

			UpdateOriginOrganizationOptions();
			SetSelectedOriginOrganization();

			UpdateBroadcastCenterOptions();
			SetSelectedBroadcastCenter();

			UpdateContractOptions();
			SetSelectedContract();

			contactFirstNameTextBox.Text = details.ContactFirstName ?? String.Empty;
			contactLastNameTextBox.Text = details.ContactLastName ?? String.Empty;
			lineUpDropDown.Selected = details.LineUp ?? lineUpDropDown.Options.FirstOrDefault();
			uplinkTelephoneNumberTextBox.Text = details.Phone ?? String.Empty;
			uplinkEmailTextBox.Text = details.Email ?? String.Empty;
			noteTextBox.Text = details.Note ?? String.Empty;
		}

		private void UpdateSatelliteOptions()
		{
			var satelliteOptions = details.Satellites.Any() ? details.Satellites.OrderBy(s => s.DisplayName).Select(s => s.DisplayName).ToList() : new List<string> { "None" };
			satelliteDropDown.SetOptions(satelliteOptions);
		}

		private void SetSelectedSatellite()
		{
			var selectedSatellite = details.Satellites.FirstOrDefault(x => x.Id.Equals(details.SatelliteId));
			satelliteDropDown.Selected = selectedSatellite != null ? selectedSatellite.DisplayName : "None";
		}

		private void UpdateUplinkOptions()
		{
			var uplinks = details.GetUplinks(virtualPlatformType);
			var uplinkOptions = uplinks.Any() ? uplinks.OrderBy(s => s.DisplayName).Select(s => s.DisplayName).ToList() : new List<string> { "None" };
			uplinkDropDown.SetOptions(uplinkOptions);
		}

		private void SetSelectedUplink()
		{
			var selectedUplink = details.GetUplinks(virtualPlatformType).FirstOrDefault(x => x.Id.Equals(details.TransportableId));
			uplinkDropDown.Selected = selectedUplink != null ? selectedUplink.DisplayName : "None";
		}

		private void UpdateOriginOrganizationOptions()
		{
			var originOrganizationOptions = details.Organizations.Any() ? details.Organizations.OrderBy(o => o.DisplayName).Select(o => o.DisplayName).ToList() : new List<string> { "None" };
			originOrganizationDropDown.SetOptions(originOrganizationOptions);
		}

		private void SetSelectedOriginOrganization()
		{
			var selectedOrganization = details.Organizations.FirstOrDefault(x => x.Code.Equals(details.OriginOrganizationCode));
			originOrganizationDropDown.Selected = selectedOrganization != null ? selectedOrganization.DisplayName : "None";
		}

		private void UpdateBroadcastCenterOptions()
		{
			var broadcastCenterOptions = details.OriginFeedpoints.Any() ? details.OriginFeedpoints.Where(x => !String.IsNullOrWhiteSpace(x?.City?.Name)).OrderBy(b => b.City.Name).Select(b => b.City.Name).ToList() : new List<string> { "None" };
			originCityDropDown.SetOptions(broadcastCenterOptions);
		}

		private void SetSelectedBroadcastCenter()
		{
			var selectedBroadcastCenter = details.OriginFeedpoints.FirstOrDefault(x => x.Code.Equals(details.OriginCityCode) && !String.IsNullOrWhiteSpace(x.City?.Name));
			originCityDropDown.Selected = selectedBroadcastCenter != null ? selectedBroadcastCenter.City.Name : "None";
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
			satelliteDropDown.Changed += (s, e) => SatelliteChanged?.Invoke(this, Satellite);
			uplinkDropDown.Changed += (s, e) => UplinkChanged?.Invoke(this, Uplink);
			originOrganizationDropDown.Changed += (s, e) => OriginChanged?.Invoke(this, OriginOrganization);
			originCityDropDown.Changed += (s, e) => OriginCityChanged?.Invoke(this, OriginCity);
			contractDropDown.Changed += (s, e) => ContractChanged?.Invoke(this, Contract);
			noteTextBox.Changed += (s, e) => NoteChanged?.Invoke(this, new StringEventArgs(Note));
			lineUpDropDown.Changed += (s, e) => LineUpChanged?.Invoke(this, new StringEventArgs(LineUp));
			contactFirstNameTextBox.Changed += (s, e) => ContactFirstNameChanged?.Invoke(this, new StringEventArgs(ContactFirstName));
			contactLastNameTextBox.Changed += (s, e) => ContactLastNameChanged?.Invoke(this, new StringEventArgs(ContactLastName));
			uplinkEmailTextBox.Changed += (s, e) => EmailChanged?.Invoke(this, new StringEventArgs(Email));
			uplinkTelephoneNumberTextBox.Changed += (s, e) => PhoneChanged?.Invoke(this, new StringEventArgs(Phone));
		}

		private void SubscribeToBookingDetails()
		{
			details.SatellitesChanged += (s, e) =>
			{
				UpdateSatelliteOptions();
				SetSelectedSatellite();
			};

			details.SatelliteIdChanged += (s, e) => SetSelectedSatellite();

			details.TransportablesChanged += (s, e) =>
			{
				UpdateUplinkOptions();
				SetSelectedUplink();
			};

			details.TransportableIdChanged += (s, e) => SetSelectedUplink();

			details.OrganizationsChanged += (s, e) =>
			{
				UpdateOriginOrganizationOptions();
				SetSelectedOriginOrganization();
			};

			details.OriginOrganizationCodeChanged += (s, e) => SetSelectedOriginOrganization();

			details.OriginFeedpointsChanged += (s, e) =>
			{
				UpdateBroadcastCenterOptions();
				SetSelectedBroadcastCenter();
			};

			details.OriginCityCodeChanged += (s, e) => SetSelectedBroadcastCenter();

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

			AddWidget(satelliteLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(satelliteDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(uplinkLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(uplinkDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(originOrganizationLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(originOrganizationDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(originCityLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(originCityDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(contractLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(contractDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(noteLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(noteTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(lineUpLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(lineUpDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(contactTitle, ++row, 0, 1, configuration.LabelSpan);

			AddWidget(contactFirstNameLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(contactFirstNameTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(contactLastNameLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(contactLastNameTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(uplinkEmailLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(uplinkEmailTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(uplinkTelephoneNumberLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(uplinkTelephoneNumberTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddSection(VideoSection, new SectionLayout(++row, 0));
			row += VideoSection.RowCount;

			AddSection(AudioSection, new SectionLayout(row + 1, 0));

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
