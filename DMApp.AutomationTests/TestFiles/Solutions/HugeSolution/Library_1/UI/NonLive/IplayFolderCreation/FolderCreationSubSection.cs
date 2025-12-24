namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.IplayFolderCreation
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public abstract class FolderCreationSubSection : YleSection
    {
        protected const int labelColumn = 0;
        protected const int inputColumn = 1;
        protected const int inputRowStretch = 1;
        protected const int inputColumnStretch = 2;

        protected readonly FolderCreationSection section;
        protected readonly ISectionConfiguration configuration;

        protected FolderCreationSubSection(Helpers helpers, ISectionConfiguration configuration, FolderCreationSection section) : base(helpers)
        {
            this.section = section;
            this.configuration = configuration;
        }

        public abstract bool IsValid();

        public abstract void UpdateFolderCreation(FolderCreation folderCreation);
    }
}
