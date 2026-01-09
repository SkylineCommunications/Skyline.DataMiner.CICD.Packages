namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.NonIplayProject
{
	using System;
	using System.Collections.Generic;
	using Library_1.UI.NonLive.NonIplayProject;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ProjectSection : MainSection
	{	
		private readonly Label projectTypeTitle = new Label("Project Type") { Style = TextStyle.Bold };

		private readonly Label projectTypeLabel = new Label("Project type");
		private readonly DropDown projectTypeDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<ProjectTypes>(), EnumExtensions.GetDescriptionFromEnumValue(ProjectTypes.AVID_ISILON));

		private readonly Label avidProjectVideoFormatLabel = new Label("Avid project video format");
		private readonly DropDown avidProjectVideoFormatDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<AvidProjectVideoFormats>(), EnumExtensions.GetDescriptionFromEnumValue(AvidProjectVideoFormats.AVC_INTRA100));

		private readonly Label projectNameLabel = new Label("Project name");
		private readonly YleTextBox projectNameTextBox = new YleTextBox { ValidationText = "Provide a project name", ValidationPredicate = text => !string.IsNullOrWhiteSpace(text) };

		private readonly Label productionNumberLabel = new Label("Production number");
		private readonly YleTextBox productionNumberTextBox = new YleTextBox();

		private readonly Label projectAdditionalInfoLabel = new Label("Additional information");
		private readonly YleTextBox projectAdditionalInfoTextBox = new YleTextBox { IsMultiline = true, Height = 200 };

		private readonly Label isilonBackupInfoTitle = new Label("Isilon Backup Information") { Style = TextStyle.Bold };

		private readonly Label storeYourBackUpsLongerLabel = new Label("Do you need to store your backups longer than one year?");
		private readonly CheckBox storeYourBackUpsLongerCheckBox = new CheckBox { IsChecked = false };

		private readonly Label backupDeletionDateLabel = new Label("Backup deletion date");
		private DateTimePicker backupDeletionDatePicker;
		private readonly Label isilonBackupFileLocationLabel = new Label("Isilon Backup File Location");
        private readonly YleTextBox isilonBackupFileLocationTextBox = new YleTextBox() { IsMultiline = true, Height = 50 };

        private readonly Label whyMustTheBackUpBeStoredLongerLabel = new Label("Why must the back up be stored longer?");
		private readonly YleTextBox whyMustTheBackUpBeStoredLongerTextBox = new YleTextBox { IsMultiline = true, Height = 50, ValidationText = "Define a reason why you want to store the backup longer", ValidationPredicate = text => !string.IsNullOrWhiteSpace(text) };

        private readonly Label additionalInformationTitle = new Label("Additional information") { Style = TextStyle.Bold };
        private readonly Label additionalInfoLabel = new Label("Additional customer information");
		private readonly YleTextBox additionalInfoTextBox = new YleTextBox { IsMultiline = true, Height = 200 };

		private readonly Label sourceMediaCanBeReturnedToKalustovarastoLabel = new Label("Source media can be returned to Kalustovarasto");
		private readonly CheckBox sourceMediaCanBeReturnedToKalustovarastoCheckBox = new CheckBox { IsChecked = true };

		private readonly Label whereShouldTheCardsBeReturnedToLabel = new Label("Where should the cards be returned to?") { IsVisible = false };
		private readonly DropDown whereShouldTheCardsBeReturnedToDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<CardReturnDestinations>(), CardReturnDestinations.ValmiitKaappi.GetDescription()) { IsVisible = false };

		private readonly Label recipientNameLabel = new Label("Name of the recipient") { IsVisible = false };
		private readonly YleTextBox recipientNameTextBox = new YleTextBox { IsVisible = false, ValidationPredicate = text => !string.IsNullOrWhiteSpace(text), ValidationText = "Please provide a value" };

		private readonly Label plNumberLabel = new Label("Enter the PL number for the return of the cards") { IsVisible = false };
		private readonly YleTextBox plNumberTextBox = new YleTextBox { IsVisible = false, ValidationPredicate = text => !string.IsNullOrWhiteSpace(text), ValidationText = "Please provide a value" };

        private readonly NotificationSection notificationSection;
		private readonly ProjectGeneralInfoSection projectGeneralInfoSection;
		private readonly Project project;

		private readonly ProjectSectionConfiguration configuration;

		public ProjectSection(Helpers helpers, ProjectSectionConfiguration configuration, Project project) : base(helpers)
		{
			this.project = project;
			this.configuration = configuration;

			DateTime nowPlusOneYear = DateTime.Now.AddYears(1);
			backupDeletionDatePicker = new YleDateTimePicker(new DateTime(nowPlusOneYear.Year, nowPlusOneYear.Month, nowPlusOneYear.Day)) { DateTimeFormat = DateTimeFormat.ShortDate };

			projectGeneralInfoSection = new ProjectGeneralInfoSection(helpers, configuration, project);
			projectGeneralInfoSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;

			projectGeneralInfoSection.IngestDepartmentChanged += (o, e) => HandleVisibilityAndEnabledUpdate();
			projectTypeDropDown.Changed += ProjectTypeDropDown_Changed;
			storeYourBackUpsLongerCheckBox.Changed += StoreYourBackUpsLongerCheckBox_Changed;
			sourceMediaCanBeReturnedToKalustovarastoCheckBox.Changed += (o, e) => HandleVisibilityAndEnabledUpdate();
			whereShouldTheCardsBeReturnedToDropDown.Changed += (o, e) => HandleVisibilityAndEnabledUpdate();
            notificationSection = new NotificationSection(helpers, configuration, project);
            InitializeProject(project);

			GenerateUi(out int row);
		}

        private void InitializeProject(Project project)
        {
			if (project is null) return;

            try
            {
                projectGeneralInfoSection.Initialize(project);

                ProjectType = EnumExtensions.GetEnumValueFromDescription<ProjectTypes>(project.ProjectType);
                if (ProjectType == ProjectTypes.AVID_ISILON)
                {
                    AvidProjectVideoFormat = EnumExtensions.GetEnumValueFromDescription<AvidProjectVideoFormats>(project.AvidProjectVideoFormat);
                }

                ProjectName = project.ProjectName;
                ProductionNumber = project.ProductionNumber;
                ProjectAdditionalInformation = project.ProjectAdditionalInfo;
                IsLongerStoredBackUpChecked = project.IsLongerStoredBackUpChecked;
                backupDeletionDatePicker = new YleDateTimePicker(new DateTime(project.BackupDeletionDate.Year, project.BackupDeletionDate.Month, project.BackupDeletionDate.Day)) { DateTimeFormat = DateTimeFormat.ShortDate };
                WhyMustBackUpBeStoredLonger = project.WhyMustBackUpBeStoredLonger;
                AdditionalInformation = project.AdditionalInfo;
				IsilonBackupFileLocation = project.IsilonBackupFileLocation;

				sourceMediaCanBeReturnedToKalustovarastoCheckBox.IsChecked = project.ReturnSourceMediaToKalustovarasto;
				if(!string.IsNullOrWhiteSpace(project.CardReturnDestination)) whereShouldTheCardsBeReturnedToDropDown.Selected = project.CardReturnDestination;
				if(!string.IsNullOrWhiteSpace(project.CardReturnRecipientName)) recipientNameTextBox.Text = project.CardReturnRecipientName;
				if(!string.IsNullOrWhiteSpace(project.CardReturnPlNumber)) plNumberTextBox.Text = project.CardReturnPlNumber;
            }
            catch (NullReferenceException ex)
            {
                helpers.Log(nameof(ProjectSection), nameof(InitializeProject), $"Error while constructing ProjectSection: {ex}");
            }      
        }

		public ProjectTypes ProjectType
		{
			get
			{
				return EnumExtensions.GetEnumValueFromDescription<ProjectTypes>(projectTypeDropDown.Selected);
			}
			private set
			{
				projectTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
			}
		}

		public AvidProjectVideoFormats AvidProjectVideoFormat
		{
			get
			{
				return EnumExtensions.GetEnumValueFromDescription<AvidProjectVideoFormats>(avidProjectVideoFormatDropDown.Selected);
			}
			private set
			{
				avidProjectVideoFormatDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
			}
		}

		public string ProjectName
		{
			get
			{
				return projectNameTextBox.Text;
			}
            private set
            {
                projectNameTextBox.Text = value;
            }
		}

		public string ProductionNumber
		{
			get
			{
				return productionNumberTextBox.Text;
			}
			private set
			{
				productionNumberTextBox.Text = value;
			}
		}

		public string ProjectAdditionalInformation
		{
			get
			{
				return projectAdditionalInfoTextBox.Text;
			}
			private set
			{
				projectAdditionalInfoTextBox.Text = value;
			}
		}

        public string IsilonBackupFileLocation
        {
			get
			{
				return isilonBackupFileLocationTextBox.Text;
			}
			private set
			{
				isilonBackupFileLocationTextBox.Text = value;
			}
		}

        public bool IsLongerStoredBackUpChecked
		{
			get
			{
				return storeYourBackUpsLongerCheckBox.IsChecked;
			}
			private set
			{
				storeYourBackUpsLongerCheckBox.IsChecked = value;
			}
		}

		public string WhyMustBackUpBeStoredLonger
		{
			get
			{
				return whyMustTheBackUpBeStoredLongerTextBox.Text;
			}
			private set
			{
				whyMustTheBackUpBeStoredLongerTextBox.Text = value;
			}
		}

		public string AdditionalInformation
		{
			get
			{
				return additionalInfoTextBox.Text;
			}
			private set
			{
				additionalInfoTextBox.Text = value;
			}
		}

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections 
		{
			get
			{
				return new NonLiveManagerTreeViewSection[0];
			}
		}

		public override bool IsValid(OrderAction action)
	    {			
			bool isReasonForLongerBackUpValid = !IsLongerStoredBackUpChecked || whyMustTheBackUpBeStoredLongerTextBox.IsValid;

			bool recipientTextBoxIsValid = !recipientNameTextBox.IsVisible || recipientNameTextBox.IsValid;
			bool plNumberTextBoxIsValid = !plNumberTextBox.IsVisible || plNumberTextBox.IsValid;

			bool optionalwidgetsAreValid = isReasonForLongerBackUpValid && recipientTextBoxIsValid && plNumberTextBoxIsValid;

            bool isGeneralInfoSectionValid = projectGeneralInfoSection.IsValid(action);

			if (action == OrderAction.Save)
            {
				return isGeneralInfoSectionValid;
            }
			else
            {
				return isGeneralInfoSectionValid
					&& projectNameTextBox.IsValid
					&& optionalwidgetsAreValid;
			}
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddSection(projectGeneralInfoSection, new SectionLayout(++row, 0));
			row += projectGeneralInfoSection.RowCount;

			AddWidget(projectTypeTitle, ++row, 0);

			AddWidget(projectTypeLabel, ++row, 0);
			AddWidget(projectTypeDropDown, row, 1, 1, 2);

			AddWidget(avidProjectVideoFormatLabel, ++row, 0);
			AddWidget(avidProjectVideoFormatDropDown, row, 1, 1, 2);

			AddWidget(projectNameLabel, ++row, 0);
			AddWidget(projectNameTextBox, row, 1, 1, 2);

			AddWidget(productionNumberLabel, ++row, 0);
			AddWidget(productionNumberTextBox, row, 1, 1, 2);

			AddWidget(projectAdditionalInfoLabel, ++row, 0);
			AddWidget(projectAdditionalInfoTextBox, row, 1, 1, 2);

			AddWidget(sourceMediaCanBeReturnedToKalustovarastoLabel, ++row, 0);
			AddWidget(sourceMediaCanBeReturnedToKalustovarastoCheckBox, row, 1, 1, 2);

			AddWidget(whereShouldTheCardsBeReturnedToLabel, ++row, 0);
			AddWidget(whereShouldTheCardsBeReturnedToDropDown, row, 1, 1, 2);

			AddWidget(recipientNameLabel, ++row, 0);
			AddWidget(recipientNameTextBox, row, 1, 1, 2);

			AddWidget(plNumberLabel, ++row, 0);
			AddWidget(plNumberTextBox, row, 1, 1, 2);

			AddWidget(isilonBackupInfoTitle, ++row, 0);

			AddWidget(storeYourBackUpsLongerLabel, ++row, 0);
			AddWidget(storeYourBackUpsLongerCheckBox, row, 1, 1, 2);

			AddWidget(backupDeletionDateLabel, ++row, 0);
			AddWidget(backupDeletionDatePicker, row, 1);

			AddWidget(whyMustTheBackUpBeStoredLongerLabel, ++row, 0);
			AddWidget(whyMustTheBackUpBeStoredLongerTextBox, row, 1, 1, 2);

			AddWidget(additionalInformationTitle, ++row, 0);
			AddWidget(additionalInfoLabel, ++row, 0, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(additionalInfoTextBox, row, 1, 1, 2);

			AddSection(notificationSection, new SectionLayout(++row, 0));

			AddWidget(isilonBackupFileLocationLabel, ++row, 0);
			AddWidget(isilonBackupFileLocationTextBox, row, 1, 1, 2);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		public override void RegenerateUi()
		{
			projectGeneralInfoSection.RegenerateUi();
			GenerateUi(out int row);	
        }

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			projectGeneralInfoSection.IsVisible = IsVisible;
			projectGeneralInfoSection.IsEnabled = IsEnabled;

			projectTypeTitle.IsVisible = IsVisible;

			projectTypeLabel.IsVisible = IsVisible;
			projectTypeDropDown.IsVisible = IsVisible;
			projectTypeDropDown.IsEnabled = IsEnabled;

			avidProjectVideoFormatLabel.IsVisible = ProjectType == ProjectTypes.AVID_ISILON;
			avidProjectVideoFormatDropDown.IsVisible = avidProjectVideoFormatLabel.IsVisible;
			avidProjectVideoFormatDropDown.IsEnabled = IsEnabled;

			projectNameLabel.IsVisible = IsVisible;
			projectNameTextBox.IsVisible = IsVisible;
			projectNameTextBox.IsEnabled = IsEnabled;

			productionNumberLabel.IsVisible = IsVisible;
			productionNumberTextBox.IsVisible = IsVisible;
			productionNumberTextBox.IsEnabled = IsEnabled;

			projectAdditionalInfoLabel.IsVisible = IsVisible;
			projectAdditionalInfoTextBox.IsVisible = IsVisible;
			projectAdditionalInfoTextBox.IsEnabled = IsEnabled;

			sourceMediaCanBeReturnedToKalustovarastoLabel.IsVisible = projectGeneralInfoSection.IngestDepartment == IngestDepartments.HELSINKI;
			sourceMediaCanBeReturnedToKalustovarastoCheckBox.IsVisible = sourceMediaCanBeReturnedToKalustovarastoLabel.IsVisible;
			sourceMediaCanBeReturnedToKalustovarastoCheckBox.IsEnabled = IsEnabled;

			whereShouldTheCardsBeReturnedToLabel.IsVisible = sourceMediaCanBeReturnedToKalustovarastoCheckBox.IsVisible && !sourceMediaCanBeReturnedToKalustovarastoCheckBox.IsChecked;
			whereShouldTheCardsBeReturnedToDropDown.IsVisible = whereShouldTheCardsBeReturnedToLabel.IsVisible;
			whereShouldTheCardsBeReturnedToDropDown.IsEnabled = IsEnabled;

			recipientNameLabel.IsVisible = whereShouldTheCardsBeReturnedToLabel.IsVisible && whereShouldTheCardsBeReturnedToDropDown.Selected == CardReturnDestinations.InternalMail.GetDescription();
			recipientNameTextBox.IsVisible = recipientNameLabel.IsVisible;
			recipientNameTextBox.IsEnabled = IsEnabled;

			plNumberLabel.IsVisible = recipientNameLabel.IsVisible;
			plNumberTextBox.IsVisible = recipientNameLabel.IsVisible;
			plNumberTextBox.IsEnabled = IsEnabled;

			isilonBackupInfoTitle.IsVisible = IsVisible;

			isilonBackupFileLocationLabel.IsVisible = IsVisible && configuration.IsilonBackupFileLocationIsVisible;
			isilonBackupFileLocationTextBox.IsVisible = IsVisible && configuration.IsilonBackupFileLocationIsVisible;
			isilonBackupFileLocationTextBox.IsEnabled = IsEnabled && configuration.IsilonBackupFileLocationIsEnabled;

			storeYourBackUpsLongerLabel.IsVisible = IsVisible;
			storeYourBackUpsLongerCheckBox.IsVisible = IsVisible;
			storeYourBackUpsLongerCheckBox.IsEnabled = IsEnabled;

			backupDeletionDateLabel.IsVisible = IsLongerStoredBackUpChecked;
			backupDeletionDatePicker.IsVisible = backupDeletionDateLabel.IsVisible;
			backupDeletionDatePicker.IsEnabled = IsEnabled;

			whyMustTheBackUpBeStoredLongerLabel.IsVisible = IsLongerStoredBackUpChecked;
			whyMustTheBackUpBeStoredLongerTextBox.IsVisible = whyMustTheBackUpBeStoredLongerLabel.IsVisible;
			whyMustTheBackUpBeStoredLongerTextBox.IsEnabled = IsEnabled;

			additionalInfoLabel.IsVisible = IsVisible;
			additionalInfoTextBox.IsVisible = IsVisible;
			additionalInfoTextBox.IsEnabled = IsEnabled;

			notificationSection.IsVisible = IsVisible;
			notificationSection.IsEnabled = IsEnabled;	

            ToolTipHandler.SetTooltipVisibility(this);
        }

		public override void UpdateNonLiveOrder(NonLiveOrder nonLiveOrder)
		{
			if (nonLiveOrder.OrderType != IngestExport.Type.NonInterplayProject) return;

			Project projectOrder = (Project)nonLiveOrder;

			if (this.project != null)
			{
				projectOrder.DataMinerId = project.DataMinerId;
				projectOrder.TicketId = project.TicketId;
			}

			projectGeneralInfoSection.UpdateProject(projectOrder);			

			projectOrder.ProjectType = EnumExtensions.GetDescriptionFromEnumValue(ProjectType);
			if (ProjectType == ProjectTypes.AVID_ISILON)
			{
				projectOrder.AvidProjectVideoFormat = EnumExtensions.GetDescriptionFromEnumValue(AvidProjectVideoFormat);
			}

            projectOrder.ProjectName = ProjectName;
			projectOrder.ProductionNumber = ProductionNumber;
			projectOrder.ProjectAdditionalInfo = ProjectAdditionalInformation;

			projectOrder.ReturnSourceMediaToKalustovarasto = sourceMediaCanBeReturnedToKalustovarastoCheckBox.IsVisible && sourceMediaCanBeReturnedToKalustovarastoCheckBox.IsChecked;

			projectOrder.CardReturnDestination = whereShouldTheCardsBeReturnedToDropDown.IsVisible ? whereShouldTheCardsBeReturnedToDropDown.Selected : null;
			projectOrder.CardReturnRecipientName = recipientNameTextBox.IsVisible ? recipientNameTextBox.Text : null;
			projectOrder.CardReturnPlNumber = plNumberTextBox.IsVisible ? plNumberTextBox.Text : null;

			projectOrder.IsilonBackupFileLocation = IsilonBackupFileLocation;
			projectOrder.IsLongerStoredBackUpChecked = IsLongerStoredBackUpChecked;
			projectOrder.BackupDeletionDate = backupDeletionDatePicker.DateTime;
			projectOrder.WhyMustBackUpBeStoredLonger = WhyMustBackUpBeStoredLonger;
			projectOrder.AdditionalInfo = AdditionalInformation;
            projectOrder.EmailReceivers = notificationSection.GetEmails();

            // Update non - live teams filtering
            projectOrder.TeamMgmt = false;
			projectOrder.TeamNews = false;
			projectOrder.TeamHki = projectGeneralInfoSection.IngestDepartment == IngestDepartments.HELSINKI;
			projectOrder.TeamTre = projectGeneralInfoSection.IngestDepartment == IngestDepartments.TAMPERE;
			projectOrder.TeamVsa = projectGeneralInfoSection.IngestDepartment == IngestDepartments.VAASA;
		}

		private void ProjectTypeDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				ProjectType = EnumExtensions.GetEnumValueFromDescription<ProjectTypes>(e.Selected);

				HandleVisibilityAndEnabledUpdate();
				IsValid(OrderAction.Book);
			}
		}

		private void StoreYourBackUpsLongerCheckBox_Changed(object sender, CheckBox.CheckBoxChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				HandleVisibilityAndEnabledUpdate();
				IsValid(OrderAction.Book);
			}
		}
	}
}
