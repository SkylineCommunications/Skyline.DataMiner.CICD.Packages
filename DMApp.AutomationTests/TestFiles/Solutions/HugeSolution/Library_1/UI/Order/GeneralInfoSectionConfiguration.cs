namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.Utils.YLE.Integrations;
	using Newtonsoft.Json;

    public class GeneralInfoSectionConfiguration : ISectionConfiguration
    {
        public GeneralInfoSectionConfiguration(Order order, Event @event, UserInfo userInfo)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));

			switch (order.Subtype)
			{
				case OrderSubType.Normal:
                    UserGroupIsVisible = userInfo.IsMcrUser;
                    IntegrationTypeIsVisible = order.IntegrationType != IntegrationType.None;
                    OrderNameIsEnabled = order.IntegrationType == IntegrationType.None;
                    StartDateIsEnabled = order.IntegrationType == IntegrationType.None && !order.ShouldBeRunning;
                    StartNowIsEnabled = order.IsSaved || (!order.ShouldBeRunning && DateTime.Now < order.Start);
                    EndDateIsEnabled = order.IntegrationType == IntegrationType.None;
                    PlasmaIdIsEnabled = order.IntegrationType == IntegrationType.None;
                    CustomerCompanyIsVisible = !UserInfo.IsInternalUser || UserInfo.IsMcrUser;
                    BillableCompanyIsVisible = !UserInfo.IsInternalUser || UserInfo.IsMcrUser;
                    LabelColumn = 0;
                    LabelSpan = 4;
                    InputColumn = 4;
                    InputSpan = 3;
                    break;
				case OrderSubType.Vizrem:
                    EditOrderInformationIsVisible = false;
                    IntegrationTypeIsVisible = false;
                    StartNowIsVisible = false;
                    PlasmaIdIsVisible = false;
                    UserGroupIsVisible = false;
                    YleIdIsVisible = false;
                    VisibilityRightsAreVisible = false;
                    BillableCompanyIsVisible = false;
                    CustomerCompanyIsVisible = false;
                    LabelColumn = 0;
                    LabelSpan = 1;
                    InputColumn = 1;
                    InputSpan = 3;
                    break;
				default:
					break;
			}

            InitPermanentlySelectedCompanies(order, @event, userInfo);
            InitVisibleViewIds(@event, userInfo);
        }

        private void InitPermanentlySelectedCompanies(Order order, Event @event, UserInfo userInfo)
        {
            var permanentlySelectedCompanies = new HashSet<string>();
            if (!order.IsBooked) foreach(string company in userInfo.UserGroups.Select(userGroup => userGroup.Company)) permanentlySelectedCompanies.Add(company);
            if (@event != null) permanentlySelectedCompanies.Add(@event.CompanyOfCreator);

            if (order.IsBooked && order.IsCreatedFromTemplate)
            {
                foreach (int securityViewId in order.SecurityViewIds)
                {
                    var matchingUsergroup = userInfo.UserGroups.FirstOrDefault(x => x.CompanySecurityViewId == securityViewId);
                    if (matchingUsergroup != null) permanentlySelectedCompanies.Add(matchingUsergroup.Company);
                }
            }

            PermanentlySelectedCompanies = permanentlySelectedCompanies;
            foreach (string permanentlySelectedCompany in permanentlySelectedCompanies) PermanentlySelectedCompanies.Add(permanentlySelectedCompany);
        }

        private void InitVisibleViewIds(Event @event, UserInfo userInfo)
        {
            VisibleViewIds = @event != null ? @event.SecurityViewIds : new HashSet<int>(userInfo.Contract.LinkedCompanies.Select(x => x.SecurityViewId));
        }

		public void SetIsVisiblePropertyValues(bool valueToSet)
		{
            ConfigurationHelper.SetIsVisiblePropertyValues(this, valueToSet);
        }

		public void SetIsEnabledPropertyValues(bool valueToSet)
		{
            ConfigurationHelper.SetIsEnabledPropertyValues(this, valueToSet);
        }

        [IsIsVisibleProperty]
        public bool IsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool IsEnabled { get; set; } = true;

        [IsIsEnabledProperty]
        public bool StartDateIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool StartDateIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool StartNowIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool StartNowIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool EndDateIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool EndDateIsVisible { get; set; } = true;

        [IsIsVisibleProperty]
        public bool TimeZoneIsVisible { get; set; } = true;

        [IsIsVisibleProperty]
        public bool VisibilityRightsAreVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool VisibilityRightsAreEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool RecurrenceIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool RecurrenceIsEnabled { get; set; } = true;

        [IsIsEnabledProperty]
        public bool CustomerCompanyIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool CustomerCompanyIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool BillableCompanyIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool BillableCompanyIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool YleIdIsEnabled { get; set; } = false;

        [IsIsVisibleProperty]
        public bool YleIdIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool PlasmaIdIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool PlasmaIdIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool UserGroupIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool UserGroupIsVisible { get; set; } = true;

        [IsIsVisibleProperty]
        public bool IntegrationTypeIsVisible { get; set; } = true;

        [IsIsVisibleProperty]
        public bool OrderNameIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool OrderNameIsEnabled { get; set; } = true;

        [IsIsEnabledProperty]
        public bool EditOrderInformationIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool EditOrderInformationIsVisible { get; set; } = true;

		// Required for Visibility Rights
		[JsonIgnore]
        public UserInfo UserInfo { get; private set; }

        // Required for Visibility Rights
        public HashSet<string> PermanentlySelectedCompanies { get; private set; } = new HashSet<string>();

        // Required for Visibility Rights
        public HashSet<int> VisibleViewIds { get; private set; } = new HashSet<int>();

        public int LabelColumn { get; set; }

        public int LabelSpan { get; set; }

        public int InputColumn { get; set; }

        public int InputSpan { get; set; }

		[JsonIgnore]
		public Dictionary<string, string> ToolTip { get; set; } = ReflectionHandler.ReadTooltipFile();
    }
}