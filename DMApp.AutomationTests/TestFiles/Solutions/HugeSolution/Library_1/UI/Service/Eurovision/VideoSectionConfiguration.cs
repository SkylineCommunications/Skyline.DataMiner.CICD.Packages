namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service.Eurovision
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Type = YLE.Integrations.Eurovision.Type;

    public class VideoSectionConfiguration : ISectionConfiguration
    {
        public VideoSectionConfiguration(Type type)
        {
            switch (type)
            {
                case Type.NewsEvent:
                case Type.OSSTransmission:
                case Type.UnilateralTransmission:
                    BandWidthIsVisible = false;
                    BitrateIsVisible = true;
                    return;
                case Type.SatelliteCapacity:
                    BandWidthIsVisible = true;
                    BitrateIsVisible = false;
                    return;
                default:
                    // Nothing to set
                    return;
            }
        }

        public int LabelSpan { get; set; } = 2;

        public int InputWidgetSpan { get; set; } = 2;

        public int InputWidgetColumn { get; set; } = 2;

        [IsIsVisibleProperty]
        public bool BandWidthIsVisible { get; set; } = true;

        [IsIsVisibleProperty]
        public bool BitrateIsVisible { get; set; } = true;

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
