namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive
{
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public abstract class MainSection : YleSection
	{
		protected const string AvidInterplayPamProtocolName = "Avid Interplay PAM";
		protected const string MediaParkkiProtocolName = "Generic SMB";

		protected MainSection(Helpers helpers) : base(helpers)
		{
			
		}

		public abstract bool IsValid(OrderAction action);

		public abstract void UpdateNonLiveOrder(NonLiveOrder nonLiveOrder);

		public abstract IEnumerable<NonLiveManagerTreeViewSection> TreeViewSections { get; }
	}
}
