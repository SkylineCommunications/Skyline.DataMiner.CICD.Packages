namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.Utils.YLE.Integrations;
	using static Skyline.DataMiner.Utils.InteractiveAutomationScript.DropDown;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;

	/// <summary>
	/// This section is used to display general service information.
	/// </summary>
	public class GeneralInfoSection : Section
	{
		private readonly Helpers helpers;

		protected readonly Label preRollLabel = new Label("Preroll");
		protected readonly Label startLabel = new Label("Start");
		protected readonly Label endLabel = new Label("End");
		protected readonly Label postRollLabel = new Label("Postroll");
		protected readonly Label timeZoneLabel = new Label("Time Zone");
		protected readonly Label integrationTypeLabel = new Label(String.Empty);
		private readonly Label title = new Label("General Service Information") { Style = TextStyle.Heading };
		private readonly Label serviceStartTimeZoneLabel = new Label("Not in the default time zone") { IsVisible = false };	
		private readonly Label serviceEndTimeZoneLabel = new Label("Not in the default time zone") { IsVisible = false };
		private readonly Label serviceDefinitionTypeSelectionLabel = new Label("Service Type");

		private readonly GeneralInfoSectionConfiguration configuration;

		private TimePicker preRollTimePicker;
		private YleDateTimePicker startDateTimePicker;
		private YleDateTimePicker endDateTimePicker;
		private TimePicker postRollTimePicker;
		private DropDown timeZoneDropDown;
		private VisibilityRightsCheckBoxes visibilityRightsCheckBoxes;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneralInfoSection" /> class.
		/// </summary>
		/// <param name="service">Service of which its general information is displayed in this section.</param>
		/// <param name="configuration">The configuration for this section.</param>
		/// <param name="serviceDefinitionTypeSelectionDropDown">The dropdown containing all service definitions for the virtual platform.</param>
		/// <param name="helpers"></param>
		/// <exception cref="ArgumentNullException"/>
		public GeneralInfoSection(DisplayedService service, GeneralInfoSectionConfiguration configuration, DropDown serviceDefinitionTypeSelectionDropDown = null, Helpers helpers = null)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.helpers = helpers;
            ServiceDefinitionTypeSelectionDropDown = serviceDefinitionTypeSelectionDropDown;

            Initialize();
            GenerateUI();
        }

        /// <summary>
        /// Gets the service that is displayed by this section.
        /// </summary>
        public DisplayedService Service { get; }

        public DropDown ServiceDefinitionTypeSelectionDropDown { get; private set; }

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

		private DateTime Start
		{
			get
			{
				var offset = TimeZoneInfo.Local.BaseUtcOffset - SelectedTimeZone.BaseUtcOffset;
				return startDateTimePicker.DateTime.Add(offset);
			}
			set
			{
				var offset = SelectedTimeZone.BaseUtcOffset - TimeZoneInfo.Local.BaseUtcOffset;
				startDateTimePicker.DateTime = value.Add(offset);
			}
		}

		private DateTime End
		{
			get
			{
				var offset = TimeZoneInfo.Local.BaseUtcOffset - SelectedTimeZone.BaseUtcOffset;
				return endDateTimePicker.DateTime.Add(offset);
			}
			set
			{
				var offset = SelectedTimeZone.BaseUtcOffset - TimeZoneInfo.Local.BaseUtcOffset;
				endDateTimePicker.DateTime = value.Add(offset);
			}
		}

		private TimeZoneInfo SelectedTimeZone
		{
			get
			{
				return TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.DisplayName.Equals(timeZoneDropDown.Selected));
			}
		}

		public void RegenerateUI()
		{
			Clear();
			GenerateUI();
		}

		public event EventHandler<DateTime> StartChanged;

		public event EventHandler<TimeSpan> PrerollChanged;

		public event EventHandler<DateTime> EndChanged;

		public event EventHandler<TimeSpan> PostrollChanged;

		public event EventHandler<Dictionary<string, int>> SecurityViewIdsChanged;

		public event EventHandler RegenerateDialog;

		public event EventHandler<DropDownChangedEventArgs> ServiceTypeChanged;

		/// <summary>
		/// Initializes the widgets within this section and the linking with the underlying model objects.
		/// </summary>
		private void Initialize()
		{
			InitializeWidgets();
			SubscribeToWidgets();
			SubscribeToService();
		}

		private void InitializeWidgets()
		{
			if (Service.IntegrationType != IntegrationType.None)
				integrationTypeLabel.Text = $"This service was automatically created by {Service.IntegrationType.GetDescription()}";

			startDateTimePicker = new YleDateTimePicker(Service.Start.ToLocalTime()) { Name = nameof(startDateTimePicker) };
			preRollTimePicker = new TimePicker(Service.PreRoll) { Minimum = TimeSpan.Zero, TimeInterval = TimeSpan.FromMinutes(1) };
			endDateTimePicker = new YleDateTimePicker(Service.End.ToLocalTime()) { Name = nameof(endDateTimePicker) };
			postRollTimePicker = new TimePicker(Service.PostRoll) { Minimum = TimeSpan.Zero, TimeInterval = TimeSpan.FromMinutes(1) };
			timeZoneDropDown = new DropDown(TimeZoneInfo.GetSystemTimeZones().Select(x => x.DisplayName), TimeZoneInfo.Local.DisplayName) { IsVisible = true, IsDisplayFilterShown = true };

			var localTimeZone = TimeZoneInfo.Local;
			timeZoneDropDown = new DropDown(TimeZoneInfo.GetSystemTimeZones().Select(x => x.DisplayName), localTimeZone.DisplayName) { IsVisible = true, IsDisplayFilterShown = true };
			
			var selectedAndDisabledCompanies = new List<string> { configuration.EventOwnerCompany };
			if (configuration.UserCompanies != null) selectedAndDisabledCompanies.AddRange(configuration.UserCompanies);
			visibilityRightsCheckBoxes = new VisibilityRightsCheckBoxes(Service.SelectableSecurityViewIds ?? new Dictionary<string, int>(), selectedAndDisabledCompanies) { SelectedViewIds = Service.SecurityViewIds ?? new HashSet<int>() };
		}

		private void SubscribeToWidgets()
		{
			startDateTimePicker.Changed += StartDateTimePicker_Changed;
			preRollTimePicker.Changed += (o, e) => PrerollChanged?.Invoke(this, new TimeSpan(preRollTimePicker.Time.Hours, preRollTimePicker.Time.Minutes, 0));
			endDateTimePicker.Changed += EndDateTimePicker_Changed;
			postRollTimePicker.Changed += (o, e) => PostrollChanged?.Invoke(this, new TimeSpan(postRollTimePicker.Time.Hours, postRollTimePicker.Time.Minutes, 0));
			timeZoneDropDown.Changed += TimeZoneDropDown_Changed;

            if (ServiceDefinitionTypeSelectionDropDown != null) ServiceDefinitionTypeSelectionDropDown.Changed += (o, e) => ServiceTypeChanged?.Invoke(this, e);

			visibilityRightsCheckBoxes.SelectedCompaniesChanged += (o, securityViewIds) => SecurityViewIdsChanged?.Invoke(this, securityViewIds);
		}

		private void StartDateTimePicker_Changed(object sender, YleValueWidgetChangedEventArgs e)
		{
			StartChanged?.Invoke(this, Start);

			if (End < Start)
			{
				End = Start;
				EndChanged?.Invoke(this, End);
			}
		}

		private void EndDateTimePicker_Changed(object sender, YleValueWidgetChangedEventArgs e)
		{
			EndChanged?.Invoke(this, End);

			if (End < Start)
			{
				Start = End;
				StartChanged?.Invoke(this, Start);
			}
		}

		private void SubscribeToService()
		{
            Service.StartValidation.ValidationInfoChanged += (o, e) =>
            {
                startDateTimePicker.ValidationState = e.State;
                startDateTimePicker.ValidationText = e.Text;
            };

			Service.PreRollChanged += (sender, newPreroll) => preRollTimePicker.Time = newPreroll;
			Service.StartChanged += (sender, newStart) => Start = newStart.ToLocalTime();
            Service.EndChanged += (sender, newEnd) => End = newEnd.ToLocalTime();
            Service.PostRollChanged += (sender, newPostRoll) => postRollTimePicker.Time = newPostRoll;

            Service.IsSharedSourceChanged += (o, e) => HandleVisibilityAndEnabledUpdate();

            Service.SelectableSecurityViewIdsChanged += Service_SelectableSecurityViewIdsChanged;
		}

		private void Service_SelectableSecurityViewIdsChanged(object sender, Dictionary<string, int> updatedSelectableCompanies)
		{
			visibilityRightsCheckBoxes.UpdateSelectableCompanies(updatedSelectableCompanies, new List<string> { configuration.EventOwnerCompany });
			visibilityRightsCheckBoxes.SelectedViewIds = Service.SecurityViewIds ?? new HashSet<int>();
			RegenerateDialog?.Invoke(this, new EventArgs());
		}

		/// <summary>
		/// Adds the widgets to this section.
		/// </summary>
		private void GenerateUI()
		{
			int row = -1;

			AddWidget(title, ++row, 0, 1, 5);

			AddWidget(preRollLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(preRollTimePicker, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(startLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(startDateTimePicker, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(serviceStartTimeZoneLabel, row, 4);

			AddWidget(endLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(endDateTimePicker, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(serviceEndTimeZoneLabel, ++row, 2);

			AddWidget(postRollLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(postRollTimePicker, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(timeZoneLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(timeZoneDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

            if (ServiceDefinitionTypeSelectionDropDown != null)
            {
                AddWidget(serviceDefinitionTypeSelectionLabel, ++row, 0, 1, configuration.LabelSpan);
                AddWidget(ServiceDefinitionTypeSelectionDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
            }

            AddSection(visibilityRightsCheckBoxes, new SectionLayout(row + 1, 0));

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		private void HandleVisibilityAndEnabledUpdate()
		{
			startDateTimePicker.IsEnabled = IsEnabled && configuration.StartDateIsEnabled;
			startDateTimePicker.IsVisible = IsVisible && configuration.StartDateIsVisible;
			startLabel.IsVisible = IsVisible && configuration.StartDateIsVisible;

			preRollLabel.IsVisible = IsVisible && configuration.PrerollIsVisible;
			preRollTimePicker.IsEnabled = IsEnabled && configuration.PrerollIsEnabled;
			preRollTimePicker.IsVisible = IsVisible && configuration.PrerollIsVisible;

			endDateTimePicker.IsEnabled = IsEnabled && configuration.EndDateIsEnabled;
			endDateTimePicker.IsVisible = IsVisible && configuration.EndDateIsVisible;
            endLabel.IsVisible = IsVisible && configuration.EndDateIsVisible;

			postRollLabel.IsVisible = IsVisible && configuration.PostrollIsVisible;
			postRollTimePicker.IsEnabled = IsEnabled && configuration.PostrollIsEnabled;
			postRollTimePicker.IsVisible = IsVisible && configuration.PostrollIsVisible;

			timeZoneDropDown.IsEnabled = IsEnabled && configuration.TimeZoneIsEnabled;
			timeZoneDropDown.IsVisible = IsVisible && configuration.TimeZoneIsVisible;
			timeZoneLabel.IsVisible = IsVisible && configuration.TimeZoneIsVisible;

			serviceStartTimeZoneLabel.IsVisible = IsVisible && configuration.TimeZoneIsVisible && !TimeZoneInfo.Local.Equals(SelectedTimeZone);
			serviceEndTimeZoneLabel.IsVisible = IsVisible && configuration.TimeZoneIsVisible && !TimeZoneInfo.Local.Equals(SelectedTimeZone);

            if (ServiceDefinitionTypeSelectionDropDown != null)
            {
                serviceDefinitionTypeSelectionLabel.IsVisible = IsVisible && configuration.ServiceDefinitionTypeSelectionIsVisible;
                ServiceDefinitionTypeSelectionDropDown.IsVisible = IsVisible && configuration.ServiceDefinitionTypeSelectionIsVisible;

                ServiceDefinitionTypeSelectionDropDown.IsEnabled = IsEnabled && configuration.ServiceDefinitionTypeSelectionIsEnabled && Service.IntegrationType == IntegrationType.None && Service.Status != Status.ServiceRunning;
            }

            visibilityRightsCheckBoxes.IsVisible = IsVisible && Service.IsSharedSource && configuration.VisibilityRightsIsVisible;
			visibilityRightsCheckBoxes.IsEnabled = IsEnabled && Service.IsSharedSource && configuration.VisibilityRightsIsEnabled;            
			
			ToolTipHandler.SetTooltipVisibility(this);
		}

		private void TimeZoneDropDown_Changed(object sender, DropDownChangedEventArgs e)
		{
			// Get difference between new TimeZone and old TimeZone
			TimeZoneInfo oldTimeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.DisplayName.Equals(e.Previous)) ?? throw new NotFoundException($"Unable to find time zone with display name {e.Previous}");
			TimeZoneInfo newTimeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.DisplayName.Equals(e.Selected)) ?? throw new NotFoundException($"Unable to find time zone with display name {e.Selected}");
			TimeSpan offset = newTimeZone.BaseUtcOffset - oldTimeZone.BaseUtcOffset;

			// Adjust Start- and End Times
			startDateTimePicker.DateTime = startDateTimePicker.DateTime.Add(offset);
			endDateTimePicker.DateTime = endDateTimePicker.DateTime.Add(offset);

			// Update visibility on TimeZone labels
			serviceStartTimeZoneLabel.IsVisible = IsVisible && !TimeZoneInfo.Local.Equals(SelectedTimeZone);
			serviceEndTimeZoneLabel.IsVisible = IsVisible && !TimeZoneInfo.Local.Equals(SelectedTimeZone);
		}
	}
}