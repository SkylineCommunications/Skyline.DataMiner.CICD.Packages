namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
    using System;
    using System.Linq;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    // Only applicable to News Users
    public class NewsInformationSection : Section
    {
        private readonly NewsInformation newsInformation;
        private readonly NewsInformationSectionConfiguration configuration;
        private readonly Helpers helpers;

        private readonly Label titleLabel = new Label("News Information") { Style = TextStyle.Bold };
        private readonly Label newsCameraOperatorLabel = new Label("News Camera Operator");
        private readonly Label journalistLabel = new Label("Journalist");
        private readonly Label virveCommandGroupOneLabel = new Label("Virve Command Group 1");
        private readonly Label virveCommandGroupTwoLabel = new Label("Virve Command Group 2");
        private readonly Label additionalInformationLabel = new Label("Additional Information");

        private readonly CollapseButton collapseButton = new CollapseButton(true) { CollapseText = "-", ExpandText = "+", Width = 44 };

        [DisplaysProperty(nameof(NewsInformation.NewsCameraOperator))]
        private readonly YleTextBox newsCameraOperatorTextBox = new YleTextBox();

        [DisplaysProperty(nameof(NewsInformation.Journalist))]
        private readonly YleTextBox journalistTextBox = new YleTextBox();

        [DisplaysProperty(nameof(NewsInformation.VirveCommandGroupOne))]
        private readonly YleTextBox virveCommandGroupOneTextBox = new YleTextBox();

        [DisplaysProperty(nameof(NewsInformation.VirveCommandGroupTwo))]
        private readonly YleTextBox virveCommandGroupTwoTextBox = new YleTextBox();

        [DisplaysProperty(nameof(NewsInformation.AdditionalInformation))]
        private readonly YleTextBox additionalInformationTextBox = new YleTextBox { IsMultiline = true, MaxHeight = 250, MinHeight = 100 };

        public NewsInformationSection(NewsInformation newsInformation, NewsInformationSectionConfiguration configuration, Helpers helpers = null)
        {
            this.newsInformation = newsInformation ?? throw new ArgumentNullException(nameof(newsInformation));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.helpers = helpers;

            Initialize();
            GenerateUi();
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

        public void RegenerateUi()
        {
            Clear();
            GenerateUi();
            HandleVisibilityAndEnabledUpdate();
        }

        public event EventHandler<DisplayedPropertyEventArgs> DisplayedPropertyChanged;

        private void Initialize()
        {
            InitializeWidgets();
            SubscribeToWidgets();
            SubscribeToNewsInformation();
        }

        private void InitializeWidgets()
        {
            collapseButton.IsCollapsed = configuration.IsCollapsed;
            newsCameraOperatorTextBox.Text = newsInformation.NewsCameraOperator;
            journalistTextBox.Text = newsInformation.Journalist;
            virveCommandGroupOneTextBox.Text = newsInformation.VirveCommandGroupOne;
            virveCommandGroupTwoTextBox.Text = newsInformation.VirveCommandGroupTwo;
            additionalInformationTextBox.Text = newsInformation.AdditionalInformation;
        }

        private void SubscribeToWidgets()
        {
            collapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();

            newsCameraOperatorTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(newsCameraOperatorTextBox)), e.Value));

            journalistTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(journalistTextBox)), e.Value));

            virveCommandGroupOneTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(virveCommandGroupOneTextBox)), e.Value));

            virveCommandGroupTwoTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(virveCommandGroupTwoTextBox)), e.Value));

            additionalInformationTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(additionalInformationTextBox)), e.Value));

        }

        private void SubscribeToNewsInformation()
        {
            newsInformation.NewsCameraOperatorChanged += (s, e) => newsCameraOperatorTextBox.Text = e;
            newsInformation.JournalistChanged += (s, e) => journalistTextBox.Text = e;
            newsInformation.VirveCommandGroupOneChanged += (s, e) => virveCommandGroupOneTextBox.Text = e;
            newsInformation.VirveCommandGroupTwoChanged += (s, e) => virveCommandGroupTwoTextBox.Text = e;
            newsInformation.AdditionalInformationChanged += (s, e) => additionalInformationTextBox.Text = e;

            this.SubscribeToDisplayedObjectValidation(newsInformation);
        }

        private void GenerateUi()
        {
            int row = -1;

            AddWidget(collapseButton, ++row, 0);
            AddWidget(titleLabel, row, 1, 1, 8);

            AddWidget(newsCameraOperatorLabel, ++row, 1, 1, 3);
            AddWidget(newsCameraOperatorTextBox, row, 4, 1, 3);

            AddWidget(journalistLabel, ++row, 1, 1, 3);
            AddWidget(journalistTextBox, row, 4, 1, 3);

            AddWidget(virveCommandGroupOneLabel, ++row, 1, 1, 3);
            AddWidget(virveCommandGroupOneTextBox, row, 4, 1, 3);

            AddWidget(virveCommandGroupTwoLabel, ++row, 1, 1, 3);
            AddWidget(virveCommandGroupTwoTextBox, row, 4, 1, 3);

            AddWidget(additionalInformationLabel, ++row, 1, 1, 3, verticalAlignment: VerticalAlignment.Top);
            AddWidget(additionalInformationTextBox, row, 4, 1, 3);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		private void HandleVisibilityAndEnabledUpdate()
        {
            collapseButton.IsVisible = IsVisible;
            collapseButton.IsEnabled = IsEnabled && configuration.CollapseButtonsAreEnabled;

            titleLabel.IsVisible = IsVisible;

            newsCameraOperatorLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.NewsCameraOperatorIsVisible;
            newsCameraOperatorTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.NewsCameraOperatorIsVisible;
            newsCameraOperatorTextBox.IsEnabled = IsEnabled && configuration.NewsCameraOperatorIsEnabled;

            journalistLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.JournalistIsVisible;
            journalistTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.JournalistIsVisible;
            journalistTextBox.IsEnabled = IsEnabled && configuration.JournalistIsEnabled;

            virveCommandGroupOneLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.VirveCommandGroupOneIsVisible;
            virveCommandGroupOneTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.VirveCommandGroupOneIsVisible;
            virveCommandGroupOneTextBox.IsEnabled = IsEnabled && configuration.VivreCommandGroupOneIsEnabled;

            virveCommandGroupTwoLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.VivreCommandGroupTwoIsVisible;
            virveCommandGroupTwoTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.VivreCommandGroupTwoIsVisible;
            virveCommandGroupTwoTextBox.IsEnabled = IsEnabled && configuration.VivreCommandGroupTwoIsEnabled;

            additionalInformationLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.AdditionalInformationIsVisible;
            additionalInformationTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.AdditionalInformationIsVisible;
            additionalInformationTextBox.IsEnabled = IsEnabled && configuration.AdditionalInformationIsEnabled;

            ToolTipHandler.SetTooltipVisibility(this);
        }
    }
}