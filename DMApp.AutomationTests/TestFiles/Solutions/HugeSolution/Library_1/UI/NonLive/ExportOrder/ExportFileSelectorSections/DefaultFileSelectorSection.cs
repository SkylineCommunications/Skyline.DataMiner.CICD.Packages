namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder.ExportFileSelectorSections
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class DefaultFileSelectorSection : FileSelectorSection
    {
        public DefaultFileSelectorSection(Helpers helpers, ISectionConfiguration configuration, Export export) : base(helpers, configuration, export)
        {
            GenerateUi(out int row);
        }

        public override bool IsValid()
        {
            // File uploading is optional for this section
            return true;
        }
    }
}
