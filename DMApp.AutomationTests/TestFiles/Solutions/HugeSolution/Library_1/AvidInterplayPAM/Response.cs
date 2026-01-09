namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM
{
	public class Response
	{
		public string Id { get; set; }

		public string Username { get; set; }

		public string[] AccessiblePaths { get; set; }

		public Folder[] RequestedFolders { get; set; }

		public File[] RequestedFiles { get; set; }
	}
}