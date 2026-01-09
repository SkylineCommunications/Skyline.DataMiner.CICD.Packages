using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson
{
	public class Source
	{
		public Source()
		{
			Type = default(string);
			Satellite = new Satellite();
			Demodulating = new Demodulating();
			Decoding = new Decoding();
			Audio = new Audio();
			Comments = default(string);
		}

		public string Type { get; set; }

		[MatchingFunction(FunctionGuids.SatelliteString)]
		public Satellite Satellite { get; set; }

		[MatchingFunction(FunctionGuids.DemodulatingString)]
		public Demodulating Demodulating { get; set; }

		[MatchingFunction(FunctionGuids.DecodingString)]
		public Decoding Decoding { get; set; }

		[ContainsProfileParameters]
		public Audio Audio { get; set; }

		[MatchingServiceProperty("Comments")]
		public string Comments { get; set; }
	}
}