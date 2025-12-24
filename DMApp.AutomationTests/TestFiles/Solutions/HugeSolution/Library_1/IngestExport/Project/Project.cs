namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Project
{
	using System;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;

    public class Project : NonLiveOrder
	{
        [JsonProperty]
		public override IngestExport.Type OrderType => IngestExport.Type.NonInterplayProject;

		[JsonIgnore]
		public override string ShortDescription => $"{OrderDescription} - {IngestExport.Type.NonInterplayProject.GetDescription()}";

        [JsonProperty]
        public DateTime MaterialDeliveryTime { get; set; }

        [JsonProperty]
        public string ImportDepartment { get; set; }

        [JsonProperty]
        public string ProductionDepartmentName { get; set; }

        [JsonProperty]
        public string OtherDepartmentName { get; set; }

        [JsonProperty]
        public string ProjectType { get; set; }

        [JsonProperty]
        public string AvidProjectVideoFormat { get; set; }

        [JsonProperty]
        public string ProjectName { get; set; }

        [JsonProperty]
        public string ProductionNumber { get; set; }

        [JsonProperty]
        public string ProjectAdditionalInfo { get; set; }

        [JsonProperty]
        public bool IsLongerStoredBackUpChecked { get; set; }

        [JsonProperty]
        public DateTime BackupDeletionDate { get; set; }

        [JsonProperty]
        public string WhyMustBackUpBeStoredLonger { get; set; }

        [JsonProperty]
        public string AdditionalInfo { get; set; }

        [JsonProperty]
		public bool ReturnSourceMediaToKalustovarasto { get; set; }

		[JsonProperty]
		public string CardReturnDestination { get; set; }

        [JsonProperty]
		public string CardReturnRecipientName { get; set; }

        [JsonProperty]
		public string CardReturnPlNumber { get; set; }

        /// <summary>
        /// Original Delete Date which was defined when Order was set to Completed.
        /// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime OriginalDeleteDate { get; protected set; }

        public void SetOriginalDeleteDate(DateTime deleteDate)
        {
            OriginalDeleteDate = OriginalDeleteDate == default(DateTime) ? deleteDate : OriginalDeleteDate;
        }
    }
}