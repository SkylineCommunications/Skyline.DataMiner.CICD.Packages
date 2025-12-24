namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Timers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class LoadAddOrUpdateEventDialog : LoadingDialog
	{
		private enum ScriptAction
		{
			Add,
			Update
		}

		private string jobId;
		private ScriptAction scriptAction;

		private Timer extendLocksTimer;
		private int attemptsExtendLocking = 1;

		public LoadAddOrUpdateEventDialog(Helpers helpers, Timer extendLocksTimer) : base(helpers)
		{
			this.extendLocksTimer = extendLocksTimer;
		}

		public Event Event { get; private set; }

		public Event EventDuplicate { get; private set; }

		public AddOrUpdateEventDialog AddOrUpdateEventDialog { get; private set; }

		protected override void GetScriptInput()
		{
			try
			{
				jobId = Engine.GetScriptParam("jobId").Value;
				scriptAction = string.IsNullOrWhiteSpace(jobId) ? ScriptAction.Add : ScriptAction.Update;
			}
			catch
			{
				scriptAction = ScriptAction.Add;
			}
		}

		protected override void CollectActions()
		{
			if (scriptAction == ScriptAction.Update)
			{
				methodsToExecute.Add(GetEvent);
				methodsToExecute.Add(VerifyEvent);
				methodsToExecute.Add(CloneEvent);
				methodsToExecute.Add(GetLock);
			}

			methodsToExecute.Add(GetUserInfo);
			methodsToExecute.Add(ConstructAddOrUpdateEventDialog);
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while loading Add or Update Event [" + DateTime.Now + "]";

			string message = $"Event: '{Event?.Name}'<br>Event ID: {Event?.Id}<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

			NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

			reportSuccessfullySentLabel.IsVisible = true;
		}

		private void GetEvent()
		{
			if (scriptAction == ScriptAction.Add) return;

			var getEventTask = new GetEventTask(Helpers, jobId);
			Tasks.Add(getEventTask);

			IsSuccessful &= getEventTask.Execute();
			if (!IsSuccessful) return;

			Event = getEventTask.Event;
		}

		private void VerifyEvent()
		{
			if (Event.Status == Status.Cancelled) PrepareUiForManualErrorMessage($"Event {Event.Name} is canceled and can't be edited anymore.");
		}

		private void CloneEvent()
		{
			EventDuplicate = (Event)Event.Clone();
		}

		private void GetLock()
		{
			if (scriptAction == ScriptAction.Add) return;

			var getEventLockTask = new GetEventLockTask(Helpers, Event);
			Tasks.Add(getEventLockTask);

			IsSuccessful &= getEventLockTask.Execute();
			if (!IsSuccessful) return;

			LockInfos.Clear();
			LockInfos.Add(getEventLockTask.LockInfo);

			if (LockInfos[0].IsLockGranted)
			{
				extendLocksTimer.Elapsed += ExtendLocksTimer_Elapsed;
				extendLocksTimer.Interval = LockInfos[0].ReleaseLocksAfter.TotalMilliseconds;
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

			if (Event != null)
			{
				var eventLockInfo = Helpers.LockManager.RequestEventLock(Event.Id, extendLock: true);
				if (eventLockInfo.LockUsername.Contains("error"))
				{
					Helpers.Log(nameof(LoadAddOrUpdateEventDialog), nameof(ExtendLocksTimer_Elapsed), $"Something went wrong when extending the event lock: {Event.Id}", Event.Name);
				}
			}

			attemptsExtendLocking++;
		}

		private void GetUserInfo()
		{
			var getUserInfoTask = new GetUserInfoTask(Helpers, Event);
			Tasks.Add(getUserInfoTask);

			IsSuccessful &= getUserInfoTask.Execute();
			if (!IsSuccessful) return;

			UserInfo = getUserInfoTask.UserInfo;
		}

		private void ConstructAddOrUpdateEventDialog()
		{
			var constructAddOrUpdateEventDialogTask = new ConstructAddOrUpdateEventDialogTask(Helpers, UserInfo, Event, LockInfos.SingleOrDefault());
			Tasks.Add(constructAddOrUpdateEventDialogTask);

			IsSuccessful &= constructAddOrUpdateEventDialogTask.Execute();
			if (!IsSuccessful) return;

			AddOrUpdateEventDialog = constructAddOrUpdateEventDialogTask.AddOrUpdateEventDialog;
		}
	}
}
