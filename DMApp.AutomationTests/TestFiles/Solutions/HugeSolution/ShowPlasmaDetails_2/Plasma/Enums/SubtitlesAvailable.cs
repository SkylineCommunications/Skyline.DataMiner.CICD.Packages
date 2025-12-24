namespace ShowPlasmaDetails_2.Plasma.Enums
{
	using System.ComponentModel;

	public enum SubtitlesAvailable
	{
		[Description("N/A")] NotFound = -1,

		[Description("Subtitles not available")]
		NotLive = 0,
		[Description("Subtitles available")] Live = 1
	}
}