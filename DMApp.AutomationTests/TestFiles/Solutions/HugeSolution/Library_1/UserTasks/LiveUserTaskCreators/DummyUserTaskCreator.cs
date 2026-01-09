namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTaskCreators;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.LiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Service = Service.Service;

	public class DummyUserTaskCreator : LiveUserTaskCreator
	{
		public DummyUserTaskCreator(Helpers helpers, Service service, Guid ticketFieldResolverId, Order.Order order)
			: base(helpers, service, order)
		{
			userTaskConstructors = new Dictionary<string, UserTask> { { Descriptions.Dummy.SelectTechnicalSystem, new LiveUserTask(helpers, ticketFieldResolverId, service, Descriptions.Dummy.SelectTechnicalSystem, UserGroup.McrOperator) } };

			userTaskConditions = new Dictionary<string, bool>
			{
				{ Descriptions.Dummy.SelectTechnicalSystem, IsEurovisionDummy() }
			};
		}

		private bool IsEurovisionDummy()
		{
			bool isEbuDummy = service.IntegrationType == IntegrationType.Eurovision && service.Definition.IsDummy && service.EurovisionServiceConfigurations != null && service.EurovisionServiceConfigurations.Any();

			Log(nameof(IsEurovisionDummy),$"Service is {(isEbuDummy ? string.Empty : "not ")}an Eurovision dummy",service.Name);

			return isEbuDummy;
		}
	}
}