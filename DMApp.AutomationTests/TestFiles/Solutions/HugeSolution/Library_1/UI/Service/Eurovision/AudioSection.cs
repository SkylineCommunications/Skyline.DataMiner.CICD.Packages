namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service.Eurovision
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class AudioSection : Section
	{
		private readonly AudioSectionConfiguration configuration;
		private readonly EurovisionBookingDetails details;
		private readonly Helpers helpers;

		private readonly Label audioTitle = new Label("AUDIO") { Style = TextStyle.Heading };

		private readonly Label audioChannel1And2Label = new Label("Audio Channel 1&2");
		private readonly CheckBox audioChannel1And2StereoCheckBox = new CheckBox("Stereo") { IsChecked = true };
		private readonly Label audioChannel1Label = new Label("Audio Channel 1");
		private readonly DropDown audioChannel1DropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };
		private readonly YleTextBox audioChannel1OtherTextBox = new YleTextBox();
		private readonly Label audioChannel2Label = new Label("Audio Channel 2");
		private readonly DropDown audioChannel2DropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };
		private readonly YleTextBox audioChannel2OtherTextBox = new YleTextBox();

		private readonly Label audioChannel3And4Label = new Label("Audio Channel 3&4");
		private readonly CheckBox audioChannel3And4StereoCheckBox = new CheckBox("Stereo") { IsChecked = true };
		private readonly Label audioChannel3Label = new Label("Audio Channel 3");
		private readonly DropDown audioChannel3DropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };
		private readonly YleTextBox audioChannel3OtherTextBox = new YleTextBox();
		private readonly Label audioChannel4Label = new Label("Audio Channel 4");
		private readonly DropDown audioChannel4DropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };
		private readonly YleTextBox audioChannel4OtherTextBox = new YleTextBox();

		private readonly Label audioChannel5And6Label = new Label("Audio Channel 5&6");
		private readonly CheckBox audioChannel5And6StereoCheckBox = new CheckBox("Stereo") { IsChecked = true };
		private readonly Label audioChannel5Label = new Label("Audio Channel 5");
		private readonly DropDown audioChannel5DropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };
		private readonly YleTextBox audioChannel5OtherTextBox = new YleTextBox();
		private readonly Label audioChannel6Label = new Label("Audio Channel 6");
		private readonly DropDown audioChannel6DropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };
		private readonly YleTextBox audioChannel6OtherTextBox = new YleTextBox();

		private readonly Label audioChannel7And8Label = new Label("Audio Channel 7&8");
		private readonly CheckBox audioChannel7And8StereoCheckBox = new CheckBox("Stereo") { IsChecked = true };
		private readonly Label audioChannel7Label = new Label("Audio Channel 7");
		private readonly DropDown audioChannel7DropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };
		private readonly YleTextBox audioChannel7OtherTextBox = new YleTextBox();
		private readonly Label audioChannel8Label = new Label("Audio Channel 8");
		private readonly DropDown audioChannel8DropDown = new DropDown { IsDisplayFilterShown = true, IsSorted = true };
		private readonly YleTextBox audioChannel8OtherTextBox = new YleTextBox();

		public AudioSection(EurovisionBookingDetails details, AudioSectionConfiguration configuration, Helpers helpers = null)
		{
			this.details = details ?? throw new ArgumentNullException(nameof(details));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.helpers = helpers;

			Initialize();
			GenerateUI();
			HandleVisibilityAndEnabledUpdate();
		}

		public event EventHandler<bool> IsAudioChannel1And2StereoChanged;

		public event EventHandler<Audio> AudioChannel1Changed;

		public event EventHandler<string> AudioChannel1OtherTextChanged;

		public event EventHandler<Audio> AudioChannel2Changed;

		public event EventHandler<string> AudioChannel2OtherTextChanged;

		public event EventHandler<bool> IsAudioChannel3And4StereoChanged;

		public event EventHandler<Audio> AudioChannel3Changed;

		public event EventHandler<string> AudioChannel3OtherTextChanged;

		public event EventHandler<Audio> AudioChannel4Changed;

		public event EventHandler<string> AudioChannel4OtherTextChanged;

		public event EventHandler<bool> IsAudioChannel5And6StereoChanged;

		public event EventHandler<Audio> AudioChannel5Changed;

		public event EventHandler<string> AudioChannel5OtherTextChanged;

		public event EventHandler<Audio> AudioChannel6Changed;

		public event EventHandler<string> AudioChannel6OtherTextChanged;

		public event EventHandler<bool> IsAudioChannel7And8StereoChanged;

		public event EventHandler<Audio> AudioChannel7Changed;

		public event EventHandler<string> AudioChannel7OtherTextChanged;

		public event EventHandler<Audio> AudioChannel8Changed;

		public event EventHandler<string> AudioChannel8OtherTextChanged;

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

		public Audio AudioChannel1 => details.Audios.FirstOrDefault(a => a.Name == audioChannel1DropDown.Selected);

		public string AudioChannel1OtherText => audioChannel1OtherTextBox.Text ?? "";

		public Audio AudioChannel2 => details.Audios.FirstOrDefault(a => a.Name == audioChannel2DropDown.Selected);

		public string AudioChannel2OtherText => audioChannel2OtherTextBox.Text ?? "";

		public Audio AudioChannel3 => details.Audios.FirstOrDefault(a => a.Name == audioChannel3DropDown.Selected);

		public string AudioChannel3OtherText => audioChannel3OtherTextBox.Text ?? "";

		public Audio AudioChannel4 => details.Audios.FirstOrDefault(a => a.Name == audioChannel4DropDown.Selected);

		public string AudioChannel4OtherText => audioChannel4OtherTextBox.Text ?? "";

		public Audio AudioChannel5 => details.Audios.FirstOrDefault(a => a.Name == audioChannel5DropDown.Selected);

		public string AudioChannel5OtherText => audioChannel5OtherTextBox.Text ?? "";

		public Audio AudioChannel6 => details.Audios.FirstOrDefault(a => a.Name == audioChannel6DropDown.Selected);

		public string AudioChannel6OtherText => audioChannel6OtherTextBox.Text ?? "";

		public Audio AudioChannel7 => details.Audios.FirstOrDefault(a => a.Name == audioChannel7DropDown.Selected);

		public string AudioChannel7OtherText => audioChannel7OtherTextBox.Text ?? "";

		public Audio AudioChannel8 => details.Audios.FirstOrDefault(a => a.Name == audioChannel8DropDown.Selected);

		public string AudioChannel8OtherText => audioChannel8OtherTextBox.Text ?? "";

		public bool AreAudioChannels1And2Stereo => audioChannel1And2StereoCheckBox.IsChecked;

		public bool AreAudioChannels3And4Stereo => audioChannel3And4StereoCheckBox.IsChecked;

		public bool AreAudioChannels5And6Stereo => audioChannel5And6StereoCheckBox.IsChecked;

		public bool AreAudioChannels7And8Stereo => audioChannel7And8StereoCheckBox.IsChecked;

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
			UpdateAudioOptions();

			var defaultAudio = details.Audios.FirstOrDefault(x => x.Code == "M");
			string defaultOption = defaultAudio == null ? "None" : defaultAudio.Name;

			var option1 = details.Audios.FirstOrDefault(x => x.Code.Equals(details.AudioChannel1.AudioChannelCode));
			helpers?.Log(nameof(AudioSection), nameof(IntializeWidgets), $"Channel 1 code: {details.AudioChannel1.AudioChannelCode}, Setting audio channel 1 to {option1?.Name}");

			audioChannel1DropDown.Selected = details.Audios.FirstOrDefault(x => x.Code.Equals(details.AudioChannel1.AudioChannelCode))?.Name ?? defaultOption;
			audioChannel2DropDown.Selected = details.Audios.FirstOrDefault(x => x.Code.Equals(details.AudioChannel2.AudioChannelCode))?.Name ?? defaultOption;
			audioChannel3DropDown.Selected = details.Audios.FirstOrDefault(x => x.Code.Equals(details.AudioChannel3.AudioChannelCode))?.Name ?? defaultOption;
			audioChannel4DropDown.Selected = details.Audios.FirstOrDefault(x => x.Code.Equals(details.AudioChannel4.AudioChannelCode))?.Name ?? defaultOption;
			audioChannel5DropDown.Selected = details.Audios.FirstOrDefault(x => x.Code.Equals(details.AudioChannel5.AudioChannelCode))?.Name ?? defaultOption;
			audioChannel6DropDown.Selected = details.Audios.FirstOrDefault(x => x.Code.Equals(details.AudioChannel6.AudioChannelCode))?.Name ?? defaultOption;
			audioChannel7DropDown.Selected = details.Audios.FirstOrDefault(x => x.Code.Equals(details.AudioChannel7.AudioChannelCode))?.Name ?? defaultOption;
			audioChannel8DropDown.Selected = details.Audios.FirstOrDefault(x => x.Code.Equals(details.AudioChannel8.AudioChannelCode))?.Name ?? defaultOption;

			audioChannel1And2StereoCheckBox.IsChecked = details.AudioChannel1.AudioChannelCode == details.AudioChannel2.AudioChannelCode;
			audioChannel3And4StereoCheckBox.IsChecked = details.AudioChannel3.AudioChannelCode == details.AudioChannel4.AudioChannelCode;
			audioChannel5And6StereoCheckBox.IsChecked = details.AudioChannel5.AudioChannelCode == details.AudioChannel6.AudioChannelCode;
			audioChannel7And8StereoCheckBox.IsChecked = details.AudioChannel7.AudioChannelCode == details.AudioChannel8.AudioChannelCode;

			audioChannel1OtherTextBox.Text = details.AudioChannel1.AudioChannelOtherText ?? String.Empty;
			audioChannel2OtherTextBox.Text = details.AudioChannel2.AudioChannelOtherText ?? String.Empty;
			audioChannel3OtherTextBox.Text = details.AudioChannel3.AudioChannelOtherText ?? String.Empty;
			audioChannel4OtherTextBox.Text = details.AudioChannel4.AudioChannelOtherText ?? String.Empty;
			audioChannel5OtherTextBox.Text = details.AudioChannel5.AudioChannelOtherText ?? String.Empty;
			audioChannel6OtherTextBox.Text = details.AudioChannel6.AudioChannelOtherText ?? String.Empty;
			audioChannel7OtherTextBox.Text = details.AudioChannel7.AudioChannelOtherText ?? String.Empty;
			audioChannel8OtherTextBox.Text = details.AudioChannel8.AudioChannelOtherText ?? String.Empty;
		}

		private void UpdateAudioOptions()
		{
			var audioOptions = new List<string> { "None" };
			audioOptions.AddRange(details.Audios.OrderBy(x => x.Code).Select(x => x.Name));

			audioChannel1DropDown.SetOptions(audioOptions);
			audioChannel2DropDown.SetOptions(audioOptions);
			audioChannel3DropDown.SetOptions(audioOptions);
			audioChannel4DropDown.SetOptions(audioOptions);
			audioChannel5DropDown.SetOptions(audioOptions);
			audioChannel6DropDown.SetOptions(audioOptions);
			audioChannel7DropDown.SetOptions(audioOptions);
			audioChannel8DropDown.SetOptions(audioOptions);
		}

		private void UpdateSelectedAudio(AudioChannel audioChannel, DropDown audioDropdown)
		{
			var selected = details.Audios.FirstOrDefault(x => x.Code == audioChannel.AudioChannelCode)?.Name ?? "None";
			audioDropdown.Selected = selected;
		}

		private void SubscribeToWidgets()
		{
			audioChannel1And2StereoCheckBox.Changed += (s, e) =>
			{
				HandleVisibilityAndEnabledUpdate();
				IsAudioChannel1And2StereoChanged?.Invoke(this, audioChannel1And2StereoCheckBox.IsChecked);
			};

			audioChannel3And4StereoCheckBox.Changed += (s, e) =>
			{
				HandleVisibilityAndEnabledUpdate();
				IsAudioChannel3And4StereoChanged?.Invoke(this, audioChannel3And4StereoCheckBox.IsChecked);
			};

			audioChannel5And6StereoCheckBox.Changed += (s, e) =>
			{
				HandleVisibilityAndEnabledUpdate();
				IsAudioChannel5And6StereoChanged?.Invoke(this, audioChannel5And6StereoCheckBox.IsChecked);
			};

			audioChannel7And8StereoCheckBox.Changed += (s, e) =>
			{
				HandleVisibilityAndEnabledUpdate();
				IsAudioChannel7And8StereoChanged?.Invoke(this, audioChannel7And8StereoCheckBox.IsChecked);
			};

			audioChannel1DropDown.Changed += (s, e) => AudioChannel1Changed?.Invoke(this, AudioChannel1);
			audioChannel2DropDown.Changed += (s, e) => AudioChannel2Changed?.Invoke(this, AudioChannel2);
			audioChannel3DropDown.Changed += (s, e) => AudioChannel3Changed?.Invoke(this, AudioChannel3);
			audioChannel4DropDown.Changed += (s, e) => AudioChannel4Changed?.Invoke(this, AudioChannel4);
			audioChannel5DropDown.Changed += (s, e) => AudioChannel5Changed?.Invoke(this, AudioChannel5);
			audioChannel6DropDown.Changed += (s, e) => AudioChannel6Changed?.Invoke(this, AudioChannel6);
			audioChannel7DropDown.Changed += (s, e) => AudioChannel7Changed?.Invoke(this, AudioChannel7);
			audioChannel8DropDown.Changed += (s, e) => AudioChannel8Changed?.Invoke(this, AudioChannel8);

			audioChannel1OtherTextBox.Changed += (s, e) => AudioChannel1OtherTextChanged?.Invoke(this, AudioChannel1OtherText);
			audioChannel2OtherTextBox.Changed += (s, e) => AudioChannel2OtherTextChanged?.Invoke(this, AudioChannel2OtherText);
			audioChannel3OtherTextBox.Changed += (s, e) => AudioChannel3OtherTextChanged?.Invoke(this, AudioChannel3OtherText);
			audioChannel4OtherTextBox.Changed += (s, e) => AudioChannel4OtherTextChanged?.Invoke(this, AudioChannel4OtherText);
			audioChannel5OtherTextBox.Changed += (s, e) => AudioChannel5OtherTextChanged?.Invoke(this, AudioChannel5OtherText);
			audioChannel6OtherTextBox.Changed += (s, e) => AudioChannel6OtherTextChanged?.Invoke(this, AudioChannel6OtherText);
			audioChannel7OtherTextBox.Changed += (s, e) => AudioChannel7OtherTextChanged?.Invoke(this, AudioChannel7OtherText);
			audioChannel8OtherTextBox.Changed += (s, e) => AudioChannel8OtherTextChanged?.Invoke(this, AudioChannel8OtherText);
		}

		private void SubscribeToBookingDetails()
		{
			details.AudiosChanged += (s, e) =>
			{
				UpdateAudioOptions();
				UpdateSelectedAudio(details.AudioChannel1, audioChannel1DropDown);
				UpdateSelectedAudio(details.AudioChannel2, audioChannel2DropDown);
				UpdateSelectedAudio(details.AudioChannel3, audioChannel3DropDown);
				UpdateSelectedAudio(details.AudioChannel4, audioChannel4DropDown);
				UpdateSelectedAudio(details.AudioChannel5, audioChannel5DropDown);
				UpdateSelectedAudio(details.AudioChannel6, audioChannel6DropDown);
				UpdateSelectedAudio(details.AudioChannel7, audioChannel7DropDown);
				UpdateSelectedAudio(details.AudioChannel8, audioChannel8DropDown);
				HandleVisibilityAndEnabledUpdate();
			};

			details.AudioChannel1.CodeChanged += (s, e) =>
			{
				UpdateSelectedAudio(details.AudioChannel1, audioChannel1DropDown);
				HandleVisibilityAndEnabledUpdate();
			};

			details.AudioChannel2.CodeChanged += (s, e) =>
			{
				UpdateSelectedAudio(details.AudioChannel2, audioChannel2DropDown);
				HandleVisibilityAndEnabledUpdate();
			};

			details.AudioChannel3.CodeChanged += (s, e) =>
			{
				UpdateSelectedAudio(details.AudioChannel3, audioChannel3DropDown);
				HandleVisibilityAndEnabledUpdate();
			};

			details.AudioChannel4.CodeChanged += (s, e) =>
			{
				UpdateSelectedAudio(details.AudioChannel4, audioChannel4DropDown);
				HandleVisibilityAndEnabledUpdate();
			};

			details.AudioChannel5.CodeChanged += (s, e) =>
			{
				UpdateSelectedAudio(details.AudioChannel5, audioChannel5DropDown);
				HandleVisibilityAndEnabledUpdate();
			};

			details.AudioChannel6.CodeChanged += (s, e) =>
			{
				UpdateSelectedAudio(details.AudioChannel6, audioChannel6DropDown);
				HandleVisibilityAndEnabledUpdate();
			};

			details.AudioChannel7.CodeChanged += (s, e) =>
			{
				UpdateSelectedAudio(details.AudioChannel7, audioChannel7DropDown);
				HandleVisibilityAndEnabledUpdate();
			};

			details.AudioChannel8.CodeChanged += (s, e) =>
			{
				UpdateSelectedAudio(details.AudioChannel8, audioChannel8DropDown);
				HandleVisibilityAndEnabledUpdate();
			};

			details.AudioChannel1.OtherTextChanged += (s, e) => audioChannel1OtherTextBox.Text = e;
			details.AudioChannel2.OtherTextChanged += (s, e) => audioChannel2OtherTextBox.Text = e;
			details.AudioChannel3.OtherTextChanged += (s, e) => audioChannel3OtherTextBox.Text = e;
			details.AudioChannel4.OtherTextChanged += (s, e) => audioChannel4OtherTextBox.Text = e;
			details.AudioChannel5.OtherTextChanged += (s, e) => audioChannel5OtherTextBox.Text = e;
			details.AudioChannel6.OtherTextChanged += (s, e) => audioChannel6OtherTextBox.Text = e;
			details.AudioChannel7.OtherTextChanged += (s, e) => audioChannel7OtherTextBox.Text = e;
			details.AudioChannel8.OtherTextChanged += (s, e) => audioChannel8OtherTextBox.Text = e;
		}

		private void GenerateUI()
		{
			int row = -1;

			AddWidget(audioTitle, ++row, 0, 1, configuration.LabelSpan);

			AddWidget(audioChannel1And2Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel1And2StereoCheckBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(audioChannel1Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel1DropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(audioChannel1OtherTextBox, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan);

			AddWidget(audioChannel2Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel2DropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(audioChannel2OtherTextBox, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan);

			AddWidget(audioChannel3And4Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel3And4StereoCheckBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(audioChannel3Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel3DropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(audioChannel3OtherTextBox, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan);

			AddWidget(audioChannel4Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel4DropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(audioChannel4OtherTextBox, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan);

			AddWidget(audioChannel5And6Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel5And6StereoCheckBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(audioChannel5Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel5DropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(audioChannel5OtherTextBox, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan);

			AddWidget(audioChannel6Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel6DropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(audioChannel6OtherTextBox, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan);

			AddWidget(audioChannel7And8Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel7And8StereoCheckBox, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);

			AddWidget(audioChannel7Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel7DropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(audioChannel7OtherTextBox, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan);

			AddWidget(audioChannel8Label, ++row, 0, 1, configuration.LabelSpan);
			AddWidget(audioChannel8DropDown, row, configuration.InputWidgetColumn, 1, configuration.InputWidgetSpan);
			AddWidget(audioChannel8OtherTextBox, row, configuration.InputWidgetColumn + configuration.InputWidgetSpan);

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>")]
		private void HandleVisibilityAndEnabledUpdate()
		{
			audioChannel1Label.IsVisible = IsVisible && !audioChannel1And2StereoCheckBox.IsChecked;
			audioChannel1OtherTextBox.IsVisible = IsVisible && AudioChannel1 != null && AudioChannel1.Name.Equals("other", StringComparison.CurrentCultureIgnoreCase);

			audioChannel2Label.IsVisible = IsVisible && !audioChannel1And2StereoCheckBox.IsChecked;
			audioChannel2DropDown.IsVisible = IsVisible && !audioChannel1And2StereoCheckBox.IsChecked;
			audioChannel2OtherTextBox.IsVisible = IsVisible && !audioChannel1And2StereoCheckBox.IsChecked && AudioChannel2 != null && AudioChannel2.Name.Equals("other", StringComparison.CurrentCultureIgnoreCase);

			audioChannel3Label.IsVisible = IsVisible && !audioChannel3And4StereoCheckBox.IsChecked;
			audioChannel3OtherTextBox.IsVisible = IsVisible && AudioChannel3 != null && AudioChannel3.Name.Equals("other", StringComparison.CurrentCultureIgnoreCase);

			audioChannel4Label.IsVisible = IsVisible && !audioChannel3And4StereoCheckBox.IsChecked;
			audioChannel4DropDown.IsVisible = IsVisible && !audioChannel3And4StereoCheckBox.IsChecked;
			audioChannel4OtherTextBox.IsVisible = IsVisible && !audioChannel3And4StereoCheckBox.IsChecked && AudioChannel4 != null && AudioChannel4.Name.Equals("other", StringComparison.CurrentCultureIgnoreCase);

			audioChannel5Label.IsVisible = IsVisible && !audioChannel5And6StereoCheckBox.IsChecked;
			audioChannel5OtherTextBox.IsVisible = IsVisible && AudioChannel5 != null && AudioChannel5.Name.Equals("other", StringComparison.CurrentCultureIgnoreCase);

			audioChannel6Label.IsVisible = IsVisible && !audioChannel5And6StereoCheckBox.IsChecked;
			audioChannel6DropDown.IsVisible = IsVisible && !audioChannel5And6StereoCheckBox.IsChecked;
			audioChannel6OtherTextBox.IsVisible = IsVisible && !audioChannel5And6StereoCheckBox.IsChecked && AudioChannel6 != null && AudioChannel6.Name.Equals("other", StringComparison.CurrentCultureIgnoreCase);

			audioChannel7Label.IsVisible = IsVisible && !audioChannel7And8StereoCheckBox.IsChecked;
			audioChannel7OtherTextBox.IsVisible = IsVisible && AudioChannel7 != null && AudioChannel7.Name.Equals("other", StringComparison.CurrentCultureIgnoreCase);

			audioChannel8Label.IsVisible = IsVisible && !audioChannel7And8StereoCheckBox.IsChecked;
			audioChannel8DropDown.IsVisible = IsVisible && !audioChannel7And8StereoCheckBox.IsChecked;
			audioChannel8OtherTextBox.IsVisible = IsVisible && !audioChannel7And8StereoCheckBox.IsChecked && AudioChannel8 != null && AudioChannel8.Name.Equals("other", StringComparison.CurrentCultureIgnoreCase);

			ToolTipHandler.SetTooltipVisibility(this);
		}
	}
}
