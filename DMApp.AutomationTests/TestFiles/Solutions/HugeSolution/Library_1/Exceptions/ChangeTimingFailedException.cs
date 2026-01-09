namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class ChangeTimingFailedException : MediaServicesException
	{
		public ChangeTimingFailedException()
		{
		}

		public ChangeTimingFailedException(string serviceName)
			: base($"Unable to change timing for service {serviceName}")
		{
		}

		public ChangeTimingFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}