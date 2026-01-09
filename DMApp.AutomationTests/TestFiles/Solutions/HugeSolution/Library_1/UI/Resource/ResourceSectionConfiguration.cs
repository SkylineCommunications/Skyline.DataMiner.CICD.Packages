namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Resource
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	public class ResourceSectionConfiguration : ISectionConfiguration
	{
		public ResourceSectionConfiguration(Helpers helpers, Function function, UserInfo userInfo)
		{
			IsVisible = userInfo.CanSwapResources;

			ResourceDropdownAlwaysVisible = userInfo.IsMcrUser;

			if (function.Id == FunctionGuids.Satellite)
			{
				ResourceLabelText = function.Name;
				OccupiedResourceSelectionIsVisible = false;
				AutomaticCheckboxIsVisible = false;
			}
		}

		/// <summary>
		/// Gets or sets a boolean indicating if Resource selection should be visible.
		/// </summary>
		[IsIsVisibleProperty]
		public bool IsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool IsEnabled { get; set; } = true;

		public string ResourceLabelText { get; set; } = "Resource";

		public bool ResourceDropdownAlwaysVisible { get; set; } = false;

		/// <summary>
		/// Gets or sets a boolean indicating if Resource selection should be enabled.
		/// </summary>
		[IsIsEnabledProperty]
		public bool ResourceSelectionEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool OccupiedResourceSelectionIsVisible { get; set; } = true;

		[IsIsVisibleProperty]
		public bool AutomaticCheckboxIsVisible { get; set; } = true;

		public string ReasonForResourceSelectionBeingDisabled { get; set; }

		public int LabelSpan { get; internal set; } = 2;

		public int InputWidgetSpan { get; internal set; } = 2;

		public int InputWidgetColumn { get; internal set; } = 2;

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
