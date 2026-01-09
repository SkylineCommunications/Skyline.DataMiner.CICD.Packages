namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class AddOrUpdateEventFailedException : MediaServicesException
	{
		public AddOrUpdateEventFailedException()
		{
		}

		public AddOrUpdateEventFailedException(string message)
			: base(message)
		{
		}

		public AddOrUpdateEventFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}