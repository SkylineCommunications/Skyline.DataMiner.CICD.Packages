namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;

	public class VideoBandwidth
	{
		public readonly string Code;

		public VideoBandwidth(string code)
		{
			Code = code ?? throw new ArgumentNullException("code");
		}
	}
}