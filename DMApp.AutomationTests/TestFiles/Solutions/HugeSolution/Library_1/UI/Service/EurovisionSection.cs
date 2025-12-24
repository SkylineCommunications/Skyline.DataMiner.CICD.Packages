namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Type = YLE.Integrations.Eurovision.Type;

	public class EurovisionSection : Section
	{
		private readonly EurovisionSectionConfiguration configuration;

		private readonly DisplayedService service;
		private readonly Helpers helpers;

		private readonly Label eurovisionDetailsLabel = new Label("Eurovision Details") { Style = TextStyle.Heading };

		private readonly CheckBox linkEurovisionIdCheckbox = new CheckBox("Link Synopsis ID");

		private readonly Label synopsisIdLabel = new Label("Synopsis ID");
		private readonly YleTextBox synopsisIdTextBox = new YleTextBox();

		private readonly Label workOrderIdLabel = new Label("Work Order ID");
		private readonly YleTextBox workOrderIdTextBox = new YleTextBox();

		private readonly Label typeLabel = new Label("Type");
		private readonly DropDown typeDropDown = new DropDown();

		private readonly Button bookButton = new Button() { Style = ButtonStyle.CallToAction };

		private readonly Label infoLabel = new Label(String.Empty);

		private readonly EurovisionBookingDetails eurovisionBookingDetails;

		public EurovisionSection(DisplayedService service, EurovisionSectionConfiguration configuration, Helpers helpers = null)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.eurovisionBookingDetails = service.EurovisionBookingDetails ?? throw new ArgumentNullException("service.EurovisionBookingDetails");
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

		public string SynopsisId => synopsisIdTextBox.Text;

		public NewsEventSection NewsEventSection { get; private set; }

		public ProgramEventSection ProgramEventSection { get; private set; }

		public SatelliteCapacitySection SatelliteCapacitySection { get; private set; }

		public UniTransmissionSection UnilateralTransmissionSection { get; private set; }

		public OssTransmissionSection OssTransmissionSection { get; private set; }

		public void RegenerateUI()
		{
			Clear();
			NewsEventSection.RegenerateUI();
			ProgramEventSection.RegenerateUI();
			SatelliteCapacitySection.RegenerateUI();
			UnilateralTransmissionSection.RegenerateUI();
			OssTransmissionSection.RegenerateUI();
			GenerateUI();
			HandleVisibilityAndEnabledUpdate();
		}

		public event EventHandler<string> SynopsisIdChanged;

		public event EventHandler<Type> TypeChanged;

		public event EventHandler BookButtonClicked;

		private void Initialize()
		{
			IntializeWidgets();
			InitializeSections();
			SubscribeToWidgets();
			SubscribeToService();
		}

		private void IntializeWidgets()
		{
			switch (service.Definition.VirtualPlatform)
			{
				case ServiceDefinition.VirtualPlatform.ReceptionEurovision:
					typeDropDown.SetOptions((new[] { Type.None, Type.NewsEvent, Type.ProgramEvent, Type.SatelliteCapacity, Type.UnilateralTransmission, Type.OSSTransmission }).Select(x => x.GetDescription()));
					typeDropDown.Selected = service.EurovisionBookingDetails.Type.GetDescription();
					bookButton.Text = "Book Eurovision Reception";
					break;
				case ServiceDefinition.VirtualPlatform.TransmissionEurovision:
					typeDropDown.SetOptions((new[] { Type.None, Type.UnilateralTransmission, Type.OSSTransmission, Type.SatelliteCapacity }).Select(x => x.GetDescription()));
					typeDropDown.Selected = service.EurovisionBookingDetails.Type.GetDescription();
					bookButton.Text = "Book Eurovision Transmission";
					break;
				default:
					// Nothing to do for non-Eurovision sections
					break;
			}

			linkEurovisionIdCheckbox.IsChecked = service.EurovisionBookingDetails.Type == Type.None;
			synopsisIdTextBox.Text = service.EurovisionTransmissionNumber;
			workOrderIdTextBox.Text = service.EurovisionWorkOrderId;
		}

		private void InitializeSections()
		{
			NewsEventSection = new NewsEventSection(eurovisionBookingDetails, configuration.NewsEventSectionConfiguration, helpers);
			ProgramEventSection = new ProgramEventSection(eurovisionBookingDetails, configuration.ProgramEventSectionConfiguration);
			SatelliteCapacitySection = new SatelliteCapacitySection(eurovisionBookingDetails, service.Definition.VirtualPlatformServiceType, configuration.SatelliteCapacitySectionConfiguration);
			UnilateralTransmissionSection = new UniTransmissionSection(eurovisionBookingDetails, service.Definition.VirtualPlatformServiceType, configuration.UnilateralTransmissionSectionConfiguration);
			OssTransmissionSection = new OssTransmissionSection(eurovisionBookingDetails, service.Definition.VirtualPlatformServiceType, configuration.OsslateralTransmissionSectionConfiguration);
		}

		private void SubscribeToWidgets()
		{
			linkEurovisionIdCheckbox.Changed += (s, e) => HandleVisibilityAndEnabledUpdate();
			synopsisIdTextBox.Changed += (s, e) => SynopsisIdChanged?.Invoke(this, SynopsisId);
			typeDropDown.Changed += (s, e) => TypeChanged?.Invoke(this, EnumExtensions.GetEnumValueFromDescription<Type>(typeDropDown.Selected));
			bookButton.Pressed += (s, e) => BookButtonClicked?.Invoke(this, new EventArgs());
		}

		private void SubscribeToService()
		{
			service.EurovisionBookingDetails.TypeChanged += (s, e) => HandleVisibilityAndEnabledUpdate();
			service.EurovisionWorkOrderIdChanged += (s, e) => workOrderIdTextBox.Text = e;
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(eurovisionDetailsLabel, new WidgetLayout(++row, 0, 1, 5, horizontalAlignment: HorizontalAlignment.Left));

			AddWidget(linkEurovisionIdCheckbox, ++row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(workOrderIdLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(workOrderIdTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(synopsisIdLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(synopsisIdTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(typeLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(typeDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddSection(NewsEventSection, new SectionLayout(++row, 0));
			row += NewsEventSection.RowCount;

			AddSection(ProgramEventSection, new SectionLayout(++row, 0));
			row += ProgramEventSection.RowCount;

			AddSection(SatelliteCapacitySection, new SectionLayout(++row, 0));
			row += SatelliteCapacitySection.RowCount;

			AddSection(UnilateralTransmissionSection, new SectionLayout(++row, 0));
			row += UnilateralTransmissionSection.RowCount;

			AddSection(OssTransmissionSection, new SectionLayout(++row, 0));
			row += OssTransmissionSection.RowCount;

			AddWidget(bookButton, ++row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(infoLabel, ++row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		private void HandleVisibilityAndEnabledUpdate()
		{
			eurovisionDetailsLabel.IsVisible = IsVisible;

			synopsisIdLabel.IsVisible = IsVisible && linkEurovisionIdCheckbox.IsChecked;
			synopsisIdTextBox.IsVisible = IsVisible && linkEurovisionIdCheckbox.IsChecked;

			workOrderIdLabel.IsVisible = IsVisible && !linkEurovisionIdCheckbox.IsChecked;

			workOrderIdTextBox.IsVisible = IsVisible && !linkEurovisionIdCheckbox.IsChecked;
			workOrderIdTextBox.IsEnabled = false;

			typeLabel.IsVisible = IsVisible && !linkEurovisionIdCheckbox.IsChecked;

			typeDropDown.IsVisible = IsVisible && !linkEurovisionIdCheckbox.IsChecked;

			bookButton.IsVisible = IsVisible && !linkEurovisionIdCheckbox.IsChecked && service.EurovisionBookingDetails.Type != Type.None;

			NewsEventSection.IsVisible = IsVisible && service.EurovisionBookingDetails.Type == Type.NewsEvent && !linkEurovisionIdCheckbox.IsChecked;
			ProgramEventSection.IsVisible = IsVisible && service.EurovisionBookingDetails.Type == Type.ProgramEvent && !linkEurovisionIdCheckbox.IsChecked;
			SatelliteCapacitySection.IsVisible = IsVisible && service.EurovisionBookingDetails.Type == Type.SatelliteCapacity && !linkEurovisionIdCheckbox.IsChecked;
			UnilateralTransmissionSection.IsVisible = IsVisible && service.EurovisionBookingDetails.Type == Type.UnilateralTransmission && !linkEurovisionIdCheckbox.IsChecked;
			OssTransmissionSection.IsVisible = IsVisible && service.EurovisionBookingDetails.Type == Type.OSSTransmission && !linkEurovisionIdCheckbox.IsChecked;

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}
