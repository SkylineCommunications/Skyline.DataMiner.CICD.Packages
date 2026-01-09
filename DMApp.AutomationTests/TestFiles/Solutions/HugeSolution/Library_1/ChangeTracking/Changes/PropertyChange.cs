namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries;

	public class PropertyChange : Change
	{
		private readonly PropertyChangeSummary summary = new PropertyChangeSummary();

		public PropertyChange()
		{

		}

		public PropertyChange(string propertyName, string oldValue, string newValue)
		{
			PropertyName = propertyName;

			Change = new ValueChange(oldValue, newValue);

			if (oldValue != newValue)
			{
				summary.MarkIsChanged();
			}
		}

		public string PropertyName { get; set; }

		public ValueChange Change { get; set; } = new ValueChange();

		[JsonIgnore]
		public override ChangeSummary Summary => summary;

		public override Change GetActualChanges()
		{
			if(!Summary.IsChanged) return null;

			return this;
		}

		public override bool TryAddChange(Change changeToAdd)
		{
			return false;
		}
	}
}
