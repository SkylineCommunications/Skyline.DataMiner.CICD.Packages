namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service.Eurovision
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class VideoSection : Section
	{
		private readonly VideoSectionConfiguration configuration;
		private readonly EurovisionBookingDetails details;
		private readonly Helpers helpers;

		private readonly Label videoTitle = new Label("VIDEO") { Style = TextStyle.Heading };

		private readonly Label videoDefinitionLabel = new Label("Video Definition");
		private readonly DropDown videoDefinitionDropDown = new DropDown();

		private readonly Label videoAspectRatioLabel = new Label("Video Aspect Ratio");
		private readonly DropDown videoAspectRatioDropDown = new DropDown();

		private readonly Label videoResolutionLabel = new Label("Video Resolution");
		private readonly DropDown videoResolutionDropDown = new DropDown();

		private readonly Label videoBitrateLabel = new Label("Video Bitrate");
		private readonly DropDown videoBitrateDropDown = new DropDown();

		private readonly Label videoBandwidthLabel = new Label("Video Bandwidth");
		private readonly DropDown videoBandwidthDropDown = new DropDown();

		private readonly Label videoFrameRateLabel = new Label("Video Frame Rate");
		private readonly DropDown videoFrameRateDropDown = new DropDown();

		public VideoSection(EurovisionBookingDetails details, VideoSectionConfiguration configuration, Helpers helpers = null)
		{
			this.details = details ?? throw new ArgumentNullException(nameof(details));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.helpers = helpers;

			Initialize();
			GenerateUI();
			HandleVisibilityAndEnabledUpdate();
		}

		public new bool IsVisible
		{
			get => base.IsVisible;

			set
			{
				base.IsVisible = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public new bool IsEnabled
		{
			get => base.IsEnabled;

			set
			{
				base.IsEnabled = value;
				HandleVisibilityAndEnabledUpdate();
			}
		}

		public event EventHandler<VideoDefinition> VideoDefinitionChanged;

		public event EventHandler<VideoResolution> VideoResolutionChanged;

		public event EventHandler<VideoAspectRatio> VideoAspectRatioChanged;

		public event EventHandler<VideoBitrate> VideoBitrateChanged;

		public event EventHandler<VideoFrameRate> VideoFrameRateChanged;

		public event EventHandler<VideoBandwidth> VideoBandwidthChanged;

		private VideoDefinition VideoDefinition => details.VideoDefinitions.FirstOrDefault(x => x.Code.Equals(videoDefinitionDropDown.Selected));

		private VideoResolution VideoResolution => details.VideoResolutions.FirstOrDefault(x => x.Code.Equals(videoResolutionDropDown.Selected));

		private VideoAspectRatio VideoAspectRatio => details.VideoAspectRatios.FirstOrDefault(x => x.Code.Equals(videoAspectRatioDropDown.Selected));

		private VideoBitrate VideoBitrate => details.VideoBitrates.FirstOrDefault(x => x.Name.Equals(videoBitrateDropDown.Selected));

		private VideoFrameRate VideoFrameRate => details.VideoFrameRates.FirstOrDefault(x => x.Name.Equals(videoFrameRateDropDown.Selected));

		private VideoBandwidth VideoBandwidth => details.VideoBandWidths.FirstOrDefault(x => x.Code.Equals(videoBandwidthDropDown.Selected));

		public void RegenerateUI()
		{
			Clear();
			GenerateUI();
			HandleVisibilityAndEnabledUpdate();
		}

		private void Initialize()
		{
			IntializeWidgets();
			SubscribeToWidgets();
			SubscribeToBookingDetails();
		}

		private void IntializeWidgets()
		{
			UpdateVideoDefinitionOptions();
			UpdateVideoAspectRatioOptions();
			UpdateVideoResolutionOptions();
			UpdateVideoBitrateOptions();
			UpdateVideoFrameRateOptions();
			UpdateVideoBandWidthOptions();

			SetSelectedVideoDefinition();
			SetSelectedVideoAspectRatio();
			SetSelectedVideoResolution();
			SetSelectedVideoBitrate();
			SetSelectedVideoFrameRate();
			SetSelectedVideoBandwidth();
		}

		private void SubscribeToWidgets()
		{
			videoDefinitionDropDown.Changed += (s, e) =>
			{
				HandleVisibilityAndEnabledUpdate();
				VideoDefinitionChanged?.Invoke(this, VideoDefinition);
			};

			videoAspectRatioDropDown.Changed += (s, e) => VideoAspectRatioChanged?.Invoke(this, VideoAspectRatio);
			videoResolutionDropDown.Changed += (s, e) => VideoResolutionChanged?.Invoke(this, VideoResolution);
			videoBitrateDropDown.Changed += (s, e) => VideoBitrateChanged?.Invoke(this, VideoBitrate);
			videoFrameRateDropDown.Changed += (s, e) => VideoFrameRateChanged?.Invoke(this, VideoFrameRate);
			videoBandwidthDropDown.Changed += (s, e) => VideoBandwidthChanged?.Invoke(this, VideoBandwidth);
		}

		private void SubscribeToBookingDetails()
		{
			details.VideoDefinitionsChanged += (s, e) =>
			{
				UpdateVideoDefinitionOptions();
				SetSelectedVideoDefinition();
				HandleVisibilityAndEnabledUpdate();
			};

			details.VideoAspectRatiosChanged += (s, e) =>
			{
				UpdateVideoAspectRatioOptions();
				SetSelectedVideoAspectRatio();
			};

			details.VideoResolutionsChanged += (s, e) =>
			{
				UpdateVideoResolutionOptions();
				SetSelectedVideoResolution();
			};

			details.VideoBitratesChanged += (s, e) =>
			{
				UpdateVideoBitrateOptions();
				SetSelectedVideoBitrate();
			};

			details.VideoFrameRatesChanged += (s, e) =>
			{
				UpdateVideoFrameRateOptions();
				SetSelectedVideoFrameRate();
			};

			details.VideoBandWidthsChanged += (s, e) =>
			{
				UpdateVideoBandWidthOptions();
				SetSelectedVideoBandwidth();
			};

			details.VideoDefinitionCodeChanged += (s, e) =>
			{
				SetSelectedVideoDefinition();
				HandleVisibilityAndEnabledUpdate();
			};

			details.VideoAspectRatioCodeChanged += (s, e) => SetSelectedVideoAspectRatio();
			details.VideoResolutionCodeChanged += (s, e) => SetSelectedVideoResolution();
			details.VideoBitrateCodeChanged += (s, e) => SetSelectedVideoBitrate();
			details.VideoFrameRateCodeChanged += (s, e) => SetSelectedVideoFrameRate();
			details.VideoBandwidthCodeChanged += (s, e) => SetSelectedVideoBandwidth();
		}

		private void UpdateVideoDefinitionOptions()
		{
			var videoDefinitionOptions = details.VideoDefinitions.Any() ? details.VideoDefinitions.OrderBy(d => d.Code).Select(d => d.Code).ToList() : new List<string> { "None" };
			videoDefinitionDropDown.SetOptions(videoDefinitionOptions);
		}

		private void SetSelectedVideoDefinition()
		{
			var videoDefinition = details.VideoDefinitions.FirstOrDefault(vd => vd.Code.Equals(details.VideoDefinitionCode));
			videoDefinitionDropDown.Selected = videoDefinition != null ? videoDefinition.Code : "None";
		}

		private void UpdateVideoAspectRatioOptions()
		{
			var videoAspectRatioOptions = details.VideoAspectRatios.Any() ? details.VideoAspectRatios.OrderBy(a => a.Code).Select(a => a.Code).ToList() : new List<string> { "None" };
			videoAspectRatioDropDown.SetOptions(videoAspectRatioOptions);
		}

		private void SetSelectedVideoAspectRatio()
		{
			var aspectRatio = details.VideoAspectRatios.FirstOrDefault(ar => ar.Code.Equals(details.VideoAspectRatioCode));
			videoAspectRatioDropDown.Selected = aspectRatio != null ? aspectRatio.Code : "None";
		}

		private void UpdateVideoResolutionOptions()
		{
			var videoResolutionOptions = details.VideoResolutions.Any() ? details.VideoResolutions.OrderBy(a => a.Code).Select(a => a.Code).ToList() : new List<string> { "None" };
			videoResolutionDropDown.SetOptions(videoResolutionOptions);
		}

		private void SetSelectedVideoResolution()
		{
			var videoResolution = details.VideoResolutions.FirstOrDefault(vr => vr.Code.Equals(details.VideoResolutionCode));
			videoResolutionDropDown.Selected = videoResolution != null ? videoResolution.Code : "None";
		}

		private void UpdateVideoBitrateOptions()
		{
			var videoBitrateOptions = details.VideoBitrates.Any() ? details.VideoBitrates.OrderBy(b => b.Name).Select(b => b.Name).ToList() : new List<string> { "None" };
			videoBitrateDropDown.SetOptions(videoBitrateOptions);
		}

		private void SetSelectedVideoBitrate()
		{
			var videoBitrate = details.VideoBitrates.FirstOrDefault(br => br.Code.Equals(details.VideoBitrateCode));
			videoBitrateDropDown.Selected = videoBitrate != null ? videoBitrate.Name : "None";
		}

		private void UpdateVideoFrameRateOptions()
		{
			var videoFrameRateOptions = details.VideoFrameRates.Any() ? details.VideoFrameRates.OrderBy(f => f.Name).Select(f => f.Name).ToList() : new List<string> { "None" };
			videoFrameRateDropDown.SetOptions(videoFrameRateOptions);
		}

		private void SetSelectedVideoFrameRate()
		{
			var videoFrameRate = details.VideoFrameRates.FirstOrDefault(fr => fr.Code.Equals(details.VideoFrameRateCode));
			videoFrameRateDropDown.Selected = videoFrameRate != null ? videoFrameRate.Name : "None";
		}

		private void UpdateVideoBandWidthOptions()
		{
			var videoBandwidthOptions = details.VideoBandWidths.Any() ? details.VideoBandWidths.OrderBy(b => b.Code).Select(b => b.Code).ToList() : new List<string> { "None" };
			videoBandwidthDropDown.SetOptions(videoBandwidthOptions);
		}

		private void SetSelectedVideoBandwidth()
		{
			var bandwidth = details.VideoBandWidths.FirstOrDefault(bw => bw.Code.Equals(details.VideoBandwidthCode));
			videoBandwidthDropDown.Selected = bandwidth != null ? bandwidth.Code : "None";
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(videoTitle, ++row, 0, 1, configuration.LabelSpan);

			AddWidget(videoDefinitionLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(videoDefinitionDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(videoAspectRatioLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(videoAspectRatioDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(videoResolutionLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(videoResolutionDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(videoBitrateLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(videoBitrateDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(videoBandwidthLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(videoBandwidthDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(videoFrameRateLabel, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(videoFrameRateDropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		private void HandleVisibilityAndEnabledUpdate()
		{
			videoResolutionLabel.IsVisible = IsVisible && details.VideoDefinitionCode != "SD";
			videoResolutionDropDown.IsVisible = IsVisible && details.VideoDefinitionCode != "SD";

			videoAspectRatioLabel.IsVisible = IsVisible && details.VideoDefinitionCode == "SD";
			videoAspectRatioDropDown.IsVisible = IsVisible && details.VideoDefinitionCode == "SD";

			videoBitrateLabel.IsVisible = IsVisible && configuration.BitrateIsVisible;
			videoBitrateDropDown.IsVisible = IsVisible && configuration.BitrateIsVisible;

			videoBandwidthLabel.IsVisible = IsVisible && configuration.BandWidthIsVisible;
			videoBandwidthDropDown.IsVisible = IsVisible && configuration.BandWidthIsVisible;

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}
