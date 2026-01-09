namespace HandleOrderAction_1
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Status;

	public abstract class OrderAction
	{
		protected readonly Helpers helpers;
		protected readonly Guid orderId;

		protected OrderAction(Helpers helpers, Guid orderId)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.orderId = orderId;
		}

		public abstract void Execute();

		//public abstract void HandleLockFailure();

		public abstract void HandleException(string errorMessage);

		protected bool TryGetOrder(out Order order, out string errorMessage)
		{
			order = null;
			errorMessage = string.Empty;

			try
			{
				order = helpers.OrderManager.GetOrder(orderId);

				errorMessage = order != null ? string.Empty : $"Order with ID {orderId} could not be found";

				return order != null;
			}
			catch (Exception e)
			{
				errorMessage = $"Exception while retrieving order with ID {orderId}: {e}";
				return false;
			}
		}

		protected bool TryGetService(Guid serviceId, out Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service service, out string errorMessage)
		{
			service = null;
			errorMessage = "";

			try
			{
				service = helpers.ServiceManager.GetService(serviceId);
				if (service == null)
				{
					errorMessage = "Service could not be retrieved";

					return false;
				}

				return true;
			}
			catch (Exception e)
			{
				errorMessage = $"Exception retrieving service: {e}";

				return false;
			}
		}		

		protected void RollBackFailedBlockingTasks(List<Task> tasks)
		{
			try
			{
				foreach (var failedTask in tasks.Where(t => t.Status == Status.Fail))
				{
					Log("RollBackFailedBlockingTasks", $"Task {failedTask.Description} failed: {failedTask.Exception}");
				}

				var shouldRollBack = tasks.Any(t => t.Status == Status.Fail && t.IsBlocking);
				if (!shouldRollBack) return;
				
				Log("RollBackFailedBlockingTasks", "Tasks will be rolled back because some blocking tasks failed");

				var rollbackTasks = tasks.Where(t => t.Status == Status.Ok).Select(t => t.CreateRollbackTask()).Where(t => t != null).Reverse().ToList();
				foreach (var rollbackTask in rollbackTasks)
				{
					if (!rollbackTask.Execute()) Log("RollBackFailedBlockingTasks", $"(BookServices) Rolling back {rollbackTask.Description} failed: {rollbackTask.Exception.ToString()}");
				}
			}
			catch (Exception e)
			{
				Log("RollBackFailedBlockingTasks", $"Exception rolling back book event level reception services: {e}");
			}
		}

		protected void SendLiveOrderServicesBookedNotification(Order order, List<Task> tasks)
		{
			var message = new StringBuilder();

			var blockingTaskFailed = false;
			var nonBlockingTaskFailed = false;
			foreach (var task in tasks)
			{
				if (task.Status == Status.Fail)
				{
					if (task.IsBlocking) blockingTaskFailed = true;
					else nonBlockingTaskFailed = true;
				}

				//message.AppendLine(String.Format("Task '{0}' {1}", task.Description, task.Status == Tasks.Status.Ok ? "Succeeded" : "Failed"));
			}

			if (blockingTaskFailed) message.Insert(0, "Order services could not successfully be booked (some blocking tasks failed) and all changes were rolled back.");
			else if (nonBlockingTaskFailed) message.Insert(0, "Order services could not be successfully booked (some non-blocking tasks failed).");
			else message.Insert(0, "Order services were successfully booked.");

			var successful = !blockingTaskFailed && !nonBlockingTaskFailed;
			SendLiveOrderServicesBookedNotification(order, successful, message.ToString());
		}

		protected void SendLiveOrderServicesBookedNotification(Order order, bool successful, string message)
		{
			Stopwatch stopwatch = LogMethodStart(nameof(SendLiveOrderServicesBookedNotification));

			try
			{
				NotificationManager.SendLiveOrderServicesBookedMail(helpers, order, successful, message);
				Log("SendLiveOrderServicesBookedNotification", "Live order booked services notification sent");
			}
			catch (Exception e)
			{
				Log("SendLiveOrderServicesBookedNotification", $"Exception sending live order booked services notification: {e}");
			}

			LogMethodCompleted(nameof(SendLiveOrderServicesBookedNotification), stopwatch);
		}

		protected void SendLiveOrderServicesBookedErrorNotification(string message, Order order = null)
		{
			Stopwatch stopwatch = LogMethodStart(nameof(SendLiveOrderServicesBookedErrorNotification));

			try
			{
				NotificationManager.SendLiveOrderServicesBookedMail(helpers, order, false, message);
				Log(nameof(SendLiveOrderServicesBookedErrorNotification), "Live order booked services notification sent");
			}
			catch (Exception e)
			{
				Log(nameof(SendLiveOrderServicesBookedErrorNotification), $"Exception sending live order booked services notification: {e}");
			}

			LogMethodCompleted(nameof(SendLiveOrderServicesBookedErrorNotification), stopwatch);
		}

		protected void Log(string nameOfMethod, string message)
		{
			helpers.Log(nameof(OrderAction),nameOfMethod, message);
		}

		protected Stopwatch LogMethodStart(string nameOfMethod, string nameOfObject = null)
		{
			helpers.LogMethodStart(nameof(OrderAction), nameOfMethod, out Stopwatch stopwatch, nameOfObject);
			return stopwatch;
		}

		protected void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch)
		{
			helpers.LogMethodCompleted(nameof(OrderAction), nameOfMethod, null, stopwatch);
		}
	}
}