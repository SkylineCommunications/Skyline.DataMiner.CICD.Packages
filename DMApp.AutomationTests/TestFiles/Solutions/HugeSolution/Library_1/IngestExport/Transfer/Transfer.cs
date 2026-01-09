namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Transfer
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	public class Transfer : NonLiveOrder
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public Transfer()
		{
			Source = default(string);
			SourceFolder = default(string);
			SourceFolderUrls = default(string[]);
			FileUrls = default(string[]);
			FileType = default(string);
			Destination = default(string);
			ReceiverEmailAddress = default(string);
			InterplayDestinationFolder = default(string);
			AdditionalCustomerInformation = default(string);
		}

        [JsonProperty]
        public override IngestExport.Type OrderType
		{
			get
			{
				return Type.IplayWgTransfer;
			}
		}

		[JsonIgnore]
		public override string ShortDescription { get => OrderDescription + " - " + EnumExtensions.GetDescriptionFromEnumValue(Type.IplayWgTransfer) + " - " + Source + " to " + Destination; }

		public string Source { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string SourceFolder { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] SourceFolderUrls { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] FileUrls { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string FileType { get; set; }

		public string Destination { get; set; }

		public string ReceiverEmailAddress { get; set; }

		public string InterplayDestinationFolder { get; set; }

        [JsonProperty]
        public string AdditionalCustomerInformation { get; set; }
	}
}