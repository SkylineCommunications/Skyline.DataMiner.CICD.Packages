namespace ShowPlasmaDetails_2.Plasma
{
	using System.ComponentModel;

	public enum ProgramCategory
	{
		[Description("N/A")] NotFound = -1,
		[Description("Not available")] NotAvailable = 0,
		[Description("Uutiset")] Uutiset = 1,
		[Description("Ajankohtainen")] Ajankohtainen = 2,
		[Description("Urheilu")] Urheilu = 3,
		[Description("Asia")] Asia = 4,
		[Description("Opetusohjelma")] Opetusohjelma = 5,
		[Description("Hartaus")] Hartaus = 6,
		[Description("Musiikki")] Musiikki = 7,
		[Description("Draama")] Draama = 8,
		[Description("Viihde")] Viihde = 9,
		[Description("Lapset")] Lapset = 10,
		[Description("Muut")] Muut = 11
	}
}