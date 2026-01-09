namespace ShowPlasmaDetails_2.Plasma.Enums
{
	using System.ComponentModel;

	public enum SubtitlingCopyRequired
	{
		[Description("N/A")] NotFound = -1,
		[Description("Required")] Required = 0,
		[Description("Not Required")] NotRequired = 1
	}
}