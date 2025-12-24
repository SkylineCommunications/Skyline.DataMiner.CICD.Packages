namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries
{
	using System.Collections.Generic;

	public abstract class ChangeSummary
	{
		public abstract bool IsChanged { get; }

		public bool TryAddChangeSummaries(List<ChangeSummary> changeSummariesToAdd)
		{
			bool success = true;

			foreach (var changeSummaryToAdd in changeSummariesToAdd)
			{
				success &= TryAddChangeSummary(changeSummaryToAdd);
			}

			return success;
		}

		public abstract bool TryAddChangeSummary(ChangeSummary changeSummaryToAdd);

		public abstract override string ToString();
	}
}
