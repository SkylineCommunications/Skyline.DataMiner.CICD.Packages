using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest
{
	public class HddIngestDetails : MaterialIngestDetails
	{
		private const MaterialTypes materialType = MaterialTypes.HDD;

		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public HddIngestDetails()
		{
			ConnectionType = default(string);
			CustomConnectionType = default(string);
			DiskFormat = default(string);
			CustomDiskFormat = default(string);
			DiskSize = default(int);
			SourceFolder = default(string);
			MaterialIncludesHighFrameRateFootage = default(bool);
			AdditionalInfoAboutHighFrameRateFootage = default(string);
			IsAvidProject = default(bool);
			AvidProjectName = default(string);
			AvidProjectAdditionalInformation = default(string);
			EditorContactInformation = default(string);
            IsHddEvsDisk = default(bool);
			AdditionalInformation = default(string);
		}

        [JsonProperty]
        public override string Type
		{
			get
			{
				return EnumExtensions.GetDescriptionFromEnumValue(materialType);
			}
		}

		public string ConnectionType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string CustomConnectionType { get; set; }

		public string DiskFormat { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string CustomDiskFormat { get; set; }

		public int DiskSize { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string SourceFolder { get; set; }

		public bool MaterialIncludesHighFrameRateFootage { get; set; }

        [JsonProperty]
        public string AdditionalInfoAboutHighFrameRateFootage { get; set; }

		public bool IsAvidProject { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string AvidProjectName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string AvidProjectAdditionalInformation { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string EditorContactInformation { get; set; }

        public bool IsHddEvsDisk { get; set; }

		public bool CanSourceMediaBeReturnedToKalustovarasto { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string KalustovarastoWhereShouldCardsBeReturnedTo { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string KalustovarastoNameOfTheRecipient { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string KalustovarastoPlNumberForReturnOfTheCards { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Source folder: {SourceFolder} | ");
			sb.AppendLine($"Material includes high frame rate: {MaterialIncludesHighFrameRateFootage} | ");
			sb.AppendLine($"additional information about high frame rate: {AdditionalInfoAboutHighFrameRateFootage} | ");
			sb.AppendLine($"Material is Avid project: {IsAvidProject} | ");
			sb.AppendLine($"Avid project name: {AvidProjectName}");
			sb.AppendLine($"Additional Information: {AvidProjectAdditionalInformation} | ");
			sb.AppendLine($"Contact information of editor: {EditorContactInformation} | ");
			sb.AppendLine($"HDD is EVS-disk: {IsHddEvsDisk} | ");
			sb.AppendLine($"Can source media be returned to Kalustovarasto: {CanSourceMediaBeReturnedToKalustovarasto} | ");
			sb.AppendLine($"Where should cards be returned to: {KalustovarastoWhereShouldCardsBeReturnedTo} | ");
			sb.AppendLine($"Name of the recipient: {KalustovarastoNameOfTheRecipient} | ");
			sb.AppendLine($"PL number for return of the cards: {KalustovarastoPlNumberForReturnOfTheCards} | ");

			return sb.ToString();
		}
	}
}