namespace ShowPlasmaDetails_2.Plasma.Enums
{
	using System.ComponentModel;

	public enum TranslationDubbing
	{
		[Description("N/A")] NotFound = -1,
		[Description("Dubbing not available")] NotAvailable = 0,
		[Description("Dubbing available")] Available = 1
	}
}