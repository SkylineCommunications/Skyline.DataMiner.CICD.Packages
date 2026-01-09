namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service
{
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using System;
	using System.Collections.Generic;

	public interface IServiceManager
	{
		bool TryGetService(Guid serviceId, out Service service);

		Service GetService(Guid serviceId);

		List<Service> GetOrderServices(Guid orderId);

		List<Service> GetOrderServices(ServiceReservationInstance orderReservationInstance, bool forceReservationInstanceToOverwriteServiceConfiguration = false, Net.ServiceManager.Objects.ServiceDefinition orderServiceDefinition = null);

		bool TryChangeServiceTime(Service service);
	}
}
