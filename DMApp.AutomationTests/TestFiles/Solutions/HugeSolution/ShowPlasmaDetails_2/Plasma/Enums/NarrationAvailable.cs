namespace ShowPlasmaDetails_2.Plasma.Enums
{
	using System.ComponentModel;

	public enum NarrationAvailable
	{
		[Description("N/A")] NotFound = -1,

		[Description("Narration not available")]
		NotAvailable = 0,
		[Description("Narration available")] Available = 1
	}
}