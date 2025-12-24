namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Notes
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class AddOrUpdateNoteDialog : Dialog
	{
		private readonly Note existingNote;
		private readonly Page page;

		private readonly Label noteLabel = new Label("Note") { Style = TextStyle.Bold };

		private readonly Label titleLabel = new Label("Title");

		private readonly YleTextBox titleTextBox = new YleTextBox { ValidationText = "Title cannot be empty" };

		private readonly Label dueDateLabel = new Label("Due Date");

		private readonly DateTimePicker dueDateDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(1));

		private readonly Label descriptionLabel = new Label("Description");

		private readonly YleTextBox descriptionTextBox = new YleTextBox { IsMultiline = true, MinHeight = 200 };

		private readonly CheckBox audibleAlarmCheckBox = new CheckBox("Audible Alarm") { IsChecked = false };

		public AddOrUpdateNoteDialog(IEngine engine, Page page, Note note = null) : base((Engine)engine)
		{
			existingNote = note;
			this.page = page;

			Initialize();
			UpdateVisibility();
			GenerateUI();
		}

		public Button ConfirmButton { get; private set; }

		public Button AcknowledgeButton { get; private set; }

		public bool IsValid()
		{
			bool titleIsValid = !String.IsNullOrWhiteSpace(titleTextBox.Text);
			titleTextBox.ValidationState = titleIsValid ? UIValidationState.Valid : UIValidationState.Invalid;

			bool descriptionIsValid = !String.IsNullOrWhiteSpace(descriptionTextBox.Text);
			descriptionTextBox.ValidationState = descriptionIsValid ? UIValidationState.Valid : UIValidationState.Invalid;
            descriptionTextBox.ValidationText = $"Please provide any content";

			return titleIsValid && descriptionIsValid;
		}

		public Note Note
		{
			get
			{
				if (existingNote != null)
				{
					existingNote.Title = titleTextBox.Text;
					existingNote.DueDate = dueDateDateTimePicker.DateTime;
					existingNote.Description = descriptionTextBox.Text;
					existingNote.Status = audibleAlarmCheckBox.IsChecked && existingNote.Status == Status.Open ? Status.Alarm : existingNote.Status;

					return existingNote;
				}
				else
				{
					return new Note
					{
						Page = page,
						Title = titleTextBox.Text,
						DueDate = dueDateDateTimePicker.DateTime,
						Description = descriptionTextBox.Text,
						Status = audibleAlarmCheckBox.IsChecked ? Status.Alarm : Status.Open
					};
				}
			}
		}

		/// <summary>
		/// Initialize all widgets and Note property.
		/// </summary>
		private void Initialize()
		{
			Title = existingNote == null ? "Add Note" : "Update Note";

			ConfirmButton = new Button("Confirm") { Width = 150, Style = ButtonStyle.CallToAction };
			AcknowledgeButton = new Button("Acknowledge") { Width = 150 };

			if (existingNote == null) return;

			titleTextBox.Text = existingNote.Title;
			dueDateDateTimePicker.DateTime = existingNote.DueDate;
			descriptionTextBox.Text = existingNote.Description;
			audibleAlarmCheckBox.IsChecked = existingNote.Status == Status.Alarm || existingNote.Status == Status.AcknowledgedAlarm;
		}

		/// <summary>
		/// Add widgets to the dialog.
		/// </summary>
		private void GenerateUI()
		{
			int row = -1;

			AddWidget(noteLabel, new WidgetLayout(++row, 0, 1, 2));

			AddWidget(titleLabel, new WidgetLayout(++row, 0));
			AddWidget(titleTextBox, new WidgetLayout(row, 1));
			AddWidget(audibleAlarmCheckBox, new WidgetLayout(row, 2));

			AddWidget(dueDateLabel, new WidgetLayout(++row, 0));
			AddWidget(dueDateDateTimePicker, new WidgetLayout(row, 1));

			AddWidget(descriptionLabel, new WidgetLayout(++row, 0));
			AddWidget(descriptionTextBox, new WidgetLayout(row, 1));

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(ConfirmButton, new WidgetLayout(++row, 0, 1, 2));
			AddWidget(AcknowledgeButton, new WidgetLayout(++row, 0, 1, 2));
		}

		private void UpdateVisibility()
        {
			audibleAlarmCheckBox.IsVisible = page == Page.MCR || page == Page.TomUt;
			AcknowledgeButton.IsVisible = (page == Page.MCR || page == Page.TomUt) && audibleAlarmCheckBox.IsChecked && existingNote != null && existingNote.Status == Status.Alarm;
        }
	}
}