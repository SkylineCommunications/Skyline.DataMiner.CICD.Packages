namespace LiveOrderForm_6
{
	using System.ComponentModel;

	public enum EventSelection
	{
		[Description("Same event")]
		SameEvent,
		[Description("Other event")]
		OtherExistingEvent,
		[Description("New event")]
		NewEvent
	}
}