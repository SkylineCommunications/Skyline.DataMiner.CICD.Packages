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

    public class SportsPlanningSection : Section
    {
        private readonly SportsPlanning sportsPlanning;
        private readonly SportsPlanningSectionConfiguration configuration;
        private readonly Helpers helpers;

        private const string NONE = "None";

        private readonly CollapseButton collapseButton = new CollapseButton(true) { CollapseText = "-", ExpandText = "+", Width = 44 };
        private readonly Label titleLabel = new Label("Sports Planning") { Style = TextStyle.Bold };

        private readonly Label sportLabel = new Label("Sport");
        private readonly Label descriptionLabel = new Label("Description");
        private readonly Label commentaryLabel = new Label("Commentary");
        private readonly Label commentary2Label = new Label("Commentary2");
        private readonly Label competitionTimeLabel = new Label("Competition Time");
        private readonly Label journalistOneLabel = new Label("Journalist 1");
        private readonly Label journalistTwoLabel = new Label("Journalist 2");
        private readonly Label journalistThreeLabel = new Label("Journalist 3");
        private readonly Label locationLabel = new Label("Location");
        private readonly Label technicalResourcesLabel = new Label("Technical Resources");
        private readonly Label liveHighlightsFileLabel = new Label("Live / Highlights / File");
        private readonly Label requestedBroadcastTimeLabel = new Label("Requested Broadcast Time");
        private readonly Label productionNumberPlasmaIdLabel = new Label("Production Number / Plasma ID");
        private readonly Label productNumberCeitonLabel = new Label("Product Number / Ceiton");
        private readonly Label costDepartmentLabel = new Label("Cost Department");
        private readonly Label additionalInformationLabel = new Label("Additional Information");

        [DisplaysProperty(nameof(SportsPlanning.Sport))]
        private readonly YleTextBox sportTextBox = new YleTextBox();

        [DisplaysProperty(nameof(SportsPlanning.Description))]
        private readonly YleTextBox descriptionTextBox = new YleTextBox();

        [DisplaysProperty(nameof(SportsPlanning.Commentary))]
        private readonly DropDown commentaryDropDown = new DropDown(new[] { NONE }.Concat(new[] { "OT", "PP" }.OrderBy(x => x)), NONE);

        [DisplaysProperty(nameof(SportsPlanning.Commentary2))]
        private readonly DropDown commentary2DropDown = new DropDown(new[] { NONE }.Concat(new[] { "NOT", "NPP" }.OrderBy(x => x)), NONE);

        [DisplaysProperty(nameof(SportsPlanning.CompetitionTime))]
        private readonly DateTimePicker competitionTimeDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(1));

        [DisplaysProperty(nameof(SportsPlanning.JournalistOne))]
        private readonly YleTextBox journalistOneTextBox = new YleTextBox();

        [DisplaysProperty(nameof(SportsPlanning.JournalistTwo))]
        private readonly YleTextBox journalistTwoTextBox = new YleTextBox();

        [DisplaysProperty(nameof(SportsPlanning.JournalistThree))]
        private readonly YleTextBox journalistThreeTextBox = new YleTextBox();

        [DisplaysProperty(nameof(SportsPlanning.Location))]
        private readonly YleTextBox locationTextBox = new YleTextBox();

        [DisplaysProperty(nameof(SportsPlanning.TechnicalResources))]
        private readonly YleTextBox technicalResourcesTextBox = new YleTextBox();

        [DisplaysProperty(nameof(SportsPlanning.LiveHighlightsFile))]
        private readonly DropDown liveHighlightsFileDropDown = new DropDown(new[] { NONE }.Concat(new[] { "Live", "File", "Highlights" }.OrderBy(x => x)), NONE);

        [DisplaysProperty(nameof(SportsPlanning.RequestedBroadcastTime))]
        private readonly DateTimePicker requestedBroadcastTimeDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(1));

        [DisplaysProperty(nameof(SportsPlanning.ProductionNumberPlasmaId))]
        private readonly YleTextBox productionNumberPlasmaIdTextBox = new YleTextBox();

        [DisplaysProperty(nameof(SportsPlanning.ProductNumberCeiton))]
        private readonly YleTextBox productNumberCeitonTextBox = new YleTextBox();

        [DisplaysProperty(nameof(SportsPlanning.CostDepartment))]
        private readonly YleTextBox costDepartmentTextBox = new YleTextBox();

        [DisplaysProperty(nameof(SportsPlanning.AdditionalInformation))]
        private readonly YleTextBox additionalInformationTextBox = new YleTextBox { IsMultiline = true, MaxHeight = 250, MinHeight = 100 };

        public SportsPlanningSection(SportsPlanning sportsPlanning, SportsPlanningSectionConfiguration configuration, Helpers helpers = null)
        {
            this.sportsPlanning = sportsPlanning ?? throw new ArgumentNullException(nameof(sportsPlanning));
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

        public event EventHandler<DateTime> CompetitionTimeChanged;

        public event EventHandler<DateTime> RequestedBroadcastTimeChanged;

        private void Initialize()
        {
            InitializeWidgets();
            SubscribeToWidgets();
            SubscribeToSportsPlanning();
        }

        private void InitializeWidgets()
        {
            collapseButton.IsCollapsed = configuration.IsCollapsed;
            sportTextBox.Text = sportsPlanning.Sport;
            descriptionTextBox.Text = sportsPlanning.Description;
            commentaryDropDown.Selected = commentaryDropDown.Options.Contains(sportsPlanning.Commentary) ? sportsPlanning.Commentary : commentaryDropDown.Options.First();
            commentary2DropDown.Selected = commentary2DropDown.Options.Contains(sportsPlanning.Commentary2) ? sportsPlanning.Commentary2 : commentary2DropDown.Options.First();
            competitionTimeDateTimePicker.DateTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local) + TimeSpan.FromMilliseconds(sportsPlanning.CompetitionTime));
            journalistOneTextBox.Text = sportsPlanning.JournalistOne;
            journalistTwoTextBox.Text = sportsPlanning.JournalistTwo;
            journalistThreeTextBox.Text = sportsPlanning.JournalistThree;
            locationTextBox.Text = sportsPlanning.Location;
            technicalResourcesTextBox.Text = sportsPlanning.TechnicalResources;
            liveHighlightsFileDropDown.Selected = liveHighlightsFileDropDown.Options.Contains(sportsPlanning.LiveHighlightsFile) ? sportsPlanning.LiveHighlightsFile : liveHighlightsFileDropDown.Options.First();
            requestedBroadcastTimeDateTimePicker.DateTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local) + TimeSpan.FromMilliseconds(sportsPlanning.RequestedBroadcastTime));
            productionNumberPlasmaIdTextBox.Text = sportsPlanning.ProductionNumberPlasmaId;
            productNumberCeitonTextBox.Text = sportsPlanning.ProductNumberCeiton;
            costDepartmentTextBox.Text = sportsPlanning.CostDepartment;
            additionalInformationTextBox.Text = sportsPlanning.AdditionalInformation;
        }

        private void SubscribeToWidgets()
        {
            collapseButton.Pressed += (s, e) => HandleVisibilityAndEnabledUpdate();

            sportTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(sportTextBox)), e.Value));

            descriptionTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(descriptionTextBox)), e.Value));

            commentaryDropDown.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(commentaryDropDown)), e.Selected));

            commentary2DropDown.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(commentary2DropDown)), e.Selected));

            competitionTimeDateTimePicker.Changed += (s, e) => CompetitionTimeChanged?.Invoke(this, e.DateTime);

            journalistOneTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(journalistOneTextBox)), e.Value));

            journalistTwoTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(journalistTwoTextBox)), e.Value));

            journalistThreeTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(journalistThreeTextBox)), e.Value));

            locationTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(locationTextBox)), e.Value));

            technicalResourcesTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(technicalResourcesTextBox)), e.Value));

            liveHighlightsFileDropDown.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(liveHighlightsFileDropDown)), e.Selected));

            requestedBroadcastTimeDateTimePicker.Changed += (s, e) => RequestedBroadcastTimeChanged?.Invoke(this, e.DateTime);

            productionNumberPlasmaIdTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(productionNumberPlasmaIdTextBox)), e.Value));

            productNumberCeitonTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(productNumberCeitonTextBox)), e.Value));

            costDepartmentTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(costDepartmentTextBox)), e.Value));

            additionalInformationTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(additionalInformationTextBox)), e.Value));
        }

        private void SubscribeToSportsPlanning()
        {
            sportsPlanning.SportChanged += (s, e) => sportTextBox.Text = e;
            sportsPlanning.DescriptionChanged += (s, e) => descriptionTextBox.Text = e;
            sportsPlanning.CommentaryChanged += (s, e) => commentaryDropDown.Selected = e;
            sportsPlanning.Commentary2Changed += (s, e) => commentary2DropDown.Selected = e;
            sportsPlanning.CompetitionTimeChanged += (s, e) => competitionTimeDateTimePicker.DateTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local) + TimeSpan.FromMilliseconds(e));
            sportsPlanning.JournalistOneChanged += (s, e) => journalistOneTextBox.Text = e;
            sportsPlanning.JournalistTwoChanged += (s, e) => journalistTwoTextBox.Text = e;
            sportsPlanning.JournalistThreeChanged += (s, e) => journalistThreeTextBox.Text = e;
            sportsPlanning.LocationChanged += (s, e) => locationTextBox.Text = e;
            sportsPlanning.TechnicalResourcesChanged += (s, e) => technicalResourcesTextBox.Text = e;
            sportsPlanning.LiveHighlightsFileChanged += (s, e) => liveHighlightsFileDropDown.Selected = e;
            sportsPlanning.RequestedBroadcastTimeChanged += (s, e) => requestedBroadcastTimeDateTimePicker.DateTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local) + TimeSpan.FromMilliseconds(e));
            sportsPlanning.ProductionNumberPlasmaIdChanged += (s, e) => productionNumberPlasmaIdTextBox.Text = e;
            sportsPlanning.ProductNumberCeitonChanged += (s, e) => productNumberCeitonTextBox.Text = e;
            sportsPlanning.CostDepartmentChanged += (s, e) => costDepartmentTextBox.Text = e;
            sportsPlanning.AdditionalInformationChanged += (s, e) => additionalInformationTextBox.Text = e;

            this.SubscribeToDisplayedObjectValidation(sportsPlanning);
        }

        private void GenerateUi()
        {
            int row = -1;

            AddWidget(collapseButton, ++row, 0);
            AddWidget(titleLabel, row, 1, 1, 8);

            AddWidget(sportLabel, ++row, 1, 1, 3);
            AddWidget(sportTextBox, row, 4, 1, 3);

            AddWidget(descriptionLabel, ++row, 1, 1, 3);
            AddWidget(descriptionTextBox, row, 4, 1, 3);

            AddWidget(commentaryLabel, ++row, 1, 1, 3);
            AddWidget(commentaryDropDown, row, 4, 1, 3);

            AddWidget(commentary2Label, ++row, 1, 1, 3);
            AddWidget(commentary2DropDown, row, 4, 1, 3);

            AddWidget(competitionTimeLabel, ++row, 1, 1, 3);
            AddWidget(competitionTimeDateTimePicker, row, 4, 1, 3);

            AddWidget(journalistOneLabel, ++row, 1, 1, 3);
            AddWidget(journalistOneTextBox, row, 4, 1, 3);

            AddWidget(journalistTwoLabel, ++row, 1, 1, 3);
            AddWidget(journalistTwoTextBox, row, 4, 1, 3);

            AddWidget(journalistThreeLabel, ++row, 1, 1, 3);
            AddWidget(journalistThreeTextBox, row, 4, 1, 3);

            AddWidget(locationLabel, ++row, 1, 1, 3);
            AddWidget(locationTextBox, row, 4, 1, 3);

            AddWidget(technicalResourcesLabel, ++row, 1, 1, 3);
            AddWidget(technicalResourcesTextBox, row, 4, 1, 3);

            AddWidget(liveHighlightsFileLabel, ++row, 1, 1, 3);
            AddWidget(liveHighlightsFileDropDown, row, 4, 1, 3);

            AddWidget(requestedBroadcastTimeLabel, ++row, 1, 1, 3);
            AddWidget(requestedBroadcastTimeDateTimePicker, row, 4, 1, 3);

            AddWidget(productionNumberPlasmaIdLabel, ++row, 1, 1, 3);
            AddWidget(productionNumberPlasmaIdTextBox, row, 4, 1, 3);

            AddWidget(productNumberCeitonLabel, ++row, 1, 1, 3);
            AddWidget(productNumberCeitonTextBox, row, 4, 1, 3);

            AddWidget(costDepartmentLabel, ++row, 1, 1, 3);
            AddWidget(costDepartmentTextBox, row, 4, 1, 3);

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

            sportLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.SportIsVisible;
            sportTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.SportIsVisible;
            sportTextBox.IsEnabled = IsEnabled && configuration.SportIsEnabled;

            descriptionLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.DescriptionIsVisible;
            descriptionTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.DescriptionIsVisible;
            descriptionTextBox.IsEnabled = IsEnabled && configuration.DescriptionIsEnabled;

            commentaryLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.CommentaryIsVisible;
            commentaryDropDown.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.CommentaryIsVisible;
            commentaryDropDown.IsEnabled = IsEnabled && configuration.CommentaryIsEnabled;

            commentary2Label.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.CommentaryTwoIsVisible;
            commentary2DropDown.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.CommentaryTwoIsVisible;
            commentary2DropDown.IsEnabled = IsEnabled && configuration.CommentaryTwoIsEnabled;

            competitionTimeLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.CompetitionTimeIsVisible;
            competitionTimeDateTimePicker.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.CompetitionTimeIsVisible;
            competitionTimeDateTimePicker.IsEnabled = IsEnabled && configuration.CompetitionTimeIsEnabled;

            journalistOneLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.JournalistOneIsVisible;
            journalistOneTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.JournalistOneIsVisible;
            journalistOneTextBox.IsEnabled = IsEnabled && configuration.JournalistOneIsEnabled;

            journalistTwoLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.JournalistTwoIsVisible;
            journalistTwoTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.JournalistTwoIsVisible;
            journalistTwoTextBox.IsEnabled = IsEnabled && configuration.JournalistTwoIsEnabled;

            journalistThreeLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.JournalistThreeIsVisible;
            journalistThreeTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.JournalistThreeIsVisible;
            journalistThreeTextBox.IsEnabled = IsEnabled && configuration.JournalistThreeIsEnabled;

            locationLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.LocationIsVisible;
            locationTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.LocationIsVisible;
            locationTextBox.IsEnabled = IsEnabled && configuration.LocationIsEnabled;

            technicalResourcesLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.TechnicalResourcesIsVisible;
            technicalResourcesTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.TechnicalResourcesIsVisible;
            technicalResourcesTextBox.IsEnabled = IsEnabled && configuration.TechnicalResourcesIsEnabled;

            liveHighlightsFileLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.LiveHighlightsFileIsVisible;
            liveHighlightsFileDropDown.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.LiveHighlightsFileIsVisible;
            liveHighlightsFileDropDown.IsEnabled = IsEnabled && configuration.LiveHighlightsFileIsEnabled;

            requestedBroadcastTimeLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.RequestedBroadcastTimeIsVisible;
            requestedBroadcastTimeDateTimePicker.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.RequestedBroadcastTimeIsVisible;
            requestedBroadcastTimeDateTimePicker.IsEnabled = IsEnabled && configuration.RequestedBroadcastTimeIsEnabled;

            productionNumberPlasmaIdLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.ProductionNumberPlasmaIdIsVisible;
            productionNumberPlasmaIdTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.ProductionNumberPlasmaIdIsVisible;
            productionNumberPlasmaIdTextBox.IsEnabled = IsEnabled && configuration.ProductionNumberPlasmaIdIsEnabled;

            productNumberCeitonLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.ProductNumberCeitonIsVisible;
            productNumberCeitonTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.ProductNumberCeitonIsVisible;
            productNumberCeitonTextBox.IsEnabled = IsEnabled && configuration.ProductNumberCeitonIsEnabled;

            costDepartmentLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.CostDepartmentIsVisible;
            costDepartmentTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.CostDepartmentIsVisible;
            costDepartmentTextBox.IsEnabled = IsEnabled && configuration.CostDepartmentIsEnabled;

            additionalInformationLabel.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.AdditionalInformationIsVisible;
            additionalInformationTextBox.IsVisible = IsVisible && !collapseButton.IsCollapsed && configuration.AdditionalInformationIsVisible;
            additionalInformationTextBox.IsEnabled = IsEnabled && configuration.AdditionalInformationIsEnabled;
            
            ToolTipHandler.SetTooltipVisibility(this);
        }
    }
}