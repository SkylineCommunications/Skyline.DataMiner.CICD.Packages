using System.CodeDom;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class ProfileParameterGuids
	{
		public static readonly Guid _DemodulatingConfiguration = new Guid("efd5d280-3997-4cb9-8bbd-28e18d7917b9");

		public static readonly Guid _ServiceConfiguration = new Guid("9083c73c-7eb3-4fc9-85ea-02fea98312a9");

		public static readonly Guid _Dummy = new Guid("68daab6a-9a7e-47fe-af62-ebd65311fcc4");

		public static readonly Guid _Matrix = new Guid("bce4b6a3-4045-4ec0-88d3-ba61c15f8038");

		public static readonly Guid _LnbFrequency = new Guid("5d1c933c-20ed-48b5-85b8-cb61d9b176e7");

		public static readonly Guid _OrbitalPosition = new Guid("a9810b80-c127-47a1-9e08-777dbb758ea1");

		public static readonly Guid _TvChannel = new Guid("26560675-38a6-4dba-af61-e23d75b3af1d");

		public static readonly Guid _FeedType = new Guid("07278965-6138-46de-b1d6-1f02861c240d");

		public static readonly Guid _IntegrationType = new Guid("ce84fa08-ae71-48e2-8731-f6e56428ae79");

		public static readonly Guid AreenaCopy = new Guid("798393c6-4ddf-48c2-92ae-668433de3428");

		public static readonly Guid AreenaDestinationLocation = new Guid("96cc6585-2b3a-4129-a7d3-85c58b61b5de");

		public static readonly Guid AudioDeembeddingRequired = new Guid("1d51086a-ca66-4e1d-b84f-6fd05a24ee68");

		public static readonly Guid AudioEmbeddingRequired = new Guid("a2726fd1-cc4b-4b23-b464-3684a0db4dd6");

		public static readonly Guid AudioShufflingRequired = new Guid("01682ff9-2cc4-4cd5-8252-d5956424479a");

		public static readonly Guid AudioDolbyDecodingRequired = new Guid("35215bb4-0faa-423f-beed-58ada9b604b3");

		public static readonly Guid AudioChannel1 = new Guid(Strings.AudioChannel1String);

		public static readonly Guid AudioChannel2 = new Guid(Strings.AudioChannel2String);

		public static readonly Guid AudioChannel3 = new Guid(Strings.AudioChannel3String);

		public static readonly Guid AudioChannel4 = new Guid(Strings.AudioChannel4String);

		public static readonly Guid AudioChannel5 = new Guid(Strings.AudioChannel5String);

		public static readonly Guid AudioChannel6 = new Guid(Strings.AudioChannel6String);

		public static readonly Guid AudioChannel7 = new Guid(Strings.AudioChannel7String);

		public static readonly Guid AudioChannel8 = new Guid(Strings.AudioChannel8String);

		public static readonly Guid AudioChannel9 = new Guid(Strings.AudioChannel9String);

		public static readonly Guid AudioChannel10 = new Guid(Strings.AudioChannel10String);

		public static readonly Guid AudioChannel11 = new Guid(Strings.AudioChannel11String);

		public static readonly Guid AudioChannel12 = new Guid(Strings.AudioChannel12String);

		public static readonly Guid AudioChannel13 = new Guid(Strings.AudioChannel13String);

		public static readonly Guid AudioChannel14 = new Guid(Strings.AudioChannel14String);

		public static readonly Guid AudioChannel15 = new Guid(Strings.AudioChannel15String);

		public static readonly Guid AudioChannel16 = new Guid(Strings.AudioChannel16String);

		public static readonly Guid AudioChannel1Description = new Guid("fab7b647-df5f-43ac-9396-511fc6ba9f81");

		public static readonly Guid AudioChannel2Description = new Guid("f08209ca-898b-40d3-835d-e5bca4420960");

		public static readonly Guid AudioChannel3Description = new Guid("c4dc3e48-1d7e-48f0-832e-af4eecf716d4");

		public static readonly Guid AudioChannel4Description = new Guid("8b2499e6-653b-409c-8c6c-ed9d56f989fe");

		public static readonly Guid AudioChannel5Description = new Guid("19e761d7-31d1-48fe-8265-9b24f386eafd");

		public static readonly Guid AudioChannel6Description = new Guid("cf014313-b2d3-46a3-8cc3-c7bb57946c5c");

		public static readonly Guid AudioChannel7Description = new Guid("4a7c08cf-67a1-4e37-85fd-89bfe39331a8");

		public static readonly Guid AudioChannel8Description = new Guid("0e478f07-4c48-4cb2-bd44-615cc197141a");

		public static readonly Guid AudioChannel9Description = new Guid("145eda52-eb24-49bd-87b5-31edf8e1002f");

		public static readonly Guid AudioChannel10Description = new Guid("868a72c4-7ed5-4d81-b8fd-c4e803681100");

		public static readonly Guid AudioChannel11Description = new Guid("5b65ad74-d72b-42be-b1d6-87319580df88");

		public static readonly Guid AudioChannel12Description = new Guid("bd8065e8-72d7-4907-a910-9f47ad48e79f");

		public static readonly Guid AudioChannel13Description = new Guid("f114e37c-3627-4dda-8bd2-b0ce466c446b");

		public static readonly Guid AudioChannel14Description = new Guid("3887c50c-c1fc-45aa-97bc-c24eeca812a4");

		public static readonly Guid AudioChannel15Description = new Guid("45f43ff0-9c3b-4dd1-a50a-34faac268612");

		public static readonly Guid AudioChannel16Description = new Guid("f28b5c26-639c-4881-80d1-dc1d6a95c422");

		public static readonly Guid AudioReturnChannel = new Guid("b765fd4c-92ee-48f1-b858-4d04e3172dd9");

		public static readonly Guid DownlinkFrequency = new Guid(Strings.DownlinkFrequencyString);

		public static readonly Guid Encoding = new Guid(Strings.EncodingString);

		public static readonly Guid EncryptionKey = new Guid(Strings.EncryptionKeyString);

		public static readonly Guid Fec = new Guid(Strings.FecString);

		public static readonly Guid FastRerun = new Guid("9100b6ea-5b40-4fce-8066-532bea045f65");

		public static readonly Guid FeedName = new Guid("50e277dc-e0b5-4ba8-8384-d7b1764a3815");

		public static readonly Guid FixedTieLineSource = new Guid("bfe1faa3-62a1-44d6-9db1-6ff7ac07605f");

		public static readonly Guid Polarization = new Guid(Strings.PolarizationString);

		public static readonly Guid RollOff = new Guid(Strings.RollOffString);

		public static readonly Guid ResourceInputConnectionsAsi = new Guid("58a7e9e4-6a39-42c9-b275-92f9dc7f9003");

		public static readonly Guid ResourceInputConnectionsLband = new Guid("5c8ea48d-fafb-4fae-9f43-8ce2e9347d90");

		public static readonly Guid ResourceInputConnectionsSdi = new Guid("3fab320b-6059-47ee-aad3-bf98fbba9f06");

		public static readonly Guid ResourceOutputConnectionsAsi = new Guid("3c08c3a7-d1fb-46f2-877b-8927345cf931");

		public static readonly Guid ResourceOutputConnectionsLband = new Guid("a12733e0-1ba5-48b8-a253-1f353eaeae79");

		public static readonly Guid ResourceOutputConnectionsSdi = new Guid("1f5d4759-1d66-4cbe-a4c0-6bb6fb6937d6");

		public static readonly Guid ResourceInputConnectionsNdi = new Guid("5b669aef-6fd9-4bdb-867c-f3997571735c");

		public static readonly Guid ResourceOutputConnectionsNdi = new Guid("65f63694-b861-4d95-879e-0d0f2f655472");

		public static readonly Guid RemoteGraphics = new Guid("7504c7e2-4c65-4230-8302-41e4fc90d210");

		public static readonly Guid SubtitleProxy = new Guid("7610d0f7-e068-4cd9-a014-0308b277d4f2");

		public static readonly Guid SymbolRate = new Guid(Strings.SymbolRateString);

		public static readonly Guid ModulationStandard = new Guid(Strings.ModulationStandardString);

		public static readonly Guid Modulation = new Guid(Strings.ModulationString);

		public static readonly Guid EncryptionType = new Guid(Strings.EncryptionTypeString);

		public static readonly Guid VideoFormat = new Guid(Strings.VideoFormatString);

		public static readonly Guid RecordingVideoFormat = new Guid("105baed9-4031-4d7c-a436-d26fe608af7b");

		public static readonly Guid ExternalCompaniesDestinationLocation = new Guid("62e5716c-85d8-4d01-a308-b87cc461b1b3");

		public static readonly Guid EbuDestinationLocation = new Guid("b4213f38-3bb7-48a4-a61c-3ec0b609785e");

		public static readonly Guid YleHelsinkiDestinationLocation = new Guid("603c52e0-ca2b-4171-bba9-9469250031d4");

		public static readonly Guid YleMediapolisDestinationLocation = new Guid("ea655627-a17d-442a-9e51-5ddebee9755f");

		public static readonly Guid FixedLineExternalCompaniesSourceLocation = new Guid("f0c352c7-5fa6-4338-a201-94c18fdb8d4e");

		public static readonly Guid FixedLineHelsinkiCityConnectionsSourceLocation = new Guid("b1224510-bfbf-4506-9120-1eb33ad353d4");

		public static readonly Guid FixedLineYleHelsinkiSourceLocation = new Guid("b9565c0d-a26f-4fa9-985a-cc162e8524d7");

		public static readonly Guid FixedLineEbuSourceLocation = new Guid("b6877c1e-99c3-4971-a4da-7c47bcf274e3");

		public static readonly Guid FixedLineLySourcePlasmaUserCode = new Guid("7bab8775-841d-4b14-ae83-e665d4768b47");

		public static readonly Guid Channel = new Guid("af8e34b9-6a5b-4126-b65e-ec670e4099bd");

		public static readonly Guid ServiceSelection = new Guid(Strings.ServiceSelectionString);

		public static readonly Guid OtherSatelliteName = new Guid("1d95c7e7-06be-4459-a5ef-a9975958ba3a");

		public static readonly IReadOnlyCollection<Guid> AllAudioProcessingRequiredGuids = new[] { AudioDeembeddingRequired, AudioDolbyDecodingRequired, AudioEmbeddingRequired, AudioShufflingRequired };

		public static readonly IReadOnlyCollection<Guid> AllAudioChannelConfigurationGuids = new[] { AudioDeembeddingRequired, AudioEmbeddingRequired, AudioShufflingRequired, AudioDolbyDecodingRequired, AudioChannel1, AudioChannel1Description, AudioChannel2, AudioChannel2Description, AudioChannel3, AudioChannel3Description, AudioChannel4, AudioChannel4Description, AudioChannel5, AudioChannel5Description, AudioChannel6, AudioChannel6Description, AudioChannel7, AudioChannel7Description, AudioChannel8, AudioChannel8Description, AudioChannel9, AudioChannel9Description, AudioChannel10, AudioChannel10Description, AudioChannel11, AudioChannel11Description, AudioChannel12, AudioChannel12Description, AudioChannel13, AudioChannel13Description, AudioChannel14, AudioChannel14Description, AudioChannel15, AudioChannel15Description, AudioChannel16, AudioChannel16Description };

		public static readonly IReadOnlyCollection<Guid> AllProcessingParameterGuids = new[] { ProfileParameterGuids.VideoFormat, ProfileParameterGuids.RemoteGraphics }.Concat(AllAudioProcessingRequiredGuids).ToArray();

		public static readonly IReadOnlyCollection<Guid> AllResourceInputConnectionGuids = new[] { ResourceInputConnectionsSdi, ResourceInputConnectionsLband, ResourceInputConnectionsAsi };

		public static readonly IReadOnlyCollection<Guid> AllResourceOutputConnectionGuids = new[] { ResourceOutputConnectionsSdi, ResourceOutputConnectionsLband, ResourceOutputConnectionsAsi };

		public static readonly Guid InputVideoFormat = Guid.Parse("79a7d1f5-d750-400f-b5b9-b5e6d198ad25");

		public static readonly Guid OutputVideoFormat = Guid.Parse("209d06c7-3b7a-49ae-a11f-b6d9f5920fca");

		public static readonly Guid InputAudioChannel1 = Guid.Parse("b60f8740-f593-431b-8043-474ccb8f9566");

		public static readonly Guid InputAudioChannel2 = Guid.Parse("1fe71070-44d5-4ad0-95db-103413935506");

		public static readonly Guid InputAudioChannel3 = Guid.Parse("00d823aa-4363-4d49-9eb6-fa2a2712bf9c");

		public static readonly Guid InputAudioChannel4 = Guid.Parse("bc63cd0f-1886-45ff-a675-659a2f840166");

		public static readonly Guid InputAudioChannel5 = Guid.Parse("7d8a7d82-a328-4f5c-9c4d-a1f634fdefc5");

		public static readonly Guid InputAudioChannel6 = Guid.Parse("81352369-5ece-4c1e-98d6-bd908589e2c4");

		public static readonly Guid InputAudioChannel7 = Guid.Parse("cb840609-c68c-4636-9225-b3667685e29c");

		public static readonly Guid InputAudioChannel8 = Guid.Parse("11ea232f-511e-463a-927a-d4ed4fac52b6");

		public static readonly Guid InputAudioChannel9 = Guid.Parse("96837519-a1be-4415-aef9-ec7c148a8ce0");

		public static readonly Guid InputAudioChannel10 = Guid.Parse("6701766b-60d4-499c-8e84-ee91f2afbc65");

		public static readonly Guid InputAudioChannel11 = Guid.Parse("e39f61ce-515f-4134-b308-32fb6c3df9c0");

		public static readonly Guid InputAudioChannel12 = Guid.Parse("b7e5efb5-8c48-4670-9d3b-c8315beeb0f8");

		public static readonly Guid InputAudioChannel13 = Guid.Parse("4da8b4de-178c-4bbe-b48c-69f25dba46c9");

		public static readonly Guid InputAudioChannel14 = Guid.Parse("c1f16b91-a429-4e09-b315-34900b6a0906");

		public static readonly Guid InputAudioChannel15 = Guid.Parse("b305ea6d-9028-44e3-8b72-e394ade2ac0c");

		public static readonly Guid InputAudioChannel16 = Guid.Parse("01133e53-fa18-48d8-bb3e-073326618f3c");

		public static readonly Guid OutputAudioChannel1 = Guid.Parse("c18bcf3e-7985-45f4-b968-56826861b761");

		public static readonly Guid OutputAudioChannel2 = Guid.Parse("3c8bc1f0-8509-4ca8-91a9-e3528cd69bc6");

		public static readonly Guid OutputAudioChannel3 = Guid.Parse("edbc7e6d-0823-41b4-a0ac-89e0b0bb70a4");

		public static readonly Guid OutputAudioChannel4 = Guid.Parse("9031245f-6c36-4437-a22d-b2b65fdedb17");

		public static readonly Guid OutputAudioChannel5 = Guid.Parse("26f31192-01ab-45a1-b214-a56dab65072f");

		public static readonly Guid OutputAudioChannel6 = Guid.Parse("f6e684f5-3627-4fb8-9406-2e6898655440");

		public static readonly Guid OutputAudioChannel7 = Guid.Parse("447bb387-397d-4565-88ff-972488e1feba");

		public static readonly Guid OutputAudioChannel8 = Guid.Parse("cdfb4027-3ebb-4167-bb87-b94e732f1461");

		public static readonly Guid OutputAudioChannel9 = Guid.Parse("0f5ecba8-3c17-4831-b7f0-cca2a80e001f");

		public static readonly Guid OutputAudioChannel10 = Guid.Parse("fee87c0a-bb43-4819-ad66-1d366f7435ed");

		public static readonly Guid OutputAudioChannel11 = Guid.Parse("de03ea6a-029e-4bf2-ad57-646a14417c36");

		public static readonly Guid OutputAudioChannel12 = Guid.Parse("9f964476-2a3b-4c49-8e75-deadfc91bc04");

		public static readonly Guid OutputAudioChannel13 = Guid.Parse("65929b68-e642-4ed6-a669-27455690137e");

		public static readonly Guid OutputAudioChannel14 = Guid.Parse("8de81be4-8daf-4e30-9a5f-2ae6fd1a22dd");

		public static readonly Guid OutputAudioChannel15 = Guid.Parse("a4e3c96b-7c15-43a8-944d-61ddcc7c2905");

		public static readonly Guid OutputAudioChannel16 = Guid.Parse("517be756-d2d3-4ce1-b9e7-cee150264c0c");

		public static readonly Guid _Direction = Guid.Parse("4313870b-6eb5-4f7f-bdc3-32c2a439f0f2");

		public static bool IsAudioChannelProfileParameter(Guid id)
		{
			return AllAudioChannelConfigurationGuids.Except(AllAudioProcessingRequiredGuids).Contains(id);
		}

		public static class Strings
		{
#pragma warning disable S2339 // Public constant members should not be used
			public const string AudioChannel1String = "d4f1e0fe-1a72-4ec3-9fd8-aefd4785ebdf";
			public const string AudioChannel2String = "bce7eff1-a884-47af-a182-559e0cfa4379";
			public const string AudioChannel3String = "471d52de-b90c-4d8d-9cb5-b11916a06d08";
			public const string AudioChannel4String = "5f014774-c3ce-492e-96c3-cd7402dbe171";
			public const string AudioChannel5String = "fad366e6-2f29-466b-a286-434446cf6437";
			public const string AudioChannel6String = "5c68fb8a-5c8f-4970-aeb4-b04b5f465724";
			public const string AudioChannel7String = "83b3d476-39dd-4660-854f-27bbe6436914";
			public const string AudioChannel8String = "e9aeb156-a027-4898-b41e-6af88169a3ff";
			public const string AudioChannel9String = "5eadca8d-f96a-464d-81a4-139dc6da0fba";
			public const string AudioChannel10String = "7178133a-d8a8-485d-99fa-b909ef73d848";
			public const string AudioChannel11String = "052babfe-b396-4d3f-a305-21442b3e0fd1";
			public const string AudioChannel12String = "acb1bef4-f80b-424b-abd1-a51a05586a6d";
			public const string AudioChannel13String = "fa83f575-0935-4952-b561-ce349c7e59fb";
			public const string AudioChannel14String = "f60e0b3e-840a-47f1-b521-9e1dbdc28562";
			public const string AudioChannel15String = "2712a2f9-273a-439f-b85a-5101789782cf";
			public const string AudioChannel16String = "cc76cdc0-925c-472f-985f-c0c5e477639c";
			public const string DownlinkFrequencyString = "d045fe27-2163-4e9b-b4e6-41ec3eac2a5d";
			public const string EncodingString = "3efcc36d-2cb9-4f22-a1e1-fae50def51ca";
			public const string EncryptionKeyString = "09995fd9-9526-485c-99cb-1d9eb6178195";
			public const string EncryptionTypeString = "27465703-cd3c-40a7-81e4-cf72119a1e3b";
			public const string FecString = "3f6fb925-e996-496c-8760-34436590c47e";
			public const string PolarizationString = "3f5f30c2-a4b1-4ab6-be06-ecec2e35cd5d";
			public const string RollOffString = "a28cf11f-ccfe-4a76-99cd-cac5ee402a9a";
			public const string SymbolRateString = "9a752cb7-77f2-4f75-8b9a-9a290eaf81b5";
			public const string ModulationString = "ea300fb7-527f-4b14-aff9-7985875b82e3";
			public const string ModulationStandardString = "ed8446e9-e373-419d-9b9f-8a1c94ec8ae7";
			public const string ServiceSelectionString = "0e4008ca-5dd8-40f5-b141-cd8b631444ee";
			public const string VideoFormatString = "8dc6df35-c574-4412-bf52-de7e3c78201c";
#pragma warning restore S2339 // Public constant members should not be used
		}
	}
}