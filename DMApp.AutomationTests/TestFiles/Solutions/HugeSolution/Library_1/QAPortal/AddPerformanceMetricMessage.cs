namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.QAPortal
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Newtonsoft.Json;

	public class AddPerformanceMetricMessage
	{
		public Agent Agent { get; set; }

		public int AmountOfActions { get; set; }

		public string DataMinerVersion { get; set; }

		[JsonIgnore]
		public DateTime DateValue { get; set; }

		public string Date => DateValue.ToString("u");

		public string ExtraInfo { get; set; }

		public bool Failed { get; set; }

		public Test Test { get; set; }

		[JsonIgnore]
		public TestResultUnit UnitValue { get; set; }

		public int Unit => (int)UnitValue;

		public int Timing { get; set; }
	}

	public class Agent
	{
		public string Name { get; set; }
	}

	public class Test
	{
		public string Name { get; set; }
	}
}
