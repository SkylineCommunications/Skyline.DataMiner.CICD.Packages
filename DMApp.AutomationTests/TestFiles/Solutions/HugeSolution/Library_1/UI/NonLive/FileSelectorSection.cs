namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class FileSelectorSection : YleSection
    {
        private readonly NonLiveOrder nonLiveOrder;
		private readonly ISectionConfiguration configuration;

        protected readonly Label subtitleSourceFileLabel = new Label("Upload file attachment(s)");
        protected readonly Label selectFilestoRemoveLabel = new Label("Select file(s) to remove") { IsVisible = false };
        protected readonly Label validationLabel = new Label(string.Empty) { IsVisible = true };

        protected FileSelectorSection(Helpers helpers, ISectionConfiguration configuration, NonLiveOrder nonLiveOrder) : base(helpers)
        {
            this.nonLiveOrder = nonLiveOrder;
			this.configuration = configuration;
        }

        public FileSelector FileSelector { get; protected set; } = new FileSelector { AllowMultipleFiles = true, IsVisible = false };

		public CheckBoxList FilesToRemoveCheckBoxList { get; protected set; } = new CheckBoxList { IsSorted = true };

		public abstract bool IsValid();

        public virtual void UpdateFileAttachmentInfo(FileAttachmentInfo fileAttachmentInfo)
        {
            fileAttachmentInfo = fileAttachmentInfo ?? new FileAttachmentInfo();

            fileAttachmentInfo.FileSelector = FileSelector;
            fileAttachmentInfo.SubtitleAttachmentFileNames.AddRange(FilterOutFileNamesFromUploadedPaths());

            var fileNamesToRemove = FilesToRemoveCheckBoxList.Checked.ToList();
            if (FilesToRemoveCheckBoxList.Options != null && fileNamesToRemove.Any())
            {
                fileAttachmentInfo.SubtitleAttachmentFilesToRemove.Clear();
                fileAttachmentInfo.SubtitleAttachmentFilesToRemove.AddRange(fileNamesToRemove);
                fileAttachmentInfo.SubtitleAttachmentFileNames.RemoveAll(x => fileNamesToRemove.Contains(x));
            }
        }

		public override void RegenerateUi()
		{
			GenerateUi(out int row);
		}

		public virtual void Initialize(FileAttachmentInfo fileAttachmentInfo)
        {
            if (fileAttachmentInfo?.SubtitleAttachmentFileNames != null && fileAttachmentInfo.SubtitleAttachmentFileNames.Any())
            {
                var allFilePaths = nonLiveOrder.GetAttachments(helpers.Engine, Constants.TicketAttachmentsFolderPath);

                foreach (var filePath in allFilePaths)
                {
                    string fileName = FilterOutFileName(filePath);

                    if (fileAttachmentInfo.SubtitleAttachmentFileNames.Contains(fileName))
                    {
                        FilesToRemoveCheckBoxList.AddOption(fileName);
                    }
                    else
                    {
                        fileAttachmentInfo.SubtitleAttachmentFileNames.Remove(fileName);
                    }
                }
            }
        }

        protected override void GenerateUi(out int row)
        {
			base.GenerateUi(out row);

            AddWidget(subtitleSourceFileLabel, ++row, 0);
            AddWidget(FileSelector, row, 1);
            AddWidget(validationLabel, ++row, 1);

            AddWidget(selectFilestoRemoveLabel, ++row, 0);
            AddWidget(FilesToRemoveCheckBoxList, row, 1);

            ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);

			HandleVisibilityAndEnabledUpdate();
        }

        protected override void HandleVisibilityAndEnabledUpdate()
        {
            subtitleSourceFileLabel.IsVisible = IsVisible;
            FileSelector.IsVisible = subtitleSourceFileLabel.IsVisible;
            validationLabel.IsVisible = subtitleSourceFileLabel.IsVisible;

            selectFilestoRemoveLabel.IsVisible = subtitleSourceFileLabel.IsVisible && FilesToRemoveCheckBoxList != null && FilesToRemoveCheckBoxList.Options.Any();
            FilesToRemoveCheckBoxList.IsVisible = subtitleSourceFileLabel.IsVisible && FilesToRemoveCheckBoxList != null && FilesToRemoveCheckBoxList.Options.Any();

            ToolTipHandler.SetTooltipVisibility(this);
        }

        private List<string> FilterOutFileNamesFromUploadedPaths()
        {
            List<string> fileNames = new List<string>();
            foreach (var uploadedFilePath in FileSelector.UploadedFilePaths)
            {
               fileNames.Add(FilterOutFileName(uploadedFilePath));
            }

            return fileNames;
        }

        private static string FilterOutFileName(string filePath)
        {
            string[] splittedFilePath = filePath?.Split(new[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);

            return splittedFilePath.Last();
        }
    }
}
