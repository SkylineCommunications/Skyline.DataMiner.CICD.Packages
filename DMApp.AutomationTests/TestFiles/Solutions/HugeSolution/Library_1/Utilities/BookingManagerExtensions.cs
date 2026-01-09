namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;

	public static class BookingManagerExtensions
	{
		public static int GetApplicationServiceViewId(this BookingManager bookingManager, IEngine engine)
		{
			var viewInfoMessage = new GetInfoMessage(InfoType.ViewInfo);
			var responses = engine.SendSLNetMessage(viewInfoMessage);
			foreach (var response in responses)
			{
				var viewInfo = (ViewInfoEventMessage)response;
				if (viewInfo == null) continue;

				if (viewInfo.Name == bookingManager.ApplicationServicesView) return viewInfo.ID;
			}

			return 0;
		}
	}
}