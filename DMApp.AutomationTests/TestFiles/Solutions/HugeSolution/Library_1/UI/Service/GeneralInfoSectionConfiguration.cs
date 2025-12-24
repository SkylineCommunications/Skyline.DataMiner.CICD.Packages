using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
using System.Collections.Generic;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	public class GeneralInfoSectionConfiguration : ISectionConfiguration
	{
		public GeneralInfoSectionConfiguration(UserInfo userInfo)
        {
			PrerollIsVisible = userInfo.IsMcrUser;
			PostrollIsVisible = userInfo.IsMcrUser;
        }

		[IsIsVisibleProperty]
		public bool IsVisible { get; set; } = true;
		
		[IsIsEnabledProperty]
		public bool IsEnabled { get; set; } = true;

		[IsIsEnabledProperty]
		public bool PrerollIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool PrerollIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool StartDateIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool StartDateIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool EndDateIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool EndDateIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool PostrollIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool PostrollIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool TimeZoneIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool TimeZoneIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool ServiceDefinitionTypeSelectionIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool ServiceDefinitionTypeSelectionIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool VisibilityRightsIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool VisibilityRightsIsEnabled { get; set; } = true;

		public string EventOwnerCompany { get; set; }

		public HashSet<string> UserCompanies { get; private set; } = new HashSet<string>();

		public int LabelSpan { get; internal set; } = 2;

		public int InputWidgetSpan { get; internal set; } = 2;

		public int InputWidgetColumn { get; internal set; } = 2;

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
