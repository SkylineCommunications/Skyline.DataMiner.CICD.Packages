using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using SLDataGateway.API.Tools;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence
{
	using System;
	using Library_1.Utilities;
	using Newtonsoft.Json;

	public class Recurrence : ICloneable
	{
		public Recurrence()
		{

		}

		private Recurrence(Recurrence other)
		{
			CloneHelper.CloneProperties(other, this);
		}

		/// <summary>
		/// Gets a boolean indicating if Recurrence is configured or not. Not configure means no recurrence is required.
		/// </summary>
		[JsonIgnore]
		public bool IsConfigured { get; set; } = false;

		/// <summary>
		/// The date time indicating the start of the recurring order sequence.
		/// </summary>
		public DateTime StartTime { get; set; }

		public RecurrenceFrequency RecurrenceFrequency { get; set; } = new RecurrenceFrequency();

		public RecurrenceRepeat RecurrenceRepeat { get; set; } = new RecurrenceRepeat();

		public RecurrenceEnding RecurrenceEnding { get; set; } = new RecurrenceEnding();

        /// <summary>
        /// The date time indicating the end of the recurring order sequence.
        /// </summary>
        public DateTime EffectiveEndDate
		{
			get
			{
				var effectiveEndDateOfRecurringOrder = DateTime.Now.AddYears(1).Truncate(TimeSpan.FromMinutes(1)); // default value for never-ending recurrence

				if (RecurrenceEnding.EndingType == EndingType.SpecificDate)
				{
					effectiveEndDateOfRecurringOrder = RecurrenceEnding.EndingDateTime.Truncate(TimeSpan.FromMinutes(1));
				}
				else if (RecurrenceEnding.EndingType == EndingType.CertainAmountOfRepeats)
				{
					TimeSpan timespan;
					switch (RecurrenceFrequency.FrequencyUnit)
					{
						case RecurrenceFrequencyUnit.Days:
							timespan = TimeSpan.FromDays(1);
							break;
						case RecurrenceFrequencyUnit.Weeks:
							timespan = TimeSpan.FromDays(7);
							break;
						case RecurrenceFrequencyUnit.Months:
							timespan = TimeSpan.FromDays(31);
							break;
						case RecurrenceFrequencyUnit.Years:
							timespan = TimeSpan.FromDays(365);
							break;
						default:
							timespan = TimeSpan.Zero;
							break;
					}

					var multiplier = RecurrenceFrequency.Frequency * RecurrenceEnding.AmountOfRepeats;

					effectiveEndDateOfRecurringOrder = StartTime.Truncate(TimeSpan.FromMinutes(1)) + timespan.Multiply(multiplier);
				}

				return effectiveEndDateOfRecurringOrder;
			}
		}

		public object Clone()
		{
			return new Recurrence(this);
		}

		public new string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}
	}
}
