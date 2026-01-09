using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson
{
	public class Demodulating
	{
		public Demodulating()
		{
			DownlinkFrequency = default(double);
			Polarization = default(string);
			ModulationStandard = default(string);
			Modulation = default(string);
			SymbolRate = default(double);
			Fec = default(string);
		}

		[JsonProperty("Downlink Frequency")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.DownlinkFrequencyString)]
		public double DownlinkFrequency { get; set; }

		[MatchingProfileParameter(ProfileParameterGuids.Strings.PolarizationString)]
		public string Polarization { get; set; }

		[JsonProperty("Modulation Standard")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.ModulationStandardString)]
		public string ModulationStandard { get; set; }

		[MatchingProfileParameter(ProfileParameterGuids.Strings.ModulationString)]
		public string Modulation { get; set; }

		[JsonProperty("Symbol Rate")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.SymbolRateString)]
		public double SymbolRate { get; set; }

		[JsonProperty("FEC")]
		[MatchingProfileParameter(ProfileParameterGuids.Strings.FecString)]
		public string Fec { get; set; }
	}
}