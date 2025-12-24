namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class ServiceGenerationFailedException : MediaServicesException
	{
		public ServiceGenerationFailedException()
		{
		}

		public ServiceGenerationFailedException(string name)
			: base(name)
		{
		}

		public ServiceGenerationFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
