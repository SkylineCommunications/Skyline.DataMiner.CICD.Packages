namespace LiveOrderForm_6.Dialogs
{
	using System;
	using LoadLiveOrderFormTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class LoadLiveOrderFormDialogAfterOtherDialog : LoadingDialog
	{
		private readonly Order order;
		private readonly LockInfo lockInfo;
		private readonly UserInfo userInfo;
		private readonly LiveOrderFormAction scriptAction;

		public LoadLiveOrderFormDialogAfterOtherDialog(Helpers helpers, Order order, LockInfo lockInfo , UserInfo userInfo, LiveOrderFormAction scriptAction) : base(helpers)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.lockInfo = lockInfo;
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.scriptAction = scriptAction;
		}

		public LiveOrderFormDialog EditOrderDialog { get; private set; }

		protected override void GetScriptInput()
		{
			// not applicable
		}

		protected override void CollectActions()
		{
			methodsToExecute.Add(ConstructLiveOrderForm);
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while loading Live Order Form after Merge or Duplicate [" + DateTime.Now + "]";

			string message = $"Orders: '{order?.Name}<br>Event: {order?.Event?.Name}'<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

			NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

			reportSuccessfullySentLabel.IsVisible = true;
		}

		private void ConstructLiveOrderForm()
		{
			var constructLiveOrderFormTask = new ConstructLiveOrderFormTask(Helpers, order, order.Event, lockInfo, userInfo, scriptAction);
			Tasks.Add(constructLiveOrderFormTask);

			IsSuccessful &= constructLiveOrderFormTask.Execute();
			if (!IsSuccessful) return;

			EditOrderDialog = constructLiveOrderFormTask.EditOrderDialog;
		}
	}
}