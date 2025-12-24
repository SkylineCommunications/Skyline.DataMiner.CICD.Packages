namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Controllers
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.Automation;
	using Service = Service.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Newtonsoft.Json;
	using Type = Integrations.Eurovision.Type;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Newtonsoft.Json.Bson;

	public class EurovisionController
	{
		private readonly Helpers helpers;
		private readonly DisplayedService service;
		private readonly EurovisionSection section;
		private readonly NormalOrderSection orderSection;

		private IActionableElement ebuElement;

		public EurovisionController(Helpers helpers, Service service, EurovisionSection section, NormalOrderSection orderSection)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			this.service = service as DisplayedService ?? throw new ArgumentNullException(nameof(service));
			this.section = section ?? throw new ArgumentNullException(nameof(section));
			this.orderSection = orderSection ?? throw new ArgumentNullException(nameof(orderSection));

			helpers.Log(nameof(EurovisionController), nameof(EurovisionController), $"Booking Details: {JsonConvert.SerializeObject(service.EurovisionBookingDetails, Formatting.Indented)}");

			SubscribeToUi();
			InitializeValues();
		}

		public event EventHandler BookEurovisionService;

		public event EventHandler ServiceTimingChanged;

		private void SubscribeToUi()
		{
			orderSection.GeneralInfoSection.UserGroupChanged += (s, e) => UpdateEurovisionElement(e);

			section.TypeChanged += (o, e) =>
			{
				service.EurovisionBookingDetails.Type = e;
				BookingDetails_TypeChanged(e);
			};

			section.BookButtonClicked += (o, e) => BookEurovisionService?.Invoke(this, new EventArgs());

			section.SynopsisIdChanged += (o, e) => service.EurovisionTransmissionNumber = e;

			SubscribeToNewsEventSection();
			SubscribeToProgramEventSection();
			SubscribeToSatelliteCapacitySection();
			SubscribeToOssTransmissionSection();
			SubscribeToUnilateralTransmissionSection();
		}

		private void SubscribeToNewsEventSection()
		{
			// Eurovision News Event events - General
			section.NewsEventSection.EventChanged += (s, e) => NewsEvent_EventChanged(e);
			section.NewsEventSection.DestinationOrganizationChanged += (s, e) => service.EurovisionBookingDetails.DestinationOrganizationCode = e?.Code ?? String.Empty;
			section.NewsEventSection.DestinationCityChanged += (s, e) => service.EurovisionBookingDetails.DestinationCityCode = e?.Code ?? String.Empty;
			section.NewsEventSection.FeedpointChanged += (s, e) => OriginFeedpointChanged(e);
			section.NewsEventSection.FacilityChanged += (s, e) => service.EurovisionBookingDetails.FacilityProductId = e?.ProductId ?? String.Empty;
			section.NewsEventSection.ContactChanged += (s, e) => service.EurovisionBookingDetails.Contact = e;
			section.NewsEventSection.DescriptionChanged += (s, e) => service.EurovisionBookingDetails.Description = e;
			section.NewsEventSection.NoteChanged += (s, e) => service.EurovisionBookingDetails.Note = e;
			section.NewsEventSection.ContractChanged += (s, e) => service.EurovisionBookingDetails.ContractCode = e?.Code ?? String.Empty;

			// Eurovision News Event events - Video Section
			SubscribeToVideoSection(section.NewsEventSection.VideoSection);

			// Eurovision News Event events - Audio Section
			SubscribeToAudioSection(section.NewsEventSection.AudioSection);
		}

		private void SubscribeToProgramEventSection()
		{
			section.ProgramEventSection.EventChanged += (s, e) => ProgramEvent_EventChanged(e);
			section.ProgramEventSection.DestinationOrganizationChanged += (s, e) => service.EurovisionBookingDetails.DestinationOrganizationCode = e?.Code ?? String.Empty;
			section.ProgramEventSection.DestinationCityChanged += (s, e) => service.EurovisionBookingDetails.DestinationCityCode = e?.Code ?? String.Empty;
			section.ProgramEventSection.ParticipationChanged += (s, e) => ProgramEvent_ParticipationChanged(e);
			section.ProgramEventSection.ContactChanged += (s, e) => service.EurovisionBookingDetails.Contact = e;
			section.ProgramEventSection.NoteChanged += (s, e) => service.EurovisionBookingDetails.Note = e;
			section.ProgramEventSection.ContractChanged += (s, e) => service.EurovisionBookingDetails.ContractCode = e?.Code ?? String.Empty;
		}

		private void SubscribeToSatelliteCapacitySection()
		{
			// Eurovision Satellite Capacity events - General
			section.SatelliteCapacitySection.SatelliteChanged += (s, e) => SatelliteCapacity_SatelliteChanged(e);
			section.SatelliteCapacitySection.UplinkChanged += (s, e) => SatelliteCapacity_UplinkChanged(e);
			section.SatelliteCapacitySection.OriginChanged += (s, e) => OriginOrganizationChanged(e);
			section.SatelliteCapacitySection.OriginCityChanged += (s, e) => OriginFeedpointChanged(e);
			section.SatelliteCapacitySection.ContractChanged += (s, e) => service.EurovisionBookingDetails.ContractCode = e?.Code ?? String.Empty;
			section.SatelliteCapacitySection.NoteChanged += (s, e) => service.EurovisionBookingDetails.Note = e.Value;
			section.SatelliteCapacitySection.LineUpChanged += (s, e) => service.EurovisionBookingDetails.LineUp = e.Value;
			section.SatelliteCapacitySection.ContactFirstNameChanged += (s, e) =>
			{
				service.EurovisionBookingDetails.ContactFirstName = e.Value;
				service.EurovisionBookingDetails.Contact = $"{e.Value} {service.EurovisionBookingDetails.ContactLastName}";
			};

			section.SatelliteCapacitySection.ContactLastNameChanged += (s, e) =>
			{
				service.EurovisionBookingDetails.ContactLastName = e.Value;
				service.EurovisionBookingDetails.Contact = $"{service.EurovisionBookingDetails.ContactFirstName} {e.Value}";
			};

			section.SatelliteCapacitySection.EmailChanged += (s, e) => service.EurovisionBookingDetails.Email = e.Value;
			section.SatelliteCapacitySection.PhoneChanged += (s, e) => service.EurovisionBookingDetails.Phone = e.Value;

			// Eurovision News Event events - Video Section
			SubscribeToVideoSection(section.SatelliteCapacitySection.VideoSection);

			// Eurovision News Event events - Audio Section
			SubscribeToAudioSection(section.SatelliteCapacitySection.AudioSection);
		}

		private void SubscribeToOssTransmissionSection()
		{
			// Eurovision OSS Transmission events - General
			section.OssTransmissionSection.OriginOrganizationChanged += (s, e) => OriginOrganizationChanged(e);
			section.OssTransmissionSection.OriginCityChanged += (s, e) => OriginFeedpointChanged(e);
			section.OssTransmissionSection.FacilityChanged += (s, e) => service.EurovisionBookingDetails.FacilityProductId = e?.ProductId ?? String.Empty;
			section.OssTransmissionSection.DestinationOrganizationChanged += (s, e) => DestinationOrganizationChanged(e);
			section.OssTransmissionSection.DestinationCityChanged += (s, e) => DestinationCityChanged(e);
			section.OssTransmissionSection.ContactChanged += (s, e) => service.EurovisionBookingDetails.Contact = e;
			section.OssTransmissionSection.ContractChanged += (s, e) => service.EurovisionBookingDetails.ContractCode = e?.Code ?? String.Empty;
			section.OssTransmissionSection.NoteChanged += (s, e) => service.EurovisionBookingDetails.Note = e;

			// Eurovision OSS Transmission events - Video Section
			SubscribeToVideoSection(section.OssTransmissionSection.VideoSection);

			// Eurovision OSS Transmission events - Audio Section
			SubscribeToAudioSection(section.OssTransmissionSection.AudioSection);
		}

		private void SubscribeToUnilateralTransmissionSection()
		{
			// Eurovision Unilateral Transmission events - General
			section.UnilateralTransmissionSection.OriginOrganizationChanged += (s, e) => OriginOrganizationChanged(e);
			section.UnilateralTransmissionSection.OriginCityChanged += (s, e) => OriginFeedpointChanged(e);
			section.UnilateralTransmissionSection.DestinationOrganizationChanged += (s, e) => DestinationOrganizationChanged(e);
			section.UnilateralTransmissionSection.DestinationCityChanged += (s, e) => DestinationCityChanged(e);
			section.UnilateralTransmissionSection.ContactChanged += (s, e) => service.EurovisionBookingDetails.Contact = e;
			section.UnilateralTransmissionSection.ContractChanged += (s, e) => service.EurovisionBookingDetails.ContractCode = e?.Code ?? String.Empty;
			section.UnilateralTransmissionSection.NoteChanged += (s, e) => service.EurovisionBookingDetails.Note = e;

			// Eurovision Unilateral Transmission events - Video Section
			SubscribeToVideoSection(section.UnilateralTransmissionSection.VideoSection);

			// Eurovision Unilateral Transmission events - Audio Section
			SubscribeToAudioSection(section.UnilateralTransmissionSection.AudioSection);
		}

		private void SubscribeToVideoSection(VideoSection videoSection)
		{
			videoSection.VideoDefinitionChanged += (s, e) => VideoDefinitionChanged(e);
			videoSection.VideoResolutionChanged += (s, e) => service.EurovisionBookingDetails.VideoResolutionCode = e?.Code ?? String.Empty;
			videoSection.VideoAspectRatioChanged += (s, e) => service.EurovisionBookingDetails.VideoAspectRatioCode = e?.Code ?? String.Empty;
			videoSection.VideoBitrateChanged += (s, e) => service.EurovisionBookingDetails.VideoBitrateCode = e?.Code ?? String.Empty;
			videoSection.VideoFrameRateChanged += (s, e) => service.EurovisionBookingDetails.VideoFrameRateCode = e?.Code ?? String.Empty;
			videoSection.VideoBandwidthChanged += (s, e) => service.EurovisionBookingDetails.VideoBandwidthCode = e?.Code ?? String.Empty;
		}

		private void SubscribeToAudioSection(AudioSection audioSection)
		{
			SubscribeToAudioPair1(audioSection);
			SubscribeToAudioPair2(audioSection);
			SubscribeToAudioPair3(audioSection);
			SubscribeToAudioPair4(audioSection);
		}

		private void SubscribeToAudioPair1(AudioSection audioSection)
		{
			audioSection.AudioChannel1Changed += (s, e) =>
			{
				service.EurovisionBookingDetails.AudioChannel1.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;
				if (audioSection.AreAudioChannels1And2Stereo) service.EurovisionBookingDetails.AudioChannel2.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;
			};

			audioSection.AudioChannel1OtherTextChanged += (s, e) =>
			{
				service.EurovisionBookingDetails.AudioChannel1.AudioChannelOtherText = e;
				if (audioSection.AreAudioChannels1And2Stereo) service.EurovisionBookingDetails.AudioChannel2.AudioChannelOtherText = e;
			};

			audioSection.AudioChannel2Changed += (s, e) => service.EurovisionBookingDetails.AudioChannel2.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;

			audioSection.AudioChannel2OtherTextChanged += (s, e) => service.EurovisionBookingDetails.AudioChannel2.AudioChannelOtherText = e;

			audioSection.IsAudioChannel1And2StereoChanged += (s, e) =>
			{
				if (!e) return;
				service.EurovisionBookingDetails.AudioChannel2.AudioChannelCode = service.EurovisionBookingDetails.AudioChannel1.AudioChannelCode;
				service.EurovisionBookingDetails.AudioChannel2.AudioChannelOtherText = service.EurovisionBookingDetails.AudioChannel1.AudioChannelOtherText;
			};
		}

		private void SubscribeToAudioPair2(AudioSection audioSection)
		{
			audioSection.AudioChannel3Changed += (s, e) =>
			{
				service.EurovisionBookingDetails.AudioChannel3.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;
				if (audioSection.AreAudioChannels3And4Stereo) service.EurovisionBookingDetails.AudioChannel4.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;
			};

			audioSection.AudioChannel3OtherTextChanged += (s, e) =>
			{
				service.EurovisionBookingDetails.AudioChannel3.AudioChannelOtherText = e;
				if (audioSection.AreAudioChannels3And4Stereo) service.EurovisionBookingDetails.AudioChannel4.AudioChannelOtherText = e;
			};

			audioSection.AudioChannel4Changed += (s, e) => service.EurovisionBookingDetails.AudioChannel4.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;

			audioSection.AudioChannel4OtherTextChanged += (s, e) => service.EurovisionBookingDetails.AudioChannel4.AudioChannelOtherText = e;

			audioSection.IsAudioChannel3And4StereoChanged += (s, e) =>
			{
				if (!e) return;
				service.EurovisionBookingDetails.AudioChannel4.AudioChannelCode = service.EurovisionBookingDetails.AudioChannel3.AudioChannelCode;
				service.EurovisionBookingDetails.AudioChannel4.AudioChannelOtherText = service.EurovisionBookingDetails.AudioChannel3.AudioChannelOtherText;
			};
		}

		private void SubscribeToAudioPair3(AudioSection audioSection)
		{
			audioSection.AudioChannel5Changed += (s, e) =>
			{
				service.EurovisionBookingDetails.AudioChannel5.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;
				if (audioSection.AreAudioChannels5And6Stereo) service.EurovisionBookingDetails.AudioChannel6.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;
			};

			audioSection.AudioChannel5OtherTextChanged += (s, e) =>
			{
				service.EurovisionBookingDetails.AudioChannel5.AudioChannelOtherText = e;
				if (audioSection.AreAudioChannels5And6Stereo) service.EurovisionBookingDetails.AudioChannel6.AudioChannelOtherText = e;
			};

			audioSection.AudioChannel6Changed += (s, e) => service.EurovisionBookingDetails.AudioChannel6.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;

			audioSection.AudioChannel6OtherTextChanged += (s, e) => service.EurovisionBookingDetails.AudioChannel6.AudioChannelOtherText = e;

			audioSection.IsAudioChannel5And6StereoChanged += (s, e) =>
			{
				if (!e) return;
				service.EurovisionBookingDetails.AudioChannel6.AudioChannelCode = service.EurovisionBookingDetails.AudioChannel5.AudioChannelCode;
				service.EurovisionBookingDetails.AudioChannel6.AudioChannelOtherText = service.EurovisionBookingDetails.AudioChannel5.AudioChannelOtherText;
			};
		}

		private void SubscribeToAudioPair4(AudioSection audioSection)
		{
			audioSection.AudioChannel7Changed += (s, e) =>
			{
				service.EurovisionBookingDetails.AudioChannel7.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;
				if (audioSection.AreAudioChannels7And8Stereo) service.EurovisionBookingDetails.AudioChannel8.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;
			};

			audioSection.AudioChannel7OtherTextChanged += (s, e) =>
			{
				service.EurovisionBookingDetails.AudioChannel7.AudioChannelOtherText = e;
				if (audioSection.AreAudioChannels7And8Stereo) service.EurovisionBookingDetails.AudioChannel8.AudioChannelOtherText = e;
			};

			audioSection.AudioChannel8Changed += (s, e) => service.EurovisionBookingDetails.AudioChannel8.AudioChannelCode = e?.Code ?? AudioChannel.DefaultCode;

			audioSection.AudioChannel8OtherTextChanged += (s, e) => service.EurovisionBookingDetails.AudioChannel8.AudioChannelOtherText = e;

			audioSection.IsAudioChannel7And8StereoChanged += (s, e) =>
			{
				if (!e) return;
				service.EurovisionBookingDetails.AudioChannel8.AudioChannelCode = service.EurovisionBookingDetails.AudioChannel7.AudioChannelCode;
				service.EurovisionBookingDetails.AudioChannel8.AudioChannelOtherText = service.EurovisionBookingDetails.AudioChannel7.AudioChannelOtherText;
			};
		}

		private void InitializeValues()
		{
			UpdateEurovisionElement(orderSection.GeneralInfoSection.UserGroup);
		}

		private void UpdateEurovisionElement(string userGroup)
		{
			helpers.Log(nameof(EurovisionController), nameof(UpdateEurovisionElement), $"Retrieving EBU element for UserGroup {userGroup}");

			string ebuElementName = helpers.OrderManagerElement.GetEurovisionElementName(userGroup);
			ebuElement = String.IsNullOrWhiteSpace(ebuElementName) ? null : helpers.Engine.FindElement(ebuElementName);

			helpers.Log(nameof(EurovisionController), nameof(UpdateEurovisionElement), $"Retrieved element: {ebuElement?.Name}");

			if (ebuElement == null)
			{
				service.EurovisionBookingDetails.Contracts = new List<Eurovision.Contract>();
				service.EurovisionBookingDetails.Events = new List<Event>();
				service.EurovisionBookingDetails.Organizations = new List<Organization>();
				service.EurovisionBookingDetails.Cities = new List<City>();
				service.EurovisionBookingDetails.Audios = new List<Audio>();
				service.EurovisionBookingDetails.Satellites = new List<Satellite>();
				service.EurovisionBookingDetails.Transportables = new List<Transportable>();
				service.EurovisionBookingDetails.Facilities = new List<Facility>();
				service.EurovisionBookingDetails.OriginFeedpoints = new List<BroadcastCenter>();
				service.EurovisionBookingDetails.DestinationFeedpoints = new List<BroadcastCenter>();
				service.EurovisionBookingDetails.VideoDefinitions = new List<VideoDefinition>();
				service.EurovisionBookingDetails.VideoAspectRatios = new List<VideoAspectRatio>();
				service.EurovisionBookingDetails.VideoBandWidths = new List<VideoBandwidth>();
				service.EurovisionBookingDetails.VideoBitrates = new List<VideoBitrate>();
				service.EurovisionBookingDetails.VideoFrameRates = new List<VideoFrameRate>();
				service.EurovisionBookingDetails.VideoResolutions = new List<Eurovision.VideoResolution>();
				return;
			}

			// Init booking details
			service.EurovisionBookingDetails.Contracts = ebuElement.GetContracts(helpers.Engine);
			service.EurovisionBookingDetails.Events = ebuElement.GetEvents(helpers.Engine).Values.ToList();
			service.EurovisionBookingDetails.Organizations = ebuElement.GetOrganizations(helpers.Engine);
			service.EurovisionBookingDetails.Cities = ebuElement.GetCities(helpers.Engine);
			service.EurovisionBookingDetails.Audios = ebuElement.GetAudios(helpers.Engine);
			service.EurovisionBookingDetails.Satellites = ebuElement.GetSatellites(helpers.Engine);
			service.EurovisionBookingDetails.Transportables = ebuElement.GetTransportables(helpers.Engine);

			BookingDetails_TypeChanged(service.EurovisionBookingDetails.Type);
		}

		/// <summary>
		/// Used to initialize default values when a new EBU booking is created or when switching types.
		/// </summary>
		/// <param name="type">Type of EBU booking to initialize.</param>
		private void BookingDetails_TypeChanged(Type type)
		{
			switch (type)
			{
				case Type.NewsEvent:
					InitializeNewsEvent();
					service.EurovisionTransmissionNumber = String.Empty;
					break;
				case Type.ProgramEvent:
					InitializeProgramEvent();
					service.EurovisionTransmissionNumber = String.Empty;
					break;
				case Type.SatelliteCapacity:
					InitializeSatelliteCapacity();
					service.EurovisionTransmissionNumber = String.Empty;
					break;
				case Type.UnilateralTransmission:
					InitializeUnilateralTransmission();
					service.EurovisionTransmissionNumber = String.Empty;
					break;
				case Type.OSSTransmission:
					InitializeOssTransmission();
					service.EurovisionTransmissionNumber = String.Empty;
					break;
				default:
					// None
					service.EurovisionTransmissionNumber = section.SynopsisId;
					return;
			}
		}

		private void InitializeNewsEvent()
		{
			// Check for news events if a news event was already defined in the booking details
			Event newsEvent;
			if (!String.IsNullOrWhiteSpace(service.EurovisionBookingDetails.EventId) && service.EurovisionBookingDetails.GetNewsEvents().Any(x => x.EventId.Equals(service.EurovisionBookingDetails.EventId)))
			{
				newsEvent = service.EurovisionBookingDetails.GetNewsEvents().FirstOrDefault(x => x.EventId.Equals(service.EurovisionBookingDetails.EventId));
				NewsEvent_EventChanged(newsEvent);
				return;
			}

			newsEvent = service.EurovisionBookingDetails.GetNewsEvents().FirstOrDefault();
			NewsEvent_EventChanged(newsEvent);

			service.EurovisionBookingDetails.ContractCode = service.EurovisionBookingDetails.Contracts.FirstOrDefault()?.Code;
			service.EurovisionBookingDetails.DestinationOrganizationCode = service.EurovisionBookingDetails.GetFinnishOrganizations().FirstOrDefault(x => x.Code.Equals("FIYLE", StringComparison.InvariantCultureIgnoreCase))?.Code ?? String.Empty;
			service.EurovisionBookingDetails.DestinationCityCode = service.EurovisionBookingDetails.Cities.FirstOrDefault(x => x.Code.Equals("HLKI", StringComparison.InvariantCultureIgnoreCase))?.Code ?? String.Empty;
		}

		private void InitializeProgramEvent()
		{
			// Check for program events if a program event was already defined in the booking details
			Event programEvent;
			if (!String.IsNullOrWhiteSpace(service.EurovisionBookingDetails.EventId) && service.EurovisionBookingDetails.GetProgramEvents().Any(x => x.EventId.Equals(service.EurovisionBookingDetails.EventId)))
			{
				programEvent = service.EurovisionBookingDetails.GetProgramEvents().FirstOrDefault(x => x.EventId.Equals(service.EurovisionBookingDetails.EventId));
				ProgramEvent_EventChanged(programEvent);
				return;
			}

			programEvent = service.EurovisionBookingDetails.GetProgramEvents().FirstOrDefault();
			ProgramEvent_EventChanged(programEvent);

			service.EurovisionBookingDetails.ContractCode = service.EurovisionBookingDetails.Contracts.FirstOrDefault()?.Code;
			service.EurovisionBookingDetails.DestinationOrganizationCode = service.EurovisionBookingDetails.GetFinnishOrganizations().FirstOrDefault(x => x.Code.Equals("FIYLE", StringComparison.InvariantCultureIgnoreCase))?.Code ?? String.Empty;
			service.EurovisionBookingDetails.DestinationCityCode = service.EurovisionBookingDetails.Cities.FirstOrDefault(x => x.Code.Equals("HLKI", StringComparison.InvariantCultureIgnoreCase))?.Code ?? String.Empty;
		}

		private void InitializeSatelliteCapacity()
		{
			Satellite satellite = service.EurovisionBookingDetails.Satellites.FirstOrDefault(x => x.Id.Equals(service.EurovisionBookingDetails.SatelliteId)) ?? service.EurovisionBookingDetails.Satellites.FirstOrDefault();
			SatelliteCapacity_SatelliteChanged(satellite);

			Transportable uplink = service.EurovisionBookingDetails.GetUplinks(service.Definition.VirtualPlatformServiceType).FirstOrDefault(x => x.Id.Equals(service.EurovisionBookingDetails.TransportableId)) ?? service.EurovisionBookingDetails.GetUplinks(service.Definition.VirtualPlatformServiceType).FirstOrDefault();
			SatelliteCapacity_UplinkChanged(uplink);

			var organization = service.EurovisionBookingDetails.Organizations.FirstOrDefault(x => x.Code.Equals(service.EurovisionBookingDetails.OriginOrganizationCode)) ?? service.EurovisionBookingDetails.Organizations.FirstOrDefault();
			OriginOrganizationChanged(organization);

			if (String.IsNullOrWhiteSpace(service.EurovisionBookingDetails.ContractCode))
			{
				service.EurovisionBookingDetails.ContractCode = service.EurovisionBookingDetails.Contracts.FirstOrDefault()?.Code;
			}
		}

		private void InitializeOssTransmission()
		{
			var originOrganization = service.EurovisionBookingDetails.GetOssTransmissionOrigins(service.Definition.VirtualPlatformServiceType).FirstOrDefault(x => x.Code.Equals(service.EurovisionBookingDetails.OriginOrganizationCode)) ?? service.EurovisionBookingDetails.GetOssTransmissionOrigins(service.Definition.VirtualPlatformServiceType).FirstOrDefault();
			OriginOrganizationChanged(originOrganization);

			var destinationOrganization = service.EurovisionBookingDetails.GetOssTransmissionDestinations(service.Definition.VirtualPlatformServiceType).FirstOrDefault(x => x.Code.Equals(service.EurovisionBookingDetails.DestinationOrganizationCode)) ?? service.EurovisionBookingDetails.GetOssTransmissionDestinations(service.Definition.VirtualPlatformServiceType).FirstOrDefault();
			DestinationOrganizationChanged(destinationOrganization);

			if (String.IsNullOrWhiteSpace(service.EurovisionBookingDetails.ContractCode))
			{
				service.EurovisionBookingDetails.ContractCode = service.EurovisionBookingDetails.Contracts.FirstOrDefault()?.Code;
			}
		}

		private void InitializeUnilateralTransmission()
		{
			var originOrganization = service.EurovisionBookingDetails.GetUniTransmissionOrigins(service.Definition.VirtualPlatformServiceType).FirstOrDefault(x => x.Code.Equals(service.EurovisionBookingDetails.OriginOrganizationCode)) ?? service.EurovisionBookingDetails.GetUniTransmissionOrigins(service.Definition.VirtualPlatformServiceType).FirstOrDefault();
			OriginOrganizationChanged(originOrganization);

			var destinationOrganization = service.EurovisionBookingDetails.GetUniTransmissionOrigins(service.Definition.VirtualPlatformServiceType).FirstOrDefault(x => x.Code.Equals(service.EurovisionBookingDetails.DestinationOrganizationCode)) ?? service.EurovisionBookingDetails.GetUniTransmissionOrigins(service.Definition.VirtualPlatformServiceType).FirstOrDefault();
			DestinationOrganizationChanged(destinationOrganization);

			if (String.IsNullOrWhiteSpace(service.EurovisionBookingDetails.ContractCode))
			{
				service.EurovisionBookingDetails.ContractCode = service.EurovisionBookingDetails.Contracts.FirstOrDefault()?.Code;
			}
		}

		private void NewsEvent_EventChanged(Event @event)
		{
			if (@event == null)
			{
				ClearEventValues();
				return;
			}

			@event.UpdateFeedpoints(ebuElement, helpers.Engine);

			service.EurovisionBookingDetails.EventId = @event.EventId;
			service.EurovisionBookingDetails.EventNumber = @event.EventNumber;
			service.EurovisionBookingDetails.OriginFeedpoints = @event.Feedpoints;

			var feedpoint = service.EurovisionBookingDetails.OriginFeedpoints.FirstOrDefault(x => x.Code.Equals(service.EurovisionBookingDetails.FeedpointCode)) ?? service.EurovisionBookingDetails.OriginFeedpoints.FirstOrDefault();
			OriginFeedpointChanged(feedpoint);

			service.Start = @event.StartDate;
			service.End = @event.EndDate;

			ServiceTimingChanged?.Invoke(this, EventArgs.Empty);
		}

		private void OriginFeedpointChanged(BroadcastCenter feedpoint)
		{
			if (feedpoint == null)
			{
				ClearOriginFeedpointValues();
				return;
			}

			service.EurovisionBookingDetails.FeedpointCode = feedpoint.Code;
			service.EurovisionBookingDetails.FeedpointId = feedpoint.Id;
			service.EurovisionBookingDetails.OriginCityCode = feedpoint.Code;

			// Update feedpoint
			feedpoint.Update(ebuElement, helpers.Engine);
			feedpoint.UpdateFacilities(ebuElement, helpers.Engine);

			// Update video definition values
			service.EurovisionBookingDetails.VideoDefinitions = feedpoint.SupportedVideoDefinitions.ToList();
			VideoDefinition videoDefinition = feedpoint.SupportedVideoDefinitions.FirstOrDefault(x => x.Code.Equals(service.EurovisionBookingDetails.VideoDefinitionCode)) ?? feedpoint.SupportedVideoDefinitions.FirstOrDefault();
			VideoDefinitionChanged(videoDefinition);

			// Update facilities
			service.EurovisionBookingDetails.Facilities = feedpoint.Facilities.ToList();
			Facility facility = feedpoint.Facilities.FirstOrDefault(x => x.ProductId.Equals(service.EurovisionBookingDetails.FacilityProductId)) ?? feedpoint.Facilities.FirstOrDefault();
			if (facility == null)
			{
				service.EurovisionBookingDetails.FacilityProductId = String.Empty;
				service.EurovisionBookingDetails.FacilityProductCode = String.Empty;
			}
			else
			{
				service.EurovisionBookingDetails.FacilityProductId = facility.ProductId;
				service.EurovisionBookingDetails.FacilityProductCode = facility.ProductCode;
			}

			// Update frame rates
			service.EurovisionBookingDetails.VideoFrameRates = feedpoint.SupportedVideoFrameRates.ToList();
			VideoFrameRate frameRate = feedpoint.SupportedVideoFrameRates.FirstOrDefault(x => x.Code.Equals(service.EurovisionBookingDetails.VideoFrameRateCode)) ?? feedpoint.SupportedVideoFrameRates.FirstOrDefault();
			if (frameRate == null)
			{
				service.EurovisionBookingDetails.VideoFrameRateCode = String.Empty;
			}
			else
			{
				service.EurovisionBookingDetails.VideoFrameRateCode = frameRate.Code;
			}
		}

		private void DestinationCityChanged(City city)
		{
			if (city == null)
			{
				ClearDestinationCityValues();
				return;
			}

			service.EurovisionBookingDetails.DestinationCityCode = city.Code;
		}

		private void VideoDefinitionChanged(VideoDefinition videoDefinition)
		{
			if (videoDefinition == null)
			{
				ClearVideoDefinitionValues();
			}
			else
			{
				var feedpoint = service.EurovisionBookingDetails.OriginFeedpoints.FirstOrDefault(x => x.Code.Equals(service.EurovisionBookingDetails.FeedpointCode));
				if (feedpoint == null) throw new InvalidOperationException($"{nameof(feedpoint)} cannot be null");

				service.EurovisionBookingDetails.VideoDefinitionCode = videoDefinition.Code;

				// Resolutions
				UpdateVideoResolutions(videoDefinition, feedpoint);

				// Aspect Ratios
				UpdateAspectRatios(videoDefinition, feedpoint);

				// Bitrates
				UpdateBitrates(videoDefinition, feedpoint);

				// Bandwidths
				UpdateBandwidths(videoDefinition, feedpoint);
			}
		}

		private void ProgramEvent_EventChanged(Event @event)
		{
			if (@event == null)
			{
				ClearEventValues();
				return;
			}

			@event.UpdateMultilateralTransmissions(ebuElement, helpers.Engine);

			service.EurovisionBookingDetails.EventId = @event.EventId;
			service.EurovisionBookingDetails.EventNumber = @event.EventNumber;
			service.EurovisionBookingDetails.Participations = @event.MultilateralTransmissions;

			var participation = service.EurovisionBookingDetails.Participations.FirstOrDefault(x => x.Id.Equals(service.EurovisionBookingDetails.MultilateralTransmissionId)) ?? service.EurovisionBookingDetails.Participations.FirstOrDefault();
			ProgramEvent_ParticipationChanged(participation);
		}

		private void ProgramEvent_ParticipationChanged(MultilateralTransmission participation)
		{
			if (participation == null)
			{
				ClearParticipationValues();
				return;
			}

			service.Start = participation.BeginDate;
			service.End = participation.EndDate;

			ServiceTimingChanged?.Invoke(this, EventArgs.Empty);

			service.EurovisionBookingDetails.MultilateralTransmissionId = participation.Id;
			service.EurovisionBookingDetails.MultilateralTransmissionNumber = participation.TransmissionNumber;
		}

		private void SatelliteCapacity_SatelliteChanged(Satellite satellite)
		{
			if (satellite == null)
			{
				service.EurovisionBookingDetails.SatelliteId = String.Empty;
				return;
			}

			service.EurovisionBookingDetails.SatelliteId = satellite.Id;
		}

		private void SatelliteCapacity_UplinkChanged(Transportable uplink)
		{
			if (uplink == null)
			{
				service.EurovisionBookingDetails.TransportableId = String.Empty;
				return;
			}

			service.EurovisionBookingDetails.TransportableId = uplink.Id;
		}

		private void OriginOrganizationChanged(Organization origin)
		{
			if (origin == null)
			{
				ClearOriginOrganizationValues();
				return;
			}

			origin.UpdateBroadcastCenters(ebuElement, helpers.Engine);

			service.EurovisionBookingDetails.OriginOrganizationCode = origin.Code;
			service.EurovisionBookingDetails.OriginFeedpoints = origin.BroadcastCenters;

			var feedpoint = service.EurovisionBookingDetails.OriginFeedpoints.FirstOrDefault(x => x.Code.Equals(service.EurovisionBookingDetails.FeedpointCode)) ?? service.EurovisionBookingDetails.OriginFeedpoints.FirstOrDefault();
			OriginFeedpointChanged(feedpoint);
		}

		private void DestinationOrganizationChanged(Organization destination)
		{
			if (destination == null)
			{
				ClearDestinationOrganizationValues();
				return;
			}

			destination.UpdateBroadcastCenters(ebuElement, helpers.Engine);

			service.EurovisionBookingDetails.DestinationOrganizationCode = destination.Code;

			service.EurovisionBookingDetails.DestinationFeedpoints = destination.BroadcastCenters;

			var destinationFeedpoint = service.EurovisionBookingDetails.DestinationFeedpoints.FirstOrDefault(x => x?.City?.Code == service.EurovisionBookingDetails.DestinationCityCode) ?? service.EurovisionBookingDetails.DestinationFeedpoints.FirstOrDefault(x => !String.IsNullOrWhiteSpace(x?.City?.Name)) ?? throw new NotFoundException("Unable to find destination feedpoint");
			DestinationCityChanged(destinationFeedpoint.City);
		}

		private void UpdateVideoResolutions(VideoDefinition videoDefinition, BroadcastCenter feedpoint)
		{
			if (videoDefinition == null) throw new ArgumentNullException(nameof(videoDefinition));
			if (feedpoint == null) throw new ArgumentNullException(nameof(feedpoint));

			if (feedpoint.SupportedVideoDefinitionResolutions.TryGetValue(videoDefinition, out List<Eurovision.VideoResolution> resolutions) && resolutions != null && resolutions.Any())
			{
				service.EurovisionBookingDetails.VideoResolutions = resolutions;
				if (!resolutions.Any(x => x.Code.Equals(service.EurovisionBookingDetails.VideoResolutionCode))) service.EurovisionBookingDetails.VideoResolutionCode = resolutions[0].Code;
			}
			else
			{
				service.EurovisionBookingDetails.VideoResolutions = new List<Eurovision.VideoResolution>();
				service.EurovisionBookingDetails.VideoResolutionCode = String.Empty;
			}
		}

		private void UpdateAspectRatios(VideoDefinition videoDefinition, BroadcastCenter feedpoint)
		{
			if (feedpoint.SupportedVideoDefinitionAspectRatios.TryGetValue(videoDefinition, out List<VideoAspectRatio> aspectRatios) && aspectRatios != null && aspectRatios.Any())
			{
				service.EurovisionBookingDetails.VideoAspectRatios = aspectRatios;
				if (!aspectRatios.Any(x => x.Code.Equals(service.EurovisionBookingDetails.VideoAspectRatioCode))) service.EurovisionBookingDetails.VideoAspectRatioCode = aspectRatios[0].Code;
			}
			else
			{
				service.EurovisionBookingDetails.VideoAspectRatios = new List<VideoAspectRatio>();
				service.EurovisionBookingDetails.VideoAspectRatioCode = String.Empty;
			}
		}

		private void UpdateBitrates(VideoDefinition videoDefinition, BroadcastCenter feedpoint)
		{
			if (feedpoint.SupportedVideoDefinitionBitrates.TryGetValue(videoDefinition, out List<VideoBitrate> bitrates) && bitrates != null && bitrates.Any())
			{
				service.EurovisionBookingDetails.VideoBitrates = bitrates;
				if (!bitrates.Any(x => x.Code.Equals(service.EurovisionBookingDetails.VideoBitrateCode))) service.EurovisionBookingDetails.VideoBitrateCode = bitrates[0].Code;
			}
			else
			{
				service.EurovisionBookingDetails.VideoBitrates = new List<VideoBitrate>();
				service.EurovisionBookingDetails.VideoBitrateCode = String.Empty;
			}
		}

		private void UpdateBandwidths(VideoDefinition videoDefinition, BroadcastCenter feedpoint)
		{
			if (feedpoint.SupportedVideoDefinitionBandwidths.TryGetValue(videoDefinition, out List<VideoBandwidth> bandwidths) && bandwidths != null && bandwidths.Any())
			{
				service.EurovisionBookingDetails.VideoBandWidths = bandwidths;
				if (!bandwidths.Any(x => x.Code.Equals(service.EurovisionBookingDetails.VideoBandwidthCode))) service.EurovisionBookingDetails.VideoBandwidthCode = bandwidths[0].Code;
			}
			else
			{
				service.EurovisionBookingDetails.VideoBandWidths = new List<VideoBandwidth>();
				service.EurovisionBookingDetails.VideoBandwidthCode = String.Empty;
			}
		}

		private void ClearEventValues()
		{
			service.EurovisionBookingDetails.EventId = String.Empty;
			service.EurovisionBookingDetails.EventNumber = String.Empty;

			service.EurovisionBookingDetails.OriginFeedpoints = new List<BroadcastCenter>();
			service.EurovisionBookingDetails.Participations = new List<MultilateralTransmission>();

			ClearOriginFeedpointValues();
			ClearParticipationValues();
		}

		private void ClearOriginFeedpointValues()
		{
			service.EurovisionBookingDetails.FeedpointCode = String.Empty;
			service.EurovisionBookingDetails.FeedpointId = String.Empty;
			service.EurovisionBookingDetails.OriginCityCode = String.Empty;

			service.EurovisionBookingDetails.VideoDefinitions = new List<VideoDefinition>();
			service.EurovisionBookingDetails.VideoFrameRates = new List<VideoFrameRate>();
			service.EurovisionBookingDetails.Facilities = new List<Facility>();

			service.EurovisionBookingDetails.VideoFrameRateCode = String.Empty;
			service.EurovisionBookingDetails.FacilityProductId = String.Empty;
			service.EurovisionBookingDetails.FacilityProductCode = String.Empty;

			ClearVideoDefinitionValues();
		}

		private void ClearDestinationCityValues()
		{
			service.EurovisionBookingDetails.DestinationCityCode = String.Empty;
		}

		private void ClearVideoDefinitionValues()
		{
			service.EurovisionBookingDetails.VideoDefinitionCode = String.Empty;

			service.EurovisionBookingDetails.VideoResolutions = new List<Eurovision.VideoResolution>();
			service.EurovisionBookingDetails.VideoAspectRatios = new List<VideoAspectRatio>();
			service.EurovisionBookingDetails.VideoBitrates = new List<VideoBitrate>();
			service.EurovisionBookingDetails.VideoBandWidths = new List<VideoBandwidth>();

			service.EurovisionBookingDetails.VideoResolutionCode = String.Empty;
			service.EurovisionBookingDetails.VideoAspectRatioCode = String.Empty;
			service.EurovisionBookingDetails.VideoBitrateCode = String.Empty;
			service.EurovisionBookingDetails.VideoBandwidthCode = String.Empty;
		}

		private void ClearParticipationValues()
		{
			service.EurovisionBookingDetails.MultilateralTransmissionId = String.Empty;
			service.EurovisionBookingDetails.MultilateralTransmissionNumber = String.Empty;
		}

		private void ClearOriginOrganizationValues()
		{
			service.EurovisionBookingDetails.OriginOrganizationCode = String.Empty;
			service.EurovisionBookingDetails.OriginFeedpoints = new List<BroadcastCenter>();

			ClearOriginFeedpointValues();
		}

		private void ClearDestinationOrganizationValues()
		{
			service.EurovisionBookingDetails.DestinationOrganizationCode = String.Empty;
			service.EurovisionBookingDetails.DestinationFeedpoints = new List<BroadcastCenter>();

			ClearDestinationCityValues();
		}
	}
}
