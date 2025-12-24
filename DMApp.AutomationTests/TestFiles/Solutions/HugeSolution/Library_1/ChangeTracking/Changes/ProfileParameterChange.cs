namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;
	using Newtonsoft.Json;

	public class ProfileParameterChange : Change
	{
		private readonly ProfileParameterChangeSummary summary = new ProfileParameterChangeSummary();

		public ProfileParameterChange()
		{

		}

		public ProfileParameterChange(ProfileParameter profileParameter, object oldValue, object newValue)
		{
			ProfileParameterName = profileParameter.Name;
			ProfileParameterId = profileParameter.Id;

			Change = new ValueChange(oldValue?.ToString() ?? string.Empty, newValue?.ToString() ?? string.Empty);

			if (Change.OldValue != Change.NewValue)
			{
				summary.MarkChanges(profileParameter, oldValue, newValue);
			}
		}

		public string ProfileParameterName { get; set; }
		
		public Guid ProfileParameterId { get; set; }

		public ValueChange Change { get; set; } = new ValueChange();

		[JsonIgnore]
		public override ChangeSummary Summary => summary;

		public override Change GetActualChanges()
		{
			if (!Summary.IsChanged) return null;

			return this;
		}

		public override bool TryAddChange(Change changeToAdd)
		{
			return false;
		}
	}
}
