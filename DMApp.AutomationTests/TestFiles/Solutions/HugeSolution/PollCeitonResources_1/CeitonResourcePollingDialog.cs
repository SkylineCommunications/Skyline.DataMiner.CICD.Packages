namespace PollCeitonResources_1
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Ceiton;
	using Skyline.DataMiner.Utils.YLE.Integrations.Ceiton;

	public class CeitonResourcePollingDialog : Dialog
	{
		private readonly List<CeitonPollRequestSection> requestSections = new List<CeitonPollRequestSection>();

		public CeitonResourcePollingDialog(IEngine engine) : base(engine)
		{
			Title = "Ceiton Resource Polling";

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			SetColumnWidth(0, 40);
			SetColumnWidthAuto(1);
			SetColumnWidthAuto(2);

			int row = -1;
			AddWidget(new Label("Resource Type"), ++row, 0, 1, 2, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(ResourceTypeRadioButtonList, row, 2);

			AddWidget(new Label("Resource ID"), ++row, 0, 1, 2, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(ResourceIdTextBox, row, 2);

			AddWidget(new Label("Start Time"), ++row, 0, 1, 2, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(StartTimeDateTimePicker, row, 2);

			AddWidget(new Label("End Time"), ++row, 0, 1, 2, HorizontalAlignment.Left, VerticalAlignment.Top);
			AddWidget(EndTimeDateTimePicker, row, 2);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(SendRequestButton, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 0);

			foreach (var section in requestSections)
			{
				AddSection(section, ++row, 0);
				row += section.RowCount;
			}
		}

		public bool IsValid()
		{
			bool isResourceTypeValid = SelectedResourceType != null;
			bool isResourceIdValid = !String.IsNullOrWhiteSpace(ResourceIdTextBox.Text);
			bool isTimingValid = StartTimeDateTimePicker.DateTime < EndTimeDateTimePicker.DateTime;

			ResourceIdTextBox.ValidationState = isResourceIdValid ? UIValidationState.Valid : UIValidationState.Invalid;
			StartTimeDateTimePicker.ValidationState = isTimingValid ? UIValidationState.Valid : UIValidationState.Invalid;
			EndTimeDateTimePicker.ValidationState = isTimingValid ? UIValidationState.Valid : UIValidationState.Invalid;

			return isResourceTypeValid && isResourceIdValid && isTimingValid;
		}

		public void AddRequestSection(ExternalRequest request)
		{
			requestSections.Add(new CeitonPollRequestSection(request));
			GenerateUi();
		}

		public ResourceType? SelectedResourceType
		{
			get
			{
				switch (ResourceTypeRadioButtonList.Selected)
				{
					case "Project":
						return ResourceType.Project;
					case "Product":
						return ResourceType.Product;
					default:
						return null;
				}
			}
		}

		public string ResourceId
		{
			get
			{
				return ResourceIdTextBox.Text;
			}
		}

		public DateTime StartTime
		{
			get
			{
				return StartTimeDateTimePicker.DateTime;
			}
		}

		public DateTime EndTime
		{
			get
			{
				return EndTimeDateTimePicker.DateTime;
			}
		}

		private RadioButtonList ResourceTypeRadioButtonList { get; set; } = new RadioButtonList(new [] { "Project", "Product" }, "Project");

		private TextBox ResourceIdTextBox { get; set; } = new TextBox { ValidationText = "Enter a Resource ID" };

		private DateTimePicker StartTimeDateTimePicker { get; set; } = new DateTimePicker(DateTime.Now) { ValidationText = "Start time should be earlier than the end time", IsEnabled = false };

		private DateTimePicker EndTimeDateTimePicker { get; set; } = new DateTimePicker(DateTime.Now.AddHours(1)) { ValidationText = "End time should be after the start time", IsEnabled = false };

		public Button SendRequestButton { get; private set; } = new Button("Send Request") { Style = ButtonStyle.CallToAction };
	}
}