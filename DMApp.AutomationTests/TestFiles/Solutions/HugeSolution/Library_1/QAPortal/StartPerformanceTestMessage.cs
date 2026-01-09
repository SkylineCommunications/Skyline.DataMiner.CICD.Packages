namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.QAPortal
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Newtonsoft.Json;

	public class StartPerformanceTestMessage
	{
		public string AgentName { get; set; }

		public int AmountOfActions { get; set; }

		public string Author { get; set; }

		public string DataMinerVersion { get; set; }

		[JsonIgnore]
		public DateTime MaxEndTimeValue { get; set; }

		public string MaxEndTime => MaxEndTimeValue.ToString("u");

		public string SquadName { get; set; }

		public string TestName { get; set; }
	}
}
