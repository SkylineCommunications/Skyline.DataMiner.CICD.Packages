namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class NonLiveOrderConfiguration : ISectionConfiguration
    {
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;

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
