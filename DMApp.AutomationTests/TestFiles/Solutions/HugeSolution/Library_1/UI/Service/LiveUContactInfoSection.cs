namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class LiveUContactInfoSection : Section
	{
		private readonly Label contactInformationTitle = new Label("Contact Information") { Style = TextStyle.Heading, IsVisible = true };
		private readonly Label contactInformationNameLabel = new Label("Name");
		private readonly Label contactInformationTelephoneNumberLabel = new Label("Telephone Number");

		private readonly Service service;
		private readonly LiveUContactInfoSectionConfiguration configuration;
		private readonly Helpers helpers;


		[DisplaysProperty(nameof(Service.ContactInformationName))]
		private YleTextBox contactInformationNameTextBox;

		[DisplaysProperty(nameof(Service.ContactInformationTelephoneNumber))]
		private YleTextBox contactInformationTelephoneNumberTextBox;

		public LiveUContactInfoSection(Service service, LiveUContactInfoSectionConfiguration configuration, Helpers helpers = null)
		{
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.helpers = helpers;

			Initialize();
			GenerateUI();
		}

		public event EventHandler<DisplayedPropertyEventArgs> DisplayedPropertyChanged;

		public void RegenerateUI()
		{
			Clear();
			GenerateUI();
		}

		private void Initialize()
		{
			InitializeWidgets();
			SubscribeToWidgets();
			SubscribeToService();
		}

		private void SubscribeToService()
		{
			this.SubscribeToDisplayedObjectValidation(service);
		}

		private void SubscribeToWidgets()
		{
			contactInformationNameTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(contactInformationNameTextBox)), e.Value));
			contactInformationTelephoneNumberTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(contactInformationTelephoneNumberTextBox)), e.Value));
		}

		private void InitializeWidgets()
		{
			contactInformationNameTextBox = new YleTextBox(service.ContactInformationName);
			contactInformationTelephoneNumberTextBox = new YleTextBox(service.ContactInformationTelephoneNumber);
		}

		private void GenerateUI()
		{
			int row = 0;

			AddWidget(contactInformationTitle, new WidgetLayout(++row, 0, 1, 5));

			AddWidget(contactInformationNameLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(contactInformationNameTextBox, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

			AddWidget(contactInformationTelephoneNumberLabel, new WidgetLayout(++row, 0, 1, configuration.LabelSpan));
			AddWidget(contactInformationTelephoneNumberTextBox, new WidgetLayout(row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan));

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}
	}
}