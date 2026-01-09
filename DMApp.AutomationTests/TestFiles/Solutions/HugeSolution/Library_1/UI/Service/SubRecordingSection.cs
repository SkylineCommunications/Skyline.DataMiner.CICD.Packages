namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

	public class SubRecordingSection : Section
	{
		private readonly Label subRecordingDetailsTitle = new Label("Sub-Recording Details") { Style = TextStyle.Heading, IsVisible = true };
		private readonly Label nameLabel = new Label("Name");
		private readonly Label additionalInfoLabel = new Label("Additional Information");
		private readonly Label timeSlotStartLabel = new Label("Time Slot Start");
		private readonly Label timeSlotEndLabel = new Label("Time Slot End");
		private readonly Label timeSlotDescriptionLabel = new Label("Time Slot Description");

		private readonly SubRecording subRecording;

		[DisplaysProperty(nameof(SubRecording.Name))]
		private YleTextBox nameTextBox;

		[DisplaysProperty(nameof(SubRecording.AdditionalInformation))]
		private YleTextBox additionalInfoTextBox;

		[DisplaysProperty(nameof(SubRecording.EstimatedTimeSlotStart))]
		private YleDateTimePicker estimatedTimeSlotStartDateTimePicker;

		[DisplaysProperty(nameof(SubRecording.EstimatedTimeSlotEnd))]
		private YleDateTimePicker estimatedTimeSlotEndDateTimePicker;

		[DisplaysProperty(nameof(SubRecording.TimeslotDescription))]
		private YleTextBox timeSlotDescriptionTextBox;

		private YleButton deleteSubRecordingButton;

		public SubRecordingSection(SubRecording subRecording)
		{
			this.subRecording = subRecording ?? throw new ArgumentNullException(nameof(subRecording));

			Initialize();
			GenerateUI();
		}

		public Guid SubRecordingId => subRecording.Id;

		public event EventHandler<DisplayedPropertyEventArgs> DisplayedPropertyChanged;

		public event EventHandler DeleteButtonPressed;

		public void RegenerateUI()
		{
			Clear();
			GenerateUI();
		}

		private void Initialize()
		{
			InitializeWidgets();
			SubscribeToWidgets();
			SubscribeToSubRecording();
		}

		private void SubscribeToSubRecording()
		{
			this.SubscribeToDisplayedObjectValidation(subRecording);
		}

		private void SubscribeToWidgets()
		{
			nameTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(nameTextBox)), e.Value));
			additionalInfoTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(additionalInfoTextBox)), e.Value));
			estimatedTimeSlotStartDateTimePicker.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(estimatedTimeSlotStartDateTimePicker)), e.Value));
			estimatedTimeSlotEndDateTimePicker.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(estimatedTimeSlotEndDateTimePicker)), e.Value));
			timeSlotDescriptionTextBox.Changed += (s, e) => DisplayedPropertyChanged?.Invoke(this, new DisplayedPropertyEventArgs(this.GetDisplayedPropertyName(nameof(timeSlotDescriptionTextBox)), e.Value));

			deleteSubRecordingButton.Changed += (s, e) => DeleteButtonPressed?.Invoke(this, EventArgs.Empty);
		}

		private void InitializeWidgets()
		{
			nameTextBox = new YleTextBox(subRecording.Name) { Id = subRecording.Id };
			additionalInfoTextBox = new YleTextBox(subRecording.AdditionalInformation) { Id = subRecording.Id, IsMultiline = true, MaxHeight = 250, MinHeight = 100 };
			estimatedTimeSlotStartDateTimePicker = new YleDateTimePicker(subRecording.EstimatedTimeSlotStart) { Id = subRecording.Id };
			estimatedTimeSlotEndDateTimePicker = new YleDateTimePicker(subRecording.EstimatedTimeSlotEnd) { Id = subRecording.Id };
			timeSlotDescriptionTextBox = new YleTextBox(subRecording.TimeslotDescription) { Id = subRecording.Id };
			deleteSubRecordingButton = new YleButton("Delete") { Id = subRecording.Id, Name = subRecording.Name, Width = 150 };
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(subRecordingDetailsTitle, new WidgetLayout(++row, 0, 1, 5));

			AddWidget(nameLabel, new WidgetLayout(++row, 0, 1, 2));
			AddWidget(nameTextBox, new WidgetLayout(row, 2, 1, 2));

			AddWidget(additionalInfoLabel, new WidgetLayout(++row, 0, 1, 2));
			AddWidget(additionalInfoTextBox, new WidgetLayout(row, 2, 1, 2));

			AddWidget(timeSlotStartLabel, new WidgetLayout(++row, 0, 1, 2));
			AddWidget(estimatedTimeSlotStartDateTimePicker, new WidgetLayout(row, 2, 1, 2));

			AddWidget(timeSlotEndLabel, new WidgetLayout(++row, 0, 1, 2));
			AddWidget(estimatedTimeSlotEndDateTimePicker, new WidgetLayout(row, 2, 1, 2));

			AddWidget(timeSlotDescriptionLabel, new WidgetLayout(++row, 0, 1, 2));
			AddWidget(timeSlotDescriptionTextBox, new WidgetLayout(row, 2, 1, 2));

			AddWidget(deleteSubRecordingButton, new WidgetLayout(++row, 2, 1, 2));
		}
	}
}