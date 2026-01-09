namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;

	public class TimingChangeSummary : ChangeSummary
	{
		[Flags]
		private enum TypesOfTimingChanges
		{
			None = 0,
			StartTimingChanged = 1,
			EndTimingChanged = 2,
			PrerollHasChanged = 3,
			PostrollHasChanged = 4,
		}

		private TypesOfTimingChanges typesOfChanges;

		public override bool IsChanged => typesOfChanges != TypesOfTimingChanges.None;

		public bool StartTimingChanged => typesOfChanges.HasFlag(TypesOfTimingChanges.StartTimingChanged);

		public bool EndTimingChanged => typesOfChanges.HasFlag(TypesOfTimingChanges.EndTimingChanged);

		public bool PrerollChanged => typesOfChanges.HasFlag(TypesOfTimingChanges.PrerollHasChanged);
		
		public bool PostrollChanged => typesOfChanges.HasFlag(TypesOfTimingChanges.PostrollHasChanged);

		public override bool TryAddChangeSummary(ChangeSummary changeSummaryToAdd)
		{
			if (changeSummaryToAdd is TimingChangeSummary timingChangeSummaryToAdd)
			{
				typesOfChanges |= timingChangeSummaryToAdd.typesOfChanges;
				return true;
			}

			return false;
		}

		public void MarkStartTimingChanged()
		{
			typesOfChanges |= TypesOfTimingChanges.StartTimingChanged;
		}

		public void MarkEndTimingChanged()
		{
			typesOfChanges |= TypesOfTimingChanges.EndTimingChanged;
		}

		public void MarkPrerollChanged()
		{
			typesOfChanges |= TypesOfTimingChanges.PrerollHasChanged;
		}

		public void MarkPostrollChanged()
		{
			typesOfChanges |= TypesOfTimingChanges.PostrollHasChanged;
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
