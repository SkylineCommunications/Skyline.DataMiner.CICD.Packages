namespace CancelMultipleOrders_2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using OrderStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status;
	using Status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Status;

	public class CancelMultipleOrdersProgressDialog : LoadingDialog
	{
		private readonly string reasonForCancellation;
		private readonly List<Guid> orderGuids = new List<Guid>();
		private readonly List<Order> retrievedOrders = new List<Order>();
		private readonly List<Guid> orderIdsThatFailedToBeRetrieved = new List<Guid>();
		private readonly List<Order> ordersToCancel = new List<Order>();
		private readonly List<Order> ordersThatFailedToBeCancelled = new List<Order>();
		private readonly List<Order> lockedOrders = new List<Order>();
		private readonly List<Order> ordersThatCantBeCancelled = new List<Order>();
		private readonly List<Order> ordersThatSucceededToBeCancelled = new List<Order>();

		public CancelMultipleOrdersProgressDialog(Helpers helpers, string reasonForCancellation) : base(helpers)
		{
			this.reasonForCancellation = reasonForCancellation;
		}

		protected override void GetScriptInput()
		{
			string scriptParameter = Engine.GetScriptParam("OrderIds").Value;

			foreach (var orderId in scriptParameter.Split(','))
			{
				if (Guid.TryParse(orderId, out var orderGuid))
				{
					orderGuids.Add(orderGuid);
					Helpers.AddOrderReferencesForLogging(orderGuid);
				}
				else
				{
					Engine.Log($"Run|Unable to parse {orderId} to a Guid");
				}
			}
		}

		protected override void CollectActions()
		{
			methodsToExecute.Add(InitializeOrderLogger);
			methodsToExecute.Add(GetOrders);
			methodsToExecute.Add(GetLocks);
			methodsToExecute.Add(CancelOrders);
			methodsToExecute.Add(ShowResultsInUi);
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while canceling multiple orders [" + DateTime.Now + "]";

			string message = $"Orders: {string.Join(",", ordersToCancel.Select(o => o.Name))}<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

			NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

			reportSuccessfullySentLabel.IsVisible = true;
		}

		private void InitializeOrderLogger()
		{
			Helpers.Log(nameof(CancelMultipleOrdersProgressDialog), "START SCRIPT", "Cancel Multiple Orders");
		}

		private void GetOrders()
		{
			LogActionStart(nameof(GetOrders), out var stopWatch);

			foreach (var orderGuid in orderGuids)
			{
				var getOrderTask = new GetOrderTask(Helpers, orderGuid);
				Tasks.Add(getOrderTask);

				IsSuccessful &= getOrderTask.Execute();
				if (IsSuccessful)
				{
					retrievedOrders.Add(getOrderTask.Order);
				}
				else
				{
					orderIdsThatFailedToBeRetrieved.Add(orderGuid);
				}
			}

			Log(nameof(GetOrders), $"Unable to retrieve orders {string.Join(", ", orderIdsThatFailedToBeRetrieved)}");

			LogActionCompleted(nameof(GetOrders), stopWatch);
		}

		private void GetLocks()
		{
			LogActionStart(nameof(GetLocks), out var stopWatch);

			foreach (var order in retrievedOrders)
			{
				var getLockTask = new GetOrderLockTask(Helpers, order.Id);
				Tasks.Add(getLockTask);

				IsSuccessful &= getLockTask.Execute();
				if (!IsSuccessful) return;

				if (getLockTask.Status == Status.Fail) continue;

				LockInfos.Add(getLockTask.LockInfo);

				if (!order.CanCancel)
				{
					ordersThatCantBeCancelled.Add(order);
				}
				else if (!getLockTask.LockInfo.IsLockGranted)
				{
					lockedOrders.Add(order);
				}
				else
				{
					ordersToCancel.Add(order);
				}
			}

			LogActionCompleted(nameof(GetLocks), stopWatch);
		}

		private void CancelOrders()
		{
			LogActionStart(nameof(CancelOrders), out var stopWatch);

			foreach (var order in ordersToCancel)
			{
				order.ReasonForCancellationOrRejection = reasonForCancellation;
				var updateOrderStatusTask = new UpdateOrderStatusTask(Helpers, order, OrderStatus.Cancelled);
				Tasks.Add(updateOrderStatusTask);
				updateOrderStatusTask.Execute();

				Helpers.LockManager.ReleaseOrderLock(order.Id);

				if (updateOrderStatusTask.Status == Status.Fail)
				{
					ordersThatFailedToBeCancelled.Add(order);
				}
				else
				{
					ordersThatSucceededToBeCancelled.Add(order);
				}
			}

			LogActionCompleted(nameof(CancelOrders), stopWatch);
		}

		private void ShowResultsInUi()
		{
			var sb = new StringBuilder();

			if (ordersThatSucceededToBeCancelled.Any())
			{
				sb.AppendLine($"Order{(lockedOrders.Count > 1 ? "s" : String.Empty)} {string.Join(", ", ordersThatSucceededToBeCancelled.Select(o => o.Name))} {(lockedOrders.Count > 1 ? "were" : "was")} successfully canceled.");
			}

			if (orderIdsThatFailedToBeRetrieved.Any())
			{
				sb.AppendLine($"Some orders could not be retrieved.");
			}

			if (ordersThatFailedToBeCancelled.Any())
			{
				sb.AppendLine($"Orders {string.Join(", ", ordersThatFailedToBeCancelled.Select(o => o.Name))} failed to get canceled.");
			}

			foreach (var order in ordersThatCantBeCancelled)
			{
				sb.AppendLine($"Order {order.Name} can't be canceled because is has state {order.Status.GetDescription()}");
			}

			if (lockedOrders.Any())
			{
				sb.AppendLine($"Order{(lockedOrders.Count > 1 ? "s" : String.Empty)} {string.Join(", ", lockedOrders.Select(o => o.Name))} couldn't be canceled because {(lockedOrders.Count > 1 ? "they are" : "it is")} locked.");
			}

			PrepareUiForManualMessage(sb.ToString(), showSendReportButton: false, showExceptionWidgets: false);
		}
	}
}