namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class NewsInformationSectionConfiguration : ISectionConfiguration
    {
        public NewsInformationSectionConfiguration(UserInfo userInfo)
        {
            if (userInfo == null) throw new ArgumentNullException(nameof(userInfo));

            IsVisible = userInfo.IsNewsUser;
        }

        [IsIsVisibleProperty]
        public bool IsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool IsEnabled { get; set; } = true;

        public bool IsCollapsed { get; set; } = true;

        [IsIsEnabledProperty]
        public bool CollapseButtonsAreEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool NewsCameraOperatorIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool NewsCameraOperatorIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool JournalistIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool JournalistIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool VirveCommandGroupOneIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool VivreCommandGroupOneIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool VivreCommandGroupTwoIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool VivreCommandGroupTwoIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool AdditionalInformationIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool AdditionalInformationIsEnabled { get; set; } = true;

		[JsonIgnore]
		public Dictionary<string, string> ToolTip { get; set; } = ReflectionHandler.ReadTooltipFile();

        public void SetIsVisiblePropertyValues(bool valueToSet)
        {
            ConfigurationHelper.SetIsVisiblePropertyValues(this, valueToSet);
        }

        public void SetIsEnabledPropertyValues(bool valueToSet)
        {
            ConfigurationHelper.SetIsEnabledPropertyValues(this, valueToSet);
        }
    }
}
