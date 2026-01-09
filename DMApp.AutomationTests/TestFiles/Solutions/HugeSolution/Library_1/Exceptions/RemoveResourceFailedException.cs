namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class RemoveResourceFailedException : MediaServicesException
	{
		public RemoveResourceFailedException()
		{
		}

		public RemoveResourceFailedException(string serviceName, string functionName, string resourceName)
			: base($"Unable to remove resource {resourceName} for function {functionName} in service {serviceName}")
		{
		}

		public RemoveResourceFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}