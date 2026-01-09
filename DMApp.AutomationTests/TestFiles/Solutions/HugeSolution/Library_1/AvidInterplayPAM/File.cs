namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM
{
	public class File
	{
		public string URL { get; set; }

		public string Parent { get; set; }

		public string DisplayName { get; set; }

		public override bool Equals(object obj)
		{
			File other = obj as File;
			if (other == null) return false;

			return this.URL == other.URL;
		}

		public override int GetHashCode()
		{
			return URL.GetHashCode();
		}
	}
}