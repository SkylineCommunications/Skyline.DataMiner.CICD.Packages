namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	using System;

	public static class SrmConfiguration
	{
		// Booking Manager names
		public static readonly string OrderBookingManagerElementName = "Order Booking Manager";

		// Resource Pool names
		public static readonly string AreenaDestinationResourcePool = "Destination.Areena Destination";

		public static readonly string MessiNewsRecordingResourcePool = "Recording.Messi News.Recording";

		public static readonly string MatrixInputSdiResourcePool = "Routing.Matrix Input SDI";

		public static readonly string ReceptionSatelliteResourcePool = "Reception.Satellite.Satellite";

		public static readonly string ReceptionSatelliteDecodingResourcePool = "Reception.Satellite.Decoding";

		public static readonly string ReceptionSatelliteDemodulatingResourcePool = "Reception.Satellite.Demodulating";

		public static readonly string TransmissionSatelliteResourcePool = "Transmission.Satellite.Satellite";

		// Profile Parameter names
		public static readonly string FeenixSourceProfileParameterName = "Feenix Source";

		public static readonly string ResourceInputConnectionsSdiProfileParameterName = "ResourceInputConnections_SDI";

		public static readonly string SubtitleProxyProfileParameterName = "Subtitle Proxy";

		public static readonly string SubtitleProxyFormatProfileParameterName = "Subtitle Proxy Format";

		public static readonly string FastRerunCopyProfileParameterName = "Fast Rerun";

		public static readonly string FastAreenaCopyProfileParameterName = "Areena Copy";

		public static readonly string YleHelsinkiDestinationLocationProfileParameterName = "YLE Helsinki Destination Location";

		public static readonly string YleMediapolisDestinationLocationProfileParameterName = "YLE Mediapolis Destination Location";

		public static readonly string RecordingTvChannelProfileParameterName = "_TV Channel";

		public static readonly string PlasmaUserCodeProfileParameterName = "Plasma User Code";

		public static readonly string FixedLineYleMediapolisSourceLocationProfileParameterName = "Fixed Line YLE Mediapolis Source Location";

		public static readonly string AudioChannel1ProfileParameterName = "Audio Channel 1";

		public static readonly string AudioChannel2ProfileParameterName = "Audio Channel 2";

		public static readonly string AudioChannel3ProfileParameterName = "Audio Channel 3";

		public static readonly string AudioChannel4ProfileParameterName = "Audio Channel 4";

		public static readonly string AudioChannel5ProfileParameterName = "Audio Channel 5";

		public static readonly string AudioChannel6ProfileParameterName = "Audio Channel 6";

		public static readonly string AudioChannel7ProfileParameterName = "Audio Channel 7";

		public static readonly string AudioChannel8ProfileParameterName = "Audio Channel 8";

		public static readonly string AudioChannel9ProfileParameterName = "Audio Channel 9";

		public static readonly string AudioChannel10ProfileParameterName = "Audio Channel 10";

		public static readonly string AudioChannel11ProfileParameterName = "Audio Channel 11";

		public static readonly string AudioChannel12ProfileParameterName = "Audio Channel 12";

		public static readonly string AudioChannel13ProfileParameterName = "Audio Channel 13";

		public static readonly string AudioChannel14ProfileParameterName = "Audio Channel 14";

		public static readonly string AudioChannel15ProfileParameterName = "Audio Channel 15";

		public static readonly string AudioChannel16ProfileParameterName = "Audio Channel 16";

		public static readonly string AudioChannel1DescriptionProfileParameterName = "Audio Channel 1 Description";

		public static readonly string AudioChannel2DescriptionProfileParameterName = "Audio Channel 2 Description";

		public static readonly string AudioChannel3DescriptionProfileParameterName = "Audio Channel 3 Description";

		public static readonly string AudioChannel4DescriptionProfileParameterName = "Audio Channel 4 Description";

		public static readonly string AudioChannel5DescriptionProfileParameterName = "Audio Channel 5 Description";

		public static readonly string AudioChannel6DescriptionProfileParameterName = "Audio Channel 6 Description";

		public static readonly string AudioChannel7DescriptionProfileParameterName = "Audio Channel 7 Description";

		public static readonly string AudioChannel8DescriptionProfileParameterName = "Audio Channel 8 Description";

		public static readonly string AudioChannel9DescriptionProfileParameterName = "Audio Channel 9 Description";

		public static readonly string AudioChannel10DescriptionProfileParameterName = "Audio Channel 10 Description";

		public static readonly string AudioChannel11DescriptionProfileParameterName = "Audio Channel 11 Description";

		public static readonly string AudioChannel12DescriptionProfileParameterName = "Audio Channel 12 Description";

		public static readonly string AudioChannel13DescriptionProfileParameterName = "Audio Channel 13 Description";

		public static readonly string AudioChannel14DescriptionProfileParameterName = "Audio Channel 14 Description";

		public static readonly string AudioChannel15DescriptionProfileParameterName = "Audio Channel 15 Description";

		public static readonly string AudioChannel16DescriptionProfileParameterName = "Audio Channel 16 Description";

		public static readonly string AudioEmbeddingRequiredProfileParameterName = "Audio Embedding Required";

		public static readonly string AudioDeembeddingRequiredProfileParameterName = "Audio Deembedding Required";

		public static readonly string AudioDolbyDecodingProfileParameterName = "Audio Dolby Decoding Required";

		public static readonly string AudioShufflingRequiredProfileParameterName = "Audio Shuffling Required";

		public static readonly string FeedTypeProfileParameterName = "_Feed Type";

		public static readonly string[] AudioProfileParameterNames = new []
		{
			// Audio Channels
			AudioChannel1ProfileParameterName,
			AudioChannel2ProfileParameterName,
			AudioChannel3ProfileParameterName,
			AudioChannel4ProfileParameterName,
			AudioChannel5ProfileParameterName,
			AudioChannel6ProfileParameterName,
			AudioChannel7ProfileParameterName,
			AudioChannel8ProfileParameterName,
			AudioChannel9ProfileParameterName,
			AudioChannel10ProfileParameterName,
			AudioChannel11ProfileParameterName,
			AudioChannel12ProfileParameterName,
			AudioChannel13ProfileParameterName,
			AudioChannel14ProfileParameterName,
			AudioChannel15ProfileParameterName,
			AudioChannel16ProfileParameterName,
			// Audio Channel Descriptions
			AudioChannel1DescriptionProfileParameterName,
			AudioChannel2DescriptionProfileParameterName,
			AudioChannel3DescriptionProfileParameterName,
			AudioChannel4DescriptionProfileParameterName,
			AudioChannel5DescriptionProfileParameterName,
			AudioChannel6DescriptionProfileParameterName,
			AudioChannel7DescriptionProfileParameterName,
			AudioChannel8DescriptionProfileParameterName,
			AudioChannel9DescriptionProfileParameterName,
			AudioChannel10DescriptionProfileParameterName,
			AudioChannel11DescriptionProfileParameterName,
			AudioChannel12DescriptionProfileParameterName,
			AudioChannel13DescriptionProfileParameterName,
			AudioChannel14DescriptionProfileParameterName,
			AudioChannel15DescriptionProfileParameterName,
			AudioChannel16DescriptionProfileParameterName,
			// Audio Config
			AudioEmbeddingRequiredProfileParameterName,
			AudioDeembeddingRequiredProfileParameterName,
			AudioDolbyDecodingProfileParameterName,
			AudioShufflingRequiredProfileParameterName
		};

		public static readonly string[] AudioChannelsProfileParameterNames = new[]
		{
			// Audio Channels
			AudioChannel1ProfileParameterName,
			AudioChannel2ProfileParameterName,
			AudioChannel3ProfileParameterName,
			AudioChannel4ProfileParameterName,
			AudioChannel5ProfileParameterName,
			AudioChannel6ProfileParameterName,
			AudioChannel7ProfileParameterName,
			AudioChannel8ProfileParameterName,
			AudioChannel9ProfileParameterName,
			AudioChannel10ProfileParameterName,
			AudioChannel11ProfileParameterName,
			AudioChannel12ProfileParameterName,
			AudioChannel13ProfileParameterName,
			AudioChannel14ProfileParameterName,
			AudioChannel15ProfileParameterName,
			AudioChannel16ProfileParameterName,
			// Audio Channel Descriptions
			AudioChannel1DescriptionProfileParameterName,
			AudioChannel2DescriptionProfileParameterName,
			AudioChannel3DescriptionProfileParameterName,
			AudioChannel4DescriptionProfileParameterName,
			AudioChannel5DescriptionProfileParameterName,
			AudioChannel6DescriptionProfileParameterName,
			AudioChannel7DescriptionProfileParameterName,
			AudioChannel8DescriptionProfileParameterName,
			AudioChannel9DescriptionProfileParameterName,
			AudioChannel10DescriptionProfileParameterName,
			AudioChannel11DescriptionProfileParameterName,
			AudioChannel12DescriptionProfileParameterName,
			AudioChannel13DescriptionProfileParameterName,
			AudioChannel14DescriptionProfileParameterName,
			AudioChannel15DescriptionProfileParameterName,
			AudioChannel16DescriptionProfileParameterName
		};

		public static readonly string[] AudioConfigProfileParameterNames = new[]
		{
			// Audio Config
			AudioEmbeddingRequiredProfileParameterName,
			AudioDeembeddingRequiredProfileParameterName,
			AudioDolbyDecodingProfileParameterName,
			AudioShufflingRequiredProfileParameterName
		};

		public static readonly string VideoFormatProfileParameterName = "Video Format";

		public static readonly string DownlinkFrequencyProfileParameterName = "Downlink Frequency";

		public static readonly string UplinkFrequencyProfileParameterName = "Uplink Frequency";

		public static readonly string FecProfileParameterName = "FEC";

		public static readonly string ModulationProfileParameterName = "Modulation";

		public static readonly string ModulationStandardProfileParameterName = "Modulation Standard";

		public static readonly string PolarizationProfileParameterName = "Polarization";

		public static readonly string SymbolRateProfileParameterName = "Symbol Rate";

		public static readonly string EncodingProfileParameterName = "Encoding";

		public static readonly string EncryptionKeyProfileParameterName = "Encryption Key";

		public static readonly string EncryptionTypeProfileParameterName = "Encryption Type";

		public static readonly string ServiceSelectionProfileParameterName = "Service Selection";

		public static readonly string FixedLineEbuSourceLocationProfileParameterName = "Fixed Line EBU Source Location";

		public static readonly string EbuDestinationLocationProfileParameterName = "EBU Destination Location";

		// Profile Parameter values
		public static readonly string DestinationLocationPlayoutProfileParameterValue = "Lähetysyksikkö";

		public static readonly string DestinationLocationDummyPlayoutProfileParameterValue = "Lähetysyksikkö [Plasma]";

		public static readonly string AudioChannelInternationalProfileParameterValue = "International Mix";

		public static readonly string AudioChannelFinnishProfileParameterValue = "Fin Mix";

		public static readonly string AudioChannelSwedishProfileParameterValue = "Swe Mix";

		public static readonly string EncryptionTypeFreeProfileParameterValue = "FREE";

		public static readonly string RecordingTvChannelNoneProfileParameterValue = "None";

		public static readonly string RecordingTvChannelTv1ProfileParameterValue = "TV 1";

		public static readonly string RecordingTvChannelTv2ProfileParameterValue = "TV 2";

		public static readonly string RecordingTvChannelTvTeemaFemProfileParameterValue = "TV TEEMA/FEM";

		public static readonly string FixedLineEbuSourceLocationAcesEbuProfileParameterValue = "ACES EBU";

		public static readonly string FixedLineEbuSourceLocationNordicRingProfileParameterValue = "Nordic Ring";

		public static readonly string FixedLineYleMediapolisSourceLocationMediapolisStudioProfileParameterValue = "Mediapolis Studio";

		public static readonly string YleMediapolisDestinationLocationMediapolisLtProfileParameterValue = "Mediapolis LT";

		// Function Definition ids
		public static readonly Guid SourceParentSystemFunctionDefinitionId = Guid.Parse("c7e8648e-7522-4724-99a8-74e48a45f380");

		// Service Definition ids
		// Receptions
		public static readonly Guid FixedLineLyServiceDefinitionId = Guid.Parse("ff913685-f764-4cf1-8ca9-7c9c751e4778");

		public static readonly Guid FixedLineMediapolisServiceDefinitionId = Guid.Parse("338527f4-3b09-46b6-bfb7-89d4ca1cb4cf");

		public static readonly Guid SatelliteReceptionServiceDefinitionId = Guid.Parse("aecb710b-558e-46b9-8a5e-86886f7baa2f");

		public static readonly Guid FiberFullCapacityReceptionServiceDefinitionId = Guid.Parse("e7d1e9bf-c158-4671-b51f-a2a6cecdd0c4");

		public static readonly Guid FixedLineEbuReceptionServiceDefinitionId = Guid.Parse("74e0b355-3f0c-437e-b605-98762bdf3f6d");

		// Transmissions
		public static readonly Guid SatelliteTransmissionServiceDefinitionId = Guid.Parse("5fad39cb-6521-4208-b49b-0cfd063807b5");

		public static readonly Guid FiberFullCapacityTransmissionServiceDefinitionId = Guid.Parse("f637fa34-a271-4253-94d8-62a379753ac2");

		// Destinations
		public static readonly Guid YleHelsinkiDestinationServiceDefinitionId = Guid.Parse("6e8b2a02-7121-41d5-bc11-d7200ce1d69b");

		public static readonly Guid AreenaDestinationServiceDefinitionId = Guid.Parse("35672072-95c7-406a-a56b-59e885a6d2dc");

		public static readonly Guid EbuDestinationServiceDefinitionId = Guid.Parse("bb43b568-745e-49cf-ae9f-0cb856f31a41");

		public static readonly Guid MediapolisDestinationServiceDefinitionId = Guid.Parse("e7128afc-c5d3-4ceb-bc2b-00c24b5ecd7b");

		// Recordings
		public static readonly Guid RecordingMessiLiveServiceDefinitionId = Guid.Parse("60d9ab48-06bb-49df-9066-d253171af5da");

		public static readonly Guid RecordingMessiLiveBackupServiceDefinitionId = Guid.Parse("f7be12d5-2bce-4c2b-a4f1-951e0cd7f444");

		public static readonly Guid RecordingMessiNewsServiceDefinitionId = Guid.Parse("4910989f-2c21-455f-b5c4-ff2e7aa2d23a");

		public static string BooleanToProfileParameterValue(bool value)
		{
			return value ? "Yes" : "No";
		}
	}
}