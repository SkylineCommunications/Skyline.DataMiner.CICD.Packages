namespace LiveOrderForm_6.Dialogs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Timers;
	using LoadLiveOrderFormTasks;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.LoadingScreenTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.OrderTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status;

	public class LoadLiveOrderFormDialog : LoadingDialog
	{
		private static readonly string MinusOne = "-1";

		private List<Guid> orderGuids = new List<Guid>();
		private string eventId = String.Empty;

		private readonly Timer extendLocksTimer;
		private int attemptsExtendLocking = 1;

		public LoadLiveOrderFormDialog(Helpers helpers, Timer extendLocksTimer) : base(helpers)
		{
			this.extendLocksTimer = extendLocksTimer;
		}

		public List<Order> Orders { get; private set; } = new List<Order>();

		public List<Event> Events { get; private set; } = new List<Event>();

		public LiveOrderFormDialog EditOrderDialog { get; private set; }

		public OrderDeletionDialog OrderDeletionDialog { get; private set; }

		public OrderMergingDialog OrderMergingDialog { get; private set; }

		public EventSelectionOrderDuplicationDialog EventSelectionOrderDuplicationDialog { get; private set; }

		public UseOrderTemplateDialog UseOrderTemplateDialog { get; private set; }

		public LiveOrderFormAction ScriptAction { get; private set; }

		private Order FirstOrder => Orders.FirstOrDefault();

		private Event FirstEvent => Events.FirstOrDefault();

		protected override void GetScriptInput()
		{
			string bookingIdValue = Engine.GetScriptParam("BookingID").Value;

			List<string> orderIds;

			if (bookingIdValue.Contains("["))
			{
				orderIds = JsonConvert.DeserializeObject<List<string>>(bookingIdValue);
			}
			else
			{
				orderIds = bookingIdValue.Split(',').Where(id => id != MinusOne).ToList();
			}

			orderGuids = orderIds.Select(oid => Guid.Parse(oid)).ToList();

			eventId = Engine.GetScriptParam("JobID").Value;

			ScriptAction = EnumExtensions.GetEnumValueFromDescription<LiveOrderFormAction>(Engine.GetScriptParam("Action").Value);
		}

		protected override void CollectActions()
		{
			methodsToExecute.Add(InitializeOrderLogger);

			if (ScriptAction != LiveOrderFormAction.View) methodsToExecute.Add(GetLocks);

			methodsToExecute.Add(GetOrders);
			methodsToExecute.Add(GetEvents);

			if (ScriptAction == LiveOrderFormAction.Duplicate) methodsToExecute.Add(CheckIntegration);

			methodsToExecute.Add(GetUserInfo);
			methodsToExecute.Add(ConstructForm);
		}

		protected override void SendReportButton_Pressed(object sender, EventArgs e)
		{
			string title = "Exception while loading Live Order Form [" + DateTime.Now + "]";

			string message = $"Order: '{FirstOrder?.Name}'<br>Order ID: {FirstOrder?.Id}<br>Event: '{FirstEvent?.Name}'<br>User: {Engine.UserLoginName}<br>Timestamp: {DateTime.Now}<br>Exception: {informationMessageLabel.Text.Replace(" at ", "<br>at ")}";

			NotificationManager.SendMailToSkylineDevelopers(Helpers, title, message);

			reportSuccessfullySentLabel.IsVisible = true;
		}

		private void InitializeOrderLogger()
		{
			if (ScriptAction == LiveOrderFormAction.Delete)
			{
				// By referencing Empty Guid in the helpers, we will log this deletion to the file with the same name.
				// This way we can find the ID back of the removed order based on the name.

				Helpers.AddOrderReferencesForLogging(Guid.Empty);
			}

			var orderId = orderGuids.Any() ? orderGuids[0] : Guid.Empty;

			if (orderId != Guid.Empty)
			{
				Helpers.AddOrderReferencesForLogging(orderId);
			}
		}

		private void PrepareOrderForDuplication()
		{
			FirstOrder.AcceptChanges();

			// Duplicate Order should be saved under the same Event
			// Update Order Name
			FirstOrder.Id = Guid.Empty;
			FirstOrder.ManualName += "_DUP";
			FirstOrder.Status = Status.Preliminary;

			// Clear Service Names, Ids and NodeIds
			foreach (var service in FirstOrder.AllServices.Where(x => !x.IsSharedSource))
			{
				service.AcceptChanges();

				var newGuid = Guid.NewGuid();

				service.Name = $"{service.Definition.VirtualPlatformServiceType.GetDescription()} [{newGuid}]";
				service.Id = newGuid;
				service.IsBooked = false;
				service.IsDuplicate = true;
				service.ContributingResource = null;
				service.OrderReferences.Clear();
				service.ReservationInstance = null;
				service.EvsId = null;
			}

			// override order lock info to make sure order lock is always granted as we are creating a new order anyway when duplicating
			var overriddenLockInfos = new List<LockInfo>();
			foreach (var lockInfo in LockInfos)
			{
				overriddenLockInfos.Add(new LockInfo(true, lockInfo.LockUsername, lockInfo.ObjectId, lockInfo.ReleaseLocksAfter));
			}

			LockInfos.Clear();
			LockInfos.AddRange(overriddenLockInfos);
		}

		private void GetOrders()
		{
			var unretrievedOrders = new List<Guid>();

			foreach (var validBookingId in orderGuids)
			{
				var getOrderTask = new GetOrderTask(Helpers, validBookingId);
				Tasks.Add(getOrderTask);

				try
				{
					IsSuccessful &= getOrderTask.Execute();

					if (!IsSuccessful) return;

					Orders.Add(getOrderTask.Order);
				}
				catch (Exception ex)
				{
					var lockInfo = LockInfos.SingleOrDefault(l => l.ObjectId == validBookingId.ToString()) ?? throw new InvalidOperationException($"No lock info found for order {validBookingId}");

					if (!lockInfo.IsLockGranted)
					{
						// Normally if lock is not granted for the order, we show a readonly version of the order. But here we are unable to retrieve the order due to an exception. That is why we show a message about the lock not being granted instead.

						Helpers?.Log(nameof(LoadLiveOrderFormDialog), nameof(GetOrders), $"Exception while retrieving order {validBookingId}: {ex}");
						PrepareUiForManualMessage($"Unable to edit or view Order because it is locked by {lockInfo.LockUsername}", false, false);
						IsSuccessful = false;
						return;
					}
					else
					{
						// if lock is granted and we have an exception, we should follow the default exception handling 
						throw;
					}
				}
			}
		}

		private void GetEvents()
		{
			// Get Events for the Orders
			Events.AddRange(Orders.Select(order => order.Event));

			// Get Event when adding an Order to an existing Event
			if (!Events.Any() && !String.IsNullOrEmpty(eventId) && eventId != MinusOne)
			{
				var getEventTask = new GetEventTask(Helpers, eventId);
				Tasks.Add(getEventTask);

				IsSuccessful &= getEventTask.Execute();

				if (!IsSuccessful) return;

				Events.Add(getEventTask.Event);
			}
		}

		private void CheckIntegration()
		{
			if (FirstOrder.IntegrationType != IntegrationType.None && FirstOrder.IntegrationType != IntegrationType.Eurovision)
				PrepareUiForManualErrorMessage($"Duplicating an order created by {FirstOrder.IntegrationType.GetDescription()} is not allowed", showExceptionWidgets: false);
		}

		private void GetLocks()
		{
			foreach (var orderId in orderGuids)
			{
				var getOrderLockTask = new GetOrderLockTask(Helpers, orderId);
				Tasks.Add(getOrderLockTask);

				IsSuccessful &= getOrderLockTask.Execute();
				if (!IsSuccessful) return;

				LockInfos.Add(getOrderLockTask.LockInfo);
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

			foreach (var order in Orders)
			{
				if (order != null)
				{
					var orderLockInfo = Helpers.LockManager.RequestOrderLock(order.Id, true);
					if (orderLockInfo.LockUsername.Contains("error"))
					{
						Log(nameof(ExtendLocksTimer_Elapsed), $"Something went wrong when extending the order lock: {order.Id}", order.Name);
					}
				}

				if (order?.Event != null)
				{
					var eventLockInfo = Helpers.LockManager.RequestEventLock(order.Event.Id, extendLock: true);
					if (eventLockInfo.LockUsername.Contains("error"))
					{
						Log(nameof(ExtendLocksTimer_Elapsed), $"Something went wrong when extending the order lock: {order.Event.Id}", order.Event.Name);
					}
				}
			}

			attemptsExtendLocking++;
		}

		private void GetUserInfo()
		{
			var getUserInfoTask = new GetUserInfoTask(Helpers, Events.FirstOrDefault());
			Tasks.Add(getUserInfoTask);

			IsSuccessful &= getUserInfoTask.Execute();

			if (!IsSuccessful) return;

			UserInfo = getUserInfoTask.UserInfo;
		}

		private void ConstructForm()
		{
			switch (ScriptAction)
			{
				case LiveOrderFormAction.Add:
					HandleAddOrder();
					break;
				case LiveOrderFormAction.Edit:
					HandleEditOrder();
					break;
				case LiveOrderFormAction.Duplicate:
					HandleDuplicateOrder();
					break;
				case LiveOrderFormAction.Delete:
					HandleDeleteOrder();
					break;
				case LiveOrderFormAction.Merge:
					HandleMergeOrders();
					break;
				case LiveOrderFormAction.FromTemplate:
					HandleOrderFromTemplate();
					break;
				case LiveOrderFormAction.View:
					HandleViewOrder();
					break;
				default:
					throw new ArgumentException("Invalid ScriptAction: " + ScriptAction);
			}
		}

		private void HandleAddOrder()
		{
			var newOrder = CreateNewOrder();
			Orders.Add(newOrder);

			ConstructLiveOrderForm();
		}

		private void HandleEditOrder()
		{
			bool orderHasValidStateToEdit = FirstOrder.Status != Status.Completed && FirstOrder.Status != Status.CompletedWithErrors;

			if (orderHasValidStateToEdit)
			{
				FirstOrder.AcceptChanges();
				foreach (var service in FirstOrder.AllServices)
				{
					service.AcceptChanges();
				}

				ConstructLiveOrderForm();
			}
			else PrepareUiForManualErrorMessage($"It is not possible to edit an order with status {FirstOrder.Status}.");
		}

		private void HandleViewOrder()
		{
			FirstOrder.AcceptChanges();
			foreach (var service in FirstOrder.AllServices)
			{
				service.AcceptChanges();
			}

			ConstructReadonlyLiveOrderForm();
		}

		private void HandleDeleteOrder()
		{
			if (FirstOrder.CanDelete || UserInfo.IsMcrUser)
			{
				OrderDeletionDialog = new OrderDeletionDialog((Engine)Engine, FirstOrder, LockInfos[0]);
			}
			else
			{
				PrepareUiForManualErrorMessage($"It is not possible to delete an order with status {FirstOrder.Status}.");
			}
		}

		private void HandleMergeOrders()
		{
			bool multipleIntegrationOrders = Orders.Count(x => x.IntegrationType != IntegrationType.None && x.IntegrationType != IntegrationType.Eurovision) > 1;
			bool rejectedOrRunningOrders = Orders.Any(x => x.Status == Status.Rejected || x.Status == Status.Running || x.HasCueingServices);
			bool lockedOrders = LockInfos.Any(x => !x.IsLockGranted);

			if (multipleIntegrationOrders) PrepareUiForManualErrorMessage("It is not possible to merge multiple orders created by integrations.");
			else if (rejectedOrRunningOrders) PrepareUiForManualErrorMessage("It is not possible to merge rejected or running orders.");
			else if (lockedOrders) PrepareUiForManualErrorMessage("It is not possible to merge locked orders.");
			else OrderMergingDialog = new OrderMergingDialog(Helpers, Orders);
		}

		private void HandleDuplicateOrder()
		{
			PrepareOrderForDuplication();
			EventSelectionOrderDuplicationDialog = new EventSelectionOrderDuplicationDialog(Helpers, FirstOrder, UserInfo);
		}

		private void HandleOrderFromTemplate()
		{
			var orderTemplates = Helpers.ContractManager.GetOrderTemplates(UserInfo.GetOrderTemplates()).Where(t => !t.IsTemplateForRecurringOrder).ToList();

			var lockInfo = new LockInfo(true, Helpers.Engine.UserLoginName, orderTemplates[0].Id.ToString(), TimeSpan.FromHours(2));

			UseOrderTemplateDialog = new UseOrderTemplateDialog(Helpers, orderTemplates, lockInfo);
		}

		private Order CreateNewOrder()
		{
			DateTime start = DateTime.Now.AddMinutes(30).RoundToMinutes();
			DateTime end = start.AddMinutes(60).RoundToMinutes();

			var receptionService = new DisplayedService(Helpers, ServiceDefinition.GenerateDummyReceptionServiceDefinition());
			receptionService.AcceptChanges();

			receptionService.Start = start;
			receptionService.End = end;

			Order newOrder = new Order
			{
				Start = start,
				End = end,
				IntegrationType = IntegrationType.None,
				Event = FirstEvent,
				IsInternal = FirstEvent != null && FirstEvent.IsInternal,
				Contract = UserInfo.Contract.Name,
				Company = UserInfo.Contract.Company,
				CreatedByUserName = Helpers.Engine.UserLoginName,
				CreatedByEmail = String.IsNullOrEmpty(UserInfo.User?.Email) ? String.Empty : UserInfo.User?.Email,
				CreatedByPhone = String.IsNullOrEmpty(UserInfo.User?.Phone) ? String.Empty : UserInfo.User?.Phone,
				LastUpdatedBy = Helpers.Engine.UserLoginName,
				LastUpdatedByEmail = String.IsNullOrEmpty(UserInfo.User?.Email) ? String.Empty : UserInfo.User?.Email,
				LastUpdatedByPhone = String.IsNullOrEmpty(UserInfo.User?.Phone) ? String.Empty : UserInfo.User?.Phone,
				BillingInfo = new BillingInfo
				{
					BillableCompany = UserInfo.Contract.Company,
					CustomerCompany = String.Empty
				}
			};

			newOrder.SetUserGroupIds(new HashSet<int>(UserInfo.UserGroups.Select(u => Convert.ToInt32(u.ID))));

			var securityViewIdsToSet = UserInfo.UserGroups.Select(u => u.CompanySecurityViewId).Concat(new[] { UserInfo.McrSecurityViewId });
			newOrder.SetSecurityViewIds(new HashSet<int>(securityViewIdsToSet));

			newOrder.AcceptChanges();
			newOrder.SourceService = receptionService;

			return newOrder;
		}

		private void ConstructLiveOrderForm()
		{
			var constructLiveOrderFormDialogTask = new ConstructLiveOrderFormTask(Helpers, Orders.FirstOrDefault(), Events.FirstOrDefault(), LockInfos.FirstOrDefault(), UserInfo, ScriptAction);
			Tasks.Add(constructLiveOrderFormDialogTask);

			IsSuccessful &= constructLiveOrderFormDialogTask.Execute();
			if (!IsSuccessful) return;

			EditOrderDialog = constructLiveOrderFormDialogTask.EditOrderDialog;
		}

		private void ConstructReadonlyLiveOrderForm()
		{
			var constructLiveOrderFormDialogTask = new ConstructReadonlyLiveOrderFormTask(Helpers, Orders.FirstOrDefault(), Events.FirstOrDefault(), UserInfo);
			Tasks.Add(constructLiveOrderFormDialogTask);

			IsSuccessful &= constructLiveOrderFormDialogTask.Execute();
			if (!IsSuccessful) return;

			EditOrderDialog = constructLiveOrderFormDialogTask.EditOrderDialog;
		}
	}
}