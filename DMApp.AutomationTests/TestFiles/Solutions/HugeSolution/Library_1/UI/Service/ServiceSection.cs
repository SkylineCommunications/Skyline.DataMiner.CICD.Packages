namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Functions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ProfileParameters;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Service = YLE.Service.Service;

	/// <summary>
	/// This section is used to visualize a Service.
	/// </summary>
	public class ServiceSection : YleSection
	{
		private readonly Guid id = Guid.NewGuid();

		private readonly ServiceSectionConfiguration configuration;
		private readonly Label reasonForBeingDisabledLabel = new Label(string.Empty) { IsVisible = false };
		private readonly Label liveUDeviceNameLabel = new Label("LiveU device");
		private readonly Label audioReturnInfoLabel = new Label("Audio Return Info");
		private readonly Label vidigoStreamSourceLinkLabel = new Label("Vidigo Stream Source Link");
		private readonly Label linkedServiceLabel = new Label("This service is the active backup for 'service name here'");
		private readonly Label serviceToRecordOrTransmitLabel = new Label();
		private readonly Label unsuccessfullResourceAssignmentLabel = new Label("WARNING: This combination of resources and parameter values will result in Resource Overbooked state");

		private readonly DropDown serviceDefinitionTypeSelectionDropDown;
		private readonly UserInfo userInfo;
		private readonly DisplayedService service;

		[DisplaysProperty(nameof(YLE.Service.Service.LiveUDeviceName))]
		private readonly YleTextBox liveUDeviceNameTextBox = new YleTextBox();

		[DisplaysProperty(nameof(YLE.Service.Service.AudioReturnInfo))]
		private readonly YleTextBox audioReturnInfoTextBox = new YleTextBox();

		[DisplaysProperty(nameof(YLE.Service.Service.VidigoStreamSourceLink))]
		private readonly YleTextBox vidigoStreamSourceLinkTextBox = new YleTextBox();

		[DisplaysProperty(nameof(YLE.Service.Service.IsSharedSource))]
		private readonly YleCheckBox promoteToSharedSourceCheckbox = new YleCheckBox("Promote to Shared Source");
		private readonly YleCheckBox adjustDetailsCheckBox = new YleCheckBox("Adjust Service Details");
		private readonly YleButton uploadJsonButton = new YleButton("Use Source from File");
		private readonly YleDropDown serviceToRecordOrTransmitDropDown = new YleDropDown { Name = nameof(serviceToRecordOrTransmitDropDown) };

		/// <summary>
		/// Constructs a ServiceSection object.
		/// </summary>
		public ServiceSection(Helpers helpers, DisplayedService service, ServiceSectionConfiguration configuration, UserInfo userInfo, DropDown serviceDefinitionTypeSelectionDropDown = null) : base(helpers)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(service));
			this.serviceDefinitionTypeSelectionDropDown = serviceDefinitionTypeSelectionDropDown;

			Initialize();
			GenerateUI();
			HandleVisibilityAndEnabledUpdate();

			Identifier = $"Section {id} (service {service.Name})";
		}

		public Guid ID => id;

		/// <summary>
		/// Gets the Service that is displayed by this section.
		/// </summary>
		public DisplayedService Service => service;

		/// <summary>
		/// Gets the subsection that displays the general service information.
		/// </summary>
		public GeneralInfoSection GeneralInfoSection { get; private set; }

		/// <summary>
		/// Gets the subsections that display the functions in the service.
		/// </summary>
		public List<FunctionSection> FunctionSections { get; private set; }

		/// <summary>
		/// Gets the subsection that displays the audio channel configuration for the service.
		/// </summary>
		public AudioChannelConfigurationSection AudioChannelConfigurationSection { get; private set; }

		/// <summary>
		/// Gets the subsection that displays the recording configuration for the service.
		/// </summary>
		public RecordingConfigurationSection RecordingConfigurationSection { get; private set; }

		/// <summary>
		/// Gets the subsection that displays the LiveU contact information. 
		/// </summary>
		public LiveUContactInfoSection LiveUContactInfoSection { get; private set; }

		/// <summary>
		/// Gets the section that displays the technical specification for an Unknown Service.
		/// </summary>
		public TechnicalSpecificationSection TechnicalSpecificationSection { get; private set; }

		/// <summary>
		/// Gets the section that displays the Eurovision configuration. (only created for dummy EBU receptions and transmissions)
		/// </summary>
		public EurovisionSection EurovisionSection { get; private set; }

		/// <summary>
		/// Gets the section that allows the user to upload Synopsis files for Satellite Reception services.
		/// </summary>
		public UploadSynopsisSection UploadSynopsisSection { get; private set; }

		public event EventHandler<DisplayedPropertyEventArgs> DisplayedPropertyChanged;

		public event EventHandler UploadJsonButtonPressed;

		public event EventHandler<Service> ServiceToRecordOrTransmitChanged;

		public override void RegenerateUi()
		{
			Clear();
			GeneralInfoSection.RegenerateUI();
			foreach (var functionSection in FunctionSections) functionSection.RegenerateUi();
			AudioChannelConfigurationSection.RegenerateUI();
			RecordingConfigurationSection.RegenerateUI();
			TechnicalSpecificationSection.RegenerateUI();
			EurovisionSection?.RegenerateUI();
			LiveUContactInfoSection.RegenerateUI();
			UploadSynopsisSection.RegenerateUI();
			GenerateUI();
			HandleVisibilityAndEnabledUpdate();
		}

		public override string ToString()
		{
			return $"{nameof(ServiceSection)} {ID} (service {Service.Name})";
		}

		/// <summary>
		/// Initializes the widgets within this section and the linking with the underlying model objects.
		/// </summary>
		private void Initialize()
		{
			using (StartPerformanceLogging())
			{
				EnableLogging();

				IntializeWidgets();
				InitializeSections();
				SubscribeToWidgets();
				SubscribeToService();
			}
		}

		private void EnableLogging()
		{
			var fields = typeof(ServiceSection).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
			var yleInteractiveWidgets = fields.Where(f => typeof(IYleInteractiveWidget).IsAssignableFrom(f.FieldType)).Select(f => f.GetValue(this)).Cast<IYleInteractiveWidget>().ToList();

			foreach (var yleWidget in yleInteractiveWidgets)
			{
				yleWidget.Helpers = helpers;
			}
		}

		private void IntializeWidgets()
		{
			if (!string.IsNullOrEmpty(configuration.ReasonForBeingDisabled))
			{
				reasonForBeingDisabledLabel.Text = configuration.ReasonForBeingDisabled;
				reasonForBeingDisabledLabel.IsVisible = true;
			}

			vidigoStreamSourceLinkTextBox.Text = Service.VidigoStreamSourceLink;
			liveUDeviceNameTextBox.Text = Service.LiveUDeviceName;
			audioReturnInfoTextBox.Text = Service.AudioReturnInfo;
			linkedServiceLabel.Text = $"This service is the active backup for {service.LinkedService?.LofDisplayName}";

			if (service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Transmission)
			{
				serviceToRecordOrTransmitLabel.Text = "Service to Transmit";
			}
			else if (service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Recording)
			{
				serviceToRecordOrTransmitLabel.Text = "Service to Record";
			}
			else
			{
				// Nothing to do
			}

			serviceToRecordOrTransmitDropDown.SetOptions(service.AvailableServicesToRecordOrTransmit.Select(x => x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception ? "Source" : x.LofDisplayName));

			UpdateServiceToRecordOrTransmit();
		}

		private void InitializeSections()
		{
			GeneralInfoSection = new GeneralInfoSection(Service, configuration.GeneralInfoSectionConfiguration, serviceDefinitionTypeSelectionDropDown);

			FunctionSections = Service.Functions.Select(f => new FunctionSection(f, configuration.FunctionSectionConfigurations.ContainsKey(f.Definition.Label) ? configuration.FunctionSectionConfigurations[f.Definition.Label] : new FunctionSectionConfiguration(helpers, f, Service.Definition, userInfo, service.Id), helpers)).ToList();

			foreach (var function in Service.Functions.OfType<DisplayedFunction>())
			{
				function.DisplayedResourcesChanged += (o, e) => HandleVisibilityAndEnabledUpdate();
				function.ResourceChanged += (o, e) => HandleVisibilityAndEnabledUpdate();
			}

			TechnicalSpecificationSection = new TechnicalSpecificationSection(Service, configuration.TechnicalSpecificationSectionConfiguration);

			AudioChannelConfigurationSection = new AudioChannelConfigurationSection(Service.AudioChannelConfiguration, configuration.AudioChannelSectionConfiguration);

			RecordingConfigurationSection = new RecordingConfigurationSection(Service.RecordingConfiguration, configuration.RecordingConfigurationSectionConfiguration) { AdjustServiceDetails = adjustDetailsCheckBox.IsChecked, ServiceDefinitionDescription = service.Definition.Description };

			LiveUContactInfoSection = new LiveUContactInfoSection(Service, configuration.LiveUContactInfoSectionConfiguration);

			UploadSynopsisSection = new UploadSynopsisSection(Service, configuration.UploadSynopsisSectionConfiguration);

			if (service.Definition.VirtualPlatformServiceName == VirtualPlatformName.Eurovision) EurovisionSection = new EurovisionSection(Service, configuration.EurovisionSectionConfiguration, helpers);
		}

		private void SubscribeToWidgets()
		{
			liveUDeviceNameTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(liveUDeviceNameTextBox)), e.Value));
			audioReturnInfoTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(audioReturnInfoTextBox)), e.Value));
			vidigoStreamSourceLinkTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(vidigoStreamSourceLinkTextBox)), e.Value));
			promoteToSharedSourceCheckbox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(promoteToSharedSourceCheckbox)), e.Value));
			adjustDetailsCheckBox.Changed += (s, e) =>
			{
				RecordingConfigurationSection.AdjustServiceDetails = (bool)e.Value;
				HandleVisibilityAndEnabledUpdate();
			};

			uploadJsonButton.Pressed += (s, e) => UploadJsonButtonPressed?.Invoke(this, EventArgs.Empty);

			serviceToRecordOrTransmitDropDown.Changed += ServiceToRecordOrTransmitDropDown_Changed;
		}

		private void ServiceToRecordOrTransmitDropDown_Changed(object sender, YleValueWidgetChangedEventArgs e)
		{
			string selected = Convert.ToString(e.Value);

			if (selected == "Source")
			{
				ServiceToRecordOrTransmitChanged?.Invoke(this, Service.AvailableServicesToRecordOrTransmit.SingleOrDefault(x => x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception) ?? throw new ServiceNotFoundException($"Unable to find source service between available services to record or transmit: {string.Join(", ", Service.AvailableServicesToRecordOrTransmit.Select(x => x.Name))}", true));
			}
			else
			{
				ServiceToRecordOrTransmitChanged?.Invoke(this, Service.AvailableServicesToRecordOrTransmit.SingleOrDefault(x => x.LofDisplayName == selected) ?? throw new ServiceNotFoundException($"Unable to find selected service {selected} between available services to record or transmit: {string.Join(", ", Service.AvailableServicesToRecordOrTransmit.Select(x => $"{x.Name} ({x.LofDisplayName})"))}", true));
			}

			HandleVisibilityAndEnabledUpdate();
		}

		private void SubscribeToService()
		{
			service.AvailableServicesToRecordOrTransmitChanged += (s, e) =>
			{
				serviceToRecordOrTransmitDropDown.SetOptions(e.Select(x => x.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception ? "Source" : x.LofDisplayName));
				UpdateServiceToRecordOrTransmit();
			};

			service.NameOfServiceToTransmitOrRecordChanged += (s, e) => UpdateServiceToRecordOrTransmit();
		}

		private void UpdateServiceToRecordOrTransmit()
		{
			var serviceToRecordOrTransmit = service.AvailableServicesToRecordOrTransmit.SingleOrDefault(x => x.Name == service.NameOfServiceToTransmitOrRecord);

			serviceToRecordOrTransmitDropDown.Selected = (serviceToRecordOrTransmit is null || serviceToRecordOrTransmit.Definition.VirtualPlatformServiceType == VirtualPlatformType.Reception) ? "Source" : serviceToRecordOrTransmit.LofDisplayName;
		}

		/// <summary>
		/// Adds the widgets to this section.
		/// </summary>
		private void GenerateUI()
		{
			int row = -1;

			AddWidget(reasonForBeingDisabledLabel, ++row, configuration.InputColumn, 1, configuration.InputSpan);

			AddWidget(linkedServiceLabel, ++row, configuration.LabelColumn, 1, 5);

			AddWidget(serviceToRecordOrTransmitLabel, ++row, configuration.LabelColumn, 1, configuration.LabelSpan);
			AddWidget(serviceToRecordOrTransmitDropDown, row, configuration.InputColumn, 1, configuration.InputSpan);

			AddWidget(promoteToSharedSourceCheckbox, ++row, configuration.InputColumn, 1, configuration.InputSpan);
			AddWidget(adjustDetailsCheckBox, ++row, configuration.InputColumn, 1, configuration.InputSpan);
			AddWidget(uploadJsonButton, ++row, configuration.InputColumn, 1, configuration.InputSpan);

			AddSection(GeneralInfoSection, new SectionLayout(++row, configuration.LabelColumn));
			row += GeneralInfoSection.RowCount;

			AddSection(UploadSynopsisSection, new SectionLayout(++row, 0));
			row += UploadSynopsisSection.RowCount;

			AddWidget(unsuccessfullResourceAssignmentLabel, new WidgetLayout(++row, 0, 1, 5));

			var orderedFunctionSections = FunctionSections.OrderBy(s => configuration.FunctionsInDisplayOrder.IndexOf(s.FunctionDefinitionLabel)).ToList();
			foreach (var functionSection in orderedFunctionSections)
			{
				AddSection(functionSection, new SectionLayout(++row, configuration.LabelColumn));
				row += functionSection.RowCount;
			}

			AddWidget(vidigoStreamSourceLinkLabel, new WidgetLayout(++row, configuration.LabelColumn, 1, configuration.LabelSpan));
			AddWidget(vidigoStreamSourceLinkTextBox, new WidgetLayout(row, configuration.InputColumn, 1, configuration.InputSpan));

			AddSection(TechnicalSpecificationSection, new SectionLayout(++row, configuration.LabelColumn));
			row += TechnicalSpecificationSection.RowCount;

			AddWidget(audioReturnInfoLabel, new WidgetLayout(++row, configuration.LabelColumn, 1, configuration.LabelSpan));
			AddWidget(audioReturnInfoTextBox, new WidgetLayout(row, configuration.InputColumn, 1, configuration.InputSpan));

			AddWidget(liveUDeviceNameLabel, new WidgetLayout(++row, configuration.LabelColumn, 1, configuration.LabelSpan));
			AddWidget(liveUDeviceNameTextBox, new WidgetLayout(row, configuration.InputColumn, 1, configuration.InputSpan));

			AddSection(AudioChannelConfigurationSection, new SectionLayout(++row, configuration.LabelColumn));
			row += AudioChannelConfigurationSection.RowCount;

			AddSection(RecordingConfigurationSection, new SectionLayout(++row, configuration.LabelColumn));
			row += RecordingConfigurationSection.RowCount;

			AddSection(LiveUContactInfoSection, new SectionLayout(++row, configuration.LabelColumn));
			row += LiveUContactInfoSection.RowCount;

			if (EurovisionSection != null)
			{
				AddSection(EurovisionSection, new SectionLayout(row + 1, configuration.LabelColumn));
			}

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		/// <summary>
		/// Updates the visibility of the widgets and underlying sections.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		protected override void HandleVisibilityAndEnabledUpdate()
		{
			reasonForBeingDisabledLabel.IsVisible = IsVisible && !string.IsNullOrEmpty(reasonForBeingDisabledLabel.Text);

			linkedServiceLabel.IsVisible = IsVisible && service.BackupType == YLE.Order.BackupType.Active && service.Definition.VirtualPlatformServiceType != VirtualPlatformType.Reception;

			serviceToRecordOrTransmitLabel.IsVisible = IsVisible && (service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Recording || service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Transmission) && configuration.ServiceToRecordOrTransmitDropDownIsVisible;

			serviceToRecordOrTransmitDropDown.IsVisible = IsVisible && (service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Recording || service.Definition.VirtualPlatformServiceType == VirtualPlatformType.Transmission) && configuration.ServiceToRecordOrTransmitDropDownIsVisible;
			serviceToRecordOrTransmitDropDown.IsEnabled = IsEnabled && configuration.ServiceToRecordOrTransmitDropDownEnabled;

			promoteToSharedSourceCheckbox.IsVisible = IsVisible && configuration.PromoteToSharedSourceIsVisible;
			promoteToSharedSourceCheckbox.IsEnabled = IsEnabled && configuration.PromoteToSharedSourceIsEnabled;

			adjustDetailsCheckBox.IsVisible = IsVisible && configuration.AdjustDetailsIsVisible;
			adjustDetailsCheckBox.IsEnabled = true;

			uploadJsonButton.IsVisible = IsVisible && configuration.UploadJsonButtonIsVisible;
			uploadJsonButton.IsEnabled = IsEnabled && configuration.UploadJsonButtonIsEnabled;

			GeneralInfoSection.IsVisible = IsVisible && configuration.GeneralInfoSectionConfiguration.IsVisible && (adjustDetailsCheckBox.IsChecked || !configuration.AdjustDetailsIsVisible);
			GeneralInfoSection.IsEnabled = IsEnabled && configuration.GeneralInfoSectionConfiguration.IsEnabled && (adjustDetailsCheckBox.IsChecked || !configuration.AdjustDetailsIsVisible);

			unsuccessfullResourceAssignmentLabel.IsVisible = IsVisible;
			unsuccessfullResourceAssignmentLabel.Text = IsVisible && configuration.UnsuccessfulResourceAssignmentWarningIsVisible && !Service.VerifyFunctionResources() ? "WARNING: This combination of resources and parameter values will result in Resource Overbooked state" : String.Empty;

			foreach (var functionSection in FunctionSections)
			{
				functionSection.IsVisible = IsVisible && configuration.FunctionSectionConfigurations[functionSection.FunctionDefinitionLabel].IsVisible;
				functionSection.IsEnabled = IsEnabled && configuration.FunctionSectionConfigurations[functionSection.FunctionDefinitionLabel].IsEnabled;
			}

			vidigoStreamSourceLinkLabel.IsVisible = IsVisible && configuration.VidigoStreamSourceLinkIsVisible && Service.Definition.VirtualPlatform == VirtualPlatform.ReceptionIp && Service.Definition.Description == "Vidigo";
			vidigoStreamSourceLinkTextBox.IsVisible = vidigoStreamSourceLinkLabel.IsVisible;
			vidigoStreamSourceLinkTextBox.IsEnabled = IsEnabled && configuration.VidigoStreamSourceLinkEnabled;

			AudioChannelConfigurationSection.IsVisible = IsVisible && configuration.AudioChannelSectionConfiguration.IsVisible && serviceToRecordOrTransmitDropDown.Selected.Equals("Source");
			AudioChannelConfigurationSection.IsEnabled = IsEnabled && configuration.AudioChannelSectionConfiguration.IsEnabled;

			RecordingConfigurationSection.IsVisible = IsVisible && configuration.RecordingConfigurationSectionConfiguration.IsVisible && Service.Definition.VirtualPlatform == VirtualPlatform.Recording;
			RecordingConfigurationSection.IsEnabled = IsEnabled && configuration.RecordingFileDetailsIsEnabled;

			TechnicalSpecificationSection.IsVisible = IsVisible && configuration.TechnicalSpecificationSectionIsVisible;
			TechnicalSpecificationSection.IsEnabled = IsEnabled && configuration.TechnicalSpecificationSectionEnabled;

			LiveUContactInfoSection.IsVisible = IsVisible && configuration.LiveUContactInfoIsVisible && Service.Definition.VirtualPlatform == VirtualPlatform.ReceptionLiveU;
			LiveUContactInfoSection.IsEnabled = IsEnabled && configuration.LiveUContactInfoEnabled;

			liveUDeviceNameLabel.IsVisible = IsVisible && configuration.LiveUDeviceNameIsVisible && Service.Definition.VirtualPlatformServiceName == VirtualPlatformName.LiveU;
			liveUDeviceNameTextBox.IsVisible = liveUDeviceNameLabel.IsVisible;
			liveUDeviceNameTextBox.IsEnabled = IsEnabled && configuration.LiveUDeviceNameEnabled;

			audioReturnInfoLabel.IsVisible = IsVisible && configuration.AudioReturnInfoIsVisible && Service.Definition.VirtualPlatformServiceName == VirtualPlatformName.LiveU && userInfo.IsNewsUser;
			audioReturnInfoTextBox.IsVisible = audioReturnInfoLabel.IsVisible;
			audioReturnInfoTextBox.IsEnabled = IsEnabled && configuration.AudioReturnInfoEnabled;

			if (EurovisionSection != null)
			{
				EurovisionSection.IsVisible = IsVisible && configuration.EurovisionIsVisible && Service.Definition.VirtualPlatformServiceName == VirtualPlatformName.Eurovision;
				EurovisionSection.IsEnabled = IsEnabled && configuration.EurovisionIsEnabled;
			}

			UploadSynopsisSection.IsVisible = IsVisible && configuration.UploadSynopsisSectionConfiguration.IsVisible;
			UploadSynopsisSection.IsEnabled = IsEnabled && configuration.UploadSynopsisSectionConfiguration.IsEnabled;

			//SetMatrixFunctionSectionsVisibility();
			ToolTipHandler.SetTooltipVisibility(this);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ServiceSection other)) return false;
			return other.id == this.ID;
		}

		public override int GetHashCode()
		{
			return Service.GetHashCode();
		}

		/// <summary>
		/// Hide matrix function sections in case both the input and output resource are null.
		/// </summary>
		private void SetMatrixFunctionSectionsVisibility()
		{
			bool allFunctionsInServiceAreMatrices = Service.Functions.All(f => f != null && FunctionGuids.IsMatrixFunction(f.Id));
			if (allFunctionsInServiceAreMatrices) return;

			var sectionsToSkip = new List<string>();
			foreach (var functionSection in FunctionSections)
			{
				var function = Service.Functions.Single(f => f.Definition.Label == functionSection.FunctionDefinitionLabel);

				if (sectionsToSkip.Contains(functionSection.FunctionDefinitionLabel)) continue;

				if (function.Resource != null)
				{
					functionSection.IsVisible = IsVisible && configuration.FunctionSectionConfigurations[functionSection.FunctionDefinitionLabel].IsVisible;
					continue;
				}

				bool functionIsMatrix = FunctionGuids.AllMatrixGuids.Contains(functionSection.FunctionId);
				if (functionIsMatrix)
				{
					SetMatrixFunctionSectionVisibility(sectionsToSkip, functionSection, function);
				}
			}
		}

		private void SetMatrixFunctionSectionVisibility(List<string> sectionsToSkip, FunctionSection functionSection, Function function)
		{
			var sectionAbove = FunctionSections.FirstOrDefault(s => configuration.FunctionsInDisplayOrder.IndexOf(s.FunctionDefinitionLabel) == configuration.FunctionsInDisplayOrder.IndexOf(function.Definition.Label) - 1);
			var sectionAboveFunctionResource = Service.Functions.SingleOrDefault(f => f.Definition.Label == sectionAbove?.FunctionDefinitionLabel)?.Resource;

			var sectionBelow = FunctionSections.FirstOrDefault(s => configuration.FunctionsInDisplayOrder.IndexOf(s.FunctionDefinitionLabel) == configuration.FunctionsInDisplayOrder.IndexOf(function.Definition.Label) + 1);
			var sectionBelowFunctionResource = Service.Functions.SingleOrDefault(f => f.Definition.Label == sectionBelow?.FunctionDefinitionLabel)?.Resource;

			if (sectionAbove != null && FunctionGuids.AllMatrixGuids.Contains(sectionAbove.FunctionId) && sectionAboveFunctionResource == null)
			{
				functionSection.IsVisible = false;
				sectionAbove.IsVisible = false;
				sectionsToSkip.Add(functionSection.FunctionDefinitionLabel);
				sectionsToSkip.Add(sectionAbove.FunctionDefinitionLabel);
			}
			else if (sectionBelow != null && FunctionGuids.AllMatrixGuids.Contains(sectionBelow.FunctionId) && sectionBelowFunctionResource == null)
			{
				functionSection.IsVisible = false;
				sectionBelow.IsVisible = false;
				sectionsToSkip.Add(functionSection.FunctionDefinitionLabel);
				sectionsToSkip.Add(sectionBelow.FunctionDefinitionLabel);
			}
			else
			{
				functionSection.IsVisible = IsVisible && configuration.FunctionSectionConfigurations[functionSection.FunctionDefinitionLabel].IsVisible;
			}
		}
	}
}