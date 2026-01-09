namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	using System;
	using System.Collections.Generic;

	public static class ServiceDefinitionGuids
	{
		public static readonly Guid RecordingMessiLive = Guid.Parse("60d9ab48-06bb-49df-9066-d253171af5da");

		public static readonly Guid RecordingMessiLiveBackup = Guid.Parse("f7be12d5-2bce-4c2b-a4f1-951e0cd7f444");

		public static readonly Guid RecordingMessiNews = Guid.Parse("4910989f-2c21-455f-b5c4-ff2e7aa2d23a");

		public static readonly Guid YleHelsinkiDestination = Guid.Parse("6e8b2a02-7121-41d5-bc11-d7200ce1d69b");

		public static readonly Guid YleVaasaDestinationServiceDefinitionId = Guid.Parse("5e080002-11a7-4aef-a495-e52d2a1a9d46");

		public static readonly Guid YleMediapolisDestinationServiceDefinitionId = Guid.Parse("e7128afc-c5d3-4ceb-bc2b-00c24b5ecd7b");

		public static readonly Guid HelsinkiCityConnectionsDestinationServiceDefinitionId = Guid.Parse("7a1a7e8d-b5b2-46c8-b40f-5fcdf56f8622");

		public static readonly Guid AreenaDestinationServiceDefinitionId = Guid.Parse("35672072-95c7-406a-a56b-59e885a6d2dc");

		public static readonly Guid EbuDestinationServiceDefinitionId = Guid.Parse("bb43b568-745e-49cf-ae9f-0cb856f31a41");

		public static readonly Guid OtherDestinationServiceDefinitionId = Guid.Parse("c9969ee2-6744-4fb2-9656-554eec969b86");

		public static readonly Guid ExternalCompaniesDestinationServiceDefinitionId = Guid.Parse("7dc30fe6-7d6f-40a7-ba33-fcb88fc22abc");

		public static readonly Guid SatelliteTransmissionServiceDefinitionId = Guid.Parse("5fad39cb-6521-4208-b49b-0cfd063807b5");

		public static readonly Guid FiberFullCapacityTransmissionServiceDefinitionId = Guid.Parse("f637fa34-a271-4253-94d8-62a379753ac2");

		public static readonly Guid FixedLineLy = Guid.Parse("ff913685-f764-4cf1-8ca9-7c9c751e4778");

		public static readonly Guid LiveUMediapolisRxServiceDefinitionId = Guid.Parse("142c5ab9-25f2-45c4-8390-755024bfdb17");

		public static readonly Guid LiveUPasilaRxServiceDefinitionId = Guid.Parse("508564f6-01ea-4973-b441-8d61ea54e1ef");

		public static readonly Guid SatelliteReceptionServiceDefinitionId = Guid.Parse("aecb710b-558e-46b9-8a5e-86886f7baa2f");

		public static readonly Guid FiberFullCapacityReceptionServiceDefinitionId = Guid.Parse("e7d1e9bf-c158-4671-b51f-a2a6cecdd0c4");

		public static readonly Guid FixedLineEbuReceptionServiceDefinitionId = Guid.Parse("74e0b355-3f0c-437e-b605-98762bdf3f6d");

		public static readonly Guid FixedLineYleHelsinkiReceptionServiceDefinitionId = Guid.Parse("eb5023f0-c782-45e9-8652-e50d3b8cc6b4");

		public static readonly Guid FixedLineAtvuReception = Guid.Parse("1555ec44-0ced-4d8d-adf5-7fdebb6d5700");

		public static readonly Guid RoutingServiceDefinitionId = Guid.Parse("956d6249-21cd-461e-b3a2-2b04b30d9c67");

		public static readonly Guid GraphicsProcessing = Guid.Parse("7d1af087-ae03-4ac8-9193-21947cecb85d");

		public static readonly Guid VideoProcessing = Guid.Parse("d013dcc8-3951-4246-a7b2-c6fbc3d4bcd6");

		public static readonly Guid AudioProcessing = Guid.Parse("eb543606-1b4f-48b6-92c2-27d197afb77b");

		public static readonly Guid IpReceptionEbuFlex = Guid.Parse("e0bd4107-88d8-4f36-8aaf-025ecd9a8138");

		public static readonly Guid IpReceptionRtmp = Guid.Parse("1775b440-4043-4ddf-9cb2-46810fe85783");

		public static readonly Guid IpReceptionSrt = Guid.Parse("bd018cf4-38f3-4395-a5e3-e65c4ecc75ab");

		public static readonly Guid VizremFarm = Guid.Parse("62f6c491-013a-4156-95c2-20fc749f4cc1");

		public static readonly Guid VizremStudioHelsinki = Guid.Parse("795dff72-f74b-464a-92c0-8d94bf8e4caa");

		public static readonly Guid VizremStudioMediapolis = Guid.Parse("c142bada-50d9-48b6-b138-90e252ad88cf");

		public static readonly Guid St26NdiRouter = Guid.Parse("b1cb45db-8e0a-4a25-9ac7-630efff83249");

		public static List<Guid> AllVizremServiceDefinitions => new List<Guid> { VizremFarm, VizremStudioHelsinki, VizremStudioMediapolis, St26NdiRouter };

		public static List<Guid> AllProcessingServiceDefinitions => new List<Guid> { VideoProcessing, AudioProcessing, GraphicsProcessing };
	}
}