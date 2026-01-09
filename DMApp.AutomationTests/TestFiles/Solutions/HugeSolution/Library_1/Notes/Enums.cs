namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes
{
	using System.ComponentModel;

	/// <summary>
	/// Enum used to filter notes to display them on correct page.
	/// </summary>
	/// <remarks>Don't change descriptions as they are used to filter.</remarks>
	public enum Page
	{
		[Description("mcr-operator")]
		MCR,

		[Description("import-export-operator")]
		IngestExport,

		[Description("media-operator")]
		MediaOperator,

		[Description("media-operator-messi-news")]
		MediaOperatorMessiNews,

		[Description("media-operator-messi-live")]
		MediaOperatorMessiLive,

		[Description("tom-ut")]
		TomUt,
	}

	public enum Status
	{
		[Description("Open")]
		Open = 1,

		[Description("Closed")]
		Closed = 2,

		[Description("Alarm")]
		Alarm = 3,

		[Description("Acknowledged Alarm")]
		AcknowledgedAlarm = 4
	}
}