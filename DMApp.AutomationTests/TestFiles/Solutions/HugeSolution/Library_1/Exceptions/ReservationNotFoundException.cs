namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class ReservationNotFoundException : MediaServicesException
	{
		public ReservationNotFoundException()
		{
		}

		public ReservationNotFoundException(string name)
			: base($"Unable to find Reservation with name {name}")
		{
		}

		public ReservationNotFoundException(Guid ID)
			: base($"Unable to find Reservation with ID {ID}")
		{
		}

		public ReservationNotFoundException(string message, Exception inner)
			: base($"No Reservation Instance found with ID {message}", inner)
		{
		}
	}
}