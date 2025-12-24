using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson
{
	public class Audio
	{
		public Audio()
		{
			Ch1 = default(string);
			Ch2 = default(string);
			Ch3 = default(string);
			Ch4 = default(string);
			Ch5 = default(string);
			Ch6 = default(string);
			Ch7 = default(string);
			Ch8 = default(string);
			Ch9 = default(string);
			Ch10 = default(string);
			Ch11 = default(string);
			Ch12 = default(string);
			Ch13 = default(string);
			Ch14 = default(string);
			Ch15 = default(string);
			Ch16 = default(string);
		}

		[JsonProperty("Ch 1")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel1String)]
		public string Ch1 { get; set; }

		[JsonProperty("Ch 2")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel2String)]
		public string Ch2 { get; set; }

		[JsonProperty("Ch 3")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel3String)]
		public string Ch3 { get; set; }

		[JsonProperty("Ch 4")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel4String)]
		public string Ch4 { get; set; }

		[JsonProperty("Ch 5")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel5String)]
		public string Ch5 { get; set; }

		[JsonProperty("Ch 6")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel6String)]
		public string Ch6 { get; set; }

		[JsonProperty("Ch 7")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel7String)]
		public string Ch7 { get; set; }

		[JsonProperty("Ch 8")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel8String)]
		public string Ch8 { get; set; }

		[JsonProperty("Ch 9")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel9String)]
		public string Ch9 { get; set; }

		[JsonProperty("Ch 10")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel10String)]
		public string Ch10 { get; set; }

		[JsonProperty("Ch 11")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel11String)]
		public string Ch11 { get; set; }

		[JsonProperty("Ch 12")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel12String)]
		public string Ch12 { get; set; }

		[JsonProperty("Ch 13")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel13String)]
		public string Ch13 { get; set; }

		[JsonProperty("Ch 14")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel14String)]
		public string Ch14 { get; set; }

		[JsonProperty("Ch 15")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel15String)]
		public string Ch15 { get; set; }

		[JsonProperty("Ch 16")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.AudioChannel16String)]
		public string Ch16 { get; set; }
	}
}