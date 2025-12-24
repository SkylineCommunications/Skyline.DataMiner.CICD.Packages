namespace ShowPlasmaDetails_2.Plasma
{
	using System.ComponentModel;

	public enum ProgramSubCategory
	{
		[Description("N/A")] NotFound = -1,
		[Description("Not available")] NotAvailable = 0,

		[Description("Uutisbulletiini, uutislähetys")]
		UutisbulletiiniUutislähetys = 11,
		[Description("Makasiini")] Makasiini = 12,
		[Description("Reportaasi, raportti ")] ReportaasiRaportti = 13,
		[Description("Tapahtuma")] Tapahtuma = 14,
		[Description("Dokumentti")] Dokumentti = 25,
		[Description("Keskustelu")] KeskusteluHaastattelu = 26,
		[Description("Lähetysvirta")] Lähetysvirta = 270,
		[Description("Asiaviihde")] Asiaviihde = 28,
		[Description("Muut")] Muut = 33,
		[Description("Urheilu-uutislähetys")] UrheiluUutislähetys = 36,
		[Description("Talk show")] TalkShow = 410,
		[Description("Jumalanpalvelukset")] Jumalanpalvelukset = 61,
		[Description("Muut hartausohjelmat")] MuutHartausohjelmat = 62,

		[Description("Konsertti (taltiointi tai juonnettu)")]
		KonserttiTaltiointiTaiJuonnettu = 71,

		[Description("Juonnettu musiikkiohjelma")]
		JuonnettuMusiikkiohjelma = 72,

		[Description("Esitys (ooppera, baletti...)")]
		EsitysOopperaBaletti = 73,
		[Description("Musiikkivideo")] Musiikkivideo = 74,
		[Description("Musiikkikilpailut")] Musiikkikilpailut = 75,
		[Description("Muu musiikkiohjelma")] MuuMusiikkiohjelma = 76,
		[Description("Toivekonsertti")] Toivekonsertti = 77,
		[Description("TV-elokuva")] TVElokuva = 810,
		[Description("Fiktiosarja")] Fiktiosarja = 811,

		[Description("Animaatio, animaatiosarja")]
		AnimaatioAnimaatiosarja = 812,

		[Description("Nukkenäytelmä, nukkesarja")]
		NukkenäytelmäNukkesarja = 813,

		[Description("(Elokuvateatteri) elokuva")]
		ElokuvateatteriElokuva = 814,
		[Description("Pistedraama, näytelmä")] PistedraamaNäytelmä = 815,
		[Description("Kuunnelma")] Kuunnelma = 816,
		[Description("Luenta")] Luenta = 817,
		[Description("Tietokilpailut")] Tietokilpailut = 92,

		[Description("Sketsiohjelmat (huumori, satiiri)")]
		SketsiohjelmatHuumoriSatiiri = 94,
		[Description("Estradishow")] Estradishow = 95,
		[Description("Panel show")] PanelShow = 96,
		[Description("Muut viihdeohjelmat")] MuutViihdeohjelmat = 97,
		[Description("Reality")] Reality = 98,

		[Description("Lasten makasiiniohjelmat")]
		LastenMakasiiniohjelmat = 101,
		[Description(" Muut lastenohjelmat")] MuutLastenohjelmat = 102,
		[Description("Ohjelmaesittelyt")] Ohjelmaesittelyt = 111,
		[Description("Pelit")] Pelit = 112,
		[Description("Kolumni")] Kolumni = 113,
		[Description("Podcast")] Podcast = 114,
		[Description("Säätiedotus")] Säätiedotus = 115,
		[Description("Ääniteos")] Ääniteos = 116,
		[Description("Kontaktiohjelmat")] Kontaktiohjelmat = 117
	}
}