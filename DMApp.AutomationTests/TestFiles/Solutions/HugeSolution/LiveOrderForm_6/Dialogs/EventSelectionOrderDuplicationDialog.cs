namespace LiveOrderForm_6.Dialogs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public class EventSelectionOrderDuplicationDialog : Dialog
	{
		private EventSelection eventSelection = EventSelection.SameEvent;

		private readonly Label GeneralInfoLabel = new Label("General Information") { Style = TextStyle.Heading };
		private readonly Label orderNameLabel = new Label("Order name:");
		private readonly YleTextBox orderNameTextBox = new YleTextBox(string.Empty);

		private readonly Label startTimeLabel = new Label("Order & Service start time:");
		private readonly Label endTimeLabel = new Label("Order & Service end time:");

		private readonly Label eventSelectionQuestionLabel = new Label("Select event type") { Style = TextStyle.Heading };
		private readonly Label chooseExistingEventLabel = new Label("Choose an existing event:") { IsVisible = false };
		private readonly Label noOtherEventsLabel = new Label("No other existing events available") { IsVisible = false };
		private readonly Label validationLabel = new Label(String.Empty);

		private string[] customizedRadioButtonListOptions;

		private RadioButtonList EventSelectionRadioButtonList;
		private DropDown AvailableExistingEventsDropdown;
		private DateTimePicker startTimeDateTimePicker;
		private DateTimePicker endTimeDateTimePicker;
		private readonly Helpers helpers;
		private readonly Order duplicatingOrder;
		private readonly UserInfo userInfo;

		private readonly List<Event> allFutureEvents = new List<Event>();

		public EventSelectionOrderDuplicationDialog(Helpers helpers, Order duplicatingOrder, UserInfo userInfo) : base(helpers.Engine)
		{
			Title = "Duplicate Configuration";
			this.helpers = helpers;
			this.duplicatingOrder = duplicatingOrder;
			this.userInfo = userInfo;

			Initialize();
		}

		private void Initialize()
		{
			GetAllFutureEvents();
			InitializeWidgets();
			UpdateOrderServiceStartAndEndTime();
			GenerateUI();
			UpdateWidgetVisibility();
			UpdateWidgetAvailability();
		}

		public Button ConfirmButton { get; set; }

		public EventSelection Selection
		{
			get
			{
				return eventSelection;
			}
		}

		public Event SelectedEvent
		{
			get
			{
				if (eventSelection == EventSelection.NewEvent) return null;
				else if (eventSelection == EventSelection.SameEvent) return duplicatingOrder.Event;
				else return allFutureEvents.Single(e => e.Name == AvailableExistingEventsDropdown.Selected);
			}
		}

		public bool IsValid()
		{
			Engine.LogMethodStart(nameof(EventSelectionOrderDuplicationDialog), nameof(IsValid));

			bool isValid = true;

			if (eventSelection == EventSelection.OtherExistingEvent && !AvailableExistingEventsDropdown.Options.Any())
			{
				validationLabel.Text = "No available existing events, please select another option";
				Engine.Log(nameof(EventSelectionOrderDuplicationDialog), nameof(IsValid), validationLabel.Text);
				Engine.LogMethodCompleted(nameof(EventSelectionOrderDuplicationDialog), nameof(IsValid));

				isValid = false;
			}

			isValid &= startTimeDateTimePicker.DateTime < endTimeDateTimePicker.DateTime;
			endTimeDateTimePicker.ValidationState = isValid ? UIValidationState.Valid : UIValidationState.Invalid;
			endTimeDateTimePicker.ValidationText = "The order start time cannot be later than the end time";

			return isValid;
		}

		internal void UpdateRecordingNamesBasedOnOrderName()
		{
			var recordingServices = duplicatingOrder.AllServices.Where(s => s != null && s.Definition?.VirtualPlatform == VirtualPlatform.Recording);
			foreach (var recordingService in recordingServices)
			{
				if (recordingService.RecordingConfiguration != null)
				{
					recordingService.RecordingConfiguration.RecordingName = duplicatingOrder.Name;
				}
			}
		}

		private void InitializeWidgets()
		{
			ConfirmButton = new Button("Confirm") { Width = 200, Style = ButtonStyle.CallToAction };
			startTimeDateTimePicker = new DateTimePicker(duplicatingOrder.Start);
			endTimeDateTimePicker = new DateTimePicker(duplicatingOrder.End);

			startTimeDateTimePicker.Changed += (s, e) => UpdateOrderServiceStartAndEndTime();
			endTimeDateTimePicker.Changed += (s, e) => UpdateOrderServiceStartAndEndTime();

			orderNameTextBox.Text = duplicatingOrder.Name;
			orderNameTextBox.Changed += (o, e) =>
			{
				duplicatingOrder.ManualName = Convert.ToString(e.Value);
			};

			InitializeAvailableExistingEventsDropdown();

			customizedRadioButtonListOptions = new string[] { "Same event " + "(" + duplicatingOrder.Event.Name + ")", "Other event", "New event" };

			EventSelectionRadioButtonList = new RadioButtonList(customizedRadioButtonListOptions, customizedRadioButtonListOptions[0]);
			EventSelectionRadioButtonList.Changed += EventSelectionRadioButtonList_Changed;
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(GeneralInfoLabel, ++row, 0);

			AddWidget(orderNameLabel, ++row, 0);
			AddWidget(orderNameTextBox, row, 1);

			AddWidget(startTimeLabel, ++row, 0);
			AddWidget(startTimeDateTimePicker, row, 1);

			AddWidget(endTimeLabel, ++row, 0);
			AddWidget(endTimeDateTimePicker, row, 1);
			
			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(eventSelectionQuestionLabel, ++row, 0);
			AddWidget(EventSelectionRadioButtonList, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(chooseExistingEventLabel, ++row, 0, 1, 2);
			AddWidget(AvailableExistingEventsDropdown, ++row, 0, 1, 2);
			AddWidget(noOtherEventsLabel, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(ConfirmButton, ++row, 0);
			AddWidget(validationLabel, row + 1, 0);
		}

		private void GetAllFutureEvents()
		{
			var existingEvents = helpers.EventManager.GetAllEventsEndingInTheFuture();

			if (!userInfo.IsMcrUser) existingEvents = existingEvents.Where(e => userInfo.AllUserCompanies.Contains(e.Company));

			allFutureEvents.AddRange(existingEvents);
		}

		private void InitializeAvailableExistingEventsDropdown()
		{
			AvailableExistingEventsDropdown = new DropDown() { IsVisible = false, IsDisplayFilterShown = true, Width = 400 };

			if (allFutureEvents == null) return;

			var allExistingEventNames = allFutureEvents.Select(e => e.Name).ToList();

			AvailableExistingEventsDropdown.Options = allExistingEventNames.OrderBy(n => n);
			AvailableExistingEventsDropdown.Selected = AvailableExistingEventsDropdown.Options.First();
		}

		private void UpdateOrderServiceStartAndEndTime()
		{
			duplicatingOrder.Start = startTimeDateTimePicker.DateTime;
			duplicatingOrder.End = endTimeDateTimePicker.DateTime;

			var duplicatingOrderAllServices = duplicatingOrder.AllServices.Where(x => !x.IsSharedSource);
			foreach (var service in duplicatingOrderAllServices)
			{
				service.Start = startTimeDateTimePicker.DateTime;
				service.End = endTimeDateTimePicker.DateTime;
			}
		}

		private void EventSelectionRadioButtonList_Changed(object sender, RadioButtonList.RadioButtonChangedEventArgs e)
		{
			eventSelection = EnumExtensions.GetEnumValueFromDescription<EventSelection>(e.SelectedValue);

			if (eventSelection != EventSelection.SameEvent)
			{
				EventSelectionRadioButtonList.SetOptions(EnumExtensions.GetEnumDescriptions<EventSelection>());
				EventSelectionRadioButtonList.Selected = EnumExtensions.GetDescriptionFromEnumValue(eventSelection);
			}
			else
			{
				EventSelectionRadioButtonList.Options = customizedRadioButtonListOptions;
				EventSelectionRadioButtonList.Selected = customizedRadioButtonListOptions[0];
			}

			UpdateWidgetVisibility();
		}

		private void UpdateWidgetVisibility()
		{
			noOtherEventsLabel.IsVisible = eventSelection == EventSelection.OtherExistingEvent && !AvailableExistingEventsDropdown.Options.Any();
			AvailableExistingEventsDropdown.IsVisible = eventSelection == EventSelection.OtherExistingEvent;
			chooseExistingEventLabel.IsVisible = AvailableExistingEventsDropdown.IsVisible;
		}

		private void UpdateWidgetAvailability()
		{
			if (duplicatingOrder.IntegrationType == IntegrationType.None) return;
			startTimeDateTimePicker.IsEnabled = false;
			endTimeDateTimePicker.IsEnabled = false;
		}
	}
}