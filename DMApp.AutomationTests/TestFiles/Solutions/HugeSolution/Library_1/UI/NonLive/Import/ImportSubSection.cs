namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.Import
{
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public abstract class ImportSubSection : YleSection
	{
		protected readonly ImportMainSection ingestMainSection;
		protected readonly Ingest ingest;
		protected readonly ISectionConfiguration configuration;

		protected ImportSubSection(Helpers helpers, ISectionConfiguration configuration, ImportMainSection section, Ingest ingest) : base(helpers)
		{
			this.configuration = configuration;
			this.ingestMainSection = section;
			this.ingest = ingest;
		}

        public abstract bool IsValid();

		public abstract void UpdateIngest(Ingest ingest);

		public abstract IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections { get; }
	}
}
