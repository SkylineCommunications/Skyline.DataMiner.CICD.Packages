namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;

	public class VideoResolution
	{
		public readonly string Code;

		public VideoResolution(string code)
		{
			Code = code ?? throw new ArgumentNullException("code");
		}
	}
}