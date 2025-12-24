namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class ResourcePropertyNotFoundException : MediaServicesException
	{
		public ResourcePropertyNotFoundException()
		{
		}

		public ResourcePropertyNotFoundException(string propertyName, string resourceName)
			: base($"Unable to find property with name {propertyName} on resource {resourceName}")
		{
		}
	}
}