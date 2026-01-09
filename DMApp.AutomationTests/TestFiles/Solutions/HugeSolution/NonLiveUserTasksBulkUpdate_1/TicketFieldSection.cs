namespace NonLiveUserTasksBulkUpdate_1
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public sealed class TicketFieldSection<TType> : Section
	{
		private readonly Label label;
		private readonly CheckBox leaveUnchangedCheckBox = new CheckBox("Leave Unchanged") { IsChecked = true };
		private readonly IYleInteractiveWidget inputWidget;

		public TicketFieldSection(string labelText, IYleInteractiveWidget inputWidget)
		{
			label = new Label(labelText);
			this.inputWidget = inputWidget;
			inputWidget.IsEnabled = false;

			leaveUnchangedCheckBox.Changed += LeaveUnchangedCheckBox_Changed;

			GenerateUi();
		}

		public TicketFieldSection(string labelText) : this (labelText, Mapping.TypeToWidget[typeof(TType)].Invoke())
		{
		}

		public string LabelValue => label.Text;

		public bool LeaveUnchanged => leaveUnchangedCheckBox.IsChecked;

		public TType InputValue => (TType)inputWidget.Value;

		private void GenerateUi()
		{
			Clear();

			AddWidget(label, 0, 0);
			AddWidget((InteractiveWidget)inputWidget, 0, 1);
			AddWidget(leaveUnchangedCheckBox, 0, 2);
		}

		private void LeaveUnchangedCheckBox_Changed(object sender, CheckBox.CheckBoxChangedEventArgs e)
		{
			inputWidget.IsEnabled = !leaveUnchangedCheckBox.IsChecked;
		}
	}
}
