namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class TechnicalSpecificationSection : Section
    {
        private readonly Service service;
        private readonly TechnicalSpecificationSectionConfiguration configuration;
        private readonly Helpers helpers;

        private readonly Label technicalSpecificationTitle = new Label("Technical Specification") { Style = TextStyle.Heading, IsVisible = true };
        private readonly Label unknownSourceDescriptionLabel = new Label("Additional information") { IsVisible = true };

        [DisplaysProperty(nameof(Service.AdditionalDescriptionUnknownSource))]
        private YleTextBox additionalInformationTextBox = new YleTextBox { IsMultiline = true, Height = 150 };

        public TechnicalSpecificationSection(Service service, TechnicalSpecificationSectionConfiguration configuration, Helpers helpers = null)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.helpers = helpers;

            Initialize();
            GenerateUI();
        }

        public new bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
            }
        }

        public new bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = value;
            }
        }

        public event EventHandler<DisplayedPropertyEventArgs> DisplayedPropertyChanged;

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
            IntializeWidgets();
            SubscribeToWidgets();
        }

        private void IntializeWidgets()
        {
            additionalInformationTextBox.Text = service.AdditionalDescriptionUnknownSource;
        }

        private void SubscribeToWidgets()
        {
            additionalInformationTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(additionalInformationTextBox)), e.Value));
        }

        /// <summary>
        /// Adds the widgets to this section.
        /// </summary>
        private void GenerateUI()
        {
            int row = -1;

            AddWidget(technicalSpecificationTitle, ++row, 0, 1, 6);

            AddWidget(unknownSourceDescriptionLabel, ++row, 0, 1, configuration.LabelSpan, verticalAlignment: VerticalAlignment.Top);
            AddWidget(additionalInformationTextBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
        }
    }
}
