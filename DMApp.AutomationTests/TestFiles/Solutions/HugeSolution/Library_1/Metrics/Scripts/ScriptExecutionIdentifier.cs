namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Scripts
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public abstract class ScriptExecutionIdentifier : ScriptIdentifier
	{
		[JsonProperty(Order = -2)]
		public int DmaId { get; set; }

		[JsonProperty(Order = -3)]
		public string UserLoginName { get; set; }

		[JsonProperty(Order = -4)]
		public string UserDisplayName { get; set; }

		public DateTime? StartTime { get; set; }

		public bool IsSameScriptAndHasRunOnSameDmaAs(ScriptExecutionIdentifier other)
		{
			if (other == null) return false;

			return IsSameScriptAs(other) && HasRunOnSameDmaAs(other);
		}

		public bool HasRunOnSameDmaAs(ScriptExecutionIdentifier second)
		{
			if(second == null) return false;

			return DmaId == second.DmaId;
		}

		public ScriptExecutionIdentifier GetScriptExectionIdentifier()
		{
			return this;
		}

		protected void SetScriptExecutionIdentifier(ScriptExecutionIdentifier identifier)
		{
			if (identifier is null) throw new ArgumentNullException(nameof(identifier));

			SetScriptIdentifier(identifier);

			this.DmaId = identifier.DmaId;
		}
	}
}
