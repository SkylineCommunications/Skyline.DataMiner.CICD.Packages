namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ServiceUpdates
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Service;

	public class EventLevelReceptionUpdateHandler : ServiceUpdateHandler
	{
		public EventLevelReceptionUpdateHandler(Helpers helpers, Service eventLevelReception, Order orderContainingEventLevelReception, Service eventLevelReceptionDuplicate)
            : base (helpers, orderContainingEventLevelReception, eventLevelReception, eventLevelReceptionDuplicate)
		{
		}

        protected override void CollectTasks()
        {
            tasks.AddRange(service.GetUpdateTasks(Helpers, orderContainingService, existingService));
        }
	}
}
