namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;

	public class City
	{
		public readonly string Code;
		public readonly string Name;
		public readonly string CountryCode;

		public City(string code, string name, string countryCode)
		{
			Code = code;
			Name = name;
			CountryCode = countryCode;
		}

		public string DisplayName { get { return String.Format("{0} ({1})", Name, CountryCode); } }
	}
}