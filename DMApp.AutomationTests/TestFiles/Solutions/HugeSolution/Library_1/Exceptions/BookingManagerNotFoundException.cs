namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class BookingManagerNotFoundException : MediaServicesException
	{
		public BookingManagerNotFoundException()
		{
		}

		public BookingManagerNotFoundException(string message)
			: base(message)
		{
		}

		public BookingManagerNotFoundException(Guid reservationInstanceId)
			: base($"Unable to find Booking Manager for reservationInstance {reservationInstanceId}")
		{
		}

		public BookingManagerNotFoundException(string bookingManagerElementName, Guid reservationInstanceId)
			: base($"Unable to find Booking Manager {bookingManagerElementName} for reservationInstance {reservationInstanceId}")
		{
		}

		public BookingManagerNotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}