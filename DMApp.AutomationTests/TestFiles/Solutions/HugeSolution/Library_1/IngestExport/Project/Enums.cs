namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project
{
	using System.ComponentModel;

	public enum IngestDepartments
	{
		[Description("TAMPERE (Mediaputiikki)")]
		TAMPERE = 0,

		[Description("HELSINKI (Messi)")]
		HELSINKI = 1,

		[Description("VAASA (Mediamylly)")]
		VAASA = 2
	}

	public enum ProjectTypes
	{
		[Description("Avid MC Project HDD + Isilon BU")]
		AVID_ISILON = 0,

		[Description("HDD + Isilon BU")]
		HDD_ISILON = 1,

		[Description("Only Isilon BU")]
		ONLY_ISILON = 2
	}

	public enum AvidProjectVideoFormats
	{
		[Description("AVC-Intra100")]
		AVC_INTRA100 = 0,

		[Description("DNxHD HQX")]
		DNxHD_HQX = 1,

		[Description("XAVC-Intra100")]
		XAVC_INTRA100 = 2,

		[Description("DNxHD LB")]
		DNxHD_LB = 3
	}

	public enum CardReturnDestinations
	{
		[Description("Internal Mail")]
		InternalMail,

		[Description("Valmiit-kaappi")]
		ValmiitKaappi
	}
}