using System;
using Library_1.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence
{
	public class RecurrenceFrequency : ICloneable
	{
		private RecurrenceFrequencyUnit frequencyUnit;

		private RecurrenceFrequency(RecurrenceFrequency other)
		{
			CloneHelper.CloneProperties(other, this);
		}

		public RecurrenceFrequency()
		{
			Frequency = 1;
			FrequencyUnit = RecurrenceFrequencyUnit.Days;
		}

		public int Frequency { get; set; }

		public RecurrenceFrequencyUnit FrequencyUnit 
		{ 
			get => frequencyUnit; 
			set
			{ 
				frequencyUnit = value; 
				RecurrenceFrequencyUnitChanged?.Invoke(this, frequencyUnit);
			} 
		}

		public event EventHandler<RecurrenceFrequencyUnit> RecurrenceFrequencyUnitChanged;

		public object Clone()
		{
			return new RecurrenceFrequency(this);
		}
	}
}
