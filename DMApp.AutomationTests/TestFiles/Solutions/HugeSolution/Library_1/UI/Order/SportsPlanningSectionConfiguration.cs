namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
    using System;
    using System.Collections.Generic;

    public class SportsPlanningSectionConfiguration : ISectionConfiguration
    {
        public SportsPlanningSectionConfiguration(UserInfo userInfo)
        {
            if (userInfo == null) throw new ArgumentNullException(nameof(userInfo));

            IsVisible = userInfo.IsSportUser;
        }

        [IsIsVisibleProperty]
        public bool IsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool IsEnabled { get; set; } = true;

        [IsIsEnabledProperty]
        public bool CollapseButtonsAreEnabled { get; set; } = true;

        public bool IsCollapsed { get; set; } = true;

        [IsIsVisibleProperty]
        public bool SportIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool SportIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool DescriptionIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool DescriptionIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool CommentaryIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool CommentaryIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool CommentaryTwoIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool CommentaryTwoIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool CompetitionTimeIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool CompetitionTimeIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool JournalistOneIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool JournalistOneIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool JournalistTwoIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool JournalistTwoIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool JournalistThreeIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool JournalistThreeIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool LocationIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool LocationIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool TechnicalResourcesIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool TechnicalResourcesIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool LiveHighlightsFileIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool LiveHighlightsFileIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool RequestedBroadcastTimeIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool RequestedBroadcastTimeIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool ProductionNumberPlasmaIdIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool ProductionNumberPlasmaIdIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool ProductNumberCeitonIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool ProductNumberCeitonIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool CostDepartmentIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool CostDepartmentIsEnabled { get; set; } = true;

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
