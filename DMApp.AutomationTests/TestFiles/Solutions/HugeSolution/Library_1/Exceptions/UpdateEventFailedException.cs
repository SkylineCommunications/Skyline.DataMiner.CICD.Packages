namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class UpdateEventFailedException : MediaServicesException
	{
		public UpdateEventFailedException()
		{
		}

		public UpdateEventFailedException(string message)
			: base(message)
		{
		}

		public UpdateEventFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}