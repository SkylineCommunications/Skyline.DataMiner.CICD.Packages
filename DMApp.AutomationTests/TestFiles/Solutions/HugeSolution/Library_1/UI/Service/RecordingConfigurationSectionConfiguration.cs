using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.AvidInterplayPAM;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	public class RecordingConfigurationSectionConfiguration : ISectionConfiguration
	{
		public RecordingConfigurationSectionConfiguration(bool isIntegrationService, string serviceDefinitionDescription = null)
		{
			IsVisible = true;
			IsEnabled = true;

			RecordingNameIsVisible = true;
			RecordingFileDestinationIsVisible = true;
			DeadlineForArchivingIsVisible = true;
			PlasmaIdForArchiveIsVisible = true;
			RecordingFileDestinationPathIsVisible = true;
			RecordingFileVideoResolutionIsVisible = true;
			RecordingFileVideoCodecIsVisible = true;
			RecordingFileTimeCodeIsVisible = true;
			SubtitleProxyIsVisible = true;
			ProxyFormatIsVisible = true;
			FastRerunCopyIsVisible = true;
			FastAreenaCopyIsVisible = isIntegrationService;
			BroadcastReadyIsVisible = true;
            VidigoRequiredIsVisible = true;

			if (!string.IsNullOrEmpty(serviceDefinitionDescription))
			{
				if (serviceDefinitionDescription.Contains("Live"))
				{
					RecordingFileDestinationPathIsVisible = false;
                    VidigoRequiredIsVisible = false;
                }
				else if (serviceDefinitionDescription.Contains("News"))
				{
					PlasmaIdForArchiveIsVisible = false;
					RecordingFileVideoResolutionIsVisible = false;
					RecordingFileVideoCodecIsVisible = false;
					RecordingFileTimeCodeIsVisible = false;
					SubtitleProxyIsVisible = false;
					ProxyFormatIsVisible = false;
					FastRerunCopyIsVisible = false;
					FastAreenaCopyIsVisible = false;
					BroadcastReadyIsVisible = false;
				}
			}
		} 

		[IsIsVisibleProperty]
		public bool IsVisible { get; set; }

		[IsIsEnabledProperty]
		public bool IsEnabled { get; set; }

		[IsIsVisibleProperty]
		public bool RecordingNameIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool PlasmaIdForArchiveIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool RecordingFileDestinationIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool RecordingFileDestinationPathIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool DeadlineForArchivingIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool RecordingFileVideoResolutionIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool RecordingFileVideoCodecIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool RecordingFileTimeCodeIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool SubtitleProxyIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool ProxyFormatIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool FastRerunCopyIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool FastAreenaCopyIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool BroadcastReadyIsVisible { get; set; }

		[IsIsVisibleProperty]
		public bool VidigoRequiredIsVisible { get; set; }

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
