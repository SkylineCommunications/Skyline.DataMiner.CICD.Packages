using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence
{
	using System;
	using System.Collections.Generic;
	using Library_1.Utilities;
	using SLDataGateway.API.Tools;

	public class RecurringSequenceInfo : ICloneable
	{
		private RecurrenceAction recurrenceAction;

		public RecurringSequenceInfo()
		{

		}

		private RecurringSequenceInfo(RecurringSequenceInfo other)
		{
			CloneHelper.CloneProperties(other, this);
		}

		[JsonIgnore]
		public RecurrenceAction RecurrenceAction 
		{ 
			get => recurrenceAction;
			set
			{
				recurrenceAction = value;
				RecurrenceActionChanged?.Invoke(this, recurrenceAction);
			}
		}

		public event EventHandler<RecurrenceAction> RecurrenceActionChanged;

		/// <summary>
		/// The name of the recurring order sequence.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The ID of the recurring order sequence in the order manager.
		/// </summary>
		public Guid Id => TemplateId;

		/// <summary>
		/// The recurrence info for the recurring order sequence.
		/// </summary>
		public Recurrence Recurrence { get; set; } = new Recurrence();

		/// <summary>
		/// The key of the template for the recurring order in the contract manager.
		/// </summary>
		public Guid TemplateId { get; set; }

		public bool TemplateIsUpdated { get; set; }

		public Guid EventId { get; set; }

		public static RecurringSequenceInfo FromTableRow(object[] fullTableRow)
		{
			if (fullTableRow == null) throw new ArgumentNullException(nameof(fullTableRow));
			if (fullTableRow.Length < 16) throw new ArgumentException($"Row contains only {fullTableRow.Length} instead of the required 16", nameof(fullTableRow));

			var recurringOrderInfo = new RecurringSequenceInfo
			{
				Name = Convert.ToString(fullTableRow[1]),
				Recurrence = new Recurrence
				{
					RecurrenceFrequency = new RecurrenceFrequency
					{
						Frequency = Convert.ToInt32(fullTableRow[2]),
						FrequencyUnit = (RecurrenceFrequencyUnit)Convert.ToInt32(fullTableRow[3])
					},
					RecurrenceRepeat = new RecurrenceRepeat
					{
						RepeatType = (RecurrenceRepeatType)Convert.ToInt32(fullTableRow[4]),
						UmpteenthDayOfTheMonth = Convert.ToInt32(fullTableRow[5]),
						Day = (DaysOfTheWeek)Convert.ToInt32(fullTableRow[6]),
						UmpteenthOccurrenceOfWeekDayOfTheMonth = Convert.ToInt32(fullTableRow[7])
					},
					RecurrenceEnding = new RecurrenceEnding
					{
						EndingType = (EndingType)Convert.ToInt32(fullTableRow[8]),
						AmountOfRepeats = Convert.ToInt32(fullTableRow[10]),
						EndingDateTime = DateTime.FromOADate(Convert.ToDouble(fullTableRow[9]))
					},
					StartTime = DateTime.FromOADate(Convert.ToDouble(fullTableRow[12]))
				},
			};

			if (Guid.TryParse(Convert.ToString(fullTableRow[11]), out var templateId))
			{
				recurringOrderInfo.TemplateId = templateId;
			}

			if (Guid.TryParse(Convert.ToString(fullTableRow[16]), out var eventId))
			{
				recurringOrderInfo.EventId = eventId;
			}

			return recurringOrderInfo;
		}

		public List<DateTime> GetAllOccurrencesForWeekly()
		{
			var allOccurrences = new List<DateTime>();

			var startOfTheFirstWeek = Recurrence.StartTime.AddDays(-(((int)Recurrence.StartTime.DayOfWeek + 6) % 7));

			DateTime weekToConsider;
			int multiplier = 0;
			do
			{
				weekToConsider = startOfTheFirstWeek + TimeSpan.FromDays(7).Multiply(Recurrence.RecurrenceFrequency.Frequency).Multiply(multiplier);

				for (int i = 0; i < 7; i++)
				{
					var dayToConsider = weekToConsider.AddDays(i);

					if (DateTime.Now < dayToConsider && dayToConsider < Recurrence.EffectiveEndDate)
					{
						var dayOfTheWeek = (DaysOfTheWeek)(1 << ((int)dayToConsider.DayOfWeek + 6) % 7); // conversion between enums

						bool nextOccurrenceIsOnAllowedDay = Recurrence.RecurrenceRepeat.Day.HasFlag(dayOfTheWeek);
						if (nextOccurrenceIsOnAllowedDay) allOccurrences.Add(dayToConsider.Truncate(TimeSpan.FromMinutes(1)));
					}
				}

				multiplier++;
			}
			while (weekToConsider < Recurrence.EffectiveEndDate);

			return allOccurrences;
		}

		public List<DateTime> GetAllFutureOccurrences()
		{
			TimeSpan frequencyUnit;
			switch (Recurrence.RecurrenceFrequency.FrequencyUnit)
			{
				case RecurrenceFrequencyUnit.Days:
					frequencyUnit = TimeSpan.FromDays(1);
					break;
				case RecurrenceFrequencyUnit.Months:
					frequencyUnit = TimeSpan.FromDays(31);
					break;
				case RecurrenceFrequencyUnit.Years:
					frequencyUnit = TimeSpan.FromDays(365);
					break;
				case RecurrenceFrequencyUnit.Weeks:
					return GetAllOccurrencesForWeekly();
				default:
					frequencyUnit = TimeSpan.Zero;
					break;
			}

			var allOccurrences = new List<DateTime>();

			DateTime nextOccurrence;
			int multiplier = 0;
			do
			{
				int totalMultiplier = multiplier * Recurrence.RecurrenceFrequency.Frequency;

				nextOccurrence = Recurrence.StartTime + frequencyUnit.Multiply(totalMultiplier);

				if (DateTime.Now < nextOccurrence && nextOccurrence < Recurrence.EffectiveEndDate)
				{
					bool nextOccurrenceIsOnAllowedDay = true;
					if (Recurrence.RecurrenceRepeat.RepeatType == RecurrenceRepeatType.UmpteenthDayOfTheMonth)
					{
						nextOccurrenceIsOnAllowedDay = nextOccurrence.Day == Recurrence.RecurrenceRepeat.UmpteenthDayOfTheMonth;
					}
					else if (Recurrence.RecurrenceRepeat.RepeatType == RecurrenceRepeatType.UmpteenthWeekDayOfTheMonth)
					{
						bool weekdayIsCorrect = Recurrence.RecurrenceRepeat.Day.HasFlag((DaysOfTheWeek)((int)nextOccurrence.DayOfWeek + 1));

						int occurrence = (int)Math.Floor(nextOccurrence.Day / 7.0) + 1;
						bool occurrenceIsCorrect = occurrence == Recurrence.RecurrenceRepeat.UmpteenthOccurrenceOfWeekDayOfTheMonth;

						nextOccurrenceIsOnAllowedDay = weekdayIsCorrect && occurrenceIsCorrect;
					}

					if (nextOccurrenceIsOnAllowedDay) allOccurrences.Add(nextOccurrence.Truncate(TimeSpan.FromMinutes(1)));
				}

				multiplier++;
			}
			while (nextOccurrence < Recurrence.EffectiveEndDate);

			return allOccurrences;
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

		public object Clone()
		{
			return new RecurringSequenceInfo(this);
		}
	}
}
