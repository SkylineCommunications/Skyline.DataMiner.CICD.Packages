namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
	using Newtonsoft.Json;

    public class AdditionalInformationSectionConfiguration : ISectionConfiguration
    {
        public AdditionalInformationSectionConfiguration(Order order, UserInfo userInfo)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (userInfo == null) throw new ArgumentNullException(nameof(userInfo));

            ErrorDescriptionIsVisible = userInfo.IsMcrUser && order.Status == Status.CompletedWithErrors;
            ReasonForBeingCancelledOrRejectedIsVisible = order.Status == Status.Cancelled || order.Status == Status.Rejected;
        }

        [IsIsVisibleProperty]
        public bool IsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool IsEnabled { get; set; } = true;

        [IsIsEnabledProperty]
        public bool CollapseButtonsAreEnabled { get; set; } = true;

        public bool IsCollapsed { get; set; } = true;

        [IsIsVisibleProperty]
        public bool CommentsAreVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool CommentsAreEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool McrOperatorNotesAreVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool McrOperatorNotesAreEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool MediaOperatorNotesAreVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool MediaOperatorNotesAreEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool ErrorDescriptionIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool ErrorDescriptionIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool ReasonForBeingCancelledOrRejectedIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool ReasonForBeingCancelledOrRejectedIsEnabled { get; set; } = true;

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
