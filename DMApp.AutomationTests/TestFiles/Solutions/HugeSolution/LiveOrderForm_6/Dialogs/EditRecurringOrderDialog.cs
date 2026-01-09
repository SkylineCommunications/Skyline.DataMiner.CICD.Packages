namespace LiveOrderForm_6.Dialogs
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class EditRecurringOrderDialog : Dialog	
	{
		private readonly Label line1 = new Label("This order is part of a recurring sequence.");
		private readonly Label line2 = new Label("What do you want to do?");
		private readonly Label warning = new Label("Warning: editing the recurring sequence will overwrite all previously made changes to individual orders."){IsVisible = false};

		private readonly RadioButtonList editOptions = new RadioButtonList(EnumExtensions.GetEnumDescriptions<RecurrenceAction>(), RecurrenceAction.ThisOrderOnly.GetDescription());
		private readonly Button okButton = new Button("OK") { Width = 100, Style = ButtonStyle.CallToAction };

		public EditRecurringOrderDialog(IEngine engine) : base(engine)
		{
			Title = "Edit Recurring Order";

			editOptions.Changed += (o, e) => warning.IsVisible = e.SelectedValue == RecurrenceAction.AllOrdersInSequence.GetDescription();

			okButton.Pressed += (o, e) => OkButtonPressed?.Invoke(this, editOptions.Selected.GetEnumValue<RecurrenceAction>());

			GenerateUi();
		}

		public event EventHandler<RecurrenceAction> OkButtonPressed;

		private void GenerateUi()
		{
			int row = -1;

			AddWidget(line1, new WidgetLayout(++row, 0));

			AddWidget(line2, new WidgetLayout(++row, 0));

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(editOptions, new WidgetLayout(++row, 0));

			AddWidget(warning, new WidgetLayout(++row, 0));

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));

			AddWidget(okButton, new WidgetLayout(++row, 0, HorizontalAlignment.Center));

			SetColumnWidth(0, 400);
		}
	}
}