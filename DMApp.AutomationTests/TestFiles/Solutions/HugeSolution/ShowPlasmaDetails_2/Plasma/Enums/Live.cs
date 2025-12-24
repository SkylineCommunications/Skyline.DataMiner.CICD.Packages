namespace ShowPlasmaDetails_2.Plasma.Enums
{
	using System.ComponentModel;

	public enum Live
	{
		[Description("N/A")] NotAvailable = -1,
		[Description("Not live")] NotLive = 0,
		[Description("Live")] Live = 1
	}
}