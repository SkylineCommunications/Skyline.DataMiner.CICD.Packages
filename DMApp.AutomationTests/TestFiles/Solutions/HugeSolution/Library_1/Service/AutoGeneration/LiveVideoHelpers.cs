namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System.Collections.Generic;

	public static class LiveVideoHelpers
	{
		public static List<LiveVideoService> FlattenServices(IEnumerable<LiveVideoService> services)
		{
			var flattenedServices = new List<LiveVideoService>();
			foreach (var service in services)
			{
				flattenedServices.Add(service);
				flattenedServices.AddRange(FlattenServices(service.Children));
			}

			return flattenedServices;
		}
	}
}
