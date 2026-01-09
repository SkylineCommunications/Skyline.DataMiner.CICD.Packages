namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class ReservationChangeTimeFailedException : MediaServicesException
	{
		public ReservationChangeTimeFailedException()
		{
		}

		public ReservationChangeTimeFailedException(string name)
			: base($"Unable to find Reservation with name {name}")
		{
		}

		public ReservationChangeTimeFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}