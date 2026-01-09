namespace UpdateTicketStatus_4
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static class GeneralFunctionality
	{

		public static void SettingNonLiveOrderToCompleteOrInComplete(Helpers helpers, NonLiveOrder nonLiveOrder, User currentUser)
		{
			if (nonLiveOrder.State != State.Completed)
			{
				helpers.NonLiveOrderManager.CompleteNonLiveOrder(nonLiveOrder, currentUser);
				helpers.NonLiveUserTaskManager.AddOrUpdateUserTasks(nonLiveOrder);
			}
			else if (nonLiveOrder.State == State.Completed)
			{
				helpers.NonLiveOrderManager.SetNonLiveOrderToWorkInProgress(nonLiveOrder, currentUser);
			}
			else
			{
				// Do nothing
			}
		}
	}
}