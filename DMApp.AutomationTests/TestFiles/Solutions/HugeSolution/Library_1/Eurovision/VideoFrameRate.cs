namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;

	public class VideoFrameRate
	{
		public readonly string Code;
		public readonly string Name;

		public VideoFrameRate(string code, string name)
		{
			Code = code ?? throw new ArgumentNullException("code");
			Name = name;
		}
	}
}