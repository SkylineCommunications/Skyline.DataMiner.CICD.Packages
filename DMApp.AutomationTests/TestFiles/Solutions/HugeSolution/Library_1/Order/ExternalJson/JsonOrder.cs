using System;
using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson
{
	public class JsonOrder
	{
		public JsonOrder()
		{
			Name = default(string);
			Start = default(DateTime);
			End = default(DateTime);
			MainSignal = new MainSignal();
			AdditionalInformation = default(string);
		}

		[MatchingOrderProperty("ManualName")]
		public string Name { get; set; }

		[MatchingOrderProperty("Start")]
		public DateTime Start { get; set; }

		[MatchingOrderProperty("End")]
		public DateTime End { get; set; }

		[JsonProperty("Main Signal")]
		public MainSignal MainSignal { get; set; }

		[JsonProperty("Additional Information")]
		[MatchingOrderProperty("Comments")]
		public string AdditionalInformation { get; set; }
	}
}