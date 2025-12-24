namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics.Scripts
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Newtonsoft.Json;

	public abstract class ScriptIdentifier
	{
		[JsonProperty(Order = -5)]
		public string ScriptName { get; set; }

		public bool IsSameScriptAs(ScriptIdentifier second)
		{
			if (second == null) return false;

			return ScriptName == second.ScriptName;
		}

		protected void SetScriptIdentifier(ScriptIdentifier identifier)
		{
			this.ScriptName = identifier.ScriptName;
		}
	}
}
