using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson
{
	public class Decoding
	{
		public Decoding()
		{
			Encoding = default(string);
			EncryptionType = default(string);
			EncryptionKey = default(string);
			VideoFormat = default(string);
			ServiceSelection = default(string);
		}

		[MatchingProfileParameter(ProfileParameterGuids.Strings.EncodingString)]
		public string Encoding { get; set; }

		[JsonProperty("Encryption Type")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.EncryptionTypeString)]
		public string EncryptionType { get; set; }

		[JsonProperty("Encryption Key")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.EncryptionKeyString)]
		public string EncryptionKey { get; set; }

		[JsonProperty("Video Format")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.VideoFormatString)]
		public string VideoFormat { get; set; }

		[JsonProperty("Service Selection")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.ServiceSelectionString)]
		public object ServiceSelection { get; set; }
	}
}