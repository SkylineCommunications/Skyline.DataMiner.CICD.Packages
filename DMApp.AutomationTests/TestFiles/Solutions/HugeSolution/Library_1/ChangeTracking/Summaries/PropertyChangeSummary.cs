using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries
{
	public class PropertyChangeSummary : ChangeSummary
	{
		private bool isChanged;

		public override bool IsChanged => isChanged;

		public bool RecordingConfigurationChanged { get; set; }

		public void MarkIsChanged()
		{
			isChanged = true;
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		public override bool TryAddChangeSummary(ChangeSummary changeSummaryToAdd)
		{
			if (changeSummaryToAdd is PropertyChangeSummary propertyChangeSummaryToAdd)
			{
				isChanged |= propertyChangeSummaryToAdd.IsChanged;
				return true;
			}
			else if(changeSummaryToAdd is CollectionChangesSummary collectionChangesSummaryToAdd)
			{
				isChanged |= collectionChangesSummaryToAdd.IsChanged;
				return true;
			}

			return false;
		}
	}
}
