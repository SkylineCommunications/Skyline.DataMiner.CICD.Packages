namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class NormalOrderSection : OrderSection
	{
		private readonly YleCollapseButton mainSignalCollapseButton = new YleCollapseButton(true) { Name = nameof(mainSignalCollapseButton) };
		private readonly YleCollapseButton sourceCollapseButton = new YleCollapseButton(true) { Name = nameof(sourceCollapseButton) };
		private readonly YleCollapseButton destinationsCollapseButton = new YleCollapseButton(true) { Name = nameof(destinationsCollapseButton) };
		private readonly YleCollapseButton recordingsCollapseButton = new YleCollapseButton(true) { Name = nameof(recordingsCollapseButton) };
		private readonly YleCollapseButton transmissionsCollapseButton = new YleCollapseButton(true) { Name = nameof(transmissionsCollapseButton) };

		private readonly YleCollapseButton backupSignalCollapseButton = new YleCollapseButton(true) { Name = nameof(backupSignalCollapseButton) };
		private readonly YleCollapseButton backupSourceCollapseButton = new YleCollapseButton(true) { Name = nameof(backupSourceCollapseButton) };
		private readonly YleCollapseButton backupDestinationsCollapseButton = new YleCollapseButton(true) { Name = nameof(backupDestinationsCollapseButton) };
		private readonly YleCollapseButton backupRecordingsCollapseButton = new YleCollapseButton(true) { Name = nameof(backupRecordingsCollapseButton) };
		private readonly YleCollapseButton backupTransmissionsCollapseButton = new YleCollapseButton(true) { Name = nameof(backupTransmissionsCollapseButton) };

		private readonly Label mainSignalTitle = new Label("Main Signal") { Style = TextStyle.Bold };
		private readonly Label sourceTitle = new Label("Source") { Style = TextStyle.Bold };
		private readonly Label backupSignalTitle = new Label("Backup Signal") { Style = TextStyle.Bold };
		private readonly Label backupSourceTitle = new Label("Source") { Style = TextStyle.Bold };

		private readonly Label destinationsTitle = new Label("Destination(s)") { Style = TextStyle.Bold };
		private readonly Label recordingsTitle = new Label("Recording(s)") { Style = TextStyle.Bold };
		private readonly Label transmissionTitle = new Label("Transmission(s)") { Style = TextStyle.Bold };
		private readonly Label backupDestinationsTitle = new Label("Destination(s)") { Style = TextStyle.Bold };
		private readonly Label backupRecordingsTitle = new Label("Recording(s)") { Style = TextStyle.Bold };
		private readonly Label backupTransmissionTitle = new Label("Transmission(s)") { Style = TextStyle.Bold };

		private readonly Label sourceLabel = new Label("Source");
		private readonly YleCheckBox useSharedSourceCheckBox = new YleCheckBox("Use Shared Source");
		private readonly YleCheckBox includeExistingSourcesCheckBox = new YleCheckBox("Include Other Sources");
		private readonly Label sharedSourceLabel = new Label("Shared Source");
		private readonly YleDropDown availableSharedSourceDropDown = new YleDropDown { Name = nameof(availableSharedSourceDropDown) };
		private readonly Label noSharedSourceAvailableLabel = new Label("No Shared Sources Available");
		private readonly Label unavailableSharedSourceLabels = new Label(String.Empty);
		private readonly YleTextBox sharedSourceDescriptionTextBox = new YleTextBox();
		private readonly Label backupSourceLabel = new Label("Backup Source");
		private readonly Label backupSourceServiceLevelsLabel = new Label("Service Levels");
		private readonly YleDropDown backupSourceDropDown = new YleDropDown { Name = nameof(backupSourceDropDown), IsSorted = true };
		private readonly YleDropDown backupSourceDescriptionDropDown = new YleDropDown { Name = nameof(backupSourceDescriptionDropDown), IsSorted = true };
		private readonly YleDropDown backupSourceServiceLevelDropdown = new YleDropDown { Name = nameof(backupSourceServiceLevelDropdown), IsSorted = true };

		private readonly YleButton addDestinationButton = new YleButton("Add Destination") { Width = 150 };
		private readonly YleButton addRecordingButton = new YleButton("Add Recording") { Width = 150 };
		private readonly YleButton addTransmissionButton = new YleButton("Add Transmission") { Width = 150 };

		public NormalOrderSection(Helpers helpers, Order order, OrderSectionConfiguration configuration, UserInfo userInfo) : base(helpers, order, configuration, userInfo)
		{
			Initialize();
			RegenerateUi();
		}

		/// <summary>
		/// Gets the subsection that displays additional information.
		/// </summary>
		public AdditionalInformationSection AdditionalInformationSection { get; private set; }

		/// <summary>
		/// Gets the subsection that displays sports information.
		/// </summary>
		public SportsPlanningSection SportsPlanningSection { get; private set; }

		/// <summary>
		/// Gets the subsection that displays news information.
		/// </summary>
		public NewsInformationSection NewsInformationSection { get; private set; }

		public string BackupSource => backupSourceDropDown.Selected;

		public string BackupSourceDescription => backupSourceDescriptionDropDown.Selected;

		public BackupType BackupSourceServiceLevel => backupSourceServiceLevelDropdown.Selected.GetEnumValue<BackupType>();

		public string ExternalSource => availableSharedSourceDropDown.Selected;

		public bool UseSharedSource => useSharedSourceCheckBox.IsChecked;

		public bool IncludeExistingSources => includeExistingSourcesCheckBox.IsChecked;

		public event EventHandler<bool> UseSharedSourceChanged;

		public event EventHandler<bool> IncludeExternalSourcesChanged;

		public event EventHandler<string> SharedSourceChanged;

		public event EventHandler AddDestination;

		public event EventHandler AddRecording;

		public event EventHandler AddTransmission;

		public event EventHandler<string> BackupSourceChanged;

		public event EventHandler<string> BackupSourceDescriptionChanged;

		public event EventHandler<BackupType> BackupSourceServiceLevelChanged;

		public event EventHandler<ServiceSection> BackupSourceServiceSectionAdded;

		public ServiceSection BackupSourceServiceSection => serviceSections.BackupSourceServiceSection;

		public IEnumerable<ServiceSelectionSection> BackupSourceChildSections => serviceSections.BackupEndpointSections.SelectMany(x => x.Value).Select(x => x.DisplayedSection);

		public override List<Section> GetServiceSections(Service service)
		{
			if (service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception)
			{
				if (service.BackupType == BackupType.None)
				{
					return ((Section)SourceServiceSection).Yield().ToList();
				}
				else
				{
					return ((Section)BackupSourceServiceSection).Yield().ToList();
				}
			}
			else if (service.IsAutogenerated || service.IsRecordingAfterDestination(order))
			{
				return serviceSections.SubSections.Values.SelectMany(x => x).Where(sss => sss.Service.Equals(service)).Select(sss => sss.ServiceSection).Cast<Section>().ToList();
			}
			else if (service.BackupType == BackupType.None)
			{
				return SourceChildSections.SingleOrDefault(s => s.Service.Equals(service))?.Yield()?.ToList<Section>() ?? throw new NotFoundException($"Unable to find section for service {service.Name}");
			}
			else
			{
				return BackupSourceChildSections.SingleOrDefault(s => s.Service.Equals(service))?.Yield()?.ToList<Section>() ?? throw new NotFoundException($"Unable to find section for service {service.Name}");
			}
		}

		public void UseSharedSources(bool includeExistingSources)
		{
			useSharedSourceCheckBox.IsChecked = true;
			includeExistingSourcesCheckBox.IsChecked = includeExistingSources;

			UseSharedSourceChanged?.Invoke(this, UseSharedSource);
			IncludeExternalSourcesChanged?.Invoke(this, IncludeExistingSources);
		}

		/// <summary>
		/// Initializes the widgets within this section and the linking with the underlying model objects.
		/// </summary>
		private void Initialize()
		{
			using (StartPerformanceLogging())
			{
				EnableLogging();
				InitializeWidgets();
				InitializeSections();
				SubscribeToWidgets();
				SubscribeToOrder();
			}
		}

		protected override void SubscribeToOrder()
		{
			base.SubscribeToOrder();

			order.AvailableSharedSources.CollectionChanged += (s, e) =>
			{
				UpdateAvailableSharedSources();
				RegenerateUi();
			};

			order.UnavailableSharedSources.CollectionChanged += (s, e) =>
			{
				UpdateUnavailableSharedSources();
				RegenerateUi();
			};

			order.AvailableSharedSourcesValidationChanged += (s, e) =>
			{
				availableSharedSourceDropDown.ValidationState = e.State;
				availableSharedSourceDropDown.ValidationText = e.Text;
			};

			order.AvailableBackupSourceServiceDescriptionsChanged += (s, e) => Order_AvailableBackupSourceServiceDescriptionsChanged(e);

			order.AvailableBackupSourceServicesChanged += (s, e) => Order_AvailableBackupSourceServicesChanged(e);

			order.BackupSourceServiceChanged += (s, e) => UpdateBackupSourceServiceSection(e);
		}

		protected override void SubscribeToWidgets()
		{
			base.SubscribeToWidgets();

			mainSignalCollapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();
			sourceCollapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();
			destinationsCollapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();
			recordingsCollapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();
			transmissionsCollapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();
			backupSignalCollapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();
			backupSourceCollapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();
			backupDestinationsCollapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();
			backupRecordingsCollapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();
			backupTransmissionsCollapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();

			useSharedSourceCheckBox.Changed += (s, e) =>
			{
				UseSharedSourceChanged?.Invoke(this, UseSharedSource);
				RegenerateUi();
			};

			includeExistingSourcesCheckBox.Changed += (s, e) =>
			{
				IncludeExternalSourcesChanged?.Invoke(this, IncludeExistingSources);
				RegenerateUi();
			};

			availableSharedSourceDropDown.Changed += (s, e) => SharedSourceChanged?.Invoke(this, Convert.ToString(e.Value));

			backupSourceDropDown.Changed += (s, e) => BackupSourceChanged?.Invoke(this, backupSourceDropDown.Selected);
			backupSourceDescriptionDropDown.Changed += (s, e) => BackupSourceDescriptionChanged?.Invoke(this, backupSourceDescriptionDropDown.Selected);
			backupSourceServiceLevelDropdown.Changed += (s, e) =>
			{
				BackupSourceServiceLevelChanged?.Invoke(this, BackupSourceServiceLevel);
				HandleVisibilityAndEnabledUpdate();
			};

			addDestinationButton.Pressed += (s, e) => AddDestination?.Invoke(this, new EventArgs());
			addRecordingButton.Pressed += (s, e) => AddRecording?.Invoke(this, new EventArgs());
			addTransmissionButton.Pressed += (s, e) => AddTransmission?.Invoke(this, new EventArgs());
		}

		protected override void InitializeOtherSections()
		{
			base.InitializeOtherSections();

			AdditionalInformationSection = new AdditionalInformationSection(order, configuration.AdditionalInformationSectionConfiguration);
			SportsPlanningSection = new SportsPlanningSection(order.SportsPlanning, configuration.SportsPlanningSectionConfiguration);
			NewsInformationSection = new NewsInformationSection(order.NewsInformation, configuration.NewsInformationSectionConfiguration);
		}

		protected override void InitializeServiceSections()
		{
			base.InitializeServiceSections();

			if (order.BackupSourceService != null)
			{
				UpdateBackupSourceServiceSection(order.BackupSourceService);

				foreach (var backupDescendant in order.BackupSourceService.Descendants)
				{
					AddOrReplaceChildServiceSection(backupDescendant);
				}
			}

			AdditionalInformationSection = new AdditionalInformationSection(order, configuration.AdditionalInformationSectionConfiguration);
			SportsPlanningSection = new SportsPlanningSection(order.SportsPlanning, configuration.SportsPlanningSectionConfiguration);
			NewsInformationSection = new NewsInformationSection(order.NewsInformation, configuration.NewsInformationSectionConfiguration);
		}

		protected override void UpdateSourceServiceSection(object sender, Service service)
		{
			base.UpdateSourceServiceSection(sender, service);

			if (!useSharedSourceCheckBox.IsChecked) sourceDescriptionDropDown.Selected = service.Definition.Description; // To make sure that we still have the last orderName values when returning to regular source service

			sharedSourceDescriptionTextBox.Text = service.SharedSourceDescription;
		}

		protected override void InitializeWidgets()
		{
			base.InitializeWidgets();

			mainSignalCollapseButton.IsCollapsed = configuration.MainSignalIsCollapsed;
			sourceCollapseButton.IsCollapsed = configuration.SourceIsCollapsed;
			destinationsCollapseButton.IsCollapsed = configuration.DestinationsAreCollapsed;
			recordingsCollapseButton.IsCollapsed = configuration.RecordingsAreCollapsed;
			transmissionsCollapseButton.IsCollapsed = configuration.TransmissionsAreCollapsed;
			backupSignalCollapseButton.IsCollapsed = configuration.BackupSignalIsCollapsed;
			backupSourceCollapseButton.IsCollapsed = configuration.BackupSourceIsCollapsed;
			backupDestinationsCollapseButton.IsCollapsed = configuration.BackupDestinationsAreCollapsed;
			backupRecordingsCollapseButton.IsCollapsed = configuration.BackupRecordingsAreCollapsed;
			backupTransmissionsCollapseButton.IsCollapsed = configuration.BackupTransmissionsAreCollapsed;

			backupSourceDropDown.SetOptions(order.AvailableBackupSourceServices);
			backupSourceDescriptionDropDown.SetOptions(order.AvailableBackupSourceServiceDescriptions);
			backupSourceServiceLevelDropdown.SetOptions((new[] { BackupType.Cold, BackupType.StandBy, BackupType.Active }).Select(x => x.GetDescription()));

			backupSourceDropDown.Selected = (order.BackupSourceService == null) ? VirtualPlatformName.None.GetDescription() : order.BackupSourceService.Definition.VirtualPlatformServiceName.GetDescription();
			backupSourceDescriptionDropDown.Selected = (order.BackupSourceService == null) ? String.Empty : order.BackupSourceService.Definition.Description;
			backupSourceServiceLevelDropdown.Selected = (order.BackupSourceService == null) ? BackupType.Cold.GetDescription() : order.BackupSourceService.BackupType.GetDescription();

			useSharedSourceCheckBox.IsChecked = order.SourceService.IsSharedSource && order.SourceService.IsBooked;
			sharedSourceDescriptionTextBox.Text = order.SourceService.SharedSourceDescription;

			UpdateAvailableSharedSources();
			UpdateUnavailableSharedSources();
		}

		private void UpdateAvailableSharedSources()
		{
			var options = order.AvailableSharedSources.Select(ss => ss.DropDownOption).OrderBy(x => x).ToList();

			helpers.Log(nameof(NormalOrderSection), nameof(UpdateAvailableSharedSources), $"Available Options: {String.Join(", ", options)}");

			availableSharedSourceDropDown.Options = options;

			var sharedSourcePartOfOrder = order.AvailableSharedSources.SingleOrDefault(ss => ss.Reservation.Name == order.SourceService?.Name);

			availableSharedSourceDropDown.Selected = order.SourceService.IsSharedSource && availableSharedSourceDropDown.Options.Contains(sharedSourcePartOfOrder?.DropDownOption) ? sharedSourcePartOfOrder?.DropDownOption : availableSharedSourceDropDown.Options.FirstOrDefault();
		}

		private void UpdateUnavailableSharedSources()
		{
			var unavailableSharedSources = order.UnavailableSharedSources.Select(x => $"{x.DropDownOption} ({x.SharedSourceStartWithPreRoll.ToString("g", CultureInfo.InvariantCulture)} - {x.SharedSourceEndWithPostRoll.ToString("g", CultureInfo.InvariantCulture)})");

			helpers.Log(nameof(NormalOrderSection), nameof(UpdateUnavailableSharedSources), $"Unavailable Options: {String.Join(", ", unavailableSharedSources)}");

			var sb = new StringBuilder();
			sb.AppendLine("Change Order timing to make the following external Sources selectable:");
			foreach (string unavailableSharedSource in unavailableSharedSources) sb.AppendLine($" \u2022 {unavailableSharedSource}");
			unavailableSharedSourceLabels.Text = sb.ToString();
		}

		private void Order_AvailableBackupSourceServicesChanged(IReadOnlyList<string> availableBackupSourceServices)
		{
			// In case the current user is not allowed to select the service type of the existing service
			var options = new List<string>(availableBackupSourceServices);
			if (order.BackupSourceService != null && !availableBackupSourceServices.Contains(order.BackupSourceService.Definition.VirtualPlatformServiceName.GetDescription())) options.Add(order.BackupSourceService.Definition.VirtualPlatformServiceName.GetDescription());

			backupSourceDropDown.Options = availableBackupSourceServices;
			if (order.BackupSourceService != null) backupSourceDropDown.Selected = order.BackupSourceService.Definition.VirtualPlatformServiceName.GetDescription();
		}

		private void Order_AvailableBackupSourceServiceDescriptionsChanged(IReadOnlyList<string> availableBackupSourceServiceDescriptions)
		{
			// In case the current user is not allowed to select the service type of the existing service
			var options = new List<string>(availableBackupSourceServiceDescriptions);
			if (order.BackupSourceService != null && !availableBackupSourceServiceDescriptions.Contains(order.BackupSourceService.Definition.Description)) options.Add(order.BackupSourceService.Definition.Description);

			backupSourceDescriptionDropDown.Options = availableBackupSourceServiceDescriptions;
			if (order.BackupSourceService != null) backupSourceDescriptionDropDown.Selected = order.BackupSourceService.Definition.Description;
		}

		private void UpdateBackupSourceServiceSection(Service backupService)
		{
			if (backupService == null)
			{
				// No backup service required (= None)
				serviceSections.BackupSourceServiceSection = null;
			}
			else
			{
				if (backupService.BackupType == BackupType.None) throw new NotSupportedException("Backup source service cannot be of BackupType.None");

				if (!serviceSections.CachedBackupSourceServiceSections.TryGetValue(backupService.Id, out ServiceSection cachedBackupServiceSection))
				{
					backupService.Children.CollectionChanged += ChildrenChanged;

					if (!configuration.BackupSourceServiceSectionConfigurations.TryGetValue(backupService.Id, out var serviceSectionConfiguration))
					{
						serviceSectionConfiguration = ServiceSectionConfiguration.CreateLiveOrderFormConfiguration(helpers, backupService, userInfo, configuration.EventOwnerCompany, configuration.UserCompanies);
						configuration.BackupSourceServiceSectionConfigurations.Add(backupService.Id, serviceSectionConfiguration);
					}

					cachedBackupServiceSection = new ServiceSection(helpers, backupService as DisplayedService, serviceSectionConfiguration, userInfo, null);

					cachedBackupServiceSection.GeneralInfoSection.RegenerateDialog += (sender, args) => InvokeRegenerateUi();
					cachedBackupServiceSection.RecordingConfigurationSection.RegenerateDialog += (sender, args) => InvokeRegenerateUi();

					serviceSections.CachedBackupSourceServiceSections[backupService.Id] = cachedBackupServiceSection;
					BackupSourceServiceSectionAdded?.Invoke(this, cachedBackupServiceSection);
				}

				serviceSections.BackupSourceServiceSection = cachedBackupServiceSection;
				backupSourceDescriptionDropDown.Selected = backupService.Definition.Description;
			}

			InvokeRegenerateUi();
		}

		/// <summary>
		/// Updates the visibility of the widgets and underlying sections.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		protected override void HandleVisibilityAndEnabledUpdate()
		{
			using (StartPerformanceLogging())
			{
				mainSignalCollapseButton.IsVisible = IsVisible && configuration.MainSignalIsVisible && configuration.MainSignalTitleIsVisible;
				mainSignalCollapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;
				mainSignalTitle.IsVisible = IsVisible && configuration.MainSignalIsVisible && configuration.MainSignalTitleIsVisible;

				sourceCollapseButton.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && configuration.SourceIsVisible && configuration.SourceTitleIsVisible;
				sourceCollapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;
				sourceTitle.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && configuration.SourceIsVisible && configuration.SourceTitleIsVisible;

				sourceLabel.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !sourceCollapseButton.IsCollapsed && configuration.SourceIsVisible && configuration.SourceLabelIsVisible;

				useSharedSourceCheckBox.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !sourceCollapseButton.IsCollapsed && configuration.SourceIsVisible && configuration.SharedSourceOptionsAreVisible;
				useSharedSourceCheckBox.IsEnabled = IsEnabled && configuration.UseSharedSourceIsEnabled;

				includeExistingSourcesCheckBox.IsVisible = IsVisible && useSharedSourceCheckBox.IsVisible && useSharedSourceCheckBox.IsChecked && configuration.SharedSourceOptionsAreVisible;
				includeExistingSourcesCheckBox.IsEnabled = IsEnabled && configuration.UseSharedSourceIsEnabled;

				HandleMainVisibilityAndEnabledUpdate(!mainSignalCollapseButton.IsCollapsed && !sourceCollapseButton.IsCollapsed && !useSharedSourceCheckBox.IsChecked, !mainSignalCollapseButton.IsCollapsed && !sourceCollapseButton.IsCollapsed && (useSharedSourceCheckBox.IsChecked && order.AvailableSharedSources.Any() || !useSharedSourceCheckBox.IsChecked));

				sharedSourceLabel.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !sourceCollapseButton.IsCollapsed && useSharedSourceCheckBox.IsChecked && order.AvailableSharedSources.Any() && configuration.SourceIsVisible;

				availableSharedSourceDropDown.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !sourceCollapseButton.IsCollapsed && useSharedSourceCheckBox.IsChecked && order.AvailableSharedSources.Any() && configuration.SourceIsVisible;
				availableSharedSourceDropDown.IsEnabled = IsEnabled;

				sharedSourceDescriptionTextBox.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !sourceCollapseButton.IsCollapsed && useSharedSourceCheckBox.IsChecked && order.AvailableSharedSources.Any() && configuration.SourceIsVisible;
				sharedSourceDescriptionTextBox.IsEnabled = false;

				noSharedSourceAvailableLabel.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !sourceCollapseButton.IsCollapsed && useSharedSourceCheckBox.IsChecked && !order.AvailableSharedSources.Any() && !order.UnavailableSharedSources.Any() && configuration.SourceIsVisible;

				unavailableSharedSourceLabels.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !sourceCollapseButton.IsCollapsed && useSharedSourceCheckBox.IsChecked && order.UnavailableSharedSources.Any() && configuration.SourceIsVisible;

				destinationsCollapseButton.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && configuration.DestinationsIsVisible && configuration.DestinationsTitleIsVisible;
				destinationsCollapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;
				destinationsTitle.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && configuration.DestinationsIsVisible && configuration.DestinationsTitleIsVisible;

				foreach (var destinationSectionCollection in serviceSections.EndpointSections[VirtualPlatformType.Destination])
				{
					destinationSectionCollection.SetVisibility(IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !destinationsCollapseButton.IsCollapsed && configuration.DestinationsIsVisible, configuration.CollapsableServiceSelectionSectionConfigurations);
					destinationSectionCollection.SetEnabled(IsEnabled);
				}

				addDestinationButton.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !destinationsCollapseButton.IsCollapsed && configuration.DestinationsIsVisible && configuration.AddDestinationButtonIsVisible;
				addDestinationButton.IsEnabled = IsEnabled && configuration.AddDestinationButtonIsEnabled && !order.SourceService.Definition.IsSourceOnly;

				recordingsCollapseButton.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && configuration.RecordingsIsVisible && configuration.RecordingsTitleIsVisible;
				recordingsCollapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;
				recordingsTitle.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && configuration.RecordingsIsVisible && configuration.RecordingsTitleIsVisible;

				foreach (var recordingSectionCollection in serviceSections.EndpointSections[VirtualPlatformType.Recording])
				{
					recordingSectionCollection.SetVisibility(IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !recordingsCollapseButton.IsCollapsed && configuration.RecordingsIsVisible, configuration.CollapsableServiceSelectionSectionConfigurations);
					recordingSectionCollection.SetEnabled(IsEnabled);
				}

				addRecordingButton.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !recordingsCollapseButton.IsCollapsed && configuration.RecordingsIsVisible && configuration.AddRecordingButtonIsVisible;
				addRecordingButton.IsEnabled = IsEnabled && configuration.AddRecordingButtonIsEnabled && !order.SourceService.Definition.IsSourceOnly;

				transmissionsCollapseButton.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && configuration.TransmissionsIsVisible && configuration.TransmissionsTitleIsVisible;
				transmissionsCollapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;
				transmissionTitle.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && configuration.TransmissionsIsVisible && configuration.TransmissionsTitleIsVisible;

				foreach (var transmissionSectionCollection in serviceSections.EndpointSections[VirtualPlatformType.Transmission])
				{
					transmissionSectionCollection.SetVisibility(IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !transmissionsCollapseButton.IsCollapsed && configuration.TransmissionsIsVisible, configuration.CollapsableServiceSelectionSectionConfigurations);
					transmissionSectionCollection.SetEnabled(IsEnabled);
				}

				addTransmissionButton.IsVisible = IsVisible && !mainSignalCollapseButton.IsCollapsed && configuration.MainSignalIsVisible && !transmissionsCollapseButton.IsCollapsed && configuration.TransmissionsIsVisible && configuration.AddTransmissionButtonIsVisible;
				addTransmissionButton.IsEnabled = IsEnabled && configuration.AddTransmissionButtonIsEnabled && !order.SourceService.Definition.IsSourceOnly;

				backupSignalCollapseButton.IsVisible = IsVisible && configuration.BackupSignalIsVisible && configuration.BackupSignalTitleIsVisible;
				backupSignalCollapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;
				backupSignalTitle.IsVisible = IsVisible && configuration.BackupSignalIsVisible && configuration.BackupSignalTitleIsVisible;

				backupSourceCollapseButton.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && configuration.BackupSignalIsVisible;
				backupSourceCollapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;
				backupSourceTitle.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible;

				backupSourceLabel.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && !backupSourceCollapseButton.IsCollapsed;
				backupSourceDropDown.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && !backupSourceCollapseButton.IsCollapsed;
				backupSourceDropDown.IsEnabled = IsEnabled;

				backupSourceDescriptionDropDown.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && !backupSourceCollapseButton.IsCollapsed && backupSourceDescriptionDropDown.Options.Count() > 1;
				backupSourceDescriptionDropDown.IsEnabled = IsEnabled;

				backupSourceServiceLevelsLabel.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && !backupSourceCollapseButton.IsCollapsed && order.BackupSourceService != null;
				backupSourceServiceLevelDropdown.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && !backupSourceCollapseButton.IsCollapsed && order.BackupSourceService != null;
				backupSourceDescriptionDropDown.IsEnabled = IsEnabled;

				if (BackupSourceServiceSection != null)
				{
					BackupSourceServiceSection.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && !backupSourceCollapseButton.IsCollapsed;
					BackupSourceServiceSection.IsEnabled = IsEnabled;
				}

				backupDestinationsCollapseButton.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && BackupSourceServiceLevel == BackupType.Active;
				backupDestinationsCollapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;
				backupDestinationsTitle.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && BackupSourceServiceLevel == BackupType.Active;

				foreach (var destinationSectionCollection in serviceSections.BackupEndpointSections[VirtualPlatformType.Destination])
				{
					destinationSectionCollection.SetVisibility(IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && !backupDestinationsCollapseButton.IsCollapsed, configuration.CollapsableServiceSelectionSectionConfigurations);
					destinationSectionCollection.SetEnabled(IsEnabled);
				}

				backupRecordingsCollapseButton.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && BackupSourceServiceLevel == BackupType.Active;
				backupRecordingsCollapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;
				backupRecordingsTitle.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && BackupSourceServiceLevel == BackupType.Active;

				foreach (var recordingSectionCollection in serviceSections.BackupEndpointSections[VirtualPlatformType.Recording])
				{
					recordingSectionCollection.SetVisibility(IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && !backupRecordingsCollapseButton.IsCollapsed, configuration.CollapsableServiceSelectionSectionConfigurations);
					recordingSectionCollection.SetEnabled(IsEnabled);
				}

				backupTransmissionsCollapseButton.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && BackupSourceServiceLevel == BackupType.Active;
				backupTransmissionsCollapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;
				backupTransmissionTitle.IsVisible = IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && BackupSourceServiceLevel == BackupType.Active;

				foreach (var transmissionSectionCollection in serviceSections.BackupEndpointSections[VirtualPlatformType.Transmission])
				{
					transmissionSectionCollection.SetVisibility(IsVisible && !backupSignalCollapseButton.IsCollapsed && configuration.BackupSignalIsVisible && !backupTransmissionsCollapseButton.IsCollapsed, configuration.CollapsableServiceSelectionSectionConfigurations);
					transmissionSectionCollection.SetEnabled(IsEnabled);
				}

				AdditionalInformationSection.IsVisible = IsVisible && configuration.AdditionalInformationSectionConfiguration.IsVisible && configuration.AdditionalInformationSectionConfiguration.IsVisible;
				AdditionalInformationSection.IsEnabled = IsEnabled && configuration.AdditionalInformationSectionConfiguration.IsEnabled;

				SportsPlanningSection.IsVisible = IsVisible && configuration.SportsPlanningSectionConfiguration.IsVisible && configuration.SportsPlanningSectionConfiguration.IsVisible;
				SportsPlanningSection.IsEnabled = IsEnabled && configuration.SportsPlanningSectionConfiguration.IsEnabled;

				NewsInformationSection.IsVisible = IsVisible && configuration.NewsInformationSectionConfiguration.IsVisible && configuration.NewsInformationSectionConfiguration.IsVisible;
				NewsInformationSection.IsEnabled = IsEnabled && configuration.NewsInformationSectionConfiguration.IsEnabled;

				helpers.LogMethodStart(nameof(ToolTipHandler), nameof(ToolTipHandler.SetTooltipVisibility), out var stopwatch);
				ToolTipHandler.SetTooltipVisibility(this);
				helpers.LogMethodCompleted(nameof(ToolTipHandler), nameof(ToolTipHandler.SetTooltipVisibility), null, stopwatch);
			}
		}

		protected override void GenerateUi()
		{
			using (StartPerformanceLogging())
			{
				Clear();

				int row = -1;

				GenerateHeaderUi(ref row);

				AddWidget(mainSignalCollapseButton, ++row, 0);
				AddWidget(mainSignalTitle, row, 1, 1, 8);

				AddWidget(sourceCollapseButton, ++row, 1);
				AddWidget(sourceTitle, row, configuration.LabelColumn, 1, 7);

				AddWidget(sourceLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);
				AddWidget(useSharedSourceCheckBox, row, configuration.InputColumn, 1, configuration.InputSpan);
				AddWidget(includeExistingSourcesCheckBox, ++row, configuration.InputColumn, 1, configuration.InputSpan);

				AddWidget(noSharedSourceAvailableLabel, ++row, configuration.InputColumn, 1, configuration.InputSpan);

				AddWidget(unavailableSharedSourceLabels, ++row, configuration.InputColumn, 1, configuration.InputSpan);

				AddWidget(sharedSourceLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);
				AddWidget(availableSharedSourceDropDown, row, configuration.InputColumn, 1, configuration.InputSpan);
				AddWidget(sharedSourceDescriptionTextBox, ++row, configuration.InputColumn, 1, configuration.InputSpan);

				GenerateSourceUi(ref row);

				AddWidget(destinationsCollapseButton, ++row, 1);
				AddWidget(destinationsTitle, row, configuration.LabelColumn, 1, 7);

				foreach (var serviceSection in serviceSections.EndpointSections[VirtualPlatformType.Destination].Select(x => x.DisplayedSection))
				{
					AddSection(serviceSection, new SectionLayout(++row, configuration.LabelColumn));
					row += serviceSection.RowCount;
				}

				AddWidget(addDestinationButton, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);

				AddWidget(recordingsCollapseButton, ++row, 1);
				AddWidget(recordingsTitle, row, configuration.LabelColumn, 1, 7);

				foreach (var serviceSection in serviceSections.EndpointSections[VirtualPlatformType.Recording].Select(x => x.DisplayedSection))
				{
					AddSection(serviceSection, new SectionLayout(++row, configuration.LabelColumn));
					row += serviceSection.RowCount;
				}

				AddWidget(addRecordingButton, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);

				AddWidget(transmissionsCollapseButton, ++row, 1);
				AddWidget(transmissionTitle, row, configuration.LabelColumn, 1, 7);

				foreach (var serviceSection in serviceSections.EndpointSections[VirtualPlatformType.Transmission].Select(x => x.DisplayedSection))
				{
					AddSection(serviceSection, new SectionLayout(++row, configuration.LabelColumn));
					row += serviceSection.RowCount;
				}

				AddWidget(addTransmissionButton, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);

				AddWidget(backupSignalCollapseButton, ++row, 0);
				AddWidget(backupSignalTitle, row, 1, 1, 8);

				AddWidget(backupSourceCollapseButton, ++row, 1);
				AddWidget(backupSourceTitle, row, configuration.LabelColumn, 1, 7);

				AddWidget(backupSourceLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);
				AddWidget(backupSourceDropDown, row, configuration.InputColumn, 1, configuration.InputSpan);
				AddWidget(backupSourceDescriptionDropDown, ++row, configuration.InputColumn, 1, configuration.InputSpan);
				AddWidget(backupSourceServiceLevelsLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);
				AddWidget(backupSourceServiceLevelDropdown, row, configuration.InputColumn, 1, configuration.InputSpan);

				if (serviceSections.BackupSourceServiceSection != null)
				{
					AddSection(serviceSections.BackupSourceServiceSection, new SectionLayout(++row, configuration.LabelColumn));
					row += serviceSections.BackupSourceServiceSection.RowCount;
				}

				AddWidget(backupDestinationsCollapseButton, ++row, 1);
				AddWidget(backupDestinationsTitle, row, configuration.LabelColumn, 1, 7);

				foreach (var serviceSection in serviceSections.BackupEndpointSections[VirtualPlatformType.Destination].Select(x => x.DisplayedSection))
				{
					AddSection(serviceSection, new SectionLayout(++row, configuration.LabelColumn));
					row += serviceSection.RowCount;
				}

				AddWidget(backupRecordingsCollapseButton, ++row, 1);
				AddWidget(backupRecordingsTitle, row, configuration.LabelColumn, 1, 7);

				foreach (var serviceSection in serviceSections.BackupEndpointSections[VirtualPlatformType.Recording].Select(x => x.DisplayedSection))
				{
					AddSection(serviceSection, new SectionLayout(++row, configuration.LabelColumn));
					row += serviceSection.RowCount;
				}

				AddWidget(backupTransmissionsCollapseButton, ++row, 1);
				AddWidget(backupTransmissionTitle, row, configuration.LabelColumn, 1, 7);

				foreach (var serviceSection in serviceSections.BackupEndpointSections[VirtualPlatformType.Transmission].Select(x => x.DisplayedSection))
				{
					AddSection(serviceSection, new SectionLayout(++row, configuration.LabelColumn));
					row += serviceSection.RowCount;
				}

				AddSection(AdditionalInformationSection, new SectionLayout(++row, 0));
				row += AdditionalInformationSection.RowCount;

				AddSection(SportsPlanningSection, new SectionLayout(++row, 0));
				row += SportsPlanningSection.RowCount;

				AddSection(NewsInformationSection, new SectionLayout(row + 1, 0));

				ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
			}
		}
	}
}
