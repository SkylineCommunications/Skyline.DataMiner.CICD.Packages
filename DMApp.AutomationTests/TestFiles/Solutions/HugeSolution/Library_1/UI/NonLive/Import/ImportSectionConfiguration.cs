namespace Library_1.UI.NonLive.Import
{
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using System;
	using System.Collections.Generic;

	public class ImportSectionConfiguration : ISectionConfiguration
	{
		public ImportSectionConfiguration(UserInfo userInfo)
		{
			if (userInfo == null) throw new ArgumentNullException(nameof(userInfo));

			IsilonBackupFileLocationIsVisible = userInfo.IsMcrUser;
			IsilonBackupFileLocationIsEnabled = userInfo.IsMcrUser;
		}

		[IsIsVisibleProperty]
		public bool IsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool IsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool IsilonBackupFileLocationIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool IsilonBackupFileLocationIsEnabled { get; set; } = true;


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
