namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	using System;
    using System.Collections.Generic;

    /// <summary>
    ///     A static class containing all the names of custom properties on service reservations as defined in the Booking
    ///     Managers.
    /// </summary>
    public static class ServicePropertyNames
	{
#pragma warning disable S2339 // Public constant members should not be used
		public const string Status = "Status";

		public const string VirtualPlatformPropertyName = "Virtual Platform";

		public const string ShortDescription = "Short Description";

		public const string ReportedIssuePropertyName = "Reported_Issue";

		public const string ServiceLevelPropertyName = "ServiceLevel";

        public const string IsGlobalEventLevelReceptionPropertyName = "IsGlobalEventLevelReception";

		public const string IntegrationTypePropertyName = "Integration";

		public const string IntegrationIsMasterPropertyName = "IntegrationIsMaster";

		public const string LinkedServiceIdPropertyName = "LinkedServiceId";

		public const string EurovisionIdPropertyName = "EurovisionId";

		public const string EurovisionTransmissionNumberPropertyName = "EurovisionTransmissionNumber";

		public const string EurovisionBookingDetailsPropertyName = "EurovisionBookingDetails";

		public const string CommentsPropertyName = "Comments";

		public const string OrderIdsPropertyName = "OrderIds";

		public const string ContactInformationNamePropertyName = "ContactInformationName";

		public const string ContactInformationTelephoneNumberPropertyName = "ContactInformationTelephoneNumber";

		public const string LiveUDeviceNamePropertyName = "LiveUDeviceName";

		public const string VidigoStreamSourceLinkPropertyName = "VidigoStreamSourceLink";

		public const string RecordingConfigurationPropertyName = "RecordingConfiguration";

		public const string BackupServicePropertyName = "UI_Backup";

		public const string CommentaryAudioPropertyName = "UI_Comm_Audio";

		public const string NameOfServiceToTransmitPropertyName = "NameOfServiceToTransmit";

		public const string OrderNamePropertyName = "OrderName";

		public const string AudioReturnInfoPropertyName = "AudioReturnInfo";

		public const string PlasmaIdsForArchivingPropertyName = "Plasma ID for Archive";

		public const string MessiLiveDescription = "MessiLiveDescription";

		public const string LateChange = "LateChange";

		public const string FileName = "FileName";

		public const string Channel = "Channel";

		public const string McrDestination = "MCRDestination";

		public const string McrDescription = "MCRDescription";

		public const string MCRStatus = "MCRStatus";
		
		public const string Automated = "Automated";

		public const string AllUserTasksCompleted = "AllUserTasksCompleted";

		public const string EvsIdPropertyName = "EvsId";

		public const string ProfileConfigurationFailReason = "ProfileConfigurationFailReason";


		public static readonly IEnumerable<string> UiProperties = new List<string>
		{
			BackupServicePropertyName,
			CommentaryAudioPropertyName,
			McrDestination,
			McrDescription,
			MessiLiveDescription,
			ShortDescription,
			FileName,
			AllUserTasksCompleted,
		};

#pragma warning restore S2339 // Public constant members should not be used
	}
}