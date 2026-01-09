namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class AdditionalInformationSectionConfiguration : ISectionConfiguration
    {
        public int LabelSpan { get; internal set; } = 2;

        public int InputWidgetSpan { get; internal set; } = 2;

        public int InputWidgetColumn { get; internal set; } = 2;

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
