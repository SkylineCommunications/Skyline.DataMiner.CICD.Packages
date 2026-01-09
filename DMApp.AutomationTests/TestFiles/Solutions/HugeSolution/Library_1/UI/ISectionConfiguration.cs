namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI
{
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public interface ISectionConfiguration
	{
		bool IsVisible { get; set; }

		bool IsEnabled { get; set; }

		[JsonIgnore]
		Dictionary<string, string> ToolTip { get; set; }

		void SetIsVisiblePropertyValues(bool valueToSet);
		
		void SetIsEnabledPropertyValues(bool valueToSet);
	}
}
