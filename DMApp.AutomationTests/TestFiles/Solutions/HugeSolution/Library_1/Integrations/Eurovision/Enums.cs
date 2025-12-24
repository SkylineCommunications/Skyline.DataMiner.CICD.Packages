namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision
{
	using System.ComponentModel;

	public enum Type
	{
		[Description("None")]
		None,

		[Description("News Event")]
		NewsEvent,

		[Description("Program Event")]
		ProgramEvent,

		[Description("Satellite Capacity")]
		SatelliteCapacity,

		[Description("Unilateral Transmission")]
		UnilateralTransmission,

		[Description("OSS Transmission")]
		OSSTransmission
	}
}