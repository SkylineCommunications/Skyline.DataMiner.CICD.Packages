namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using NPOI.OpenXmlFormats.Dml;

	public class UseEventTemplateDialog : Dialog
	{
		private readonly Label TemplatesTitle = new Label("Templates") { Style = TextStyle.Bold };
		private readonly Label OrderTypeTitle = new Label("Order Type");
		private readonly CheckBox OrderTypeVizremCheckBox = new CheckBox("Vizrem");
		private readonly RadioButtonList TemplatesRadioButtonList = new RadioButtonList();

		private readonly CollapseButton OrdersCollapseButton = new CollapseButton { Width = 44, CollapseText = "-", ExpandText = "+", IsCollapsed = true };
		private readonly Label OrdersTitle = new Label("Linked Orders") { Style = TextStyle.Bold };

		private readonly Label EventDetailsTitle = new Label("Event Details") { Style = TextStyle.Bold };
		private readonly Label EventSelectionLabel = new Label("Select Event") { Style = TextStyle.Heading };
		private readonly RadioButtonList eventSelectionRadioButtonList = new RadioButtonList(new[] { EventSelection.OtherExistingEvent.GetDescription(), EventSelection.NewEvent.GetDescription() }, EventSelection.NewEvent.GetDescription());
		private readonly Label EventNameLabel = new Label("Event Name");
		private readonly TextBox eventNameTextBox = new TextBox();
		private readonly Label StartTimeLabel = new Label("Start Time");
		private readonly DateTimePicker StartTimeDateTimePicker = new DateTimePicker(DateTime.Now.RoundToMinutes().AddHours(1));
		private readonly Label EndTimeLabel = new Label("End Time");
		private readonly DateTimePicker EndTimeDateTimePicker = new DateTimePicker { IsEnabled = false };
		private readonly DropDown availableExistingEventsDropdown = new DropDown { IsVisible = false, IsDisplayFilterShown = true, Width = 400 };

		private readonly CollapseButton OccupiedOrdersCollapseButton = new CollapseButton { Width = 44, CollapseText = "-", ExpandText = "+", IsCollapsed = true };
		private readonly Label OccupiedOrdersTitle = new Label("Occupying Orders") { Style = TextStyle.Bold };
		private readonly Label OccupiedOrdersValidationLabel = new Label("Unable to continue because there are still resources used by other orders") { Style = TextStyle.None };

		private readonly Helpers helpers;
		private readonly UserInfo userInfo;
		private readonly List<EventTemplate> templates;
		private readonly List<EventTemplate> vizremTemplates;
		private readonly Dictionary<ReservationInstance, List<OccupiedResource>> allOccupiedOrderAndResources = new Dictionary<ReservationInstance, List<OccupiedResource>>();

		private readonly List<Event> allFutureEvents = new List<Event>();

		public UseEventTemplateDialog(Helpers helpers, List<EventTemplate> eventTemplates, UserInfo userInfo) : base(helpers.Engine)
		{
			if (eventTemplates == null) throw new ArgumentNullException(nameof(eventTemplates));

			Title = "Event From Template";

			this.helpers = helpers;
			this.templates = eventTemplates.Where(evt => evt.EventSubType == EventSubType.Normal).ToList();
			this.vizremTemplates = eventTemplates.Where(evt => evt.EventSubType == EventSubType.Vizrem).ToList();
			this.userInfo = userInfo;

			GetAllFutureEvents();
			InitializeWidgets();
			InitializeEventSubscriptions();
			UpdateTemplateList();
			UpdateEndTime();
			UpdateLinkedOrderTemplates();
			GenerateUI();
			UpdateWidgetVisibility();
		}

		public List<OrderTemplate> LinkedOrderTemplates { get; private set; } = new List<OrderTemplate>();

		public List<Order> SelectedTemplateLinkedOrders { get; private set; } = new List<Order>();

		public Event SelectedEvent
		{
			get => EventSelection == EventSelection.OtherExistingEvent ? allFutureEvents.Single(e => e.Name == availableExistingEventsDropdown.Selected) : Event.FromTemplate(helpers, SelectedTemplate, EventName, StartTime);
		}

		public EventSubType EventSubType => OrderTypeVizremCheckBox.IsChecked ? EventSubType.Vizrem : EventSubType.Normal;

		public Button ContinueButton { get; private set; } = new Button("Continue") { Width = 150, Style = ButtonStyle.CallToAction };

		public Button EditSelectedTemplateButton { get; private set; } = new Button("Edit Template") { Width = 150, Style = ButtonStyle.None, IsVisible = true };

		public bool IsValid
		{
			get
			{
				bool templateSelected = SelectedTemplate != null;
				bool containsEventNameIllegalCharacters = EventName.ContainsIllegalCharacters();

				bool eventNameSpecified = !String.IsNullOrWhiteSpace(EventName) && !containsEventNameIllegalCharacters;
				if (!eventNameSpecified)
				{
					eventNameTextBox.ValidationText = containsEventNameIllegalCharacters ? "Specify an event name without illegal characters like ('/', '?', etc)" : "Specify an Event name";
					eventNameTextBox.ValidationState = UIValidationState.Invalid;
					return false;
				}

				bool isEventNameUnique = helpers.EventManager.GetEventByName(EventName) is null;
				if (!isEventNameUnique)
				{
					eventNameTextBox.ValidationText = "This name is already in use by another Event";
					eventNameTextBox.ValidationState = UIValidationState.Invalid;
					return false;
				}

				eventNameTextBox.ValidationState = UIValidationState.Valid;

				bool isVizremResourceAvailabilityValid = CheckVizremResourceAvailability();

				bool startTimeIsInTheFuture = StartTime > DateTime.Now.AddMinutes(Order.StartInTheFutureDelayInMinutes);

				StartTimeDateTimePicker.ValidationState = startTimeIsInTheFuture ? UIValidationState.Valid : UIValidationState.Invalid;
				StartTimeDateTimePicker.ValidationText = $"Start time should be at least {Order.StartInTheFutureDelayInMinutes} minutes in the future";

				return templateSelected && isVizremResourceAvailabilityValid && startTimeIsInTheFuture;
			}
		}

		public EventTemplate SelectedTemplate
		{
			get
			{
				if (String.IsNullOrEmpty(TemplatesRadioButtonList.Selected)) return null;

				return OrderTypeVizremCheckBox.IsChecked ? vizremTemplates.FirstOrDefault(x => x.Name == TemplatesRadioButtonList.Selected) : templates.FirstOrDefault(x => x.Name == TemplatesRadioButtonList.Selected);
			}
		}

		private int OrderTemplatesDepth
		{
			get
			{
				if (SelectedTemplate == null || LinkedOrderTemplates == null || !LinkedOrderTemplates.Any()) return 0;
				return !OrderTypeVizremCheckBox.IsChecked ? LinkedOrderTemplates.Max(x => GetDepth(x.Sources, 2)) : LinkedOrderTemplates.Max(x => GetDepth(x.Sources, 5));
			}
		}

		private EventSelection EventSelection => eventSelectionRadioButtonList.Selected.GetEnumValue<EventSelection>();

		private string EventName
		{
			get
			{
				if (EventSelection == EventSelection.NewEvent) return eventNameTextBox.Text;
				else return allFutureEvents.Single(e => e.Name == availableExistingEventsDropdown.Selected).Name;
			}
		}

		public DateTime StartTime => StartTimeDateTimePicker.DateTime;

		public DateTime EndTime => EndTimeDateTimePicker.DateTime;

		public List<Order> CreateLinkedOrders(Event eventInfo)
		{
			var orders = new List<Order>();

			foreach (OrderTemplate orderTemplate in LinkedOrderTemplates)
			{
				helpers.Log(nameof(UseEventTemplateDialog), nameof(CreateLinkedOrders), "Template Company: " + orderTemplate.Company);

				string orderName = $"{eventInfo.Name}.{orderTemplate.Name}";
				int suffix = 0;
				string orderNameWithSuffix = orderName;
				while (helpers.ReservationManager.GetReservation(orderNameWithSuffix) != null)
				{
					suffix++;
					orderNameWithSuffix = $"{orderName}_{suffix}";
				}

				var orderStartTime = StartTime.Add(SelectedTemplate.OrderOffsets[orderTemplate.Id]);

				var order = Order.FromTemplate(helpers, orderTemplate, orderName, orderStartTime);

				// Event will be linked to the order when continue button is pressed.
				order.CreatedByUserName = helpers.Engine.UserLoginName;
				order.LastUpdatedBy = helpers.Engine.UserLoginName;
				order.Status = DetermineOrderStatus(order, eventInfo.Status);

				var securityViewIdsToSet = eventInfo.SecurityViewIds.Concat(userInfo.UserGroups.Select(x => Convert.ToInt32(x.ID)));

				helpers.Log(nameof(UseEventTemplateDialog), nameof(CreateLinkedOrders), $"Setting order {order.Name} security view IDs to '{string.Join(", ", securityViewIdsToSet)}'");

				order.SetSecurityViewIds(securityViewIdsToSet);

				orders.Add(order);
			}

			return orders;
		}

		public YLE.Order.Status DetermineOrderStatus(Order order, YLE.Event.Status eventStatus)
		{
			if (eventStatus == YLE.Event.Status.Preliminary)
			{
				return YLE.Order.Status.Preliminary;
			}
			else if (order.HasEurovisionServices || !order.CanBeBooked)
			{
				return YLE.Order.Status.Preliminary;
			}
			else if (userInfo.IsMcrUser)
			{
				return YLE.Order.Status.Confirmed;
			}
			else
			{
				return YLE.Order.Status.Planned;
			}
		}

		public void GatherOccupationsOfSelectedTemplate()
		{
			allOccupiedOrderAndResources.Clear();

			foreach (var service in SelectedTemplateLinkedOrders.SelectMany(o => o.AllServices))
			{
				if (!service.AllCurrentlyAssignedResourcesAreAvailable(helpers, out var unavailableResources))
				{
					helpers.Log(nameof(UseEventTemplateDialog), nameof(GatherOccupationsOfSelectedTemplate), $"Service {service.Name} is using unavailable resources {string.Join(", ", unavailableResources.Select(r => r.Name))}, linked vizrem order will not be created");

					GatherOccupiedOrdersAndResources(service, unavailableResources);
				}
			}
		}

		private void InitializeEventSubscriptions()
		{
			eventNameTextBox.FocusLost += (s, a) =>
			{
				UpdateLinkedOrderNames();
				GenerateUI();
			};

			TemplatesRadioButtonList.Changed += (s, a) =>
			{
				UpdateEndTime();
				UpdateLinkedOrderTemplates();
				UpdateLinkedOrders();
				GenerateUI();
				UpdateWidgetVisibility();
			};

			StartTimeDateTimePicker.Changed += (o, e) => StartTimeDateTimePicker_Changed();

			OrderTypeVizremCheckBox.Changed += (s, a) =>
			{
				UpdateTemplateList();
				UpdateEndTime();
				UpdateLinkedOrderTemplates();
				UpdateLinkedOrders();
				GenerateUI();
				UpdateWidgetVisibility();
			};
		}

		private void StartTimeDateTimePicker_Changed()
		{
			UpdateEndTime();
			UpdateLinkedOrders();
			GatherOccupationsOfSelectedTemplate();
			GenerateUI();
			UpdateWidgetVisibility();
		}

		private void GetAllFutureEvents()
		{
			var existingEvents = helpers.EventManager.GetAllEventsEndingInTheFuture();

			if (!userInfo.IsMcrUser) existingEvents = existingEvents.Where(e => userInfo.AllUserCompanies.Contains(e.Company));

			allFutureEvents.AddRange(existingEvents);
		}

		private void InitializeWidgets()
		{
			eventSelectionRadioButtonList.Changed += (o, e) =>
			{
				StartTimeDateTimePicker.DateTime = SelectedEvent.Start;
				StartTimeDateTimePicker_Changed();
				UpdateWidgetVisibility();
			};

			availableExistingEventsDropdown.Options = allFutureEvents.Select(e => e.Name).OrderBy(n => n);
			availableExistingEventsDropdown.Selected = availableExistingEventsDropdown.Options.First();
			availableExistingEventsDropdown.Changed += AvailableExistingEventsDropdown_Changed;
		}

		private void AvailableExistingEventsDropdown_Changed(object sender, DropDown.DropDownChangedEventArgs e)
		{
			StartTimeDateTimePicker.DateTime = SelectedEvent.Start;

			StartTimeDateTimePicker_Changed();
		}

		private void UpdateWidgetVisibility()
		{
			availableExistingEventsDropdown.IsVisible = EventSelection == EventSelection.OtherExistingEvent;

			EventNameLabel.IsVisible = EventSelection == EventSelection.NewEvent;
			eventNameTextBox.IsVisible = EventSelection == EventSelection.NewEvent;

			EndTimeLabel.IsVisible = EventSelection == EventSelection.NewEvent;
			EndTimeDateTimePicker.IsVisible = EventSelection == EventSelection.NewEvent;

			EditSelectedTemplateButton.IsVisible = EventSubType == EventSubType.Vizrem;

			OccupiedOrdersValidationLabel.IsVisible = EventSubType == EventSubType.Vizrem && allOccupiedOrderAndResources.Any();
		}

		private void GatherOccupiedOrdersAndResources(YLE.Service.Service service, List<FunctionResource> unavailableResources)
		{
			foreach (var unavailableResource in unavailableResources)
			{
				var occupiedResource = new OccupiedResource(unavailableResource);
				var occupyingServices = helpers.ResourceManager.GetOccupyingServices(unavailableResource, service.StartWithPreRoll, service.EndWithPostRoll, Guid.Empty);

				occupiedResource.OccupyingServices = occupyingServices;

				foreach (var occupyingService in occupiedResource.OccupyingServices)
				{
					var linkedOrder = occupyingService.Orders[0];
					if (!allOccupiedOrderAndResources.TryGetValue(linkedOrder, out var occupiedDataCollection))
					{
						allOccupiedOrderAndResources.Add(linkedOrder, new List<OccupiedResource> { occupiedResource });
					}
					else
					{
						occupiedDataCollection.Add(occupiedResource);
					}
				}
			}
		}

		private bool CheckVizremResourceAvailability()
		{
			if (EventSubType == EventSubType.Vizrem)
			{
				GatherOccupationsOfSelectedTemplate();
				if (allOccupiedOrderAndResources.Any())
				{
					GenerateUI();
					return false;
				}
			}

			return true;
		}

		private void UpdateTemplateList()
		{
			IEnumerable<string> options = OrderTypeVizremCheckBox.IsChecked ? vizremTemplates.Select(x => x.Name) : templates.Select(x => x.Name);

			TemplatesRadioButtonList.Options = options.OrderBy(x => x);
			TemplatesRadioButtonList.Selected = TemplatesRadioButtonList.Options.FirstOrDefault();
		}

		private void UpdateEndTime()
		{
			if (SelectedTemplate == null)
			{
				EndTimeDateTimePicker.DateTime = StartTime;
			}
			else
			{
				EndTimeDateTimePicker.DateTime = StartTime.Add(SelectedTemplate.Duration);
			}
		}

		private void UpdateLinkedOrderNames()
		{
			foreach (var linkedOrder in SelectedTemplateLinkedOrders)
			{
				if (linkedOrder == null) continue;

				var rawOrderName = linkedOrder.Name.Split('.').Last();

				linkedOrder.ManualName = $"{EventName}.{rawOrderName}";
			}
		}

		private void UpdateLinkedOrderTemplates()
		{
			if (SelectedTemplate == null)
			{
				LinkedOrderTemplates = new List<OrderTemplate>();
				return;
			}

			LinkedOrderTemplates = helpers.ContractManager.GetLinkedOrderTemplates(SelectedTemplate.Id);
		}

		private void UpdateLinkedOrders()
		{
			if (EventSubType == EventSubType.Normal) return;

			SelectedTemplateLinkedOrders = CreateLinkedOrders(SelectedEvent);
		}

		private void GenerateUI()
		{
			Clear();

			int row = -1;
			int depth = OrderTemplatesDepth;
			int valueColumn = 2 + depth;

			AddWidget(OrderTypeTitle, ++row, 0);
			AddWidget(OrderTypeVizremCheckBox, row, 1, 1, valueColumn);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(TemplatesTitle, ++row, 0, 1, valueColumn);

			AddWidget(TemplatesRadioButtonList, ++row, 0, 1, valueColumn);

			AddWidget(new WhiteSpace(), ++row, 0);

			if (SelectedTemplate != null && LinkedOrderTemplates != null && LinkedOrderTemplates.Any())
			{
				AddWidget(OrdersCollapseButton, ++row, 0);
				AddWidget(OrdersTitle, row, 1, 1, depth + 1);

				OrdersCollapseButton.LinkedWidgets.Clear();
				OrdersCollapseButton.LinkedWidgets.AddRange(AddOrderWidgets(LinkedOrderTemplates.OrderBy(x => x.Name).ToList(), 1, valueColumn, ref row));
			}

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(EventDetailsTitle, ++row, 0, 1, valueColumn + 1);

			AddWidget(EventSelectionLabel, ++row, 0, 1, valueColumn);

			AddWidget(eventSelectionRadioButtonList, ++row, 0, 1, 2);

			AddWidget(availableExistingEventsDropdown, ++row, 0, 1, 20);

			AddWidget(EventNameLabel, ++row, 0, 1, valueColumn);
			AddWidget(eventNameTextBox, row, valueColumn);

			AddWidget(StartTimeLabel, ++row, 0, 1, valueColumn);
			AddWidget(StartTimeDateTimePicker, row, valueColumn);

			AddWidget(EndTimeLabel, ++row, 0, 1, valueColumn);
			AddWidget(EndTimeDateTimePicker, row, valueColumn);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(OccupiedOrdersValidationLabel, ++row, 0, 1, valueColumn + 1, HorizontalAlignment.Right, VerticalAlignment.Center);
			AddWidget(ContinueButton, ++row, 0, 1, valueColumn + 1, HorizontalAlignment.Right, VerticalAlignment.Center);
			AddWidget(EditSelectedTemplateButton, ++row, 0, 1, valueColumn + 1, HorizontalAlignment.Right, VerticalAlignment.Center);

			AddWidget(new WhiteSpace(), ++row, 0);

			if (SelectedTemplate != null && allOccupiedOrderAndResources != null && allOccupiedOrderAndResources.Any())
			{
				AddWidget(OccupiedOrdersCollapseButton, ++row, 0);
				AddWidget(OccupiedOrdersTitle, row, 1, 1, depth + 1);

				OccupiedOrdersCollapseButton.LinkedWidgets.Clear();
				OccupiedOrdersCollapseButton.LinkedWidgets.AddRange(AddOrderWidgets(1, valueColumn, ref row));
				OccupiedOrdersCollapseButton.Collapse();
			}

			SetColumnWidth(0, 40);
			for (int i = 1; i <= depth; i++) SetColumnWidth(0, 25);
		}

		private List<Widget> AddOrderWidgets(int columnIdx, int valueColumn, ref int row)
		{
			List<Widget> widgets = new List<Widget>();
			if (allOccupiedOrderAndResources == null || !allOccupiedOrderAndResources.Any()) return widgets;

			foreach (var orderAndOccupiedDataPair in allOccupiedOrderAndResources)
			{
				var order = orderAndOccupiedDataPair.Key;
				var occupiedData = orderAndOccupiedDataPair.Value;

				Label orderNameLabel = new Label(order.Name) { IsVisible = !OrdersCollapseButton.IsCollapsed, Style = TextStyle.Heading };
				Label orderTimingLabel = new Label($"{order.Start.ToLocalTime().ToString("g")} - {order.End.ToLocalTime().ToString("g")}") { IsVisible = !OrdersCollapseButton.IsCollapsed };

				widgets.Add(orderNameLabel);
				widgets.Add(orderTimingLabel);

				AddWidget(orderNameLabel, ++row, columnIdx, 1, valueColumn - columnIdx);
				AddWidget(orderTimingLabel, row, valueColumn, HorizontalAlignment.Right);

				widgets.AddRange(AddServiceWidgets(occupiedData, columnIdx + 1, valueColumn, ref row));
			}

			return widgets;
		}

		private List<Widget> AddOrderWidgets(List<OrderTemplate> templates, int columnIdx, int valueColumn, ref int row)
		{
			List<Widget> widgets = new List<Widget>();
			if (templates == null || !templates.Any()) return widgets;

			foreach (OrderTemplate template in templates)
			{
				TimeSpan orderStartOffset = SelectedTemplate.OrderOffsets[template.Id];
				DateTime start = StartTimeDateTimePicker.DateTime.Add(orderStartOffset);
				DateTime end = start.Add(template.Duration);

				Label orderNameLabel = new Label(template.Name) { IsVisible = !OrdersCollapseButton.IsCollapsed, Style = TextStyle.Heading };
				Label orderTimingLabel = new Label($"{start.ToString("g")} - {end.ToString("g")}") { IsVisible = !OrdersCollapseButton.IsCollapsed };

				widgets.Add(orderNameLabel);
				widgets.Add(orderTimingLabel);

				AddWidget(orderNameLabel, ++row, columnIdx, 1, valueColumn - columnIdx);
				AddWidget(orderTimingLabel, row, valueColumn, HorizontalAlignment.Right);

				widgets.AddRange(AddServiceWidgets(template, template.Sources, columnIdx + 1, valueColumn, ref row));
			}

			return widgets;
		}

		private List<Widget> AddServiceWidgets(OrderTemplate orderTemplate, List<ServiceTemplate> serviceTemplates, int columnIdx, int valueColumn, ref int row)
		{
			List<Widget> widgets = new List<Widget>();
			if (serviceTemplates == null || !serviceTemplates.Any()) return widgets;

			TimeSpan orderStartOffset = SelectedTemplate.OrderOffsets[orderTemplate.Id];
			foreach (ServiceTemplate serviceTemplate in serviceTemplates)
			{
				TimeSpan serviceStartOffset = orderTemplate.ServiceOffsets[serviceTemplate.Id];
				DateTime start = StartTimeDateTimePicker.DateTime.Add(orderStartOffset).Add(serviceStartOffset);
				DateTime end = start.Add(serviceTemplate.Duration);

				Label serviceNameLabel = new Label(serviceTemplate.ServiceDefinitionName) { IsVisible = !OrdersCollapseButton.IsCollapsed };
				Label serviceTimingLabel = new Label($"{start.ToString("g")} - {end.ToString("g")}") { IsVisible = !OrdersCollapseButton.IsCollapsed };

				widgets.Add(serviceNameLabel);
				widgets.Add(serviceTimingLabel);

				AddWidget(serviceNameLabel, ++row, columnIdx, 1, valueColumn - columnIdx);
				AddWidget(serviceTimingLabel, row, valueColumn, HorizontalAlignment.Right);

				widgets.AddRange(AddServiceWidgets(orderTemplate, serviceTemplate.Children, columnIdx + 1, valueColumn, ref row));
			}

			return widgets;
		}

		private List<Widget> AddServiceWidgets(List<OccupiedResource> occupiedData, int columnIdx, int valueColumn, ref int row)
		{
			List<Widget> widgets = new List<Widget>();
			if (occupiedData is null) return widgets;

			foreach (var occupiedResourceData in occupiedData)
			{
				var linkedService = occupiedResourceData.OccupyingServices[0];

				Label serviceNameLabel = new Label(occupiedResourceData.Name) { IsVisible = !OrdersCollapseButton.IsCollapsed };
				Label serviceTimingLabel = new Label($"{linkedService.Service.Start.ToLocalTime().ToString("g")} - {linkedService.Service.End.ToLocalTime().ToString("g")}") { IsVisible = !OrdersCollapseButton.IsCollapsed };

				widgets.Add(serviceNameLabel);
				widgets.Add(serviceTimingLabel);

				AddWidget(serviceNameLabel, ++row, columnIdx, 1, valueColumn - columnIdx);
				AddWidget(serviceTimingLabel, row, valueColumn, HorizontalAlignment.Right);
			}

			return widgets;
		}

		private static int GetDepth(IEnumerable<ServiceTemplate> templates, int currentDepth)
		{
			if (templates == null || !templates.Any()) return currentDepth;
			int maxDepth = currentDepth;
			foreach (var template in templates)
			{
				int newDepth = GetDepth(template.Children, currentDepth++);
				if (newDepth > maxDepth) maxDepth = newDepth;
			}

			return maxDepth;
		}
	}
}
