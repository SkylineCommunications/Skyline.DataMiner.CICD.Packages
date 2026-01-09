namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class EditBookingFailedException : MediaServicesException
	{
		public EditBookingFailedException()
		{
		}

		public EditBookingFailedException(string message)
			: base(message)
		{
		}

		public EditBookingFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}