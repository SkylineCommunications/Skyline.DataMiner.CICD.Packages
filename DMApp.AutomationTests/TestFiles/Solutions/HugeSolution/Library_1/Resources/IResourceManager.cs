namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources
{
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using System;
	using System.Collections.Generic;

	public interface IResourceManager
	{
		ResourcePool[] GetResourcePools(params ResourcePool[] filters);

		IEnumerable<Resource> GetResources(FilterElement<Resource> filter);

		IEnumerable<Resource> GetResources(params Resource[] filters);

		ResourcePool GetResourcePoolByName(string name);

		Resource[] GetFeenixSourceResources(Guid poolGuid, Guid feenixSourceParameterId);

		IEnumerable<Resource> GetResourcesByName(string name);

		ReservationInstance GetReservationInstance(Guid id);
	}
}
