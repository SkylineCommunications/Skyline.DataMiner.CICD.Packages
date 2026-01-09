namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.MediaParkki;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder.ExportFileSelectorSections;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ExportInformationSection : ExportSourceSection
	{
        private readonly Label title = new Label("Export Information") { Style = TextStyle.Bold }; 
		private readonly Label titleForAdditionalInfo = new Label("Additional Info") { Style = TextStyle.Bold };

        private readonly Label exportDepartmentLabel = new Label("Export department");
		private readonly DropDown exportDepartmentDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<ExportDepartments>().OrderBy(x => x), EnumExtensions.GetDescriptionFromEnumValue(ExportDepartments.HELSINKI));

		private readonly Label yleLogoInUpperLeftCornerLabel = new Label("YLE logo in upper left corner of the video");
		private readonly CheckBox yleLogoInUpperLeftCornerCheckbox = new CheckBox();

		private readonly Label sourceTimecodeBurninToVideoLabel = new Label("Source time code burn-in to video");
		private readonly CheckBox sourceTimecodeBurninToVideoCheckbox = new CheckBox();

		private readonly Label addSubsToVideoLabel = new Label("Add subtitles to video");
		private readonly CheckBox addSubsToVideoCheckbox = new CheckBox();

		private readonly Label exportTargetLabel = new Label("Target of export");
		private readonly DropDown exportTargetDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<ExportTargets>().OrderBy(x => x), EnumExtensions.GetDescriptionFromEnumValue(ExportTargets.Mediaparkki));

		private readonly NonLiveManagerTreeViewSection mediaParkkiTreeViewSection;
        private readonly ExportInformationFileSelectorSection exportInformationFileSelectorSection;

		private readonly string hddInstructionHelsinki = "Deliver HDD to Messi's locker in Lahetyskeskus 3rd floor's staircase lobby.";
		private readonly string hddInstructionTampere = "Deliver HDD to Mediaputiikki";
		private readonly string hddInstructionVaasa = "Deliver HDD to Mediamylly";
		private readonly Label hddInstructionLabel = new Label("Deliver HDD to Messi's locker in Lahetyskeskus 4th floor's staircase lobby.");

		private readonly Label asperaFaspexReceiversEmailLabel = new Label("Receivers email address");
		private readonly YleTextBox asperaFaspexReceiversEmailTextBox = new YleTextBox { IsMultiline = true, Height = 50 };

		private readonly Label asperaFaspexMessageHeadlineLabel = new Label("Message headline");
		private readonly YleTextBox asperaFaspexMessageHeadlineTextBox = new YleTextBox();

		private readonly Label asperaFaspexMessageLabel = new Label("Message content");
		private readonly YleTextBox asperaFaspexMessageTextBox = new YleTextBox(string.Empty) { IsMultiline = true, Height = 250 };

		private readonly Label otherExportTargetLabel = new Label("Destination for the export");
		private readonly YleTextBox otherExportTargetTextBox = new YleTextBox { IsMultiline = true, Height = 100 };

        private readonly Label additionalInfoLabel = new Label("Additional info about this order");
        private readonly YleTextBox additionalInfoTextBox = new YleTextBox { IsMultiline = true, Height = 200, PlaceHolder = "Provide some additional information..." };

        public ExportInformationSection(Helpers helpers, ISectionConfiguration configuration, ExportMainSection section, Export export) : base(helpers, configuration, section, export)
		{
            mediaParkkiTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new MediaParkkiManager(base.helpers), TreeViewType.FolderSelector);
			mediaParkkiTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			mediaParkkiTreeViewSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			mediaParkkiTreeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);
			mediaParkkiTreeViewSection.SourceLabel.Text = "Target folder";
			mediaParkkiTreeViewSection.InitRoot();

            exportInformationFileSelectorSection = new ExportInformationFileSelectorSection(helpers, configuration, this, export);
			exportInformationFileSelectorSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			exportInformationFileSelectorSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			InitializeExport(export);

			exportDepartmentDropDown.Changed += ExportdepartmentDropDown_Changed;
			addSubsToVideoCheckbox.Changed += AddSubsToVideoCheckBox_Changed;
			exportTargetDropDown.Changed += ExportTargetDropDown_Changed;

			GenerateUi(out int row);
		}

        public Folder Folder => mediaParkkiTreeViewSection.SelectedFolder;

		public ExportDepartments ExportDepartment
		{
			get
			{
				return EnumExtensions.GetEnumValueFromDescription<ExportDepartments>(exportDepartmentDropDown.Selected);
			}
			private set
			{
				exportDepartmentDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
				switch (value)
				{
					case ExportDepartments.TAMPERE:
						hddInstructionLabel.Text = hddInstructionTampere;
						break;
					case ExportDepartments.HELSINKI:
						hddInstructionLabel.Text = hddInstructionHelsinki;
						break;
					case ExportDepartments.VAASA:
						hddInstructionLabel.Text = hddInstructionVaasa;
						break;
                    default:
                        // do nothing
                        break;
				}
			}
		}

        public bool YleLogoInUpperLeftCorner
		{
			get
			{
				return yleLogoInUpperLeftCornerCheckbox.IsChecked;
			}
			private set
			{
				if (value != YleLogoInUpperLeftCorner)
				{
					yleLogoInUpperLeftCornerCheckbox.IsChecked = value;
				}
			}
		}

		public bool SourceTimecodeBurnin
		{
			get
			{
				return sourceTimecodeBurninToVideoCheckbox.IsChecked;
			}
			private set
			{
				if (value != SourceTimecodeBurnin)
				{
					sourceTimecodeBurninToVideoCheckbox.IsChecked = value;
				}
			}
		}

		public bool AddSubtitles
		{
			get
			{
				return addSubsToVideoCheckbox.IsChecked;
			}
			private set
			{
				if (value != AddSubtitles)
				{
					addSubsToVideoCheckbox.IsChecked = value;
				}
			}
		}

        public ExportTargets ExportTarget
		{
			get
			{
				return EnumExtensions.GetEnumValueFromDescription<ExportTargets>(exportTargetDropDown.Selected);
			}
			private set
			{
				exportTargetDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(value);
			}
		}

		public string ReceiversEmailAddress
		{
			get
			{
				return asperaFaspexReceiversEmailTextBox.Text;
			}
			private set
			{
				asperaFaspexReceiversEmailTextBox.Text = value;
			}
		}

		public string MessageHeadline
		{
			get
			{
				return asperaFaspexMessageHeadlineTextBox.Text;
			}
			private set
			{
				asperaFaspexMessageHeadlineTextBox.Text = value;
			}
		}

		public string Message
		{
			get
			{
				return asperaFaspexMessageTextBox.Text;
			}
			private set
			{
				asperaFaspexMessageTextBox.Text = value;
			}
		}

		public string OtherExportTarget
		{
			get
			{
				return otherExportTargetTextBox.Text;
			}
			private set
			{
				otherExportTargetTextBox.Text = value;
			}
		}

        public string AdditionalInfo
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
				return new[] { mediaParkkiTreeViewSection };
			}
		}

		public override bool IsValid(OrderAction action)
		{
			using (UiDisabler.StartNew(this))
			{
				if (action == OrderAction.Save) return true;

				bool isMediaParkkiTargetFolderValid = ExportTarget != ExportTargets.Mediaparkki || Folder != null;
				mediaParkkiTreeViewSection.ValidationState = isMediaParkkiTargetFolderValid ? UIValidationState.Valid : UIValidationState.Invalid;
				mediaParkkiTreeViewSection.ValidationText = "Select a target folder";

				bool isReceiversEmailAddressValid = ExportTarget != ExportTargets.AsperaFaspex || !String.IsNullOrWhiteSpace(ReceiversEmailAddress);
				asperaFaspexReceiversEmailTextBox.ValidationState = isReceiversEmailAddressValid ? UIValidationState.Valid : UIValidationState.Invalid;
				asperaFaspexReceiversEmailTextBox.ValidationText = "Provide the receivers email address";

				bool isOtherExportTargetValid = ExportTarget != ExportTargets.Other || !String.IsNullOrEmpty(OtherExportTarget);
				otherExportTargetTextBox.ValidationState = isOtherExportTargetValid ? UIValidationState.Valid : UIValidationState.Invalid;
				otherExportTargetTextBox.ValidationText = "Provide an export destination";

				if (section.ExportSourceSection is InterplayPamMaterialExportSource pamMaterialExportSource && pamMaterialExportSource.Element == InterplayPamElements.Vaasa)
				{
					isMediaParkkiTargetFolderValid = true;
				}

				bool isAsperaFaspexMessageValid = ExportTarget != ExportTargets.AsperaFaspex || !string.IsNullOrWhiteSpace(asperaFaspexMessageTextBox.Text);
				asperaFaspexMessageTextBox.ValidationState = isAsperaFaspexMessageValid ? UIValidationState.Valid : UIValidationState.Invalid;
				asperaFaspexMessageTextBox.ValidationText = $"Please provide any valid message";

				bool areAllTextFieldsCorrectlyFilledIn = isMediaParkkiTargetFolderValid && isReceiversEmailAddressValid && isOtherExportTargetValid && isAsperaFaspexMessageValid;

				return exportInformationFileSelectorSection.IsValid() && areAllTextFieldsCorrectlyFilledIn;
			}
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(title, ++row, 0);

			AddWidget(exportDepartmentLabel, ++row, 0);
			AddWidget(exportDepartmentDropDown, row, 1, 1, 2);

			AddWidget(yleLogoInUpperLeftCornerLabel, ++row, 0);
			AddWidget(yleLogoInUpperLeftCornerCheckbox, row, 1, 1, 2);

			AddWidget(sourceTimecodeBurninToVideoLabel, ++row, 0);
			AddWidget(sourceTimecodeBurninToVideoCheckbox, row, 1, 1, 2);

			AddWidget(addSubsToVideoLabel, ++row, 0);
			AddWidget(addSubsToVideoCheckbox, row, 1, 1, 2);

            AddWidget(new WhiteSpace(), ++row, 0);

			AddSection(exportInformationFileSelectorSection, new SectionLayout(++row, 0));
            row += exportInformationFileSelectorSection.RowCount;

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(exportTargetLabel, ++row, 0);
			AddWidget(exportTargetDropDown, row, 1, 1, 2);

			AddSection(mediaParkkiTreeViewSection, new SectionLayout(++row, 0));

			AddWidget(hddInstructionLabel, ++row, 1, 1, 2);

			AddWidget(asperaFaspexReceiversEmailLabel, ++row, 0);
			AddWidget(asperaFaspexReceiversEmailTextBox, row, 1, 1, 2);

			AddWidget(asperaFaspexMessageHeadlineLabel, ++row, 0);
			AddWidget(asperaFaspexMessageHeadlineTextBox, row, 1, 1, 2);

			AddWidget(asperaFaspexMessageLabel, ++row, 0);
			AddWidget(asperaFaspexMessageTextBox, row, 1, 1, 2);

			AddWidget(otherExportTargetLabel, ++row, 0);
			AddWidget(otherExportTargetTextBox, row, 1, 1, 2);

            AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(titleForAdditionalInfo, ++row, 0);
            AddWidget(additionalInfoLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
            AddWidget(additionalInfoTextBox, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

        public override void InitializeExport(Export export)
        {
            if (export == null || export.ExportInformation == null) return;

            exportInformationFileSelectorSection.Initialize(export.ExportInformation.ExportInformationFileAttachmentInfo);

            if (!String.IsNullOrEmpty(export.ExportInformation.MediaparkkiTargetFolder)) mediaParkkiTreeViewSection.Initialize(new string[] { export.ExportInformation.MediaparkkiTargetFolder });

            ExportDepartment = EnumExtensions.GetEnumValueFromDescription<ExportDepartments>(export.ExportInformation.ExportDepartment);
            YleLogoInUpperLeftCorner = export.ExportInformation.YleLogoInUpperLeftCornerOfVideo;
            SourceTimecodeBurnin = export.ExportInformation.SourceTimecodeBurninToVideo;
            AddSubtitles = export.ExportInformation.AddSubtitlesToVideo;
            ExportTarget = EnumExtensions.GetEnumValueFromDescription<ExportTargets>(export.ExportInformation.TargetOfExport);
            ReceiversEmailAddress = export.ExportInformation.AsperaFaspexReceiversEmailAddress;
            MessageHeadline = export.ExportInformation.AsperaFaspexMessageHeadline;
            Message = export.ExportInformation.AsperaFaspexMessage;
            OtherExportTarget = export.ExportInformation.OtherExportTarget;
			additionalInfoTextBox.Text = export.AdditionalInformation;
        }

        public override void UpdateExport(Export export)
        {
            export.ExportInformation = export.ExportInformation ?? new ExportInformation();
          
            export.ExportInformation.ExportDepartment = EnumExtensions.GetDescriptionFromEnumValue(ExportDepartment);
            export.ExportInformation.YleLogoInUpperLeftCornerOfVideo = YleLogoInUpperLeftCorner;
            export.ExportInformation.SourceTimecodeBurninToVideo = SourceTimecodeBurnin;
            export.ExportInformation.AddSubtitlesToVideo = AddSubtitles;
            export.ExportInformation.TargetOfExport = EnumExtensions.GetDescriptionFromEnumValue(ExportTarget);
            export.ExportInformation.MediaparkkiTargetFolder = ExportTarget == ExportTargets.Mediaparkki ? Folder?.URL : null;
            export.ExportInformation.AsperaFaspexReceiversEmailAddress = ExportTarget == ExportTargets.AsperaFaspex ? ReceiversEmailAddress : null;
            export.ExportInformation.AsperaFaspexMessageHeadline = ExportTarget == ExportTargets.AsperaFaspex ? MessageHeadline : null;
            export.ExportInformation.AsperaFaspexMessage = ExportTarget == ExportTargets.AsperaFaspex ? Message : null;
            export.ExportInformation.OtherExportTarget = OtherExportTarget;
			export.AdditionalInformation = additionalInfoTextBox.Text;
            if (AddSubtitles)
            {
                exportInformationFileSelectorSection.UpdateFileAttachmentInfo(export.ExportInformation.ExportInformationFileAttachmentInfo);
            }

            UpdateExportUiTeamsFiltering(export);
        }       

        internal void UpdateTargetOfExportHddDescription()
        {
            if (ExportTarget == ExportTargets.HDD && section.ExportSourceSection is InterplayPamMaterialExportSource pamMaterialExportSource)
            {
                switch (pamMaterialExportSource.Element)
                {
                    case InterplayPamElements.Tampere:
                        hddInstructionLabel.Text = hddInstructionTampere;
                        break;
                    case InterplayPamElements.Helsinki:
                        hddInstructionLabel.Text = hddInstructionHelsinki;
                        break;
                    case InterplayPamElements.Vaasa:
                        hddInstructionLabel.Text = hddInstructionVaasa;
                        break;
                    default:
                        // Do nothing
                        break;
                }
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		protected override void HandleVisibilityAndEnabledUpdate()
        {
			title.IsVisible = IsVisible;

			exportDepartmentLabel.IsVisible = IsVisible && !(section.ExportSourceSection is InterplayPamMaterialExportSource);
			exportDepartmentDropDown.IsVisible = IsVisible && exportDepartmentLabel.IsVisible;
			exportDepartmentDropDown.IsEnabled = IsEnabled;

			bool showFieldsLinkedToInterplayPamExportSource = true;
			if (section.ExportSourceSection is InterplayPamMaterialExportSource pamMaterialExportSource)
			{
				showFieldsLinkedToInterplayPamExportSource = pamMaterialExportSource.ViewTargetFormat != ViewTargetFormats.MPEG1 || pamMaterialExportSource.ExportFileType != InterplayPamExportFileTypes.ViewingVideo;

				// To avoid that the subtitle source file field can be invalid when it is invisible.
				if (pamMaterialExportSource.ViewTargetFormat == ViewTargetFormats.MPEG1) addSubsToVideoCheckbox.IsChecked = false;
			}

			yleLogoInUpperLeftCornerLabel.IsVisible = IsVisible && showFieldsLinkedToInterplayPamExportSource;
			yleLogoInUpperLeftCornerCheckbox.IsVisible = yleLogoInUpperLeftCornerLabel.IsVisible;
			yleLogoInUpperLeftCornerCheckbox.IsEnabled = IsEnabled;

			sourceTimecodeBurninToVideoLabel.IsVisible = yleLogoInUpperLeftCornerLabel.IsVisible;
			sourceTimecodeBurninToVideoCheckbox.IsVisible = yleLogoInUpperLeftCornerLabel.IsVisible;
			sourceTimecodeBurninToVideoCheckbox.IsEnabled = IsEnabled;

			addSubsToVideoLabel.IsVisible = yleLogoInUpperLeftCornerLabel.IsVisible;
			addSubsToVideoCheckbox.IsVisible = yleLogoInUpperLeftCornerLabel.IsVisible;
			addSubsToVideoCheckbox.IsEnabled = IsEnabled;

			exportInformationFileSelectorSection.IsVisible = IsVisible && AddSubtitles;
			exportInformationFileSelectorSection.IsEnabled = IsEnabled;

			exportTargetLabel.IsVisible = IsVisible;
			exportTargetDropDown.IsVisible = IsVisible;
			exportTargetDropDown.IsEnabled = IsEnabled;

			mediaParkkiTreeViewSection.IsVisible = IsVisible && ExportTarget == ExportTargets.Mediaparkki;
			mediaParkkiTreeViewSection.IsEnabled = IsEnabled;

            hddInstructionLabel.IsVisible = IsVisible && ExportTarget == ExportTargets.HDD;

			asperaFaspexReceiversEmailLabel.IsVisible = IsVisible && ExportTarget == ExportTargets.AsperaFaspex;
			asperaFaspexReceiversEmailTextBox.IsVisible = IsVisible && ExportTarget == ExportTargets.AsperaFaspex;
			asperaFaspexReceiversEmailTextBox.IsEnabled = IsEnabled;

			asperaFaspexMessageHeadlineLabel.IsVisible = IsVisible && ExportTarget == ExportTargets.AsperaFaspex;
            asperaFaspexMessageHeadlineTextBox.IsVisible = IsVisible && ExportTarget == ExportTargets.AsperaFaspex;
			asperaFaspexMessageHeadlineTextBox.IsEnabled = IsEnabled;

            asperaFaspexMessageLabel.IsVisible = IsVisible && ExportTarget == ExportTargets.AsperaFaspex;
            asperaFaspexMessageTextBox.IsVisible = IsVisible && ExportTarget == ExportTargets.AsperaFaspex;
			asperaFaspexMessageTextBox.IsEnabled = IsEnabled;

            otherExportTargetLabel.IsVisible = IsVisible && ExportTarget == ExportTargets.Other;
            otherExportTargetTextBox.IsVisible = IsVisible && otherExportTargetLabel.IsVisible;
			otherExportTargetTextBox.IsEnabled = IsEnabled;

			titleForAdditionalInfo.IsVisible = IsVisible;
			additionalInfoLabel.IsVisible = IsVisible;
			additionalInfoTextBox.IsVisible = IsVisible;
			additionalInfoTextBox.IsEnabled = IsEnabled;
			      
            ToolTipHandler.SetTooltipVisibility(this);
        }

		private void UpdateExportUiTeamsFiltering(Export export)
        {
            export.TeamMgmt = false;
            export.TeamNews = false;
            export.TeamHki = false;
            export.TeamTre = false;
            export.TeamVsa = false;

            if (section.ExportSourceSection is InterplayPamMaterialExportSource pamMaterialExportSource)
            {
                export.TeamHki = pamMaterialExportSource.Element == InterplayPamElements.Helsinki;
                export.TeamTre = pamMaterialExportSource.Element == InterplayPamElements.Tampere;
                export.TeamVsa = pamMaterialExportSource.Element == InterplayPamElements.Vaasa;
            }
            else
            {
                export.TeamHki = ExportDepartment == ExportDepartments.HELSINKI;
                export.TeamTre = ExportDepartment == ExportDepartments.TAMPERE;
                export.TeamVsa = ExportDepartment == ExportDepartments.VAASA;
            }
        }

        private void ExportdepartmentDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			ExportDepartment = EnumExtensions.GetEnumValueFromDescription<ExportDepartments>(e.Selected);
		}

		private void AddSubsToVideoCheckBox_Changed(object sender, EventArgs e)
		{
			HandleVisibilityAndEnabledUpdate();
		}

		private void ExportTargetDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				UpdateTargetOfExportHddDescription();
				InvokeRegenerateUi();
				IsValid(OrderAction.Book);
			}
        }

		public override void RegenerateUi()
		{
			exportInformationFileSelectorSection.RegenerateUi();
			mediaParkkiTreeViewSection.RegenerateUi();
			GenerateUi(out int row);
		}
	}
}
