namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition
{
	using System.ComponentModel;

	public enum VirtualPlatform
	{
		[Description("Reception.None")]
		//[IsDummy]
		ReceptionNone,

		[Description("Reception.Satellite")]
		ReceptionSatellite,

		[Description("Reception.LiveU")]
		ReceptionLiveU,

		[Description("Reception.Fiber")]
		ReceptionFiber,

		[Description("Reception.Fixed Line")]
		ReceptionFixedLine,

        [Description("Reception.Fixed Service")]
        ReceptionFixedService,

        [Description("Reception.IP")]
		ReceptionIp,

		[Description("Reception.Microwave")]
		ReceptionMicrowave,

		[Description("Reception.Eurovision")]
		//[IsDummy]
		ReceptionEurovision,

        [Description("Reception.Unknown")]
		//[IsDummy]
		ReceptionUnknown,

        [Description("Reception.Commentary Audio")]
		ReceptionCommentaryAudio,

		[Description("Recording")]
		Recording,

		[Description("Routing")]
		Routing,

		[Description("Destination")]
		Destination,

		[Description("Audio Processing")]
		AudioProcessing,

		[Description("Video Processing")]
		VideoProcessing,

		[Description("Graphics Processing")]
		GraphicsProcessing,

		[Description("Transmission.None")]
		//[IsDummy]
		TransmissionNone,

		[Description("Transmission.Satellite")]
		TransmissionSatellite,

		[Description("Transmission.LiveU")]
		TransmissionLiveU,

		[Description("Transmission.Fiber")]
		TransmissionFiber,

		[Description("Transmission.IP")]
		TransmissionIp,

		[Description("Transmission.Microwave")]
		TransmissionMicrowave,

		[Description("Transmission.Eurovision")]
		//[IsDummy]
		TransmissionEurovision,

		// TODO: is this still being used?
		[Description("File Playout")]
		FilePlayout,

		[Description("File Processing")]
		FileProcessing,

		[Description("VIZREM Studio")]
		VizremStudio,

		[Description("VIZREM Farm")]
		VizremFarm,
		
		[Description("VIZREM NC2 Converter")]
		VizremNC2Converter,
	}

	public enum VirtualPlatformType
	{
		[Description("Reception")]
		Reception,

		[Description("Recording")]
		Recording,

		[Description("Destination")]
		Destination,

		[Description("Routing")]
		Routing,

		[Description("Transmission")]
		Transmission,

		[Description("Audio Processing")]
		AudioProcessing,

		[Description("Video Processing")]
		VideoProcessing,

		[Description("Graphics Processing")]
		GraphicsProcessing,

		[Description("VIZREM Studio")]
		VizremStudio,

		[Description("VIZREM Farm")]
		VizremFarm,

		[Description("VIZREM NC2 Converter")]
		VizremNC2Converter,
	}

	public enum VirtualPlatformName
	{
		[Description("None")]
		None,

		[Description("Satellite")]
		Satellite,

		[Description("LiveU")]
		LiveU,

		[Description("Fiber")]
		Fiber,

		[Description("Fixed Line")]
		FixedLine,

        [Description("Fixed Service")]
        FixedService,

        [Description("IP")]
		IP,

		[Description("Microwave")]
		Microwave,

		[Description("Eurovision")]
		Eurovision,

        [Description("Unknown")]
        Unknown,

        [Description("Recording")]
		Recording,

		[Description("Destination")]
		Destination,

		[Description("Routing")]
		Routing,

		[Description("Audio Processing")]
		AudioProcessing,

		[Description("Video Processing")]
		VideoProcessing,

		[Description("Graphics Processing")]
		GraphicsProcessing,

		[Description("Commentary Audio")]
		CommentaryAudio,

		[Description("VIZREM Studio")]
		VizremStudio,

		[Description("VIZREM Farm")]
		VizremFarm,

		[Description("VIZREM NC2 Converter")]
		VizremNC2Converter,
	}
}