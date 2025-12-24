namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.MediaParkki;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class FileImportDetailsSection : ImportSubSection
	{
		private SourceFileLocations selectedSourceFileLocation = SourceFileLocations.MEDIAPARKKI;

		private readonly Label sourceFileLocationLabel = new Label("Source file location");
		private readonly DropDown sourceFileLocationDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<SourceFileLocations>().OrderBy(s => s).Where(s => !s.Equals(EnumExtensions.GetDescriptionFromEnumValue(SourceFileLocations.GOOGLE_DRIVE))), EnumExtensions.GetDescriptionFromEnumValue(SourceFileLocations.MEDIAPARKKI));

		private readonly NonLiveManagerTreeViewSection mediaParkkiTreeViewSection;

		private readonly Label emailAddressOfTheSenderLabel = new Label("Email address of the sender");
		private readonly YleTextBox emailAddressOfTheSenderTextBox = new YleTextBox();

		private readonly Label customSourceFileLocationLabel = new Label("Custom source file location");
		private readonly YleTextBox customSourceFileLocationTextBox = new YleTextBox() { IsMultiline = true, Height = 100 };

		private readonly Label fileMaterialTypeLabel = new Label("Material type");
		private readonly DropDown fileMaterialTypeDropDown = new DropDown();

		private readonly Label materialIncludesHighFrameRateLabel = new Label("Material includes high frame rate");
		private readonly CheckBox materialIncludesHighFrameRateCheckBox = new CheckBox { IsChecked = false };

		private readonly Label additionalInfoAboutHighFrameRateLabel = new Label("Additional info about high frame rate footage");
		private readonly YleTextBox additionalInfoAboutHighFrameRateTextBox = new YleTextBox() { IsMultiline = true, Height = 200, PlaceHolder = "Please give the names of the clips that have special frame rate and the purpose for it (are supposed to be in slow motion or fast motion after the import)." };

		private readonly Label additionalInformationLabel = new Label("Additional Information");
		private readonly YleTextBox additionalInformationTextBox = new YleTextBox { IsMultiline = true, Height = 100, PlaceHolder = "Provide some additional information about the request." };

		public FileImportDetailsSection(Helpers helpers, ISectionConfiguration configuration, ImportMainSection section, FileIngestDetails fileIngestDetails = null) : base(helpers, configuration, section, null)
		{
			mediaParkkiTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new MediaParkkiManager(helpers), TreeViewType.FileSelector);
			mediaParkkiTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			mediaParkkiTreeViewSection.RegenerateUiRequired += HandleRegenerateUiRequired;
			mediaParkkiTreeViewSection.SelectedItemChanged += (sender, args) => IsValid();
			mediaParkkiTreeViewSection.SourceLabel.Text = "Source File";
			mediaParkkiTreeViewSection.InitRoot();

			InitializeFileMaterialTypeDropDown();
			InitializeFileIngest(fileIngestDetails);

			sourceFileLocationDropDown.Changed += SourceFileLocationDropDown_Changed;
			materialIncludesHighFrameRateCheckBox.Changed += MaterialIncludesHighFrameRateCheckBox_Changed;
			fileMaterialTypeDropDown.Changed += FileMaterialTypeDropDown_Changed;

			GenerateUi(out int row);
		}

		public SourceFileLocations SourceFileLocation
		{
			get
			{
				return selectedSourceFileLocation;
			}
			private set
			{
				selectedSourceFileLocation = value;
				sourceFileLocationDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedSourceFileLocation);
			}
		}

		public HashSet<File> SourceFiles { get => mediaParkkiTreeViewSection.SelectedFiles; }

		public string CustomSourceFileLocation
		{
			get
			{
				return customSourceFileLocationTextBox.Text;
			}
			private set
			{
				customSourceFileLocationTextBox.Text = value;
			}
		}

		public string EmailAddressOfTheSender
		{
			get
			{
				return emailAddressOfTheSenderTextBox.Text;
			}
			private set
			{
				emailAddressOfTheSenderTextBox.Text = value;
			}
		}

		public string AdditionalInfoAboutHighFrameRateFootage
		{
			get
			{
				return additionalInfoAboutHighFrameRateTextBox.Text;
			}
			private set
			{
				additionalInfoAboutHighFrameRateTextBox.Text = value;
			}
		}

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return new[] { mediaParkkiTreeViewSection };
			}
		}

		public override bool IsValid()
		{
			bool sourceFileLocationIsValid = false;
			switch (SourceFileLocation)
			{
				case SourceFileLocations.MEDIAPARKKI:
					sourceFileLocationIsValid = SourceFiles != null && SourceFiles.Any();
					break;

				case SourceFileLocations.ASPERA_FASPEX:
					sourceFileLocationIsValid = true;
					break;

				case SourceFileLocations.OTHER:
					sourceFileLocationIsValid = !String.IsNullOrWhiteSpace(CustomSourceFileLocation);
					break;
				default:
					return false;
			}

			mediaParkkiTreeViewSection.ValidationState = (SourceFileLocation == SourceFileLocations.MEDIAPARKKI && !sourceFileLocationIsValid) ? UIValidationState.Invalid : UIValidationState.Valid;
			mediaParkkiTreeViewSection.ValidationText = "Select a source file";

			customSourceFileLocationTextBox.ValidationState = (SourceFileLocation == SourceFileLocations.OTHER && !sourceFileLocationIsValid) ? UIValidationState.Invalid : UIValidationState.Valid;
			customSourceFileLocationTextBox.ValidationText = "Provide a custom source file location";

			bool highFrameRateInfoIsValid = fileMaterialTypeDropDown.Selected != EnumExtensions.GetDescriptionFromEnumValue(FileMaterialTypes.VIDEO) || !materialIncludesHighFrameRateCheckBox.IsChecked || !String.IsNullOrWhiteSpace(AdditionalInfoAboutHighFrameRateFootage);

			additionalInfoAboutHighFrameRateTextBox.ValidationState = highFrameRateInfoIsValid ? UIValidationState.Valid : UIValidationState.Invalid;
			additionalInfoAboutHighFrameRateTextBox.ValidationText = "Provide additional info";

			return sourceFileLocationIsValid && highFrameRateInfoIsValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(sourceFileLocationLabel, ++row, 0);
			AddWidget(sourceFileLocationDropDown, row, 1, 1, 2);

			AddSection(mediaParkkiTreeViewSection, new SectionLayout(++row, 0));
			row += mediaParkkiTreeViewSection.RowCount;

			AddWidget(emailAddressOfTheSenderLabel, ++row, 0);
			AddWidget(emailAddressOfTheSenderTextBox, row, 1, 1, 2);

			AddWidget(customSourceFileLocationLabel, ++row, 0);
			AddWidget(customSourceFileLocationTextBox, row, 1, 1, 2);

			AddWidget(fileMaterialTypeLabel, ++row, 0);
			AddWidget(fileMaterialTypeDropDown, row, 1, 1, 2);

			AddWidget(materialIncludesHighFrameRateLabel, ++row, 0);
			AddWidget(materialIncludesHighFrameRateCheckBox, row, 1, 1, 2);

			AddWidget(additionalInfoAboutHighFrameRateLabel, ++row, 0);
			AddWidget(additionalInfoAboutHighFrameRateTextBox, row, 1, 1, 2);

			AddWidget(additionalInformationLabel, ++row, 0);
			AddWidget(additionalInformationTextBox, row, 1, 1, 2);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			sourceFileLocationLabel.IsVisible = IsVisible;
			sourceFileLocationDropDown.IsVisible = IsVisible;
			sourceFileLocationDropDown.IsEnabled = IsEnabled;

			mediaParkkiTreeViewSection.IsVisible = IsVisible && SourceFileLocation == SourceFileLocations.MEDIAPARKKI;
			mediaParkkiTreeViewSection.IsEnabled = IsEnabled;

			emailAddressOfTheSenderLabel.IsVisible = IsVisible && SourceFileLocation == SourceFileLocations.ASPERA_FASPEX;
			emailAddressOfTheSenderTextBox.IsVisible = emailAddressOfTheSenderLabel.IsVisible;
			emailAddressOfTheSenderTextBox.IsEnabled = IsEnabled;

			customSourceFileLocationLabel.IsVisible = SourceFileLocation == SourceFileLocations.OTHER;
			customSourceFileLocationTextBox.IsVisible = customSourceFileLocationLabel.IsVisible;
			customSourceFileLocationTextBox.IsEnabled = IsEnabled;

			fileMaterialTypeLabel.IsVisible = IsVisible;
			fileMaterialTypeDropDown.IsVisible = IsVisible;
			fileMaterialTypeDropDown.IsEnabled = IsEnabled;

			materialIncludesHighFrameRateLabel.IsVisible = fileMaterialTypeDropDown.Selected == EnumExtensions.GetDescriptionFromEnumValue(FileMaterialTypes.VIDEO);
			materialIncludesHighFrameRateCheckBox.IsVisible = materialIncludesHighFrameRateLabel.IsVisible;
			materialIncludesHighFrameRateCheckBox.IsEnabled = IsEnabled;

			additionalInfoAboutHighFrameRateLabel.IsVisible = materialIncludesHighFrameRateCheckBox.IsChecked && materialIncludesHighFrameRateLabel.IsVisible;
			additionalInfoAboutHighFrameRateTextBox.IsVisible = additionalInfoAboutHighFrameRateLabel.IsVisible;
			additionalInfoAboutHighFrameRateTextBox.IsEnabled = IsEnabled;

			additionalInformationLabel.IsVisible = IsVisible;
			additionalInformationTextBox.IsVisible = IsVisible;
			additionalInformationTextBox.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}

		public override void UpdateIngest(Ingest ingest)
		{
			ingest.FileImportDetails = ingest.FileImportDetails ?? new List<FileIngestDetails>();

			ingest.FileImportDetails.Add(new FileIngestDetails
			{
				SourceFileLocation = EnumExtensions.GetDescriptionFromEnumValue(SourceFileLocation),
				SourceFolderNameUrls = (SourceFileLocation == SourceFileLocations.MEDIAPARKKI) ? mediaParkkiTreeViewSection.GetFolderUrls().ToArray() : new string[0],
				SourceFileUrls = (SourceFileLocation == SourceFileLocations.MEDIAPARKKI) ? mediaParkkiTreeViewSection.GetSelectedFileUrls() : null,
				CustomSourceFileLocation = SourceFileLocation == SourceFileLocations.OTHER ? CustomSourceFileLocation : null,
				EmailAddressOfTheSender = SourceFileLocation == SourceFileLocations.ASPERA_FASPEX ? EmailAddressOfTheSender : null,
				MaterialType = fileMaterialTypeDropDown.Selected,
				MaterialIncludesHighFrameRateFootage = fileMaterialTypeDropDown.Selected == EnumExtensions.GetDescriptionFromEnumValue(FileMaterialTypes.VIDEO) ? materialIncludesHighFrameRateCheckBox.IsChecked : false,
				AdditionalInfoAboutHighFrameRateFootage = fileMaterialTypeDropDown.Selected == EnumExtensions.GetDescriptionFromEnumValue(FileMaterialTypes.VIDEO) && materialIncludesHighFrameRateCheckBox.IsChecked ? this.AdditionalInfoAboutHighFrameRateFootage : null,
				AdditionalInformation = additionalInformationTextBox.Text
			});
		}

		internal void InitializeFileMaterialTypeDropDown()
		{
			if (ingestMainSection.IngestDestination == InterplayPamElements.Vaasa)
			{
				fileMaterialTypeDropDown.Options = EnumExtensions.GetEnumDescriptions<FileMaterialTypes>().OrderBy(x => x);
			}
			else
			{
				fileMaterialTypeDropDown.Options = EnumExtensions.GetEnumDescriptions<FileMaterialTypes>().Where(x => !x.Equals(EnumExtensions.GetDescriptionFromEnumValue(FileMaterialTypes.GRAPHICS))).OrderBy(x => x);
			}

			fileMaterialTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(FileMaterialTypes.VIDEO);
		}

		private void InitializeFileIngest(FileIngestDetails fileIngestDetails)
		{
			if (fileIngestDetails == null) return;

			SourceFileLocation = EnumExtensions.GetEnumValueFromDescription<SourceFileLocations>(fileIngestDetails.SourceFileLocation);
			if (fileIngestDetails.CustomSourceFileLocation != null) CustomSourceFileLocation = fileIngestDetails.CustomSourceFileLocation;
			if (fileIngestDetails.EmailAddressOfTheSender != null) EmailAddressOfTheSender = fileIngestDetails.EmailAddressOfTheSender;
			fileMaterialTypeDropDown.Selected = fileIngestDetails.MaterialType;
			materialIncludesHighFrameRateCheckBox.IsChecked = fileIngestDetails.MaterialIncludesHighFrameRateFootage;
			if (fileIngestDetails.AdditionalInfoAboutHighFrameRateFootage != null) AdditionalInfoAboutHighFrameRateFootage = fileIngestDetails.AdditionalInfoAboutHighFrameRateFootage;

			if (SourceFileLocation == SourceFileLocations.MEDIAPARKKI && fileIngestDetails.SourceFolderNameUrls != null && fileIngestDetails.SourceFolderNameUrls.Any())
			{
				mediaParkkiTreeViewSection.Initialize(fileIngestDetails.SourceFolderNameUrls, fileIngestDetails.SourceFileUrls);
			}

			additionalInformationTextBox.Text = String.IsNullOrEmpty(fileIngestDetails.AdditionalInformation) ? String.Empty : fileIngestDetails.AdditionalInformation;
		}

		private void SourceFileLocationDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				SourceFileLocation = EnumExtensions.GetEnumValueFromDescription<SourceFileLocations>(e.Selected);

				HandleVisibilityAndEnabledUpdate();
				IsValid();
			}
		}

		private void MaterialIncludesHighFrameRateCheckBox_Changed(object sender, CheckBox.CheckBoxChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				HandleVisibilityAndEnabledUpdate();
				IsValid();
			}
		}

		private void FileMaterialTypeDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				HandleVisibilityAndEnabledUpdate();
				IsValid();
			}
		}

		public override void RegenerateUi()
		{
			mediaParkkiTreeViewSection.RegenerateUi();
			GenerateUi(out int row);
		}
	}

}
