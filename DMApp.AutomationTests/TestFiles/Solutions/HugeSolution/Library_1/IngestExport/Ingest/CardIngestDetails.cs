using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest
{
	public class CardIngestDetails : MaterialIngestDetails
	{
		private const MaterialTypes materialType = MaterialTypes.CARD;

		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public CardIngestDetails()
		{
			CameraOrAudioType = default(string);
			CustomCameraType = default(string);
			CardType = default(string);
			CustomCardType = default(string);
			SourceFormat = default(string);
			MaterialIncludesHighFrameRateFootage = default(bool);
			AdditionalInfoAboutHighFrameRateFootage = default(string);
			NumberOfSimilarCards = default(int);
			SizeOfCard = default(int?);
			CardCanBeReused = default(bool);
			CardsToBeReturned = default(string);
			CardsToBeReturnedLocation = default(string);
            CardsToBeReturnedNameOfTheRecipient = default(string);
            CardsToBeReturnedPlNumber = default(string);
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

		public string CameraOrAudioType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string CustomCameraType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string CardType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string CustomCardType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string SourceFormat { get; set; }

		public bool MaterialIncludesHighFrameRateFootage { get; set; }

        [JsonProperty]
        public string AdditionalInfoAboutHighFrameRateFootage { get; set; }

		public int NumberOfSimilarCards { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? SizeOfCard { get; set; }

		public bool CardCanBeReused { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string CardsToBeReturned { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CardsToBeReturnedLocation { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string CardsToBeReturnedNameOfTheRecipient { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CardsToBeReturnedPlNumber { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();

			var chosenCameraType = (CameraOrAudioType != EnumExtensions.GetDescriptionFromEnumValue(CameraOrAudioTypes.OTHER)) ? CameraOrAudioType : "Other: " + CustomCameraType;
			sb.AppendLine($"Camera Type: {chosenCameraType} | ");
			sb.AppendLine($"Material includes high frame rate: {MaterialIncludesHighFrameRateFootage} | ");
			sb.AppendLine($"additional information about high frame rate: {AdditionalInfoAboutHighFrameRateFootage} | ");
			sb.AppendLine($"Number of similar cards: {NumberOfSimilarCards} | ");
			sb.AppendLine($"Cards can be returned to kalustovarasto: {CardCanBeReused} | ");
			sb.AppendLine($"Which cards are to be returned to orderer: {CardsToBeReturned} | ");
			sb.AppendLine($"Where should the cards be returned to: {CardsToBeReturnedLocation} | ");
			sb.AppendLine($"Name of the recipient: {CardsToBeReturnedNameOfTheRecipient} | ");
			sb.AppendLine($"PL number: {CardsToBeReturnedPlNumber} | ");

			return sb.ToString();
		}

	}
}