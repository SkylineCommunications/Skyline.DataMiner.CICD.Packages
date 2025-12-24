namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ProfileParameters
{
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;

	public class ProfileParameterSectionConfiguration : ISectionConfiguration
	{
		[IsIsVisibleProperty]
		public bool IsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool IsEnabled { get; set; } = true;

		public List<string> DisallowedValues { get; set; } = new List<string>();

		public int LabelSpan { get; internal set; } = 2;

		public int InputWidgetSpan { get; internal set; } = 2;

		public int InputWidgetColumn { get; internal set; } = 2;

		[JsonIgnore]
		public Dictionary<string, string> ToolTip { get; set; } = ReflectionHandler.ReadTooltipFile();

		public void SetIsEnabledPropertyValues(bool valueToSet)
		{
			ConfigurationHelper.SetIsEnabledPropertyValues(this, valueToSet);
		}

		public void SetIsVisiblePropertyValues(bool valueToSet)
		{
			ConfigurationHelper.SetIsVisiblePropertyValues(this, valueToSet);
		}
	}
}