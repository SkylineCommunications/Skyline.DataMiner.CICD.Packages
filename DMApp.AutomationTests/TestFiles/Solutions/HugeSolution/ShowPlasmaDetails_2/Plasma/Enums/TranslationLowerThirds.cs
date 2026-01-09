namespace ShowPlasmaDetails_2.Plasma.Enums
{
	using System.ComponentModel;

	public enum TranslationLowerThirds
	{
		[Description("N/A")] NotFound = -1,

		[Description("Translation not available")]
		NotAvailable = 0,
		[Description("Translation available")] Available = 1
	}
}