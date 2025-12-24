namespace ShowCeitonDetails_2.Ceiton
{
	using System.ComponentModel;

	public enum IsCancelled
	{
		[Description("Not Canceled")]
		NotCanceled = 0,
		[Description("Canceled")]
		Canceled = 1
	}
}