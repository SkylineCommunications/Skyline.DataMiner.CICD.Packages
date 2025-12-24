namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;

	public abstract class Change
	{
		public abstract ChangeSummary Summary { get; }

		public bool TryAddChanges(List<Change> changesToAdd)
		{
			bool success = true;

			foreach (var changeToAdd in changesToAdd)
			{
				success &= TryAddChange(changeToAdd);
			}

			return success;
		}

		public abstract bool TryAddChange(Change changeToAdd);

		public abstract Change GetActualChanges();

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
