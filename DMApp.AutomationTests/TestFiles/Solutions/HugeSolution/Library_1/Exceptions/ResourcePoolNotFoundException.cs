namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class ResourcePoolNotFoundException : MediaServicesException
	{
		public ResourcePoolNotFoundException()
		{
		}

		public ResourcePoolNotFoundException(string name)
			: base($"Unable to find Resource Pool with name '{name}'")
		{
		}

		public ResourcePoolNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}