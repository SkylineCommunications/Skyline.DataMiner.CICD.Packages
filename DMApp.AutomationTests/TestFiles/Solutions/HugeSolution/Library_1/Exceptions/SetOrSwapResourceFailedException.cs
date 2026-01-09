namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class SetOrSwapResourceFailedException : MediaServicesException
	{
		public SetOrSwapResourceFailedException()
		{
		}

		public SetOrSwapResourceFailedException(string serviceName, string functionName, string resourceName)
			: base($"Unable to set resource {resourceName} for function {functionName} in service {serviceName}")
		{
		}

		public SetOrSwapResourceFailedException(string serviceName, string functionName, string resourceName, Exception inner)
			: base($"Unable to set resource {resourceName} for function {functionName} in service {serviceName}: {inner.ToString()}")
		{
		}
	}
}