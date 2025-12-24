namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public interface INonLiveIntegrationManager
	{
		List<Folder> GetRootFolders();

		NonLiveManagerResponse GetChildren(string parentPath);

		Helpers Helpers { get; }
	}
}
