namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class RecurrenceSection : Section
	{
		private readonly RecurringSequenceInfo recurringSequenceInfo;

		private readonly CheckBox recurringCheckBox = new CheckBox("Recurring");

		private readonly Label repeatEveryLabel = new Label("Repeat every") { Width = 100 };
		private readonly Numeric repeatEveryNumeric = new Numeric { Minimum = 1, Width = 100 };
		private readonly DropDown repeatEveryDropDown = new DropDown(EnumExtensions.GetEnumDescriptions<RecurrenceFrequencyUnit>()) { Width = 200 };

		private readonly Label repeatOnLabel = new Label("Repeat on");
		private readonly Label repeatOnLabel2 = new Label("Repeat on");
		private RadioButtonList dayOfTheMonthRadioButtonList;
		private readonly List<CheckBox> daysOfTheWeekCheckboxes = new List<CheckBox>();

		private readonly Label endsLabel = new Label("Ends");
		private readonly RadioButtonList endsRadioButtonList = new RadioButtonList(EnumExtensions.GetEnumDescriptions<EndingType>(), EndingType.SpecificDate.GetDescription());
		private DateTimePicker endsOnDateTimePicker;
		private readonly Numeric endsAfterNumeric = new Numeric(13) { Minimum = 1 };
		private readonly Label endsAfterUnitLabel = new Label("Repeats");

		public RecurrenceSection(RecurringSequenceInfo recurringSequenceInfo)
		{
			this.recurringSequenceInfo = recurringSequenceInfo ?? throw new ArgumentNullException(nameof(recurringSequenceInfo));

			Initialize();
			GenerateUi();
			HandleVisibilityAndEnabledUpdate();
		}

		public new bool IsVisible
		{
			get => base.IsVisible;

			set
			{
				base.IsVisible = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public new bool IsEnabled
		{
			get => base.IsEnabled;

			set
			{
				base.IsEnabled = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public event EventHandler<bool> RecurrenceCheckBoxChanged;

		public event EventHandler<int> RepeatEveryAmountChanged;

		public RecurrenceFrequencyUnit RepeatEveryUnit => repeatEveryDropDown.Selected.GetEnumValue<RecurrenceFrequencyUnit>();

		public event EventHandler<RecurrenceFrequencyUnit> RepeatEveryUnitChanged;

		public event EventHandler<RecurrenceRepeatType> RepeatTypeChanged;

		public event EventHandler<int> UmpteenthDayOfTheMonthChanged;

		public event EventHandler<DaysOfTheWeek> DayOfTheWeekChanged;

		public event EventHandler<int> UmpteenthOccurrenceOfWeekDayOfTheMonthChanged;

		public event EventHandler<EndingType> EndingTypeChanged;

		public event EventHandler<DateTime> EndingDateTimeChanged;

		public event EventHandler<int> AmountOfRepeatsChanged;

		private RecurrenceFrequency RecurrenceFrequency => recurringSequenceInfo.Recurrence.RecurrenceFrequency;
		
		private RecurrenceRepeat RecurrenceRepeat => recurringSequenceInfo.Recurrence.RecurrenceRepeat;
		
		private RecurrenceEnding RecurrenceEnding => recurringSequenceInfo.Recurrence.RecurrenceEnding;

		private RecurrenceRepeatType RepeatType	
		{
			get
			{
				switch (repeatEveryDropDown.Selected.GetEnumValue<RecurrenceFrequencyUnit>())
				{
					case RecurrenceFrequencyUnit.Weeks:
						return RecurrenceRepeatType.DaysOfTheWeek;
					case RecurrenceFrequencyUnit.Days:
					case RecurrenceFrequencyUnit.Years:
						return RecurrenceRepeatType.None;
					case RecurrenceFrequencyUnit.Months when !string.IsNullOrWhiteSpace(dayOfTheMonthRadioButtonList.Selected) && dayOfTheMonthRadioButtonList.Selected == RecurrenceRepeat.SelectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption.DisplayValue:
						return RecurrenceRepeatType.UmpteenthWeekDayOfTheMonth;
					case RecurrenceFrequencyUnit.Months when !string.IsNullOrWhiteSpace(dayOfTheMonthRadioButtonList.Selected) && dayOfTheMonthRadioButtonList.Selected == RecurrenceRepeat.SelectableUmpteenthDayOfTheMonthOption.DisplayValue:
						return RecurrenceRepeatType.UmpteenthDayOfTheMonth;
					default:
						return RecurrenceRepeatType.None;
				}
			}
		}

		public void RegenerateUi()
		{
			Clear();
			GenerateUi();
		}

		private void Initialize()
		{
			InitializeWidgets();
			SubscribeToWidgets();
			SubscribeToOrder();
		}
		private void InitializeWidgets()
		{
			recurringCheckBox.IsChecked = recurringSequenceInfo.Recurrence.IsConfigured;

			repeatEveryNumeric.Value = RecurrenceFrequency.Frequency;
			repeatEveryDropDown.Selected = RecurrenceFrequency.FrequencyUnit.GetDescription();

			foreach (var day in EnumExtensions.GetEnumDescriptions<DaysOfTheWeek>())
			{
				daysOfTheWeekCheckboxes.Add(new CheckBox(day) { IsChecked = RecurrenceRepeat.Day.HasFlag(day.GetEnumValue<DaysOfTheWeek>()) });
			}

			dayOfTheMonthRadioButtonList = new RadioButtonList();

			endsRadioButtonList.Selected = RecurrenceEnding.EndingType.GetDescription();

			endsOnDateTimePicker = new YleDateTimePicker(RecurrenceEnding.EndingDateTime) { IsTimePickerVisible = false, DateTimeFormat = DateTimeFormat.ShortDate, Minimum = DateTime.Now };

			endsAfterNumeric.Value = RecurrenceEnding.AmountOfRepeats;		
		}

		private void SubscribeToWidgets()
		{
			recurringCheckBox.Changed += (o, e) =>
			{
				RecurrenceCheckBoxChanged?.Invoke(this, e.IsChecked);
				HandleVisibilityAndEnabledUpdate();
			};

			repeatEveryNumeric.Changed += (o, e) => RepeatEveryAmountChanged?.Invoke(this, (int)repeatEveryNumeric.Value);
			repeatEveryDropDown.Changed += (o, e) =>
			{
				RepeatEveryUnitChanged?.Invoke(this, RepeatEveryUnit);
				InvokeEventsForRecurrenceRepeat();
				HandleVisibilityAndEnabledUpdate();
			};

			foreach (var checkBox in daysOfTheWeekCheckboxes)
			{
				checkBox.Changed += (s, e) =>
				{
					var days = DaysOfTheWeek.None;
					foreach (var checkBox2 in daysOfTheWeekCheckboxes)
					{
						if (checkBox2.IsChecked)
						{
							days |= checkBox2.Text.GetEnumValue<DaysOfTheWeek>();
						}
					}

					DayOfTheWeekChanged?.Invoke(this, days);
				};
			}

			dayOfTheMonthRadioButtonList.Changed += (o, e) => InvokeEventsForRecurrenceRepeat();

			endsRadioButtonList.Changed += (o, e) =>
			{
				EndingTypeChanged?.Invoke(this, e.SelectedValue.GetEnumValue<EndingType>());
				HandleVisibilityAndEnabledUpdate();
			};

			endsOnDateTimePicker.Changed += (o, e) => EndingDateTimeChanged?.Invoke(this, endsOnDateTimePicker.DateTime);

			endsAfterNumeric.Changed += (o, e) => AmountOfRepeatsChanged?.Invoke(this, (int)e.Value);
		}

		private void SubscribeToOrder()
		{
			recurringSequenceInfo.RecurrenceActionChanged += (o, e) => HandleVisibilityAndEnabledUpdate();

			RecurrenceFrequency.RecurrenceFrequencyUnitChanged += (s, e) =>
			{
				repeatEveryDropDown.Selected = e.GetDescription();
				InvokeEventsForRecurrenceRepeat();
				HandleVisibilityAndEnabledUpdate();
			};

			RecurrenceRepeat.DayChanged += (s, e) =>
			{
				foreach (var checkbox in daysOfTheWeekCheckboxes)
				{
					checkbox.IsChecked = RecurrenceRepeat.Day.HasFlag(checkbox.Text.GetEnumValue<DaysOfTheWeek>());
				}
			};

			RecurrenceRepeat.SelectableOptionChanged += SelectableOption_Changed;
		}

		private void SelectableOption_Changed(object sender, RecurrenceRepeat.SelectableOption newValue)
		{
			if (RecurrenceRepeat.SelectableUmpteenthDayOfTheMonthOption?.DisplayValue is null || RecurrenceRepeat.SelectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption?.DisplayValue is null) return;

			dayOfTheMonthRadioButtonList.Options = new[] { RecurrenceRepeat.SelectableUmpteenthDayOfTheMonthOption.DisplayValue, RecurrenceRepeat.SelectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption.DisplayValue };

			if (RecurrenceRepeat.RepeatType == RecurrenceRepeatType.UmpteenthDayOfTheMonth)
			{
				dayOfTheMonthRadioButtonList.Selected = RecurrenceRepeat.SelectableUmpteenthDayOfTheMonthOption.DisplayValue;
			}
			else if (RecurrenceRepeat.RepeatType == RecurrenceRepeatType.UmpteenthWeekDayOfTheMonth)
			{
				dayOfTheMonthRadioButtonList.Selected = RecurrenceRepeat.SelectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption.DisplayValue;
			}
		}

		private void EndsRadioButtonList_Changed(object sender, RadioButtonList.RadioButtonChangedEventArgs e)
		{
			EndingTypeChanged?.Invoke(this, e.SelectedValue.GetEnumValue<EndingType>());

			HandleVisibilityAndEnabledUpdate();
		}

		private void InvokeEventsForRecurrenceRepeat()
		{
			RepeatTypeChanged?.Invoke(this, RepeatType);

			switch (RepeatType)
			{
				case RecurrenceRepeatType.DaysOfTheWeek:
					var days = DaysOfTheWeek.None;
					foreach (var checkBox in daysOfTheWeekCheckboxes)
					{
						if (checkBox.IsChecked)
						{
							days |= checkBox.Text.GetEnumValue<DaysOfTheWeek>();
						}
					}

					DayOfTheWeekChanged?.Invoke(this, days);
					break;
				case RecurrenceRepeatType.UmpteenthDayOfTheMonth:
					UmpteenthDayOfTheMonthChanged?.Invoke(this, RecurrenceRepeat.SelectableUmpteenthDayOfTheMonthOption.UmpteethDay);
					break;
				case RecurrenceRepeatType.UmpteenthWeekDayOfTheMonth:
					DayOfTheWeekChanged?.Invoke(this, RecurrenceRepeat.SelectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption.Day);
					UmpteenthOccurrenceOfWeekDayOfTheMonthChanged?.Invoke(this, RecurrenceRepeat.SelectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption.UmpteethDay);
					break;
				default:

					break;
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		private void HandleVisibilityAndEnabledUpdate()
        {
			recurringCheckBox.IsVisible = IsVisible;
			recurringCheckBox.IsEnabled = IsEnabled && recurringSequenceInfo.RecurrenceAction != RecurrenceAction.ThisOrderOnly;

			repeatEveryLabel.IsVisible = IsVisible && recurringCheckBox.IsChecked;

			repeatEveryNumeric.IsVisible = IsVisible && recurringCheckBox.IsChecked;
			repeatEveryNumeric.IsEnabled = IsEnabled && recurringSequenceInfo.RecurrenceAction != RecurrenceAction.ThisOrderOnly;

			repeatEveryDropDown.IsVisible = IsVisible && recurringCheckBox.IsChecked;
			repeatEveryDropDown.IsEnabled = IsEnabled && recurringSequenceInfo.RecurrenceAction != RecurrenceAction.ThisOrderOnly;

			repeatOnLabel.IsVisible = IsVisible && recurringCheckBox.IsChecked && recurringSequenceInfo.Recurrence.RecurrenceFrequency.FrequencyUnit == RecurrenceFrequencyUnit.Weeks;

			foreach (var checkBox in daysOfTheWeekCheckboxes)
			{
				checkBox.IsVisible = IsVisible && recurringCheckBox.IsChecked && recurringSequenceInfo.Recurrence.RecurrenceFrequency.FrequencyUnit == RecurrenceFrequencyUnit.Weeks;
				checkBox.IsEnabled = IsEnabled && recurringSequenceInfo.RecurrenceAction != RecurrenceAction.ThisOrderOnly;
			}

			repeatOnLabel2.IsVisible = IsVisible && recurringCheckBox.IsChecked && recurringSequenceInfo.Recurrence.RecurrenceFrequency.FrequencyUnit == RecurrenceFrequencyUnit.Months;

			dayOfTheMonthRadioButtonList.IsVisible = IsVisible && recurringCheckBox.IsChecked && recurringSequenceInfo.Recurrence.RecurrenceFrequency.FrequencyUnit == RecurrenceFrequencyUnit.Months;
			dayOfTheMonthRadioButtonList.IsEnabled = IsEnabled && recurringSequenceInfo.RecurrenceAction != RecurrenceAction.ThisOrderOnly;

			endsLabel.IsVisible = IsVisible && recurringCheckBox.IsChecked;

			endsRadioButtonList.IsVisible = IsVisible && recurringCheckBox.IsChecked;
			endsRadioButtonList.IsEnabled = IsEnabled && recurringSequenceInfo.RecurrenceAction != RecurrenceAction.ThisOrderOnly;

			if (!string.IsNullOrWhiteSpace(endsRadioButtonList.Selected))
			{
				var selectedEnd = endsRadioButtonList.Selected.GetEnumValue<EndingType>();

				endsOnDateTimePicker.IsVisible = IsVisible && recurringCheckBox.IsChecked && selectedEnd == EndingType.SpecificDate;
				endsOnDateTimePicker.IsEnabled = IsEnabled && recurringSequenceInfo.RecurrenceAction != RecurrenceAction.ThisOrderOnly;

				endsAfterNumeric.IsVisible = IsVisible && recurringCheckBox.IsChecked && selectedEnd == EndingType.CertainAmountOfRepeats;
				endsAfterNumeric.IsEnabled = IsEnabled && recurringSequenceInfo.RecurrenceAction != RecurrenceAction.ThisOrderOnly;
			}
			else
			{
				endsOnDateTimePicker.IsVisible = false;
				endsAfterNumeric.IsVisible = false;
			}

			endsAfterUnitLabel.IsVisible = endsAfterNumeric.IsVisible;
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(recurringCheckBox, new WidgetLayout(++row, 0, 1, 3));

			AddWidget(repeatEveryLabel, new WidgetLayout(++row, 0));
			AddWidget(repeatEveryNumeric, new WidgetLayout(row, 1));
			AddWidget(repeatEveryDropDown, new WidgetLayout(row, 2));

			AddWidget(repeatOnLabel, new WidgetLayout(++row, 0));

			--row;
			foreach (var checkBox in daysOfTheWeekCheckboxes)
			{
				AddWidget(checkBox, new WidgetLayout(++row, 1, 1, 2));
			}

			AddWidget(repeatOnLabel2, new WidgetLayout(++row, 0));
			AddWidget(dayOfTheMonthRadioButtonList, new WidgetLayout(row, 1, 1, 2));

			AddWidget(endsLabel, new WidgetLayout(++row, 0));
			AddWidget(endsRadioButtonList, new WidgetLayout(row, 1));
			AddWidget(endsOnDateTimePicker, new WidgetLayout(++row, 1));
			AddWidget(endsAfterNumeric, new WidgetLayout(++row, 1));
			AddWidget(endsAfterUnitLabel, new WidgetLayout(row, 2, verticalAlignment: VerticalAlignment.Center));
		}
	}
}