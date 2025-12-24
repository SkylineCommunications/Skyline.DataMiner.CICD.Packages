namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	public class Contract
	{
		public readonly string Id;
		public readonly string Code;
		public readonly string Name;

		public Contract(string id, string code, string name)
		{
			Id = id;
			Code = code;
			Name = name;
		}
	}
}