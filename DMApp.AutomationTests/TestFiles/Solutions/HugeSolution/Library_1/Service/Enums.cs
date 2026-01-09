namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service
{
	using System;
	using System.ComponentModel;

	public enum Status
	{
		/// <summary>
		/// Service which is under preliminary order.
		/// </summary>
		[Description("Preliminary")]
		Preliminary = 0,

		/// <summary>
		/// Service which is waiting for device configuration.
		/// User tasks are generated for specific service related manual tasks.
		/// </summary>
		[Description("Configuration Pending")]
		ConfigurationPending = 1,

		/// <summary>
		/// Service where all related user tasks are completed.
		/// </summary>
		// temporarily disabled because of slack https://yle.slack.com/archives/C025D6X84E9/p1672390883295109
		//[Description("Automated Config Completed")]
		[Description("Configuration Completed")]
		AutomatedConfigurationCompleted = 2,

		/// <summary>
		/// Service where some component is overbooked.
		/// This is actually not possible in SRM and will more likely be like "Quarantined" e.g. "Resource Missing"
		/// </summary>
		[Description("Resource Overbooked")]
		ResourceOverbooked = 3,

		/// <summary>
		/// Service which is within 30min of start time.
		/// </summary>
		[Description("Service Queuing + Config OK")]
		ServiceQueuingConfigOk = 4,

		/// <summary>
		/// Service where start time is reached and is not yet finished.
		/// </summary>
		[Description("Service Running")]
		ServiceRunning = 5,

		/// <summary>
		/// Service in post roll.
		/// </summary>
		[Description("Post Roll")]
		PostRoll = 6,

		/// <summary>
		/// Service where end time is reached but was only partially delivered or not delivered at all.
		/// </summary>
		[Description("Service Completed With Errors")]
		ServiceCompletedWithErrors = 7,

		/// <summary>
		/// Service where end time is reached.
		/// </summary>
		[Description("Service Completed")]
		ServiceCompleted = 8,

		/// <summary>
		/// Service which was cancelled.
		/// </summary>
		[Description("Cancelled")]
		Cancelled = 9,

		/// <summary>
		/// Service which has incompleted file processing user tasks.
		/// </summary>
		[Description("File Processing")]
		FileProcessing = 10,

		/// <summary>
		/// Service will have this status whenever the user tasks were completed which followed in a failed profile configuration set.
		/// </summary>
		[Description("Automated Config Failed")]
		AutomatedConfigurationFailed = 11,

		/// <summary>
		/// Service will have this status whenever pre roll starts which followed in a failed profile configuration set.
		/// </summary>
		[Description("Service Queuing + Config Failed")]
		ServiceQueingAndConfigFailed = 12
	}

	[Flags]
	public enum IsUpdatedOptions
	{
		None = 0,
		SkipDtrParameters = 1
	}

	[Flags]
	public enum RequiredUpdateType
	{
		None = 0,

		Property = 1, // Use TryUpdateAllCustomProperties to handle these updates

		Resource = 2, // Use TrySetOrSwapResource or TryRemoveResource to handle these updates

		Timing = 4, // Use TryChangeServiceTime to handle these updates

		FullAddOrUpdate = 8, // Use AddOrUpdateService to handle these updates

		ProfileParameter = 16,
	}

	public enum FileDestination
	{
		[Description("ARCHIVE (METRO)")]
		ArchiveMetro,

		[Description("IPLAY HKI")]
		IplayHki,

		[Description("IPLAY VSA")]
		IplayVsa,

		[Description("IPLAY TRE")]
		IplayTre,

		[Description("UA IPLAY")]
		UaIplay,

		[Description("ISILON")]
		Isilon
	}

	public enum VideoResolution
	{
		[Description("576i50")]
		Resolution576i50,

		[Description("1080i50")]
		Resolution1080i50,

		[Description("1080p50")]
		Resolution1080p50
	}

	public enum VideoCodec
	{
		[Description("AVCi100")]
		AvcI100,

		[Description("XDcamHD50")]
		XdCamHd50
	}

	public enum TimeCodec
	{
		[Description("Real")]
		Real,

		[Description("Non-Real")]
		NonReal
	}

	public enum ProxyFormat
	{
		[Description("Both")]
		Both,

		[Description("MPEG-1")]
		Mpeg1,

		[Description("MPEG-4")]
		Mpeg4
	}

	public enum MCRStatus
    {
		[Description("OK")]
		OK,

		[Description("NOK")]
		NOK
	}
}