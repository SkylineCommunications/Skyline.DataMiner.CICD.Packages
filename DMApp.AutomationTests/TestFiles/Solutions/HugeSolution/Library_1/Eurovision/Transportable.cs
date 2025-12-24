namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;

	public class Transportable
	{
		public readonly string Id;
		public readonly string Code;
		public readonly string Name;

		public readonly string DisplayName;

		public Transportable(string id, string code, string name)
		{
			Id = id;
			Code = code;
			Name = name;

			if (Code == Name || Name.StartsWith(Code))
				DisplayName = Name;
			else
				DisplayName = String.Format("{0} - {1}", Code, Name);
		}
	}
}