namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision
{
	using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;

    public class EurovisionBookingDetails
	{
		private Type type;

		public EurovisionBookingDetails()
        {
			Type = Type.None;
        }

		public Type Type
        {
			get { return type; }
			set
            {
				type = value;
				TypeChanged?.Invoke(this, Type);
			}
        }

		public event EventHandler<Type> TypeChanged;

		private string eventId;

		public event EventHandler<string> EventIdChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string EventId
        {
			get => eventId;
			set
            {
				eventId = value;
				EventIdChanged?.Invoke(this, value);
			}
        }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string EventNumber { get; set; }

		private string multilateralTransmissionId;

		public event EventHandler<string> MultilateralTransmissionIdChanged;

		/// <summary>
		/// Gets or sets the id of the participation.
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string MultilateralTransmissionId
        {
			get => multilateralTransmissionId;
			set
            {
				multilateralTransmissionId = value;
				MultilateralTransmissionIdChanged?.Invoke(this, value);
            }
        }

		/// <summary>
		/// Gets or sets the transmission number of the participation.
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string MultilateralTransmissionNumber { get; set; }

		private string satelliteId;

		public event EventHandler<string> SatelliteIdChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string SatelliteId
        {
			get => satelliteId;
			set
            {
				satelliteId = value;
				SatelliteIdChanged?.Invoke(this, value);
            }
        }

		private string transportableId;

		public event EventHandler<string> TransportableIdChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string TransportableId
        {
			get => transportableId;
			set
            {
				transportableId = value;
				TransportableIdChanged?.Invoke(this, value);
            }
        }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? Start { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? End { get; set; }

		private string destinationOrganizationCode;

		public event EventHandler<string> DestinationOrganizationCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string DestinationOrganizationCode
        {
			get => destinationOrganizationCode;
			set
            {
				destinationOrganizationCode = value;
				DestinationOrganizationCodeChanged?.Invoke(this, value);
            }
        }

		private string destinationCityCode;

		public event EventHandler<string> DestinationCityCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string DestinationCityCode
        {
			get => destinationCityCode;
			set
            {
				destinationCityCode = value;
				DestinationCityCodeChanged?.Invoke(this, value);
            }
        }

		private string originOrganizatinCode;

		public event EventHandler<string> OriginOrganizationCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string OriginOrganizationCode
        {
			get => originOrganizatinCode;
			set
            {
				originOrganizatinCode = value;
				OriginOrganizationCodeChanged?.Invoke(this, value);
            }
        }

		private string originCityCode;

		public event EventHandler<string> OriginCityCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string OriginCityCode
        {
			get => originCityCode;
			set
            {
				originCityCode = value;
				OriginCityCodeChanged?.Invoke(this, value);
            }
        }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ContactFirstName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ContactLastName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Contact { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Description { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Note { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string LineUp { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Phone { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Email { get; set; }

		private string contractCode;

		public event EventHandler<string> ContractCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ContractCode
        {
			get => contractCode;
			set
            {
				contractCode = value;
				ContractCodeChanged?.Invoke(this, value);
            }
        }

		private string feedpointCode;

		public event EventHandler<string> FeedpointCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string FeedpointCode
        {
			get => feedpointCode;
			set
            {
				feedpointCode = value;
				FeedpointCodeChanged?.Invoke(this, value);
            }
        }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string FeedpointId { get; set; }

		private string facilityProductId;

		public event EventHandler<string> FacilityProductIdChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string FacilityProductId
        {
			get => facilityProductId;
			set
            {
				facilityProductId = value;
				FacilityProductIdChanged?.Invoke(this, value);
            }
        }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string FacilityProductCode { get; set; }

		private string videoDefinitionCode;

		public event EventHandler<string> VideoDefinitionCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string VideoDefinitionCode
        {
			get => videoDefinitionCode;
			set
            {
				videoDefinitionCode = value;
				VideoDefinitionCodeChanged?.Invoke(this, value);
            }
        }

		private string videoResolutionCode;

		public event EventHandler<string> VideoResolutionCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string VideoResolutionCode
        {
			get => videoResolutionCode;
			set
            {
				videoResolutionCode = value;
				VideoResolutionCodeChanged?.Invoke(this, value);
			}
		}

		private string videoAspectRatioCode;

		public event EventHandler<string> VideoAspectRatioCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string VideoAspectRatioCode
        {
			get => videoAspectRatioCode;
			set
            {
				videoAspectRatioCode = value;
				VideoAspectRatioCodeChanged?.Invoke(this, value);
			}
        }

		private string videoBitrateCode;

		public event EventHandler<string> VideoBitrateCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string VideoBitrateCode
        {
			get => videoBitrateCode;
			set
            {
				videoBitrateCode = value;
				VideoBitrateCodeChanged?.Invoke(this, value);
            }
        }

		private string videoFrameRateCode;

		public event EventHandler<string> VideoFrameRateCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string VideoFrameRateCode
        {
			get => videoFrameRateCode;
			set
            {
				videoFrameRateCode = value;
				VideoFrameRateCodeChanged?.Invoke(this, value);
            }
        }

		private string videoBandwidthCode;

		public event EventHandler<string> VideoBandwidthCodeChanged;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string VideoBandwidthCode
        {
			get => videoBandwidthCode;
			set
            {
				videoBandwidthCode = value;
				VideoBandwidthCodeChanged?.Invoke(this, value);
            }
        }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AudioChannel AudioChannel1 { get; set; } = new AudioChannel();

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AudioChannel AudioChannel2 { get; set; } = new AudioChannel();

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AudioChannel AudioChannel3 { get; set; } = new AudioChannel();

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AudioChannel AudioChannel4 { get; set; } = new AudioChannel();

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AudioChannel AudioChannel5 { get; set; } = new AudioChannel();

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AudioChannel AudioChannel6 { get; set; } = new AudioChannel();

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AudioChannel AudioChannel7 { get; set; } = new AudioChannel();

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AudioChannel AudioChannel8 { get; set; } = new AudioChannel();

		private IReadOnlyList<Event> events = new List<Event>();

		public event EventHandler<IReadOnlyList<Event>> EventsChanged;

		// General UI collections
		[JsonIgnore]
		public IReadOnlyList<Event> Events
		{
			get => events;
			set
            {
				events = value ?? throw new ArgumentNullException(nameof(value)); ;
				EventsChanged?.Invoke(this, value);
			}
		}

		private IReadOnlyList<MultilateralTransmission> participations = new List<MultilateralTransmission>();

		public event EventHandler<IReadOnlyList<MultilateralTransmission>> ParticipationsChanged;

		[JsonIgnore]
		public IReadOnlyList<MultilateralTransmission> Participations
		{
			get => participations;
			set
            {
				participations = value ?? throw new ArgumentNullException(nameof(value));
				ParticipationsChanged?.Invoke(this, value);
            }
		}

		private IReadOnlyList<Organization> organizations = new List<Organization>();

		public event EventHandler<IReadOnlyList<Organization>> OrganizationsChanged;

		[JsonIgnore]
		public IReadOnlyList<Organization> Organizations
		{
			get => organizations;
			set
            {
				organizations = value ?? throw new ArgumentNullException(nameof(value));
				OrganizationsChanged?.Invoke(this, value);

			}
		}

		private IReadOnlyList<BroadcastCenter> originFeedpoints = new List<BroadcastCenter>();

		public event EventHandler<IReadOnlyList<BroadcastCenter>> OriginFeedpointsChanged;

		[JsonIgnore]
		public IReadOnlyList<BroadcastCenter> OriginFeedpoints
		{
			get => originFeedpoints;
			set
			{
				originFeedpoints = value ?? throw new ArgumentNullException(nameof(value));
				OriginFeedpointsChanged?.Invoke(this, value);
			}
		}

		private IReadOnlyList<BroadcastCenter> destinationFeedpoints = new List<BroadcastCenter>();

		public event EventHandler<IReadOnlyList<BroadcastCenter>> DestinationFeedpointsChanged;

		[JsonIgnore]
		public IReadOnlyList<BroadcastCenter> DestinationFeedpoints
		{
			get => destinationFeedpoints;
			set
			{
				destinationFeedpoints = value ?? throw new ArgumentNullException(nameof(value));
				DestinationFeedpointsChanged?.Invoke(this, value);
			}
		}

		private IReadOnlyList<Facility> facilities = new List<Facility>();

		public event EventHandler<IReadOnlyList<Facility>> FacilitiesChanged;

		[JsonIgnore]
		public IReadOnlyList<Facility> Facilities
		{
			get => facilities;
			set
			{
				facilities = value ?? throw new ArgumentNullException(nameof(value));
				FacilitiesChanged?.Invoke(this, value);

			}
		}

		private IReadOnlyList<City> cities = new List<City>();

		public event EventHandler<IReadOnlyList<City>> CitiesChanged;

		[JsonIgnore]
		public IReadOnlyList<City> Cities
        {
			get => cities;
			set
            {
				cities = value ?? throw new ArgumentNullException(nameof(value)); ;
				CitiesChanged?.Invoke(this, value);
            }
        }

		private IReadOnlyList<Audio> audios = new List<Audio>();

		public event EventHandler<IReadOnlyList<Audio>> AudiosChanged;

		[JsonIgnore]
		public IReadOnlyList<Audio> Audios
        {
			get => audios;
			set
            {
				audios = value ?? throw new ArgumentNullException(nameof(value)); ;
				AudiosChanged?.Invoke(this, value);
            }
        }

		private IReadOnlyList<Contract> contracts = new List<Contract>();

		public event EventHandler<IReadOnlyList<Contract>> ContractsChanged;

		[JsonIgnore]
		public IReadOnlyList<Contract> Contracts
        {
			get => contracts;
			set
            {
				contracts = value ?? throw new ArgumentNullException(nameof(value)); ;
				ContractsChanged?.Invoke(this, value);
            }
        }

		private IReadOnlyList<VideoDefinition> videoDefinitions = new List<VideoDefinition>();

		public event EventHandler<IReadOnlyList<VideoDefinition>> VideoDefinitionsChanged;

		[JsonIgnore]
		public IReadOnlyList<VideoDefinition> VideoDefinitions
        {
			get => videoDefinitions;
			set
            {
				videoDefinitions = value ?? throw new ArgumentNullException(nameof(value));
				VideoDefinitionsChanged?.Invoke(this, value);
			}
        }

		private IReadOnlyList<VideoAspectRatio> videoAspectRatios = new List<VideoAspectRatio>();

		public event EventHandler<IReadOnlyList<VideoAspectRatio>> VideoAspectRatiosChanged;

		[JsonIgnore]
		public IReadOnlyList<VideoAspectRatio> VideoAspectRatios
		{
			get => videoAspectRatios;
			set
			{
				videoAspectRatios = value ?? throw new ArgumentNullException(nameof(value));
				VideoAspectRatiosChanged?.Invoke(this, value);
			}
		}

		private IReadOnlyList<VideoResolution> videoResolutions = new List<VideoResolution>();

		public event EventHandler<IReadOnlyList<VideoResolution>> VideoResolutionsChanged;

		[JsonIgnore]
		public IReadOnlyList<VideoResolution> VideoResolutions
		{
			get => videoResolutions;
			set
			{
				videoResolutions = value ?? throw new ArgumentNullException(nameof(value));
				VideoResolutionsChanged?.Invoke(this, value);
			}
		}

		private IReadOnlyList<VideoBitrate> videoBitrates = new List<VideoBitrate>();

		public event EventHandler<IReadOnlyList<VideoBitrate>> VideoBitratesChanged;

		[JsonIgnore]
		public IReadOnlyList<VideoBitrate> VideoBitrates
		{
			get => videoBitrates;
			set
			{
				videoBitrates = value ?? throw new ArgumentNullException(nameof(value));
				VideoBitratesChanged?.Invoke(this, value);
			}
		}

		private IReadOnlyList<VideoFrameRate> videoFrameRates = new List<VideoFrameRate>();

		public event EventHandler<IReadOnlyList<VideoFrameRate>> VideoFrameRatesChanged;

		[JsonIgnore]
		public IReadOnlyList<VideoFrameRate> VideoFrameRates
		{
			get => videoFrameRates;
			set
			{
				videoFrameRates = value ?? throw new ArgumentNullException(nameof(value));
				VideoFrameRatesChanged?.Invoke(this, value);
			}
		}

		private IReadOnlyList<VideoBandwidth> videoBandwidths = new List<VideoBandwidth>();

		public event EventHandler<IReadOnlyList<VideoBandwidth>> VideoBandWidthsChanged;

		[JsonIgnore]
		public IReadOnlyList<VideoBandwidth> VideoBandWidths
		{
			get => videoBandwidths;
			set
			{
				videoBandwidths = value ?? throw new ArgumentNullException(nameof(value));
				VideoBandWidthsChanged?.Invoke(this, value);
			}
		}

		// News Event UI collections
		//private Dictionary<string, Event> newsEvents; // EventType == "NEWS"; default = None
		//private List<Organization> finnishOrganizations; //Code.StartsWith("FI"); default = FIYLE
		//private List<City> cities; // All cities; default = HLKI
		//private List<Audio> audios; // All Audios; default = M
		//private List<Contract> contracts; // All contracts; default = None

		// newsEvents; // EventType == "NEWS"; default = None
		public IReadOnlyList<Event> GetNewsEvents()
		{
			return Events.Where(e => e.EventType == "NEWS").ToList();
		}

		// finnishOrganizations; //Code.StartsWith("FI"); default = FIYLE
		public IReadOnlyList<Organization> GetFinnishOrganizations()
		{
			return Organizations.Where(o => o.Code.StartsWith("FI", StringComparison.CurrentCultureIgnoreCase)).ToList();
		}

		// UNI Transmission UI Collections
		//private List<Organization> origins; // allOrganizations.Where(o => o != null && o.SupportsUNI); if source: !o.Code.StartsWith("FI") else o.Code.StartsWith("FI")
		//private List<Organization> destinations; // allOrganizations.Where(o => o != null && o.SupportsUNI); if source: o.Code.StartsWith("FI") else !o.Code.StartsWith("FI")
		//private List<Contract> contracts; // All contracts; default = None
		//private List<Audio> audios; // All Audios; default = M

		public IReadOnlyList<Organization> GetUniTransmissionOrigins(VirtualPlatformType virtualPlatformType)
        {
			switch (virtualPlatformType)
            {
				case VirtualPlatformType.Reception:
					return Organizations.Where(o => o != null && o.SupportsUNI && !o.Code.StartsWith("FI")).ToList();
				case VirtualPlatformType.Transmission:
					return Organizations.Where(o => o != null && o.SupportsUNI && o.Code.StartsWith("FI")).ToList();
				default:
					throw new NotSupportedException();
			}
        }

		public IReadOnlyList<Organization> GetUniTransmissionDestinations(VirtualPlatformType virtualPlatformType)
		{
			switch (virtualPlatformType)
			{
				case VirtualPlatformType.Reception:
					return Organizations.Where(o => o != null && o.SupportsUNI && o.Code.StartsWith("FI")).ToList();
				case VirtualPlatformType.Transmission:
					return Organizations.Where(o => o != null && o.SupportsUNI && !o.Code.StartsWith("FI")).ToList();
				default:
					throw new NotSupportedException();
			}
		}

		// OSS Transmission UI Collections
		//private List<Organization> origins; // origins = allOrganizations.Where(o => o != null && o.SupportsOSS); if source: !o.Code.StartsWith("FI") else o.Code.StartsWith("FI")
		//private List<Organization> destinations; // origins = allOrganizations.Where(o => o != null && o.SupportsOSS); if source: o.Code.StartsWith("FI") else !o.Code.StartsWith("FI")
		//private List<Contract> contracts; // All contracts; default = None
		//private List<Audio> audios; // All Audios; default = M

		public IReadOnlyList<Organization> GetOssTransmissionOrigins(VirtualPlatformType virtualPlatformType)
		{
			switch (virtualPlatformType)
			{
				case VirtualPlatformType.Reception:
					return Organizations.Where(o => o != null && o.SupportsOSS && !o.Code.StartsWith("FI")).ToList();
				case VirtualPlatformType.Transmission:
					return Organizations.Where(o => o != null && o.SupportsOSS && o.Code.StartsWith("FI")).ToList();
				default:
					throw new NotSupportedException();
			}
		}

		public IReadOnlyList<Organization> GetOssTransmissionDestinations(VirtualPlatformType virtualPlatformType)
		{
			switch (virtualPlatformType)
			{
				case VirtualPlatformType.Reception:
					return Organizations.Where(o => o != null && o.SupportsOSS && o.Code.StartsWith("FI")).ToList();
				case VirtualPlatformType.Transmission:
					return Organizations.Where(o => o != null && o.SupportsOSS && !o.Code.StartsWith("FI")).ToList();
				default:
					throw new NotSupportedException();
			}
		}

		// Satellite Capacity UI Collections
		//private List<Satellite> satellites; // All satellites; default = "None"
		//private List<Transportable> uplinks; // if isSource: transportables.Where(u => !u.Name.StartsWith("FIN")) else transportables.Where(u => u.Name.StartsWith("FIN"))
		//private List<Organization> organizations; // All organizations
		//private List<Contract> contracts; // All contracts; default = None
		//private List<Audio> audios; // All Audios; default = M

		private IReadOnlyList<Satellite> satellites = new List<Satellite>();

		public event EventHandler<IReadOnlyList<Satellite>> SatellitesChanged;

		[JsonIgnore]
		public IReadOnlyList<Satellite> Satellites
        {
			get => satellites;
			set
            {
				satellites = value ?? throw new ArgumentNullException(nameof(value));
				SatellitesChanged?.Invoke(this, value);
            }
        }

		private IReadOnlyList<Transportable> transportables = new List<Transportable>();

		public event EventHandler<IReadOnlyList<Transportable>> TransportablesChanged;

		[JsonIgnore]
		public IReadOnlyList<Transportable> Transportables
        {
			get => transportables;
			set
            {
				transportables = value ?? throw new ArgumentNullException(nameof(value));
				TransportablesChanged?.Invoke(this, value);
            }
        }

		public IReadOnlyList<Transportable> GetUplinks(VirtualPlatformType virtualPlatformType)
        {
			switch (virtualPlatformType)
			{
				case VirtualPlatformType.Reception:
					return Transportables.Where(u => u?.Name != null && !u.Name.StartsWith("FIN")).ToList();
				case VirtualPlatformType.Transmission:
					return Transportables.Where(u => u?.Name != null && u.Name.StartsWith("FIN")).ToList();
				default:
					throw new NotSupportedException();
			}
		}

		// Program Event UI Collections
		//private Dictionary<string, Event> programEvents; // EventType == "PROG"; default = None
		//private List<Organization> organizations; // All organizations; default = FIYLE
		//private List<City> cities; // All cities; default = HLKI
		//private List<Contract> contracts; // All contracts; default = None

		public IReadOnlyList<Event> GetProgramEvents()
		{
			return Events.Where(e => e.EventType == "PROG").ToList();
		}
	}
}