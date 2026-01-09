namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Export
{
    using Newtonsoft.Json;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class Export : NonLiveOrder
	{
		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public Export()
		{
			MaterialSource = default(Sources);
			InterplayPamExport = default(InterplayPamExport);
			MediaParkkiExport = default(MediaParkkiExport);
			IsilonBuExport = default(IsilonBuExport);
			MamExport = default(MamExport);
			OtherExport = default(OtherExport);
			ExportInformation = default(ExportInformation);
            AdditionalInformation = default(string);
		}

		[JsonIgnore]
		public override Type OrderType
		{
			get
			{
				return Type.Export;
			}
		}

		[JsonIgnore]
		public override string ShortDescription { get => OrderDescription + " - " + EnumExtensions.GetDescriptionFromEnumValue(MaterialSource) + " to " + ExportInformation.TargetOfExport; }

        [JsonProperty]
        public Sources MaterialSource { get; set; }

        [JsonProperty]
        public InterplayPamExport InterplayPamExport { get; set; }

        [JsonProperty]
        public MediaParkkiExport MediaParkkiExport { get; set; }

        [JsonProperty]
        public IsilonBuExport IsilonBuExport { get; set; }

        [JsonProperty]
        public MamExport MamExport { get; set; }

        [JsonProperty]
        public OtherExport OtherExport { get; set; }

        [JsonProperty]
        public ExportInformation ExportInformation { get; set; }
       
        [JsonProperty]
        public string AdditionalInformation { get; set; }

        public void UpdateExportFilesToSpecificDirectory(Helpers helpers, string folderPath)
        {
            InterplayPamExport?.OtherHiresFileAttachmentInfo?.UpdateFilesToDirectory(helpers, folderPath);
            InterplayPamExport?.OtherViewingVideoFileAttachmentInfo?.UpdateFilesToDirectory(helpers, folderPath);

            MamExport?.OtherTargetVideoFormatFileAttachmentInfo?.UpdateFilesToDirectory(helpers, folderPath);

            MediaParkkiExport?.OtherTargetVideoFormatFileAttachmentInfo?.UpdateFilesToDirectory(helpers, folderPath);

            OtherExport?.OtherExportSourceFileAttachmentInfo?.UpdateFilesToDirectory(helpers, folderPath);

            ExportInformation?.ExportInformationFileAttachmentInfo?.UpdateFilesToDirectory(helpers, folderPath);
        }
    }
}