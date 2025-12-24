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

	public class MediaParkkiMaterialExportSource : ExportSourceSection
	{
		private TargetVideoFormats selectedTargetVideoFormat = TargetVideoFormats.AVCI100;
		private VideoViewingQualities selectedVideoViewingQuality = VideoViewingQualities.HIGH;

		private readonly Label title = new Label("MediaParkki") { Style = TextStyle.Bold };

		private readonly NonLiveManagerTreeViewSection treeViewSection;

		private readonly Label targetVideoFormatLabel = new Label("Target video format");
		private readonly DropDown targetVideoFormatDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<TargetVideoFormats>(), EnumExtensions.GetDescriptionFromEnumValue(TargetVideoFormats.AVCI100));

		private readonly Label otherTargetVideoFormatInfoLabel = new Label("Other video format");
		private readonly YleTextBox otherTargetVideoFormatInfoTextBox = new YleTextBox { IsMultiline = true, Height = 200, PlaceHolder = "Please give additional specifications for the file format and wrapper." };

		private readonly Label viewingVideoQualityLabel = new Label("Video quality");
		private readonly DropDown viewingVideoQualityDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<VideoViewingQualities>(), EnumExtensions.GetDescriptionFromEnumValue(VideoViewingQualities.HIGH));

		private readonly DefaultFileSelectorSection mediaParkiiOtherTargetVideoFormatFileSelectorSection;

		public MediaParkkiMaterialExportSource(Helpers helpers, ISectionConfiguration configuration, Element element, ExportMainSection section, Export export) : base(helpers, configuration, section, export)
		{
			treeViewSection = new NonLiveManagerTreeViewSection(helpers, configuration, new MediaParkkiManager(helpers), TreeViewType.FileSelector);
			treeViewSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			treeViewSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			treeViewSection.SelectedItemChanged += (sender, args) => IsValid(OrderAction.Book);
			treeViewSection.SourceLabel.Text = "Source file";
			treeViewSection.InitRoot();

			mediaParkiiOtherTargetVideoFormatFileSelectorSection = new DefaultFileSelectorSection(helpers, configuration, export);
			mediaParkiiOtherTargetVideoFormatFileSelectorSection.UiEnabledStateChangeRequired += HandleUiEnabledStateChangeRequired;
			mediaParkiiOtherTargetVideoFormatFileSelectorSection.RegenerateUiRequired += HandleRegenerateUiRequired;

			targetVideoFormatDropDown.Changed += TargetVideoFormatDropDown_Changed;

			InitializeExport(export);

			GenerateUi(out int row);
		}

		public Folder Folder => treeViewSection.SelectedFolder;

		public HashSet<File> Files => treeViewSection.SelectedFiles;

		public TargetVideoFormats TargetVideoFormat
		{
			get
			{
				return selectedTargetVideoFormat;
			}
			private set
			{
				selectedTargetVideoFormat = value;
				targetVideoFormatDropDown.Selected = EnumExtensions.GetDescriptionFromEnumValue(selectedTargetVideoFormat);
			}
		}

		public string OtherVideoFormatInfo
		{
			get
			{
				return otherTargetVideoFormatInfoTextBox.Text;
			}
			private set
			{
				otherTargetVideoFormatInfoTextBox.Text = value;
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

		public override IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections
		{
			get
			{
				return new[] { treeViewSection };
			}
		}

		public override bool IsValid(OrderAction action)
		{
			if (action == OrderAction.Save) return true;

			bool isSourceFileValid = Files != null;

			treeViewSection.ValidationState = isSourceFileValid ? UIValidationState.Valid : UIValidationState.Invalid;
			treeViewSection.ValidationText = "Provide a source file";

			bool isOtherVideoFormatInfoValid = TargetVideoFormat != TargetVideoFormats.OTHER || !String.IsNullOrWhiteSpace(OtherVideoFormatInfo);
			otherTargetVideoFormatInfoTextBox.ValidationState = isOtherVideoFormatInfoValid ? UIValidationState.Valid : UIValidationState.Invalid;
			otherTargetVideoFormatInfoTextBox.ValidationText = "Provide a video format";

			return isSourceFileValid
				&& isOtherVideoFormatInfoValid;
		}

		protected override void GenerateUi(out int row)
		{
			base.GenerateUi(out row);

			AddWidget(title, ++row, 0, 1, 2);

			AddSection(treeViewSection, new SectionLayout(++row, 0));
			row += treeViewSection.RowCount;

			AddWidget(targetVideoFormatLabel, ++row, 0);
			AddWidget(targetVideoFormatDropDown, row, 1, 1, 2);

			AddWidget(otherTargetVideoFormatInfoLabel, ++row, 0);
			AddWidget(otherTargetVideoFormatInfoTextBox, row, 1, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddSection(mediaParkiiOtherTargetVideoFormatFileSelectorSection, new SectionLayout(++row, 0));
			row += mediaParkiiOtherTargetVideoFormatFileSelectorSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(viewingVideoQualityLabel, ++row, 0);
			AddWidget(viewingVideoQualityDropDown, row, 1, 1, 2);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
		}

		public override void UpdateExport(Export export)
		{
			export.MediaParkkiExport = export.MediaParkkiExport ?? new MediaParkkiExport();

			export.MediaParkkiExport.SourceFolderUrls = treeViewSection.GetFolderUrls().ToArray();
			export.MediaParkkiExport.SourceFileUrls = treeViewSection.GetSelectedFileUrls();
			export.MediaParkkiExport.TargetVideoFormat = this.TargetVideoFormat;

			if (TargetVideoFormat == TargetVideoFormats.H264) export.MediaParkkiExport.VideoViewingQuality = VideoViewingQuality;
			if (TargetVideoFormat == TargetVideoFormats.OTHER)
			{
				export.MediaParkkiExport.OtherVideoFormatInfo = OtherVideoFormatInfo;
				mediaParkiiOtherTargetVideoFormatFileSelectorSection.UpdateFileAttachmentInfo(export.MediaParkkiExport.OtherTargetVideoFormatFileAttachmentInfo);
			}
		}

		public override void InitializeExport(Export export)
		{
			if (export == null || export.MediaParkkiExport == null) return;

			if (export.MediaParkkiExport.SourceFileUrls != null && export.MediaParkkiExport.SourceFolderUrls != null && export.MediaParkkiExport.SourceFileUrls.Any())
			{
				treeViewSection.Initialize(export.MediaParkkiExport.SourceFolderUrls, export.MediaParkkiExport.SourceFileUrls);
			}

			if (export.MediaParkkiExport.TargetVideoFormat.HasValue) TargetVideoFormat = export.MediaParkkiExport.TargetVideoFormat.Value;
			OtherVideoFormatInfo = export.MediaParkkiExport.OtherVideoFormatInfo;

			if (export.MediaParkkiExport.VideoViewingQuality.HasValue) VideoViewingQuality = export.MediaParkkiExport.VideoViewingQuality.Value;

			mediaParkiiOtherTargetVideoFormatFileSelectorSection.Initialize(export.MediaParkkiExport.OtherTargetVideoFormatFileAttachmentInfo);
		}

		private void TargetVideoFormatDropDown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				TargetVideoFormat = EnumExtensions.GetEnumValueFromDescription<TargetVideoFormats>(e.Selected);

				HandleVisibilityAndEnabledUpdate();
				IsValid(OrderAction.Book);
			}
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			title.IsVisible = IsVisible;

			treeViewSection.IsVisible = IsVisible;
			treeViewSection.IsEnabled = IsEnabled;

			targetVideoFormatLabel.IsVisible = IsVisible;
			targetVideoFormatDropDown.IsVisible = IsVisible;
			targetVideoFormatDropDown.IsEnabled = IsEnabled;

			otherTargetVideoFormatInfoLabel.IsVisible = TargetVideoFormat == TargetVideoFormats.OTHER;
			otherTargetVideoFormatInfoTextBox.IsVisible = otherTargetVideoFormatInfoLabel.IsVisible;
			otherTargetVideoFormatInfoTextBox.IsEnabled = IsEnabled;

			mediaParkiiOtherTargetVideoFormatFileSelectorSection.IsVisible = IsVisible && TargetVideoFormat == TargetVideoFormats.OTHER;
			mediaParkiiOtherTargetVideoFormatFileSelectorSection.IsEnabled = IsEnabled;

			viewingVideoQualityLabel.IsVisible = TargetVideoFormat == TargetVideoFormats.H264;
			viewingVideoQualityDropDown.IsVisible = viewingVideoQualityLabel.IsVisible;
			viewingVideoQualityDropDown.IsEnabled = IsEnabled;

			ToolTipHandler.SetTooltipVisibility(this);
		}

		public override void RegenerateUi()
		{
			treeViewSection.RegenerateUi();
			mediaParkiiOtherTargetVideoFormatFileSelectorSection.RegenerateUi();
			GenerateUi(out int row);
		}
	}

}
