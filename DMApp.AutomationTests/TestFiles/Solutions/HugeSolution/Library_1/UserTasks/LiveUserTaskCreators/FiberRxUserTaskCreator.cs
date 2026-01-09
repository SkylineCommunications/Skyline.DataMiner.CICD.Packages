namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class FiberRxUserTaskCreator : LiveUserTaskCreator
	{
		private const string FiberExpert = "Kuituasiantuntija";

		public FiberRxUserTaskCreator(Helpers helpers, YLE.Service.Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			bool eventHasCeitonResourceFiberExpert = EventHasCeitonResource(FiberExpert);

			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.FiberReception.AllocationNeeded, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.FiberReception.AllocationNeeded, UserGroup.BookingOffice) },
																		{ Descriptions.FiberReception.EquipmentAllocation, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.FiberReception.EquipmentAllocation, UserGroup.FiberSpecialist) }};

			userTaskConditions = new Dictionary<string, bool>
			{
				{ Descriptions.FiberReception.AllocationNeeded, eventHasCeitonResourceFiberExpert },
				{ Descriptions.FiberReception.EquipmentAllocation, true }
			};
		}
	}
}