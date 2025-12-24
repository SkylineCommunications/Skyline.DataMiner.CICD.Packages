namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class YleDateTimePicker : DateTimePicker, IYleInteractiveWidget
	{
		private bool triggersOnFocusLost = false;

		public YleDateTimePicker(DateTime dateTime) : base(dateTime)
		{
			UpdateSubscriptions(true);
		}

        public YleDateTimePicker() : this(DateTime.Now)
		{
		}

		public Guid Id { get; set; } = Guid.Empty;

		public string Name { get; set; } = string.Empty;

		public Helpers Helpers { get; set; }

		public object Value
		{
			get => DateTime;
			set => DateTime = Convert.ToDateTime(value);
		}

		public new DateTimeFormat DateTimeFormat
		{
			get => base.DateTimeFormat;
			set
            {
				if (value == DateTimeFormat.FullDateTime)
                {
					UpdateSubscriptions(true);
                }
				else
                {
					UpdateSubscriptions(false);
				}

				base.DateTimeFormat = value;
            }
        }

		public new event EventHandler<YleValueWidgetChangedEventArgs> Changed;

		public Predicate<DateTime> ValidationPredicate { get; set; } = dateTime => dateTime > DateTime.Now;

		public bool IsValid
		{
			get
			{
				bool isValid = ValidationPredicate(DateTime);

				ValidationState = isValid ? UIValidationState.Valid : UIValidationState.Invalid;

				return isValid;
			}
		}

		// Workaround for DCP: https://collaboration.dataminer.services/task/210955
		private void UpdateSubscriptions(bool triggersOnFocusLost)
        {
			if (this.triggersOnFocusLost == triggersOnFocusLost) return;

			if (triggersOnFocusLost)
            {
				base.Changed -= YleDateTimePicker_Changed;
				FocusLost += YleDateTimePicker_FocusLost;
			}
			else
            {
				base.Changed += YleDateTimePicker_Changed;
				FocusLost -= YleDateTimePicker_FocusLost;
			}

			this.triggersOnFocusLost = triggersOnFocusLost;
        }

        private void YleDateTimePicker_Changed(object sender, DateTimePickerChangedEventArgs e)
        {
			Helpers?.Log(nameof(YleDateTimePicker), nameof(YleDateTimePicker_Changed), $"USER INPUT: user changed value to {e.DateTime}. DateTimePicker Name='{Name}'. ID='{Id}'");
			Changed?.Invoke(this, new YleValueWidgetChangedEventArgs(Id, e.DateTime));
		}

        private void YleDateTimePicker_FocusLost(object sender, DateTimePickerFocusLostEventArgs e)
		{
			Helpers?.Log(nameof(YleDateTimePicker), nameof(YleDateTimePicker_FocusLost), $"USER INPUT: user changed value to {e.DateTime}. DateTimePicker Name='{Name}'. ID='{Id}'");
			Changed?.Invoke(this, new YleValueWidgetChangedEventArgs(Id, e.DateTime));
		}
	}
}