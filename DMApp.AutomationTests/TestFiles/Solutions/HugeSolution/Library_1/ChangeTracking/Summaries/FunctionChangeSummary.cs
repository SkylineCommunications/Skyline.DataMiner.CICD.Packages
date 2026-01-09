using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries
{
	public class FunctionChangeSummary : ChangeSummary
	{
		public override bool IsChanged => IsNew || ProfileParameterChangeSummary.IsChanged || ResourceChangeSummary.IsChanged;

		public bool IsNew { get; set; }

		public ProfileParameterChangeSummary ProfileParameterChangeSummary { get; } = new ProfileParameterChangeSummary();

		public ResourceChangeSummary ResourceChangeSummary { get; } = new ResourceChangeSummary();

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		public override bool TryAddChangeSummary(ChangeSummary changeSummaryToAdd)
		{
			if (changeSummaryToAdd is FunctionChangeSummary functionChangeSummaryToAdd)
			{
				IsNew &= functionChangeSummaryToAdd.IsNew;
				ResourceChangeSummary.TryAddChangeSummary(functionChangeSummaryToAdd.ResourceChangeSummary);
				ProfileParameterChangeSummary.TryAddChangeSummary(functionChangeSummaryToAdd.ProfileParameterChangeSummary);
				return true;
			}
			else if(changeSummaryToAdd is ResourceChangeSummary resourceChangeSummaryToAdd)
			{
				ResourceChangeSummary.TryAddChangeSummary(resourceChangeSummaryToAdd);
				return true;
			}
			else if (changeSummaryToAdd is ProfileParameterChangeSummary profileParameterChangeSummaryToAdd)
			{
				ProfileParameterChangeSummary.TryAddChangeSummary(profileParameterChangeSummaryToAdd);
				return true;
			}

			return false;
		}
	}
}
