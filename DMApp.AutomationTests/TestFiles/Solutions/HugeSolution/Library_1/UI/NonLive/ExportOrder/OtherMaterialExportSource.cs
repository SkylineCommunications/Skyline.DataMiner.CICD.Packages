namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder.ExportFileSelectorSections;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;

	public class OtherMaterialExportSource : ExportSourceSection
	{
		private readonly Label title = new Label("Other") { Style = TextStyle.Bold };

		private readonly Label sourceFileLocationLabel = new Label("Source file location");
		private readonly YleTextBox sourceFileLocationTextBox = new YleTextBox { IsMultiline = true, Height = 50 };

		private readonly Label sourceFileNameLabel = new Label("Source file name");
		private readonly YleTextBox sourceFileNameTextBox = new YleTextBox();

		private readonly Label exportTypeLabel = new Label("Export Type");
		private readonly YleTextBox exportTypeTextBox = new YleTextBox();

		private readonly Label additionalInformationLabel = new Label("Additional information");
		private readonly YleTextBox additionalInformationTextBox = new YleTextBox { IsMultiline = true, Height = 200 };

        private readonly DefaultFileSelectorSection fileSelectorSection;

		public OtherMaterialExportSource(Helpers helpers, ISectionConfiguration configuration, ExportMainSection section, Export export) : base(helpers, configuration, section, export)
        {
            fileSelectorSection = new DefaultFileSelectorSection(helpers, configuration, export);
			fileSelectorSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			fileSelectorSection.RegenerateUiRequired += HandleRegenerateUiRequired;

            InitializeExport(export);

			GenerateUi(out int row);
		}

		public string SourceFileLocation
		{
			get
			{
				return sourceFileLocationTextBox.Text;
			}
			private set
			{
				sourceFileLocationTextBox.Text = value;
			}
		}

		public string SourceFileName
		{
			get
			{
				return sourceFileNameTextBox.Text;
			}
			private set
			{
				sourceFileNameTextBox.Text = value;
			}
		}

		public string ExportType
		{
			get
			{
				return exportTypeTextBox.Text;
			}
			private set
			{
				exportTypeTextBox.Text = value;
			}
		}

		public string AdditionalInformation
		{
			get
			{
				return additionalInformationTextBox.Text;
			}
			private set
			{
				additionalInformationTextBox.Text = value;
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

			bool isSourceFileLocationValid = !String.IsNullOrWhiteSpace(SourceFileLocation);
			sourceFileLocationTextBox.ValidationState = isSourceFileLocationValid ? UIValidationState.Valid : UIValidationState.Invalid;
			sourceFileLocationTextBox.ValidationText = "Provide the source file location";

			bool isSourceFileNameValid = !String.IsNullOrWhiteSpace(SourceFileName);
			sourceFileNameTextBox.ValidationState = isSourceFileNameValid ? UIValidationState.Valid : UIValidationState.Invalid;
			sourceFileNameTextBox.ValidationText = "Provide the source file name";

			bool isExportTypeValid = !String.IsNullOrWhiteSpace(ExportType);
			exportTypeTextBox.ValidationState = isExportTypeValid ? UIValidationState.Valid : UIValidationState.Invalid;
			exportTypeTextBox.ValidationText = "Provide an export type";

			return isSourceFileLocationValid
				&& isSourceFileNameValid
				&& isExportTypeValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(title, ++row, 0);

			AddWidget(sourceFileLocationLabel, ++row, 0);
			AddWidget(sourceFileLocationTextBox, row, 1, 1, 2);

			AddWidget(sourceFileNameLabel, ++row, 0);
			AddWidget(sourceFileNameTextBox, row, 1, 1, 2);

			AddWidget(exportTypeLabel, ++row, 0);
			AddWidget(exportTypeTextBox, row, 1, 1, 2);

			AddWidget(additionalInformationLabel, ++row, 0);
			AddWidget(additionalInformationTextBox, row, 1, 1, 2);

            AddWidget(new WhiteSpace(), ++row, 0);

            AddSection(fileSelectorSection, new SectionLayout(++row, 0));
            row += fileSelectorSection.RowCount;

            AddWidget(new WhiteSpace(), ++row, 0);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

		public override void UpdateExport(Export export)
		{
            export.OtherExport = export.OtherExport ?? new OtherExport();

            export.OtherExport.SourceFileLocation = SourceFileLocation;
            export.OtherExport.SourceFileName = SourceFileName;
            export.OtherExport.ExportType = ExportType;
            export.OtherExport.AdditionalInformation = AdditionalInformation;

            fileSelectorSection.UpdateFileAttachmentInfo(export.OtherExport.OtherExportSourceFileAttachmentInfo);
		}

		public override void InitializeExport(Export export)
		{
			if (export == null || export.OtherExport == null)
			{
				return;
			}

			SourceFileLocation = export.OtherExport.SourceFileLocation;
			SourceFileName = export.OtherExport.SourceFileName;
			ExportType = export.OtherExport.ExportType;
			AdditionalInformation = export.OtherExport.AdditionalInformation;

            fileSelectorSection.Initialize(export.OtherExport.OtherExportSourceFileAttachmentInfo);
		}

        protected override void HandleVisibilityAndEnabledUpdate()
        {
			title.IsVisible = IsVisible;

			sourceFileLocationLabel.IsVisible = IsVisible;
			sourceFileLocationTextBox.IsVisible = IsVisible;
			sourceFileLocationTextBox.IsEnabled = IsEnabled;

			sourceFileNameLabel.IsVisible = IsVisible;
			sourceFileNameTextBox.IsVisible = IsVisible;
			sourceFileNameTextBox.IsEnabled = IsEnabled;

			exportTypeLabel.IsVisible = IsVisible;
			exportTypeTextBox.IsVisible = IsVisible;
			exportTypeTextBox.IsEnabled = IsEnabled;

			additionalInformationLabel.IsVisible = IsVisible;
			additionalInformationTextBox.IsVisible = IsVisible;
			additionalInformationTextBox.IsEnabled = IsEnabled;

            fileSelectorSection.IsVisible = IsVisible;
			fileSelectorSection.IsEnabled = IsEnabled;

            ToolTipHandler.SetTooltipVisibility(this);
        }

		public override void RegenerateUi()
		{
			fileSelectorSection.RegenerateUi();
			GenerateUi(out int row);
		}
	}

}
