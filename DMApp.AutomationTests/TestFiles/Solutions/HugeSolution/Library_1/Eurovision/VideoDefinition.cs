namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;

	public class VideoDefinition
	{
		private readonly string code;

		public VideoDefinition(string code)
		{
			this.code = code ?? throw new ArgumentNullException("code");
		}

		public string Code => code;

		public override bool Equals(Object obj)
		{
			return obj is VideoDefinition definition && this.code == definition.code;
		}

		public override int GetHashCode()
		{
			return code.GetHashCode();
		}
	}
}