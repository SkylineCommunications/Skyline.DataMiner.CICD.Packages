namespace ShowCeitonDetails_2.Ceiton
{
	using System.ComponentModel;

	public enum TaskStatus
	{
		[Description("Deleted")]
		Deleted = 0,
		[Description("Unactivated")]
		Unactivated = 1,
		[Description("Pre-Planned")]
		PrePlanned = 2,
		[Description("Planned")]
		Planned = 3,
		[Description("Confirmed")]
		Confirmed = 4,
		[Description("Completed")]
		Completed = 5
	}
}