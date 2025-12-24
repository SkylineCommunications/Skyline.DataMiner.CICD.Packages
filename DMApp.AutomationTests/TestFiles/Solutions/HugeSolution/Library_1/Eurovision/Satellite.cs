namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;

	public class Satellite
	{
		public readonly string Id;
		public readonly string Code;
		public readonly string Name;
		public readonly string ProductCode;
		public readonly string FamilyName;

		public readonly string DisplayName;

		public Satellite(string id, string code, string name, string productCode, string familyName)
		{
			Id = id;
			Code = code;
			Name = name;
			ProductCode = productCode;
			FamilyName = familyName;

			DisplayName = String.Format("{0}, {1}", FamilyName, Name);
		}
	}
}