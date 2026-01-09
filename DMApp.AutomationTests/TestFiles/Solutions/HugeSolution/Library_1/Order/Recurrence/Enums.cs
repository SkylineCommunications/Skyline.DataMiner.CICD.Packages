namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence
{
	using System;
	using System.ComponentModel;

	public enum RecurrenceFrequencyUnit
	{
		[Description("day(s)")]
		Days,
		[Description("week(s)")]
		Weeks,
		[Description("month(s)")]
		Months,
		[Description("year(s)")]
		Years
	}

	public enum RecurrenceRepeatType
	{
		None,
		DaysOfTheWeek,
		UmpteenthDayOfTheMonth,
		UmpteenthWeekDayOfTheMonth
	}

	[Flags]
	public enum DaysOfTheWeek
	{
		None = 0,
		[Description("Monday")]
		Monday = 1,
		[Description("Tuesday")]
		Tuesday = 2,
		[Description("Wednesday")]
		Wednesday = 4,
		[Description("Thursday")]
		Thursday = 8,
		[Description("Friday")]
		Friday = 16,
		[Description("Saturday")]
		Saturday = 32,
		[Description("Sunday")]
		Sunday = 64
	}

	public enum EndingType
	{
		[Description("Never")]
		Never,
		[Description("On a specific date")]
		SpecificDate,
		[Description("After certain amount of repeats")]
		CertainAmountOfRepeats
	}

	public enum RecurrenceAction
	{
		New = 0,
		[Description("Edit this order only")]
		ThisOrderOnly = 1,
		[Description("Edit all orders in recurring sequence")]
		AllOrdersInSequence = 2
	}
}
