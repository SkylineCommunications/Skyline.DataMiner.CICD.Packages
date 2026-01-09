namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using NonLiveOrderType = IngestExport.Type;

	public class Ingest : NonLiveOrder
	{        
        private readonly string cardImportMaterialType = "CARD";
		private readonly string hddImportMaterialType = "HDD";
		private readonly string fileImportMaterialType = "FILE";
		private readonly string metroMamImportMaterialType = "METRO MAM";

		[JsonIgnore]
		public override NonLiveOrderType OrderType => NonLiveOrderType.Import;		
		
		[JsonIgnore]
		public override string ShortDescription => OrderDescription + " - " + NonLiveOrderType.Import.GetDescription() + AddDeliveryTimeToShortDescription() + CreateDescriptionFromMaterialIngestRequestDetails();

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime DeliveryTime { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ProductNumber { get; set; }

        /// <summary>
        /// Original Delete Date which was defined when Order was set to Completed.
        /// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime OriginalDeleteDate { get; protected set; }

        [JsonProperty]
        public string AdditionalInformation { get; set; }

		/// <summary>
		/// Gets or sets an object containing the details about the destination of the ingest.
		/// Can never be null.
		/// </summary>
		public IngestDestination IngestDestination { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ImportMaterialType { get; set; }

		/// <summary>
		/// Gets or sets a list containing details per card ingest requests.
		/// Is null if there are no card ingest requests.
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<CardIngestDetails> CardImportDetails { get; set; }

		/// <summary>
		/// Gets or sets a list containing details per card ingest requests.
		/// Is null if there are no card ingest requests. To be removed later on, to support old tickets which has another name saved
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<CardIngestDetails> CardIngestDetails { get; set; }

		/// <summary>
		/// Gets or sets a list containing details per HDD ingest requests.
		/// Is null if there are no HDD ingest requests.
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<HddIngestDetails> HddImportDetails { get; set; }

		/// <summary>
		/// Gets or sets a list containing details per HDD ingest requests.
		/// Is null if there are no HDD ingest requests. To be removed later on, to support old tickets which has another name saved
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<HddIngestDetails> HddIngestDetails { get; set; }

		/// <summary>
		/// Gets or sets a list containing details per File ingest requests.
		/// Is null if there are no File ingest requests. 
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<FileIngestDetails> FileImportDetails { get; set; }

		/// <summary>
		/// Gets or sets a list containing details per File ingest requests.
		/// Is null if there are no File ingest requests. To be removed later on, to support old tickets which has another name saved
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<FileIngestDetails> FileIngestDetails { get; set; }

		/// <summary>
		/// Gets or sets a list containing details per File ingest requests.
		/// Is null if there are no File ingest requests.
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<MetroMamImportDetails> MetroMamImportDetails { get; set; }

		/// <summary>
		/// Gets or sets a list containing details per File ingest requests.
		/// Is null if there are no File ingest requests. To be removed later on, to support old tickets which has another name saved
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<MetroMamImportDetails> MetroMamIngestDetails { get; set; }

		public bool MaterialToBeRelinkedToOriginalMaterialsInColorPostProduction { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string InterplayFormat { get; set; }

		public bool MultiCameraMaterial { get; set; }

		public bool? BackUpsLongerStored { get; set; }

        [JsonProperty]
        public string WhyBackUpLongerStored { get; set; }

        [JsonProperty]
        public DateTime BackupDeletionDate { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

			CardImportDetails?.ForEach((cardImportDetail) =>
			{
				sb.AppendLine($"Material Card Import {CardImportDetails.IndexOf(cardImportDetail) + 1}:");
				sb.AppendLine(cardImportDetail.ToString());
				sb.AppendLine();
			});

			HddImportDetails?.ForEach((hddImportDetail) =>
			{
				sb.AppendLine($"Material HDD Import {HddImportDetails.IndexOf(hddImportDetail) + 1}:");
				sb.AppendLine(hddImportDetail.ToString());
				sb.AppendLine();
			});

			FileImportDetails?.ForEach((fileImportDetail) =>
			{
				sb.AppendLine($"Material File Import {FileImportDetails.IndexOf(fileImportDetail) + 1}:");
				sb.AppendLine(fileImportDetail.ToString());
				sb.AppendLine();
			});

			MetroMamImportDetails?.ForEach((metroMamImportDetail) =>
			{
				sb.AppendLine($"Material Metro MAM Import {MetroMamImportDetails.IndexOf(metroMamImportDetail) + 1}:");
				sb.AppendLine(metroMamImportDetail.ToString());
				sb.AppendLine();
			});
			
			return sb.ToString();
        }

        /// <summary>
        /// Creates a description based on existing Material Ingest Request Details within the ingest order.
        /// </summary>
        /// <returns> Returns the merged description.</returns>
        private string CreateDescriptionFromMaterialIngestRequestDetails()
		{
			StringBuilder mergedDescription = new StringBuilder();

			if (CardImportDetails != null && CardImportDetails.Any())
			{
				string chosenCameraType;
				foreach (CardIngestDetails cardIngestDetail in CardImportDetails)
				{
					chosenCameraType = (cardIngestDetail.CameraOrAudioType != EnumExtensions.GetDescriptionFromEnumValue(CameraOrAudioTypes.OTHER)) ? cardIngestDetail.CameraOrAudioType : "Other: " + cardIngestDetail.CustomCameraType;
					mergedDescription.Append(" / " + cardImportMaterialType + " - " + chosenCameraType + " - " + cardIngestDetail.NumberOfSimilarCards + " card(s)");
				}
			}

			if (HddImportDetails != null && HddImportDetails.Any())
			{
				mergedDescription.Append(" / " + HddImportDetails.Count + "x " + hddImportMaterialType);
			}

			if (FileImportDetails != null && FileImportDetails.Any())
			{
				mergedDescription.Append(" / " + FileImportDetails.Count + "x " + fileImportMaterialType);
			}

			if (MetroMamImportDetails != null && MetroMamImportDetails.Any())
			{
				mergedDescription.Append(" / " + MetroMamImportDetails.Count + "x " + metroMamImportMaterialType);
			}

			return mergedDescription.ToString();
		}

		private string AddDeliveryTimeToShortDescription()
		{
            if (IngestDestination != null && IngestDestination.Destination != EnumExtensions.GetDescriptionFromEnumValue(InterplayPamElements.UA))
            {
                return " - Material delivery: " + DeliveryTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            }

            return String.Empty;
		}

        public void SetOriginalDeleteDate(DateTime deleteDate)
        {
            OriginalDeleteDate = OriginalDeleteDate == default ? deleteDate : OriginalDeleteDate;
        }
    }
}