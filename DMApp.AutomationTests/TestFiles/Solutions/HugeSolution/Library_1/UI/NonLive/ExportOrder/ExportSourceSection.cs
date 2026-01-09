namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive.ExportOrder
{
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public abstract class ExportSourceSection : YleSection
	{
		protected readonly ExportMainSection section;
		protected readonly Export export;
        protected readonly ISectionConfiguration configuration;

        protected ExportSourceSection(Helpers helpers, ISectionConfiguration configuration, ExportMainSection section, Export export) : base(helpers)
		{
			this.section = section;
			this.export = export;
			this.configuration = configuration;
		}

		public abstract bool IsValid(OrderAction action);

		public abstract void InitializeExport(Export export);

		public abstract void UpdateExport(Export export);

		public abstract IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections { get; }
	}
}
