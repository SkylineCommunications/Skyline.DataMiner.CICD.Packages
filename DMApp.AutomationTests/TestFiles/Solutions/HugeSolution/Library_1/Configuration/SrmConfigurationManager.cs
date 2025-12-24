namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Library.Solutions.SRM;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Net.ServiceManager.Objects;

	public static class SrmConfigurationManager
	{
		/// <summary>
		/// Sets the SwapResource property on all nodes of all service definitions.
		/// </summary>
		/// <remarks>This is a method to facilitate SRM configuration. The SwapResource property should be registered on Cube for this method to work.</remarks>
		public static void AddSwapResourcePropertyToEachNodeOfEachServiceDefinition()
		{
			var serviceManagerHelper = SrmManagers.ServiceManager;

            var serviceDefinitions = serviceManagerHelper.GetServiceDefinitions(ServiceDefinitionExposers.IsTemplate.Equal(true));
            foreach (var def in serviceDefinitions)
			{
				foreach (Node node in def.Diagram.Nodes)
				{
					if (!node.Properties.Any(p => p.Name == "SwapResource"))
					{
						node.Properties.Add(new Property("SwapResource", "{\"Enabled\":true}"));
					}
				}
			}

			serviceManagerHelper.SetServiceDefinitions(serviceDefinitions, false, true);
		}

		/// <summary>
		/// Sets the RemoveResource property on all nodes of all service definitions.
		/// </summary>
		/// <remarks>This is a method to facilitate SRM configuration. The RemoveResource property should be registered on Cube for this method to work.</remarks>
		public static void AddRemoveResourcePropertyToEachNodeOfEachServiceDefinition()
		{
			var serviceManagerHelper = SrmManagers.ServiceManager;

            var serviceDefinitions = serviceManagerHelper.GetServiceDefinitions(ServiceDefinitionExposers.IsTemplate.Equal(true));
			foreach (var def in serviceDefinitions)
			{
				foreach (Node node in def.Diagram.Nodes)
				{
					if (!node.Properties.Any(p => p.Name == "RemoveResource"))
					{
						node.Properties.Add(new Property("RemoveResource", "{\"Enabled\":true}"));
					}
				}
			}

			serviceManagerHelper.SetServiceDefinitions(serviceDefinitions, false, true);
		}
	}
}