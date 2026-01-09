using System;
using System.Collections.Generic;
using System.Text;
using Library_1.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence
{
	public class RecurrenceEnding : ICloneable
    {
        public RecurrenceEnding()
        {
            EndingType = EndingType.SpecificDate;
            EndingDateTime = DateTime.Now.AddHours(25).AddMonths(2);
            AmountOfRepeats = 13;
        }

		private RecurrenceEnding(RecurrenceEnding other)
		{
			CloneHelper.CloneProperties(other, this);
		}

        public EndingType EndingType { get; set; }

        public DateTime EndingDateTime { get; set; }

        public int AmountOfRepeats { get; set; }

		public object Clone()
		{
			return new RecurrenceEnding(this);
		}
	}
}
