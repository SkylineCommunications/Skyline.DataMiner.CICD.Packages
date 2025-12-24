namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class IpUserTaskCreator : LiveUserTaskCreator
	{
		public IpUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask>
			{
				{ Descriptions.IP.EquipmentConfiguration, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.IP.EquipmentConfiguration, orderType == OrderType.Video ? UserGroup.McrOperator : UserGroup.AudioMcrOperator) }
			};

			userTaskConditions = new Dictionary<string, bool>
			{
				{ Descriptions.IP.EquipmentConfiguration, RequiresEquipmentConfigurationUserTask() }
			};
		}

		/// <summary>
		/// Checks if a IP Equipment configuration user task is required.
		/// This task is required for all IP receptions and transmissions except for IP Vidigo receptions if the order has a Messi News Recording.
		/// </summary>
		/// <returns>Boolean indicating if the IP Equipment Configuration user task is required.</returns>
		private bool RequiresEquipmentConfigurationUserTask()
		{
			if (service.Definition.VirtualPlatform != ServiceDefinition.VirtualPlatform.ReceptionIp) return true;

			string serviceDefinitionDescription = service.Definition?.Description;
			bool isVidigoReception = !String.IsNullOrEmpty(serviceDefinitionDescription) && serviceDefinitionDescription.Equals("Vidigo", StringComparison.InvariantCultureIgnoreCase);

			if (!isVidigoReception) return true;

			bool orderHasMessiNewsRecording = order.AllServices.Exists(x => x.Definition.VirtualPlatform == ServiceDefinition.VirtualPlatform.Recording && !String.IsNullOrEmpty(x.Definition.Description) && x.Definition.Description.Equals("Messi News", StringComparison.InvariantCultureIgnoreCase));
			return !orderHasMessiNewsRecording;
		}
	}
}