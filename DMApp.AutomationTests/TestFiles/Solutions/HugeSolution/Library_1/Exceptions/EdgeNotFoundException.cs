namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class EdgeNotFoundException : MediaServicesException
	{
		public EdgeNotFoundException()
		{
		}

		public EdgeNotFoundException(int NodeId, Guid serviceDefinitionId)
			: base($"Unable to find Edge connected to node {NodeId} in service definition {serviceDefinitionId}")
		{
		}

		public EdgeNotFoundException(string message)
			: base(message)
		{
		}

		public EdgeNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}