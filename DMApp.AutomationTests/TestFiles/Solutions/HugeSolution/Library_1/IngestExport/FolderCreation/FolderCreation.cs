using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.FolderCreation
{
	public class FolderCreation : NonLiveOrder
	{
        /// <summary>
        /// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
        /// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
        /// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
        /// </summary>
        public FolderCreation()
		{
			Destination = default(string);
			ContentType = default(string);
			ParentFolder = default(string);
			AdditionalInformation = default(string);
			NewProgramFolderRequestDetails = default(NewProgramFolderRequestDetails);
			NewEpisodeFolderRequestDetails = default(List<NewEpisodeFolderRequestDetails>);
            OriginalDeleteDate = default(DateTime);
        }

		[JsonIgnore]
		public override Type OrderType
		{
			get
			{
				return Type.IplayFolderCreation;
			}
		}

		[JsonIgnore]
		public override string ShortDescription
		{
			get
			{
				var earliestDeleteDate = RetrieveTheEarliestDeleteDate();
				return OrderDescription + " - " + EnumExtensions.GetDescriptionFromEnumValue(Type.IplayFolderCreation) + " - " + Destination + " - " + (earliestDeleteDate != DateTime.MinValue ? earliestDeleteDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "Unknown");
			}
		}

        /// <summary>
        /// Original Delete Date which was defined when Order was set to Completed.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime OriginalDeleteDate { get; protected set; }

        public DateTime EarliestDeletionDate { get; set; }

        public string Destination { get; set; }

		public string ContentType { get; set; }

		public string ParentFolder { get; set; }

		public string AdditionalInformation { get; set; }

        [JsonProperty]
        public NewProgramFolderRequestDetails NewProgramFolderRequestDetails { get; set; }

        [JsonProperty]
        public List<NewEpisodeFolderRequestDetails> NewEpisodeFolderRequestDetails { get; set; }

		public DateTime RetrieveTheEarliestDeleteDate()
		{
			List<DateTime> folderRequestDates = new List<DateTime>();

			if (NewEpisodeFolderRequestDetails != null && NewEpisodeFolderRequestDetails.Any())
			{
				folderRequestDates.AddRange(NewEpisodeFolderRequestDetails.Select(r => r.DeleteDate.Date).Where(d => d != DateTime.MinValue));
			}

			if (NewProgramFolderRequestDetails != null && !NewProgramFolderRequestDetails.IsDeleteDateUnknown && NewProgramFolderRequestDetails.DeleteDate != DateTime.MinValue)
			{
				folderRequestDates.Add(NewProgramFolderRequestDetails.DeleteDate.Date);
			}
			else if (!folderRequestDates.Any()) return DateTime.MinValue;
			else
			{
				//Nothing
			}
			
			return folderRequestDates.OrderBy(d => d).First();
		}

        public void SetOriginalDeleteDate(DateTime deleteDate)
        {
            OriginalDeleteDate = OriginalDeleteDate == default(DateTime) ? deleteDate : OriginalDeleteDate;
        }
    }
}