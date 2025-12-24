namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using OrderStatus = YLE.Order.Status;

	public abstract class EditOrderDialog : YleDialog
	{
		protected readonly LockInfo lockInfo;
		protected readonly UserInfo userInfo;
		protected readonly Scripts script;
		protected readonly Order order;
		protected readonly Event @event;
		private Order originalOrder;

		protected readonly Label lockInfoLabel = new Label(String.Empty);
		protected readonly Label validationLabel = new Label(String.Empty);

		protected OrderSection orderSection;
		protected OrderSectionConfiguration orderSectionConfiguration;

		protected OrderController orderController;
		protected readonly bool isReadOnly;

		protected bool orderIsValid;

		protected EditOrderDialog(Helpers helpers, Order order, Event @event, UserInfo userInfo, Scripts script, EditOrderFlows flow, LockInfo lockInfo = null) : base(helpers)
		{
			this.userInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
			this.script = script;
			this.order = order ?? throw new ArgumentNullException(nameof(order));
			this.lockInfo = lockInfo;
			this.@event = @event;
			Flow = flow;

			AllowOverlappingWidgets = true;

			isReadOnly = Flow == EditOrderFlows.ViewOrder || Flow == EditOrderFlows.ViewService || (lockInfo != null && !lockInfo.IsLockGranted);
		}

		public Order Order => order;

		public EditOrderFlows Flow { get; }

		public OrderSection OrderSection => orderSection;

		public bool IsValid(bool saveOrder, bool confirmOrder, bool requestEventLock = false)
		{
			var options = Options.None;
			if (saveOrder) options |= Options.SaveOrder;
			if (order.ShouldBeRunning) options |= Options.IsRunning;
			if (Flow == EditOrderFlows.MergeOrders) options |= Options.MergeOrder;
			if (confirmOrder) options |= Options.ConfirmOrder;
			if (requestEventLock) options |= Options.RequestEventLock;

			var validator = new OrderValidator(helpers, order, userInfo, options);

			orderIsValid = validator.Validate();

			validationLabel.Text = String.Join("\n", validator.ValidationMessages);

			Log(nameof(IsValid), $"Validation messages: '{validationLabel.Text}'");

			return orderIsValid;
		}

		public void UnsubscribeFromUi()
		{
			orderController.UnsubscribeFromUi();
		}

		public UpdateResult Finish(OrderAction action)
		{
			UnsubscribeFromUi();

			Order.Status = CalculateOrderStatus(action);

			Log(nameof(CalculateOrderStatus), $"Order status has been set to: {Order.Status} during script action: {Flow} and order action: {action}");

			GetAddOrUpdateOptions(out var optionFlags, out var orderChangeInfo);

			optionFlags |= OrderUpdateHandler.OptionFlags.SkipGenerateProcessing;

			// Update LastUpdatedBy properties on the Order
			Order.LastUpdatedBy = Engine.UserLoginName;
			Order.LastUpdatedByEmail = userInfo.User?.Email ?? string.Empty;
			Order.LastUpdatedByPhone = userInfo.User?.Phone ?? string.Empty;
			Order.SetMcrLateChangeRequired(helpers, userInfo.IsMcrUser, orderChangeInfo);

			bool occupiedResourcesNeedToBeAssigned = Order.AllServices.SelectMany(s => s.Functions).Any(f => f.Resource is OccupiedResource occupiedResource && occupiedResource.IsFullyOccupied);

			if (occupiedResourcesNeedToBeAssigned)
			{
				var result = new UpdateResult { UpdateWasSuccessful = true };

				if (!order.IsBooked)
				{
					optionFlags |= OrderUpdateHandler.OptionFlags.SkipUpdatingOrderManager; // avoid executing book services in background

					var orderBookingResult = Order.AddOrUpdate(helpers, userInfo.IsMcrUser, optionFlags, false, originalOrder);
					result.Add(orderBookingResult);
				}

				helpers.ClearCache(); // clear cache of available resources

				var handler = new AssignOccupiedResourcesToOrderHandler(helpers, Order);

				var serviceBookingResult = handler.Execute();

				result.Add(serviceBookingResult);

				return result;
			}
			else
			{
				return Order.AddOrUpdate(helpers, userInfo.IsMcrUser, optionFlags, false, originalOrder);
			}
		}

		private void GetAddOrUpdateOptions(out OrderUpdateHandler.OptionFlags optionFlags, out OrderChangeSummary orderChangeInfo)
		{
			optionFlags = Order.GetAddOrUpdateOptions(helpers, out orderChangeInfo);
			optionFlags |= OrderUpdateHandler.OptionFlags.ForceAllOrderCustomPropertiesUpdate; // Plasma ID for Archive on order level is based on the value in the service.
			if (script == Scripts.UpdateService) optionFlags |= OrderUpdateHandler.OptionFlags.SkipServiceResourceAssignment; // resource assignment has been done in the UI
			if (orderChangeInfo.ServiceChangeSummary.PropertyChangeSummary.RecordingConfigurationChanged) optionFlags |= OrderUpdateHandler.OptionFlags.ForceAllOrderCustomPropertiesUpdate; // Plasma ID for Archive on order level is based on the value in the service.

			Log(nameof(Finish), $"Update flags: {optionFlags}");
		}

		protected OrderStatus CalculateOrderStatus(OrderAction orderAction)
		{
			var orderChangeSummary = order.Change.Summary as OrderChangeSummary;

			switch (Flow)
			{
				case EditOrderFlows.EditOrder when orderAction == OrderAction.Book:
					return CalculateOrderStatusForEditingAndBooking(orderChangeSummary);

				case EditOrderFlows.EditOrder:
					if (order.SourceService.IsUnknownSourceService) return OrderStatus.PlannedUnknownSource;
					else if (HasBookedEurovisionServices) return OrderStatus.WaitingOnEbu;
					else if (order.Status == OrderStatus.Cancelled) return OrderStatus.Preliminary;
					else return order.Status;

				case EditOrderFlows.MergeOrders when orderAction == OrderAction.Book:
					return CalculateOrderStatusWhenMergingAndBooking(orderChangeSummary);

				case EditOrderFlows.MergeOrders:
					if (order.SourceService.Definition.VirtualPlatform == VirtualPlatform.ReceptionUnknown) return OrderStatus.PlannedUnknownSource;
					else return OrderStatus.Preliminary;

				default:
					return CalculateOrderStatusForCreatingNewOrder(orderAction);
			}
		}

		private OrderStatus CalculateOrderStatusForCreatingNewOrder(OrderAction orderAction)
		{
			if (orderAction == OrderAction.Save)
			{
				if (order.SourceService.Definition.VirtualPlatform == VirtualPlatform.ReceptionUnknown) return OrderStatus.PlannedUnknownSource;
				else return HasBookedEurovisionServices ? OrderStatus.WaitingOnEbu : OrderStatus.Preliminary;
			}
			else if (userInfo.IsMcrUser)
			{
				// auto confirm orders created by MCR
				return OrderStatus.Confirmed;
			}
			else
			{
				return OrderStatus.Planned;
			}
		}

		private OrderStatus CalculateOrderStatusWhenMergingAndBooking(OrderChangeSummary orderChangeSummary)
		{
			if (userInfo.IsMcrUser) return OrderStatus.Confirmed;
			else if (order.IsSaved) return OrderStatus.Planned;
			else if (order.Status == OrderStatus.Confirmed && (orderChangeSummary.ServicesWereAdded || orderChangeSummary.ServicesWereRemoved || orderChangeSummary.AreThereAnyCrucialServiceChanges)) return OrderStatus.ChangeRequested;
			else return order.Status;
		}

		private OrderStatus CalculateOrderStatusForEditingAndBooking(OrderChangeSummary orderChangeSummary)
		{
			bool serviceChanges = orderChangeSummary.ServicesWereAdded || orderChangeSummary.ServicesWereRemoved || orderChangeSummary.AreThereAnyCrucialServiceChanges;
			var sourceServiceChange = order.SourceService.Change as ServiceChange;
			bool synopsisFilesChanged = sourceServiceChange?.GetCollectionChanges(nameof(Service.SynopsisFiles))?.Summary?.IsChanged == true;

			if (order.Status == OrderStatus.Running) return OrderStatus.Running;
			else if (userInfo.IsMcrUser) return OrderStatus.Confirmed;
			else if (order.IsSaved || order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Rejected) return OrderStatus.Planned;
			else if (order.Status == OrderStatus.Confirmed && (serviceChanges || synopsisFilesChanged)) return OrderStatus.ChangeRequested;
			else if (order.Status == OrderStatus.Planned && userInfo.IsMcrUser) return OrderStatus.Confirmed;
			else return order.Status;
		}

		protected void MultiThreadedInitialize()
		{
			helpers.LogMethodStart(nameof(EditOrderDialog), nameof(MultiThreadedInitialize), out var stopwatch, null, true);

			Parallel.Invoke(
				() => CloneOrder(),
				() => InitializeServiceDefinitions(),
				() => orderSectionConfiguration = CreateOrderSectionConfiguration());

			helpers.LogMethodCompleted(nameof(EditOrderDialog), nameof(MultiThreadedInitialize), null, stopwatch, true);
		}

		private void InitializeServiceDefinitions()
		{
			// assign an unused variable to get the property and therefore fill the SD-manager cache
			var serviceDefinitionsForLiveOrderForm = helpers.ServiceDefinitionManager.ServiceDefinitionsForLiveOrderForm;
		}

		private void CloneOrder()
		{
			LogMethodStart(nameof(CloneOrder), out var stopwatch);

			if (isReadOnly)
			{
				Log(nameof(CloneOrder), $"Not cloning order because dialog is read-only");
			}
			else
			{
				originalOrder = order?.Clone() as Order;
			}

			LogMethodCompleted(nameof(CloneOrder), stopwatch);
		}

		private bool HasBookedEurovisionServices
		{
			get
			{
				var eurovisionTransmission = order.AllServices.FirstOrDefault(x => x.BackupType == BackupType.None && x.Definition.VirtualPlatform == VirtualPlatform.TransmissionEurovision);
				var backupEurovisionTransmission = order.AllServices.FirstOrDefault(x => x.BackupType != BackupType.None && x.Definition.VirtualPlatform == VirtualPlatform.TransmissionEurovision);

				bool hasEurovisionSource = order.SourceService.Definition.VirtualPlatformServiceName == VirtualPlatformName.Eurovision && (order.SourceService.IsBooked || order.SourceService.LinkEurovisionId);
				bool hasEurovisionBackupSource = order.BackupSourceService != null && order.BackupSourceService.Definition.VirtualPlatformServiceName == VirtualPlatformName.Eurovision && (order.BackupSourceService.IsBooked || order.BackupSourceService.LinkEurovisionId);
				bool hasEurovisionTransmission = eurovisionTransmission != null && (eurovisionTransmission.IsBooked || eurovisionTransmission.LinkEurovisionId);
				bool hasEurovisionBackupTransmission = backupEurovisionTransmission != null && (backupEurovisionTransmission.IsBooked || backupEurovisionTransmission.LinkEurovisionId);

				return hasEurovisionSource ||
					   hasEurovisionBackupSource ||
					   hasEurovisionTransmission ||
					   hasEurovisionBackupTransmission;
			}
		}

		protected void Initialize()
		{
			InitializeWidgets();
			InitializeOrderSectionAndController();

			AdditionalScriptActionsAsFinalInitialization();
		}

		protected abstract void AdditionalScriptActionsAsFinalInitialization();

		private void InitializeWidgets()
		{
			Title = GetTitle();

			lockInfoLabel.Text = (lockInfo == null || lockInfo.IsLockGranted) ? String.Empty : $"Unable to edit Order as it is currently locked by {lockInfo.LockUsername}";
		}

		protected abstract string GetTitle();

		protected void InitializeOrderSectionAndController()
		{
			switch (order.Subtype)
			{
				case OrderSubType.Normal:
					orderController = new NormalOrderController(helpers, order, userInfo);
					orderSection = new NormalOrderSection(helpers, order, orderSectionConfiguration, userInfo);
					break;

				case OrderSubType.Vizrem:
					orderController = new VizremOrderController(helpers, order, userInfo);
					orderSection = new VizremOrderSection(helpers, order, orderSectionConfiguration, userInfo);
					break;

				default:
					throw new InvalidOperationException($"Unknown order subtype");
			}

			orderController.AddOrReplaceSection(orderSection);

			orderSection.RegenerateUiRequired += (s, e) => RegenerateUI();

			orderController.UiDisableRequired += (o, e) => DisableUi();
			orderController.UiEnableRequired += (o, e) => EnableUi();

			SubscribeToOrderControllerAndOrderSection();

			orderController.InvokeValidationRequired();
		}

		protected abstract void SubscribeToOrderControllerAndOrderSection();

		protected OrderSectionConfiguration CreateOrderSectionConfiguration()
		{
			var configuration = new OrderSectionConfiguration(helpers, order, order.Event, userInfo, isReadOnly, lockInfo, script, Flow);

			return configuration;
		}

		protected abstract void GenerateUi();

		protected abstract void HandleVisibilityAndEnabledUpdate();

		protected void RegenerateUI()
		{
			Clear();
			orderSection.RegenerateUi();
			GenerateUi();
		}

		protected override void HandleEnabledUpdate()
		{
			HandleVisibilityAndEnabledUpdate();
		}
	}
}
