namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.ProfileParameters
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
    using System;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using System.Collections.Generic;
	using Newtonsoft.Json;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	public class AudioChannelConfigurationSectionConfiguration : ISectionConfiguration
	{
		public AudioChannelConfigurationSectionConfiguration(Service service, UserInfo userInfo)
		{
			if (userInfo == null) throw new ArgumentNullException(nameof(userInfo));

			IsVisible = service.Functions.SelectMany(f => f.Parameters).Any(p => ProfileParameterGuids.AllAudioProcessingRequiredGuids.Contains(p.Id)) && service.Definition.VirtualPlatform != VirtualPlatform.AudioProcessing;
			CopyFromSourceCheckBoxIsVisible = service.Definition.VirtualPlatformServiceType != VirtualPlatformType.Reception;
			AudioDeembeddingRequiredDropDownIsVisible = userInfo.IsMcrUser;
			AudioShufflingRequiredDropDownIsVisible = userInfo.IsMcrUser;
			AudioEmbeddingRequiredDropDownIsVisible = userInfo.IsMcrUser;
			CopyFromSourceCheckBoxIsEnabled = userInfo.Contract?.IsAudioProcessingAllowed() ?? false;

			AudioChannelPairSectionConfiguration = new AudioChannelPairSectionConfiguration();

			if (service.Definition.VirtualPlatformServiceName == VirtualPlatformName.Satellite)
			{
				SectionDividerCharacter = '░';
			}
		}

		[IsIsVisibleProperty]
		public bool IsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool IsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool CopyFromSourceCheckBoxIsVisible { get; set; } = true;

		[IsIsEnabledProperty]
		public bool CopyFromSourceCheckBoxIsEnabled { get; set; } = true;

		[IsIsVisibleProperty]
		public bool AudioDeembeddingRequiredDropDownIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool AudioShufflingRequiredDropDownIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool AudioEmbeddingRequiredDropDownIsVisible { get; set; }

		public AudioChannelPairSectionConfiguration AudioChannelPairSectionConfiguration { get; set; }

		public char SectionDividerCharacter { get; set; }

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