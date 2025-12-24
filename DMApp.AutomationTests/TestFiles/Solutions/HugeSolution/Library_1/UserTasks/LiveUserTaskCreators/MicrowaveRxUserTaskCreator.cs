namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class MicrowaveRxUserTaskCreator : LiveUserTaskCreator
	{
		private const string LinkOperator = "Linkkioperaattori";

		public MicrowaveRxUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> {
				{ Descriptions.MicrowaveReception.EquipmentConfiguration, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.MicrowaveReception.EquipmentConfiguration, UserGroup.MwSpecialist) },
				{ Descriptions.MicrowaveReception.EquipmentAllocation, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.MicrowaveReception.EquipmentAllocation, UserGroup.BookingOffice) } };

			userTaskConditions = new Dictionary<string, bool> {
				{ Descriptions.MicrowaveReception.EquipmentConfiguration, true },
				{ Descriptions.MicrowaveReception.EquipmentAllocation, EventHasCeitonResource(LinkOperator) },
			};
		}
	}
}