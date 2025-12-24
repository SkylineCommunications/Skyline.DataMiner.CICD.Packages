namespace BookMultipleOrders_2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using OrderStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status;
	using Status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Status;

	public class BookMultipleOrdersDialog : LoadingDialog
	{
		private readonly List<Guid> orderGuids = new List<Guid>();
		private readonly List<Order> ordersToBook = new List<Order>();
		private readonly List<Order> ordersThatShouldNotBeBooked = new List<Order>();
		private readonly List<Order> ordersThatWereSuccessfullyBooked = new List<Order>();
		private readonly List<Order> ordersThatFailedToGetBooked = new List<Order>();

		public BookMultipleOrdersDialog(Helpers helpers) : base(helpers)
		{
		}

		protected override void GetScriptInput()
		{
			string scriptParameter = Engine.GetScriptParam("OrderIds").Value;

			foreach (var orderId in scriptParameter.Split(','))
			{
				if (Guid.TryParse(orderId, out var orderGuid))
				{
					orderGuids.Add(orderGuid);
				}
				else
				{
					Log(nameof(GetScriptInput), $"Unable to parse {orderId} to a Guid");
				}
			}
		}

		protected override void CollectActions()
		{
			methodsToExecute.Add(GetOrdersAndLocks);
			methodsToExecute.Add(StartChangeTracking);
			methodsToExecute.Add(GetUserInfo);
			methodsToExecute.Add(BookOrders);
			methodsToExecute.Add(ShowResultsInUi);
		}

		private void StartChangeTracking()
		{
			ordersToBook.ForEach(o => o.AcceptChanges());
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while booking multiple orders [" + DateTime.Now + "]";

			string message = $"Orders: {string.Join(",", ordersToBook.Select(o => o.Name))}<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

			NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

			reportSuccessfullySentLabel.IsVisible = true;
		}

		private void GetOrdersAndLocks()
		{
			foreach (var orderGuid in orderGuids)
			{
				var getOrderTask = new GetOrderTask(Helpers, orderGuid);
				Tasks.Add(getOrderTask);
				getOrderTask.Execute();
				if (getOrderTask.Status == Status.Fail) continue;

				var getLockTask = new GetOrderLockTask(Helpers, getOrderTask.Order.Id);
				Tasks.Add(getLockTask);
				getLockTask.Execute();

				if (getLockTask.Status == Status.Fail) continue;

				if (getOrderTask.Order.Status == OrderStatus.Preliminary && getLockTask.LockInfo != null && getLockTask.LockInfo.IsLockGranted)
				{
					ordersToBook.Add(getOrderTask.Order);
				}
				else
				{
					ordersThatShouldNotBeBooked.Add(getOrderTask.Order);
				}
			}
		}

		private void GetUserInfo()
		{
			var getUserInfoTask = new GetBaseUserInfoTask(Helpers);
			Tasks.Add(getUserInfoTask);

			IsSuccessful &= getUserInfoTask.Execute();
			if (!IsSuccessful) return;

			UserInfo = getUserInfoTask.UserInfo;
		}

		private void BookOrders()
		{
			foreach (var order in ordersToBook)
			{
				Helpers.AddOrderReferencesForLogging(order.Id);

				if (UserInfo.IsMcrUser) order.Status = OrderStatus.Confirmed;
				else if (order.Status == OrderStatus.Preliminary || order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Rejected) order.Status = OrderStatus.Planned;
				else if (order.Status == OrderStatus.Confirmed) order.Status = OrderStatus.ChangeRequested;
				else
				{
					// No order status update required
				}

				if (!new OrderValidator(Helpers, order, UserInfo, Options.None).Validate())
				{
					ordersThatShouldNotBeBooked.Add(order);
					continue;
				}

				var updateResult = order.AddOrUpdate(Helpers, UserInfo.IsMcrUser);
				Helpers.LockManager.ReleaseOrderLock(order.Id);
				Tasks.AddRange(updateResult.Tasks);

				if (!updateResult.UpdateWasSuccessful)
				{
					ordersThatFailedToGetBooked.Add(order);
				}
				else
				{
					ordersThatWereSuccessfullyBooked.Add(order);
				}
			}
		}

		private void ShowResultsInUi()
		{
			var sb = new StringBuilder();

			if (ordersThatWereSuccessfullyBooked.Any())
			{
				sb.Append($"Orders {string.Join(", ", ordersThatWereSuccessfullyBooked.Select(o => o.Name))} were successfully booked. \n");
			}

			if (ordersThatFailedToGetBooked.Any())
			{
				sb.Append($"Orders {string.Join(", ", ordersThatFailedToGetBooked.Select(o => o.Name))} failed to get booked. \n");
			}

			if (ordersThatShouldNotBeBooked.Any())
			{
				sb.Append($"Orders {string.Join(", ", ordersThatShouldNotBeBooked.Select(o => o.Name))} are not Preliminary, not valid or couldn't get a valid lock \n");
			}

			PrepareUiForManualMessage(sb.ToString(), showSendReportButton: false, showExceptionWidgets: false);
		}
	}
}