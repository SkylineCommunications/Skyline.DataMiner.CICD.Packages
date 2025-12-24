namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport
{
	using System.ComponentModel;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;

	public enum Type
	{
		[Description("Export")]
		Export = 0,

		[Description("Import")]
		Import = 1,

		[Description("Iplay Folder Creation")]
		IplayFolderCreation = 2,

		[Description("Iplay WG Transfer")]
		IplayWgTransfer = 3,

		[Description("Non-Interplay Project")]
		NonInterplayProject = 4,

		[Description("Aspera Order")]
		AsperaOrder = 5
	}

	public enum State
	{
		[Description("Preliminary")]
		Preliminary = 0,

		[Description("Submitted")]
		Submitted = 1,

		[Description("Work in Progress")]
		WorkInProgress = 2,

		[Description("Change Requested")]
		ChangeRequested = 3,

		[Description("Completed")]
		Completed = 4,

		[Description("Cancelled")]
		Cancelled = 5,

		[Description("Folder Created")]
		FolderCreated = 6,

		[Description("Delete Date Near")]
		DeleteDateNear = 7,

		[Description("Folder Deleted")]
		FolderDeleted = 8,

		[Description("Import Completed")]
		ImportCompleted = 9,

		[Description("Backup Delete Date Near")]
		BackupDeleteDateNear = 10,

		[Description("Backup Deleted")]
		BackupDeleted = 11,
	}

	public enum MgmtState
	{
		[Description("Folder Created")]
		FolderCreated = 0,

		[Description("Delete Date Near")]
		DeleteDateNear = 1,

		[Description("Folder Hidden")]
		FolderHidden = 2,

		[Description("Folder Deleted")]
		FolderDeleted = 3,
	}

	public enum Sources
	{
		[Description("Interplay PAM")]
		INTERPLAY_PAM = 0,

		[Description("Mediaparkki")]
		MEDIAPARKKI = 1,

		[Description("Isilon BU")]
		ISILON_BU = 2,

		[Description("MAM (metro)")]
		MAM = 3,

		[Description("Other")]
		OTHER = 4
	}

	public enum InterplayPamExportFileTypes
	{
		[Description("Hires Video")]
		HiresVideo = 0,

		[Description("Viewing Video")]
		ViewingVideo = 1,

		[Description("AAF-Sequence")]
		AFFSequence = 2,

		[Description("Metro Transfer")]
		MetroTransfer = 3
	}

	public enum MediaParkkiExportFileTypes
	{
		[Description("Hires Video")]
		HIRES_VIDEO = 0,

		[Description("Viewing Video")]
		VIEWING_VIDEO = 1
	}

	public enum MamExportFileTypes
	{
		[Description("Hires Video")]
		HIRES_VIDEO = 0,

		[Description("Viewing Video")]
		VIEWING_VIDEO = 1,

		[Description("Other")]
		OTHER = 2
	}

	public enum HiresTargetVideoFormats
	{
		[Description("AVC-Intra100")]
		AVCI100 = 0,

		[Description("DNxHD HQX")]
		DNxHD_HQX = 1,

		[Description("Prores HQ 4:2:2")]
		PRORES_HQ_422 = 2,

		[Description("Other")]
		OTHER = 3
	}

	public enum TargetVideoFormats
	{
		[Description("AVC-Intra100 Hires")]
		AVCI100 = 0,

		[Description("DNxHD HQX Hires")]
		DNxHD_HQX = 1,

		[Description("Prores HQ 422 Hires")]
		PRORES_HQ_422 = 2,

		[Description("H.264 Katselu")]
		H264 = 3,

		[Description("MPEG1 Translation")]
		MPEG1 = 4,

		[Description("Other")]
		OTHER = 5
	}

	public enum InterplayItemTypes
	{
		[Description("Sequence")]
		SEQUENCE = 0,

		[Description("Masterclip")]
		MASTERCLIP = 1,

		[Description("Folder")]
		FOLDER = 2,

		[Description("None")]
		NONE = 3
	}

	public enum InterplayPamVaasaItemTypes
	{
		[Description("Program")]
		Program = 0,

		[Description("Clip")]
		Clip = 1
	}

	public enum ViewTargetFormats
	{
		[Description("H.264 Katselu")]
		H264 = 0,

		[Description("MPEG1 Translation")]
		MPEG1 = 1,

		[Description("Other")]
		OTHER = 2
	}

	public enum AafMediaFormats
	{
		[Description("Embedded Media")]
		EMBEDDED_MEDIA = 0,

		[Description("Separate Media Folder")]
		SEPARATE_MEDIA_FOLDER = 1
	}

	public enum VideoViewingQualities
	{
		[Description("HIGH (8 Mbit/s)")]
		HIGH = 0,

		[Description("LOW (4 Mbit/s)")]
		LOW = 1,

		[Description("SUPER (12 Mbit/s)")]
		SUPER = 2
	}

	public enum BackupOrigins
	{
		[Description("TAMPERE (Mediaputiikki)")]
		TAMPERE = 0,

		[Description("HELSINKI (Messi)")]
		HELSINKI = 1,

		[Description("VAASA (Mediamylly)")]
		VAASA = 2
	}

	public enum HelsinkiDepartmentNames
	{
		[Description("ASIA")]
		ASIA = 0,

		[Description("ASIA TUTKIVA")]
		ASIA_TUTKIVA = 1,

		[Description("DOKUMENTTI")]
		DOKUMENTTI = 2,

		[Description("DRAAMA")]
		DRAAMA = 3,

		[Description("KULTTUURI")]
		KULTTUURI = 4,

		[Description("LAPSET JA NUORET")]
		LAPSET_JA_NUORET = 5,

		[Description("SVENSKA")]
		SVENSKA = 6,

		[Description("URHEILU")]
		URHEILU = 7
	}

	public enum TampereDepartmentNames
	{
		[Description("ASIA")]
		ASIA = 0,

		[Description("DRAAMA")]
		DRAAMA = 3,

		[Description("URHEILU")]
		URHEILU = 7,

		[Description("AJANKOHTAINEN")]
		AJANKOHTAINEN = 8,

		[Description("LAPSET")]
		LAPSET = 9,

		[Description("VIIHDE")]
		VIIHDE = 10,
	}

	public enum ExportDepartments
	{
		[Description("Tampere (Mediaputiikki)")]
		TAMPERE,

		[Description("Helsinki (Messi)")]
		HELSINKI,

		[Description("Vaasa (Mediamylly)")]
		VAASA
	}

	public enum ExportTargets
	{
		[Description("Mediaparkki")]
		Mediaparkki,

		[Description("HDD")]
		HDD,

		[Description("Aspera Faspex")]
		AsperaFaspex,

		[Description("Other")]
		Other
	}

	public enum IngestDestinations
	{
		[Description("HKI IPLAY PAM")]
		HKI_IPLAY_PAM = 0,

		[Description("TRE IPLAY PAM")]
		TRE_IPLAY_PAM = 1,

		[Description("VSA IPLAY PAM")]
		VSA_IPLAY_PAM = 2,

		[Description("UA IPLAY")]
		UA_IPLAY = 3,
	}

	/// <summary>
	/// Missing options and some descriptions don't match name due to DCP221522
	/// </summary>
	public enum ProductNumbers
	{
		[Description("Uutiset ja ajankohtaistoiminta")]
		[OldDescription("Uutiset (001)")]
		UUTISET = 0,

		[Description("Urheilu ja tapahtumat")]
		[OldDescription("Urheilu (002)")]
		URHEILU = 1,

		[Description("Luovat sisällöt ja media")]
		[OldDescription("Puoliseiska")]
		PUOLISEISKA = 3,

		[Description("Svenska Yle")]
		[OldDescription("TV-Nytt")]
		TVNytt = 5,
	}

	public enum MaterialTypes
	{
		[Description("Card")]
		CARD = 0,

		[Description("HDD")]
		HDD = 1,

		[Description("File")]
		FILE = 2,

		[Description("Metro MAM")]
		MetroMam = 3
	}

	public enum CameraOrAudioTypes
	{
		[Description("Sony FS")]
		SonyFS = 0,

		[Description("Sony FX")]
		SonyFX = 1,

		[Description("Canon DSLR")]
		CANON_DSLR = 2,

		[Description("GoPro")]
		GOPRO = 3,

		[Description("Drone")]
		DRONE = 4,

		[Description("ARRI")]
		ARRI = 5,

		[Description("Panasonic P2")]
		PanasonicP2 = 6,

		[Description("AJA")]
		AJA = 7,

		[Description("Other")]
		OTHER = 8,

		[Description("Sony A7")]
		SonyA7 = 9,

		[Description("Audio Card")]

		AudioCard = 10
	}

	public enum LocationOfCardsToBeReturned
	{
		[Description("Valmiit-kaappi")]
		ValmiitKaapi = 0,

		[Description("Internal Mail")]
		InternalMail = 1
	}

	public enum CardTypes
	{
		[Description("XQD")]
		XQD = 0,

		[Description("SD")]
		SD = 1,

		[Description("MicroSD")]
		MICRO_SD = 2,

		[Description("SF")]
		SF = 3,

		[Description("P2")]
		P2 = 4,

		[Description("Camera SSD")]
		CAMERA_SSD = 5,

		[Description("EVS SSD")]
		EVS_SSD = 6,

		[Description("Other")]
		OTHER = 7
	}

	public enum HddConnectionTypes
	{
		[Description("USB (v1-3)")]
		USB_V1_V3 = 0,

		[Description("USB-C")]
		USB_C = 1,

		[Description("Other")]
		OTHER = 2
	}

	public enum HddDiskFormatTypes
	{
		[Description("NTFS")]
		NTFS = 0,

		[Description("EX-FAT")]
		EXFAT = 1,

		[Description("FAT32")]
		FAT32 = 2,

		[Description("MAC OS")]
		MAC_OS = 3,

		[Description("Other")]
		OTHER = 4
	}

	public enum SourceFileLocations
	{
		[Description("Mediaparkki")]
		MEDIAPARKKI = 0,

		[Description("Aspera Faspex")]
		ASPERA_FASPEX = 1,

		[Description("Google Drive")]
		GOOGLE_DRIVE = 2,

		[Description("Other")]
		OTHER = 3
	}

	public enum FileMaterialTypes
	{
		[Description("Video")]
		VIDEO = 0,

		[Description("Audio")]
		AUDIO = 1,

		[Description("Still Picture")]
		STILL_PICTURE = 2,

		[Description("Graphics")]
		GRAPHICS = 3,
	}

	public enum ScreenCaptureVideoSource
	{
		[Description("Internet Video Stream")]
		INTERNET_VIDEO_STREAM = 0,

		[Description("PC Screen Capture")]
		PC_SCREEN_CAPTURE = 1
	}

	public enum HkiInterplayformat
	{
		//[Description("XAVC-Intra100")]
		//XAVC_INTRA100 = 0,
		// Removed because of DCP172576

		[Description("AVC-Intra100")]
		AVC_INTRA100 = 1,

		[Description("DNxHD HQX")]
		DNXHD_HQX = 2,

		[Description("DNxHD LB")]
		DNXHD_LB = 3
	}

	public enum TreInterplayformat
	{
		[Description("AVC-Intra100")]
		AVC_INTRA100 = 0,

		[Description("DNxHD HQX")]
		DNXHD_HQX = 1,

		[Description("DNxHD LB")]
		DNXHD_LB = 2
	}

	public enum VsaInterplayformat
	{
		[Description("AVC-Intra100")]
		AVC_INTRA100 = 0,

		[Description("XAVC-Intra100")]
		XAVC_INTRA100 = 1,

		[Description("DNxHD HQX")]
		DNXHD_HQX = 2,

		[Description("AVC-Intra50")]
		AVCIntra50 = 3
	}

	public enum NewFolderContentTypes
	{
		[Description("Program")]
		PROGRAM = 0,

		[Description("Episode")]
		EPISODE = 1,

		[Description("None")]
		NONE = 2
	}

	public enum HelsinkiProductionDepartmentNames
	{
		[Description("ASIA")]
		ASIA = 0,

		[Description("DOKUMENTTI")]
		DOKUMENTTI = 1,

		[Description("DRAAMA")]
		DRAAMA = 2,

		[Description("KULTTUURI")]
		KULTTUURI = 3,

		[Description("TAPAHTUMAT")]
		TAPAHTUMAT = 4,

		[Description("LAPSET JA NUORET")]
		LAPSET_JA_NUORET = 5,

		[Description("SVENSKA")]
		SVENSKA = 6,

		[Description("URHEILU")]
		URHEILU = 7,

		[Description("None")]
		None = 8
	}

	public enum TampereProductionDepartmentNames
	{
		[Description("ASIA")]
		ASIA = 0,

		[Description("AJANKOHTAINEN")]
		AJANKOHTAINEN = 1,

		[Description("DRAAMA")]
		DRAAMA = 2,

		[Description("LAPSET")]
		LASPET = 3,

		[Description("URHEILU")]
		URHEILU = 4,

		[Description("VIIHDE")]
		VIIHDE = 5,

		[Description("None")]
		None = 6
	}

	public enum VaasaProductionDepartmentNames
	{
		[Description("SV Barn")]
		BARN = 0,

		[Description("SV Desk")]
		DESK = 1,

		[Description("SV Fakta")]
		FAKTA = 2,

		[Description("SV Kultur")]
		KULTUR = 3,

		[Description("SV Samhalle")]
		SAMHALLE = 4,

		[Description("SV Sport")]
		SPORT = 5,

		[Description("SV Ung")]
		UNG = 6,

		[Description("Other")]
		OTHER = 7,

		[Description("None")]
		None = 8
	}
}