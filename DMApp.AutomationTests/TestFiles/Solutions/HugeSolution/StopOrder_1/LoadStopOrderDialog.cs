namespace StopOrder
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Timers;
	using Library.Utilities.EqualityComparers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using StopOrder.Dialogs;

	public class LoadStopOrderDialog : LoadingDialog
	{
		private string serviceId;
		private Service service;

		private readonly Timer extendLocksTimer;
		private int attemptsExtendLocking = 1;

		public LoadStopOrderDialog(Helpers helpers, Timer extendLocksTimer) : base(helpers)
		{
			this.extendLocksTimer = extendLocksTimer;
		}

		public List<Order> Orders { get; private set; } = new List<Order>();

		public ConfirmStopDialog ConfirmStopOrderDialog { get; private set; }

		protected override void GetScriptInput()
		{
			serviceId = Engine.GetScriptParam("ServiceId").Value;
		}

		protected override void CollectActions()
		{
			methodsToExecute.Add(GetService);
			methodsToExecute.Add(VerifyService);
			methodsToExecute.Add(InitializeOrderLogger);
			methodsToExecute.Add(GetOrders);
			methodsToExecute.Add(GetUserInfo);
			methodsToExecute.Add(ConstructForm);
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while loading stop order dialog [" + DateTime.Now + "]";

			string message = $"Order(s): '{String.Join(", ", Orders.Select(x => x?.Name))}'<br>Order ID(s): {String.Join(", ", Orders.Select(x => x?.Id))}<br>Service: '{service?.Name}'<br>Service ID: {serviceId}<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

			NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

			reportSuccessfullySentLabel.IsVisible = true;
		}

		private void GetService()
		{
			var getServiceTask = new GetServiceTask(Helpers, serviceId);
			Tasks.Add(getServiceTask);

			IsSuccessful &= getServiceTask.Execute();

			if (!IsSuccessful) return;

			service = getServiceTask.Service;
		}

		private void VerifyService()
		{
			if (!service.IsSharedSource && service.OrderReferences.Count > 1)
			{
				PrepareUiForManualErrorMessage("This service is unexpectedly part of multiple orders. Process is unable to proceed.");
			}
		}

		private void InitializeOrderLogger()
		{
			Helpers.AddOrderReferencesForLogging(service.OrderReferences.ToArray());
		}

		private void GetOrders()
		{
			Orders.Clear();
			if (service.TryGetLinkedOrders(Helpers, out List<Order> linkedOrders))
			{
				foreach (var linkedOrder in linkedOrders) Orders.Add(linkedOrder);
			}
			else
			{
				IsSuccessful &= false;
			}

			if (Orders.Any(o => o.Subtype == OrderSubType.Vizrem))
			{
				PrepareUiForManualMessage("Editing Vizrem orders is currently not supported.");
				IsSuccessful = false;
			}
		}

		private void GetUserInfo()
		{
			var getUserInfoTask = new GetUserInfoTask(Helpers);
			Tasks.Add(getUserInfoTask);

			IsSuccessful &= getUserInfoTask.Execute();

			if (!IsSuccessful) return;

			UserInfo = getUserInfoTask.UserInfo;
		}

		private void ConstructForm()
		{
			ConstructStopOrderDialog();
		}

		private void ConstructStopOrderDialog()
		{
			var constructStopOrderDialogTask = new ConstructStopOrderDialogTask(Helpers);
			Tasks.Add(constructStopOrderDialogTask);

			IsSuccessful &= constructStopOrderDialogTask.Execute();
			if (!IsSuccessful) return;

			ConfirmStopOrderDialog = constructStopOrderDialogTask.ConfirmStopDialog;
		}

		private void ExtendLocksTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (attemptsExtendLocking >= 10)
			{
				extendLocksTimer.Stop();
				extendLocksTimer.Dispose();
				return;
			}

			foreach (var order in Orders)
			{
				var orderLockInfo = Helpers.LockManager.RequestOrderLock(order.Id, extendLock: true);
				if (orderLockInfo.LockUsername.Contains("error"))
				{
					Helpers.Log(nameof(LoadStopOrderDialog), nameof(ExtendLocksTimer_Elapsed), $"Something went wrong when extending the order lock: {order.Id}", order.Name);
				}
			}

			attemptsExtendLocking++;
		}
	}
}