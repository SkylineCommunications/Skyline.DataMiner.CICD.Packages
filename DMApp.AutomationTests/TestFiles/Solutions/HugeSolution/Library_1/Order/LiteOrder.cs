namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using Library_1.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Reservation;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public class LiteOrder : DisplayedOrder, ICloneable
	{
		public static readonly char[] OrderNameDisallowedCharacters = new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

		public const string PropertyNameEventId = "EventId";
		public const string PropertyNameType = "Type";
		public const string PropertyNameIntegration = "Integration";
		public const string PropertyNameStatus = "Status";
		public const string PropertyNameUsergroups = "UserGroups";
		public const string PropertyNameInternal = "Internal";
		public const string PropertyNamePlasmaId = "PlasmaId";
		public const string PropertyNameEditorialObjectId = "EditorialObjectId";
		public const string PropertyNameEurovisionId = "EurovisionId";
		public const string PropertyNameEurovisionTransmissionNumber = "EurovisionTransmissionNumber";
		public const string PropertyNameCreatedBy = "CreatedBy";
		public const string PropertyNameCreatedByEmail = "CreatedByEmail";
		public const string PropertyNameCreatedByPhone = "CreatedByPhone";
		public const string PropertyNameLastUpdatedBy = "Last Updated By";
		public const string PropertyNameLastUpdatedByEmail = "LastUpdatedByEmail";
		public const string PropertyNameLastUpdatedByPhone = "LastUpdatedByPhone";
		public const string PropertyNameMediaOperatorNotes = "OperatorNotes";
		public const string PropertyNameMcrOperatorNotes = "MCROperatorNotes";
		public const string PropertyNameErrorDescription = "ErrorDescription";
		public const string PropertyNameReasonForCancellationOrRejection = "ReasonForCancellationOrRejection";
		public const string PropertyNameComments = "Comments";
		public const string PropertyNameShortDescription = "Short Description";
		public const string PropertyNameRecurrence = "Recurrence";
		public const string PropertyNameFromTemplate = "FromTemplate";
		public const string PropertyNameBillingInfo = "BillingInfo";
		public const string PropertyNameCustomer = "Customer";
		public const string PropertyNameAttachmentCount = "AttachmentCount";

		public const string PropertyNameUiTeamMessiLive = "UI_Team_Messi_Live";
		public const string PropertyNameUiTeamMessiNews = "UI_Team_Messi_News";
		public const string PropertyNameUiAreena = "UI_Areena";
		public const string PropertyNameUiPlasma = "UI_Plasma";
		public const string PropertyNameUiMessiNewsRec = "UI_Messi_News_Rec";
		public const string PropertyNameUiTomTre = "UI_TRE";
		public const string PropertyNameUiTomSho = "UI_SHO";
		public const string PropertyNameUiTomNews = "UI_News";
		public const string PropertyNameUiTomSvenska = "UI_Svenska";
		public const string PropertyNameUiMcrChange = "UI_MCR_Change";
		public const string PropertyNameUiRecording = "UI_Recording";
		public const string PropertyNameUiNewsArea = "News Area";
		public const string PropertyNameUiOffice = "UI_Office";
		public const string PropertyNameUiMcrSpecialist = "UI_Fiber";
		public const string PropertyNameUiStudioHelsinkiUT = "UI_UT";
		public const string PropertyNameIsLocked = "IsLocked";
		public const string PropertyNameIsPending = "IsPending";
		public const string PropertyNameHiddenMessiNews = "HiddenMessiNews";
		public const string PropertyNameHiddenMessiLive = "HiddenMessiLive";
		public const string PropertyNameDestinationOrder = "DestinationOrder";
		public const string PropertyNameEventName = "EventName";

		public const string PropertyNameSportsplanningSport = "SportsPlanning_Sport";
		public const string PropertyNameSportsplanningDescr = "SportsPlanning_Description";
		public const string PropertyNameSportsplanningCommentary = "SportsPlanning_Commentary";
		public const string PropertyNameSportsplanningCommentary2 = "SportsPlanning_Commentary2";
		public const string PropertyNameSportsplanningCompetitionTime = "SportsPlanning_CompetitionTime";
		public const string PropertyNameSportsplanningJournalist1 = "SportsPlanning_JournalistOne";
		public const string PropertyNameSportsplanningJournalist2 = "SportsPlanning_JournalistTwo";
		public const string PropertyNameSportsplanningJournalist3 = "SportsPlanning_JournalistThree";
		public const string PropertyNameSportsplanningLocation = "SportsPlanning_Location";
		public const string PropertyNameSportsplanningTechResources = "SportsPlanning_TechnicalResources";
		public const string PropertyNameSportsplanningLivehighlights = "SportsPlanning_LiveHighlightsFile";
		public const string PropertyNameSportsplanningReqBroadcastTime = "SportsPlanning_RequestedBroadcastTime";
		public const string PropertyNameSportsplanningProdNrPlasmaId = "SportsPlanning_ProductionNumberPlasmaId";
		public const string PropertyNameSportsplanningProdNrCeiton = "SportsPlanning_ProductNumberCeiton";
		public const string PropertyNameSportsplanningCostDep = "SportsPlanning_CostDepartment";
		public const string PropertyNameSportsplanningAdditionalInformation = "SportsPlanning_AdditionalInformation";

		public const string PropertyNameNewsInformationNewsCameraOperator = "NewsInformation_NewsCameraOperator";
		public const string PropertyNameNewsInformationJournalist = "NewsInformation_Journalist";
		public const string PropertyNameNewsInformationVirveCommandGroupOne = "NewsInformation_VirveCommandGroupOne";
		public const string PropertyNameNewsInformationVirveCommandGroupTwo = "NewsInformation_VirveCommandGroupTwo";
		public const string PropertyNameNewsInformationAdditionalInformation = "NewsInformation_AdditionalInformation";

		public const string PropertyNameAudioReturnInfo = "AudioReturnInfo";
		public const string PropertyNameLiveUDeviceNames = "LiveUDeviceNames";
		public const string PropertyNameVidigoStreamSourceLinks = "VidigoStreamSourceLinks";
		public const string PropertyNamePlasmaIdsForArchiving = "PlasmaIdsForArchiving";
		public const string PropertyNameServiceConfigurations = "ServiceConfigurations";
		public const string PropertyNameStartnow = "StartNow";
		public const string PropertyNameConvertedFromRunningStartnow = "ConvertedFromRunningToStartNow";
		public const string PropertyNamePreviousRunningOrderId = "PreviousRunningOrderId";
		public const string PropertyNameFixedSourcePlasma = "Fixed_Source_Plasma";
		public const string PropertyNameYleId = "YleId";
		public const string PropertyNameSources = "Sources";
		public const string PropertyNameDestinations = "Destinations";
		public const string PropertyNameRecordings = "Recordings";
		public const string PropertyNameTransmissions = "Transmissions";
		public const string PropertyNameMcrLateChange = "MCR Late Change";
		public const string PropertyNameMainSourceType = "MainSourceType";
		public const string PropertyNameTOMViewDescription = "TOMViewDescription";
		public const string PropertyNameEngine = "Engine";
		public const string PropertyNameVizremType = "VizremType";
		public const string PropertyNamePublicationStart = "PublicationStart";
		public const string PropertyNamePublicationEnd = "PublicationEnd";


		private DateTime start;
		private DateTime end;
		private string comments;
		private string mcrOperatorNotes;
		private string mediaOperatorNotes;
		private string errorDescription;
		private string reasonForBeingCancelledOrRejected;
		private string namePostFix;
		private bool useGraphicsEngineAsInput;
		protected readonly Dictionary<string, object> initialPropertyValues = new Dictionary<string, object>();

		public LiteOrder()
		{

		}

		private LiteOrder(LiteOrder other)
		{
			CloneHelper.CloneProperties(other, this);
		}

		/// <summary>
		/// Reservation object for the order. Saved as property for performance reasons.
		/// </summary>
		public ReservationInstance Reservation { get; set; }

		/// <summary>
		/// Reservation instance ID of the order.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The event this order belongs to.
		/// </summary>
		public Event.Event Event { get; set; }

		/// <summary>
		/// Name of the order.
		/// </summary>
		[ChangeTracked]
		public string Name => ManualName + NamePostFix;

		public string ManualName { get; set; }

		public string NamePostFix
		{
			get => namePostFix;
			set
			{
				namePostFix = value;
				NamePostFixChanged?.Invoke(this, namePostFix);
			}
		}

		public event EventHandler<string> NamePostFixChanged;

		/// <summary>
		/// Used by UI.
		/// </summary>
		[ChangeTracked]
		public string DisplayName => Name;

		/// <summary>
		/// Start date and time of the order.
		/// </summary>
		[ChangeTracked]
		public DateTime Start
		{
			get => start;

			set
			{
				if (start != value)
				{
					start = value;
					StartChanged?.Invoke(this, start);
				}
			}
		}

		internal event EventHandler<DateTime> StartChanged;

		/// <summary>
		/// End date and time of the order.
		/// </summary>
		[ChangeTracked]
		public DateTime End
		{
			get => end;

			set
			{
				if (end != value)
				{
					end = value;
					EndChanged?.Invoke(this, end);
				}
			}
		}

		internal event EventHandler<DateTime> EndChanged;

		[ChangeTracked]
		public RecurringSequenceInfo RecurringSequenceInfo { get; set; } = new RecurringSequenceInfo();

		/// <summary>
		/// The status of this order.
		/// </summary>
		[ChangeTracked]
		public Status Status { get; set; } = Status.Preliminary;

		internal event EventHandler<string> CommentsChanged;

		/// <summary>
		/// Comments for the order.
		/// </summary>
		[ChangeTracked]
		public string Comments
		{
			get => comments;
			set
			{
				comments = value;
				CommentsChanged?.Invoke(this, comments);
			}
		}

		/// <summary>
		/// The type of this order.
		/// </summary>
		public OrderType Type { get; set; }

		/// <summary>
		/// The subtype of this order.
		/// </summary>
		public OrderSubType Subtype { get; set; }

		/// <summary>
		/// Indicates which integration created this Order.
		/// If not specified this will be set to None.
		/// </summary>
		[ChangeTracked]
		public IntegrationType IntegrationType { get; set; }

		/// <summary>
		/// Plasma ID of the Program provided by the Plasma/MediaGenix integration.
		/// This will only applicable when the IntegrationType is Plasma.
		/// </summary>
		[ChangeTracked]
		public string PlasmaId { get; set; }

		/// <summary>
		/// In case of Plasma Order: this contains the ID of the Editorial Object used to create this order.
		/// </summary>
		[ChangeTracked]
		public string EditorialObjectId { get; set; }

		/// <summary>
		/// Id of the Plasma Program or Feenix Order in YLE.
		/// </summary>
		[ChangeTracked]
		public string YleId { get; set; }

		/// <summary>
		/// A list containing IDs of the User Groups that are allowed to see/edit this order.
		/// </summary>
		[ChangeTracked]
		public ObservableCollection<int> UserGroupIds { get; private set; } = new ObservableCollection<int>();

		public void SetUserGroupIds(IEnumerable<int> userGroupsIds)
		{
			UserGroupIds.Clear();
			foreach (int userGroupId in userGroupsIds) UserGroupIds.Add(userGroupId);
		}

		/// <summary>
		/// Gets a collection of Cube View IDs of views where this order is visible.
		/// </summary>
		[ChangeTracked]
		public ObservableCollection<int> SecurityViewIds { get; set; } = new ObservableCollection<int>();

		public void SetSecurityViewIds(IEnumerable<int> securityViewIds)
		{
			SecurityViewIds.Clear();
			foreach (int securityViewId in securityViewIds) SecurityViewIds.Add(securityViewId);
		}

		[ChangeTracked]
		public SportsPlanning SportsPlanning { get; set; } = new SportsPlanning();

		[ChangeTracked]
		public NewsInformation NewsInformation { get; set; } = new NewsInformation();

		/// <summary>
		/// The name of the contract that should be used to create the event.
		/// </summary>
		public string Contract { get; set; }

		/// <summary>
		/// The name of the company that should be used to create the event.
		/// </summary>
		public string Company { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether an order is part of an internal event or not.
		/// </summary>
		[ChangeTracked]
		public bool IsInternal { get; set; }

		/// <summary>
		/// Login name of the user that created the Order.
		/// </summary>
		[ChangeTracked]
		public string CreatedByUserName { get; set; }

		/// <summary>
		/// Email address of the user that created the Order.
		/// </summary>
		[ChangeTracked]
		public string CreatedByEmail { get; set; }

		/// <summary>
		/// Phone number of the user that created the Order.
		/// </summary>
		[ChangeTracked]
		public string CreatedByPhone { get; set; }

		/// <summary>
		/// Login name of the user that last modified the Order.
		/// </summary>
		[ChangeTracked]
		public string LastUpdatedBy { get; set; }

		/// <summary>
		/// Email address of the user that last modified the Order.
		/// </summary>
		[ChangeTracked]
		public string LastUpdatedByEmail { get; set; }

		/// <summary>
		/// Phone number of the user that last modified the Order.
		/// </summary>
		[ChangeTracked]
		public string LastUpdatedByPhone { get; set; }

		internal event EventHandler<string> McrOperatorNotesChanged;

		/// <summary>
		/// Notes that can only be seen and updated by an MCR user from the CustomerUI.
		/// </summary>
		[ChangeTracked]
		public string McrOperatorNotes
		{
			get => mcrOperatorNotes;
			set
			{
				mcrOperatorNotes = value;
				McrOperatorNotesChanged?.Invoke(this, mcrOperatorNotes);
			}
		}

		internal event EventHandler<string> MediaOperatorNotesChanged;

		[ChangeTracked]
		public string MediaOperatorNotes
		{
			get => mediaOperatorNotes;
			set
			{
				mediaOperatorNotes = value;
				MediaOperatorNotesChanged?.Invoke(this, mediaOperatorNotes);
			}
		}

		internal event EventHandler<string> ErrorDescriptionChanged;

		/// <summary>
		/// Gets or sets the description that should be filled out by an MCR user when the Order was completed with errors.
		/// If the order was completed with errors and this value is not filled out, then the order should be visible in the UI_MCR_Change and UI_Recording views.
		/// </summary>
		[ChangeTracked]
		public string ErrorDescription
		{
			get => errorDescription;
			set
			{
				errorDescription = value;
				ErrorDescriptionChanged?.Invoke(this, errorDescription);
			}
		}

		internal event EventHandler<string> ReasonForCancellationOrRejectionChanged;

		/// <summary>
		/// Gets or sets a property containing the reason why a user has rejected or canceled an Order.
		/// </summary>
		[ChangeTracked]
		public string ReasonForCancellationOrRejection
		{
			get => reasonForBeingCancelledOrRejected;
			set
			{
				reasonForBeingCancelledOrRejected = value;
				ReasonForCancellationOrRejectionChanged?.Invoke(this, reasonForBeingCancelledOrRejected);
			}
		}

		public bool UseGraphicsEngineAsInput
		{
			get => useGraphicsEngineAsInput;
			set
			{
				useGraphicsEngineAsInput = value;
				UseGraphicsEngineAsInputChanged?.Invoke(this, useGraphicsEngineAsInput);
			}
		}

		internal event EventHandler<bool> UseGraphicsEngineAsInputChanged;

		/// <summary>
		/// When adding new services to a running order, a new start now order will be created based on the running one.
		/// </summary>
		public bool ConvertedFromRunningToStartNow { get; set; }

		/// <summary>
		/// When adding new services to a running order, a new start now order will be created.
		/// For referring purposes the old order Id is saved inside the new order.
		/// </summary>
		[ChangeTracked]
		public Guid PreviousRunningOrderId { get; set; }

		/// <summary>
		/// Indicates whether the Order should start as soon as possible or not.
		/// </summary>
		[ChangeTracked]
		public bool StartNow { get; set; } = false;

		/// <summary>
		/// Indicates whether the Order should stop immediately.
		/// </summary>
		public bool StopNow { get; set; } = false;

		[ChangeTracked]
		public BillingInfo BillingInfo { get; set; } = new BillingInfo();

		/// <summary>
		/// Indicates if this order can be cancelled.
		/// </summary>
		public bool CanCancel
		{
			get
			{
				return Status == Status.Preliminary || Status == Status.WaitingOnEbu || Status == Status.Planned || Status == Status.Rejected || Status == Status.Confirmed || Status == Status.ChangeRequested || Status == Status.FileProcessing;
			}
		}

		/// <summary>
		/// Indicates if this order can be rejected.
		/// </summary>
		public bool CanReject
		{
			get => Status == Status.Planned;
		}

		/// <summary>
		/// Indicates if this order can be confirmed.
		/// </summary>
		public bool CanConfirm
		{
			get
			{
				return Status == Status.Planned || Status == Status.ChangeRequested;
			}
		}

		/// <summary>
		/// Indicates if this order can be deleted.
		/// </summary>
		public bool CanDelete
		{
			get
			{
				return Status == Status.Rejected || Status == Status.Cancelled || Status == Status.Completed || Status == Status.CompletedWithErrors;
			}
		}

		public bool IsSaved => (Status == Status.Preliminary || Status == Status.WaitingOnEbu || Status == Status.PlannedUnknownSource);

		/// <summary>
		/// Indication if the order is generated from a template.
		/// </summary>
		public bool IsCreatedFromTemplate { get; set; }

		public string ExternalJsonFilePath { get; set; }

		public bool LateChange { get; set; }

		public DateTime PublicationStart { get; set; }

		public DateTime PublicationEnd { get; set; }

		/// <summary>
		/// This method is used to directly updated the Media Operator Notes property on the ReservationInstance.
		/// </summary>
		/// <returns>True if update was successful.</returns>
		public bool UpdateMediaOperatorNotesProperty(Helpers helpers)
		{
			var dictionary = new Dictionary<string, object>
			{
				{ PropertyNameMediaOperatorNotes, MediaOperatorNotes }
			};

			return TryUpdateCustomProperties(helpers, dictionary);
		}

		/// <summary>
		/// This method is used to directly updated the MCR Operator Notes property on the ReservationInstance.
		/// </summary>
		/// <returns>True if update was successful.</returns>
		public bool UpdateMcrOperatorNotesProperty(Helpers helpers)
		{
			var dictionary = new Dictionary<string, object>
			{
				{ PropertyNameMcrOperatorNotes, McrOperatorNotes }
			};

			return TryUpdateCustomProperties(helpers, dictionary);
		}

		public bool ClearIntegrationReferences(Helpers helpers, bool clearAllowed = false)
		{
			if (IntegrationType != IntegrationType.None && clearAllowed)
			{
				var integrationReferenceProperties = new Dictionary<string, object>
				{
					{ PropertyNamePlasmaId, string.Empty },
					{ PropertyNameYleId, string.Empty },
					{ PropertyNameEurovisionId, string.Empty },
					{ PropertyNameEurovisionTransmissionNumber, string.Empty },
					{ PropertyNameEditorialObjectId, string.Empty },
				};

				return TryUpdateCustomProperties(helpers, integrationReferenceProperties);
			}

			return false;
		}

		/// <summary>
		/// This method is used to directly updated the Comments property on the ReservationInstance.
		/// </summary>
		/// <returns>True if update was successful.</returns>
		public bool UpdateCommentsProperty(Helpers helpers)
		{
			var dictionary = new Dictionary<string, object>
			{
				{ PropertyNameComments, Comments }
			};

			return TryUpdateCustomProperties(helpers, dictionary);
		}

		public bool TryUpdateCustomProperties(Helpers helpers, Dictionary<string, object> customProperties)
		{
			if (customProperties.TryGetValue(PropertyNameServiceConfigurations, out var serviceConfigsToSave))
			{
				bool success = helpers.OrderManagerElement.AddOrUpdateServiceConfigurations(Reservation.ID, Reservation.End, Convert.ToString(serviceConfigsToSave));
				if(!success) throw new ServiceConfigurationsUpdateFailedException(Reservation.ID);

				customProperties.Remove(PropertyNameServiceConfigurations);
			}

			return helpers.ReservationManager.TryUpdateCustomProperties(Reservation, customProperties);
		}

		public void UpdateSecurityViewIds(Helpers helpers, HashSet<int> securityViewIds)
		{
			if (Reservation.SecurityViewIDs.OrderBy(x => x).SequenceEqual(securityViewIds.OrderBy(x => x)))
			{
				helpers.Log(nameof(LiteOrder), nameof(UpdateSecurityViewIds), $"Security View IDs on reservation are already {string.Join(",", securityViewIds)}", Reservation.Name);
				return;
			}

			SetSecurityViewIds(securityViewIds);

			Reservation = helpers.ReservationManager.UpdateSecurityViewIds(Reservation, securityViewIds);
		}

		public bool UpdateEventReference(Helpers helpers)
		{
			var dictionary = new Dictionary<string, object>
			{
				{ PropertyNameEventId, Event.Id.ToString() }
			};

			return TryUpdateCustomProperties(helpers, dictionary);
		}

		public bool UpdateConvertRunningToStartNowProperty(Helpers helpers)
		{
			var dictionary = new Dictionary<string, object>
			{
				{ PropertyNameConvertedFromRunningStartnow, ConvertedFromRunningToStartNow.ToString() }
			};

			return TryUpdateCustomProperties(helpers, dictionary);
		}

		/// <summary>
		/// This method is used to directly update the LastUpdatedBy property on the Order ReservationInstance.
		/// </summary>
		/// <returns>True if update was successful.</returns>
		public bool UpdateLastUpdatedByProperty(Helpers helpers)
		{
			var dictionary = new Dictionary<string, object>
			{
				{ PropertyNameLastUpdatedBy, LastUpdatedBy }
			};

			return TryUpdateCustomProperties(helpers, dictionary);
		}

		public virtual object Clone()
		{
			return new LiteOrder(this);
		}
	}
}
