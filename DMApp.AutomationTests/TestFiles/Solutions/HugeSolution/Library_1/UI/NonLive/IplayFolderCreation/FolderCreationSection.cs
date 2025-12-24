namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.IplayFolderCreation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class FolderCreationSection : MainSection
	{
		private readonly FolderCreation folderCreation;

        private readonly Label additionalInformationTitle = new Label("Additional information") { Style = TextStyle.Bold };
        private readonly Label additionalInformationLabel = new Label("Additional customer information");
        private readonly YleTextBox additionalInformationTextBox = new YleTextBox { IsMultiline = true, Height = 200 };

        private readonly Label newFolderRequestsTitle = new Label("New Folder Requests") { Style = TextStyle.Bold };

        private readonly NotificationSection notificationSection;

		private readonly Button addEpisodeFolderRequestButton = new Button("Add episode folder request");
		private readonly ISectionConfiguration configuration = new NonLiveOrderConfiguration();

		public FolderCreationSection(Helpers helpers ,FolderCreation folderCreation = null) : base(helpers)
		{
			this.folderCreation = folderCreation;
            notificationSection = new NotificationSection(helpers, configuration, folderCreation);
            InitializeFolderCreation();

			addEpisodeFolderRequestButton.Pressed += AddEpisodeFolderRequestButton_Pressed;

			GenerateUi(out int row);
		}

		public IPlayFolderCreationGeneralInfoSection GeneralInfoSection { get; private set; }

		public NewProgramFolderRequestSection NewProgramFolderRequestSection { get; set; }

		public List<NewEpisodeFolderRequestSection> NewEpisodeFolderRequestSections { get; private set; }

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return GeneralInfoSection.TreeViewSections;
			}
		}

		public override bool IsValid(OrderAction action)
		{
			bool isGeneralInfoSectionValid = GeneralInfoSection.IsValid(action);
			bool isNewProgramFolderRequestSectionValid = (NewProgramFolderRequestSection == null || (NewProgramFolderRequestSection != null && NewProgramFolderRequestSection.IsValid()));
			bool areNewEpisodeFolderRequestSectionsValid = !NewEpisodeFolderRequestSections.Any(x => !x.IsValid());

            if (action == OrderAction.Save)
            {
                return isGeneralInfoSectionValid;
            }
            else
            {
                return isGeneralInfoSectionValid
                    && isNewProgramFolderRequestSectionValid
                    && areNewEpisodeFolderRequestSectionsValid;
            }
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddSection(GeneralInfoSection, new SectionLayout(++row, 0));
			row += GeneralInfoSection.RowCount;

			AddWidget(newFolderRequestsTitle, ++row, 0);

			if (NewProgramFolderRequestSection != null)
			{
				AddSection(NewProgramFolderRequestSection, new SectionLayout(++row, 0));
				row += NewProgramFolderRequestSection.RowCount;
			}

			foreach (var newEpisodeFolderRequestSection in NewEpisodeFolderRequestSections)
			{
				AddSection(newEpisodeFolderRequestSection, new SectionLayout(++row, 0));
				row += newEpisodeFolderRequestSection.RowCount;
			}

			AddWidget(addEpisodeFolderRequestButton, ++row, 0);

            AddWidget(additionalInformationTitle, ++row, 0);
            AddWidget(additionalInformationLabel, ++row, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
            AddWidget(additionalInformationTextBox, row, 1, 1, 2);

            AddSection(notificationSection, new SectionLayout(row + 1, 0));

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			GeneralInfoSection.IsVisible = IsVisible;
			GeneralInfoSection.IsEnabled = IsEnabled;

			newFolderRequestsTitle.IsVisible = NewEpisodeFolderRequestSections.Count > 0 || NewProgramFolderRequestSection != null;

			if (NewProgramFolderRequestSection != null)
			{
				NewProgramFolderRequestSection.IsVisible = IsVisible;
				NewProgramFolderRequestSection.IsEnabled = IsEnabled;
			}

			foreach (var newEpisodeFolderRequestSection in NewEpisodeFolderRequestSections)
			{
				newEpisodeFolderRequestSection.IsVisible = IsVisible;
				newEpisodeFolderRequestSection.IsEnabled = IsEnabled;
			}

			addEpisodeFolderRequestButton.IsVisible = NewEpisodeFolderRequestSections.Count > 0 || NewProgramFolderRequestSection != null;
			addEpisodeFolderRequestButton.IsEnabled = IsEnabled;

			additionalInformationLabel.IsVisible = IsVisible;
			additionalInformationTextBox.IsVisible = IsVisible;
			additionalInformationTextBox.IsEnabled = IsEnabled;

			notificationSection.IsVisible = IsVisible;
			notificationSection.IsEnabled = IsEnabled;

            ToolTipHandler.SetTooltipVisibility(this);
        }

		public override void UpdateNonLiveOrder(NonLiveOrder nonLiveOrder)
        {
            if (nonLiveOrder.OrderType != IngestExport.Type.IplayFolderCreation) return;

            FolderCreation folderCreationOrder = (FolderCreation)nonLiveOrder;

            if (this.folderCreation != null)
            {
                folderCreationOrder.DataMinerId = this.folderCreation.DataMinerId != null ? this.folderCreation.DataMinerId : null;
                folderCreationOrder.TicketId = this.folderCreation.TicketId != null ? this.folderCreation.TicketId : null;
            }

            GeneralInfoSection.UpdateFolderCreation(folderCreationOrder);

            NewProgramFolderRequestSection?.UpdateFolderCreation(folderCreationOrder);

            folderCreationOrder.NewEpisodeFolderRequestDetails = new List<NewEpisodeFolderRequestDetails>();
            foreach (var requestDetails in NewEpisodeFolderRequestSections)
            {
                requestDetails.UpdateFolderCreation(folderCreationOrder);
            }

            folderCreationOrder.AdditionalInformation = additionalInformationTextBox.Text;
            folderCreationOrder.EmailReceivers = notificationSection.GetEmails();

            UpdateFolderCreationTeamsFiltering(folderCreationOrder);
        }

        private void UpdateFolderCreationTeamsFiltering(FolderCreation folderCreationOrder)
        {
            folderCreationOrder.TeamHki = false;
            folderCreationOrder.TeamNews = false;
            folderCreationOrder.TeamTre = false;
            folderCreationOrder.TeamVsa = false;
            folderCreationOrder.TeamMgmt = true;

            if (GeneralInfoSection.Destination == AvidInterplayPAM.InterplayPamElements.Vaasa && NewProgramFolderRequestSection == null && NewEpisodeFolderRequestSections.Count > 0)
            {
                folderCreationOrder.TeamVsa = true;
                folderCreationOrder.TeamMgmt = false;
            }
            else if (GeneralInfoSection.Destination == AvidInterplayPAM.InterplayPamElements.Vaasa && NewProgramFolderRequestSection != null && NewEpisodeFolderRequestSections.Count > 0)
            {
                folderCreationOrder.TeamVsa = true;
                folderCreationOrder.TeamMgmt = true;
            }
            else
            {
                // Do nothing
            }
        }

        public void DeleteButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				NewEpisodeFolderRequestSections.Remove(NewEpisodeFolderRequestSections.First(x => x.DeleteButton == (Button)sender));

				InvokeRegenerateUi();
			}
		}

        private void InitializeFolderCreation()
        {
            GeneralInfoSection = new IPlayFolderCreationGeneralInfoSection(helpers, configuration, this, folderCreation);
			GeneralInfoSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			GeneralInfoSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			NewEpisodeFolderRequestSections = new List<NewEpisodeFolderRequestSection>();

            if (folderCreation != null)
            {
                if (folderCreation.NewProgramFolderRequestDetails != null)
                {
                    NewProgramFolderRequestSection = new NewProgramFolderRequestSection(helpers, configuration, this, folderCreation.NewProgramFolderRequestDetails);

					NewProgramFolderRequestSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
					NewProgramFolderRequestSection.RegenerateUiRequired += HandleRegenerateUiRequired;
				}

				if (folderCreation.NewEpisodeFolderRequestDetails != null)
                {
                    foreach (var newEpisodeRequestDetails in folderCreation.NewEpisodeFolderRequestDetails)
                    {
						var newEpisodeRequestSection = new NewEpisodeFolderRequestSection(helpers, configuration, this, newEpisodeRequestDetails);

						newEpisodeRequestSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
						newEpisodeRequestSection.RegenerateUiRequired += HandleRegenerateUiRequired;

						NewEpisodeFolderRequestSections.Add(newEpisodeRequestSection);
                    }
                }

                additionalInformationTextBox.Text = folderCreation.AdditionalInformation;
            }
        }

		private void AddEpisodeFolderRequestButton_Pressed(object sender, EventArgs e)
        {
			using (UiDisabler.StartNew(this))
			{
				var newEpisodeFolderRequestSection = new NewEpisodeFolderRequestSection(helpers, configuration, this);
				newEpisodeFolderRequestSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

				AutoPopulateNewlyCreatedEpisodeFolderSection(newEpisodeFolderRequestSection);

				NewEpisodeFolderRequestSections.Add(newEpisodeFolderRequestSection);

				InvokeRegenerateUi();
			}
        }

        private void AutoPopulateNewlyCreatedEpisodeFolderSection(NewEpisodeFolderRequestSection newEpisodeFolderRequestSections)
        {
            if (NewEpisodeFolderRequestSections.Any())
            {
                var firstEpisodeFolderRequestSection = NewEpisodeFolderRequestSections[0];
                PropertyCopier<NewEpisodeFolderRequestSection>.Copy(firstEpisodeFolderRequestSection, newEpisodeFolderRequestSections);
            }
        }

		public override void RegenerateUi()
		{
			GeneralInfoSection.RegenerateUi();
			NewProgramFolderRequestSection?.RegenerateUi();
			NewEpisodeFolderRequestSections.ForEach(x => x.RegenerateUi());
			GenerateUi(out int row);
		}
	}
}
