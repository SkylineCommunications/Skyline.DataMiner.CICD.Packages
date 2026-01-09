namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;

	public abstract class NonLiveOrder
	{
        public static readonly string DefaultOwner = "None";

        public static readonly string OrderDescriptionTicketField = "Order Description";

        public static readonly string DeadlineTicketField = "Deadline";

        public static readonly string StartTimeTicketField = "Start Time";

        public static readonly string OwnerTicketField = "Owner";

        public static readonly string TypeTicketField = "Type";

        public static readonly string DataTicketField = "Data";

        public static readonly string StateTicketField = "State";

        public static readonly string MgmtStateTicketField = "MGMT State";

        public static readonly string MaterialSourceTicketField = "Material Source";

        public static readonly string ProgramNameTicketField = "Program Name";

        public static readonly string DeliveryDateTicketField = "Delivery Date";

        public static readonly string SourceFolderPathTicketField = "Source Folder Path";

        public static readonly string TargetFolderPathTicketField = "Target Folder Path";

        public static readonly string InterplayFolderPathTicketField = "Interplay Folder Path";

        public static readonly string InterplayFormatTicketField = "Interplay Format";
        public static readonly string CardsCanBeReturnedToKalustovarastoTicketField = "Cards can be returned to Kalustovarasto";
        public static readonly string DeletionDateTicketField = "Deletion Date";
        public static readonly string BackupDeletionDateTicketField = "Backup Deletion Date";
        public static readonly string OriginalDeleteDateTicketField = "Original Delete Date";

        public static readonly string ExportInformationSubtitleAttachmentsPathTicketField = "Subtitle Attachments Path";

        public static readonly string AdditionalInformationTicketField = "Additional Information";

        public static readonly string CreatedByTicketField = "Created By";
        public static readonly string CreatedByEmailTicketField = "Created By Email";
        public static readonly string CreatedByPhoneTicketField = "Created By Phone";

        public static readonly string ModifiedByTicketField = "Modified By";

        public static readonly string LastModifiedByTicketField = "Last Modified By";
        public static readonly string LastModifiedByEmailTicketField = "Last Modified By Email";
        public static readonly string LastModifiedByPhoneTicketField = "Last Modified By Phone";

        public static readonly string ShortDescriptionTicketField = "Short Description";

        public static readonly string ReasonOfRejectionField = "Reason of Rejection";

		public static readonly string TeamHkiField = "Team_HKI";

        public static readonly string TeamNewsField = "Team_NEWS";

        public static readonly string TeamTreField = "Team_TRE";

        public static readonly string TeamVsaField = "Team_VSA";

        public static readonly string TeamMgmtField = "Team_MGMT";
        
		public static readonly string OperatorCommentTicketField = "Operator Comment";

		public static readonly string IsilonBackupFileLocationTicketField = "Isilon Backup File Location";

		public abstract Type OrderType { get; }

		public abstract string ShortDescription { get; }

		public int? DataMinerId { get; set; }

		public int? TicketId { get; set; }

		public State State { get; set; }

		public MgmtState MgmtState { get; set; }

		public string OrderDescription { get; set; }

        [JsonProperty]
        public DateTime Deadline { get; set; }

		[JsonProperty]
        public List<string> EmailReceivers { get; set; } = new List<string>();

        public DateTime StartTime
		{
			get
			{
				return Deadline - TimeSpan.FromHours(1);
			}
		}

		/// <summary>
		/// Gets or sets the user to which the ticket is assigned.
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		/// Gets or sets the user that created the ticket.
		/// </summary>
		public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the UID of the ticket.
        /// </summary>
        public Guid UID { get; set; }

        /// <summary>
        /// Email address of the user that created the Order.
        /// </summary>
        public string CreatedByEmail { get; set; }

		/// <summary>
		/// Phone number of the user that created the Order.
		/// </summary>
		public string CreatedByPhone { get; set; }

		/// <summary>
		/// Gets or sets the user that updated the ticket.
		/// </summary>
		public string LastModifiedBy { get; set; }

		/// <summary>
		/// Email address of the user that last modified the Order.
		/// </summary>
		public string LastModifiedByEmail { get; set; }

        /// <summary>
        /// Phone number of the user that last modified the Order.
        /// </summary>
        public string LastModifiedByPhone { get; set; }

		/// <summary>
		/// Gets or sets the users that modified the ticket.
		/// </summary>
		public HashSet<string> ModifiedBy { get; internal set; } = new HashSet<string>();

		public string ReasonOfRejection { get; set; }

		public string OperatorComment { get; set; }

		public bool TeamHki { get; set; }

		public bool TeamNews { get; set; }

		public bool TeamTre { get; set; }

		public bool TeamVsa { get; set; }

		public bool TeamMgmt { get; set; }

		public bool IsAssignedToSomeone => !string.IsNullOrWhiteSpace(Owner) && Owner.ToLower() != DefaultOwner.ToLower();

		public string IsilonBackupFileLocation { get; set; }

        /// <summary>
        /// Retrieving all existing file attachments from this Non Live Order.
        /// </summary>
        public List<string> GetAttachments(IEngine engine, string path)
        {
            string[] filePaths = new string[0];

            try
            {
                string fullPath = Path.Combine(path, string.Join("_", DataMinerId, TicketId));
                if (Directory.Exists(fullPath)) filePaths = Directory.GetFiles(fullPath);
            }
            catch (Exception e)
            {
                engine.Log($"Something went wrong while collecting the non live order files: " + e);
            }

            return filePaths.ToList();
        }
    }
}