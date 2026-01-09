namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	public class Facility
	{
		public readonly string ProductId;
		public readonly string ProductCode;
		public readonly string ProductName;

		public Facility(string productId, string productCode, string productName)
		{
			ProductId = productId;
			ProductCode = productCode;
			ProductName = productName;
		}
	}
}