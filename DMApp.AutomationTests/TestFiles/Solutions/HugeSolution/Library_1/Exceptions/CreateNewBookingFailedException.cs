namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class CreateNewBookingFailedException : Exception
	{
		public CreateNewBookingFailedException()
		{
		}

		public CreateNewBookingFailedException(string message)
			: base(message)
		{
		}

		public CreateNewBookingFailedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}