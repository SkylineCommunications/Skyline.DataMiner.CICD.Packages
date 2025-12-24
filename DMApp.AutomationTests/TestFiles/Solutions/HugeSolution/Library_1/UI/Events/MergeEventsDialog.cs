namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public class MergeEventsDialog : Dialog
	{
		private readonly List<LiteOrder> orders;
		private readonly LockManager lockManager;

		private List<OrderDetailsSection> orderDetailsSections;

		private readonly Label primaryEventTitle = new Label("Primary Event") { Style = TextStyle.Bold };
		private readonly Label primaryEventInstructionsLabel = new Label("The selected orders will be moved to this event.");
		private readonly Label primaryEventLabel = new Label("Primary Event");
		private readonly DropDown primaryEventDropDown = new DropDown();
		private readonly Label primaryEventSelectionReasonLabel = new Label { IsVisible = false };
		private readonly Label orderTitleLabel = new Label("Orders") { Style = TextStyle.Bold };
		private readonly Label ordersInstructionsLabel = new Label("Select the orders to include in the merged event.");
		private readonly Label integrationOrdersInfoLabel = new Label("Orders created by an integration are not displayed here but will be merged automatically.");

		private readonly Label includeHeader = new Label("Include") { Style = TextStyle.Heading };
		private readonly Label eventHeader = new Label("Event") { Style = TextStyle.Heading };
		private readonly Label orderHeader = new Label("Order") { Style = TextStyle.Heading };
		private readonly Label startHeader = new Label("Start") { Style = TextStyle.Heading };
		private readonly Label endHeader = new Label("End") { Style = TextStyle.Heading };

		public MergeEventsDialog(Helpers helpers, List<Event> events, List<LiteOrder> orders, LockManager lockManager) : base(helpers.Engine)
		{
			Events = events ?? throw new ArgumentNullException(nameof(events));
			this.orders = orders ?? throw new ArgumentNullException(nameof(orders));
			this.lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));

			Initialize();
			GenerateUI();
		}

        public Event PrimaryEvent => Events.FirstOrDefault(x => x.Name == primaryEventDropDown.Selected);

		public Button MergeEventsButton { get; private set; }

		public List<Event> Events { get; }

		public List<Event> GetEventsToRemove()
		{
			return Events.Where(x => x.Id != PrimaryEvent.Id).ToList();
		}

		public List<LiteOrder> GetOrdersToRemove()
		{
			return orders.Where(x => !GetSelectedOrders().Select(y => y.Id).Contains(x.Id)).ToList();
		}

		public List<LiteOrder> GetOrdersToMove()
		{
			return GetSelectedOrders().Where(x => x.Event.Id != PrimaryEvent.Id).ToList();
		}

		public List<LiteOrder> GetNonIntegrationOrdersToMove()
		{
			return GetSelectedOrders().Where(x => x.Event.Id != PrimaryEvent.Id).ToList();
		}

		private List<LiteOrder> GetSelectedOrders()
		{
			return orderDetailsSections.Where(x => x.IsSelected).Select(x => x.Order).ToList();
		}

		private bool IsReadonly { get; set; }

		private void Initialize()
		{
			Title = "Merge Events";
			MergeEventsButton = new Button("Merge Events") { Width = 200, Style = ButtonStyle.CallToAction };

			InitializePrimaryEvent();
			InitializeOrderDetailsSections();
		}

		private void InitializeOrderDetailsSections()
		{
			orderDetailsSections = new List<OrderDetailsSection>();
			foreach (var order in orders.OrderBy(x => x.Name).ThenBy(x => x.Event.Name))
			{
				var orderDetailsSection = new OrderDetailsSection(order)
				{
					IsSelected = true,
					IsCheckBoxEnabled = order.IntegrationType == IntegrationType.None
				};

				orderDetailsSections.Add(orderDetailsSection);
			}
		}

		private void InitializePrimaryEvent()
		{
			primaryEventDropDown.Options = Events.Select(x => x.Name);

			var ceitonEvent = Events.FirstOrDefault(e => e.IntegrationType == IntegrationType.Plasma);
			var plasmaIntegrationEvent = Events.FirstOrDefault(e => e.IntegrationType == IntegrationType.Plasma);
			var feenixIntegrationEvent = Events.FirstOrDefault(e => e.IntegrationType == IntegrationType.Feenix);
			var ebuIntegrationEvent = Events.FirstOrDefault(e => e.IntegrationType == IntegrationType.Eurovision);
			var anyIntegrationEvent = Events.FirstOrDefault(x => x.IntegrationType != IntegrationType.None);

			if (ceitonEvent != null)
			{
				primaryEventDropDown.Selected = ceitonEvent.Name;
				primaryEventDropDown.IsEnabled = false;
				primaryEventSelectionReasonLabel.Text = $"Event {primaryEventDropDown.Selected} was selected as the primary event as it is an Event created by Ceiton integration.";
				primaryEventSelectionReasonLabel.IsVisible = true;
				return;
			}
			else if (plasmaIntegrationEvent != null)
			{
				primaryEventDropDown.Selected = plasmaIntegrationEvent.Name;
			}
			else if (feenixIntegrationEvent != null)
			{
				primaryEventDropDown.Selected = feenixIntegrationEvent.Name;
			}
			else if (ebuIntegrationEvent != null)
			{
				primaryEventDropDown.Selected = ebuIntegrationEvent.Name;
			}
			else if (anyIntegrationEvent != null)
			{
				primaryEventDropDown.Selected = anyIntegrationEvent.Name;
			}
			else
			{
				// nothing
			}

			bool onlyOneIntegrationEvent = Events.Count(e => e.IntegrationType != IntegrationType.None) == 1;
			if (onlyOneIntegrationEvent)
			{
				primaryEventDropDown.IsEnabled = false;
				primaryEventSelectionReasonLabel.Text = $"Event {primaryEventDropDown.Selected} was selected as the primary event as it is the only one created by an integration.";
				primaryEventSelectionReasonLabel.IsVisible = true;
			}
		}

		private void GenerateUI()
		{
			int row = -1;

			if (!lockManager.AreLocksGranted)
			{
				AddWidget(new Label("Unable to merge the events as one or multiple events and/or orders are locked."), ++row, 0, 1, 5);
				foreach (var eventLockInfo in lockManager.GetDeniedEventLocks())
				{
					var deniedEvent = Events.FirstOrDefault(x => x.Id.ToString().Equals(eventLockInfo.ObjectId));
					AddWidget(new Label($"\t- Event {deniedEvent?.Name} is locked by {eventLockInfo.LockUsername}"), ++row, 0, 1, 5);
				}

				foreach (var orderLockInfo in lockManager.GetDeniedOrderLocks())
				{
					var deniedOrder = orders.FirstOrDefault(x => x.Id.ToString().Equals(orderLockInfo.ObjectId));
					AddWidget(new Label($"\t- Order {deniedOrder?.Name} is locked by {orderLockInfo.LockUsername}"), ++row, 0, 1, 5);
				}

				IsReadonly = true;
			}

			AddWidget(primaryEventTitle, ++row, 0, 1, 5);

			AddWidget(primaryEventInstructionsLabel, ++row, 0, 1, 5);

			AddWidget(primaryEventLabel, ++row, 0, 1, 2);
			AddWidget(primaryEventDropDown, row, 2, 1, 3);

			AddWidget(primaryEventSelectionReasonLabel, ++row, 0, 1, 5);

			AddWidget(orderTitleLabel, ++row, 0, 1, 5);

			AddWidget(ordersInstructionsLabel, ++row, 0, 1, 5);

			AddWidget(integrationOrdersInfoLabel, ++row, 0, 1, 5);

			AddWidget(includeHeader, ++row, 0, HorizontalAlignment.Center);
			AddWidget(eventHeader, row, 1, HorizontalAlignment.Center);
			AddWidget(orderHeader, row, 2, HorizontalAlignment.Center);
			AddWidget(startHeader, row, 3, HorizontalAlignment.Center);
			AddWidget(endHeader, row, 4, HorizontalAlignment.Center);

			foreach (OrderDetailsSection orderDetailsSection in orderDetailsSections)
			{
				AddSection(orderDetailsSection, new SectionLayout(++row, 0));
			}

			AddWidget(new WhiteSpace(), ++row, 0, 1, 5);

			AddWidget(MergeEventsButton, ++row, 0, 1, 2);

			// Disable Interactive Widgets if Dialog is Readonly
			if (IsReadonly)
			{
				InteractiveWidget interactiveWidget;
				foreach (Widget widget in Widgets)
				{
					interactiveWidget = widget as InteractiveWidget;
					if (interactiveWidget != null && !(interactiveWidget is CollapseButton))
					{
						interactiveWidget.IsEnabled = false;
					}
				}
			}
		}
	}
}

