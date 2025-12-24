namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Comments;
	using Skyline.DataMiner.Net.Ticketing;
	using System;
	using System.Timers;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class LoadLiveOrderUpdateCommentsDialog : LoadingDialog
	{
		private string receivedId;

		private readonly System.Timers.Timer extendLocksTimer;
        private int attemptsExtendLocking = 1;

        public LoadLiveOrderUpdateCommentsDialog(Helpers helpers, System.Timers.Timer extendLocksTimer) : base(helpers)
		{
            this.extendLocksTimer = extendLocksTimer;
		}

		protected override void CollectActions()
		{
			methodsToExecute.Add(GetOrder);
			methodsToExecute.Add(GetLock);
			methodsToExecute.Add(GetUserInfo);
			methodsToExecute.Add(ConstructUpdateCommentsForm);
		}

		protected override void GetScriptInput()
		{
			receivedId = Engine.GetScriptParam("ID").Value;
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while loading Update Comments Form [" + DateTime.Now + "]";
			string message = $"Order: '{Order.Name}'<br>Order ID: {Order.Id}<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";
			NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);
			reportSuccessfullySentLabel.IsVisible = true;
		}

		private void GetOrder()
		{
			var getOrderTask = new GetOrderTask(Helpers, receivedId);
			Tasks.Add(getOrderTask);

			IsSuccessful &= getOrderTask.Execute();

			if (!IsSuccessful) return;

			Order = getOrderTask.Order;
		}

		private void GetLock()
		{
			if (Order != null)
			{
				var getLockTask = new GetOrderLockTask(Helpers, Order.Id);
				Tasks.Add(getLockTask);

				IsSuccessful &= getLockTask.Execute();

				LockInfo = getLockTask.LockInfo;
			}

            if (LockInfo.IsLockGranted)
            {
                extendLocksTimer.Elapsed += ExtendLocksTimer_Elapsed;
                extendLocksTimer.Interval = LockInfo.ReleaseLocksAfter.TotalMilliseconds;
                extendLocksTimer.AutoReset = true;
                extendLocksTimer.Enabled = true;
            }
        }

        private void ExtendLocksTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (attemptsExtendLocking >= 10)
            {
                extendLocksTimer.Stop();
                extendLocksTimer.Dispose();
                return;
            }

            if (Order != null)
            {
                var orderLockInfo = Helpers.LockManager.RequestOrderLock(Order.Id, true);
                if (orderLockInfo.LockUsername.Contains("error"))
                {
                    Helpers.Log(nameof(LoadLiveOrderUpdateCommentsDialog), nameof(ExtendLocksTimer_Elapsed), $"Something went wrong when extending the order lock: {Order.Id}", Order.Name);
                }
            }

            attemptsExtendLocking++;
        }

        private void GetUserInfo()
		{
			var getUserInfoTask = new GetUserInfoTask(Helpers, null);
			Tasks.Add(getUserInfoTask);

			IsSuccessful &= getUserInfoTask.Execute();
			if (!IsSuccessful) return;

			UserInfo = getUserInfoTask.UserInfo;
		}

		private void ConstructUpdateCommentsForm()
		{
			var constructDialogTask = new ConstructUpdateCommentsDialogTask(Helpers, UserInfo, Order, LockInfo);
			Tasks.Add(constructDialogTask);

			IsSuccessful &= constructDialogTask.Execute();
			if (!IsSuccessful) return;

			UpdateCommentsDialog = constructDialogTask.UpdateCommentsDialog;
		}

		public UpdateCommentsDialog UpdateCommentsDialog { get; private set; }

		public YLE.Order.Order Order {get; private set; }

		public LockInfo LockInfo { get; private set; }
	}
}
