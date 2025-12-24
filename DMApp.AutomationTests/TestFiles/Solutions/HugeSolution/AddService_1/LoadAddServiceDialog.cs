namespace AddService_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Timers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ServiceTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class LoadAddServiceDialog : LoadingDialog
	{
		private string serviceId;
		private Service service;

		private readonly Timer extendLocksTimer;
		private int attemptsExtendLocking = 1;

		public LoadAddServiceDialog(Helpers helpers, Timer extendLocksTimer) : base(helpers)
		{
			this.extendLocksTimer = extendLocksTimer;
		}

		public Order Order { get; private set; }

		public LiveOrderFormDialog EditOrderDialog { get; private set; }

		public EditSharedSourceDialog EditSharedSourceDialog { get; private set; }

		public LiveOrderFormAction ScriptAction { get; private set; }

		protected override void GetScriptInput()
		{
			serviceId = Helpers.Engine.GetScriptParam("ServiceId").Value;
			ScriptAction = Helpers.Engine.GetScriptParam("Action").Value.GetEnumValue<LiveOrderFormAction>();
		}

		protected override void CollectActions()
		{
			methodsToExecute.Add(GetService);
			methodsToExecute.Add(InitializeOrderLogger);
			methodsToExecute.Add(GetOrder);
			methodsToExecute.Add(GetLockInfo);
			methodsToExecute.Add(GetUserInfo);
			methodsToExecute.Add(ConstructForm);
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while loading Update Service [" + DateTime.Now + "]";

			string message = $"Order(s): '{Order.Name}'<br>Order ID(s): {Order.Id}<br>Service: '{service?.Name}'<br>Service ID: {serviceId}<br>User: {Helpers.Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

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

		private void InitializeOrderLogger()
		{
			Helpers.AddOrderReferencesForLogging(service.OrderReferences.ToArray());
		}

		private void GetOrder()
		{
			if (service.TryGetLinkedOrders(Helpers, out List<Order> linkedOrders) && linkedOrders.Count == 1)
			{
				// there can only be 1 order linked to a Destination or Transmission
				Order = linkedOrders.Single();

				Order.AcceptChanges();
			}
			else
			{
				IsSuccessful &= false;
			}

			if (Order.Subtype == OrderSubType.Vizrem)
			{
				PrepareUiForManualMessage("Editing Vizrem orders is currently not supported.");
				IsSuccessful = false;
			}
		}

		private void GetLockInfo()
		{
			var getLockInfoTask = new GetOrderLockTask(Helpers, Order.Id);
			Tasks.Add(getLockInfoTask);

			IsSuccessful &= getLockInfoTask.Execute();

			if (!IsSuccessful) return;

			LockInfos.Add(getLockInfoTask.LockInfo);

			if (LockInfos.Single().IsLockGranted)
			{
				extendLocksTimer.Elapsed += ExtendLocksTimer_Elapsed;
				extendLocksTimer.Interval = LockInfos.Single().ReleaseLocksAfter.TotalMilliseconds;
				extendLocksTimer.AutoReset = true;
				extendLocksTimer.Enabled = true;
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
			var constructAddServiceDialogTask = new ConstructLiveOrderFormTask(Helpers, Order, null, LockInfos.Single(), UserInfo, ScriptAction);
			Tasks.Add(constructAddServiceDialogTask);

			IsSuccessful &= constructAddServiceDialogTask.Execute();
			if (!IsSuccessful) return;

			EditOrderDialog = constructAddServiceDialogTask.EditOrderDialog;
		}

		private void ExtendLocksTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (attemptsExtendLocking >= 10)
			{
				extendLocksTimer.Stop();
				extendLocksTimer.Dispose();
				return;
			}

			var orderLockInfo = Helpers.LockManager.RequestOrderLock(Order.Id, extendLock: true);
			if (orderLockInfo.LockUsername.Contains("error"))
			{
				Helpers.Log(nameof(LoadAddServiceDialog), nameof(ExtendLocksTimer_Elapsed), $"Something went wrong when extending the order lock: {Order.Id}", Order.Name);
			}	

			attemptsExtendLocking++;
		}
	}
}