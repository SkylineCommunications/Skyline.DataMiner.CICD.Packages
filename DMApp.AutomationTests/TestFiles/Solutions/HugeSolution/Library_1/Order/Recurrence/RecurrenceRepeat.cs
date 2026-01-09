namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence
{
	using System;
	using Library_1.Utilities;
	using Newtonsoft.Json;

	public class RecurrenceRepeat : ICloneable
	{
		private SelectableOption selectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption;
		private SelectableOption selectableUmpteenthDayOfTheMonthOption;
		private DaysOfTheWeek day;

		public RecurrenceRepeat()
		{
			RepeatType = RecurrenceRepeatType.None;
			UmpteenthDayOfTheMonth = 0;
			Day = DaysOfTheWeek.None;
			UmpteenthOccurrenceOfWeekDayOfTheMonth = 0;
		}

		private RecurrenceRepeat(RecurrenceRepeat other)
		{
			CloneHelper.CloneProperties(other, this);
		}

		public RecurrenceRepeatType RepeatType { get; set; }

		/// <summary>
		/// Only relevant in case <see cref="RepeatType"/> has value <see cref="RecurrenceRepeatType.UmpteenthDayOfTheMonth"/>. 
		/// </summary>
		public int UmpteenthDayOfTheMonth { get; set; }

		/// <summary>
		/// Only relevant in case <see cref="RepeatType"/> has value <see cref="RecurrenceRepeatType.DaysOfTheWeek"/> or <see cref="RecurrenceRepeatType.UmpteenthWeekDayOfTheMonth"/>. 
		/// </summary>
		public DaysOfTheWeek Day
		{
			get => day;
			set
			{
				day = value;
				DayChanged?.Invoke(this, day);
			}
		}

		public event EventHandler<DaysOfTheWeek> DayChanged;

		/// <summary>
		/// Only relevant in case <see cref="RepeatType"/> has value <see cref="RecurrenceRepeatType.UmpteenthWeekDayOfTheMonth"/>. 
		/// </summary>
		public int UmpteenthOccurrenceOfWeekDayOfTheMonth { get; set; }

		/// <summary>
		/// Only used by UI.
		/// </summary>
		[JsonIgnore]
		public SelectableOption SelectableUmpteenthDayOfTheMonthOption
		{
			get => selectableUmpteenthDayOfTheMonthOption;
			set
			{
				selectableUmpteenthDayOfTheMonthOption = value;
				SelectableOptionChanged?.Invoke(this, value);
			}
		}

		/// <summary>
		/// Only used by UI.
		/// </summary>
		[JsonIgnore]
		public SelectableOption SelectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption
		{
			get => selectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption;
			set
			{
				selectableUmpteenthOccurrenceOfWeekDayOfTheMonthOption = value;
				SelectableOptionChanged?.Invoke(this, value);
			}
		}

		public event EventHandler<SelectableOption> SelectableOptionChanged;

		public class SelectableOption
		{
			public string DisplayValue { get; set; }

			public DaysOfTheWeek Day { get; set; }

			public int UmpteethDay { get; set; }
		}

		public object Clone()
		{
			return new RecurrenceRepeat(this);
		}
	}
}
