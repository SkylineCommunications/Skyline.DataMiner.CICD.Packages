namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder.ExportFileSelectorSections;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class MamMetroMaterialExportSource : ExportSourceSection
	{
		private readonly Label title = new Label("MAM (Metro)") { Style = TextStyle.Bold };

		private readonly Label programNameLabel = new Label("Program name");
		private readonly YleTextBox programNameTextBox = new YleTextBox();

		private readonly Label programIdLabel = new Label("Program ID");
		private readonly YleTextBox programIdTextBox = new YleTextBox();

		private readonly Label exportFileTypeLabel = new Label("Export file type");
		private readonly DropDown exportFileTypeDropdown = new DropDown(EnumExtensions.GetEnumDescriptions<MamExportFileTypes>().OrderBy(x => x), EnumExtensions.GetDescriptionFromEnumValue(MamExportFileTypes.VIEWING_VIDEO));

		private readonly Label viewingVideoTargetFormatLabel = new Label("Target format");
		private readonly DropDown viewingvideoTargetFormatDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<ViewTargetFormats>().OrderBy(x => x), EnumExtensions.GetDescriptionFromEnumValue(ViewTargetFormats.H264));

		private readonly Label exportFileTypeFileSpecificationsLabel = new Label("Specifications for file");
		private readonly YleTextBox exportFileTypeFileSpecificationsTextBox = new YleTextBox { IsMultiline = true, Height = 200, PlaceHolder = "Please give specifications for the exported file: video and audio format, framerate, bitrate, etc." };

        private readonly DefaultFileSelectorSection mamMetroOtherTargetVideoFormatFileSelectorSection;

        private MamExportFileTypes selectedExportFileType = MamExportFileTypes.VIEWING_VIDEO;
		private ViewTargetFormats selectedViewingVideoTargetFormat = ViewTargetFormats.H264;

		public MamMetroMaterialExportSource(Helpers helpers, ISectionConfiguration configuration, ExportMainSection section, Export export) : base(helpers, configuration, section, export)
		{
            mamMetroOtherTargetVideoFormatFileSelectorSection = new DefaultFileSelectorSection(helpers, configuration, export);
			mamMetroOtherTargetVideoFormatFileSelectorSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			mamMetroOtherTargetVideoFormatFileSelectorSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			exportFileTypeDropdown.Changed += ExportFileTypeDropdown_Changed;
			viewingvideoTargetFormatDropDown.Changed += ViewingvideoTargetFormatDropDown_Changed;

			InitializeExport(export);

			GenerateUi(out int row);
		}

		public string ProgramName
		{
			get
			{
				return programNameTextBox.Text;
			}
			private set
			{
				programNameTextBox.Text = value;
			}
		}

		public string ProgramId
		{
			get
			{
				return programIdTextBox.Text;
			}
			private set
			{
				programIdTextBox.Text = value;
			}
		}

		public MamExportFileTypes ExportFileTypes
		{
			get
			{
				return selectedExportFileType;
			}
			private set
			{
				selectedExportFileType = value;
				exportFileTypeDropdown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedExportFileType);
			}
		}

		public ViewTargetFormats ViewingVideoTargetFormat
		{
			get
			{
				return selectedViewingVideoTargetFormat;
			}
			private set
			{
				selectedViewingVideoTargetFormat = value;
				viewingvideoTargetFormatDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedViewingVideoTargetFormat);
			}
		}

		public string ExportFileTypeFileSpecifications
		{
			get
			{
				return exportFileTypeFileSpecificationsTextBox.Text;
			}
			private set
			{
				exportFileTypeFileSpecificationsTextBox.Text = value;
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
			if (action == OrderAction.Save) return true;

			bool isProgramNameValid = !String.IsNullOrWhiteSpace(ProgramName);
			programNameTextBox.ValidationState = isProgramNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
			programNameTextBox.ValidationText = "Provide a program name";

			bool isProgramIdValid = !String.IsNullOrWhiteSpace(ProgramId);
			programIdTextBox.ValidationState = isProgramIdValid ? UIValidationState.Valid : UIValidationState.Invalid;
			programIdTextBox.ValidationText = "Provide a program Id";

			bool needToCheckFileSpecificationsValidation = (ExportFileTypes == MamExportFileTypes.VIEWING_VIDEO && ViewingVideoTargetFormat == ViewTargetFormats.OTHER) || ExportFileTypes == MamExportFileTypes.OTHER;
			bool areFileSpecificationsValid = !needToCheckFileSpecificationsValidation || !String.IsNullOrEmpty(ExportFileTypeFileSpecifications);
			exportFileTypeFileSpecificationsTextBox.ValidationState = areFileSpecificationsValid ? UIValidationState.Valid : UIValidationState.Invalid;
            exportFileTypeFileSpecificationsTextBox.ValidationText = "Provide some file specifications";

            return isProgramNameValid && isProgramIdValid && areFileSpecificationsValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(title, ++row, 0);

			AddWidget(programNameLabel, ++row, 0);
			AddWidget(programNameTextBox, row, 1, 1, 2);

			AddWidget(programIdLabel, ++row, 0);
			AddWidget(programIdTextBox, row, 1, 1, 2);

			AddWidget(exportFileTypeLabel, ++row, 0);
			AddWidget(exportFileTypeDropdown, row, 1, 1, 2);

			AddWidget(viewingVideoTargetFormatLabel, ++row, 0);
			AddWidget(viewingvideoTargetFormatDropDown, row, 1, 1, 2);

            AddWidget(exportFileTypeFileSpecificationsLabel, ++row, 0);
            AddWidget(exportFileTypeFileSpecificationsTextBox, row, 1, 1, 2);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddSection(mamMetroOtherTargetVideoFormatFileSelectorSection, new SectionLayout(++row, 0));
            row += mamMetroOtherTargetVideoFormatFileSelectorSection.RowCount;

            AddWidget(new WhiteSpace(), row + 1, 0);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

        public override void UpdateExport(Export export)
        {
            export.MamExport = export.MamExport ?? new MamExport();

            export.MamExport.ProgramName = ProgramName;
            export.MamExport.ProgramId = ProgramId;
            export.MamExport.ExportFileTypes = this.ExportFileTypes;
            export.MamExport.ExportFileTypeFileSpecifications = ExportFileTypeFileSpecifications;

            if (ExportFileTypes == MamExportFileTypes.VIEWING_VIDEO)
            {
                export.MamExport.ViewingVideoTargetFormat = ViewingVideoTargetFormat;
                mamMetroOtherTargetVideoFormatFileSelectorSection.UpdateFileAttachmentInfo(export.MamExport.OtherTargetVideoFormatFileAttachmentInfo);
            }
        }

        public override void InitializeExport(Export export)
		{
			if (export == null || export.MamExport == null) return;

			ProgramName = export.MamExport.ProgramName;
			ProgramId = export.MamExport.ProgramId;
			if (export.MamExport.ExportFileTypes.HasValue) ExportFileTypes = export.MamExport.ExportFileTypes.Value;
			if (export.MamExport.ViewingVideoTargetFormat.HasValue) ViewingVideoTargetFormat = export.MamExport.ViewingVideoTargetFormat.Value;
			ExportFileTypeFileSpecifications = export.MamExport.ExportFileTypeFileSpecifications;

            mamMetroOtherTargetVideoFormatFileSelectorSection.Initialize(export.MamExport.OtherTargetVideoFormatFileAttachmentInfo);
		}

		private void ExportFileTypeDropdown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (!String.IsNullOrEmpty(e.Selected))
				{
					ExportFileTypes = EnumExtensions.GetEnumValueFromDescription<MamExportFileTypes>(e.Selected);
					HandleVisibilityAndEnabledUpdate();
					IsValid(OrderAction.Book);
				}
			}
		}

		private void ViewingvideoTargetFormatDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				if (!String.IsNullOrEmpty(e.Selected))
				{
					ViewingVideoTargetFormat = EnumExtensions.GetEnumValueFromDescription<ViewTargetFormats>(e.Selected);
					HandleVisibilityAndEnabledUpdate();
					IsValid(OrderAction.Book);
				}
			}
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			title.IsVisible = IsVisible;

			programNameLabel.IsVisible = IsVisible;
			programNameTextBox.IsVisible = IsVisible;
			programNameTextBox.IsEnabled = IsEnabled;

			programIdLabel.IsVisible = IsVisible;
			programIdTextBox.IsVisible = IsVisible;
			programIdTextBox.IsEnabled = IsEnabled;

			exportFileTypeLabel.IsVisible = IsVisible;
			exportFileTypeDropdown.IsVisible = IsVisible;
			exportFileTypeDropdown.IsEnabled = IsEnabled;

			viewingVideoTargetFormatLabel.IsVisible = IsVisible && ExportFileTypes == MamExportFileTypes.VIEWING_VIDEO;
			viewingvideoTargetFormatDropDown.IsVisible = viewingVideoTargetFormatLabel.IsVisible;
			viewingvideoTargetFormatDropDown.IsEnabled = IsEnabled;

			exportFileTypeFileSpecificationsLabel.IsVisible = IsVisible && ExportFileTypes == MamExportFileTypes.OTHER || (ExportFileTypes == MamExportFileTypes.VIEWING_VIDEO && ViewingVideoTargetFormat == ViewTargetFormats.OTHER);
			exportFileTypeFileSpecificationsTextBox.IsVisible = exportFileTypeFileSpecificationsLabel.IsVisible;
			exportFileTypeFileSpecificationsTextBox.IsEnabled = IsEnabled;

			bool shouldSectionBeVisible = ExportFileTypes == MamExportFileTypes.VIEWING_VIDEO && ViewingVideoTargetFormat == ViewTargetFormats.OTHER;

			mamMetroOtherTargetVideoFormatFileSelectorSection.IsVisible = IsVisible && shouldSectionBeVisible;

            ToolTipHandler.SetTooltipVisibility(this);
        }

		public override void RegenerateUi()
		{
			mamMetroOtherTargetVideoFormatFileSelectorSection.RegenerateUi();
			GenerateUi(out int row);
		}
	}
}
