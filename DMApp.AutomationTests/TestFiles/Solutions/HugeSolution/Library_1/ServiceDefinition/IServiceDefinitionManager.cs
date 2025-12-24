namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

	using Service = Service.Service;

	public interface IServiceDefinitionManager
	{
		ServiceDefinition RoutingServiceDefinition { get; }

		ServiceDefinition AudioProcessingServiceDefinition { get; }

		ServiceDefinition GraphicsProcessingServiceDefinition { get; }

		ServiceDefinition VideoProcessingServiceDefinition { get; }

		ServiceDefinition VizremConverterHelsinkiServiceDefinition { get; }

		ServiceDefinition VizremConverterMediapolisServiceDefinition { get; }

		ServiceDefinitionsForLiveOrderForm ServiceDefinitionsForLiveOrderForm { get; }

		ServiceDefinition GetServiceDefinition(Guid serviceDefinitionGuid);

		ServiceDefinition GetServiceDefinition(string nameFilter);

		Net.ServiceManager.Objects.ServiceDefinition GetRawServiceDefinition(Guid serviceDefinitionGuid);

		ServiceDefinition GetDummyRxServiceDefinition();

		Element GetBookingManager(ServiceDefinition serviceDefinition);

		Element GetBookingManager(VirtualPlatform virtualPlatform);

		Net.ServiceManager.Objects.ServiceDefinition GetServiceDefinitionFromOrder(Order order, List<Service> servicesToRemove);


		Net.ServiceManager.Objects.ServiceDefinition AddOrUpdateServiceDefinition(Net.ServiceManager.Objects.ServiceDefinition serviceDefinition);

		void DeleteServiceDefinition(Guid serviceDefinitionGuid);

		IEnumerable<ServiceDefinition> GetReceptionServiceDefinitions();
	}
}