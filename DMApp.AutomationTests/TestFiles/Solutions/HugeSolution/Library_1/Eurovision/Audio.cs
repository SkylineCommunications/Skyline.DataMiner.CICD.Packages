namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	public class Audio
	{
		public readonly string Code;
		public readonly string Name;

		public Audio(Audio other)
		{
			Code = other.Code;
			Name = other.Name;
		}

		public Audio(string code, string name)
		{
			Code = code;
			Name = name;
		}

		public string Text { get; set; }
	}
}