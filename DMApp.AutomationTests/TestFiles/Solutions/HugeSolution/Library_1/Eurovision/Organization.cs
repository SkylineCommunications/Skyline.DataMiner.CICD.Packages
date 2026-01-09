namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class Organization
	{
		public string Code { get; set; }

		public string Name { get; set; }

		public BroadcastCenter[] BroadcastCenters { get; set; }

		public bool SupportsUNI { get; set; }

		public bool SupportsOSS { get; set; }

		public string DisplayName { get { return String.Format("{0} ({1})", Name, Code); } }

		public void UpdateBroadcastCenters(IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 8000, new string[] { String.Format("fullFilter=8007 == '{0}';forcefulltable=true", Code) });
			if (columns == null || columns.Count < 12) return;

			var broadcastCenterIds = columns[0];
			var broadcastCenterCodes = columns[1];
			var broadcastCenterNames = columns[2];
			var broadcastCenterCityCodes = columns[7];
			var broadcastCenterSupportsUNI = columns[10];
			var broadcastCenterSupportsOSSUNI = columns[11];

			BroadcastCenters = new BroadcastCenter[broadcastCenterIds.Length];
			for (int i = 0; i < broadcastCenterIds.Length; i++)
			{
				BroadcastCenters[i] = new BroadcastCenter
				{
					Id = broadcastCenterIds[i],
					Code = broadcastCenterCodes[i],
					Name = broadcastCenterNames[i],
					City = new City(
						(string)eurovision.GetParameterByPrimaryKey(5011, broadcastCenterCityCodes[i]),
						(string)eurovision.GetParameterByPrimaryKey(5012, broadcastCenterCityCodes[i]),
						(string)eurovision.GetParameterByPrimaryKey(5013, broadcastCenterCityCodes[i])),
					SupportsUNI = broadcastCenterSupportsUNI[i] == "1",
					SupportsOSSUNI = broadcastCenterSupportsOSSUNI[i] == "1"
				};
			}
		}
	}
}