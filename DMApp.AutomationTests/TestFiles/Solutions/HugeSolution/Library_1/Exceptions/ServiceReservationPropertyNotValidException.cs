namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	class ServiceReservationPropertyNotValidException : MediaServicesException
	{
		public ServiceReservationPropertyNotValidException()
		{
		}

		public ServiceReservationPropertyNotValidException(string message)
			: base(message)
		{
		}

		public ServiceReservationPropertyNotValidException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}