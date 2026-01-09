namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class ServiceReservationPropertyNotFoundException : MediaServicesException
	{
		public ServiceReservationPropertyNotFoundException()
		{
		}

		public ServiceReservationPropertyNotFoundException(string propertyName, string serviceName)
			: base($"Unable to find property {propertyName} on service {serviceName}")
		{
		}

		public ServiceReservationPropertyNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}