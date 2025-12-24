namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
    using System.Timers;
    using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.Utils.YLE.Integrations;

    public class LoadMergeEventsDialog : LoadingDialog
	{
        private const string MergeEventsLoggingFileName = "MergeEventsLogging";

        private string[] eventIds;

		private readonly Helpers helpers;
		private readonly List<Event> events = new List<Event>();
		private readonly List<LiteOrder> orders = new List<LiteOrder>();

        private Timer extendLocksTimer;
        private int attemptsExtendLocking = 1;

        public LoadMergeEventsDialog(Helpers helpers, Timer extendLocksTimer) : base(helpers)
		{
			this.helpers = helpers;
            this.extendLocksTimer = extendLocksTimer;
		}

		public MergeEventsDialog MergeEventsDialog { get; private set; }

        protected override void GetScriptInput()
		{
			var eventIdsScriptParameter = Engine.GetScriptParam("EventIds");
			eventIds = eventIdsScriptParameter.Value.Split(',');
		}

		protected override void CollectActions()
		{
			methodsToExecute.Add(InitializeOrderLogger);
			methodsToExecute.Add(GetEvents);
			methodsToExecute.Add(ValidateEvents);
			methodsToExecute.Add(GetOrders);
			methodsToExecute.Add(GetLocks);
			methodsToExecute.Add(ConstructMergeEventsDialog);
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while loading Merge Events [" + DateTime.Now + "]";

			string message = $"<br>Events: <br>'{string.Join("<br>", events.Select(ev => ev.Name + " [" + ev.Id + "]"))}'<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

			NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

			reportSuccessfullySentLabel.IsVisible = true;
		}

        private void InitializeOrderLogger()
        {
            Helpers.Log(nameof(LoadMergeEventsDialog), "START SCRIPT", "MERGE EVENTS");
        }

        private void GetEvents()
		{
			foreach (var eventId in eventIds)
			{
				var getEventTask = new GetEventTask(Helpers, eventId);
				Tasks.Add(getEventTask);

				IsSuccessful &= getEventTask.Execute();
				if (!IsSuccessful) return;

				events.Add(getEventTask.Event);
			}
		}

		private void ValidateEvents()
		{
			bool multipleCeitonEvents = events.Count(e => e.IntegrationType == IntegrationType.Ceiton) > 1;

			if (multipleCeitonEvents)
			{
				PrepareUiForManualErrorMessage($"Unable to merge the events as the following events were created by a Ceiton integration:\n{String.Join(Environment.NewLine, events.Where(e => e.IntegrationType == IntegrationType.Ceiton).Select(x => "\t- " + x.Name))}");
			}
			else if (events.Count < 2)
			{
				PrepareUiForManualErrorMessage("Merging events is only possible for two or more events. Make sure that the events with the provided IDs still exist.");
			}
		}

		private void GetOrders()
		{
			foreach (var @event in events)
			{
				foreach (var eventOrderId in @event.OrderIds)
				{
					bool orderIsIntegration = @event.OrderIsIntegrations[eventOrderId];
					if (orderIsIntegration) continue;

					var getOrderTask = new GetLiteOrderTask(Helpers, eventOrderId);
					Tasks.Add(getOrderTask);

					IsSuccessful &= getOrderTask.Execute();
					if (!IsSuccessful) return;

					orders.Add(getOrderTask.LiteOrder);
				}
			}
		}

		private void GetLocks()
		{
			foreach (var @event in events)
			{
				var getLockTask = new GetEventLockTask(Helpers, @event);
				Tasks.Add(getLockTask);

				IsSuccessful &= getLockTask.Execute();
				if (!IsSuccessful) return;

				LockInfos.Add(getLockTask.LockInfo);
			}

			foreach (var order in orders)
			{
				var getLockTask = new GetOrderLockTask(Helpers, order.Id);
				Tasks.Add(getLockTask);

				IsSuccessful &= getLockTask.Execute();
				if (!IsSuccessful) return;

				LockInfos.Add(getLockTask.LockInfo);
			}

            if (LockInfos.Any() && LockInfos.All(x => x.IsLockGranted))
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

            foreach (var order in orders)
            {
                if (order != null)
                {
                    var orderLockInfo = Helpers.LockManager.RequestOrderLock(order.Id, true);
                    if (orderLockInfo.LockUsername.Contains("error"))
                    {
						helpers.Log(nameof(LoadMergeEventsDialog), nameof(ExtendLocksTimer_Elapsed), $"Something went wrong when extending the order lock: {order.Id}", order.Name);
                    }
                }
            }

            foreach (var @event in events)
            {
                if (@event != null)
                {
                    var eventLockInfo = Helpers.LockManager.RequestEventLock(@event.Id, extendLock: true);
                    if (eventLockInfo.LockUsername.Contains("error"))
                    {
						helpers.Log(nameof(LoadMergeEventsDialog), nameof(ExtendLocksTimer_Elapsed), $"Something went wrong when extending the event lock: {@event.Id}", @event.Name);
                    }
                }
            }

            attemptsExtendLocking++;
        }

        private void ConstructMergeEventsDialog()
		{
			var constructMergeEventsDialogTask = new ConstructMergeEventsDialogTask(Helpers, events, orders);
			Tasks.Add(constructMergeEventsDialogTask);

			IsSuccessful &= constructMergeEventsDialogTask.Execute();
			if (!IsSuccessful)
			{
				// calling roll back method to avoid errors by library not including this method as it wasn't used
				// this method doesn't do anything actually
				//constructMergeEventsDialogTask.CreateRollbackTask();

				return;
			}

			MergeEventsDialog = constructMergeEventsDialogTask.MergeEventsDialog;
		}
	}
}
