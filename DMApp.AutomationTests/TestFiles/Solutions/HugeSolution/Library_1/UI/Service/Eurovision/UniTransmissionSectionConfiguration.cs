using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
using System.Collections.Generic;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service.Eurovision
{
    public class UniTransmissionSectionConfiguration : ISectionConfiguration
    {
        public int LabelSpan { get; set; } = 2;

        public int InputWidgetSpan { get; set; } = 2;

        public int InputWidgetColumn { get; set; } = 2;

        public AudioSectionConfiguration AudioSectionConfiguration { get; private set; } = new AudioSectionConfiguration();

        public VideoSectionConfiguration VideoSectionConfiguration { get; private set; } = new VideoSectionConfiguration(YLE.Integrations.Eurovision.Type.UnilateralTransmission);
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