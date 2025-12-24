namespace UpdateTicketStatus_4
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public static class NonLiveOrderHandler
	{
		public static bool TryUpdateNonLiveOrder(Helpers helpers, int[] convertedSplitTicketId, Guid parsedId)
		{
			var currentUser = helpers.ContractManager.GetBaseUserInfo(helpers.Engine.UserLoginName)?.User;

			if (parsedId != Guid.Empty && helpers.NonLiveOrderManager.TryGetNonLiveOrder(parsedId, out NonLiveOrder nonLiveOrderFromUniqueId))
			{
				GeneralFunctionality.SettingNonLiveOrderToCompleteOrInComplete(helpers, nonLiveOrderFromUniqueId, currentUser);

				return true;
			}
			else if (convertedSplitTicketId.Length == 2 && helpers.NonLiveOrderManager.TryGetNonLiveOrder(convertedSplitTicketId[0], convertedSplitTicketId[1], out var nonLiveOrderFromTicketId))
			{
				GeneralFunctionality.SettingNonLiveOrderToCompleteOrInComplete(helpers, nonLiveOrderFromTicketId, currentUser);

				return true;
			}
			else
			{
				return false;
			}
		}
	}
}