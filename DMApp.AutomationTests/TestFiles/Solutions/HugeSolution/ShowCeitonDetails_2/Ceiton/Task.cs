namespace ShowCeitonDetails_2.Ceiton
{
	using System;
	using System.Collections.Generic;

	public class Task
	{
		public string ProjectOrProductId { get; set; }

		public string Status { get; set; }

		public string IsCancelled { get; set; }

		public string IsFlexible { get; set; }

		public string Comment { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public TimeSpan Duration { get; set; }

		public string ActivityType { get; set; }

		public string ResourceType { get; set; }

		public string EquipmentName { get; set; }

		public string ResourceName { get; set; }

		public string ResourceId { get; set; }

		public IEnumerable<SubTask> SubTasks { get; set; }
	}
}