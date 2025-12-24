namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder.ExportFileSelectorSections;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class InterplayPamMaterialExportSource : ExportSourceSection
	{
		private InterplayPamExportFileTypes selectedFileType = InterplayPamExportFileTypes.HiresVideo;
		private HiresTargetVideoFormats selectedHiresTargetVideoFormat = HiresTargetVideoFormats.AVCI100;
		private InterplayItemTypes selectedHiresInterplayItemType = InterplayItemTypes.MASTERCLIP;
		private ViewTargetFormats selectedViewTargetFormat = ViewTargetFormats.H264;
		private VideoViewingQualities selectedVideoViewingQuality = VideoViewingQualities.HIGH;
		private AafMediaFormats selectedAafMediaFormat = AafMediaFormats.EMBEDDED_MEDIA;

		private readonly Label title = new Label("Interplay PAM") { Style = TextStyle.Bold };

		private readonly Label elementLabel = new Label("Which Interplay PAM?");
		private readonly DropDown elementDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<InterplayPamElements>().Where(x => x != EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.UA)), EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.Helsinki));

		private readonly Dictionary<InterplayPamElements, NonLiveManagerTreeViewSection> treeViewSections = new Dictionary<InterplayPamElements, NonLiveManagerTreeViewSection>();

		private readonly Label hiresInterplayItemTypeLabel = new Label("Type of Interplay Item");
		private readonly DropDown hiresInterplayItemTypeDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<InterplayItemTypes>(), EnumExtensions.GetDescriptionFromEnumValue(InterplayItemTypes.NONE));

		private readonly Label hiresOtherVideoFormatAdditionalTextLabel = new Label("Other video format");
		private readonly YleTextBox hiresOtherVideoFormatAdditionalTextTextBox = new YleTextBox { IsMultiline = true, Height = 50 };

		private readonly Label informationLabel = new Label("Additional source file information");
		private readonly YleTextBox informationTextBox = new YleTextBox { IsMultiline = true, Height = 100 };

		private readonly Label fileTypeLabel = new Label("Export file type");
		private readonly DropDown fileTypeDropDown = new DropDown();

        private readonly Label vaasaItemTypeLabel = new Label("Item Type");
        private readonly DropDown vaasaItemTypeDropDown = new DropDown { Options = EnumExtensions.GetEnumDescriptions<InterplayPamVaasaItemTypes>(), Selected = EnumExtensions.GetDescriptionFromEnumValue(InterplayPamVaasaItemTypes.Program) };

        private readonly Label vaasaProgramIdLabel = new Label("Program id");
        private readonly YleTextBox vaasaProgramIdTextbox = new YleTextBox();

        private readonly Label vaasaClipNameLabel = new Label("Clip name");
        private readonly YleTextBox vaasaClipNameTextbox = new YleTextBox();

        private readonly Label vaasaProductionNumberLabel = new Label("Production number");
        private readonly YleTextBox vaasaProductionNumberTextbox = new YleTextBox();

        private readonly Label hiresTargetVideoFormatLabel = new Label("Target video format");
		private readonly DropDown hiresTargetVideoFormatDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<HiresTargetVideoFormats>(), EnumExtensions.GetDescriptionFromEnumValue(HiresTargetVideoFormats.AVCI100));

        private readonly Label hiresExportSpecificationsFileLabel = new Label("Export specifications file");
        private readonly YleTextBox hiresExportSpecificationsFileTextBox = new YleTextBox { IsMultiline = true, Height = 200, PlaceHolder = "Please give specifications for the exported file: video and audio format, framerate, bitrate, etc." };

        private readonly Label viewingTargetFormatLabel = new Label("Target viewing format");
		private readonly DropDown viewingTargetFormatDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<ViewTargetFormats>(), EnumExtensions.GetDescriptionFromEnumValue(ViewTargetFormats.H264));

		private readonly Label viewingOtherTargetFormatLabel = new Label("Other target viewing format");
		private readonly YleTextBox viewingOtherTargetFormatTextBox = new YleTextBox { IsMultiline = true, Height = 50 };

		private readonly Label viewingVideoQualityLabel = new Label("Video quality");
		private readonly DropDown viewingVideoQualityDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<VideoViewingQualities>(), EnumExtensions.GetDescriptionFromEnumValue(VideoViewingQualities.HIGH));

		private readonly Label viewingExportSpecificationsFileLabel = new Label("Export specifications file");
		private readonly YleTextBox viewingExportSpecificationsFileTextBox = new YleTextBox { IsMultiline = true, Height = 200, PlaceHolder = "Please give specifications for the exported file: video and audio format, framerate, bitrate, etc." };

		private readonly Label aafExportContainsMediaLabel = new Label("Contain media in Export");
		private readonly CheckBox aafExportContainsMediaCheckBox = new CheckBox { IsChecked = false };

		private readonly Label aafMediaFormatLabel = new Label("Media format");
		private readonly DropDown aafMediaformatDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<AafMediaFormats>(), EnumExtensions.GetDescriptionFromEnumValue(AafMediaFormats.EMBEDDED_MEDIA));

        private readonly DefaultFileSelectorSection otherHiresFileSelectorSection;
        private readonly DefaultFileSelectorSection otherViewingVideoFileSelectorSection;

        public InterplayPamMaterialExportSource(Helpers helpers, ISectionConfiguration configuration, ExportMainSection section, Export export) : base(helpers, configuration, section, export)
		{
			var helsinkiTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Helsinki), TreeViewType.FileSelector);
			helsinkiTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			helsinkiTreeViewSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			var tampereTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Tampere), TreeViewType.FileSelector);
			tampereTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			tampereTreeViewSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			var vaasaTreeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new AvidInterplayPamManager(helpers, InterplayPamElements.Vaasa), TreeViewType.FileSelector);
			vaasaTreeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			vaasaTreeViewSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			helsinkiTreeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);
			tampereTreeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);
			vaasaTreeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);

			helsinkiTreeViewSection.SourceLabel.Text = "Interplay source file(s)";
			tampereTreeViewSection.SourceLabel.Text = "Interplay source file(s)";
			vaasaTreeViewSection.SourceLabel.Text = "Interplay source file(s)";

			helsinkiTreeViewSection.InitRoot();
			tampereTreeViewSection.InitRoot();
			vaasaTreeViewSection.InitRoot();

			treeViewSections.Add(InterplayPamElements.Helsinki, helsinkiTreeViewSection);
			treeViewSections.Add(InterplayPamElements.Tampere, tampereTreeViewSection);
			treeViewSections.Add(InterplayPamElements.Vaasa, vaasaTreeViewSection);

            otherHiresFileSelectorSection = new DefaultFileSelectorSection(helpers, configuration, export);
			otherHiresFileSelectorSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			otherHiresFileSelectorSection.RegenerateUiRequired += HandleRegenerateUiRequired;

            otherViewingVideoFileSelectorSection = new DefaultFileSelectorSection(helpers, configuration, export);
			otherViewingVideoFileSelectorSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			otherViewingVideoFileSelectorSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			elementDropDown.Changed += ElementDropdown_Changed;
			fileTypeDropDown.Changed += FileTypeDropdown_Changed;
            vaasaItemTypeDropDown.Changed += VaasaItemTypeDropDown_Changed;

            hiresTargetVideoFormatDropDown.Changed += HiresTargetVideoFormatDropDown_Changed;
			hiresInterplayItemTypeDropDown.Changed += HiresInterplayItemTypeDropDown_Changed;
			viewingTargetFormatDropDown.Changed += ViewingTargetFormatDropDown_Changed;
			aafExportContainsMediaCheckBox.Changed += AafExportContainsMediaCheckBox_Changed;
			aafMediaformatDropDown.Changed += AafMediaformatDropDown_Changed;

            InitializeExport(export);

			GenerateUi(out int row);
		}

        public InterplayPamElements Element { get => EnumExtensions.GetEnumValueFromDescription<InterplayPamElements>(elementDropDown.Selected); }

		public Folder Folder { get => treeViewSections[Element].SelectedFolder; }

		public HashSet<File> Files { get => treeViewSections[Element].SelectedFiles; }

		public string CurrentSelectedExportFileType { get => fileTypeDropDown.Selected; }

        public InterplayPamExportFileTypes ExportFileType
		{
			get
			{
				return selectedFileType;
			}
			private set
			{
				selectedFileType = value;
				fileTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedFileType);
			}
		}

		public HiresTargetVideoFormats HiresTargetVideoFormat
		{
			get
			{
				return selectedHiresTargetVideoFormat;
			}
			private set
			{
				selectedHiresTargetVideoFormat = value;
				hiresTargetVideoFormatDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedHiresTargetVideoFormat);
			}
		}

		public InterplayItemTypes InterplayItemType
		{
			get
			{
				return selectedHiresInterplayItemType;
			}
			private set
			{
				selectedHiresInterplayItemType = value;
				hiresInterplayItemTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedHiresInterplayItemType);
			}
		}

		public string HiresOtherVideoFormatAdditionalText
		{
			get
			{
				return hiresOtherVideoFormatAdditionalTextTextBox.Text;
			}
			private set
			{
				hiresOtherVideoFormatAdditionalTextTextBox.Text = value;
			}
		}

		public ViewTargetFormats ViewTargetFormat
		{
			get
			{
				return selectedViewTargetFormat;
			}
			private set
			{
				selectedViewTargetFormat = value;
				viewingTargetFormatDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedViewTargetFormat);
			}
		}

		public string ViewingOtherTargetFormat
		{
			get
			{
				return viewingOtherTargetFormatTextBox.Text;
			}
			private set
			{
				viewingOtherTargetFormatTextBox.Text = value;
			}
		}

		public VideoViewingQualities VideoViewingQuality
		{
			get
			{
				return selectedVideoViewingQuality;
			}
			private set
			{
				selectedVideoViewingQuality = value;
				viewingVideoQualityDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedVideoViewingQuality);
			}
		}

		public string ViewingExportSpecificationsFile
		{
			get
			{
				return viewingExportSpecificationsFileTextBox.Text;
			}
			private set
			{
				viewingExportSpecificationsFileTextBox.Text = value;
			}
		}

		public bool AafExportContainsMedia
		{
			get
			{
				return aafExportContainsMediaCheckBox.IsChecked;
			}
			private set
			{
				aafExportContainsMediaCheckBox.IsChecked = value;
			}
		}

		public AafMediaFormats AafMediaFormats
		{
			get
			{
				return selectedAafMediaFormat;
			}
			private set
			{
				selectedAafMediaFormat = value;
				aafMediaformatDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedAafMediaFormat);
			}
		}

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return treeViewSections.Select(x => x.Value);
			}
		}

        public override bool IsValid(OrderAction action)
		{
			if (action == OrderAction.Save) return true;

			bool isFileValid = Files != null && Files.Any();

			treeViewSections[Element].ValidationState = isFileValid ? UIValidationState.Valid : UIValidationState.Invalid;
			treeViewSections[Element].ValidationText = "Select a file";

			bool isHiresOtherVideoValid = ExportFileType != InterplayPamExportFileTypes.HiresVideo || HiresTargetVideoFormat != HiresTargetVideoFormats.OTHER || !String.IsNullOrEmpty(HiresOtherVideoFormatAdditionalText);
			hiresOtherVideoFormatAdditionalTextTextBox.ValidationState = isHiresOtherVideoValid ? UIValidationState.Valid : UIValidationState.Invalid;
			hiresOtherVideoFormatAdditionalTextTextBox.ValidationText = "Provide an other video format";

            hiresExportSpecificationsFileTextBox.ValidationState = isHiresOtherVideoValid ? UIValidationState.Valid : UIValidationState.Invalid;
            hiresExportSpecificationsFileTextBox.ValidationText = "Provide some specifications";

            viewingExportSpecificationsFileTextBox.ValidationState = isHiresOtherVideoValid ? UIValidationState.Valid : UIValidationState.Invalid;
            viewingExportSpecificationsFileTextBox.ValidationText = "Provide some specifications";

            bool isViewingVideoOtherTargetValid = ExportFileType != InterplayPamExportFileTypes.ViewingVideo || ViewTargetFormat != ViewTargetFormats.OTHER || !String.IsNullOrEmpty(ViewingOtherTargetFormat);
			viewingOtherTargetFormatTextBox.ValidationState = isViewingVideoOtherTargetValid ? UIValidationState.Valid : UIValidationState.Invalid;
			viewingOtherTargetFormatTextBox.ValidationText = "Provide an other viewing format";

            bool isProgramIdValid = Element != InterplayPamElements.Vaasa || !vaasaProgramIdTextbox.IsVisible || vaasaItemTypeDropDown.Selected == EnumExtensions.GetDescriptionFromEnumValue(InterplayPamVaasaItemTypes.Clip) || !string.IsNullOrWhiteSpace(vaasaProgramIdTextbox.Text);
            vaasaProgramIdTextbox.ValidationState = isProgramIdValid ? UIValidationState.Valid : UIValidationState.Invalid;
            vaasaProgramIdTextbox.ValidationText = "Please provide a program Id";

            bool isClipNameValid = Element != InterplayPamElements.Vaasa || !vaasaClipNameTextbox.IsVisible || vaasaItemTypeDropDown.Selected == EnumExtensions.GetDescriptionFromEnumValue(InterplayPamVaasaItemTypes.Program) || !string.IsNullOrWhiteSpace(vaasaClipNameTextbox.Text);
            vaasaClipNameTextbox.ValidationState = isClipNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
            vaasaClipNameTextbox.ValidationText = "Please provide a clip name";

            return isFileValid
				&& isHiresOtherVideoValid
                && isProgramIdValid
                && isClipNameValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(title, new WidgetLayout(++row, 0, 1, 2));

			AddWidget(elementLabel, ++row, 0);
			AddWidget(elementDropDown, row, 1, 1, 2);

			AddSection(treeViewSections[Element], new SectionLayout(++row, 0));
			row += treeViewSections[Element].RowCount;

			AddWidget(hiresInterplayItemTypeLabel, ++row, 0);
			AddWidget(hiresInterplayItemTypeDropDown, row, 1, 1, 2);

			AddWidget(informationLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(informationTextBox, row, 1, 1, 2);

			AddWidget(fileTypeLabel, ++row, 0);
			AddWidget(fileTypeDropDown, row, 1, 1, 2);

			AddWidget(hiresTargetVideoFormatLabel, ++row, 0);
			AddWidget(hiresTargetVideoFormatDropDown, row, 1, 1, 2);

			AddWidget(hiresOtherVideoFormatAdditionalTextLabel, ++row, 0);
			AddWidget(hiresOtherVideoFormatAdditionalTextTextBox, row, 1, 1, 2);

            AddWidget(hiresExportSpecificationsFileLabel, ++row, 0);
            AddWidget(hiresExportSpecificationsFileTextBox, row, 1, 1, 2);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddSection(otherHiresFileSelectorSection, new SectionLayout(++row, 0));
            row += otherHiresFileSelectorSection.RowCount;

            AddWidget(new WhiteSpace(), ++row, 0);

            AddWidget(viewingTargetFormatLabel, ++row, 0);
			AddWidget(viewingTargetFormatDropDown, row, 1, 1, 2);

			AddWidget(viewingOtherTargetFormatLabel, ++row, 0);
			AddWidget(viewingOtherTargetFormatTextBox, row, 1, 1, 2);

			AddWidget(viewingVideoQualityLabel, ++row, 0);
			AddWidget(viewingVideoQualityDropDown, row, 1, 1, 2);

			AddWidget(viewingExportSpecificationsFileLabel, ++row, 0);
			AddWidget(viewingExportSpecificationsFileTextBox, row, 1, 1, 2);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddSection(otherViewingVideoFileSelectorSection, new SectionLayout(++row, 0));
            row += otherViewingVideoFileSelectorSection.RowCount;

            AddWidget(aafExportContainsMediaLabel, ++row, 0);
			AddWidget(aafExportContainsMediaCheckBox, row, 1, 1, 2);

			AddWidget(aafMediaFormatLabel, ++row, 0);
			AddWidget(aafMediaformatDropDown, row, 1, 1, 2);

            AddWidget(vaasaItemTypeLabel, ++row, 0);
			AddWidget(vaasaItemTypeDropDown, row, 1, 1, 2);

            AddWidget(vaasaProgramIdLabel, ++row, 0);
            AddWidget(vaasaProgramIdTextbox, row, 1, 1, 2);

            AddWidget(vaasaClipNameLabel, ++row, 0);
            AddWidget(vaasaClipNameTextbox, row, 1, 1, 2);

            AddWidget(vaasaProductionNumberLabel, ++row, 0);
            AddWidget(vaasaProductionNumberTextbox, row, 1, 1, 2);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

        public override void InitializeExport(Export export)
        {
            ConfigureExportFileTypeDropDown();

            if (export == null || export.InterplayPamExport == null) return;

            informationTextBox.Text = export.InterplayPamExport.Information;
            if (export.InterplayPamExport.ExportFileType.HasValue) ExportFileType = export.InterplayPamExport.ExportFileType.Value;
            if (export.InterplayPamExport.TargetVideoFormat.HasValue) HiresTargetVideoFormat = export.InterplayPamExport.TargetVideoFormat.Value;
            if (export.InterplayPamExport.InterplayItemType.HasValue) InterplayItemType = export.InterplayPamExport.InterplayItemType.Value;
            HiresOtherVideoFormatAdditionalText = export.InterplayPamExport.HiresOtherVideoFormatAdditionalText;
            hiresExportSpecificationsFileTextBox.Text = export.InterplayPamExport.HiresExportSpecificationsFile;
            if (export.InterplayPamExport.ViewTargetFormat.HasValue) ViewTargetFormat = export.InterplayPamExport.ViewTargetFormat.Value;
            ViewingOtherTargetFormat = export.InterplayPamExport.ViewingExportSpecifications;
            if (export.InterplayPamExport.VideoViewingQuality.HasValue) VideoViewingQuality = export.InterplayPamExport.VideoViewingQuality.Value;
            ViewingExportSpecificationsFile = export.InterplayPamExport.ViewingExportSpecificationsFile;
            if (export.InterplayPamExport.AafExportContainsMedia.HasValue) AafExportContainsMedia = export.InterplayPamExport.AafExportContainsMedia.Value;
            if (export.InterplayPamExport.AafMediaFormats.HasValue) AafMediaFormats = export.InterplayPamExport.AafMediaFormats.Value;
            vaasaItemTypeDropDown.Selected = export.InterplayPamExport.VaasaItemType;
            vaasaProgramIdTextbox.Text = export.InterplayPamExport.VaasaProgramId;
            vaasaClipNameTextbox.Text = export.InterplayPamExport.VaasaClipName;
            vaasaProductionNumberTextbox.Text = export.InterplayPamExport.VaasaProductionNumber;

            elementDropDown.Selected = export.InterplayPamExport.ElementName;

			if (export.InterplayPamExport?.FolderUrls != null && export.InterplayPamExport.FolderUrls.Any())
            {
				treeViewSections[Element].Initialize(export.InterplayPamExport.FolderUrls, export.InterplayPamExport.FileUrls);
			}

			InitializeFileAttachmentFields();
        }

        public override void UpdateExport(Export export)
        {
            export.InterplayPamExport = export.InterplayPamExport ?? new InterplayPamExport();

            export.InterplayPamExport.ElementName = EnumExtensions.GetDescriptionFromEnumValue(Element);
            export.InterplayPamExport.FolderUrls = treeViewSections[Element].GetFolderUrls().ToArray();
            export.InterplayPamExport.FileUrls = treeViewSections[Element].GetSelectedFileUrls();
            export.InterplayPamExport.Information = informationTextBox.Text;
            export.InterplayPamExport.ExportFileType = ExportFileType;
            export.InterplayPamExport.InterplayItemType = InterplayItemType;

            if (Element == InterplayPamElements.Vaasa)
            {
                export.InterplayPamExport.VaasaItemType = vaasaItemTypeDropDown.Selected;
                export.InterplayPamExport.VaasaClipName = vaasaClipNameTextbox.Text;
                export.InterplayPamExport.VaasaProgramId = vaasaProgramIdTextbox.Text;
                export.InterplayPamExport.VaasaProductionNumber = vaasaProductionNumberTextbox.Text;
            }

            UpdateExportBasedOnExportFileType(export);
        }

        private void InitializeFileAttachmentFields()
        {
            otherHiresFileSelectorSection.Initialize(export.InterplayPamExport.OtherHiresFileAttachmentInfo);

            otherViewingVideoFileSelectorSection.Initialize(export.InterplayPamExport.OtherViewingVideoFileAttachmentInfo);
        }

        private void UpdateExportBasedOnExportFileType(Export export)
        {
            if (ExportFileType == InterplayPamExportFileTypes.HiresVideo)
            {
                export.InterplayPamExport.TargetVideoFormat = HiresTargetVideoFormat;
                export.InterplayPamExport.HiresOtherVideoFormatAdditionalText = HiresTargetVideoFormat == HiresTargetVideoFormats.OTHER ? HiresOtherVideoFormatAdditionalText : null;
                export.InterplayPamExport.HiresExportSpecificationsFile = HiresTargetVideoFormat == HiresTargetVideoFormats.OTHER ? hiresExportSpecificationsFileTextBox.Text : null;

                otherHiresFileSelectorSection.UpdateFileAttachmentInfo(export.InterplayPamExport.OtherHiresFileAttachmentInfo);
            }
            else if (ExportFileType == InterplayPamExportFileTypes.ViewingVideo)
            {
                export.InterplayPamExport.ViewTargetFormat = ViewTargetFormat;
                export.InterplayPamExport.ViewingExportSpecificationsFile = ViewTargetFormat != ViewTargetFormats.MPEG1 ? ViewingExportSpecificationsFile : null;
                export.InterplayPamExport.ViewingExportSpecifications = ViewTargetFormat == ViewTargetFormats.OTHER ? ViewingOtherTargetFormat : null;
                if (ViewTargetFormat == ViewTargetFormats.H264) export.InterplayPamExport.VideoViewingQuality = VideoViewingQuality;

                otherViewingVideoFileSelectorSection.UpdateFileAttachmentInfo(export.InterplayPamExport.OtherViewingVideoFileAttachmentInfo);
            }
            else
            {
                export.InterplayPamExport.AafExportContainsMedia = AafExportContainsMedia;
                if (AafExportContainsMedia) export.InterplayPamExport.AafMediaFormats = AafMediaFormats;
            }
        }

        private void ConfigureExportFileTypeDropDown()
        {
            var interplayPAMExportFileTypeDescriptions = EnumExtensions.GetEnumDescriptions<InterplayPamExportFileTypes>();
            fileTypeDropDown.Options = Element != InterplayPamElements.Vaasa ? interplayPAMExportFileTypeDescriptions.Where(x => x != EnumExtensions.GetDescriptionFromEnumValue(InterplayPamExportFileTypes.MetroTransfer)).OrderBy(x => x) : interplayPAMExportFileTypeDescriptions.OrderBy(x => x);
            fileTypeDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(InterplayPamExportFileTypes.HiresVideo);
        }

        private void ElementDropdown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				ConfigureExportFileTypeDropDown();
				if (section.ExportInformationSection != null) section.ExportInformationSection.UpdateTargetOfExportHddDescription();

				InvokeRegenerateUi();
				IsValid(OrderAction.Book);
			}
        }

		private void FileTypeDropdown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				selectedFileType = EnumExtensions.GetEnumValueFromDescription<InterplayPamExportFileTypes>(e.Selected);

				HandleVisibilityAndEnabledUpdate();
			}
        }

		private void HiresTargetVideoFormatDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				HiresTargetVideoFormat = EnumExtensions.GetEnumValueFromDescription<HiresTargetVideoFormats>(e.Selected);
				HandleVisibilityAndEnabledUpdate();
				IsValid(OrderAction.Book);
			}
		}

		private void HiresInterplayItemTypeDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				InterplayItemType = EnumExtensions.GetEnumValueFromDescription<InterplayItemTypes>(e.Selected);
				HandleVisibilityAndEnabledUpdate();
			}
		}

		private void ViewingTargetFormatDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				ViewTargetFormat = EnumExtensions.GetEnumValueFromDescription<ViewTargetFormats>(e.Selected);
				HandleVisibilityAndEnabledUpdate();
				IsValid(OrderAction.Book);
			}
		}

		private void AafExportContainsMediaCheckBox_Changed(object sender, CheckBox.CheckBoxChangedEventArgs e)
		{
			HandleVisibilityAndEnabledUpdate();
		}

		private void AafMediaformatDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (!String.IsNullOrEmpty(e.Selected))
				{
					AafMediaFormats = EnumExtensions.GetEnumValueFromDescription<AafMediaFormats>(e.Selected);
					HandleVisibilityAndEnabledUpdate();
				}
			}
		}

        private void VaasaItemTypeDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
        {
			using (UiDisabler.StartNew(this))
			{
				HandleVisibilityAndEnabledUpdate();
				IsValid(OrderAction.Book);
			}
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		protected override void HandleVisibilityAndEnabledUpdate()
		{
			title.IsVisible = IsVisible;

			elementLabel.IsVisible = IsVisible;
			elementDropDown.IsVisible = IsVisible;
			elementDropDown.IsEnabled = IsEnabled;

			treeViewSections.Values.ForEach(section =>
			{
				section.IsVisible = IsVisible;
				section.IsEnabled = IsEnabled;
			});

			hiresInterplayItemTypeLabel.IsVisible = IsVisible;
			hiresInterplayItemTypeDropDown.IsVisible = IsVisible;
			hiresInterplayItemTypeDropDown.IsEnabled = IsEnabled;

			informationLabel.IsVisible = IsVisible;
			informationTextBox.IsVisible = IsVisible;
			informationTextBox.IsEnabled = IsEnabled;

			fileTypeLabel.IsVisible = IsVisible;
			fileTypeDropDown.IsVisible = IsVisible;
			fileTypeDropDown.IsEnabled = IsEnabled;

			hiresTargetVideoFormatLabel.IsVisible = IsVisible && selectedFileType == InterplayPamExportFileTypes.HiresVideo;
			hiresTargetVideoFormatDropDown.IsVisible = hiresTargetVideoFormatLabel.IsVisible;
			hiresTargetVideoFormatDropDown.IsEnabled = IsEnabled;

			hiresOtherVideoFormatAdditionalTextLabel.IsVisible = IsVisible && selectedFileType == InterplayPamExportFileTypes.HiresVideo && HiresTargetVideoFormat == HiresTargetVideoFormats.OTHER;
			hiresOtherVideoFormatAdditionalTextTextBox.IsVisible = hiresOtherVideoFormatAdditionalTextLabel.IsVisible;
			hiresOtherVideoFormatAdditionalTextTextBox.IsEnabled = IsEnabled;

			hiresExportSpecificationsFileLabel.IsVisible = IsVisible && selectedFileType == InterplayPamExportFileTypes.HiresVideo && HiresTargetVideoFormat == HiresTargetVideoFormats.OTHER;
			hiresExportSpecificationsFileTextBox.IsVisible = hiresExportSpecificationsFileLabel.IsVisible;
			hiresExportSpecificationsFileTextBox.IsEnabled = IsEnabled;

			bool otherHiresFileSelectorSectionShouldBeVisible = ExportFileType == InterplayPamExportFileTypes.HiresVideo && HiresTargetVideoFormat == HiresTargetVideoFormats.OTHER;

			otherHiresFileSelectorSection.IsVisible = IsVisible && otherHiresFileSelectorSectionShouldBeVisible;
			otherHiresFileSelectorSection.IsEnabled = IsEnabled;

			viewingTargetFormatLabel.IsVisible = IsVisible && selectedFileType == InterplayPamExportFileTypes.ViewingVideo;
			viewingTargetFormatDropDown.IsVisible = viewingTargetFormatLabel.IsVisible;
			viewingTargetFormatDropDown.IsEnabled = IsEnabled;

			viewingOtherTargetFormatLabel.IsVisible = IsVisible && selectedFileType == InterplayPamExportFileTypes.ViewingVideo && ViewTargetFormat == ViewTargetFormats.OTHER;
			viewingOtherTargetFormatTextBox.IsVisible = viewingOtherTargetFormatLabel.IsVisible;
			viewingOtherTargetFormatTextBox.IsEnabled = IsEnabled;

			viewingVideoQualityLabel.IsVisible = IsVisible && selectedFileType == InterplayPamExportFileTypes.ViewingVideo && ViewTargetFormat == ViewTargetFormats.H264;
			viewingVideoQualityDropDown.IsVisible = viewingVideoQualityLabel.IsVisible;
			viewingVideoQualityDropDown.IsEnabled = IsEnabled;

			viewingExportSpecificationsFileLabel.IsVisible = IsVisible && selectedFileType == InterplayPamExportFileTypes.ViewingVideo && ViewTargetFormat != ViewTargetFormats.MPEG1;
			viewingExportSpecificationsFileTextBox.IsVisible = viewingExportSpecificationsFileLabel.IsVisible;
			viewingExportSpecificationsFileTextBox.IsEnabled = IsEnabled;

			bool otherViewingVideoFileSelectorSectionShouldBeVisible = ExportFileType == InterplayPamExportFileTypes.ViewingVideo && ViewTargetFormat == ViewTargetFormats.H264;

			otherViewingVideoFileSelectorSection.IsVisible = IsVisible && otherViewingVideoFileSelectorSectionShouldBeVisible;
			otherViewingVideoFileSelectorSection.IsEnabled = IsEnabled;

			aafExportContainsMediaLabel.IsVisible = IsVisible && selectedFileType == InterplayPamExportFileTypes.AFFSequence;
			aafExportContainsMediaCheckBox.IsVisible = aafExportContainsMediaLabel.IsVisible;
			aafExportContainsMediaCheckBox.IsEnabled = IsEnabled;

			aafMediaFormatLabel.IsVisible = IsVisible && aafExportContainsMediaLabel.IsVisible && aafExportContainsMediaCheckBox.IsChecked;
			aafMediaformatDropDown.IsVisible = aafMediaFormatLabel.IsVisible;
			aafMediaformatDropDown.IsEnabled = IsEnabled;

			vaasaItemTypeLabel.IsVisible = Element == InterplayPamElements.Vaasa && ExportFileType == InterplayPamExportFileTypes.MetroTransfer;
			vaasaItemTypeDropDown.IsVisible = vaasaItemTypeLabel.IsVisible;

			vaasaProgramIdLabel.IsVisible = Element == InterplayPamElements.Vaasa && ExportFileType == InterplayPamExportFileTypes.MetroTransfer;
			vaasaProgramIdTextbox.IsVisible = vaasaProgramIdLabel.IsVisible;

			vaasaClipNameLabel.IsVisible = Element == InterplayPamElements.Vaasa && ExportFileType == InterplayPamExportFileTypes.MetroTransfer && vaasaItemTypeDropDown.Selected == EnumExtensions.GetDescriptionFromEnumValue(InterplayPamVaasaItemTypes.Clip);
			vaasaClipNameTextbox.IsVisible = vaasaClipNameLabel.IsVisible;

			vaasaProductionNumberLabel.IsVisible = vaasaClipNameTextbox.IsVisible;
			vaasaProductionNumberTextbox.IsVisible = vaasaProductionNumberLabel.IsVisible;

            ToolTipHandler.SetTooltipVisibility(this);
        }

		public override void RegenerateUi()
		{
			otherHiresFileSelectorSection.RegenerateUi();
			otherViewingVideoFileSelectorSection.RegenerateUi();
			GenerateUi(out int row);
		}
	}
}
