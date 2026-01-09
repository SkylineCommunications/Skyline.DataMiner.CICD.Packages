namespace ShowCeitonDetails_2.Ceiton
{
	using System;

	public class SubTask
	{
		private readonly object[] subTaskRow;

		public SubTask(object[] subTaskRow)
		{
			this.subTaskRow = subTaskRow;

			InitializeTimingValues();
		}

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public TimeSpan Duration { get; set; }

		private void InitializeTimingValues()
		{
			var startDate = DateTime.FromOADate(Convert.ToDouble(subTaskRow[2]));
			var endDate = startDate;

			var startTime = TimeSpan.FromMinutes(Convert.ToInt32(subTaskRow[3]));
			var endTime = TimeSpan.FromMinutes(Convert.ToInt32(subTaskRow[4]));

			bool endIsOnNextDay = endTime < startTime;
			if (endIsOnNextDay)
			{
				endDate = endDate.AddDays(1);
			}

			StartTime = startDate + startTime;
			EndTime = endDate + endTime;
			Duration = TimeSpan.FromMinutes(Convert.ToDouble(subTaskRow[5]));
		}
	}
}