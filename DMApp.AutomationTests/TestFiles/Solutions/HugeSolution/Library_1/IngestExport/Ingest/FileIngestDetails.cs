using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Ingest
{
	public class FileIngestDetails : MaterialIngestDetails
	{
		private const MaterialTypes materialType = MaterialTypes.FILE;

		/// <summary>
		/// Default constructor that was added so all fields are always included in the auto-generated EXE block in the Automation Scripts.
		/// This caused issues in the past were data was lost because properties were not filled out when deserializing and serializing an object of this type.
		/// HIGHLY RECOMMENDED for all classes that are serialized in a Class Library.
		/// </summary>
		public FileIngestDetails()
		{
			SourceFileLocation = default(string);
			SourceFolderName = default(string);
			SourceFolderNameUrls = default(string[]);
			SourceFileUrls = default(string[]);
			CustomSourceFileLocation = default(string);
			EmailAddressOfTheSender = default(string);
			MaterialType = default(string);
			SourceFormat = default(string);
			MaterialIncludesHighFrameRateFootage = default(bool);
			AdditionalInfoAboutHighFrameRateFootage = default(string);
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

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string SourceFileLocation { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string SourceFolderName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] SourceFolderNameUrls { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] SourceFileUrls { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string CustomSourceFileLocation { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string EmailAddressOfTheSender { get; set; }

		public string MaterialType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string SourceFormat { get; set; }

		public bool MaterialIncludesHighFrameRateFootage { get; set; }

        [JsonProperty]
        public string AdditionalInfoAboutHighFrameRateFootage { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Source file location: {SourceFileLocation} | ");
			sb.AppendLine($"Source file: {string.Join(";", SourceFolderNameUrls)} | ");
			sb.AppendLine($"Material type: {MaterialType} | ");
			sb.AppendLine($"Material includes high frame rate: {MaterialIncludesHighFrameRateFootage} | ");
			sb.AppendLine($"additional information about high frame rate: {AdditionalInfoAboutHighFrameRateFootage} | ");

			return sb.ToString();
		}
	}
}