namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;

	public static class Agents
    {
		public enum DataMinerSystem
		{
			Internal,
			Staging,
			Production,
		}

        public static readonly IReadOnlyDictionary<int, string> AgentIDs = new Dictionary<int, string> {
            { 665, "slc-h42-g06.skyline.local" },
            { 127601, "dataminer-test.yleisradio.fi" },
            { 127602, "dataminer-test.yleisradio.fi" },
            { 127609, "dataminer-test.yleisradio.fi" },
            { 127603, "dataminer.ylead.fi" },
            { 127604, "dataminer.ylead.fi" },
            { 127608, "dataminer.ylead.fi" },
        };

		public const int InternalSetupAgentId = 665;

		public static readonly IReadOnlyList<int> StagingAgentIds = new List<int> { 127601, 127602, 127609 };

		public static readonly IReadOnlyList<int> ProductionAgentIds = new List<int> { 127603, 127604, 127608 };

		public static DataMinerSystem CurrentSystem
		{
			get
			{
				int agentId = Engine.SLNetRaw.ServerDetails.AgentID;

				if (agentId == InternalSetupAgentId)
				{
					return DataMinerSystem.Internal;
				}
				else if (IsStaging(agentId))
				{
					return DataMinerSystem.Staging;
				}
				else if (IsProduction(agentId))
				{
					return DataMinerSystem.Production;
				}
				else throw new InvalidOperationException($"Unknown Agent ID: '{agentId}'");
			}
		} 

		public static bool IsStaging(int agentId)
		{
			return StagingAgentIds.Contains(agentId);
		}

		public static bool IsProduction(int agentId)
		{
			return ProductionAgentIds.Contains(agentId);
		}
	}
}
