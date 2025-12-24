namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public class ServiceSelectionSectionConfiguration : ISectionConfiguration
    {
        public ServiceSelectionSectionConfiguration(Helpers helpers, Service service, string eventOwnerCompany, bool isReadOnly, UserInfo userInfo, EditOrderFlows flow)
        {
            DeleteButtonIsVisible = service.BackupType == BackupType.None && service.IntegrationType == IntegrationType.None;
            VirtualPlatformDropDownIsEnabled = service.IntegrationType == IntegrationType.None;
            ServiceDefinitionDescriptionDropDownIsEnabled = service.IntegrationType == IntegrationType.None;

            var userCompanies = userInfo.UserGroups?.Select(userGroup => userGroup.Company).ToList();

			switch (flow)
			{
				case EditOrderFlows.ChangeResourcesForService:
				case EditOrderFlows.ChangeResourcesForService_FromRecordingApp:
					ServiceSectionConfiguration = ServiceSectionConfiguration.CreateConfigurationForResourceSelectionOnly(helpers, service, userInfo, !isReadOnly, eventOwnerCompany, userCompanies);
					break;
				case EditOrderFlows.EditTimingForService_FromRecordingApp:
					ServiceSectionConfiguration = ServiceSectionConfiguration.CreateConfigurationForTimingSelectionOnly(helpers, service, userInfo, !isReadOnly, eventOwnerCompany, userCompanies);
					break;

				default:
					ServiceSectionConfiguration = ServiceSectionConfiguration.CreateLiveOrderFormConfiguration(helpers, service, userInfo, eventOwnerCompany, userCompanies);
					break;
			}

			switch (service.Definition.VirtualPlatform)
			{
				case VirtualPlatform.VizremStudio:
				case VirtualPlatform.VizremFarm:
                    ServiceDefinitionDescriptionLabelIsVisible = false;
                    CollapseButtonIsVisible = false;
                    TitleIsVisible = false;
                    DeleteButtonIsVisible = false;
                    LabelWidgetColumn = 0;
                    InputWidgetColumn = 1;
					break;

				case VirtualPlatform.Routing:
				case VirtualPlatform.VideoProcessing:
				case VirtualPlatform.AudioProcessing:
				case VirtualPlatform.GraphicsProcessing:
					ServiceSectionConfiguration.AdjustDetailsIsVisible = false;
					ServiceSectionConfiguration.GeneralInfoSectionConfiguration.IsVisible = false;
					DeleteButtonIsVisible = false;
					break;
				default:
					break;
			}
		}

        [IsIsVisibleProperty]
        public bool IsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool IsEnabled { get; set; } = true;

        public bool CollapseButtonIsVisible { get; set; } = true;

        public bool TitleIsVisible { get; set; } = true;

        public bool DisplayServiceTimingsInTitle { get; set; } = false;

        [IsIsVisibleProperty]
        public bool VirtualPlatformDropDownIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool VirtualPlatformDropDownIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool ServiceDefinitionDescriptionLabelIsVisible { get; set; } = true;

        [IsIsVisibleProperty]
        public bool ServiceDefinitionDescriptionDropDownIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool ServiceDefinitionDescriptionDropDownIsEnabled { get; set; } = true;

        [IsIsVisibleProperty]
        public bool DeleteButtonIsVisible { get; set; } = true;

        [IsIsEnabledProperty]
        public bool DeleteButtonIsEnabled { get; set; } = true;

        public bool IsCollapsed { get; set; } = false;

        public ServiceSectionConfiguration ServiceSectionConfiguration { get; private set; }

        public string EventOwnerCompany => ServiceSectionConfiguration?.GeneralInfoSectionConfiguration?.EventOwnerCompany;

        [JsonIgnore]
        public Dictionary<string, string> ToolTip { get; set; } = ReflectionHandler.ReadTooltipFile();

        public int LabelWidgetColumn { get; set; } = 1;
       
        public int InputWidgetColumn { get; set; } = 2;

        public void DisableForIntegration(IntegrationType integrationType, BackupType backupType)
		{
            ServiceSectionConfiguration.DisableForIntegration(integrationType, backupType);
		}

        public void SetIsEnabledPropertyValues(bool valueToSet)
        {
            ConfigurationHelper.SetIsEnabledPropertyValues(this, valueToSet);
        }

        public void SetIsVisiblePropertyValues(bool valueToSet)
        {
            ConfigurationHelper.SetIsVisiblePropertyValues(this, valueToSet);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
