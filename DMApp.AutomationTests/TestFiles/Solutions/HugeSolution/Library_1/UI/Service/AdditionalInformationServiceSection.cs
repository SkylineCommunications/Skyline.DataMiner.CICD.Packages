namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	/// <summary>
	/// This section is used to display the additional information of a Service.
	/// </summary>
	public class AdditionalInformationServiceSection : Section
	{
		private readonly Label title = new Label("Additional Information") { Style = TextStyle.Heading };
		private readonly Label commentsLabel = new Label("Comments");
		private readonly Label profileConfigurationFailReasonLabel = new Label("Profile Configuration Fail Reason");

		[DisplaysProperty(nameof(Service.Comments))]
		private YleTextBox commentsTextBox;

		private readonly Service service;
		private readonly AdditionalInformationSectionConfiguration configuration;
		private readonly Helpers helpers;

		/// <summary>
		/// Initializes a new instance of the <see cref="AdditionalInformationServiceSection" /> class.
		/// </summary>
		/// <param name="service">Service of which its additional information is displayed by this section.</param>
		/// <param name="configuration">Configuration object for this section.</param>
		/// <param name="helpers"></param>
		/// <exception cref="ArgumentNullException">Thrown when the provided Service is null.</exception>
		public AdditionalInformationServiceSection(Service service, AdditionalInformationSectionConfiguration configuration, Helpers helpers = null)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.helpers = helpers;

            Initialize();
            GenerateUI();
        }

        public event EventHandler<string> CommentsChanged;

		/// <summary>
		/// Gets the textBox that contains the profile configuration failure reason of the service.
		/// </summary>
		public YleTextBox ProfileConfigurationFailReasonTextBox { get; private set; }

		public void RegenerateUI()
		{
			Clear();
			GenerateUI();
		}

		/// <summary>
		/// Initializes the widgets within this section and the linking with the underlying model objects.
		/// </summary>
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
			commentsTextBox.Changed += (s, e) => CommentsChanged?.Invoke(this, Convert.ToString(e.Value));
		}

		private void InitializeWidgets()
		{
			commentsTextBox = new YleTextBox(service.Comments) { IsMultiline = true, MinHeight = 100, MaxHeight = 250 };
			ProfileConfigurationFailReasonTextBox = new YleTextBox(service.ProfileConfigurationFailReason) { IsMultiline = true, MinHeight = 100, MaxHeight = 250 };
		}

		/// <summary>
		/// Adds the widgets to this section.
		/// </summary>
		private void GenerateUI()
		{
			int row = -1;

			AddWidget(title, ++row, 0, 1, 5);

			AddWidget(commentsLabel, ++row, 0, 1, configuration.LabelSpan, verticalAlignment: VerticalAlignment.Top);
			AddWidget(commentsTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			// temporarily disabled of slack https://yle.slack.com/archives/C025D6X84E9/p1672322884425319
			//AddWidget(profileConfigurationFailReasonLabel, ++row, 0, 1, 2, verticalAlignment: VerticalAlignment.Top);
			//AddWidget(ProfileConfigurationFailReasonTextBox, row, 2, 1, 2);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}
	}
}