namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ServiceSelectionSection : YleSection, ICloneable
    {
        private readonly YleCollapseButton collapseButton = new YleCollapseButton(true);
        private readonly YleButton deleteButton = new YleButton("Delete");
        private readonly Label virtualPlatformTypeLabel = new Label();
        private readonly Label virtualPlatformTypeLabel2 = new Label();
        private readonly YleDropDown serviceVirtualPlatformNamesDropDown = new YleDropDown { Name = nameof(serviceVirtualPlatformNamesDropDown) };
        private readonly YleDropDown serviceDefinitionDescriptionDropDown = new YleDropDown { Name = nameof(serviceDefinitionDescriptionDropDown) };

        private readonly Label serviceNameLabel = new Label { Style = TextStyle.Bold };
        private readonly Label linkedRecordingsTitleLabel = new Label("Linked Recordings") { Style = TextStyle.Bold };

        private readonly ServiceSelectionSectionConfiguration configuration;
        private readonly UserInfo userInfo;

        private ServiceSection serviceSection;

		private ServiceSelectionSection(ServiceSelectionSection other) : this(other.helpers, other.Service, other.configuration, other.userInfo)
		{

		}

        public ServiceSelectionSection(Helpers helpers, DisplayedService service, ServiceSelectionSectionConfiguration configuration, UserInfo userInfo) : base(helpers)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));

            Initialize();
            GenerateUi();
			HandleVisibilityAndEnabledUpdate();
		}

		public DisplayedService Service { get; private set; }

        public ServiceSection ServiceSection => serviceSection;

        public List<ServiceSelectionSection> SubServiceSections { get; private set; } = new List<ServiceSelectionSection>();

        public string SelectedVirtualPlatformName => serviceVirtualPlatformNamesDropDown.Selected;

        public event EventHandler<string> ServiceVirtualPlatformNameChanged;

        public event EventHandler<string> ServiceDescriptionChanged;

        public event EventHandler DeleteButtonPressed;

		public override void RegenerateUi()
		{
			Clear();
			ServiceSection.RegenerateUi();

			foreach (var subServiceSection in SubServiceSections)
			{
				subServiceSection.RegenerateUi();
			}

			GenerateUi();
		}

		public void SetSubServiceSections(IEnumerable<ServiceSelectionSection> subServiceSelectionSections)
		{
			SubServiceSections = subServiceSelectionSections.ToList();

			helpers.Log(nameof(ServiceSelectionSection), nameof(SetSubServiceSections), $"Set sub service sections {string.Join(", ", subServiceSelectionSections.Select(s => s.ToString()))}", Service.Name);

			RegenerateUi();

			InvokeRegenerateUi();
		}

		public override string ToString()
		{
			return $"Section {serviceSection.ID} (service {Service.Name})";
		}

		/// <summary>
		/// Initializes the widgets within this section and the linking with the underlying model objects.
		/// </summary>
		private void Initialize()
        {
            IntializeWidgets();
            InitializeSections();
            SubscribeToWidgets();
            SubscribeToService();
        }

        private void IntializeWidgets()
        {
            collapseButton.IsCollapsed = configuration.IsCollapsed;

            // Finnish date time format: d.M.yyyy H.mm
            string dateTimeFormat = "d.M.yyyy H.mm";
            serviceNameLabel.Text = configuration.DisplayServiceTimingsInTitle ? $"{Service.Definition.Name.Trim('_')} [{Service.Start.ToString(dateTimeFormat)} - {Service.End.ToString(dateTimeFormat)}]" : Service.LofDisplayName;
            virtualPlatformTypeLabel.Text = Service.Definition.VirtualPlatformServiceType.GetDescription();
            virtualPlatformTypeLabel2.Text = Service.Definition.VirtualPlatform == VirtualPlatform.Destination || Service.Definition.VirtualPlatform == VirtualPlatform.Recording ? Service.Definition.VirtualPlatformServiceType.GetDescription() : string.Empty;

            collapseButton.Name = serviceNameLabel.Text;

            serviceVirtualPlatformNamesDropDown.Options = Service.AvailableVirtualPlatformNames;
            serviceVirtualPlatformNamesDropDown.Selected = Service.Definition.VirtualPlatformServiceName.GetDescription();

            var serviceDefinitionDescriptionDropDownOptions = new HashSet<string>(Service.AvailableServiceDescriptions);
            serviceDefinitionDescriptionDropDownOptions.Add(Service.Definition.Description ?? Service.Definition.Name);

            serviceDefinitionDescriptionDropDown.Options = serviceDefinitionDescriptionDropDownOptions;
            serviceDefinitionDescriptionDropDown.Selected = Service.Definition.Description;
        }

        private void InitializeSections()
        {
            serviceSection = new ServiceSection(helpers, Service, configuration.ServiceSectionConfiguration, userInfo, null);

			Identifier = ToString();
        }

        private void SubscribeToWidgets()
        {
            serviceVirtualPlatformNamesDropDown.Changed += (s, e) =>
            {
                // Fire event with selected value and set back to actual current service definition virtual platform name
                ServiceVirtualPlatformNameChanged?.Invoke(this, Convert.ToString(e.Value));
                serviceVirtualPlatformNamesDropDown.Selected = Service.Definition.VirtualPlatformServiceName.GetDescription();
            };

            serviceDefinitionDescriptionDropDown.Changed += (s, e) =>
            {
				// Fire event with selected value and set back to actual current service definition description
				ServiceDescriptionChanged?.Invoke(this, Convert.ToString(e.Value));
                serviceDefinitionDescriptionDropDown.Selected = Service.Definition.Description;
            };

            deleteButton.Pressed += (s, e) => DeleteButtonPressed?.Invoke(this, new EventArgs());
            collapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();
        }

        private void SubscribeToService()
        {
            Service.AvailableVirtualPlatformNames.CollectionChanged += (s, e) =>
            {
                // In case the current user is not allowed to select the service type of the existing service
                var options = new List<string>(Service.AvailableVirtualPlatformNames);
                if (!Service.AvailableVirtualPlatformNames.Contains(Service.Definition.VirtualPlatformServiceName.GetDescription())) options.Add(Service.Definition.VirtualPlatformServiceName.GetDescription());

                serviceVirtualPlatformNamesDropDown.Options = options;
                serviceVirtualPlatformNamesDropDown.Selected = Service.Definition.VirtualPlatformServiceName.GetDescription();
            };

            Service.AvailableVirtualPlatformNamesValidationChanged += (s, e) =>
            {
                serviceVirtualPlatformNamesDropDown.ValidationState = e.State;
                serviceVirtualPlatformNamesDropDown.ValidationText = e.Text;
            };

            Service.AvailableServiceDescriptions.CollectionChanged += (s, e) =>
            {
                // In case the current user is not allowed to select the service type of the existing service
                var options = new HashSet<string>(Service.AvailableServiceDescriptions);
                options.Add(Service.Definition.Description);

                serviceDefinitionDescriptionDropDown.Options = options;
                serviceDefinitionDescriptionDropDown.Selected = Service.Definition.Description;
            };
        }

		private void GenerateUi()
        {
            int row = -1;

            AddWidget(collapseButton, ++row, 0);
            AddWidget(serviceNameLabel, row, configuration.LabelWidgetColumn, 1, 4);

            AddWidget(virtualPlatformTypeLabel, ++row, configuration.LabelWidgetColumn);
            AddWidget(serviceVirtualPlatformNamesDropDown, row, configuration.InputWidgetColumn, 1, 3);

            AddWidget(virtualPlatformTypeLabel2, ++row, configuration.LabelWidgetColumn);
            AddWidget(serviceDefinitionDescriptionDropDown, row, configuration.InputWidgetColumn, 1, 3);

            AddSection(ServiceSection, new SectionLayout(++row, configuration.LabelWidgetColumn));
            row += ServiceSection.RowCount;

            AddWidget(linkedRecordingsTitleLabel, ++row, configuration.LabelWidgetColumn, 1, 6);
			var linkedRecordingSections = SubServiceSections.Where(s => s.Service.Definition.VirtualPlatform == VirtualPlatform.Recording);
			foreach (var childServiceSection in linkedRecordingSections)
            {
                AddSection(childServiceSection, new SectionLayout(++row, configuration.LabelWidgetColumn));
                row += childServiceSection.RowCount;
            }

			foreach (var subServiceSection in SubServiceSections.Except(linkedRecordingSections).Where(s => s.Service.Definition.VirtualPlatform != VirtualPlatform.Routing))
			{
				AddSection(subServiceSection, new SectionLayout(++row, configuration.LabelWidgetColumn));
				row += subServiceSection.RowCount;
			}

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
            AddWidget(deleteButton, row, configuration.LabelWidgetColumn);
        }

        protected override void HandleVisibilityAndEnabledUpdate()
        {
			collapseButton.IsVisible = IsVisible && configuration.CollapseButtonIsVisible;
			collapseButton.IsEnabled = true;

			serviceNameLabel.IsVisible = IsVisible && configuration.TitleIsVisible;

			serviceVirtualPlatformNamesDropDown.IsVisible = IsVisible && !collapseButton.IsCollapsed && serviceVirtualPlatformNamesDropDown.Options.Count() > 1 && configuration.VirtualPlatformDropDownIsVisible;
			serviceVirtualPlatformNamesDropDown.IsEnabled = IsEnabled && configuration.VirtualPlatformDropDownIsEnabled;
			virtualPlatformTypeLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && serviceVirtualPlatformNamesDropDown.IsVisible;

			serviceDefinitionDescriptionDropDown.IsVisible = IsVisible && !collapseButton.IsCollapsed && serviceDefinitionDescriptionDropDown.Options.Count() > 1 && configuration.ServiceDefinitionDescriptionDropDownIsVisible;
			serviceDefinitionDescriptionDropDown.IsEnabled = IsEnabled && configuration.ServiceDefinitionDescriptionDropDownIsEnabled;
			virtualPlatformTypeLabel2.IsVisible = IsVisible && !collapseButton.IsCollapsed && serviceDefinitionDescriptionDropDown.IsVisible && configuration.ServiceDefinitionDescriptionLabelIsVisible;

			ServiceSection.IsVisible = IsVisible && !collapseButton.IsCollapsed;
			ServiceSection.IsEnabled = IsEnabled;

			deleteButton.IsVisible = IsVisible && configuration.DeleteButtonIsVisible && !collapseButton.IsCollapsed;
			deleteButton.IsEnabled = IsEnabled && configuration.DeleteButtonIsEnabled;

			linkedRecordingsTitleLabel.IsVisible = IsVisible && SubServiceSections.Any(ss => ss.Service.Definition.VirtualPlatform == VirtualPlatform.Recording) && !collapseButton.IsCollapsed;

			foreach (var subServiceSelectionSection in SubServiceSections)
			{
				subServiceSelectionSection.IsVisible = IsVisible && !collapseButton.IsCollapsed;
				subServiceSelectionSection.IsEnabled = IsEnabled;
			}

			ToolTipHandler.SetTooltipVisibility(this);	
        }

		public object Clone()
		{
			return new ServiceSelectionSection(this);
		}
	}
}
