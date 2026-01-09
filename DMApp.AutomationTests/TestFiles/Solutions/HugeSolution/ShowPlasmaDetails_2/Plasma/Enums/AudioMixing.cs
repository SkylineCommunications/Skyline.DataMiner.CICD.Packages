namespace ShowPlasmaDetails_2.Plasma.Enums
{
	using System.ComponentModel;

	public enum AudioMixing
	{
		[Description("N/A")] NotFound = -1,

		[Description("Audio mixing available")]
		NotLive = 0,

		[Description("Audio mixing not available")]
		Live = 1
	}
}