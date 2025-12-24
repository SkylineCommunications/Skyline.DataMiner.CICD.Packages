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

	public class BookServicesAction : OrderAction
	{
		public BookServicesAction(Helpers helpers, Guid orderId) : base(helpers, orderId)
		{

		}

		public override void Execute()
		{
			var stopwatch = LogMethodStart(nameof(Execute), nameof(BookServicesAction));

			if (!TryBookServices(out var errorMessage))
			{
				Log(nameof(Execute), errorMessage);
				Log(nameof(Execute), "Update order manager element book service status with failure");
				helpers.OrderManagerElement.UpdateBookServiceStatus(new Order { Id = orderId }, false, errorMessage);
			}
			else
			{
				Log(nameof(Execute), "Update order manager element book service status with success");
				helpers.OrderManagerElement.UpdateBookServiceStatus(new Order { Id = orderId }, true);
			}

			LogMethodCompleted(nameof(Execute), stopwatch);
		}

		public override void HandleException(string errorMessage)
		{
			SendLiveOrderServicesBookedErrorNotification(errorMessage);

			if (errorMessage == LockNotGrantedException.DefaultMessage)
			{
				Log(nameof(HandleException), "Update order manager element book service status with failure");
				helpers.OrderManagerElement.UpdateBookServiceStatus(new Order { Id = orderId }, false, errorMessage);
			}
		}

		private bool TryBookServices(out string errorMessage)
		{
			errorMessage = "";

			if (!TryGetOrder(out var order, out errorMessage))
			{
				Log(nameof(TryBookServices), errorMessage);
				SendLiveOrderServicesBookedErrorNotification(errorMessage);
				return false;
			}

			UpdateResult result = null;
			try
			{
				result = order.BookServices(helpers);
			}
			catch (Exception e)
			{
				errorMessage = $"Exception executing book services: {e}";
				SendLiveOrderServicesBookedErrorNotification(errorMessage, order);
				return false;
			}

			if (result.UpdateWasSuccessful)
			{
				Log("TryBookServices", "Services successfully booked (no tasks failed)");
				SendLiveOrderServicesBookedNotification(order, result.Tasks);
				return true;
			}

			RollBackFailedBlockingTasks(result.Tasks);

			var failedTaskCount = result.Tasks.Count(t => t.Status == Status.Fail);
			var exceptions = new StringBuilder($"Services not successfully booked ({failedTaskCount}/{result.Tasks.Count} tasks failed):");
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