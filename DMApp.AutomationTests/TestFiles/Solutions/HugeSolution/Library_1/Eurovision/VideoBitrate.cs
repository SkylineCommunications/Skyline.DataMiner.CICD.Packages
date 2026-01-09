namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;

	public class VideoBitrate
	{
		public readonly string Code;
		public readonly string Name;

		public VideoBitrate(string code, string name)
		{
			Code = code ?? throw new ArgumentNullException("code");
			Name = name;
		}
	}
}