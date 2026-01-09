namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;

	public class VideoAspectRatio
	{
		public readonly string Code;
		public readonly string Name;

		public VideoAspectRatio(string code)
		{
			Code = code ?? throw new ArgumentNullException("code");
			Name = code;
		}
	}
}