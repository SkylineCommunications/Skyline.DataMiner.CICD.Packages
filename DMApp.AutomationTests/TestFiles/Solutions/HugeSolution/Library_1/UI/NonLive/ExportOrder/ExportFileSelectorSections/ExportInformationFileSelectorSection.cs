namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder.ExportFileSelectorSections
{
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ExportInformationFileSelectorSection : FileSelectorSection
    {
        private readonly ExportInformationSection exportInformationSection;

        public ExportInformationFileSelectorSection(Helpers helpers, ISectionConfiguration configuration, ExportInformationSection exportInformationSection, Export export) : base(helpers, configuration, export)
        {
            this.exportInformationSection = exportInformationSection;

            GenerateUi(out int row);
        }

        public override bool IsValid()
        {
            bool isSubtitleSourceFileValid = !exportInformationSection.AddSubtitles || FileSelector.UploadedFilePaths.Any() || FilesToRemoveCheckBoxList.Options.Any();

            validationLabel.Text = !isSubtitleSourceFileValid ? "Please upload any subtitle file attachment(s)" : string.Empty;

            return isSubtitleSourceFileValid;
        }
	}
}
