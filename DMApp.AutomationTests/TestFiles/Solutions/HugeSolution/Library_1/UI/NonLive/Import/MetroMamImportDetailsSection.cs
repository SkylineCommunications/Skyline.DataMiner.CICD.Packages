namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import
{
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class MetroMamImportDetailsSection : ImportSubSection
    {
        private readonly Label programNameLabel = new Label("Program Name");
        private readonly YleTextBox programNameTextBox = new YleTextBox { ValidationPredicate = x => !string.IsNullOrWhiteSpace(x), ValidationText = "Please provide any program name" };

        private readonly Label idLabel = new Label("ID");
        private readonly YleTextBox idTextBox = new YleTextBox { ValidationPredicate = x => !string.IsNullOrWhiteSpace(x), ValidationText = "Please provide any ID" };

        private readonly Label additionalInfoLabel = new Label("Additional info about high frame rate footage");
        private readonly YleTextBox additionalInfoTextBox = new YleTextBox { IsMultiline = true, Height = 200, PlaceHolder = "Provide some additional information..." };

        public MetroMamImportDetailsSection(Helpers helpers, ISectionConfiguration configuration, ImportMainSection section, MetroMamImportDetails metroMamImportDetails = null) : base(helpers, configuration, section, null)
        {
            if (metroMamImportDetails != null)
            {
                programNameTextBox.Text = metroMamImportDetails.ProgramName;
                idTextBox.Text = metroMamImportDetails.Id;
                additionalInfoTextBox.Text = metroMamImportDetails.AdditionalInformation;
            }

            GenerateUi(out int row);
        }

        public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
        {
            get
            {
                return new NonLiveManagerTreeViewSection[0];
            }
        }

        protected override void GenerateUi(out int row)
        {
            base.GenerateUi(out row);

            AddWidget(programNameLabel, ++row, 0);
            AddWidget(programNameTextBox, row, 1, 1, 2);

            AddWidget(idLabel, ++row, 0);
            AddWidget(idTextBox, row, 1, 1, 2);

            AddWidget(additionalInfoLabel, ++row, 0);
            AddWidget(additionalInfoTextBox, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

        public override bool IsValid()
        {
            bool isProgramNameValid = programNameTextBox.IsValid;
            bool isIdValid = idTextBox.IsValid;

            return isProgramNameValid && isIdValid;
        }

        public override void UpdateIngest(Ingest ingest)
        {
            ingest.MetroMamImportDetails = ingest.MetroMamImportDetails ?? new List<MetroMamImportDetails>();

            ingest.MetroMamImportDetails.Add(new MetroMamImportDetails
            {
                ProgramName = programNameTextBox.Text,
                Id = idTextBox.Text,
                AdditionalInformation = additionalInfoTextBox.Text,
            });
        }

        protected override void HandleVisibilityAndEnabledUpdate()
        {
            programNameLabel.IsVisible = IsVisible && ingestMainSection.IngestDestination == AvidInterplayPAM.InterplayPamElements.Vaasa;
            programNameTextBox.IsVisible = programNameLabel.IsVisible;
			programNameTextBox.IsEnabled = IsEnabled;

            idLabel.IsVisible = IsVisible && ingestMainSection.IngestDestination == AvidInterplayPAM.InterplayPamElements.Vaasa;
            idTextBox.IsVisible = idLabel.IsVisible;
			idTextBox.IsEnabled = IsEnabled;

            additionalInfoLabel.IsVisible = IsVisible && ingestMainSection.IngestDestination == AvidInterplayPAM.InterplayPamElements.Vaasa;
            additionalInfoTextBox.IsVisible = additionalInfoLabel.IsVisible;
			additionalInfoTextBox.IsEnabled = IsEnabled;

            ToolTipHandler.SetTooltipVisibility(this);
        }

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}
	}
}
