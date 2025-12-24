namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service.Eurovision;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class EurovisionSectionConfiguration : ISectionConfiguration
    {
        public int LabelSpan { get; set; } = 2;

        public int InputWidgetSpan { get; set; } = 2;

        public int InputWidgetColumn { get; set; } = 2;

        public NewsEventSectionConfiguration NewsEventSectionConfiguration { get; private set; } = new NewsEventSectionConfiguration();

        public ProgramEventSectionConfiguration ProgramEventSectionConfiguration { get; private set; } = new ProgramEventSectionConfiguration();

        public SatelliteCapacitySectionConfiguration SatelliteCapacitySectionConfiguration { get; private set; } = new SatelliteCapacitySectionConfiguration();

        public UniTransmissionSectionConfiguration UnilateralTransmissionSectionConfiguration { get; private set; } = new UniTransmissionSectionConfiguration();

        public OssTransmissionSectionConfiguration OsslateralTransmissionSectionConfiguration { get; private set; } = new OssTransmissionSectionConfiguration();
        
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
