namespace HandleOrderAction_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Status;

	public class BookEventLevelReceptionServicesAction : OrderAction
	{
		public BookEventLevelReceptionServicesAction(Helpers helpers, Guid orderId) : base(helpers, orderId)
		{

		}

		public override void Execute()
		{
			if (!TryBookEventLevelReceptionServices(out var errorMessage))
			{
				Log("Execute", "Update order manager element book event level reception service status with failure");
				helpers.OrderManagerElement.UpdateBookEventLevelReceptionServiceStatus(new Order { Id = orderId }, false, errorMessage);
			}
			else
			{
				Log("Execute", "Update order manager element book event level reception service status with success");
				helpers.OrderManagerElement.UpdateBookEventLevelReceptionServiceStatus(new Order { Id = orderId }, true);
			}
		}

		public override void HandleException(string errorMessage)
		{
			SendLiveOrderServicesBookedErrorNotification(errorMessage);

			if (errorMessage == LockNotGrantedException.DefaultMessage)
			{
				Log(nameof(HandleException), "Update order manager element book service status with failure");
				helpers.OrderManagerElement.UpdateBookEventLevelReceptionServiceStatus(new Order { Id = orderId }, false, errorMessage);
			}
		}

		private bool TryBookEventLevelReceptionServices(out string errorMessage)
		{
			errorMessage = "";

			if (!TryGetOrder(out var order, out errorMessage))
			{
				Log(nameof(TryBookEventLevelReceptionServices), errorMessage);
				SendLiveOrderServicesBookedErrorNotification(errorMessage);
				return false;
			}

			UpdateResult result = null;
			try
			{
				result = order.BookEventLevelReceptionServices(helpers);
			}
			catch (Exception e)
			{
				errorMessage = $"Exception executing book event level reception services: {e}";
				SendLiveOrderServicesBookedErrorNotification(errorMessage, order);
				return false;
			}

			if (result.UpdateWasSuccessful)
			{
				Log(nameof(TryBookEventLevelReceptionServices), "Event level reception services successfully booked (no tasks failed)");
				SendLiveOrderServicesBookedNotification(order, result.Tasks);
				return true;
			}

			RollBackFailedBlockingTasks(result.Tasks);

			var failedTaskCount = result.Tasks.Count(t => t.Status == Status.Fail);
			var exceptions = new StringBuilder($"Event level reception services not successfully booked ({failedTaskCount}/{result.Tasks.Count} tasks failed):");
			foreach (var exception in result.Exceptions)
			{
				exceptions.AppendLine(exception.ToString());
			}

			errorMessage = exceptions.ToString();

			SendLiveOrderServicesBookedErrorNotification(errorMessage, order);
			return false;
		}
	}
}