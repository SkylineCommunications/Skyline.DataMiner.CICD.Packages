namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs
{
	using System;
	using System.Linq;
	using Library_1.EventArguments;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public sealed class LiveOrderFormDialog : EditOrderDialog
	{
		public LiveOrderFormDialog(Helpers helpers, Order order, Event @event, UserInfo userInfo, Scripts script, EditOrderFlows flow, LockInfo lockInfo = null) : base(helpers, order, @event, userInfo, script, flow, lockInfo)
		{
			MultiThreadedInitialize();
			Initialize();

			GenerateUi();
			HandleVisibilityAndEnabledUpdate();
		}

		public YleButton SaveOrderButton { get; private set; } = new YleButton("Save Order as Preliminary");

		public YleButton BookOrderButton { get; private set; } = new YleButton("Book Order") { Style = ButtonStyle.CallToAction };

		public YleButton CancelOrderButton { get; private set; } = new YleButton("Cancel Order");

		public YleButton RejectButton { get; private set; } = new YleButton("Reject Order");

		public YleButton ConfirmButton { get; private set; } = new YleButton("Confirm Order");

		public YleButton StopOrderButton { get; private set; } = new YleButton("Stop Order");

		public YleButton SaveAsTemplateButton { get; private set; } = new YleButton("Save as Template");

		public YleButton HistoryButton { get; private set; } = new YleButton("Show Order Change History");

		public YleButton ExitButton { get; private set; } = new YleButton("Exit");

		public event EventHandler UploadJsonButtonPressed;

		public event EventHandler<ServiceEventArgs> BookEurovisionService;

		public event EventHandler<ServiceEventArgs> UploadSynopsisButtonPressed;

		public event EventHandler<ServiceEventArgs> SharedSourceUnavailableDueToOrderTimingChange;

		protected override void AdditionalScriptActionsAsFinalInitialization()
		{
			if (Flow == EditOrderFlows.AddDestinationToOrder)
			{
				var newService = new DisplayedService(helpers, helpers.ServiceDefinitionManager.GetServiceDefinition(ServiceDefinitionGuids.YleHelsinkiDestination))
				{
					Start = order.Start,
					End = order.End,
				};

				orderController.AddChildService(newService);
				BookOrderButton.Text = "Add Destination";
			}
			else if (Flow == EditOrderFlows.AddTransmissionToOrder)
			{
				var newService = new DisplayedService(helpers, helpers.ServiceDefinitionManager.GetServiceDefinition(ServiceDefinitionGuids.FiberFullCapacityTransmissionServiceDefinitionId))
				{
					Start = order.Start,
					End = order.End,
				};

				orderController.AddChildService(newService);
				BookOrderButton.Text = "Add Transmission";
			}
			else
			{
				// nothing
			}
		}

		protected override void GenerateUi()
		{
			Clear();
			MinWidth = 800;

			int row = -1;

			AddWidget(lockInfoLabel, ++row, 0, 1, 4);

			AddSection(orderSection, new SectionLayout(++row, 0));
			row += orderSection.RowCount;

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(validationLabel, ++row, 0, 1, 10);
			AddWidget(BookOrderButton, ++row, 0, 1, 4);
			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(SaveOrderButton, ++row, 0, 1, 4);
			AddWidget(ConfirmButton, ++row, 0, 1, 4);
			AddWidget(RejectButton, ++row, 0, 1, 4);
			AddWidget(CancelOrderButton, ++row, 0, 1, 4);
			AddWidget(StopOrderButton, ++row, 0, 1, 4);
			AddWidget(HistoryButton, ++row, 0, 1, 4);
			AddWidget(SaveAsTemplateButton, ++row, 0, 1, 4);
			AddWidget(ExitButton, row + 1, 0, 1, 4);

			SetColumnWidth(0, 40); // column 0 contains the highest level collapse buttons
			SetColumnWidth(1, 40); // column 1 contains the source/destinations/transmissions/recordings collapse buttons
			SetColumnWidth(2, 40); // column 2 contains the endpoint service collapse buttons
			SetColumnWidth(3, 120); // column 3 contains the labels for the endpoint service sections
									// column 4 contains the input widgets for the endpoint service sections
									// column 5 same as column 4
									// column 6 same as column 5
									// column 7 contains the unit label for profile parameter sections and the start now checkbox on order level
									// column 8 contains the resource label for right-hand side resource selection
			SetColumnWidth(9, 300); // column 9 contains the resource dropdown for right-hand side resource selection

			foreach (var yleWidget in Widgets.OfType<IYleWidget>())
			{
				// To enable logging of user input
				yleWidget.Helpers = helpers;
			}
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			using (StartPerformanceLogging())
			{
				lockInfoLabel.IsVisible = lockInfo != null && !lockInfo.IsLockGranted;

				orderSection.IsEnabled = IsEnabled;

				bool noRecurrence = !order.RecurringSequenceInfo.Recurrence.IsConfigured;
				bool editingSingleOrderInRecurringSequence = order.RecurringSequenceInfo.RecurrenceAction == YLE.Order.Recurrence.RecurrenceAction.ThisOrderOnly;

				bool addDestinationOrTranmission = Flow == EditOrderFlows.AddDestinationToOrder || Flow == EditOrderFlows.AddTransmissionToOrder;

				SaveAsTemplateButton.IsVisible = !isReadOnly && userInfo.CanConfigureTemplate && order.IntegrationType == IntegrationType.None && !addDestinationOrTranmission && order.Subtype == OrderSubType.Normal;

				HistoryButton.IsVisible = !addDestinationOrTranmission;

				SaveOrderButton.IsVisible = !isReadOnly && order.CanBeSaved && !addDestinationOrTranmission;

				BookOrderButton.IsVisible = !isReadOnly;
				BookOrderButton.IsEnabled = IsEnabled && orderIsValid;

				CancelOrderButton.IsVisible = !isReadOnly && order.IsBooked && order.CanCancel && !addDestinationOrTranmission;

				RejectButton.IsVisible = !isReadOnly && order.IsBooked && userInfo.IsMcrUser && order.CanReject && (noRecurrence || editingSingleOrderInRecurringSequence) && !addDestinationOrTranmission;

				ConfirmButton.IsVisible = !isReadOnly && order.IsBooked && userInfo.IsMcrUser && order.CanConfirm && (noRecurrence || editingSingleOrderInRecurringSequence) && !addDestinationOrTranmission;

				StopOrderButton.IsVisible = !isReadOnly && order.IsBooked && userInfo.IsMcrUser && (order.ShouldBeRunning || order.HasCueingServices) && !addDestinationOrTranmission && order.Subtype == OrderSubType.Normal;

				ExitButton.IsVisible = isReadOnly;
			}
		}

		protected override string GetTitle()
		{
			if (!order.IsBooked) return "New Order";
			if (isReadOnly) return "View Order";
			return "Order Edit";
		}

		protected override void SubscribeToOrderControllerAndOrderSection()
		{
			orderSection.OrderTypeChanged += OrderSection_OrderTypeChanged;

			orderController.SourceChanged += (s, e) => UpdateSaveOrderButtonText();
			orderController.UploadJsonButtonPressed += (s, e) => UploadJsonButtonPressed?.Invoke(this, EventArgs.Empty);
			orderController.UploadSynopsisButtonPressed += (s, e) => UploadSynopsisButtonPressed?.Invoke(this, e);
			orderController.BookEurovisionService += (s, e) => BookEurovisionService?.Invoke(this, e);
			orderController.SharedSourceUnavailableDueToOrderTimingChange += (s, e) => SharedSourceUnavailableDueToOrderTimingChange?.Invoke(this, e);
			orderController.ValidationRequired += (s, e) => BookOrderButton.IsEnabled = IsValid(false, false, false);
		}

		private void UpdateSaveOrderButtonText()
		{
			var status = CalculateOrderStatus(OrderAction.Save);

			SaveOrderButton.Text = $"Save Order as {status.GetDescription()}";
		}

		private void OrderSection_OrderTypeChanged(object sender, bool orderIsVizrem)
		{
			order.Subtype = orderIsVizrem ? OrderSubType.Vizrem : OrderSubType.Normal;

			order.Sources.Clear();

			orderController.UnsubscribeFromUi(); // unsubscribe controller from UI
												 // TODO unsubscribe UI from Order

			var defaultSourceServiceDefinition = order.Subtype == OrderSubType.Normal ? ServiceDefinition.GenerateDummyReceptionServiceDefinition() : helpers.ServiceDefinitionManager.GetServiceDefinition(ServiceDefinitionGuids.VizremStudioHelsinki);
			var defaultSourceService = new DisplayedService(helpers, defaultSourceServiceDefinition);
			defaultSourceService.Start = order.Start;
			defaultSourceService.End = order.End;
			defaultSourceService.AcceptChanges();
			order.SourceService = defaultSourceService;

			if (order.Subtype == OrderSubType.Vizrem)
			{
				InitializeVizremFarmService();

				InitializeVizremStudioAsDestination();
			}

			order.IsInternal = orderIsVizrem ? true : (order.Event?.IsInternal ?? false);

			orderSectionConfiguration = CreateOrderSectionConfiguration();

			InitializeOrderSectionAndController();

			IsValid(false, false, false);

			HandleVisibilityAndEnabledUpdate();
			RegenerateUI();
		}

		private void InitializeVizremFarmService()
		{
			var vizremFarmService = new DisplayedService(helpers, helpers.ServiceDefinitionManager.GetServiceDefinition(ServiceDefinitionGuids.VizremFarm))
			{
				Start = order.Start,
				End = order.End,
			};

			vizremFarmService.AcceptChanges();

			order.Sources.Single().Children.Add(vizremFarmService);
		}

		private void InitializeVizremStudioAsDestination()
		{
			var vizremFarmService = order.Sources.Single().Children.Single();

			var vizremStudioHelsinkiServiceDefinition = helpers.ServiceDefinitionManager.GetServiceDefinition(ServiceDefinitionGuids.VizremStudioHelsinki);
			var vizremStudioHelsinkiService = new DisplayedService(helpers, vizremStudioHelsinkiServiceDefinition);
			vizremStudioHelsinkiService.Start = order.Start;
			vizremStudioHelsinkiService.End = order.End;
			vizremStudioHelsinkiService.AcceptChanges();

			vizremFarmService.Children.Add(vizremStudioHelsinkiService);
		}
	}
}
