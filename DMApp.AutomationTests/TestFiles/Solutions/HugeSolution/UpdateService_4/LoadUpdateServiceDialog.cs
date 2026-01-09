namespace UpdateService_4
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Timers;
	using Library.Utilities.EqualityComparers;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reservations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class LoadUpdateServiceDialog : LoadingDialog
	{
		private string serviceId;
		private ReservationInstance serviceReservation;
		private readonly List<Order> orders = new List<Order>();

		private readonly Timer extendLocksTimer;
		private int attemptsExtendLocking = 1;

		public LoadUpdateServiceDialog(Helpers helpers, Timer extendLocksTimer) : base(helpers)
		{
			this.extendLocksTimer = extendLocksTimer;
		}

		public UpdateServiceDialog UpdateServiceDialog { get; private set; }

		public DeleteServiceDialog DeleteServiceDialog { get; private set; }

		public EditSharedSourceDialog EditSharedSourceDialog { get; private set; }

		public UseSharedSourceDialog UseSharedSourceDialog { get; private set; }

		public SelectOrderDialog SelectOrderDialog { get; private set; }

		public ScriptAction ScriptAction { get; private set; }

		public bool OrderSelectionRequired { get; private set; } = false;

		public bool IsSharedSource => serviceReservation?.GetBooleanProperty(ServicePropertyNames.IsGlobalEventLevelReceptionPropertyName) == true;

		protected override void GetScriptInput()
		{
			string serviceIdScriptParamValue = Engine.GetScriptParam("ServiceId").Value;
			if (serviceIdScriptParamValue.Contains("["))
			{
				serviceId = JsonConvert.DeserializeObject<List<string>>(serviceIdScriptParamValue)[0];
			}
			else
			{
				serviceId = serviceIdScriptParamValue;
			}

			ScriptAction = Engine.GetScriptParam("Action").Value.GetEnumValue<ScriptAction>();
		}

		protected override void CollectActions()
		{
			methodsToExecute.Add(GetService);
			methodsToExecute.Add(VerifyService);
			methodsToExecute.Add(InitializeOrderLogger);
			methodsToExecute.Add(GetOrders);

			if (ScriptAction != ScriptAction.View) methodsToExecute.Add(GetLockInfos);

			methodsToExecute.Add(GetUserInfo);
			methodsToExecute.Add(ConstructForm);
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while loading Update Service [" + DateTime.Now + "]";

			string message = $"Order(s): '{String.Join(", ", orders.Select(x => x?.Name))}'<br>Order ID(s): {String.Join(", ", orders.Select(x => x?.Id))}<br>Service: '{serviceReservation?.Name}'<br>Service ID: {serviceId}<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

			NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

			reportSuccessfullySentLabel.IsVisible = true;
		}

		private void GetService()
		{
			serviceReservation = Helpers.ResourceManager.GetReservationInstance(Guid.Parse(serviceId)) ?? throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceNotFoundException(serviceId);
		}

		private void VerifyService()
		{
			if (serviceReservation.GetStringProperty(LiteOrder.PropertyNameType) == "Video")
			{
				PrepareUiForManualErrorMessage("The ID passed to the script refers to an Order instead of a Service.");
			}

			var now = DateTime.Now;

			bool reservationIsPast = serviceReservation.End.FromReservation() < now;
			bool reservationIsOngoing = serviceReservation.Start.FromReservation() <= now && now <= serviceReservation.End.FromReservation();

			if (!IsSharedSource && serviceReservation.GetOrderReferences().Count > 1)
			{
				PrepareUiForManualErrorMessage("This service is unexpectedly part of multiple orders. Process is unable to proceed.");
			}
			else if (ScriptAction == ScriptAction.ResourceChange_FromRecordingApp)
			{
				if (reservationIsPast)
				{
					PrepareUiForManualErrorMessage("Not allowed to swap the resource of a recording in the past.", false);
				}
				else if (reservationIsOngoing)
				{
					PrepareUiForManualErrorMessage("Not allowed to swap the resource of an ongoing recording.", false);
				}

			}
			else if (ScriptAction == ScriptAction.UpdateTiming)
			{
				if (reservationIsPast)
				{
					PrepareUiForManualErrorMessage("Not allowed to edit timing of a recording in the past.", false);
				}
			}
			else if (ScriptAction == ScriptAction.Delete)
			{
				if (reservationIsPast)
				{
					PrepareUiForManualErrorMessage("Editing recordings in the past is not possible.", false);
				}
			}
			else
			{
				// do nothing
			}
		}

		private void InitializeOrderLogger()
		{
			Helpers.AddOrderReferencesForLogging(serviceReservation.GetOrderReferences().ToArray());
		}

		private void GetOrders()
		{
			orders.Clear();

			foreach (var orderId in serviceReservation.GetOrderReferences())
			{
				var getOrderTask = new GetOrderTask(Helpers, orderId);
				Tasks.Add(getOrderTask);

				IsSuccessful &= getOrderTask.Execute();

				if (IsSuccessful)
				{
					orders.Add(getOrderTask.Order);
				}
			}

			if (orders.Any(o => o.Subtype == OrderSubType.Vizrem))
			{
				PrepareUiForManualMessage("Editing Vizrem orders is currently not supported.");
				IsSuccessful = false;
			}
		}

		private void GetLockInfos()
		{
			LockInfos.Clear();
			foreach (var order in orders.Distinct(new OrderByIdEqualityComparer()))
			{
				var getLockInfoTask = new GetOrderLockTask(Helpers, order.Id);
				Tasks.Add(getLockInfoTask);

				IsSuccessful &= getLockInfoTask.Execute();

				if (!IsSuccessful) return;

				LockInfos.Add(getLockInfoTask.LockInfo);
			}

			if (LockInfos.Any() && LockInfos.All(x => x.IsLockGranted))
			{
				extendLocksTimer.Elapsed += ExtendLocksTimer_Elapsed;
				extendLocksTimer.Interval = LockInfos.First().ReleaseLocksAfter.TotalMilliseconds;
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
			switch (ScriptAction)
			{
				case ScriptAction.Edit:
					if (IsSharedSource) ConstructUpdateSharedSourceDialog();
					else ConstructUpdateServiceDialog(EditOrderFlows.EditService);
					break;

				case ScriptAction.View:
					ConstructUpdateServiceDialog(EditOrderFlows.ViewService);
					break;

				case ScriptAction.ResourceChange:
					if (IsSharedSource) ConstructResourceOnlyUpdateSharedSourceDialogTask();
					else ConstructUpdateServiceDialog(EditOrderFlows.ChangeResourcesForService);
					break;

				case ScriptAction.ResourceChange_FromRecordingApp:
					if (IsSharedSource) ConstructResourceOnlyUpdateSharedSourceDialogTask();
					else ConstructUpdateServiceDialog(EditOrderFlows.ChangeResourcesForService_FromRecordingApp);
					break;

				case ScriptAction.UpdateTiming:
					ConstructUpdateServiceDialog(EditOrderFlows.EditTimingForService_FromRecordingApp);
					break;
				case ScriptAction.Delete:
					if (LockInfos.Single().IsLockGranted) ConstructDeleteServiceDialog();
					else PrepareUiForManualErrorMessage($"Lock for this order is taken by {LockInfos.Single().LockUsername}.");
					break;
				case ScriptAction.UseSharedSource:
					if (orders.Count > 1)
					{
						OrderSelectionRequired = true;
						SelectOrderDialog = new SelectOrderDialog(Helpers.Engine, orders);
						SelectOrderDialog.ContinueButton.Pressed += (s, e) =>
						{
							orders.RemoveAll(x => !x.Id.Equals(SelectOrderDialog.SelectedOrder.Id));
							LockInfos.RemoveAll(x => !x.ObjectId.Equals(SelectOrderDialog.SelectedOrder.Id));
							ConstructUseSharedSourceDialog(EditOrderFlows.UseSharedSource);
						};
					}
					else
					{
						ConstructUseSharedSourceDialog(EditOrderFlows.UseSharedSource);
					}
					break;
				default:
					throw new ArgumentException("Invalid ScriptAction: " + ScriptAction);
			}
		}

		private void ConstructUpdateSharedSourceDialog()
		{
			var constructUpdateEventLevelReceptionsDialogTask = new ConstructUpdateSharedSourceDialogTask(Helpers, serviceReservation.ID, orders, LockInfos, UserInfo);
			Tasks.Add(constructUpdateEventLevelReceptionsDialogTask);

			IsSuccessful &= constructUpdateEventLevelReceptionsDialogTask.Execute();
			if (!IsSuccessful) return;

			EditSharedSourceDialog = constructUpdateEventLevelReceptionsDialogTask.EditSharedSourceDialog;
		}

		private void ConstructUpdateServiceDialog(EditOrderFlows flow)
		{
			var constructUpdateServiceDialog = new ConstructUpdateServiceDialogTask(Helpers, serviceReservation.ID, orders.Single(), LockInfos.SingleOrDefault(), UserInfo, flow);
			Tasks.Add(constructUpdateServiceDialog);

			IsSuccessful &= constructUpdateServiceDialog.Execute();
			if (!IsSuccessful) return;

			UpdateServiceDialog = constructUpdateServiceDialog.UpdateServiceDialog;
		}

		private void ConstructResourceOnlyUpdateSharedSourceDialogTask()
		{
			var constructResourceOnlyUpdateSharedSourceDialog = new ConstructResourceOnlyUpdateSharedSourceDialogTask(Helpers, serviceReservation.ID, orders, LockInfos, UserInfo);
			Tasks.Add(constructResourceOnlyUpdateSharedSourceDialog);

			IsSuccessful &= constructResourceOnlyUpdateSharedSourceDialog.Execute();
			if (!IsSuccessful) return;

			EditSharedSourceDialog = constructResourceOnlyUpdateSharedSourceDialog.EditSharedSourceDialog;
		}

		private void ConstructDeleteServiceDialog()
		{
			var constructDeleteServiceDialog = new ConstructDeleteServiceDialogTask(Helpers, serviceReservation.ID, orders.Single(), UserInfo);
			Tasks.Add(constructDeleteServiceDialog);

			IsSuccessful &= constructDeleteServiceDialog.Execute();
			if (!IsSuccessful) return;

			DeleteServiceDialog = constructDeleteServiceDialog.DeleteServiceDialog;
		}

		private void ConstructUseSharedSourceDialog(EditOrderFlows flow)
		{
			var constructUseSharedSourceDialog = new ConstructUseSharedSourceDialogTask(Helpers, serviceReservation.ID, orders.Single(), LockInfos.SingleOrDefault(), UserInfo, flow);
			Tasks.Add(constructUseSharedSourceDialog);

			IsSuccessful &= constructUseSharedSourceDialog.Execute();
			if (!IsSuccessful) return;

			UseSharedSourceDialog = constructUseSharedSourceDialog.UseSharedSourceDialog;
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
				var orderLockInfo = Helpers.LockManager.RequestOrderLock(order.Id, extendLock: true);
				if (orderLockInfo.LockUsername.Contains("error"))
				{
					Helpers.Log(nameof(LoadUpdateServiceDialog), nameof(ExtendLocksTimer_Elapsed), $"Something went wrong when extending the order lock: {order.Id}", order.Name);
				}
			}

			attemptsExtendLocking++;
		}
	}
}